using UnityEngine;
using System;

/// <summary>
/// Screen shake effect for Rutt/Etra.
/// Adds camera shake triggered by beats, manually, or continuously.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public bool enableShake = false;
    [Range(0f, 1f)] public float shakeIntensity = 0.3f;
    [Range(0.05f, 1f)] public float shakeDuration = 0.2f;
    [Range(1f, 50f)] public float shakeFrequency = 25f;
    
    [Header("Shake Type")]
    public ShakeType shakeType = ShakeType.Perlin;
    public bool shakePosition = true;
    public bool shakeRotation = true;
    [Range(0f, 10f)] public float rotationMultiplier = 2f;
    
    [Header("Continuous Shake")]
    public bool continuousShake = false;
    [Range(0f, 0.5f)] public float continuousIntensity = 0.1f;
    
    [Header("Beat Reactive")]
    public bool shakeOnBeat = false;
    public AudioReactive audioReactive;
    [Range(0f, 2f)] public float beatShakeMultiplier = 1f;
    
    [Header("Audio Reactive Continuous")]
    public bool audioReactiveContinuous = false;
    [Range(0f, 1f)] public float bassShakeAmount = 0.3f;
    
    [Header("Directional Shake")]
    public bool directionalShake = false;
    public Vector3 shakeDirection = new Vector3(1, 1, 0);
    
    public enum ShakeType
    {
        Perlin,
        Random,
        Sine,
        Bounce
    }
    
    // Events
    public event Action OnShake;
    
    // State
    private Camera _camera;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private float _shakeTimer;
    private float _currentIntensity;
    private float _trauma; // Accumulated shake energy
    private Vector3 _perlinSeed;
    
    private void Start()
    {
        _camera = Camera.main;
        
        if (_camera != null)
        {
            _originalPosition = _camera.transform.localPosition;
            _originalRotation = _camera.transform.localRotation;
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
        
        // Random seed for Perlin noise
        _perlinSeed = new Vector3(
            UnityEngine.Random.Range(0f, 100f),
            UnityEngine.Random.Range(0f, 100f),
            UnityEngine.Random.Range(0f, 100f)
        );
    }
    
    private void OnDestroy()
    {
        if (audioReactive != null)
        {
            audioReactive.OnBeat -= OnBeatDetected;
        }
        
        // Restore camera position
        if (_camera != null)
        {
            _camera.transform.localPosition = _originalPosition;
            _camera.transform.localRotation = _originalRotation;
        }
    }
    
    private void OnBeatDetected()
    {
        if (shakeOnBeat && enableShake)
        {
            TriggerShake(shakeIntensity * beatShakeMultiplier);
        }
    }
    
    private void Update()
    {
        if (!enableShake || _camera == null) return;
        
        // Decay trauma over time
        _trauma = Mathf.Max(0, _trauma - Time.deltaTime / shakeDuration);
        
        // Add continuous trauma
        if (continuousShake)
        {
            _trauma = Mathf.Max(_trauma, continuousIntensity);
        }
        
        // Audio reactive continuous shake
        if (audioReactiveContinuous && audioReactive != null && audioReactive.enableAudio)
        {
            float audioTrauma = audioReactive.bass * bassShakeAmount;
            _trauma = Mathf.Max(_trauma, audioTrauma);
        }
        
        // Apply shake
        if (_trauma > 0)
        {
            ApplyShake();
        }
        else
        {
            // Reset to original
            _camera.transform.localPosition = _originalPosition;
            _camera.transform.localRotation = _originalRotation;
        }
    }
    
    private void ApplyShake()
    {
        float shake = _trauma * _trauma * shakeIntensity; // Quadratic falloff
        float time = Time.time * shakeFrequency;
        
        Vector3 offset = Vector3.zero;
        Vector3 rotOffset = Vector3.zero;
        
        switch (shakeType)
        {
            case ShakeType.Perlin:
                offset = GetPerlinShake(time, shake);
                rotOffset = GetPerlinShake(time + 100f, shake * rotationMultiplier);
                break;
                
            case ShakeType.Random:
                offset = GetRandomShake(shake);
                rotOffset = GetRandomShake(shake * rotationMultiplier);
                break;
                
            case ShakeType.Sine:
                offset = GetSineShake(time, shake);
                rotOffset = GetSineShake(time + 1f, shake * rotationMultiplier);
                break;
                
            case ShakeType.Bounce:
                offset = GetBounceShake(time, shake);
                rotOffset = GetBounceShake(time, shake * rotationMultiplier);
                break;
        }
        
        // Apply directional constraint
        if (directionalShake)
        {
            offset = Vector3.Scale(offset, shakeDirection.normalized);
        }
        
        // Apply position shake
        if (shakePosition)
        {
            _camera.transform.localPosition = _originalPosition + offset;
        }
        
        // Apply rotation shake
        if (shakeRotation)
        {
            Quaternion shakeRot = Quaternion.Euler(rotOffset);
            _camera.transform.localRotation = _originalRotation * shakeRot;
        }
    }
    
    private Vector3 GetPerlinShake(float time, float intensity)
    {
        return new Vector3(
            (Mathf.PerlinNoise(_perlinSeed.x + time, 0) - 0.5f) * 2f * intensity,
            (Mathf.PerlinNoise(_perlinSeed.y + time, 0) - 0.5f) * 2f * intensity,
            (Mathf.PerlinNoise(_perlinSeed.z + time, 0) - 0.5f) * 2f * intensity
        );
    }
    
    private Vector3 GetRandomShake(float intensity)
    {
        return new Vector3(
            UnityEngine.Random.Range(-1f, 1f) * intensity,
            UnityEngine.Random.Range(-1f, 1f) * intensity,
            UnityEngine.Random.Range(-1f, 1f) * intensity
        );
    }
    
    private Vector3 GetSineShake(float time, float intensity)
    {
        return new Vector3(
            Mathf.Sin(time * 1.1f) * intensity,
            Mathf.Sin(time * 1.3f) * intensity,
            Mathf.Sin(time * 0.9f) * intensity
        );
    }
    
    private Vector3 GetBounceShake(float time, float intensity)
    {
        float bounce = Mathf.Abs(Mathf.Sin(time * 2f)) * intensity;
        return new Vector3(
            Mathf.Sin(time) * bounce,
            bounce,
            0
        );
    }
    
    /// <summary>
    /// Trigger a shake effect
    /// </summary>
    public void TriggerShake(float intensity = -1)
    {
        if (intensity < 0) intensity = shakeIntensity;
        _trauma = Mathf.Min(1f, _trauma + intensity);
        Debug.Log($"[ScreenShake] TriggerShake intensity={intensity:F2}, trauma={_trauma:F2}");
        OnShake?.Invoke();
    }
    
    /// <summary>
    /// Trigger a big shake
    /// </summary>
    public void TriggerBigShake()
    {
        TriggerShake(1f);
    }
    
    /// <summary>
    /// Toggle shake on/off
    /// </summary>
    public void Toggle()
    {
        enableShake = !enableShake;
        if (!enableShake && _camera != null)
        {
            _camera.transform.localPosition = _originalPosition;
            _camera.transform.localRotation = _originalRotation;
        }
    }
    
    /// <summary>
    /// Set shake intensity
    /// </summary>
    public void SetIntensity(float intensity)
    {
        shakeIntensity = Mathf.Clamp01(intensity);
    }
    
    /// <summary>
    /// Add trauma (accumulates)
    /// </summary>
    public void AddTrauma(float amount)
    {
        _trauma = Mathf.Min(1f, _trauma + amount);
    }
}
