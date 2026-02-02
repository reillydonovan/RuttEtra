using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Motion trails/afterimage effect for Rutt/Etra.
/// Creates ghost copies of the mesh at previous positions.
/// </summary>
public class MotionTrails : MonoBehaviour
{
    [Header("Trail Settings")]
    public bool enableTrails = false;
    [Range(1, 20)] public int trailCount = 5;
    [Range(0.01f, 0.5f)] public float trailInterval = 0.05f;
    
    [Header("Appearance")]
    [Range(0f, 1f)] public float trailOpacity = 0.5f;
    public TrailFadeMode fadeMode = TrailFadeMode.Linear;
    public bool colorShift = true;
    [Range(0f, 0.5f)] public float hueShiftAmount = 0.1f;
    
    [Header("Transform")]
    public bool scaleTrails = false;
    [Range(0.8f, 1.2f)] public float scaleMultiplier = 0.95f;
    public bool rotateTrails = false;
    [Range(-10f, 10f)] public float rotationOffset = 2f;
    
    [Header("Position Offset")]
    public bool offsetTrails = false;
    public Vector3 offsetDirection = new Vector3(0, 0, -0.1f);
    
    [Header("Audio Reactive")]
    public bool audioReactiveCount = false;
    public AudioReactive audioReactive;
    [Range(1, 10)] public int minTrails = 2;
    [Range(5, 20)] public int maxTrails = 10;
    
    public enum TrailFadeMode
    {
        Linear,
        Exponential,
        Stepped,
        Reverse
    }
    
    // Trail data
    private List<TrailFrame> _trailFrames;
    private float _lastFrameTime;
    private RuttEtraMeshGenerator _meshGenerator;
    private MeshFilter _sourceMeshFilter;
    private Material _trailMaterial;
    
    // Trail objects
    private List<GameObject> _trailObjects;
    private List<MeshFilter> _trailMeshFilters;
    private List<MeshRenderer> _trailRenderers;
    
    private class TrailFrame
    {
        public Vector3[] vertices;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public float timestamp;
    }
    
    private void Start()
    {
        _meshGenerator = FindFirstObjectByType<RuttEtraMeshGenerator>();
        if (_meshGenerator != null)
        {
            _sourceMeshFilter = _meshGenerator.GetComponent<MeshFilter>();
        }
        
        if (audioReactive == null)
        {
            audioReactive = FindFirstObjectByType<AudioReactive>();
        }
        
        _trailFrames = new List<TrailFrame>();
        _trailObjects = new List<GameObject>();
        _trailMeshFilters = new List<MeshFilter>();
        _trailRenderers = new List<MeshRenderer>();
        
        CreateTrailMaterial();
        CreateTrailObjects();
    }
    
    private void CreateTrailMaterial()
    {
        // Try to get the same shader as the main mesh
        var shader = Shader.Find("RuttEtra/ScanLine");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        
        _trailMaterial = new Material(shader);
    }
    
    private void CreateTrailObjects()
    {
        // Clean up existing
        foreach (var obj in _trailObjects)
        {
            if (obj != null) Destroy(obj);
        }
        _trailObjects.Clear();
        _trailMeshFilters.Clear();
        _trailRenderers.Clear();
        
        // Create new trail objects
        for (int i = 0; i < trailCount; i++)
        {
            var trailObj = new GameObject($"Trail_{i}");
            trailObj.transform.SetParent(transform);
            
            var meshFilter = trailObj.AddComponent<MeshFilter>();
            var meshRenderer = trailObj.AddComponent<MeshRenderer>();
            
            meshRenderer.material = new Material(_trailMaterial);
            trailObj.SetActive(false);
            
            _trailObjects.Add(trailObj);
            _trailMeshFilters.Add(meshFilter);
            _trailRenderers.Add(meshRenderer);
        }
    }
    
    private void Update()
    {
        if (!enableTrails || _sourceMeshFilter == null) return;
        
        // Adjust trail count based on audio
        int currentTrailCount = trailCount;
        if (audioReactiveCount && audioReactive != null && audioReactive.enableAudio)
        {
            currentTrailCount = Mathf.RoundToInt(Mathf.Lerp(minTrails, maxTrails, audioReactive.overall));
        }
        
        // Ensure we have enough trail objects
        if (_trailObjects.Count != currentTrailCount)
        {
            trailCount = currentTrailCount;
            CreateTrailObjects();
        }
        
        // Capture new frame
        if (Time.time - _lastFrameTime >= trailInterval)
        {
            CaptureFrame();
            _lastFrameTime = Time.time;
        }
        
        // Update trail visuals
        UpdateTrails();
    }
    
    private void CaptureFrame()
    {
        if (_sourceMeshFilter.sharedMesh == null) return;
        
        var frame = new TrailFrame
        {
            vertices = _sourceMeshFilter.sharedMesh.vertices.Clone() as Vector3[],
            position = _sourceMeshFilter.transform.position,
            rotation = _sourceMeshFilter.transform.rotation,
            scale = _sourceMeshFilter.transform.localScale,
            timestamp = Time.time
        };
        
        _trailFrames.Insert(0, frame);
        
        // Limit frame count
        while (_trailFrames.Count > trailCount)
        {
            _trailFrames.RemoveAt(_trailFrames.Count - 1);
        }
    }
    
    private void UpdateTrails()
    {
        for (int i = 0; i < _trailObjects.Count; i++)
        {
            if (i >= _trailFrames.Count)
            {
                _trailObjects[i].SetActive(false);
                continue;
            }
            
            var frame = _trailFrames[i];
            var trailObj = _trailObjects[i];
            var meshFilter = _trailMeshFilters[i];
            var meshRenderer = _trailRenderers[i];
            
            trailObj.SetActive(true);
            
            // Calculate fade based on trail index
            float t = (float)(i + 1) / trailCount;
            float opacity = CalculateFade(t) * trailOpacity;
            
            // Update mesh
            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            
            // Copy mesh data - handle line topology properly
            var sourceMesh = _sourceMeshFilter.sharedMesh;
            var destMesh = meshFilter.sharedMesh;
            
            destMesh.Clear();
            destMesh.vertices = frame.vertices;
            
            // Copy colors if available
            if (sourceMesh.colors != null && sourceMesh.colors.Length > 0)
            {
                destMesh.colors = sourceMesh.colors;
            }
            
            // Copy indices using the correct topology (Lines, not Triangles)
            for (int sub = 0; sub < sourceMesh.subMeshCount; sub++)
            {
                var topology = sourceMesh.GetTopology(sub);
                var indices = sourceMesh.GetIndices(sub);
                destMesh.SetIndices(indices, topology, sub);
            }
            
            // Update transform
            Vector3 pos = frame.position;
            Quaternion rot = frame.rotation;
            Vector3 scale = frame.scale;
            
            if (offsetTrails)
            {
                pos += offsetDirection * (i + 1);
            }
            
            if (rotateTrails)
            {
                rot *= Quaternion.Euler(0, rotationOffset * (i + 1), 0);
            }
            
            if (scaleTrails)
            {
                float scaleAmount = Mathf.Pow(scaleMultiplier, i + 1);
                scale *= scaleAmount;
            }
            
            trailObj.transform.position = pos;
            trailObj.transform.rotation = rot;
            trailObj.transform.localScale = scale;
            
            // Update material
            if (meshRenderer.material != null)
            {
                Color color = meshRenderer.material.color;
                color.a = opacity;
                
                if (colorShift)
                {
                    Color.RGBToHSV(color, out float h, out float s, out float v);
                    h = (h + hueShiftAmount * (i + 1)) % 1f;
                    color = Color.HSVToRGB(h, s, v);
                    color.a = opacity;
                }
                
                meshRenderer.material.color = color;
            }
        }
    }
    
    private float CalculateFade(float t)
    {
        switch (fadeMode)
        {
            case TrailFadeMode.Linear:
                return 1f - t;
                
            case TrailFadeMode.Exponential:
                return Mathf.Pow(1f - t, 2f);
                
            case TrailFadeMode.Stepped:
                return Mathf.Floor((1f - t) * 4f) / 4f;
                
            case TrailFadeMode.Reverse:
                return t;
                
            default:
                return 1f - t;
        }
    }
    
    /// <summary>
    /// Toggle trails on/off
    /// </summary>
    public void Toggle()
    {
        enableTrails = !enableTrails;
        if (!enableTrails)
        {
            ClearTrails();
        }
    }
    
    /// <summary>
    /// Clear all trail frames
    /// </summary>
    public void ClearTrails()
    {
        _trailFrames.Clear();
        foreach (var obj in _trailObjects)
        {
            obj.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        foreach (var obj in _trailObjects)
        {
            if (obj != null) Destroy(obj);
        }
        
        if (_trailMaterial != null)
        {
            Destroy(_trailMaterial);
        }
    }
    
    private void OnDisable()
    {
        ClearTrails();
    }
}
