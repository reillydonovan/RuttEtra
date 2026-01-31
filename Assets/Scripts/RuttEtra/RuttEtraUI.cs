using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class RuttEtraUI : MonoBehaviour
{
    [Header("References")]
    public RuttEtraController controller;
    public RuttEtraSettings settings;
    public WebcamCapture webcamCapture;
    public RuttEtraAnimator animator;
    
    [Header("UI Panels")]
    public GameObject controlPanel;
    public Key toggleUIKey = Key.H;
    
    [Header("Input Signal")]
    public Slider brightnessSlider;
    public Slider contrastSlider;
    public Slider thresholdSlider;
    public Slider gammaSlider;
    public Toggle edgeDetectToggle;
    public Slider posterizeSlider;
    
    [Header("Z-Axis")]
    public Slider displacementSlider;
    public Slider smoothingSlider;
    public Slider displacementOffsetSlider;
    public Toggle invertToggle;
    public Slider zModulationSlider;
    public Slider zModFreqSlider;
    
    [Header("Raster Position")]
    public Slider hPositionSlider;
    public Slider vPositionSlider;
    
    [Header("Raster Scale")]
    public Slider hScaleSlider;
    public Slider vScaleSlider;
    public Slider meshScaleSlider;
    
    [Header("Raster Rotation")]
    public Slider rotationXSlider;
    public Slider rotationYSlider;
    public Slider rotationZSlider;
    
    [Header("Distortion")]
    public Slider keystoneHSlider;
    public Slider keystoneVSlider;
    public Slider barrelSlider;
    
    [Header("Scan Lines")]
    public Slider scanLineSkipSlider;
    public Toggle horizontalLinesToggle;
    public Toggle verticalLinesToggle;
    public Toggle interlaceToggle;
    
    [Header("Deflection Wave")]
    public Slider horizontalWaveSlider;
    public Slider verticalWaveSlider;
    public Slider waveFrequencySlider;
    public Slider waveSpeedSlider;
    
    [Header("Line Style")]
    public Slider lineWidthSlider;
    public Slider lineTaperSlider;
    public Slider glowSlider;
    
    [Header("Colors")]
    public Slider primaryHueSlider;
    public Slider primarySatSlider;
    public Slider secondaryHueSlider;
    public Slider secondarySatSlider;
    public Slider colorBlendSlider;
    public Toggle sourceColorToggle;
    
    [Header("Resolution")]
    public Slider horizontalResSlider;
    public Slider verticalResSlider;
    public TMP_Text resolutionText;
    
    [Header("Feedback")]
    public Slider feedbackSlider;
    public Slider feedbackZoomSlider;
    public Slider feedbackRotSlider;
    
    [Header("Post Effects")]
    public Slider noiseSlider;
    public Slider persistenceSlider;
    public Slider flickerSlider;
    public Slider bloomSlider;
    
    [Header("Webcam")]
    public Toggle mirrorHorizontalToggle;
    public Toggle mirrorVerticalToggle;
    
    [Header("Buttons")]
    public Button resetCameraButton;
    public Button resetAllButton;
    
    [Header("Animation")]
    public Toggle animRotationToggle;
    public Slider animRotationSpeedSlider;
    public Toggle animHueToggle;
    public Slider animHueSpeedSlider;
    public Toggle animWaveToggle;
    public Slider animWaveSpeedSlider;
    public Toggle animZToggle;
    public Slider animZSpeedSlider;
    public Toggle animScaleToggle;
    public Slider animScaleSpeedSlider;
    public Toggle animDistortToggle;
    public Slider animDistortSpeedSlider;
    
    private bool _panelVisible = true;
    
    private void Start()
    {
        InitializeUI();
        InitializeAnimator();
        BindEvents();
    }
    
    private void InitializeAnimator()
    {
        // Auto-find animator if not assigned
        if (animator == null)
            animator = FindFirstObjectByType<RuttEtraAnimator>();
        
        if (animator == null) return;
        
        animator.CaptureBaseValues();
    }
    
    private void InitializeUI()
    {
        if (settings == null) return;
        
        // Input Signal
        Set(brightnessSlider, settings.brightness);
        Set(contrastSlider, settings.contrast);
        Set(thresholdSlider, settings.threshold);
        Set(gammaSlider, settings.gamma);
        Set(edgeDetectToggle, settings.edgeDetect);
        Set(posterizeSlider, settings.posterize);
        
        // Z-Axis
        Set(displacementSlider, settings.displacementStrength);
        Set(smoothingSlider, settings.displacementSmoothing);
        Set(displacementOffsetSlider, settings.displacementOffset);
        Set(invertToggle, settings.invertDisplacement);
        Set(zModulationSlider, settings.zModulation);
        Set(zModFreqSlider, settings.zModFrequency);
        
        // Raster Position
        Set(hPositionSlider, settings.horizontalPosition);
        Set(vPositionSlider, settings.verticalPosition);
        
        // Raster Scale
        Set(hScaleSlider, settings.horizontalScale);
        Set(vScaleSlider, settings.verticalScale);
        Set(meshScaleSlider, settings.meshScale);
        
        // Raster Rotation
        Set(rotationXSlider, settings.rotationX);
        Set(rotationYSlider, settings.rotationY);
        Set(rotationZSlider, settings.rotationZ);
        
        // Distortion
        Set(keystoneHSlider, settings.keystoneH);
        Set(keystoneVSlider, settings.keystoneV);
        Set(barrelSlider, settings.barrelDistortion);
        
        // Scan Lines
        Set(scanLineSkipSlider, settings.scanLineSkip);
        Set(horizontalLinesToggle, settings.showHorizontalLines);
        Set(verticalLinesToggle, settings.showVerticalLines);
        Set(interlaceToggle, settings.interlace);
        
        // Deflection Wave
        Set(horizontalWaveSlider, settings.horizontalWave);
        Set(verticalWaveSlider, settings.verticalWave);
        Set(waveFrequencySlider, settings.waveFrequency);
        Set(waveSpeedSlider, settings.waveSpeed);
        
        // Line Style
        Set(lineWidthSlider, settings.lineWidth);
        Set(lineTaperSlider, settings.lineTaper);
        Set(glowSlider, settings.glowIntensity);
        
        // Colors
        Color.RGBToHSV(settings.primaryColor, out float pH, out float pS, out _);
        Color.RGBToHSV(settings.secondaryColor, out float sH, out float sS, out _);
        Set(primaryHueSlider, pH);
        Set(primarySatSlider, pS);
        Set(secondaryHueSlider, sH);
        Set(secondarySatSlider, sS);
        Set(colorBlendSlider, settings.colorBlend);
        Set(sourceColorToggle, settings.useSourceColor);
        
        // Resolution
        Set(horizontalResSlider, settings.horizontalResolution);
        Set(verticalResSlider, settings.verticalResolution);
        UpdateResolutionText();
        
        // Feedback
        Set(feedbackSlider, settings.feedback);
        Set(feedbackZoomSlider, settings.feedbackZoom);
        Set(feedbackRotSlider, settings.feedbackRotation);
        
        // Post Effects
        Set(noiseSlider, settings.noiseAmount);
        Set(persistenceSlider, settings.persistence);
        Set(flickerSlider, settings.scanlineFlicker);
        Set(bloomSlider, settings.bloom);
        
        // Webcam
        if (webcamCapture)
        {
            Set(mirrorHorizontalToggle, webcamCapture.mirrorHorizontal);
            Set(mirrorVerticalToggle, webcamCapture.mirrorVertical);
        }
    }
    
    void Set(Slider s, float v) { if (s) s.value = v; }
    void Set(Toggle t, bool v) { if (t) t.isOn = v; }
    
    private void BindEvents()
    {
        // Input Signal
        Bind(brightnessSlider, v => settings.brightness = v);
        Bind(contrastSlider, v => settings.contrast = v);
        Bind(thresholdSlider, v => settings.threshold = v);
        Bind(gammaSlider, v => settings.gamma = v);
        Bind(edgeDetectToggle, v => settings.edgeDetect = v);
        Bind(posterizeSlider, v => settings.posterize = Mathf.RoundToInt(v));
        
        // Z-Axis
        Bind(displacementSlider, v => settings.displacementStrength = v);
        Bind(smoothingSlider, v => settings.displacementSmoothing = v);
        Bind(displacementOffsetSlider, v => settings.displacementOffset = v);
        Bind(invertToggle, v => settings.invertDisplacement = v);
        Bind(zModulationSlider, v => settings.zModulation = v);
        Bind(zModFreqSlider, v => settings.zModFrequency = v);
        
        // Raster Position
        Bind(hPositionSlider, v => settings.horizontalPosition = v);
        Bind(vPositionSlider, v => settings.verticalPosition = v);
        
        // Raster Scale
        Bind(hScaleSlider, v => settings.horizontalScale = v);
        Bind(vScaleSlider, v => settings.verticalScale = v);
        Bind(meshScaleSlider, v => settings.meshScale = v);
        
        // Raster Rotation
        Bind(rotationXSlider, v => settings.rotationX = v);
        Bind(rotationYSlider, v => settings.rotationY = v);
        Bind(rotationZSlider, v => settings.rotationZ = v);
        
        // Distortion
        Bind(keystoneHSlider, v => settings.keystoneH = v);
        Bind(keystoneVSlider, v => settings.keystoneV = v);
        Bind(barrelSlider, v => settings.barrelDistortion = v);
        
        // Scan Lines
        Bind(scanLineSkipSlider, v => { settings.scanLineSkip = Mathf.RoundToInt(v); Refresh(); });
        Bind(horizontalLinesToggle, v => { settings.showHorizontalLines = v; Refresh(); });
        Bind(verticalLinesToggle, v => { settings.showVerticalLines = v; Refresh(); });
        Bind(interlaceToggle, v => settings.interlace = v);
        
        // Deflection Wave
        Bind(horizontalWaveSlider, v => settings.horizontalWave = v);
        Bind(verticalWaveSlider, v => settings.verticalWave = v);
        Bind(waveFrequencySlider, v => settings.waveFrequency = v);
        Bind(waveSpeedSlider, v => settings.waveSpeed = v);
        
        // Line Style
        Bind(lineWidthSlider, v => settings.lineWidth = v);
        Bind(lineTaperSlider, v => settings.lineTaper = v);
        Bind(glowSlider, v => settings.glowIntensity = v);
        
        // Colors
        Bind(primaryHueSlider, v => UpdatePrimaryColor());
        Bind(primarySatSlider, v => UpdatePrimaryColor());
        Bind(secondaryHueSlider, v => UpdateSecondaryColor());
        Bind(secondarySatSlider, v => UpdateSecondaryColor());
        Bind(colorBlendSlider, v => settings.colorBlend = v);
        Bind(sourceColorToggle, v => settings.useSourceColor = v);
        
        // Resolution
        Bind(horizontalResSlider, v => { settings.horizontalResolution = Mathf.RoundToInt(v); UpdateResolutionText(); Refresh(); });
        Bind(verticalResSlider, v => { settings.verticalResolution = Mathf.RoundToInt(v); UpdateResolutionText(); Refresh(); });
        
        // Feedback
        Bind(feedbackSlider, v => settings.feedback = v);
        Bind(feedbackZoomSlider, v => settings.feedbackZoom = v);
        Bind(feedbackRotSlider, v => settings.feedbackRotation = v);
        
        // Post Effects
        Bind(noiseSlider, v => settings.noiseAmount = v);
        Bind(persistenceSlider, v => settings.persistence = v);
        Bind(flickerSlider, v => settings.scanlineFlicker = v);
        Bind(bloomSlider, v => settings.bloom = v);
        
        // Webcam
        Bind(mirrorHorizontalToggle, v => { if (webcamCapture) webcamCapture.mirrorHorizontal = v; });
        Bind(mirrorVerticalToggle, v => { if (webcamCapture) webcamCapture.mirrorVertical = v; });
        
        // Buttons
        resetCameraButton?.onClick.AddListener(ResetCamera);
        resetAllButton?.onClick.AddListener(ResetAll);
        
        // Animation - use simplified API
        Bind(animRotationToggle, v => { if (animator) animator.SetAutoRotate(v); });
        Bind(animRotationSpeedSlider, v => { if (animator) animator.SetRotateSpeed(v); });
        Bind(animHueToggle, v => { if (animator) animator.SetHueCycle(v); });
        Bind(animHueSpeedSlider, v => { if (animator) animator.SetHueSpeed(v); });
        Bind(animWaveToggle, v => { if (animator) animator.SetWaveAnimate(v); });
        Bind(animWaveSpeedSlider, v => { if (animator) animator.SetWaveAnimSpeed(v); });
        Bind(animZToggle, v => { if (animator) animator.SetZPulse(v); });
        Bind(animZSpeedSlider, v => { if (animator) animator.SetZPulseSpeed(v); });
        Bind(animScaleToggle, v => { if (animator) animator.SetBreathe(v); });
        Bind(animScaleSpeedSlider, v => { if (animator) animator.SetBreatheSpeed(v); });
        Bind(animDistortToggle, v => { if (animator) animator.SetDistortAnimate(v); });
        Bind(animDistortSpeedSlider, v => { if (animator) animator.SetDistortSpeed(v); });
    }
    
    void Bind(Slider s, System.Action<float> a) { s?.onValueChanged.AddListener(v => a(v)); }
    void Bind(Toggle t, System.Action<bool> a) { t?.onValueChanged.AddListener(v => a(v)); }
    void Refresh() { controller?.meshGenerator?.RefreshMesh(); }
    
    void UpdatePrimaryColor()
    {
        float h = primaryHueSlider ? primaryHueSlider.value : 0;
        float s = primarySatSlider ? primarySatSlider.value : 1;
        settings.primaryColor = Color.HSVToRGB(h, s, 1f);
    }
    
    void UpdateSecondaryColor()
    {
        float h = secondaryHueSlider ? secondaryHueSlider.value : 0;
        float s = secondarySatSlider ? secondarySatSlider.value : 1;
        settings.secondaryColor = Color.HSVToRGB(h, s, 1f);
    }
    
    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb[toggleUIKey].wasPressedThisFrame)
        {
            _panelVisible = !_panelVisible;
            controlPanel?.SetActive(_panelVisible);
        }
    }
    
    void UpdateResolutionText()
    {
        if (resolutionText && settings)
            resolutionText.text = $"{settings.horizontalResolution} x {settings.verticalResolution}";
    }
    
    public void ResetCamera()
    {
        Camera.main?.GetComponent<OrbitCamera>()?.ResetView();
        controller?.ResetCamera();
    }
    
    public void ResetAll()
    {
        if (!settings) return;
        
        settings.brightness = 0; settings.contrast = 1; settings.threshold = 0; settings.gamma = 1;
        settings.edgeDetect = false; settings.posterize = 1;
        settings.displacementStrength = 1; settings.displacementSmoothing = 0.5f;
        settings.displacementOffset = 0; settings.invertDisplacement = false;
        settings.zModulation = 0; settings.zModFrequency = 1;
        settings.horizontalPosition = 0; settings.verticalPosition = 0;
        settings.horizontalScale = 1; settings.verticalScale = 1; settings.meshScale = 1;
        settings.rotationX = 0; settings.rotationY = 0; settings.rotationZ = 0;
        settings.keystoneH = 0; settings.keystoneV = 0; settings.barrelDistortion = 0;
        settings.scanLineSkip = 1; settings.showHorizontalLines = true;
        settings.showVerticalLines = false; settings.interlace = false;
        settings.horizontalWave = 0; settings.verticalWave = 0;
        settings.waveFrequency = 2; settings.waveSpeed = 1;
        settings.lineWidth = 0.01f; settings.lineTaper = 0; settings.glowIntensity = 0.5f;
        settings.primaryColor = Color.green; settings.secondaryColor = Color.cyan;
        settings.colorBlend = 0.5f; settings.useSourceColor = false;
        settings.horizontalResolution = 128; settings.verticalResolution = 64;
        settings.feedback = 0; settings.feedbackZoom = 0; settings.feedbackRotation = 0;
        settings.noiseAmount = 0; settings.persistence = 0; settings.scanlineFlicker = 0; settings.bloom = 0;
        
        if (webcamCapture) { webcamCapture.mirrorHorizontal = true; webcamCapture.mirrorVertical = false; }
        
        InitializeUI();
        Refresh();
        ResetCamera();
    }
}
