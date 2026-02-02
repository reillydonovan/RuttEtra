using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class RuttEtraUI : MonoBehaviour
{
    [Header("References")]
    public RuttEtraController controller;
    public RuttEtraSettings settings;
    public WebcamCapture webcamCapture;
    public RuttEtraAnimator animator;
    
    [Header("New Feature References")]
    public AudioReactive audioReactive;
    public AnalogEffects analogEffects;
    public FeedbackEffect feedbackEffect;
    public PresetManager presetManager;
    public VideoRecorder videoRecorder;
    public OSCReceiver oscReceiver;
    public MIDIInput midiInput;
    
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
    
    [Header("Feedback (Settings)")]
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
    
    // ====== NEW FEATURE UI ELEMENTS ======
    
    [Header("Audio Reactive")]
    public Toggle audioEnableToggle;
    public TMP_Dropdown audioDeviceDropdown;
    public Slider audioGainSlider;
    public Slider audioSmoothingSlider;
    public Toggle audioDisplacementToggle;
    public Slider audioDisplacementAmountSlider;
    public Toggle audioWaveToggle;
    public Slider audioWaveAmountSlider;
    public Toggle audioHueToggle;
    public Slider audioHueAmountSlider;
    public Toggle audioScaleToggle;
    public Slider audioScaleAmountSlider;
    public Toggle audioRotationToggle;
    public Slider audioRotationAmountSlider;
    public Toggle audioGlowToggle;
    public Slider audioGlowAmountSlider;
    public Toggle audioBeatFlashToggle;
    public Toggle audioBeatPulseToggle;
    public Slider audioBeatIntensitySlider;
    public TMP_Text audioLevelText;
    
    [Header("Analog Effects - CRT")]
    public Toggle crtEnableToggle;
    public Slider crtScanlineSlider;
    public Slider crtPhosphorSlider;
    public Slider crtCurvatureSlider;
    public Slider crtVignetteSlider;
    
    [Header("Analog Effects - VHS")]
    public Toggle vhsEnableToggle;
    public Slider vhsTrackingSlider;
    public Slider vhsColorBleedSlider;
    public Slider vhsTapeNoiseSlider;
    public Slider vhsJitterSlider;
    
    [Header("Analog Effects - Other")]
    public Toggle chromaticEnableToggle;
    public Slider chromaticAmountSlider;
    public Toggle holdDriftEnableToggle;
    public Slider holdHorizontalSlider;
    public Slider holdVerticalSlider;
    public Toggle signalNoiseEnableToggle;
    public Slider staticNoiseSlider;
    public Slider snowAmountSlider;
    public Slider analogSaturationSlider;
    public Slider analogHueShiftSlider;
    public Slider analogGammaSlider;
    
    [Header("Visual Feedback Effect")]
    public Toggle feedbackEffectEnableToggle;
    public Slider feedbackEffectAmountSlider;
    public Slider feedbackEffectZoomSlider;
    public Slider feedbackEffectRotationSlider;
    public Slider feedbackEffectOffsetXSlider;
    public Slider feedbackEffectOffsetYSlider;
    public Slider feedbackHueShiftSlider;
    public Slider feedbackSaturationSlider;
    public Slider feedbackBrightnessSlider;
    public Button feedbackClearButton;
    
    [Header("Presets")]
    public TMP_Dropdown presetDropdown;
    public Button presetSaveButton;
    public Button presetNextButton;
    public Button presetPrevButton;
    public Button presetRandomButton;
    public Toggle presetMorphToggle;
    public Slider presetMorphDurationSlider;
    public TMP_Text presetNameText;
    
    [Header("Recording")]
    public Toggle recordingToggle;
    public Button screenshotButton;
    public TMP_Text recordingStatusText;
    public Slider recordingFPSSlider;
    
    [Header("OSC")]
    public Toggle oscEnableToggle;
    public TMP_Text oscStatusText;
    public Toggle oscLogToggle;
    
    [Header("MIDI")]
    public Toggle midiEnableToggle;
    public Toggle midiLearnToggle;
    public TMP_Text midiStatusText;
    
    // ====== NEW EXPERIMENTAL FEATURES ======
    
    [Header("New Feature References")]
    public GlitchEffects glitchEffects;
    public ColorPaletteSystem colorPalette;
    public MirrorKaleidoscope mirrorKaleidoscope;
    public AutoRandomizer autoRandomizer;
    public SynthwaveGrid synthwaveGrid;
    public MotionTrails motionTrails;
    public DepthColorizer depthColorizer;
    public StrobeController strobeController;
    public ScreenShake screenShake;
    
    [Header("Glitch Effects")]
    public Toggle glitchEnableToggle;
    public Slider glitchIntensitySlider;
    public Toggle glitchAutoTriggerToggle;
    public Slider glitchIntervalSlider;
    public Toggle glitchOnBeatToggle;
    public Button glitchTriggerButton;
    
    [Header("Color Palette")]
    public Toggle paletteEnableToggle;
    public TMP_Dropdown paletteDropdown;
    public Button paletteNextButton;
    public Button palettePrevButton;
    public Toggle paletteAutoToggle;
    public Slider paletteTransitionSlider;
    
    [Header("Mirror Kaleidoscope")]
    public Toggle mirrorEnableToggle;
    public TMP_Dropdown mirrorModeDropdown;
    public Toggle kaleidoscopeEnableToggle;
    public Slider kaleidoscopeSegmentsSlider;
    public Toggle kaleidoscopeAnimateToggle;
    public Slider kaleidoscopeSpeedSlider;
    
    [Header("Auto Randomizer")]
    public Toggle randomizerEnableToggle;
    public Slider randomizerSpeedSlider;
    public Slider randomizerIntensitySlider;
    public Toggle randomizerDisplacementToggle;
    public Toggle randomizerWaveToggle;
    public Toggle randomizerRotationToggle;
    public Toggle randomizerHueToggle;
    public Button randomizerResetButton;
    
    [Header("Synthwave Grid")]
    public Toggle gridEnableToggle;
    public Slider gridScrollSpeedSlider;
    public Toggle gridAudioReactiveToggle;
    public Toggle gridShowSunToggle;
    public Toggle gridAnimateColorsToggle;
    
    [Header("Motion Trails")]
    public Toggle trailsEnableToggle;
    public Slider trailsCountSlider;
    public Slider trailsOpacitySlider;
    public Toggle trailsColorShiftToggle;
    public Button trailsClearButton;
    
    [Header("Depth Colorizer")]
    public Toggle depthColorEnableToggle;
    public TMP_Dropdown depthColorModeDropdown;
    public Toggle depthRainbowAnimateToggle;
    
    [Header("Strobe Controller")]
    public Toggle strobeEnableToggle;
    public Slider strobeRateSlider;
    public Toggle strobeBeatSyncToggle;
    public Toggle flashOnBeatToggle;
    public Button flashTriggerButton;
    public Button blackoutButton;
    
    [Header("Screen Shake")]
    public Toggle shakeEnableToggle;
    public Slider shakeIntensitySlider;
    public Toggle shakeOnBeatToggle;
    public Toggle shakeContinuousToggle;
    public Button shakeTriggerButton;
    
    private bool _panelVisible = true;
    
    private void Start()
    {
        FindReferences();
        InitializeUI();
        InitializeAnimator();
        InitializeNewFeatures();
        BindEvents();
        BindNewFeatureEvents();
        
        Debug.Log("RuttEtraUI initialized. Settings: " + (settings != null ? "OK" : "NULL") + 
                  ", Controller: " + (controller != null ? "OK" : "NULL") +
                  ", AudioReactive: " + (audioReactive != null ? "OK" : "NULL") +
                  ", GlitchEffects: " + (glitchEffects != null ? "OK" : "NULL") +
                  ", ColorPalette: " + (colorPalette != null ? "OK" : "NULL") +
                  ", AutoRandomizer: " + (autoRandomizer != null ? "OK" : "NULL"));
    }
    
    private void FindReferences()
    {
        // Find core references
        if (controller == null) controller = FindFirstObjectByType<RuttEtraController>();
        if (controller != null && settings == null) settings = controller.settings;
        if (webcamCapture == null) webcamCapture = FindFirstObjectByType<WebcamCapture>();
        if (animator == null) animator = FindFirstObjectByType<RuttEtraAnimator>();
        
        // Auto-find feature components
        if (audioReactive == null) audioReactive = FindFirstObjectByType<AudioReactive>();
        if (analogEffects == null) analogEffects = FindFirstObjectByType<AnalogEffects>();
        if (feedbackEffect == null) feedbackEffect = FindFirstObjectByType<FeedbackEffect>();
        if (presetManager == null) presetManager = FindFirstObjectByType<PresetManager>();
        if (videoRecorder == null) videoRecorder = FindFirstObjectByType<VideoRecorder>();
        if (oscReceiver == null) oscReceiver = FindFirstObjectByType<OSCReceiver>();
        if (midiInput == null) midiInput = FindFirstObjectByType<MIDIInput>();
        
        // Auto-find new experimental features
        if (glitchEffects == null) glitchEffects = FindFirstObjectByType<GlitchEffects>();
        if (colorPalette == null) colorPalette = FindFirstObjectByType<ColorPaletteSystem>();
        if (mirrorKaleidoscope == null) mirrorKaleidoscope = FindFirstObjectByType<MirrorKaleidoscope>();
        if (autoRandomizer == null) autoRandomizer = FindFirstObjectByType<AutoRandomizer>();
        if (synthwaveGrid == null) synthwaveGrid = FindFirstObjectByType<SynthwaveGrid>();
        if (motionTrails == null) motionTrails = FindFirstObjectByType<MotionTrails>();
        if (depthColorizer == null) depthColorizer = FindFirstObjectByType<DepthColorizer>();
        if (strobeController == null) strobeController = FindFirstObjectByType<StrobeController>();
        if (screenShake == null) screenShake = FindFirstObjectByType<ScreenShake>();
    }
    
    private void InitializeAnimator()
    {
        if (animator == null) return;
        animator.CaptureBaseValues();
    }
    
    private void InitializeUI()
    {
        if (settings == null) 
        {
            Debug.LogWarning("RuttEtraUI: Settings is null, cannot initialize UI");
            return;
        }
        
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
    
    private void InitializeNewFeatures()
    {
        // Audio Reactive
        if (audioReactive != null)
        {
            Set(audioEnableToggle, audioReactive.enableAudio);
            Set(audioGainSlider, audioReactive.inputGain);
            Set(audioSmoothingSlider, audioReactive.smoothing);
            Set(audioDisplacementToggle, audioReactive.modulateDisplacement);
            Set(audioDisplacementAmountSlider, audioReactive.displacementAmount);
            Set(audioWaveToggle, audioReactive.modulateWave);
            Set(audioWaveAmountSlider, audioReactive.waveAmount);
            Set(audioHueToggle, audioReactive.modulateHue);
            Set(audioHueAmountSlider, audioReactive.hueAmount);
            Set(audioScaleToggle, audioReactive.modulateScale);
            Set(audioScaleAmountSlider, audioReactive.scaleAmount);
            Set(audioRotationToggle, audioReactive.modulateRotation);
            Set(audioRotationAmountSlider, audioReactive.rotationAmount);
            Set(audioGlowToggle, audioReactive.modulateGlow);
            Set(audioGlowAmountSlider, audioReactive.glowAmount);
            Set(audioBeatFlashToggle, audioReactive.flashOnBeat);
            Set(audioBeatPulseToggle, audioReactive.pulseOnBeat);
            Set(audioBeatIntensitySlider, audioReactive.beatIntensity);
            
            // Audio device dropdown
            UpdateAudioDeviceDropdown();
            audioReactive.OnDevicesChanged += _ => UpdateAudioDeviceDropdown();
        }
        
        // Analog Effects
        if (analogEffects != null)
        {
            // CRT
            Set(crtEnableToggle, analogEffects.enableCRT);
            Set(crtScanlineSlider, analogEffects.scanlineIntensity);
            Set(crtPhosphorSlider, analogEffects.phosphorGlow);
            Set(crtCurvatureSlider, analogEffects.screenCurvature);
            Set(crtVignetteSlider, analogEffects.vignette);
            
            // VHS
            Set(vhsEnableToggle, analogEffects.enableVHS);
            Set(vhsTrackingSlider, analogEffects.trackingNoise);
            Set(vhsColorBleedSlider, analogEffects.colorBleed);
            Set(vhsTapeNoiseSlider, analogEffects.tapeNoise);
            Set(vhsJitterSlider, analogEffects.horizontalJitter);
            
            // Other
            Set(chromaticEnableToggle, analogEffects.enableChromatic);
            Set(chromaticAmountSlider, analogEffects.chromaticAmount);
            Set(holdDriftEnableToggle, analogEffects.enableHoldDrift);
            Set(holdHorizontalSlider, analogEffects.horizontalHold);
            Set(holdVerticalSlider, analogEffects.verticalHold);
            Set(signalNoiseEnableToggle, analogEffects.enableSignalNoise);
            Set(staticNoiseSlider, analogEffects.staticNoise);
            Set(snowAmountSlider, analogEffects.snowAmount);
            Set(analogSaturationSlider, analogEffects.saturation);
            Set(analogHueShiftSlider, analogEffects.hueShift);
            Set(analogGammaSlider, analogEffects.gamma);
        }
        
        // Feedback Effect
        if (feedbackEffect != null)
        {
            Set(feedbackEffectEnableToggle, feedbackEffect.enableFeedback);
            Set(feedbackEffectAmountSlider, feedbackEffect.feedbackAmount);
            Set(feedbackEffectZoomSlider, feedbackEffect.feedbackZoom);
            Set(feedbackEffectRotationSlider, feedbackEffect.feedbackRotation);
            Set(feedbackEffectOffsetXSlider, feedbackEffect.feedbackOffsetX);
            Set(feedbackEffectOffsetYSlider, feedbackEffect.feedbackOffsetY);
            Set(feedbackHueShiftSlider, feedbackEffect.feedbackHueShift);
            Set(feedbackSaturationSlider, feedbackEffect.feedbackSaturation);
            Set(feedbackBrightnessSlider, feedbackEffect.feedbackBrightness);
        }
        
        // Presets
        if (presetManager != null)
        {
            Set(presetMorphToggle, true);
            Set(presetMorphDurationSlider, presetManager.morphDuration);
            UpdatePresetDropdown();
            
            presetManager.OnPresetLoaded += OnPresetLoaded;
            presetManager.OnPresetSaved += _ => UpdatePresetDropdown();
        }
        
        // Recording
        if (videoRecorder != null)
        {
            Set(recordingToggle, videoRecorder.isRecording);
            Set(recordingFPSSlider, videoRecorder.targetFPS);
            
            videoRecorder.OnRecordingStarted += () => UpdateRecordingStatus(true);
            videoRecorder.OnRecordingStopped += _ => UpdateRecordingStatus(false);
        }
        
        // OSC
        if (oscReceiver != null)
        {
            Set(oscEnableToggle, oscReceiver.enableOSC);
            Set(oscLogToggle, oscReceiver.logMessages);
            UpdateOSCStatus();
        }
        
        // MIDI
        if (midiInput != null)
        {
            Set(midiEnableToggle, midiInput.enableMIDI);
            Set(midiLearnToggle, midiInput.learnMode);
        }
        
        // ===== NEW EXPERIMENTAL FEATURES =====
        
        // Glitch Effects
        if (glitchEffects != null)
        {
            Set(glitchEnableToggle, glitchEffects.enableGlitch);
            Set(glitchIntensitySlider, glitchEffects.glitchIntensity);
            Set(glitchAutoTriggerToggle, glitchEffects.autoTrigger);
            Set(glitchIntervalSlider, glitchEffects.triggerInterval);
            Set(glitchOnBeatToggle, glitchEffects.glitchOnBeat);
            Debug.Log($"[UI] GlitchEffects initialized: enabled={glitchEffects.enableGlitch}");
        }
        else Debug.LogWarning("[UI] GlitchEffects not found in scene");
        
        // Color Palette
        if (colorPalette != null)
        {
            Set(paletteEnableToggle, colorPalette.enablePalette);
            Set(paletteAutoToggle, colorPalette.autoTransition);
            Set(paletteTransitionSlider, colorPalette.transitionDuration);
            Debug.Log($"[UI] ColorPalette initialized: enabled={colorPalette.enablePalette}");
        }
        else Debug.LogWarning("[UI] ColorPaletteSystem not found in scene");
        
        // Mirror/Kaleidoscope
        if (mirrorKaleidoscope != null)
        {
            Set(mirrorEnableToggle, mirrorKaleidoscope.enableMirror);
            Set(kaleidoscopeEnableToggle, mirrorKaleidoscope.enableKaleidoscope);
            Set(kaleidoscopeSegmentsSlider, mirrorKaleidoscope.kaleidoscopeSegments);
            Set(kaleidoscopeAnimateToggle, mirrorKaleidoscope.animateRotation);
            Set(kaleidoscopeSpeedSlider, mirrorKaleidoscope.rotationSpeed);
            Debug.Log($"[UI] MirrorKaleidoscope initialized");
        }
        else Debug.LogWarning("[UI] MirrorKaleidoscope not found in scene");
        
        // Auto Randomizer
        if (autoRandomizer != null)
        {
            Set(randomizerEnableToggle, autoRandomizer.enableRandomizer);
            Set(randomizerSpeedSlider, autoRandomizer.globalSpeed);
            Set(randomizerIntensitySlider, autoRandomizer.globalIntensity);
            Set(randomizerDisplacementToggle, autoRandomizer.randomizeDisplacement);
            Set(randomizerWaveToggle, autoRandomizer.randomizeWave);
            Set(randomizerRotationToggle, autoRandomizer.randomizeRotation);
            Set(randomizerHueToggle, autoRandomizer.randomizeHue);
            Debug.Log($"[UI] AutoRandomizer initialized: enabled={autoRandomizer.enableRandomizer}");
        }
        else Debug.LogWarning("[UI] AutoRandomizer not found in scene");
        
        // Synthwave Grid
        if (synthwaveGrid != null)
        {
            Set(gridEnableToggle, synthwaveGrid.enableGrid);
            Set(gridScrollSpeedSlider, synthwaveGrid.scrollSpeed);
            Set(gridAudioReactiveToggle, synthwaveGrid.reactToAudio);
            Set(gridShowSunToggle, synthwaveGrid.showSun);
            Set(gridAnimateColorsToggle, synthwaveGrid.animateColors);
            Debug.Log($"[UI] SynthwaveGrid initialized: enabled={synthwaveGrid.enableGrid}");
        }
        else Debug.LogWarning("[UI] SynthwaveGrid not found in scene");
        
        // Motion Trails
        if (motionTrails != null)
        {
            Set(trailsEnableToggle, motionTrails.enableTrails);
            Set(trailsCountSlider, motionTrails.trailCount);
            Set(trailsOpacitySlider, motionTrails.trailOpacity);
            Set(trailsColorShiftToggle, motionTrails.colorShift);
            Debug.Log($"[UI] MotionTrails initialized: enabled={motionTrails.enableTrails}");
        }
        else Debug.LogWarning("[UI] MotionTrails not found in scene");
        
        // Depth Colorizer
        if (depthColorizer != null)
        {
            Set(depthColorEnableToggle, depthColorizer.enableDepthColor);
            Set(depthRainbowAnimateToggle, depthColorizer.animateRainbow);
            Debug.Log($"[UI] DepthColorizer initialized: enabled={depthColorizer.enableDepthColor}");
        }
        else Debug.LogWarning("[UI] DepthColorizer not found in scene");
        
        // Strobe Controller
        if (strobeController != null)
        {
            Set(strobeEnableToggle, strobeController.enableStrobe);
            Set(strobeRateSlider, strobeController.strobeRate);
            Set(strobeBeatSyncToggle, strobeController.syncToBeat);
            Set(flashOnBeatToggle, strobeController.flashOnBeat);
            Debug.Log($"[UI] StrobeController initialized: enabled={strobeController.enableStrobe}");
        }
        else Debug.LogWarning("[UI] StrobeController not found in scene");
        
        // Screen Shake
        if (screenShake != null)
        {
            Set(shakeEnableToggle, screenShake.enableShake);
            Set(shakeIntensitySlider, screenShake.shakeIntensity);
            Set(shakeOnBeatToggle, screenShake.shakeOnBeat);
            Set(shakeContinuousToggle, screenShake.continuousShake);
            Debug.Log($"[UI] ScreenShake initialized: enabled={screenShake.enableShake}");
        }
        else Debug.LogWarning("[UI] ScreenShake not found in scene");
    }
    
    void Set(Slider s, float v) { if (s) s.SetValueWithoutNotify(v); }
    void Set(Toggle t, bool v) { if (t) t.SetIsOnWithoutNotify(v); }
    
    private void BindEvents()
    {
        if (settings == null) return;
        
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
        
        // Animation
        if (animator != null)
        {
            Bind(animRotationToggle, v => animator.SetAutoRotate(v));
            Bind(animRotationSpeedSlider, v => animator.SetRotateSpeed(v));
            Bind(animHueToggle, v => animator.SetHueCycle(v));
            Bind(animHueSpeedSlider, v => animator.SetHueSpeed(v));
            Bind(animWaveToggle, v => animator.SetWaveAnimate(v));
            Bind(animWaveSpeedSlider, v => animator.SetWaveAnimSpeed(v));
            Bind(animZToggle, v => animator.SetZPulse(v));
            Bind(animZSpeedSlider, v => animator.SetZPulseSpeed(v));
            Bind(animScaleToggle, v => animator.SetBreathe(v));
            Bind(animScaleSpeedSlider, v => animator.SetBreatheSpeed(v));
            Bind(animDistortToggle, v => animator.SetDistortAnimate(v));
            Bind(animDistortSpeedSlider, v => animator.SetDistortSpeed(v));
        }
    }
    
    private void BindNewFeatureEvents()
    {
        // ===== AUDIO REACTIVE =====
        if (audioReactive != null)
        {
            Bind(audioEnableToggle, v => audioReactive.SetEnabled(v));
            Bind(audioGainSlider, v => audioReactive.inputGain = v);
            Bind(audioSmoothingSlider, v => audioReactive.smoothing = v);
            Bind(audioDisplacementToggle, v => { audioReactive.modulateDisplacement = v; audioReactive.RecaptureBaseValues(); });
            Bind(audioDisplacementAmountSlider, v => audioReactive.displacementAmount = v);
            Bind(audioWaveToggle, v => { audioReactive.modulateWave = v; audioReactive.RecaptureBaseValues(); });
            Bind(audioWaveAmountSlider, v => audioReactive.waveAmount = v);
            Bind(audioHueToggle, v => { audioReactive.modulateHue = v; audioReactive.RecaptureBaseValues(); });
            Bind(audioHueAmountSlider, v => audioReactive.hueAmount = v);
            Bind(audioScaleToggle, v => { audioReactive.modulateScale = v; audioReactive.RecaptureBaseValues(); });
            Bind(audioScaleAmountSlider, v => audioReactive.scaleAmount = v);
            Bind(audioRotationToggle, v => { audioReactive.modulateRotation = v; audioReactive.RecaptureBaseValues(); });
            Bind(audioRotationAmountSlider, v => audioReactive.rotationAmount = v);
            Bind(audioGlowToggle, v => { audioReactive.modulateGlow = v; audioReactive.RecaptureBaseValues(); });
            Bind(audioGlowAmountSlider, v => audioReactive.glowAmount = v);
            Bind(audioBeatFlashToggle, v => audioReactive.flashOnBeat = v);
            Bind(audioBeatPulseToggle, v => audioReactive.pulseOnBeat = v);
            Bind(audioBeatIntensitySlider, v => audioReactive.beatIntensity = v);
            
            // Audio device dropdown
            audioDeviceDropdown?.onValueChanged.AddListener(i => audioReactive.SelectDevice(i));
        }
        
        // ===== ANALOG EFFECTS =====
        if (analogEffects != null)
        {
            // CRT
            Bind(crtEnableToggle, v => analogEffects.enableCRT = v);
            Bind(crtScanlineSlider, v => analogEffects.scanlineIntensity = v);
            Bind(crtPhosphorSlider, v => analogEffects.phosphorGlow = v);
            Bind(crtCurvatureSlider, v => analogEffects.screenCurvature = v);
            Bind(crtVignetteSlider, v => analogEffects.vignette = v);
            
            // VHS
            Bind(vhsEnableToggle, v => analogEffects.enableVHS = v);
            Bind(vhsTrackingSlider, v => analogEffects.trackingNoise = v);
            Bind(vhsColorBleedSlider, v => analogEffects.colorBleed = v);
            Bind(vhsTapeNoiseSlider, v => analogEffects.tapeNoise = v);
            Bind(vhsJitterSlider, v => analogEffects.horizontalJitter = v);
            
            // Other
            Bind(chromaticEnableToggle, v => analogEffects.enableChromatic = v);
            Bind(chromaticAmountSlider, v => analogEffects.chromaticAmount = v);
            Bind(holdDriftEnableToggle, v => analogEffects.enableHoldDrift = v);
            Bind(holdHorizontalSlider, v => analogEffects.horizontalHold = v);
            Bind(holdVerticalSlider, v => analogEffects.verticalHold = v);
            Bind(signalNoiseEnableToggle, v => analogEffects.enableSignalNoise = v);
            Bind(staticNoiseSlider, v => analogEffects.staticNoise = v);
            Bind(snowAmountSlider, v => analogEffects.snowAmount = v);
            Bind(analogSaturationSlider, v => analogEffects.saturation = v);
            Bind(analogHueShiftSlider, v => analogEffects.hueShift = v);
            Bind(analogGammaSlider, v => analogEffects.gamma = v);
        }
        
        // ===== FEEDBACK EFFECT =====
        if (feedbackEffect != null)
        {
            Bind(feedbackEffectEnableToggle, v => feedbackEffect.enableFeedback = v);
            Bind(feedbackEffectAmountSlider, v => feedbackEffect.feedbackAmount = v);
            Bind(feedbackEffectZoomSlider, v => feedbackEffect.feedbackZoom = v);
            Bind(feedbackEffectRotationSlider, v => feedbackEffect.feedbackRotation = v);
            Bind(feedbackEffectOffsetXSlider, v => feedbackEffect.feedbackOffsetX = v);
            Bind(feedbackEffectOffsetYSlider, v => feedbackEffect.feedbackOffsetY = v);
            Bind(feedbackHueShiftSlider, v => feedbackEffect.feedbackHueShift = v);
            Bind(feedbackSaturationSlider, v => feedbackEffect.feedbackSaturation = v);
            Bind(feedbackBrightnessSlider, v => feedbackEffect.feedbackBrightness = v);
            feedbackClearButton?.onClick.AddListener(() => feedbackEffect.ClearFeedback());
        }
        
        // ===== PRESETS =====
        if (presetManager != null)
        {
            presetDropdown?.onValueChanged.AddListener(i => { if (i >= 0 && i < presetManager.presets.Count) presetManager.LoadPresetByIndex(i, presetMorphToggle?.isOn ?? true); });
            presetSaveButton?.onClick.AddListener(() => { presetManager.SaveCurrentAsPreset($"Preset_{System.DateTime.Now:HHmmss}"); UpdatePresetDropdown(); });
            presetNextButton?.onClick.AddListener(() => presetManager.NextPreset(presetMorphToggle?.isOn ?? true));
            presetPrevButton?.onClick.AddListener(() => presetManager.PreviousPreset(presetMorphToggle?.isOn ?? true));
            presetRandomButton?.onClick.AddListener(() => presetManager.RandomPreset(presetMorphToggle?.isOn ?? true));
            Bind(presetMorphDurationSlider, v => presetManager.morphDuration = v);
        }
        
        // ===== RECORDING =====
        if (videoRecorder != null)
        {
            Bind(recordingToggle, v => { if (v) videoRecorder.StartRecording(); else videoRecorder.StopRecording(); });
            screenshotButton?.onClick.AddListener(() => videoRecorder.TakeScreenshot());
            Bind(recordingFPSSlider, v => videoRecorder.targetFPS = Mathf.RoundToInt(v));
        }
        
        // ===== OSC =====
        if (oscReceiver != null)
        {
            Bind(oscEnableToggle, v => { oscReceiver.enableOSC = v; if (v) oscReceiver.StartListening(); else oscReceiver.StopListening(); UpdateOSCStatus(); });
            Bind(oscLogToggle, v => oscReceiver.logMessages = v);
        }
        
        // ===== MIDI =====
        if (midiInput != null)
        {
            Bind(midiEnableToggle, v => midiInput.enableMIDI = v);
            Bind(midiLearnToggle, v => midiInput.learnMode = v);
        }
        
        // ===== GLITCH EFFECTS =====
        if (glitchEffects != null)
        {
            Bind(glitchEnableToggle, v => { glitchEffects.enableGlitch = v; Debug.Log($"[GlitchEffects] Enabled: {v}"); });
            Bind(glitchIntensitySlider, v => glitchEffects.glitchIntensity = v);
            Bind(glitchAutoTriggerToggle, v => { glitchEffects.autoTrigger = v; Debug.Log($"[GlitchEffects] AutoTrigger: {v}"); });
            Bind(glitchIntervalSlider, v => glitchEffects.triggerInterval = v);
            Bind(glitchOnBeatToggle, v => { glitchEffects.glitchOnBeat = v; Debug.Log($"[GlitchEffects] GlitchOnBeat: {v}"); });
            glitchTriggerButton?.onClick.AddListener(() => glitchEffects.TriggerGlitch());
        }
        
        // ===== COLOR PALETTE =====
        if (colorPalette != null)
        {
            Bind(paletteEnableToggle, v => { colorPalette.enablePalette = v; Debug.Log($"[ColorPalette] Enabled: {v}"); });
            paletteDropdown?.onValueChanged.AddListener(i => { colorPalette.ApplyPaletteByIndex(i); Debug.Log($"[ColorPalette] Selected palette index: {i}"); });
            paletteNextButton?.onClick.AddListener(() => { colorPalette.NextPalette(); Debug.Log("[ColorPalette] Next palette"); });
            palettePrevButton?.onClick.AddListener(() => { colorPalette.PreviousPalette(); Debug.Log("[ColorPalette] Previous palette"); });
            Bind(paletteAutoToggle, v => { colorPalette.autoTransition = v; Debug.Log($"[ColorPalette] AutoTransition: {v}"); });
            Bind(paletteTransitionSlider, v => colorPalette.transitionDuration = v);
        }
        
        // ===== MIRROR KALEIDOSCOPE =====
        if (mirrorKaleidoscope != null)
        {
            Bind(mirrorEnableToggle, v => { mirrorKaleidoscope.enableMirror = v; Debug.Log($"[MirrorKaleidoscope] Mirror Enabled: {v}"); });
            mirrorModeDropdown?.onValueChanged.AddListener(i => { mirrorKaleidoscope.mirrorMode = (MirrorKaleidoscope.MirrorMode)i; Debug.Log($"[MirrorKaleidoscope] Mode: {i}"); });
            Bind(kaleidoscopeEnableToggle, v => { mirrorKaleidoscope.enableKaleidoscope = v; Debug.Log($"[MirrorKaleidoscope] Kaleidoscope Enabled: {v}"); });
            Bind(kaleidoscopeSegmentsSlider, v => mirrorKaleidoscope.kaleidoscopeSegments = Mathf.RoundToInt(v));
            Bind(kaleidoscopeAnimateToggle, v => mirrorKaleidoscope.animateRotation = v);
            Bind(kaleidoscopeSpeedSlider, v => mirrorKaleidoscope.rotationSpeed = v);
        }
        
        // ===== AUTO RANDOMIZER =====
        if (autoRandomizer != null)
        {
            Bind(randomizerEnableToggle, v => { 
                autoRandomizer.enableRandomizer = v; 
                Debug.Log($"[AutoRandomizer] Enabled: {v}");
            });
            Bind(randomizerSpeedSlider, v => autoRandomizer.globalSpeed = v);
            Bind(randomizerIntensitySlider, v => autoRandomizer.globalIntensity = v);
            Bind(randomizerDisplacementToggle, v => autoRandomizer.randomizeDisplacement = v);
            Bind(randomizerWaveToggle, v => autoRandomizer.randomizeWave = v);
            Bind(randomizerRotationToggle, v => autoRandomizer.randomizeRotation = v);
            Bind(randomizerHueToggle, v => autoRandomizer.randomizeHue = v);
            randomizerResetButton?.onClick.AddListener(() => autoRandomizer.ResetToBase());
        }
        
        // ===== SYNTHWAVE GRID =====
        if (synthwaveGrid != null)
        {
            Bind(gridEnableToggle, v => { synthwaveGrid.enableGrid = v; Debug.Log($"[SynthwaveGrid] Enabled: {v}"); });
            Bind(gridScrollSpeedSlider, v => synthwaveGrid.scrollSpeed = v);
            Bind(gridAudioReactiveToggle, v => { synthwaveGrid.reactToAudio = v; Debug.Log($"[SynthwaveGrid] AudioReactive: {v}"); });
            Bind(gridShowSunToggle, v => synthwaveGrid.showSun = v);
            Bind(gridAnimateColorsToggle, v => synthwaveGrid.animateColors = v);
        }
        
        // ===== MOTION TRAILS =====
        if (motionTrails != null)
        {
            Bind(trailsEnableToggle, v => { motionTrails.enableTrails = v; Debug.Log($"[MotionTrails] Enabled: {v}"); });
            Bind(trailsCountSlider, v => motionTrails.trailCount = Mathf.RoundToInt(v));
            Bind(trailsOpacitySlider, v => motionTrails.trailOpacity = v);
            Bind(trailsColorShiftToggle, v => motionTrails.colorShift = v);
            trailsClearButton?.onClick.AddListener(() => { motionTrails.ClearTrails(); Debug.Log("[MotionTrails] Cleared"); });
        }
        
        // ===== DEPTH COLORIZER =====
        if (depthColorizer != null)
        {
            Bind(depthColorEnableToggle, v => { depthColorizer.enableDepthColor = v; Debug.Log($"[DepthColorizer] Enabled: {v}"); });
            depthColorModeDropdown?.onValueChanged.AddListener(i => { depthColorizer.colorMode = (DepthColorizer.ColorMode)i; Debug.Log($"[DepthColorizer] Mode: {i}"); });
            Bind(depthRainbowAnimateToggle, v => depthColorizer.animateRainbow = v);
        }
        
        // ===== STROBE CONTROLLER =====
        if (strobeController != null)
        {
            Bind(strobeEnableToggle, v => { strobeController.enableStrobe = v; Debug.Log($"[StrobeController] Enabled: {v}"); });
            Bind(strobeRateSlider, v => strobeController.strobeRate = v);
            Bind(strobeBeatSyncToggle, v => { strobeController.syncToBeat = v; Debug.Log($"[StrobeController] SyncToBeat: {v}"); });
            Bind(flashOnBeatToggle, v => { strobeController.flashOnBeat = v; Debug.Log($"[StrobeController] FlashOnBeat: {v}"); });
            flashTriggerButton?.onClick.AddListener(() => { strobeController.TriggerFlash(); Debug.Log("[StrobeController] Flash triggered"); });
            blackoutButton?.onClick.AddListener(() => { strobeController.TriggerBlackout(); Debug.Log("[StrobeController] Blackout triggered"); });
        }
        
        // ===== SCREEN SHAKE =====
        if (screenShake != null)
        {
            Bind(shakeEnableToggle, v => { screenShake.enableShake = v; Debug.Log($"[ScreenShake] Enabled: {v}"); });
            Bind(shakeIntensitySlider, v => screenShake.shakeIntensity = v);
            Bind(shakeOnBeatToggle, v => { screenShake.shakeOnBeat = v; Debug.Log($"[ScreenShake] ShakeOnBeat: {v}"); });
            Bind(shakeContinuousToggle, v => { screenShake.continuousShake = v; Debug.Log($"[ScreenShake] Continuous: {v}"); });
            shakeTriggerButton?.onClick.AddListener(() => { screenShake.TriggerShake(); Debug.Log("[ScreenShake] Shake triggered"); });
        }
    }
    
    void Bind(Slider s, System.Action<float> a) { s?.onValueChanged.AddListener(v => a(v)); }
    void Bind(Toggle t, System.Action<bool> a) { t?.onValueChanged.AddListener(v => a(v)); }
    void Refresh() { controller?.meshGenerator?.RefreshMesh(); }
    
    void UpdatePrimaryColor()
    {
        if (settings == null) return;
        float h = primaryHueSlider ? primaryHueSlider.value : 0;
        float s = primarySatSlider ? primarySatSlider.value : 1;
        settings.primaryColor = Color.HSVToRGB(h, s, 1f);
    }
    
    void UpdateSecondaryColor()
    {
        if (settings == null) return;
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
        
        // Update audio level display
        if (audioReactive != null && audioLevelText != null)
        {
            audioLevelText.text = $"B:{audioReactive.bass:F2} M:{audioReactive.mid:F2} T:{audioReactive.treble:F2}";
        }
        
        // Update recording status
        if (videoRecorder != null && recordingStatusText != null && videoRecorder.isRecording)
        {
            recordingStatusText.text = $"REC {videoRecorder.GetRecordingDuration():F1}s ({videoRecorder.GetFrameCount()} frames)";
        }
    }
    
    void UpdateResolutionText()
    {
        if (resolutionText && settings)
            resolutionText.text = $"{settings.horizontalResolution} x {settings.verticalResolution}";
    }
    
    void UpdateAudioDeviceDropdown()
    {
        if (audioDeviceDropdown == null || audioReactive == null) return;
        
        audioDeviceDropdown.ClearOptions();
        var devices = audioReactive.GetDeviceList();
        audioDeviceDropdown.AddOptions(devices);
        
        if (audioReactive.selectedDeviceIndex < devices.Count)
        {
            audioDeviceDropdown.SetValueWithoutNotify(audioReactive.selectedDeviceIndex);
        }
    }
    
    void UpdatePresetDropdown()
    {
        if (presetDropdown == null || presetManager == null) return;
        
        presetDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var preset in presetManager.presets)
        {
            options.Add(preset.name);
        }
        if (options.Count == 0)
        {
            options.Add("No presets");
        }
        presetDropdown.AddOptions(options);
        
        if (presetManager.currentPresetIndex >= 0 && presetManager.currentPresetIndex < options.Count)
            presetDropdown.SetValueWithoutNotify(presetManager.currentPresetIndex);
    }
    
    void OnPresetLoaded(PresetManager.Preset preset)
    {
        if (presetNameText != null) presetNameText.text = preset.name;
        if (presetDropdown != null && presetManager != null) 
            presetDropdown.SetValueWithoutNotify(presetManager.currentPresetIndex);
        InitializeUI(); // Refresh all UI values
    }
    
    void UpdateRecordingStatus(bool isRecording)
    {
        if (recordingToggle != null) recordingToggle.SetIsOnWithoutNotify(isRecording);
        if (recordingStatusText != null) recordingStatusText.text = isRecording ? "Recording..." : "Ready";
    }
    
    void UpdateOSCStatus()
    {
        if (oscStatusText != null && oscReceiver != null)
        {
            oscStatusText.text = oscReceiver.enableOSC ? $"Listening on port {oscReceiver.listenPort}" : "Disabled";
        }
    }
    
    public void ResetCamera()
    {
        Camera.main?.GetComponent<OrbitCamera>()?.ResetView();
        controller?.ResetCamera();
    }
    
    public void ResetAll()
    {
        Debug.Log("[ResetAll] Resetting all settings and features...");
        
        if (!settings) 
        {
            Debug.LogWarning("[ResetAll] Settings is null!");
            return;
        }
        
        // Core settings
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
        
        // Webcam
        if (webcamCapture) { webcamCapture.mirrorHorizontal = true; webcamCapture.mirrorVertical = false; }
        
        // Reset feedback effect
        feedbackEffect?.ClearFeedback();
        
        // Reset audio reactive
        if (audioReactive != null)
        {
            audioReactive.modulateDisplacement = false;
            audioReactive.modulateWave = false;
            audioReactive.modulateHue = false;
            audioReactive.modulateScale = false;
            audioReactive.modulateRotation = false;
            audioReactive.modulateGlow = false;
            audioReactive.flashOnBeat = false;
            audioReactive.pulseOnBeat = false;
            audioReactive.RecaptureBaseValues();
        }
        
        // Reset analog effects
        if (analogEffects != null)
        {
            analogEffects.enableCRT = false;
            analogEffects.enableVHS = false;
            analogEffects.enableChromatic = false;
            analogEffects.enableHoldDrift = false;
            analogEffects.enableSignalNoise = false;
        }
        
        // Reset animator
        if (animator != null)
        {
            animator.SetAutoRotate(false);
            animator.SetHueCycle(false);
            animator.SetWaveAnimate(false);
            animator.SetZPulse(false);
            animator.SetBreathe(false);
            animator.SetDistortAnimate(false);
        }
        
        // ===== RESET NEW EXPERIMENTAL FEATURES =====
        
        // Glitch Effects
        if (glitchEffects != null)
        {
            glitchEffects.enableGlitch = false;
            glitchEffects.autoTrigger = false;
            glitchEffects.glitchOnBeat = false;
            Debug.Log("[ResetAll] GlitchEffects disabled");
        }
        
        // Color Palette
        if (colorPalette != null)
        {
            colorPalette.enablePalette = false;
            colorPalette.autoTransition = false;
            Debug.Log("[ResetAll] ColorPalette disabled");
        }
        
        // Mirror/Kaleidoscope
        if (mirrorKaleidoscope != null)
        {
            mirrorKaleidoscope.enableMirror = false;
            mirrorKaleidoscope.enableKaleidoscope = false;
            mirrorKaleidoscope.animateRotation = false;
            Debug.Log("[ResetAll] MirrorKaleidoscope disabled");
        }
        
        // Auto Randomizer
        if (autoRandomizer != null)
        {
            autoRandomizer.enableRandomizer = false;
            autoRandomizer.ResetToBase();
            Debug.Log("[ResetAll] AutoRandomizer disabled");
        }
        
        // Synthwave Grid
        if (synthwaveGrid != null)
        {
            synthwaveGrid.enableGrid = false;
            synthwaveGrid.reactToAudio = false;
            Debug.Log("[ResetAll] SynthwaveGrid disabled");
        }
        
        // Motion Trails
        if (motionTrails != null)
        {
            motionTrails.enableTrails = false;
            motionTrails.ClearTrails();
            Debug.Log("[ResetAll] MotionTrails disabled");
        }
        
        // Depth Colorizer
        if (depthColorizer != null)
        {
            depthColorizer.enableDepthColor = false;
            Debug.Log("[ResetAll] DepthColorizer disabled");
        }
        
        // Strobe Controller
        if (strobeController != null)
        {
            strobeController.enableStrobe = false;
            strobeController.flashOnBeat = false;
            strobeController.syncToBeat = false;
            Debug.Log("[ResetAll] StrobeController disabled");
        }
        
        // Screen Shake
        if (screenShake != null)
        {
            screenShake.enableShake = false;
            screenShake.shakeOnBeat = false;
            screenShake.continuousShake = false;
            Debug.Log("[ResetAll] ScreenShake disabled");
        }
        
        // Re-initialize all UI elements to reflect reset values
        InitializeUI();
        InitializeNewFeatures();
        
        Refresh();
        ResetCamera();
        
        Debug.Log("[ResetAll] Complete - all settings and features reset to defaults");
    }
}
