using UnityEngine;
using System;

[Serializable]
public class LFO
{
    public bool enabled = false;
    [Range(0.01f, 5f)] public float speed = 1f;
    [Range(0f, 2f)] public float amplitude = 1f;
    public float offset = 0f;
    public LFOWaveform waveform = LFOWaveform.Sine;
    
    public float Evaluate(float time)
    {
        if (!enabled) return 0f;
        
        float phase = time * speed;
        float value = waveform switch
        {
            LFOWaveform.Sine => Mathf.Sin(phase * Mathf.PI * 2f),
            LFOWaveform.Triangle => Mathf.PingPong(phase * 2f, 1f) * 2f - 1f,
            LFOWaveform.Square => Mathf.Sin(phase * Mathf.PI * 2f) > 0 ? 1f : -1f,
            LFOWaveform.Sawtooth => ((phase % 1f) * 2f) - 1f,
            LFOWaveform.Random => Mathf.PerlinNoise(phase, 0f) * 2f - 1f,
            _ => 0f
        };
        
        return value * amplitude + offset;
    }
}

public enum LFOWaveform { Sine, Triangle, Square, Sawtooth, Random }

public class RuttEtraAnimator : MonoBehaviour
{
    [Header("Settings (auto-finds if null)")]
    public RuttEtraSettings settings;
    
    [Header("=== QUICK ANIMATIONS ===")]
    [Tooltip("Enable auto-rotation around Y axis")]
    public bool autoRotate = false;
    [Range(0.1f, 3f)] public float rotateSpeed = 0.5f;
    [Range(5f, 180f)] public float rotateAmount = 30f;
    
    [Tooltip("Enable color hue cycling")]
    public bool hueCycle = false;
    [Range(0.1f, 2f)] public float hueSpeed = 0.3f;
    
    [Tooltip("Enable wave animation")]
    public bool waveAnimate = false;
    [Range(0.1f, 3f)] public float waveAnimSpeed = 0.5f;
    [Range(0.1f, 1f)] public float waveAnimAmount = 0.5f;
    
    [Tooltip("Enable Z-axis pulsing")]
    public bool zPulse = false;
    [Range(0.1f, 3f)] public float zPulseSpeed = 0.5f;
    [Range(0.1f, 1f)] public float zPulseAmount = 0.3f;
    
    [Tooltip("Enable breathing/scale animation")]
    public bool breathe = false;
    [Range(0.1f, 2f)] public float breatheSpeed = 0.3f;
    [Range(0.1f, 0.5f)] public float breatheAmount = 0.2f;
    
    [Tooltip("Enable distortion animation")]
    public bool distortAnimate = false;
    [Range(0.1f, 2f)] public float distortSpeed = 0.4f;
    [Range(0.1f, 0.5f)] public float distortAmount = 0.3f;
    
    [Header("=== ADVANCED LFOs ===")]
    public LFO brightnessLFO = new LFO();
    public LFO contrastLFO = new LFO();
    public LFO displacementLFO = new LFO();
    public LFO zOffsetLFO = new LFO();
    public LFO hPositionLFO = new LFO();
    public LFO vPositionLFO = new LFO();
    public LFO hScaleLFO = new LFO();
    public LFO vScaleLFO = new LFO();
    public LFO scaleLFO = new LFO();
    public LFO rotationXLFO = new LFO();
    public LFO rotationYLFO = new LFO();
    public LFO rotationZLFO = new LFO();
    public LFO keystoneHLFO = new LFO();
    public LFO keystoneVLFO = new LFO();
    public LFO barrelLFO = new LFO();
    public LFO hWaveLFO = new LFO();
    public LFO vWaveLFO = new LFO();
    public LFO lineWidthLFO = new LFO();
    public LFO glowLFO = new LFO();
    public LFO primaryHueLFO = new LFO();
    public LFO secondaryHueLFO = new LFO();
    
    // Base values
    private float _baseBrightness, _baseContrast;
    private float _baseDisplacement, _baseZOffset;
    private float _baseHPos, _baseVPos;
    private float _baseHScale, _baseVScale, _baseScale;
    private float _baseRotX, _baseRotY, _baseRotZ;
    private float _baseKeystoneH, _baseKeystoneV, _baseBarrel;
    private float _baseHWave, _baseVWave;
    private float _baseLineWidth, _baseGlow;
    private float _basePrimaryHue, _baseSecondaryhue;
    
    private float _time;
    private bool _initialized;
    
    private void Start()
    {
        // Auto-find settings if not assigned
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller != null)
                settings = controller.settings;
        }
        
        CaptureBaseValues();
    }
    
    public void CaptureBaseValues()
    {
        if (settings == null) return;
        
        _baseBrightness = settings.brightness;
        _baseContrast = settings.contrast;
        _baseDisplacement = settings.displacementStrength;
        _baseZOffset = settings.displacementOffset;
        _baseHPos = settings.horizontalPosition;
        _baseVPos = settings.verticalPosition;
        _baseHScale = settings.horizontalScale;
        _baseVScale = settings.verticalScale;
        _baseScale = settings.meshScale;
        _baseRotX = settings.rotationX;
        _baseRotY = settings.rotationY;
        _baseRotZ = settings.rotationZ;
        _baseKeystoneH = settings.keystoneH;
        _baseKeystoneV = settings.keystoneV;
        _baseBarrel = settings.barrelDistortion;
        _baseHWave = settings.horizontalWave;
        _baseVWave = settings.verticalWave;
        _baseLineWidth = settings.lineWidth;
        _baseGlow = settings.glowIntensity;
        
        Color.RGBToHSV(settings.primaryColor, out _basePrimaryHue, out _, out _);
        Color.RGBToHSV(settings.secondaryColor, out _baseSecondaryhue, out _, out _);
        
        _initialized = true;
    }
    
    private void Update()
    {
        if (settings == null) return;
        if (!_initialized) CaptureBaseValues();
        
        _time += Time.deltaTime;
        
        // === QUICK ANIMATIONS ===
        
        // Auto rotation
        if (autoRotate)
        {
            float rot = Mathf.Sin(_time * rotateSpeed * Mathf.PI * 2f) * rotateAmount;
            settings.rotationY = _baseRotY + rot;
        }
        
        // Hue cycling
        if (hueCycle)
        {
            float hueOffset = (_time * hueSpeed) % 1f;
            float h1 = (_basePrimaryHue + hueOffset) % 1f;
            float h2 = (_baseSecondaryhue + hueOffset) % 1f;
            Color.RGBToHSV(settings.primaryColor, out _, out float s1, out float v1);
            Color.RGBToHSV(settings.secondaryColor, out _, out float s2, out float v2);
            settings.primaryColor = Color.HSVToRGB(h1, s1, v1);
            settings.secondaryColor = Color.HSVToRGB(h2, s2, v2);
        }
        
        // Wave animation
        if (waveAnimate)
        {
            float wave = Mathf.Sin(_time * waveAnimSpeed * Mathf.PI * 2f) * waveAnimAmount;
            settings.horizontalWave = Mathf.Max(0, _baseHWave + wave);
            settings.verticalWave = Mathf.Max(0, _baseVWave + wave * 0.7f);
        }
        
        // Z pulse
        if (zPulse)
        {
            float pulse = Mathf.Sin(_time * zPulseSpeed * Mathf.PI * 2f) * zPulseAmount;
            settings.displacementStrength = Mathf.Max(0, _baseDisplacement + pulse);
            settings.displacementOffset = _baseZOffset + pulse * 0.5f;
        }
        
        // Breathe/scale
        if (breathe)
        {
            float breath = Mathf.Sin(_time * breatheSpeed * Mathf.PI * 2f) * breatheAmount;
            settings.meshScale = Mathf.Max(0.1f, _baseScale + breath);
        }
        
        // Distortion animation
        if (distortAnimate)
        {
            float t = _time * distortSpeed;
            float kh = Mathf.Sin(t * Mathf.PI * 2f) * distortAmount;
            float kv = Mathf.Sin(t * Mathf.PI * 2f * 1.3f) * distortAmount;
            float barrel = Mathf.Sin(t * Mathf.PI * 2f * 0.7f) * distortAmount * 0.5f;
            settings.keystoneH = Mathf.Clamp(_baseKeystoneH + kh, -1f, 1f);
            settings.keystoneV = Mathf.Clamp(_baseKeystoneV + kv, -1f, 1f);
            settings.barrelDistortion = Mathf.Clamp(_baseBarrel + barrel, -0.5f, 0.5f);
        }
        
        // === ADVANCED LFOs ===
        
        if (brightnessLFO.enabled)
            settings.brightness = Mathf.Clamp(_baseBrightness + brightnessLFO.Evaluate(_time), -1f, 1f);
        if (contrastLFO.enabled)
            settings.contrast = Mathf.Clamp(_baseContrast + contrastLFO.Evaluate(_time), 0.1f, 3f);
        if (displacementLFO.enabled)
            settings.displacementStrength = Mathf.Max(0, _baseDisplacement + displacementLFO.Evaluate(_time));
        if (zOffsetLFO.enabled)
            settings.displacementOffset = Mathf.Clamp(_baseZOffset + zOffsetLFO.Evaluate(_time), -1f, 1f);
        if (hPositionLFO.enabled)
            settings.horizontalPosition = _baseHPos + hPositionLFO.Evaluate(_time);
        if (vPositionLFO.enabled)
            settings.verticalPosition = _baseVPos + vPositionLFO.Evaluate(_time);
        if (hScaleLFO.enabled)
            settings.horizontalScale = Mathf.Max(0.1f, _baseHScale + hScaleLFO.Evaluate(_time));
        if (vScaleLFO.enabled)
            settings.verticalScale = Mathf.Max(0.1f, _baseVScale + vScaleLFO.Evaluate(_time));
        if (scaleLFO.enabled)
            settings.meshScale = Mathf.Max(0.1f, _baseScale + scaleLFO.Evaluate(_time));
        if (rotationXLFO.enabled)
            settings.rotationX = _baseRotX + rotationXLFO.Evaluate(_time);
        if (rotationYLFO.enabled)
            settings.rotationY = _baseRotY + rotationYLFO.Evaluate(_time);
        if (rotationZLFO.enabled)
            settings.rotationZ = _baseRotZ + rotationZLFO.Evaluate(_time);
        if (keystoneHLFO.enabled)
            settings.keystoneH = Mathf.Clamp(_baseKeystoneH + keystoneHLFO.Evaluate(_time), -1f, 1f);
        if (keystoneVLFO.enabled)
            settings.keystoneV = Mathf.Clamp(_baseKeystoneV + keystoneVLFO.Evaluate(_time), -1f, 1f);
        if (barrelLFO.enabled)
            settings.barrelDistortion = Mathf.Clamp(_baseBarrel + barrelLFO.Evaluate(_time), -0.5f, 0.5f);
        if (hWaveLFO.enabled)
            settings.horizontalWave = Mathf.Max(0, _baseHWave + hWaveLFO.Evaluate(_time));
        if (vWaveLFO.enabled)
            settings.verticalWave = Mathf.Max(0, _baseVWave + vWaveLFO.Evaluate(_time));
        if (lineWidthLFO.enabled)
            settings.lineWidth = Mathf.Clamp(_baseLineWidth + lineWidthLFO.Evaluate(_time), 0.001f, 0.05f);
        if (glowLFO.enabled)
            settings.glowIntensity = Mathf.Max(0, _baseGlow + glowLFO.Evaluate(_time));
        if (primaryHueLFO.enabled)
        {
            float h = (_basePrimaryHue + primaryHueLFO.Evaluate(_time)) % 1f;
            if (h < 0) h += 1f;
            Color.RGBToHSV(settings.primaryColor, out _, out float s, out float v);
            settings.primaryColor = Color.HSVToRGB(h, s, v);
        }
        if (secondaryHueLFO.enabled)
        {
            float h = (_baseSecondaryhue + secondaryHueLFO.Evaluate(_time)) % 1f;
            if (h < 0) h += 1f;
            Color.RGBToHSV(settings.secondaryColor, out _, out float s, out float v);
            settings.secondaryColor = Color.HSVToRGB(h, s, v);
        }
    }
    
    public void ResetAnimation()
    {
        _time = 0;
        CaptureBaseValues();
    }
    
    // Called by UI
    public void SetAutoRotate(bool enabled) { autoRotate = enabled; if (enabled) CaptureBaseValues(); }
    public void SetRotateSpeed(float speed) { rotateSpeed = speed; }
    public void SetHueCycle(bool enabled) { hueCycle = enabled; if (enabled) CaptureBaseValues(); }
    public void SetHueSpeed(float speed) { hueSpeed = speed; }
    public void SetWaveAnimate(bool enabled) { waveAnimate = enabled; if (enabled) CaptureBaseValues(); }
    public void SetWaveAnimSpeed(float speed) { waveAnimSpeed = speed; }
    public void SetZPulse(bool enabled) { zPulse = enabled; if (enabled) CaptureBaseValues(); }
    public void SetZPulseSpeed(float speed) { zPulseSpeed = speed; }
    public void SetBreathe(bool enabled) { breathe = enabled; if (enabled) CaptureBaseValues(); }
    public void SetBreatheSpeed(float speed) { breatheSpeed = speed; }
    public void SetDistortAnimate(bool enabled) { distortAnimate = enabled; if (enabled) CaptureBaseValues(); }
    public void SetDistortSpeed(float speed) { distortSpeed = speed; }
}
