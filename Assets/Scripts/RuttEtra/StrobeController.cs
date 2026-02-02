using UnityEngine;
using System;

/// <summary>
/// Strobe and flash effects controller for Rutt/Etra.
/// Creates rhythmic flashing, beat-synced strobes, and flash effects.
/// </summary>
public class StrobeController : MonoBehaviour
{
    [Header("Strobe Settings")]
    public bool enableStrobe = false;
    [Range(0.5f, 30f)] public float strobeRate = 10f; // Flashes per second
    [Range(0f, 1f)] public float strobeDutyCycle = 0.5f; // On time vs off time
    
    [Header("Flash Settings")]
    public bool enableFlash = false;
    [Range(0.05f, 1f)] public float flashDuration = 0.1f;
    [Range(0f, 1f)] public float flashIntensity = 1f;
    
    [Header("Colors")]
    public Color strobeOnColor = Color.white;
    public Color strobeOffColor = Color.black;
    public bool affectBackground = true;
    public bool affectMeshColor = false;
    
    [Header("Beat Sync")]
    public bool syncToBeat = false;
    public AudioReactive audioReactive;
    [Range(0.5f, 4f)] public float beatMultiplier = 1f;
    public bool flashOnBeat = true;
    
    [Header("Pattern Mode")]
    public bool usePattern = false;
    public StrobePattern pattern = StrobePattern.Regular;
    
    [Header("Blackout")]
    public bool blackoutMode = false;
    [Range(0f, 10f)] public float blackoutDuration = 2f;
    
    [Header("References")]
    public RuttEtraSettings settings;
    
    public enum StrobePattern
    {
        Regular,
        Double,
        Triple,
        Syncopated,
        Random,
        Ramp
    }
    
    // Events
    public event Action OnFlash;
    public event Action OnBlackout;
    
    // State
    private float _strobeTimer;
    private bool _strobeState;
    private float _flashTimer;
    private bool _isFlashing;
    private float _blackoutTimer;
    private bool _isBlackout;
    private Color _originalBackgroundColor;
    private Color _originalPrimaryColor;
    private int _patternStep;
    private Camera _camera;
    
    private void Start()
    {
        _camera = Camera.main;
        
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller != null) settings = controller.settings;
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
        
        // Store original colors
        if (_camera != null)
        {
            _originalBackgroundColor = _camera.backgroundColor;
        }
        if (settings != null)
        {
            _originalPrimaryColor = settings.primaryColor;
        }
    }
    
    private void OnDestroy()
    {
        if (audioReactive != null)
        {
            audioReactive.OnBeat -= OnBeatDetected;
        }
        
        // Restore colors
        RestoreColors();
    }
    
    private void OnBeatDetected()
    {
        if (syncToBeat && enableStrobe)
        {
            // Trigger strobe on beat
            _strobeState = true;
            _strobeTimer = 0f;
        }
        
        if (flashOnBeat && enableFlash)
        {
            TriggerFlash();
        }
    }
    
    private void Update()
    {
        // Handle blackout
        if (_isBlackout)
        {
            _blackoutTimer -= Time.deltaTime;
            if (_blackoutTimer <= 0)
            {
                _isBlackout = false;
                RestoreColors();
            }
            else
            {
                ApplyBlackout();
                return;
            }
        }
        
        // Handle strobe
        if (enableStrobe && !syncToBeat)
        {
            UpdateStrobe();
        }
        
        // Handle flash
        if (_isFlashing)
        {
            UpdateFlash();
        }
    }
    
    private void UpdateStrobe()
    {
        float period = 1f / strobeRate;
        _strobeTimer += Time.deltaTime;
        
        if (usePattern)
        {
            UpdatePatternStrobe(period);
        }
        else
        {
            // Regular strobe
            if (_strobeTimer >= period)
            {
                _strobeTimer -= period;
                _strobeState = !_strobeState;
            }
            
            // Apply strobe based on duty cycle
            bool isOn = _strobeTimer < period * strobeDutyCycle;
            ApplyStrobeState(isOn);
        }
    }
    
    private void UpdatePatternStrobe(float period)
    {
        switch (pattern)
        {
            case StrobePattern.Double:
                // Two quick flashes then pause
                float doubleCycle = _strobeTimer % (period * 4);
                bool doubleOn = doubleCycle < period * 0.3f || 
                               (doubleCycle > period * 0.5f && doubleCycle < period * 0.8f);
                ApplyStrobeState(doubleOn);
                break;
                
            case StrobePattern.Triple:
                // Three quick flashes then pause
                float tripleCycle = _strobeTimer % (period * 6);
                bool tripleOn = tripleCycle < period * 0.2f || 
                               (tripleCycle > period * 0.4f && tripleCycle < period * 0.6f) ||
                               (tripleCycle > period * 0.8f && tripleCycle < period * 1f);
                ApplyStrobeState(tripleOn);
                break;
                
            case StrobePattern.Syncopated:
                // Off-beat pattern
                float syncCycle = _strobeTimer % (period * 2);
                bool syncOn = syncCycle > period * 0.5f && syncCycle < period * 0.7f ||
                             syncCycle > period * 1.5f && syncCycle < period * 1.7f;
                ApplyStrobeState(syncOn);
                break;
                
            case StrobePattern.Random:
                // Random flashes
                if (_strobeTimer >= period)
                {
                    _strobeTimer -= period;
                    _strobeState = UnityEngine.Random.value > 0.5f;
                }
                ApplyStrobeState(_strobeState);
                break;
                
            case StrobePattern.Ramp:
                // Ramping intensity
                float rampCycle = (_strobeTimer % period) / period;
                float rampIntensity = Mathf.PingPong(rampCycle * 2f, 1f);
                ApplyStrobeIntensity(rampIntensity);
                break;
                
            default:
                bool regularOn = (_strobeTimer % period) < period * strobeDutyCycle;
                ApplyStrobeState(regularOn);
                break;
        }
    }
    
    private void ApplyStrobeState(bool isOn)
    {
        Color targetColor = isOn ? strobeOnColor : strobeOffColor;
        
        if (affectBackground && _camera != null)
        {
            _camera.backgroundColor = isOn ? strobeOnColor : strobeOffColor;
        }
        
        if (affectMeshColor && settings != null)
        {
            settings.primaryColor = isOn ? strobeOnColor : _originalPrimaryColor;
        }
    }
    
    private void ApplyStrobeIntensity(float intensity)
    {
        Color targetColor = Color.Lerp(strobeOffColor, strobeOnColor, intensity);
        
        if (affectBackground && _camera != null)
        {
            _camera.backgroundColor = targetColor;
        }
        
        if (affectMeshColor && settings != null)
        {
            settings.primaryColor = Color.Lerp(_originalPrimaryColor, strobeOnColor, intensity);
        }
    }
    
    private void UpdateFlash()
    {
        _flashTimer -= Time.deltaTime;
        
        float t = _flashTimer / flashDuration;
        float intensity = t * flashIntensity;
        
        if (affectBackground && _camera != null)
        {
            _camera.backgroundColor = Color.Lerp(_originalBackgroundColor, strobeOnColor, intensity);
        }
        
        if (_flashTimer <= 0)
        {
            _isFlashing = false;
            RestoreColors();
        }
    }
    
    private void ApplyBlackout()
    {
        if (_camera != null)
        {
            _camera.backgroundColor = Color.black;
        }
        
        if (settings != null)
        {
            settings.primaryColor = Color.black;
            settings.secondaryColor = Color.black;
        }
    }
    
    private void RestoreColors()
    {
        if (_camera != null)
        {
            _camera.backgroundColor = _originalBackgroundColor;
        }
        
        if (settings != null)
        {
            settings.primaryColor = _originalPrimaryColor;
        }
    }
    
    /// <summary>
    /// Trigger a single flash effect
    /// </summary>
    public void TriggerFlash()
    {
        _isFlashing = true;
        _flashTimer = flashDuration;
        Debug.Log($"[StrobeController] Flash triggered, duration={flashDuration:F2}s");
        OnFlash?.Invoke();
    }
    
    /// <summary>
    /// Trigger a blackout
    /// </summary>
    public void TriggerBlackout(float duration = -1)
    {
        _isBlackout = true;
        _blackoutTimer = duration > 0 ? duration : blackoutDuration;
        OnBlackout?.Invoke();
    }
    
    /// <summary>
    /// Toggle strobe on/off
    /// </summary>
    public void ToggleStrobe()
    {
        enableStrobe = !enableStrobe;
        if (!enableStrobe)
        {
            RestoreColors();
        }
    }
    
    /// <summary>
    /// Set strobe BPM (beats per minute)
    /// </summary>
    public void SetBPM(float bpm)
    {
        strobeRate = bpm / 60f * beatMultiplier;
    }
}
