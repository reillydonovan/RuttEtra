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
    
    private int _lastHRes;
    private int _lastVRes;
    private int _lastScanSkip;
    private bool _lastShowH;
    private bool _lastShowV;
    
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        
        CreateLineMaterial();
    }
    
    private void Start()
    {
        if (webcamCapture != null)
        {
            webcamCapture.OnFrameReady += OnWebcamFrame;
        }
        
        GenerateMesh();
    }
    
    private void CreateLineMaterial()
    {
        // Use the custom line shader
        Shader lineShader = Shader.Find("RuttEtra/ScanLine");
        if (lineShader == null)
        {
            lineShader = Shader.Find("Sprites/Default");
            Debug.LogWarning("RuttEtra/ScanLine shader not found, using fallback");
        }
        
        _lineMaterial = new Material(lineShader);
        _meshRenderer.material = _lineMaterial;
    }
    
    public void GenerateMesh()
    {
        if (settings == null)
        {
            Debug.LogError("RuttEtraSettings not assigned!");
            return;
        }
        
        int hRes = settings.horizontalResolution;
        int vRes = settings.verticalResolution;
        
        // Cache settings for change detection
        _lastHRes = hRes;
        _lastVRes = vRes;
        _lastScanSkip = settings.scanLineSkip;
        _lastShowH = settings.showHorizontalLines;
        _lastShowV = settings.showVerticalLines;
        
        _mesh = new Mesh();
        _mesh.name = "RuttEtra Mesh";
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        // Generate vertices in a grid
        int vertexCount = hRes * vRes;
        _vertices = new Vector3[vertexCount];
        _baseVertices = new Vector3[vertexCount];
        _colors = new Color[vertexCount];
        
        float halfWidth = meshWidth / 2f;
        float halfHeight = meshHeight / 2f;
        
        for (int y = 0; y < vRes; y++)
        {
            for (int x = 0; x < hRes; x++)
            {
                int index = y * hRes + x;
                float px = (x / (float)(hRes - 1)) * meshWidth - halfWidth;
                float py = (y / (float)(vRes - 1)) * meshHeight - halfHeight;
                
                _baseVertices[index] = new Vector3(px, py, 0);
                _vertices[index] = _baseVertices[index];
                _colors[index] = settings.primaryColor;
            }
        }
        
        // Generate line indices (horizontal scan lines)
        List<int> hIndices = new List<int>();
        for (int y = 0; y < vRes; y += settings.scanLineSkip)
        {
            for (int x = 0; x < hRes - 1; x++)
            {
                int index = y * hRes + x;
                hIndices.Add(index);
                hIndices.Add(index + 1);
            }
        }
        _horizontalIndices = hIndices.ToArray();
        
        // Generate vertical line indices
        List<int> vIndices = new List<int>();
        for (int x = 0; x < hRes; x += settings.scanLineSkip)
        {
            for (int y = 0; y < vRes - 1; y++)
            {
                int index = y * hRes + x;
                vIndices.Add(index);
                vIndices.Add((y + 1) * hRes + x);
            }
        }
        _verticalIndices = vIndices.ToArray();
        
        _mesh.vertices = _vertices;
        _mesh.colors = _colors;
        
        UpdateMeshTopology();
        
        _meshFilter.mesh = _mesh;
        
        // Initialize luminance buffers
        _luminanceBuffer = new float[vertexCount];
        _smoothedLuminance = new float[vertexCount];
        _pixelBuffer = new Color[hRes * vRes];
    }
    
    private void UpdateMeshTopology()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.colors = _colors;
        
        // Combine indices based on settings
        List<int> activeIndices = new List<int>();
        if (settings.showHorizontalLines && _horizontalIndices != null)
            activeIndices.AddRange(_horizontalIndices);
        if (settings.showVerticalLines && _verticalIndices != null)
            activeIndices.AddRange(_verticalIndices);
        
        if (activeIndices.Count > 0)
        {
            _mesh.SetIndices(activeIndices.ToArray(), MeshTopology.Lines, 0);
        }
    }
    
    private void OnWebcamFrame(Texture sourceTexture)
    {
        if (settings == null) return;
        
        // Check if we need to regenerate mesh
        if (NeedsMeshRegeneration())
        {
            GenerateMesh();
        }
        
        ProcessLuminance(sourceTexture);
        UpdateDisplacement();
        UpdateMaterialProperties();
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
        
        // Create or resize luminance texture
        if (_luminanceTexture == null || 
            _luminanceTexture.width != hRes || 
            _luminanceTexture.height != vRes)
        {
            if (_luminanceTexture != null) Destroy(_luminanceTexture);
            _luminanceTexture = new Texture2D(hRes, vRes, TextureFormat.RGBA32, false);
            _luminanceTexture.filterMode = FilterMode.Bilinear;
        }
        
        // Sample source texture at mesh resolution
        RenderTexture tempRT = RenderTexture.GetTemporary(hRes, vRes, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, tempRT);
        
        RenderTexture.active = tempRT;
        _luminanceTexture.ReadPixels(new Rect(0, 0, hRes, vRes), 0, 0);
        _luminanceTexture.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(tempRT);
        
        // Extract luminance values
        _pixelBuffer = _luminanceTexture.GetPixels();
        
        for (int i = 0; i < _pixelBuffer.Length && i < _smoothedLuminance.Length; i++)
        {
            Color c = _pixelBuffer[i];
            // Standard luminance calculation
            float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            
            if (settings.invertDisplacement)
                lum = 1f - lum;
            
            // Smooth luminance over time
            _smoothedLuminance[i] = Mathf.Lerp(_smoothedLuminance[i], lum, 
                1f - settings.displacementSmoothing);
            _luminanceBuffer[i] = _smoothedLuminance[i];
        }
    }
    
    private void UpdateDisplacement()
    {
        if (_vertices == null || _baseVertices == null) return;
        
        float strength = settings.displacementStrength;
        bool useSourceCol = settings.useSourceColor;
        Color primColor = settings.primaryColor;
        Color secColor = settings.secondaryColor;
        float blend = settings.colorBlend;
        
        for (int i = 0; i < _vertices.Length; i++)
        {
            float lum = i < _luminanceBuffer.Length ? _luminanceBuffer[i] : 0f;
            
            // Apply displacement along Z axis
            _vertices[i] = _baseVertices[i] + Vector3.forward * (lum * strength);
            
            // Calculate color
            if (useSourceCol && _pixelBuffer != null && i < _pixelBuffer.Length)
            {
                _colors[i] = _pixelBuffer[i];
            }
            else
            {
                // Blend between primary and secondary based on luminance
                _colors[i] = Color.Lerp(primColor, secColor, lum * blend);
            }
        }
        
        _mesh.vertices = _vertices;
        _mesh.colors = _colors;
        _mesh.RecalculateBounds();
    }
    
    private void UpdateMaterialProperties()
    {
        if (_lineMaterial == null || settings == null) return;
        
        _lineMaterial.SetFloat("_LineWidth", settings.lineWidth);
        _lineMaterial.SetFloat("_GlowIntensity", settings.glowIntensity);
        _lineMaterial.SetFloat("_NoiseAmount", settings.noiseAmount);
    }
    
    public void RefreshMesh()
    {
        GenerateMesh();
    }
    
    private void OnDestroy()
    {
        if (webcamCapture != null)
            webcamCapture.OnFrameReady -= OnWebcamFrame;
        
        if (_mesh != null) Destroy(_mesh);
        if (_luminanceTexture != null) Destroy(_luminanceTexture);
        if (_lineMaterial != null) Destroy(_lineMaterial);
    }
}




