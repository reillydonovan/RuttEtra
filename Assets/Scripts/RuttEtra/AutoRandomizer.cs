using UnityEngine;
using System;

/// <summary>
/// Auto-randomizer for Rutt/Etra parameters. Creates evolving, 
/// ever-changing visuals through smooth parameter drift using Perlin noise.
/// </summary>
public class AutoRandomizer : MonoBehaviour
{
    [Header("Master Control")]
    public bool enableRandomizer = false;
    [Range(0.01f, 1f)] public float globalSpeed = 0.1f;
    [Range(0f, 1f)] public float globalIntensity = 0.5f;
    
    [Header("Displacement")]
    public bool randomizeDisplacement = true;
    [Range(0f, 1f)] public float displacementIntensity = 0.5f;
    [Range(0.5f, 3f)] public float displacementMin = 0.5f;
    [Range(1f, 5f)] public float displacementMax = 3f;
    
    [Header("Wave")]
    public bool randomizeWave = true;
    [Range(0f, 1f)] public float waveIntensity = 0.3f;
    [Range(0f, 1f)] public float waveMin = 0f;
    [Range(0.5f, 2f)] public float waveMax = 1f;
    
    [Header("Rotation")]
    public bool randomizeRotation = true;
    [Range(0f, 1f)] public float rotationIntensity = 0.3f;
    [Range(0f, 90f)] public float rotationRange = 30f;
    
    [Header("Colors")]
    public bool randomizeHue = true;
    [Range(0f, 1f)] public float hueIntensity = 0.2f;
    [Range(0.01f, 0.5f)] public float hueSpeed = 0.05f;
    
    [Header("Line Style")]
    public bool randomizeLineWidth = false;
    [Range(0f, 1f)] public float lineWidthIntensity = 0.3f;
    [Range(0.001f, 0.02f)] public float lineWidthMin = 0.002f;
    [Range(0.01f, 0.05f)] public float lineWidthMax = 0.02f;
    
    [Header("Glow")]
    public bool randomizeGlow = true;
    [Range(0f, 1f)] public float glowIntensity = 0.4f;
    [Range(0f, 1f)] public float glowMin = 0.1f;
    [Range(0.5f, 2f)] public float glowMax = 1.5f;
    
    [Header("Distortion")]
    public bool randomizeDistortion = false;
    [Range(0f, 1f)] public float distortionIntensity = 0.2f;
    
    [Header("Scale")]
    public bool randomizeScale = false;
    [Range(0f, 1f)] public float scaleIntensity = 0.2f;
    [Range(0.5f, 1f)] public float scaleMin = 0.8f;
    [Range(1f, 1.5f)] public float scaleMax = 1.2f;
    
    [Header("Advanced")]
    public bool useDifferentSpeeds = true;
    [Range(0.5f, 2f)] public float speedVariation = 1.5f;
    public bool snapToBeats = false;
    public AudioReactive audioReactive;
    
    [Header("References")]
    public RuttEtraSettings settings;
    
    // Events
    public event Action OnRandomize;
    
    // Noise offsets for each parameter
    private float _noiseOffsetDisp;
    private float _noiseOffsetWaveH;
    private float _noiseOffsetWaveV;
    private float _noiseOffsetRotX;
    private float _noiseOffsetRotY;
    private float _noiseOffsetRotZ;
    private float _noiseOffsetHue;
    private float _noiseOffsetLineWidth;
    private float _noiseOffsetGlow;
    private float _noiseOffsetDistH;
    private float _noiseOffsetDistV;
    private float _noiseOffsetScale;
    private float _noiseOffsetWaveFreq;
    
    // Base values
    private float _baseDisplacement;
    private float _baseWaveH, _baseWaveV;
    private float _baseRotX, _baseRotY, _baseRotZ;
    private float _baseHue;
    private float _baseLineWidth;
    private float _baseGlow;
    private float _baseKeystoneH, _baseKeystoneV;
    private float _baseScale;
    private float _baseWaveFreq;
    private bool _basesCaptured;
    
    private void Start()
    {
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller != null) settings = controller.settings;
        }
        
        if (audioReactive == null)
        {
            audioReactive = FindFirstObjectByType<AudioReactive>();
        }
        
        // Initialize noise offsets with random values
        RandomizeNoiseOffsets();
    }
    
    /// <summary>
    /// Randomize all noise offsets for new patterns
    /// </summary>
    public void RandomizeNoiseOffsets()
    {
        _noiseOffsetDisp = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetWaveH = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetWaveV = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetRotX = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetRotY = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetRotZ = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetHue = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetLineWidth = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetGlow = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetDistH = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetDistV = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetScale = UnityEngine.Random.Range(0f, 1000f);
        _noiseOffsetWaveFreq = UnityEngine.Random.Range(0f, 1000f);
        
        OnRandomize?.Invoke();
    }
    
    /// <summary>
    /// Capture current settings as base values
    /// </summary>
    public void CaptureBaseValues()
    {
        if (settings == null) return;
        
        _baseDisplacement = settings.displacementStrength;
        _baseWaveH = settings.horizontalWave;
        _baseWaveV = settings.verticalWave;
        _baseRotX = settings.rotationX;
        _baseRotY = settings.rotationY;
        _baseRotZ = settings.rotationZ;
        Color.RGBToHSV(settings.primaryColor, out _baseHue, out _, out _);
        _baseLineWidth = settings.lineWidth;
        _baseGlow = settings.glowIntensity;
        _baseKeystoneH = settings.keystoneH;
        _baseKeystoneV = settings.keystoneV;
        _baseScale = settings.meshScale;
        _baseWaveFreq = settings.waveFrequency;
        _basesCaptured = true;
    }
    
    private float _debugTimer;
    
    private void Update()
    {
        if (!enableRandomizer || settings == null) return;
        
        if (!_basesCaptured)
        {
            CaptureBaseValues();
        }
        
        // Debug output every 3 seconds
        _debugTimer += Time.deltaTime;
        if (_debugTimer >= 3f)
        {
            _debugTimer = 0f;
            Debug.Log($"[AutoRandomizer] Active - speed={globalSpeed:F2}, intensity={globalIntensity:F2}, disp={randomizeDisplacement}, wave={randomizeWave}, rot={randomizeRotation}, hue={randomizeHue}");
        }
        
        float time = Time.time * globalSpeed;
        float intensity = globalIntensity;
        
        // Displacement
        if (randomizeDisplacement)
        {
            float speed = useDifferentSpeeds ? 1f : speedVariation;
            float noise = Mathf.PerlinNoise(time * speed + _noiseOffsetDisp, 0f);
            float value = Mathf.Lerp(displacementMin, displacementMax, noise);
            settings.displacementStrength = Mathf.Lerp(_baseDisplacement, value, displacementIntensity * intensity);
        }
        
        // Wave
        if (randomizeWave)
        {
            float speedH = useDifferentSpeeds ? 0.8f : speedVariation;
            float speedV = useDifferentSpeeds ? 1.2f : speedVariation;
            
            float noiseH = Mathf.PerlinNoise(time * speedH + _noiseOffsetWaveH, 0f);
            float noiseV = Mathf.PerlinNoise(time * speedV + _noiseOffsetWaveV, 0f);
            
            float valueH = Mathf.Lerp(waveMin, waveMax, noiseH);
            float valueV = Mathf.Lerp(waveMin, waveMax, noiseV);
            
            settings.horizontalWave = Mathf.Lerp(_baseWaveH, valueH, waveIntensity * intensity);
            settings.verticalWave = Mathf.Lerp(_baseWaveV, valueV, waveIntensity * intensity);
            
            // Also randomize frequency
            float noiseFreq = Mathf.PerlinNoise(time * 0.5f + _noiseOffsetWaveFreq, 0f);
            settings.waveFrequency = Mathf.Lerp(1f, 5f, noiseFreq * waveIntensity * intensity);
        }
        
        // Rotation
        if (randomizeRotation)
        {
            float speedX = useDifferentSpeeds ? 0.7f : speedVariation;
            float speedY = useDifferentSpeeds ? 1f : speedVariation;
            float speedZ = useDifferentSpeeds ? 1.3f : speedVariation;
            
            float noiseX = (Mathf.PerlinNoise(time * speedX + _noiseOffsetRotX, 0f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(time * speedY + _noiseOffsetRotY, 0f) - 0.5f) * 2f;
            float noiseZ = (Mathf.PerlinNoise(time * speedZ + _noiseOffsetRotZ, 0f) - 0.5f) * 2f;
            
            settings.rotationX = _baseRotX + noiseX * rotationRange * rotationIntensity * intensity;
            settings.rotationY = _baseRotY + noiseY * rotationRange * rotationIntensity * intensity;
            settings.rotationZ = _baseRotZ + noiseZ * rotationRange * rotationIntensity * intensity;
        }
        
        // Hue
        if (randomizeHue)
        {
            float hueNoise = Mathf.PerlinNoise(time * hueSpeed + _noiseOffsetHue, 0f);
            float newHue = (_baseHue + hueNoise * hueIntensity * intensity) % 1f;
            
            Color.RGBToHSV(settings.primaryColor, out _, out float s, out float v);
            settings.primaryColor = Color.HSVToRGB(newHue, s, v);
            
            // Secondary color offset
            float secHue = (newHue + 0.5f) % 1f; // Complementary
            Color.RGBToHSV(settings.secondaryColor, out _, out float s2, out float v2);
            settings.secondaryColor = Color.HSVToRGB(secHue, s2, v2);
        }
        
        // Line Width
        if (randomizeLineWidth)
        {
            float noise = Mathf.PerlinNoise(time * 0.6f + _noiseOffsetLineWidth, 0f);
            float value = Mathf.Lerp(lineWidthMin, lineWidthMax, noise);
            settings.lineWidth = Mathf.Lerp(_baseLineWidth, value, lineWidthIntensity * intensity);
        }
        
        // Glow
        if (randomizeGlow)
        {
            float noise = Mathf.PerlinNoise(time * 1.5f + _noiseOffsetGlow, 0f);
            float value = Mathf.Lerp(glowMin, glowMax, noise);
            settings.glowIntensity = Mathf.Lerp(_baseGlow, value, glowIntensity * intensity);
        }
        
        // Distortion
        if (randomizeDistortion)
        {
            float noiseH = (Mathf.PerlinNoise(time * 0.4f + _noiseOffsetDistH, 0f) - 0.5f) * 2f;
            float noiseV = (Mathf.PerlinNoise(time * 0.4f + _noiseOffsetDistV, 0f) - 0.5f) * 2f;
            
            settings.keystoneH = _baseKeystoneH + noiseH * 0.3f * distortionIntensity * intensity;
            settings.keystoneV = _baseKeystoneV + noiseV * 0.3f * distortionIntensity * intensity;
        }
        
        // Scale
        if (randomizeScale)
        {
            float noise = Mathf.PerlinNoise(time * 0.3f + _noiseOffsetScale, 0f);
            float value = Mathf.Lerp(scaleMin, scaleMax, noise);
            settings.meshScale = Mathf.Lerp(_baseScale, value, scaleIntensity * intensity);
        }
    }
    
    /// <summary>
    /// Toggle randomizer on/off
    /// </summary>
    public void Toggle()
    {
        enableRandomizer = !enableRandomizer;
        if (enableRandomizer)
        {
            CaptureBaseValues();
        }
    }
    
    /// <summary>
    /// Reset to base values
    /// </summary>
    public void ResetToBase()
    {
        if (settings == null || !_basesCaptured) return;
        
        settings.displacementStrength = _baseDisplacement;
        settings.horizontalWave = _baseWaveH;
        settings.verticalWave = _baseWaveV;
        settings.rotationX = _baseRotX;
        settings.rotationY = _baseRotY;
        settings.rotationZ = _baseRotZ;
        settings.lineWidth = _baseLineWidth;
        settings.glowIntensity = _baseGlow;
        settings.keystoneH = _baseKeystoneH;
        settings.keystoneV = _baseKeystoneV;
        settings.meshScale = _baseScale;
        settings.waveFrequency = _baseWaveFreq;
    }
}
