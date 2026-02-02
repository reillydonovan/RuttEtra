using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RuttEtraMeshGenerator : MonoBehaviour
{
    [Header("References")]
    public RuttEtraSettings settings;
    public WebcamCapture webcamCapture;
    
    [Header("Mesh Settings")]
    public float meshWidth = 16f;
    public float meshHeight = 9f;
    
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    
    private Vector3[] _vertices;
    private Vector3[] _baseVertices;
    private Color[] _colors;
    private int[] _horizontalIndices;
    private int[] _verticalIndices;
    
    private Texture2D _luminanceTexture;
    private Color[] _pixelBuffer;
    private float[] _luminanceBuffer;
    private float[] _smoothedLuminance;
    
    private Material _lineMaterial;
    
    private int _lastHRes, _lastVRes, _lastScanSkip;
    private bool _lastShowH, _lastShowV;
    
    private float _waveTime;
    private int _frameCount;
    
    // Optional effect components
    private GlitchEffects _glitchEffects;
    private MirrorKaleidoscope _mirrorKaleidoscope;
    private DepthColorizer _depthColorizer;
    
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        CreateLineMaterial();
    }
    
    private void Start()
    {
        if (webcamCapture != null)
            webcamCapture.OnFrameReady += OnWebcamFrame;
            
        // Find optional effect components
        _glitchEffects = FindFirstObjectByType<GlitchEffects>();
        _mirrorKaleidoscope = FindFirstObjectByType<MirrorKaleidoscope>();
        _depthColorizer = FindFirstObjectByType<DepthColorizer>();
        
        GenerateMesh();
    }
    
    private void CreateLineMaterial()
    {
        Shader lineShader = Shader.Find("RuttEtra/ScanLine");
        if (lineShader == null)
            lineShader = Shader.Find("Sprites/Default");
        _lineMaterial = new Material(lineShader);
        _meshRenderer.material = _lineMaterial;
    }
    
    public void GenerateMesh()
    {
        if (settings == null) return;
        
        int hRes = settings.horizontalResolution;
        int vRes = settings.verticalResolution;
        
        _lastHRes = hRes; _lastVRes = vRes;
        _lastScanSkip = settings.scanLineSkip;
        _lastShowH = settings.showHorizontalLines;
        _lastShowV = settings.showVerticalLines;
        
        _mesh = new Mesh();
        _mesh.name = "RuttEtra Mesh";
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        int vertexCount = hRes * vRes;
        _vertices = new Vector3[vertexCount];
        _baseVertices = new Vector3[vertexCount];
        _colors = new Color[vertexCount];
        
        float halfW = meshWidth / 2f;
        float halfH = meshHeight / 2f;
        
        for (int y = 0; y < vRes; y++)
        {
            for (int x = 0; x < hRes; x++)
            {
                int i = y * hRes + x;
                float px = (x / (float)(hRes - 1)) * meshWidth - halfW;
                float py = (y / (float)(vRes - 1)) * meshHeight - halfH;
                _baseVertices[i] = new Vector3(px, py, 0);
                _vertices[i] = _baseVertices[i];
                _colors[i] = settings.primaryColor;
            }
        }
        
        // Horizontal lines
        List<int> hIdx = new List<int>();
        for (int y = 0; y < vRes; y += settings.scanLineSkip)
        {
            for (int x = 0; x < hRes - 1; x++)
            {
                int i = y * hRes + x;
                hIdx.Add(i);
                hIdx.Add(i + 1);
            }
        }
        _horizontalIndices = hIdx.ToArray();
        
        // Vertical lines
        List<int> vIdx = new List<int>();
        for (int x = 0; x < hRes; x += settings.scanLineSkip)
        {
            for (int y = 0; y < vRes - 1; y++)
            {
                int i = y * hRes + x;
                vIdx.Add(i);
                vIdx.Add((y + 1) * hRes + x);
            }
        }
        _verticalIndices = vIdx.ToArray();
        
        _mesh.vertices = _vertices;
        _mesh.colors = _colors;
        UpdateMeshTopology();
        _meshFilter.mesh = _mesh;
        
        _luminanceBuffer = new float[vertexCount];
        _smoothedLuminance = new float[vertexCount];
        _pixelBuffer = new Color[hRes * vRes];
    }
    
    private void UpdateMeshTopology()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.colors = _colors;
        
        List<int> idx = new List<int>();
        if (settings.showHorizontalLines && _horizontalIndices != null) idx.AddRange(_horizontalIndices);
        if (settings.showVerticalLines && _verticalIndices != null) idx.AddRange(_verticalIndices);
        
        if (idx.Count > 0)
            _mesh.SetIndices(idx.ToArray(), MeshTopology.Lines, 0);
    }
    
    private void Update()
    {
        if (settings == null) return;
        
        _waveTime += Time.deltaTime * settings.waveSpeed;
        _frameCount++;
        
        UpdateMaterialProperties();
    }
    
    private void OnWebcamFrame(Texture sourceTexture)
    {
        if (settings == null) return;
        
        if (NeedsMeshRegeneration()) GenerateMesh();
        
        ProcessLuminance(sourceTexture);
        UpdateDisplacement();
    }
    
    private bool NeedsMeshRegeneration()
    {
        return _lastHRes != settings.horizontalResolution ||
               _lastVRes != settings.verticalResolution ||
               _lastScanSkip != settings.scanLineSkip ||
               _lastShowH != settings.showHorizontalLines ||
               _lastShowV != settings.showVerticalLines;
    }
    
    private void ProcessLuminance(Texture source)
    {
        int hRes = settings.horizontalResolution;
        int vRes = settings.verticalResolution;
        
        if (_luminanceTexture == null || _luminanceTexture.width != hRes || _luminanceTexture.height != vRes)
        {
            if (_luminanceTexture != null) Destroy(_luminanceTexture);
            _luminanceTexture = new Texture2D(hRes, vRes, TextureFormat.RGBA32, false);
            _luminanceTexture.filterMode = FilterMode.Bilinear;
        }
        
        RenderTexture tempRT = RenderTexture.GetTemporary(hRes, vRes, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, tempRT);
        RenderTexture.active = tempRT;
        _luminanceTexture.ReadPixels(new Rect(0, 0, hRes, vRes), 0, 0);
        _luminanceTexture.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(tempRT);
        
        _pixelBuffer = _luminanceTexture.GetPixels();
        
        float brightness = settings.brightness;
        float contrast = settings.contrast;
        float threshold = settings.threshold;
        float gamma = settings.gamma;
        int posterize = settings.posterize;
        bool edge = settings.edgeDetect;
        
        for (int i = 0; i < _pixelBuffer.Length && i < _smoothedLuminance.Length; i++)
        {
            Color c = _pixelBuffer[i];
            float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            
            // Edge detection
            if (edge && i > hRes && i < _pixelBuffer.Length - hRes)
            {
                float left = i > 0 ? GetLum(_pixelBuffer[i - 1]) : lum;
                float right = i < _pixelBuffer.Length - 1 ? GetLum(_pixelBuffer[i + 1]) : lum;
                float up = GetLum(_pixelBuffer[i - hRes]);
                float down = GetLum(_pixelBuffer[i + hRes]);
                lum = Mathf.Abs(lum - left) + Mathf.Abs(lum - right) + Mathf.Abs(lum - up) + Mathf.Abs(lum - down);
                lum = Mathf.Clamp01(lum * 2f);
            }
            
            // Input processing
            lum = (lum + brightness) * contrast;
            lum = Mathf.Pow(Mathf.Clamp01(lum), gamma);
            lum = lum > threshold ? lum : 0f;
            
            // Posterize
            if (posterize > 1)
                lum = Mathf.Floor(lum * posterize) / (posterize - 1);
            
            lum = Mathf.Clamp01(lum);
            if (settings.invertDisplacement) lum = 1f - lum;
            
            // Temporal smoothing
            float smooth = 1f - settings.displacementSmoothing;
            _smoothedLuminance[i] = Mathf.Lerp(_smoothedLuminance[i], lum, smooth);
            
            // Persistence
            if (settings.persistence > 0)
                _luminanceBuffer[i] = Mathf.Max(_smoothedLuminance[i], _luminanceBuffer[i] * settings.persistence);
            else
                _luminanceBuffer[i] = _smoothedLuminance[i];
        }
    }
    
    float GetLum(Color c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
    
    private void UpdateDisplacement()
    {
        if (_vertices == null || _baseVertices == null) return;
        
        float strength = settings.displacementStrength;
        float offset = settings.displacementOffset;
        float waveH = settings.horizontalWave;
        float waveV = settings.verticalWave;
        float waveFreq = settings.waveFrequency;
        float zMod = settings.zModulation;
        float zModFreq = settings.zModFrequency;
        float keystoneH = settings.keystoneH;
        float keystoneV = settings.keystoneV;
        float barrel = settings.barrelDistortion;
        float taper = settings.lineTaper;
        bool useSource = settings.useSourceColor;
        Color prim = settings.primaryColor;
        Color sec = settings.secondaryColor;
        float blend = settings.colorBlend;
        bool interlace = settings.interlace;
        
        // Raster position offset (applied to vertices directly)
        float hPosOffset = settings.horizontalPosition;
        float vPosOffset = settings.verticalPosition;
        
        // Raster scale
        float hScale = settings.horizontalScale;
        float vScale = settings.verticalScale;
        float meshScl = settings.meshScale;
        
        // Raster rotation
        Quaternion rot = Quaternion.Euler(settings.rotationX, settings.rotationY, settings.rotationZ);
        
        float flicker = 1f;
        if (settings.scanlineFlicker > 0)
            flicker = 1f - (Random.value * settings.scanlineFlicker * 0.5f);
        
        int hRes = settings.horizontalResolution;
        int vRes = settings.verticalResolution;
        
        for (int i = 0; i < _vertices.Length; i++)
        {
            int x = i % hRes;
            int y = i / hRes;
            float nx = x / (float)(hRes - 1);  // 0-1
            float ny = y / (float)(vRes - 1);  // 0-1
            float cx = nx - 0.5f;  // -0.5 to 0.5
            float cy = ny - 0.5f;
            
            // Interlace
            if (interlace && (y + _frameCount) % 2 == 0)
            {
                _colors[i] = Color.clear;
                continue;
            }
            
            float lum = i < _luminanceBuffer.Length ? _luminanceBuffer[i] : 0f;
            Vector3 pos = _baseVertices[i];
            
            // Barrel/Pincushion distortion
            if (Mathf.Abs(barrel) > 0.001f)
            {
                float r2 = cx * cx + cy * cy;
                float distort = 1f + barrel * r2 * 4f;
                pos.x = cx * distort * meshWidth;
                pos.y = cy * distort * meshHeight;
            }
            
            // Keystone distortion
            if (Mathf.Abs(keystoneH) > 0.001f)
                pos.x *= 1f + keystoneH * cy * 2f;
            if (Mathf.Abs(keystoneV) > 0.001f)
                pos.y *= 1f + keystoneV * cx * 2f;
            
            // Wave deflection
            if (waveH > 0)
                pos.x += Mathf.Sin((ny * waveFreq * Mathf.PI * 2f) + _waveTime) * waveH;
            if (waveV > 0)
                pos.y += Mathf.Sin((nx * waveFreq * Mathf.PI * 2f) + _waveTime) * waveV;
            
            // Z displacement
            float z = (lum + offset) * strength * flicker;
            
            // Z modulation
            if (zMod > 0)
                z += Mathf.Sin(_waveTime * zModFreq * Mathf.PI * 2f) * zMod;
            
            // Taper
            if (taper > 0)
            {
                float edge = 1f - (Mathf.Abs(cx) * 2f * taper);
                z *= Mathf.Clamp01(edge);
            }
            
            pos.z = z;
            
            // Apply scale
            pos.x *= hScale * meshScl;
            pos.y *= vScale * meshScl;
            pos.z *= meshScl;
            
            // Apply rotation
            pos = rot * pos;
            
            // Apply position offset
            pos.x += hPosOffset;
            pos.y += vPosOffset;
            
            _vertices[i] = pos;
            
            // Color
            if (useSource && _pixelBuffer != null && i < _pixelBuffer.Length)
                _colors[i] = _pixelBuffer[i] * flicker;
            else
                _colors[i] = Color.Lerp(prim, sec, lum * blend) * flicker;
        }
        
        // Apply glitch effects to vertices
        if (_glitchEffects != null && _glitchEffects.enableGlitch)
        {
            _glitchEffects.ApplyGlitchToVertices(_vertices, hRes, vRes);
        }
        
        // Apply mirror/kaleidoscope effects
        if (_mirrorKaleidoscope != null && (_mirrorKaleidoscope.enableMirror || _mirrorKaleidoscope.enableKaleidoscope))
        {
            ApplyMirrorEffects(hRes, vRes);
        }
        
        // Apply depth colorizer
        if (_depthColorizer != null && _depthColorizer.enableDepthColor)
        {
            _depthColorizer.ApplyDepthColors(_vertices, _colors, settings.displacementStrength);
        }
        
        _mesh.vertices = _vertices;
        _mesh.colors = _colors;
        _mesh.RecalculateBounds();
    }
    
    private void ApplyMirrorEffects(int hRes, int vRes)
    {
        if (_mirrorKaleidoscope == null) return;
        
        for (int i = 0; i < _vertices.Length; i++)
        {
            int x = i % hRes;
            int y = i / hRes;
            float u = x / (float)(hRes - 1);
            float v = y / (float)(vRes - 1);
            
            // Transform UV through mirror/kaleidoscope
            Vector2 uv = _mirrorKaleidoscope.TransformUV(new Vector2(u, v));
            
            // Map back to vertex index and copy position
            int srcX = Mathf.Clamp(Mathf.RoundToInt(uv.x * (hRes - 1)), 0, hRes - 1);
            int srcY = Mathf.Clamp(Mathf.RoundToInt(uv.y * (vRes - 1)), 0, vRes - 1);
            int srcIndex = srcY * hRes + srcX;
            
            if (srcIndex != i && srcIndex < _vertices.Length)
            {
                // Only modify Z displacement based on mirrored source
                _vertices[i].z = _vertices[srcIndex].z;
            }
        }
    }
    
    private void UpdateMaterialProperties()
    {
        if (_lineMaterial == null || settings == null) return;
        
        _lineMaterial.SetFloat("_LineWidth", settings.lineWidth);
        _lineMaterial.SetFloat("_GlowIntensity", settings.glowIntensity);
        _lineMaterial.SetFloat("_NoiseAmount", settings.noiseAmount);
        _lineMaterial.SetFloat("_LineTaper", settings.lineTaper);
        _lineMaterial.SetFloat("_Bloom", settings.bloom);
    }
    
    public void RefreshMesh() => GenerateMesh();
    
    private void OnDestroy()
    {
        if (webcamCapture != null) webcamCapture.OnFrameReady -= OnWebcamFrame;
        if (_mesh) Destroy(_mesh);
        if (_luminanceTexture) Destroy(_luminanceTexture);
        if (_lineMaterial) Destroy(_lineMaterial);
    }
}
