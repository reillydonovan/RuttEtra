using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Analog video effects: CRT simulation, VHS artifacts, chromatic aberration, etc.
/// Disabled by default - enable individual effects in Inspector or UI.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class AnalogEffects : MonoBehaviour
{
    [Header("Enable")]
    public bool enableEffects = true;
    
    [Header("CRT Simulation")]
    public bool enableCRT = false;
    [Range(0f, 1f)] public float scanlineIntensity = 0.3f;
    [Range(100f, 1000f)] public float scanlineCount = 300f;
    [Range(0f, 1f)] public float phosphorGlow = 0.2f;
    [Range(0f, 0.5f)] public float screenCurvature = 0.1f;
    [Range(0f, 0.1f)] public float vignette = 0.05f;
    
    [Header("VHS Artifacts")]
    public bool enableVHS = false;
    [Range(0f, 1f)] public float trackingNoise = 0.1f;
    [Range(0f, 1f)] public float colorBleed = 0.2f;
    [Range(0f, 1f)] public float tapeNoise = 0.1f;
    [Range(0f, 0.1f)] public float horizontalJitter = 0.02f;
    
    [Header("Chromatic Aberration")]
    public bool enableChromatic = false;
    [Range(0f, 0.05f)] public float chromaticAmount = 0.01f;
    
    [Header("Hold Drift")]
    public bool enableHoldDrift = false;
    [Range(0f, 1f)] public float horizontalHold = 0f;
    [Range(0f, 1f)] public float verticalHold = 0f;
    [Range(0f, 5f)] public float driftSpeed = 1f;
    
    [Header("Signal Noise")]
    public bool enableSignalNoise = false;
    [Range(0f, 1f)] public float staticNoise = 0.1f;
    [Range(0f, 1f)] public float snowAmount = 0f;
    
    [Header("Color Adjustment")]
    [Range(0f, 2f)] public float saturation = 1f;
    [Range(-1f, 1f)] public float hueShift = 0f;
    [Range(0.5f, 2f)] public float gamma = 1f;
    
    // Material and shader
    private Material _effectMaterial;
    private Camera _camera;
    
    private static readonly int ScanlineIntensity = Shader.PropertyToID("_ScanlineIntensity");
    private static readonly int ScanlineCount = Shader.PropertyToID("_ScanlineCount");
    private static readonly int PhosphorGlow = Shader.PropertyToID("_PhosphorGlow");
    private static readonly int ScreenCurvature = Shader.PropertyToID("_ScreenCurvature");
    private static readonly int Vignette = Shader.PropertyToID("_Vignette");
    private static readonly int TrackingNoise = Shader.PropertyToID("_TrackingNoise");
    private static readonly int ColorBleed = Shader.PropertyToID("_ColorBleed");
    private static readonly int TapeNoise = Shader.PropertyToID("_TapeNoise");
    private static readonly int HorizontalJitter = Shader.PropertyToID("_HorizontalJitter");
    private static readonly int ChromaticAmount = Shader.PropertyToID("_ChromaticAmount");
    private static readonly int HorizontalHold = Shader.PropertyToID("_HorizontalHold");
    private static readonly int VerticalHold = Shader.PropertyToID("_VerticalHold");
    private static readonly int DriftSpeed = Shader.PropertyToID("_DriftSpeed");
    private static readonly int StaticNoise = Shader.PropertyToID("_StaticNoise");
    private static readonly int SnowAmount = Shader.PropertyToID("_SnowAmount");
    private static readonly int Saturation = Shader.PropertyToID("_Saturation");
    private static readonly int HueShift = Shader.PropertyToID("_HueShift");
    private static readonly int Gamma = Shader.PropertyToID("_Gamma");
    private static readonly int EnableCRT = Shader.PropertyToID("_EnableCRT");
    private static readonly int EnableVHS = Shader.PropertyToID("_EnableVHS");
    private static readonly int EnableChromatic = Shader.PropertyToID("_EnableChromatic");
    private static readonly int EnableHoldDrift = Shader.PropertyToID("_EnableHoldDrift");
    private static readonly int EnableSignalNoise = Shader.PropertyToID("_EnableSignalNoise");
    private static readonly int TimeID = Shader.PropertyToID("_EffectTime");
    
    private void OnEnable()
    {
        _camera = GetComponent<Camera>();
        CreateMaterial();
    }
    
    private void CreateMaterial()
    {
        if (_effectMaterial != null) return;
        
        Shader shader = Shader.Find("RuttEtra/AnalogEffects");
        if (shader == null)
        {
            Debug.LogWarning("AnalogEffects: Shader 'RuttEtra/AnalogEffects' not found");
            return;
        }
        _effectMaterial = new Material(shader);
        _effectMaterial.hideFlags = HideFlags.HideAndDontSave;
    }
    
    // Note: Effects are applied via AnalogEffectsFeature (URP Renderer Feature)
    // This component just holds the settings
    
    public void UpdateMaterialProperties()
    {
        if (_effectMaterial == null) return;
        
        // Enable flags
        _effectMaterial.SetFloat(EnableCRT, enableCRT ? 1 : 0);
        _effectMaterial.SetFloat(EnableVHS, enableVHS ? 1 : 0);
        _effectMaterial.SetFloat(EnableChromatic, enableChromatic ? 1 : 0);
        _effectMaterial.SetFloat(EnableHoldDrift, enableHoldDrift ? 1 : 0);
        _effectMaterial.SetFloat(EnableSignalNoise, enableSignalNoise ? 1 : 0);
        
        // Time for animations
        _effectMaterial.SetFloat(TimeID, Time.time);
        
        // CRT
        _effectMaterial.SetFloat(ScanlineIntensity, scanlineIntensity);
        _effectMaterial.SetFloat(ScanlineCount, scanlineCount);
        _effectMaterial.SetFloat(PhosphorGlow, phosphorGlow);
        _effectMaterial.SetFloat(ScreenCurvature, screenCurvature);
        _effectMaterial.SetFloat(Vignette, vignette);
        
        // VHS
        _effectMaterial.SetFloat(TrackingNoise, trackingNoise);
        _effectMaterial.SetFloat(ColorBleed, colorBleed);
        _effectMaterial.SetFloat(TapeNoise, tapeNoise);
        _effectMaterial.SetFloat(HorizontalJitter, horizontalJitter);
        
        // Chromatic
        _effectMaterial.SetFloat(ChromaticAmount, chromaticAmount);
        
        // Hold drift
        _effectMaterial.SetFloat(HorizontalHold, horizontalHold);
        _effectMaterial.SetFloat(VerticalHold, verticalHold);
        _effectMaterial.SetFloat(DriftSpeed, driftSpeed);
        
        // Noise
        _effectMaterial.SetFloat(StaticNoise, staticNoise);
        _effectMaterial.SetFloat(SnowAmount, snowAmount);
        
        // Color
        _effectMaterial.SetFloat(Saturation, saturation);
        _effectMaterial.SetFloat(HueShift, hueShift);
        _effectMaterial.SetFloat(Gamma, gamma);
    }
    
    public Material GetMaterial()
    {
        if (_effectMaterial == null) CreateMaterial();
        return _effectMaterial;
    }
    
    private void OnDestroy()
    {
        if (_effectMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(_effectMaterial);
            else
                DestroyImmediate(_effectMaterial);
        }
    }
}
