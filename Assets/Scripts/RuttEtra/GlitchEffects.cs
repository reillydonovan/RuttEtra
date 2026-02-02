using UnityEngine;

/// <summary>
/// Digital glitch effects for the Rutt/Etra mesh.
/// Includes pixel displacement, RGB split, scan corruption, and data moshing.
/// </summary>
public class GlitchEffects : MonoBehaviour
{
    [Header("Master Control")]
    public bool enableGlitch = false;
    [Range(0f, 1f)] public float glitchIntensity = 0.5f;
    
    [Header("Vertex Displacement Glitch")]
    public bool enableVertexGlitch = true;
    [Range(0f, 1f)] public float vertexGlitchAmount = 0.3f;
    [Range(0f, 10f)] public float vertexGlitchSpeed = 5f;
    [Range(0f, 1f)] public float vertexGlitchChance = 0.1f;
    
    [Header("Scan Line Disruption")]
    public bool enableScanDisrupt = true;
    [Range(0f, 1f)] public float scanDisruptAmount = 0.2f;
    [Range(1f, 50f)] public float scanDisruptFrequency = 10f;
    
    [Header("Block Displacement")]
    public bool enableBlockGlitch = true;
    [Range(0f, 1f)] public float blockGlitchAmount = 0.3f;
    [Range(1, 20)] public int blockSize = 5;
    [Range(0f, 1f)] public float blockGlitchChance = 0.05f;
    
    [Header("Wave Corruption")]
    public bool enableWaveCorrupt = false;
    [Range(0f, 2f)] public float waveCorruptAmount = 0.5f;
    [Range(0.1f, 10f)] public float waveCorruptFrequency = 2f;
    
    [Header("Time Stretch")]
    public bool enableTimeStretch = false;
    [Range(0f, 1f)] public float timeStretchAmount = 0.3f;
    
    [Header("Random Trigger")]
    public bool autoTrigger = true;
    [Range(0.1f, 5f)] public float triggerInterval = 1f;
    [Range(0.05f, 1f)] public float triggerDuration = 0.2f;
    
    [Header("Audio Reactive")]
    public bool glitchOnBeat = false;
    public AudioReactive audioReactive;
    
    // Runtime state
    private float _glitchTimer;
    private float _triggerTimer;
    private bool _isGlitching;
    private float _currentGlitchDuration;
    private Vector3[] _blockOffsets;
    private float _timeOffset;
    
    // Reference to mesh generator
    private RuttEtraMeshGenerator _meshGenerator;
    private RuttEtraSettings _settings;
    
    private void Start()
    {
        _meshGenerator = FindFirstObjectByType<RuttEtraMeshGenerator>();
        
        var controller = FindFirstObjectByType<RuttEtraController>();
        if (controller != null)
        {
            _settings = controller.settings;
        }
        
        if (audioReactive == null)
        {
            audioReactive = FindFirstObjectByType<AudioReactive>();
        }
        
        // Subscribe to beat events
        if (audioReactive != null)
        {
            audioReactive.OnBeat += OnBeatDetected;
        }
        
        _blockOffsets = new Vector3[100];
        RandomizeBlockOffsets();
    }
    
    private void OnDestroy()
    {
        if (audioReactive != null)
        {
            audioReactive.OnBeat -= OnBeatDetected;
        }
    }
    
    private void OnBeatDetected()
    {
        if (glitchOnBeat && enableGlitch)
        {
            TriggerGlitch(triggerDuration);
        }
    }
    
    /// <summary>
    /// Manually trigger a glitch effect
    /// </summary>
    public void TriggerGlitch(float duration = 0.2f)
    {
        _isGlitching = true;
        _currentGlitchDuration = duration;
        _glitchTimer = 0f;
        RandomizeBlockOffsets();
        Debug.Log($"[GlitchEffects] Triggered glitch for {duration}s");
    }
    
    private void RandomizeBlockOffsets()
    {
        for (int i = 0; i < _blockOffsets.Length; i++)
        {
            _blockOffsets[i] = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            );
        }
    }
    
    private float _debugTimer2;
    
    private void Update()
    {
        if (!enableGlitch) return;
        
        // Debug output every 3 seconds
        _debugTimer2 += Time.deltaTime;
        if (_debugTimer2 >= 3f)
        {
            _debugTimer2 = 0f;
            Debug.Log($"[GlitchEffects] Active - autoTrigger={autoTrigger}, glitchOnBeat={glitchOnBeat}, isGlitching={_isGlitching}");
        }
        
        // Auto trigger logic
        if (autoTrigger && !glitchOnBeat)
        {
            _triggerTimer += Time.deltaTime;
            if (_triggerTimer >= triggerInterval)
            {
                _triggerTimer = 0f;
                if (Random.value < 0.5f) // 50% chance each interval
                {
                    TriggerGlitch(triggerDuration);
                }
            }
        }
        
        // Update glitch state
        if (_isGlitching)
        {
            _glitchTimer += Time.deltaTime;
            if (_glitchTimer >= _currentGlitchDuration)
            {
                _isGlitching = false;
            }
        }
        
        // Time stretch effect
        if (enableTimeStretch && _isGlitching)
        {
            _timeOffset += Time.deltaTime * timeStretchAmount * Random.Range(-1f, 1f);
        }
    }
    
    /// <summary>
    /// Apply glitch effects to vertex positions.
    /// Call this from the mesh generator after calculating base positions.
    /// </summary>
    public void ApplyGlitchToVertices(Vector3[] vertices, int width, int height)
    {
        if (!enableGlitch || !_isGlitching) return;
        
        float intensity = glitchIntensity;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index >= vertices.Length) continue;
                
                Vector3 offset = Vector3.zero;
                
                // Vertex displacement glitch
                if (enableVertexGlitch && Random.value < vertexGlitchChance)
                {
                    offset += new Vector3(
                        Random.Range(-1f, 1f) * vertexGlitchAmount,
                        Random.Range(-1f, 1f) * vertexGlitchAmount,
                        Random.Range(-1f, 1f) * vertexGlitchAmount
                    ) * intensity;
                }
                
                // Scan line disruption
                if (enableScanDisrupt)
                {
                    float scanPhase = Mathf.Sin(y * scanDisruptFrequency + Time.time * vertexGlitchSpeed);
                    if (Mathf.Abs(scanPhase) > 0.9f)
                    {
                        offset.x += scanDisruptAmount * intensity * Mathf.Sign(scanPhase);
                    }
                }
                
                // Block displacement
                if (enableBlockGlitch)
                {
                    int blockX = x / blockSize;
                    int blockY = y / blockSize;
                    int blockIndex = (blockY * (width / blockSize + 1) + blockX) % _blockOffsets.Length;
                    
                    if (Random.value < blockGlitchChance || (_isGlitching && blockIndex % 3 == 0))
                    {
                        offset += _blockOffsets[blockIndex] * blockGlitchAmount * intensity;
                    }
                }
                
                // Wave corruption
                if (enableWaveCorrupt)
                {
                    float wave = Mathf.Sin(x * waveCorruptFrequency + Time.time * 3f);
                    wave += Mathf.Sin(y * waveCorruptFrequency * 0.7f + Time.time * 2f);
                    offset.z += wave * waveCorruptAmount * intensity * 0.5f;
                }
                
                vertices[index] += offset;
            }
        }
    }
    
    /// <summary>
    /// Get time offset for time stretch effect
    /// </summary>
    public float GetTimeOffset()
    {
        return enableTimeStretch ? _timeOffset : 0f;
    }
    
    /// <summary>
    /// Check if currently glitching
    /// </summary>
    public bool IsGlitching => _isGlitching;
    
    /// <summary>
    /// Get current glitch progress (0-1)
    /// </summary>
    public float GlitchProgress => _isGlitching ? _glitchTimer / _currentGlitchDuration : 0f;
}
