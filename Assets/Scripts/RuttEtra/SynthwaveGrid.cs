using UnityEngine;

/// <summary>
/// Creates a classic synthwave/retrowave infinite grid effect.
/// Adds an animated horizon grid beneath the Rutt/Etra visualization.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SynthwaveGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public bool enableGrid = false;
    [Range(10, 100)] public int gridSize = 40;
    [Range(0.5f, 5f)] public float cellSize = 1f;
    [Range(0f, 10f)] public float scrollSpeed = 2f;
    
    [Header("Position")]
    public float gridHeight = -3f;
    public float gridDistance = 5f;
    [Range(0f, 45f)] public float gridTilt = 10f;
    
    [Header("Appearance")]
    public Color gridColor = new Color(1f, 0f, 0.5f, 1f);
    public Color horizonColor = new Color(1f, 0.5f, 0f, 1f);
    [Range(0f, 1f)] public float horizonBlend = 0.5f;
    [Range(0.001f, 0.02f)] public float lineWidth = 0.005f;
    [Range(0f, 2f)] public float glowIntensity = 0.5f;
    
    [Header("Horizon Sun")]
    public bool showSun = true;
    public Color sunColor = new Color(1f, 0.3f, 0.1f);
    [Range(0.5f, 3f)] public float sunSize = 1.5f;
    public float sunHeight = 2f;
    
    [Header("Mountains")]
    public bool showMountains = false;
    [Range(0.5f, 3f)] public float mountainHeight = 1.5f;
    public Color mountainColor = new Color(0.1f, 0f, 0.2f);
    
    [Header("Animation")]
    public bool animateColors = false;
    [Range(0f, 1f)] public float colorCycleSpeed = 0.1f;
    public bool pulseGlow = false;
    [Range(0.5f, 5f)] public float pulseSpeed = 2f;
    
    [Header("Audio Reactive")]
    public bool reactToAudio = false;
    public AudioReactive audioReactive;
    [Range(0f, 2f)] public float audioScrollMultiplier = 1f;
    [Range(0f, 1f)] public float audioGlowMultiplier = 0.5f;
    
    // Components
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Material _gridMaterial;
    private Mesh _gridMesh;
    
    // State
    private float _scrollOffset;
    private float _hueOffset;
    private GameObject _sunObject;
    private MeshRenderer _sunRenderer;
    
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    
    private void Start()
    {
        if (audioReactive == null)
        {
            audioReactive = FindFirstObjectByType<AudioReactive>();
        }
        
        CreateGridMaterial();
        CreateGridMesh();
        
        if (showSun)
        {
            CreateSun();
        }
        
        UpdateVisibility();
    }
    
    private void CreateGridMaterial()
    {
        // Use an unlit shader for the grid
        var shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        
        _gridMaterial = new Material(shader);
        _gridMaterial.color = gridColor;
        _meshRenderer.material = _gridMaterial;
    }
    
    private void CreateGridMesh()
    {
        _gridMesh = new Mesh();
        _gridMesh.name = "SynthwaveGrid";
        
        RegenerateMesh();
        
        _meshFilter.mesh = _gridMesh;
    }
    
    private void RegenerateMesh()
    {
        int lineCount = gridSize * 2 + 2; // Horizontal + vertical lines
        int vertexCount = lineCount * 2;
        
        Vector3[] vertices = new Vector3[vertexCount];
        int[] indices = new int[vertexCount];
        Color[] colors = new Color[vertexCount];
        
        int v = 0;
        float halfSize = gridSize * cellSize * 0.5f;
        
        // Vertical lines (going into distance)
        for (int i = 0; i <= gridSize; i++)
        {
            float x = (i - gridSize * 0.5f) * cellSize;
            
            vertices[v] = new Vector3(x, 0, 0);
            vertices[v + 1] = new Vector3(x, 0, halfSize * 2);
            
            // Fade alpha with distance
            colors[v] = gridColor;
            colors[v + 1] = new Color(horizonColor.r, horizonColor.g, horizonColor.b, 0.1f);
            
            indices[v] = v;
            indices[v + 1] = v + 1;
            
            v += 2;
        }
        
        // Horizontal lines
        for (int i = 0; i <= gridSize; i++)
        {
            float z = i * cellSize;
            
            vertices[v] = new Vector3(-halfSize, 0, z);
            vertices[v + 1] = new Vector3(halfSize, 0, z);
            
            // Color based on distance
            float t = (float)i / gridSize;
            Color lineColor = Color.Lerp(gridColor, horizonColor, t * horizonBlend);
            lineColor.a = Mathf.Lerp(1f, 0.2f, t);
            
            colors[v] = lineColor;
            colors[v + 1] = lineColor;
            
            indices[v] = v;
            indices[v + 1] = v + 1;
            
            v += 2;
        }
        
        _gridMesh.Clear();
        _gridMesh.vertices = vertices;
        _gridMesh.colors = colors;
        _gridMesh.SetIndices(indices, MeshTopology.Lines, 0);
    }
    
    private void CreateSun()
    {
        _sunObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _sunObject.name = "SynthwaveSun";
        _sunObject.transform.SetParent(transform);
        
        // Remove collider
        var collider = _sunObject.GetComponent<Collider>();
        if (collider != null) DestroyImmediate(collider);
        
        // Create sun material
        var shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        
        var sunMat = new Material(shader);
        sunMat.color = sunColor;
        
        _sunRenderer = _sunObject.GetComponent<MeshRenderer>();
        _sunRenderer.material = sunMat;
        
        UpdateSunPosition();
    }
    
    private void UpdateSunPosition()
    {
        if (_sunObject == null) return;
        
        _sunObject.transform.localPosition = new Vector3(0, sunHeight, gridSize * cellSize);
        _sunObject.transform.localScale = Vector3.one * sunSize;
        _sunObject.SetActive(showSun && enableGrid);
    }
    
    private void Update()
    {
        UpdateVisibility();
        
        if (!enableGrid) return;
        
        // Scroll animation
        float speed = scrollSpeed;
        
        // Audio reactive scroll
        if (reactToAudio && audioReactive != null && audioReactive.enableAudio)
        {
            speed += audioReactive.bass * audioScrollMultiplier * 5f;
        }
        
        _scrollOffset += speed * Time.deltaTime;
        if (_scrollOffset > cellSize)
        {
            _scrollOffset -= cellSize;
        }
        
        // Apply scroll via position
        Vector3 pos = transform.localPosition;
        pos.z = gridDistance - _scrollOffset;
        transform.localPosition = pos;
        
        // Update transform
        transform.localPosition = new Vector3(0, gridHeight, gridDistance - _scrollOffset);
        transform.localRotation = Quaternion.Euler(gridTilt, 0, 0);
        
        // Animate colors
        if (animateColors)
        {
            _hueOffset += colorCycleSpeed * Time.deltaTime;
            if (_hueOffset > 1f) _hueOffset -= 1f;
            
            Color.RGBToHSV(gridColor, out float h, out float s, out float v);
            Color newColor = Color.HSVToRGB((h + _hueOffset) % 1f, s, v);
            _gridMaterial.color = newColor;
        }
        
        // Pulse glow
        if (pulseGlow)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float currentGlow = glowIntensity * (0.5f + pulse * 0.5f);
            
            // Audio reactive glow
            if (reactToAudio && audioReactive != null && audioReactive.enableAudio)
            {
                currentGlow += audioReactive.overall * audioGlowMultiplier;
            }
            
            // Would need a custom shader to actually apply glow
        }
        
        // Update sun
        if (showSun && _sunObject != null)
        {
            UpdateSunPosition();
            
            if (animateColors && _sunRenderer != null)
            {
                Color.RGBToHSV(sunColor, out float h, out float s, out float v);
                Color newSunColor = Color.HSVToRGB((h + _hueOffset) % 1f, s, v);
                _sunRenderer.material.color = newSunColor;
            }
        }
    }
    
    private void UpdateVisibility()
    {
        _meshRenderer.enabled = enableGrid;
        if (_sunObject != null)
        {
            _sunObject.SetActive(showSun && enableGrid);
        }
    }
    
    /// <summary>
    /// Toggle grid on/off
    /// </summary>
    public void Toggle()
    {
        enableGrid = !enableGrid;
        UpdateVisibility();
    }
    
    /// <summary>
    /// Set grid color scheme
    /// </summary>
    public void SetColors(Color grid, Color horizon, Color sun)
    {
        gridColor = grid;
        horizonColor = horizon;
        sunColor = sun;
        
        if (_gridMaterial != null)
        {
            _gridMaterial.color = gridColor;
        }
        
        if (_sunRenderer != null)
        {
            _sunRenderer.material.color = sunColor;
        }
        
        RegenerateMesh();
    }
    
    /// <summary>
    /// Apply a synthwave preset
    /// </summary>
    public void ApplySynthwavePreset()
    {
        gridColor = new Color(1f, 0f, 0.5f);
        horizonColor = new Color(0.5f, 0f, 1f);
        sunColor = new Color(1f, 0.3f, 0f);
        showSun = true;
        glowIntensity = 0.8f;
        
        SetColors(gridColor, horizonColor, sunColor);
    }
    
    /// <summary>
    /// Apply a tron preset
    /// </summary>
    public void ApplyTronPreset()
    {
        gridColor = new Color(0f, 0.8f, 1f);
        horizonColor = new Color(0f, 0.4f, 0.8f);
        sunColor = new Color(1f, 0.5f, 0f);
        showSun = false;
        glowIntensity = 1f;
        
        SetColors(gridColor, horizonColor, sunColor);
    }
    
    private void OnDestroy()
    {
        if (_gridMaterial != null)
        {
            DestroyImmediate(_gridMaterial);
        }
        if (_gridMesh != null)
        {
            DestroyImmediate(_gridMesh);
        }
        if (_sunObject != null)
        {
            DestroyImmediate(_sunObject);
        }
    }
}
