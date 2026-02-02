#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

public static class RuttEtraUICreator
{
    const float PW = 360f, RH = 26f;
    const int FS = 13;
    static Font F() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [MenuItem("RuttEtra/Fix EventSystem")]
    public static void FixEventSystem() { EnsureES(); }

    [MenuItem("RuttEtra/Add Orbit Camera")]
    public static void AddOrbitCamera()
    {
        var cam = Camera.main; if (!cam) return;
        var t = System.Type.GetType("OrbitCamera, Assembly-CSharp"); if (t == null) return;
        if (!cam.GetComponent(t)) cam.gameObject.AddComponent(t);
        var mesh = Object.FindFirstObjectByType<RuttEtraMeshGenerator>();
        if (mesh) t.GetField("target")?.SetValue(cam.GetComponent(t), mesh.transform);
        EditorUtility.SetDirty(cam.gameObject);
    }

    [MenuItem("RuttEtra/Create UI")]
    public static void CreateRuttEtraUI()
    {
        var ctrl = Object.FindFirstObjectByType<RuttEtraController>();
        if (!ctrl || !ctrl.settings) { EditorUtility.DisplayDialog("Error", "Setup scene first", "OK"); return; }
        
        var old = GameObject.Find("RuttEtra_Canvas"); if (old) Object.DestroyImmediate(old);
        var s = ctrl.settings;
        var wc = Object.FindFirstObjectByType<WebcamCapture>();
        EnsureES();

        var canvas = new GameObject("RuttEtra_Canvas");
        var c = canvas.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 100;
        var sc = canvas.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        canvas.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("ControlPanel"); panel.transform.SetParent(canvas.transform, false);
        var prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(1, 0); prt.anchorMax = new Vector2(1, 1);
        prt.pivot = new Vector2(1, 1); prt.sizeDelta = new Vector2(PW, 0); prt.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.09f, 0.96f);

        var scroll = new GameObject("Scroll"); scroll.transform.SetParent(panel.transform, false);
        var srt = scroll.AddComponent<RectTransform>();
        srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
        srt.offsetMin = new Vector2(4, 4); srt.offsetMax = new Vector2(-4, -4);
        scroll.AddComponent<RectMask2D>();
        var sr = scroll.AddComponent<ScrollRect>(); sr.horizontal = false; sr.scrollSensitivity = 25;

        var content = new GameObject("Content"); content.transform.SetParent(scroll.transform, false);
        var crt = content.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1); crt.sizeDelta = crt.anchoredPosition = Vector2.zero;
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.spacing = 1; vlg.padding = new RectOffset(2, 2, 2, 12);
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = crt; sr.viewport = srt;

        var uiGo = new GameObject("RuttEtra_UI"); uiGo.transform.SetParent(canvas.transform, false);
        uiGo.AddComponent<RectTransform>();
        var ui = uiGo.AddComponent<RuttEtraUI>();
        ui.controller = ctrl; ui.settings = s; ui.webcamCapture = wc; ui.controlPanel = panel;
        ui.animator = Object.FindFirstObjectByType<RuttEtraAnimator>();
        
        // Find new feature components
        ui.audioReactive = Object.FindFirstObjectByType<AudioReactive>();
        ui.analogEffects = Object.FindFirstObjectByType<AnalogEffects>();
        ui.feedbackEffect = Object.FindFirstObjectByType<FeedbackEffect>();
        ui.presetManager = Object.FindFirstObjectByType<PresetManager>();
        ui.videoRecorder = Object.FindFirstObjectByType<VideoRecorder>();
        ui.oscReceiver = Object.FindFirstObjectByType<OSCReceiver>();
        ui.midiInput = Object.FindFirstObjectByType<MIDIInput>();

        // ============ ORIGINAL CONTROLS ============
        
        // INPUT SIGNAL
        H(content, "INPUT SIGNAL");
        ui.brightnessSlider = S(content, "Brightness", -1, 1, s.brightness);
        ui.contrastSlider = S(content, "Contrast", 0.1f, 3, s.contrast);
        ui.thresholdSlider = S(content, "Threshold", 0, 1, s.threshold);
        ui.gammaSlider = S(content, "Gamma", 0.1f, 3, s.gamma);
        ui.edgeDetectToggle = T(content, "Edge Detect", s.edgeDetect);
        ui.posterizeSlider = S(content, "Posterize", 1, 8, s.posterize, true);

        // Z-AXIS
        H(content, "Z-AXIS DISPLACEMENT");
        ui.displacementSlider = S(content, "Strength", 0, 5, s.displacementStrength);
        ui.smoothingSlider = S(content, "Smoothing", 0, 1, s.displacementSmoothing);
        ui.displacementOffsetSlider = S(content, "Offset", -1, 1, s.displacementOffset);
        ui.invertToggle = T(content, "Invert", s.invertDisplacement);
        ui.zModulationSlider = S(content, "Z Modulation", 0, 1, s.zModulation);
        ui.zModFreqSlider = S(content, "Z Mod Freq", 0, 5, s.zModFrequency);

        // RASTER POSITION
        H(content, "RASTER POSITION");
        ui.hPositionSlider = S(content, "H Position", -10, 10, s.horizontalPosition);
        ui.vPositionSlider = S(content, "V Position", -10, 10, s.verticalPosition);

        // RASTER SCALE
        H(content, "RASTER SCALE");
        ui.hScaleSlider = S(content, "H Scale", 0.1f, 3, s.horizontalScale);
        ui.vScaleSlider = S(content, "V Scale", 0.1f, 3, s.verticalScale);
        ui.meshScaleSlider = S(content, "Overall Scale", 0.1f, 3, s.meshScale);

        // RASTER ROTATION
        H(content, "RASTER ROTATION");
        ui.rotationXSlider = S(content, "Rotation X", -180, 180, s.rotationX);
        ui.rotationYSlider = S(content, "Rotation Y", -180, 180, s.rotationY);
        ui.rotationZSlider = S(content, "Rotation Z", -180, 180, s.rotationZ);

        // DISTORTION
        H(content, "DISTORTION");
        ui.keystoneHSlider = S(content, "Keystone H", -1, 1, s.keystoneH);
        ui.keystoneVSlider = S(content, "Keystone V", -1, 1, s.keystoneV);
        ui.barrelSlider = S(content, "Barrel", -0.5f, 0.5f, s.barrelDistortion);

        // SCAN LINES
        H(content, "SCAN LINES");
        ui.scanLineSkipSlider = S(content, "Line Skip", 1, 8, s.scanLineSkip, true);
        ui.horizontalLinesToggle = T(content, "Horizontal", s.showHorizontalLines);
        ui.verticalLinesToggle = T(content, "Vertical", s.showVerticalLines);
        ui.interlaceToggle = T(content, "Interlace", s.interlace);

        // DEFLECTION WAVE
        H(content, "DEFLECTION WAVE");
        ui.horizontalWaveSlider = S(content, "H Wave", 0, 2, s.horizontalWave);
        ui.verticalWaveSlider = S(content, "V Wave", 0, 2, s.verticalWave);
        ui.waveFrequencySlider = S(content, "Frequency", 0, 10, s.waveFrequency);
        ui.waveSpeedSlider = S(content, "Speed", 0, 5, s.waveSpeed);

        // LINE STYLE
        H(content, "LINE STYLE");
        ui.lineWidthSlider = S(content, "Width", 0.001f, 0.05f, s.lineWidth);
        ui.lineTaperSlider = S(content, "Taper", 0, 1, s.lineTaper);
        ui.glowSlider = S(content, "Glow", 0, 2, s.glowIntensity);

        // COLORS
        H(content, "COLORS");
        Color.RGBToHSV(s.primaryColor, out float pH, out float pS, out _);
        Color.RGBToHSV(s.secondaryColor, out float sH, out float sS, out _);
        ui.primaryHueSlider = S(content, "Primary Hue", 0, 1, pH);
        ui.primarySatSlider = S(content, "Primary Sat", 0, 1, pS);
        ui.secondaryHueSlider = S(content, "Secondary Hue", 0, 1, sH);
        ui.secondarySatSlider = S(content, "Secondary Sat", 0, 1, sS);
        ui.colorBlendSlider = S(content, "Blend", 0, 1, s.colorBlend);
        ui.sourceColorToggle = T(content, "Source Color", s.useSourceColor);

        // RESOLUTION
        H(content, "RESOLUTION");
        ui.horizontalResSlider = S(content, "Horizontal", 16, 512, s.horizontalResolution, true);
        ui.verticalResSlider = S(content, "Vertical", 8, 256, s.verticalResolution, true);
        ui.resolutionText = TMP(content, $"{s.horizontalResolution} x {s.verticalResolution}");

        // FEEDBACK (Settings)
        H(content, "FEEDBACK (Settings)");
        ui.feedbackSlider = S(content, "Amount", 0, 0.98f, s.feedback);
        ui.feedbackZoomSlider = S(content, "Zoom", -0.1f, 0.1f, s.feedbackZoom);
        ui.feedbackRotSlider = S(content, "Rotation", -10, 10, s.feedbackRotation);

        // POST EFFECTS
        H(content, "POST EFFECTS");
        ui.noiseSlider = S(content, "Noise", 0, 1, s.noiseAmount);
        ui.persistenceSlider = S(content, "Persistence", 0, 1, s.persistence);
        ui.flickerSlider = S(content, "Flicker", 0, 1, s.scanlineFlicker);
        ui.bloomSlider = S(content, "Bloom", 0, 1, s.bloom);

        // WEBCAM
        H(content, "WEBCAM");
        ui.mirrorHorizontalToggle = T(content, "Mirror H", wc ? wc.mirrorHorizontal : true);
        ui.mirrorVerticalToggle = T(content, "Mirror V", wc ? wc.mirrorVertical : false);

        // ANIMATION
        H(content, "ANIMATION (LFO)");
        ui.animRotationToggle = T(content, "Rotate", false);
        ui.animRotationSpeedSlider = S(content, "Rot Speed", 0.1f, 3, 0.5f);
        ui.animHueToggle = T(content, "Hue Cycle", false);
        ui.animHueSpeedSlider = S(content, "Hue Speed", 0.1f, 2, 0.3f);
        ui.animWaveToggle = T(content, "Wave", false);
        ui.animWaveSpeedSlider = S(content, "Wave Spd", 0.1f, 3, 0.5f);
        ui.animZToggle = T(content, "Z Pulse", false);
        ui.animZSpeedSlider = S(content, "Z Speed", 0.1f, 3, 0.5f);
        ui.animScaleToggle = T(content, "Breathe", false);
        ui.animScaleSpeedSlider = S(content, "Breathe Spd", 0.1f, 2, 0.3f);
        ui.animDistortToggle = T(content, "Distortion", false);
        ui.animDistortSpeedSlider = S(content, "Distort Spd", 0.1f, 2, 0.4f);

        // ============ NEW FEATURE CONTROLS ============
        
        // AUDIO REACTIVE
        var ar = ui.audioReactive;
        H(content, "AUDIO REACTIVE", new Color(1f, 0.6f, 0.3f));
        ui.audioEnableToggle = T(content, "Enable Audio", ar ? ar.enableAudio : false);
        ui.audioDeviceDropdown = D(content, "Audio Device", ar != null ? ar.GetDeviceList() : new List<string>{ "No devices" });
        ui.audioGainSlider = S(content, "Input Gain", 0.1f, 10, ar ? ar.inputGain : 1);
        ui.audioSmoothingSlider = S(content, "Smoothing", 0, 1, ar ? ar.smoothing : 0.8f);
        ui.audioLevelText = TMP(content, "B:0.00 M:0.00 T:0.00");
        ui.audioDisplacementToggle = T(content, "Mod Displacement", ar ? ar.modulateDisplacement : false);
        ui.audioDisplacementAmountSlider = S(content, "Displace Amt", 0, 3, ar ? ar.displacementAmount : 1);
        ui.audioWaveToggle = T(content, "Mod Wave", ar ? ar.modulateWave : false);
        ui.audioWaveAmountSlider = S(content, "Wave Amt", 0, 2, ar ? ar.waveAmount : 0.5f);
        ui.audioHueToggle = T(content, "Mod Hue", ar ? ar.modulateHue : false);
        ui.audioHueAmountSlider = S(content, "Hue Amt", 0, 1, ar ? ar.hueAmount : 0.3f);
        ui.audioScaleToggle = T(content, "Mod Scale", ar ? ar.modulateScale : false);
        ui.audioScaleAmountSlider = S(content, "Scale Amt", 0, 0.5f, ar ? ar.scaleAmount : 0.2f);
        ui.audioRotationToggle = T(content, "Mod Rotation", ar ? ar.modulateRotation : false);
        ui.audioRotationAmountSlider = S(content, "Rotation Amt", 0, 30, ar ? ar.rotationAmount : 10);
        ui.audioGlowToggle = T(content, "Mod Glow", ar ? ar.modulateGlow : false);
        ui.audioGlowAmountSlider = S(content, "Glow Amt", 0, 2, ar ? ar.glowAmount : 1);
        H2(content, "Beat Detection");
        ui.audioBeatFlashToggle = T(content, "Flash on Beat", ar ? ar.flashOnBeat : false);
        ui.audioBeatPulseToggle = T(content, "Pulse on Beat", ar ? ar.pulseOnBeat : false);
        ui.audioBeatIntensitySlider = S(content, "Beat Intensity", 0, 1, ar ? ar.beatIntensity : 0.5f);
        
        // CRT EFFECTS
        var ae = ui.analogEffects;
        H(content, "CRT EFFECTS", new Color(0.3f, 1f, 0.6f));
        ui.crtEnableToggle = T(content, "Enable CRT", ae ? ae.enableCRT : false);
        ui.crtScanlineSlider = S(content, "Scanlines", 0, 1, ae ? ae.scanlineIntensity : 0.3f);
        ui.crtPhosphorSlider = S(content, "Phosphor Glow", 0, 1, ae ? ae.phosphorGlow : 0.2f);
        ui.crtCurvatureSlider = S(content, "Curvature", 0, 0.5f, ae ? ae.screenCurvature : 0.1f);
        ui.crtVignetteSlider = S(content, "Vignette", 0, 0.1f, ae ? ae.vignette : 0.05f);
        
        // VHS EFFECTS
        H(content, "VHS EFFECTS", new Color(0.3f, 1f, 0.6f));
        ui.vhsEnableToggle = T(content, "Enable VHS", ae ? ae.enableVHS : false);
        ui.vhsTrackingSlider = S(content, "Tracking Noise", 0, 1, ae ? ae.trackingNoise : 0.1f);
        ui.vhsColorBleedSlider = S(content, "Color Bleed", 0, 1, ae ? ae.colorBleed : 0.2f);
        ui.vhsTapeNoiseSlider = S(content, "Tape Noise", 0, 1, ae ? ae.tapeNoise : 0.1f);
        ui.vhsJitterSlider = S(content, "H Jitter", 0, 0.1f, ae ? ae.horizontalJitter : 0.02f);
        
        // ANALOG COLOR
        H(content, "ANALOG COLOR", new Color(0.3f, 1f, 0.6f));
        ui.chromaticEnableToggle = T(content, "Chromatic Abb", ae ? ae.enableChromatic : false);
        ui.chromaticAmountSlider = S(content, "Chromatic Amt", 0, 0.05f, ae ? ae.chromaticAmount : 0.01f);
        ui.holdDriftEnableToggle = T(content, "Hold Drift", ae ? ae.enableHoldDrift : false);
        ui.holdHorizontalSlider = S(content, "H Hold", 0, 1, ae ? ae.horizontalHold : 0);
        ui.holdVerticalSlider = S(content, "V Hold", 0, 1, ae ? ae.verticalHold : 0);
        ui.signalNoiseEnableToggle = T(content, "Signal Noise", ae ? ae.enableSignalNoise : false);
        ui.staticNoiseSlider = S(content, "Static", 0, 1, ae ? ae.staticNoise : 0.1f);
        ui.snowAmountSlider = S(content, "Snow", 0, 1, ae ? ae.snowAmount : 0);
        ui.analogSaturationSlider = S(content, "Saturation", 0, 2, ae ? ae.saturation : 1);
        ui.analogHueShiftSlider = S(content, "Hue Shift", -1, 1, ae ? ae.hueShift : 0);
        ui.analogGammaSlider = S(content, "Gamma", 0.5f, 2, ae ? ae.gamma : 1);
        
        // VISUAL FEEDBACK EFFECT
        var fe = ui.feedbackEffect;
        H(content, "VISUAL FEEDBACK", new Color(1f, 0.5f, 1f));
        ui.feedbackEffectEnableToggle = T(content, "Enable Feedback", fe ? fe.enableFeedback : false);
        ui.feedbackEffectAmountSlider = S(content, "Amount", 0, 0.99f, fe ? fe.feedbackAmount : 0.85f);
        ui.feedbackEffectZoomSlider = S(content, "Zoom", -0.1f, 0.1f, fe ? fe.feedbackZoom : 0.01f);
        ui.feedbackEffectRotationSlider = S(content, "Rotation", -10, 10, fe ? fe.feedbackRotation : 1);
        ui.feedbackEffectOffsetXSlider = S(content, "Offset X", -0.1f, 0.1f, fe ? fe.feedbackOffsetX : 0);
        ui.feedbackEffectOffsetYSlider = S(content, "Offset Y", -0.1f, 0.1f, fe ? fe.feedbackOffsetY : 0);
        ui.feedbackHueShiftSlider = S(content, "Hue Shift", 0.9f, 1.1f, fe ? fe.feedbackHueShift : 1);
        ui.feedbackSaturationSlider = S(content, "Saturation", 0.9f, 1.1f, fe ? fe.feedbackSaturation : 1);
        ui.feedbackBrightnessSlider = S(content, "Brightness", 0.9f, 1.1f, fe ? fe.feedbackBrightness : 0.98f);
        ui.feedbackClearButton = B(content, "Clear Feedback");
        
        // PRESETS
        var pm = ui.presetManager;
        H(content, "PRESETS", new Color(1f, 1f, 0.3f));
        ui.presetDropdown = D(content, "Select Preset", pm != null ? GetPresetNames(pm) : new List<string>());
        ui.presetNameText = TMP(content, pm != null && pm.currentPresetIndex >= 0 ? pm.presets[pm.currentPresetIndex].name : "No preset");
        var presetBtnRow = R(content, RH);
        var hlg1 = presetBtnRow.AddComponent<HorizontalLayoutGroup>();
        hlg1.spacing = 4; hlg1.childControlWidth = hlg1.childControlHeight = true;
        ui.presetPrevButton = BSmall(presetBtnRow, "<");
        ui.presetNextButton = BSmall(presetBtnRow, ">");
        ui.presetRandomButton = BSmall(presetBtnRow, "?");
        ui.presetSaveButton = BSmall(presetBtnRow, "Save");
        ui.presetMorphToggle = T(content, "Morph Transition", true);
        ui.presetMorphDurationSlider = S(content, "Morph Duration", 0.5f, 5, pm ? pm.morphDuration : 2);
        
        // RECORDING
        var vr = ui.videoRecorder;
        H(content, "RECORDING", new Color(1f, 0.3f, 0.3f));
        ui.recordingToggle = T(content, "Record", vr ? vr.isRecording : false);
        ui.screenshotButton = B(content, "Screenshot (F12)");
        ui.recordingFPSSlider = S(content, "Target FPS", 15, 60, vr ? vr.targetFPS : 30, true);
        ui.recordingStatusText = TMP(content, "Ready");
        
        // OSC CONTROL
        var osc = ui.oscReceiver;
        H(content, "OSC CONTROL", new Color(0.5f, 0.8f, 1f));
        ui.oscEnableToggle = T(content, "Enable OSC", osc ? osc.enableOSC : false);
        ui.oscLogToggle = T(content, "Log Messages", osc ? osc.logMessages : false);
        ui.oscStatusText = TMP(content, osc != null ? $"Port: {osc.listenPort}" : "Not available");
        
        // MIDI CONTROL
        var midi = ui.midiInput;
        H(content, "MIDI CONTROL", new Color(0.5f, 0.8f, 1f));
        ui.midiEnableToggle = T(content, "Enable MIDI", midi ? midi.enableMIDI : false);
        ui.midiLearnToggle = T(content, "Learn Mode", midi ? midi.learnMode : false);
        ui.midiStatusText = TMP(content, "Move control to learn");

        // ACTIONS
        H(content, "ACTIONS");
        ui.resetCameraButton = B(content, "Reset Camera");
        ui.resetAllButton = B(content, "Reset All");
        L(content, "Press H to hide | F1 for shortcuts");

        Undo.RegisterCreatedObjectUndo(canvas, "Create UI");
        Selection.activeGameObject = uiGo;
        
        Debug.Log("UI Created with all features! Run 'RuttEtra > Add All Features' if components are missing.");
    }
    
    static List<string> GetPresetNames(PresetManager pm)
    {
        var names = new List<string>();
        if (pm != null)
        {
            foreach (var preset in pm.presets)
                names.Add(preset.name);
        }
        return names;
    }

    static void EnsureES()
    {
        var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (!es) { var go = new GameObject("EventSystem"); es = go.AddComponent<UnityEngine.EventSystems.EventSystem>(); }
        if (!es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>())
        {
            var old = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (old) Object.DestroyImmediate(old);
            es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    static void H(GameObject p, string t, Color? c = null) 
    { 
        var go = R(p, RH + 4); 
        var txt = go.AddComponent<Text>(); 
        txt.font = F(); 
        txt.text = t; 
        txt.fontSize = FS; 
        txt.fontStyle = FontStyle.Bold; 
        txt.color = c ?? new Color(0.5f, 0.8f, 1f); 
        txt.alignment = TextAnchor.LowerLeft; 
    }
    
    static void H2(GameObject p, string t) 
    { 
        var go = R(p, RH); 
        var txt = go.AddComponent<Text>(); 
        txt.font = F(); 
        txt.text = "  " + t; 
        txt.fontSize = FS - 1; 
        txt.fontStyle = FontStyle.Italic; 
        txt.color = new Color(0.6f, 0.6f, 0.7f); 
        txt.alignment = TextAnchor.MiddleLeft; 
    }
    
    static void L(GameObject p, string t) { var go = R(p, RH); var txt = go.AddComponent<Text>(); txt.font = F(); txt.text = t; txt.fontSize = 10; txt.color = new Color(0.5f, 0.5f, 0.6f); txt.alignment = TextAnchor.MiddleCenter; }
    static TMP_Text TMP(GameObject p, string t) { var go = R(p, RH); var tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = FS; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; return tmp; }

    static Slider S(GameObject p, string lbl, float mn, float mx, float v, bool w = false)
    {
        var row = R(p, RH); var hlg = row.AddComponent<HorizontalLayoutGroup>(); hlg.spacing = 3; hlg.childControlWidth = hlg.childControlHeight = true; hlg.childForceExpandHeight = true;
        var lblGo = new GameObject("L"); lblGo.transform.SetParent(row.transform, false); lblGo.AddComponent<RectTransform>(); lblGo.AddComponent<LayoutElement>().preferredWidth = 100;
        var txt = lblGo.AddComponent<Text>(); txt.font = F(); txt.text = lbl; txt.fontSize = FS; txt.color = Color.white; txt.alignment = TextAnchor.MiddleLeft;
        var sld = new GameObject("S"); sld.transform.SetParent(row.transform, false); sld.AddComponent<RectTransform>(); sld.AddComponent<LayoutElement>().flexibleWidth = 1;
        var slider = sld.AddComponent<Slider>(); slider.minValue = mn; slider.maxValue = mx; slider.wholeNumbers = w; slider.value = v;
        var bg = new GameObject("BG"); bg.transform.SetParent(sld.transform, false); var bgRt = bg.AddComponent<RectTransform>(); bgRt.anchorMin = new Vector2(0, 0.4f); bgRt.anchorMax = new Vector2(1, 0.6f); bgRt.offsetMin = bgRt.offsetMax = Vector2.zero; bg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
        var fa = new GameObject("FA"); fa.transform.SetParent(sld.transform, false); var faRt = fa.AddComponent<RectTransform>(); faRt.anchorMin = new Vector2(0, 0.4f); faRt.anchorMax = new Vector2(1, 0.6f); faRt.offsetMin = new Vector2(3, 0); faRt.offsetMax = new Vector2(-3, 0);
        var fill = new GameObject("F"); fill.transform.SetParent(fa.transform, false); var fRt = fill.AddComponent<RectTransform>(); fRt.anchorMin = fRt.offsetMin = fRt.offsetMax = Vector2.zero; fRt.anchorMax = Vector2.one; fill.AddComponent<Image>().color = new Color(0.3f, 0.65f, 1f); slider.fillRect = fRt;
        var ha = new GameObject("HA"); ha.transform.SetParent(sld.transform, false); var haRt = ha.AddComponent<RectTransform>(); haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one; haRt.offsetMin = new Vector2(5, 0); haRt.offsetMax = new Vector2(-5, 0);
        var h = new GameObject("H"); h.transform.SetParent(ha.transform, false); var hRt = h.AddComponent<RectTransform>(); hRt.sizeDelta = new Vector2(12, 0); var hImg = h.AddComponent<Image>(); hImg.color = Color.white; slider.handleRect = hRt; slider.targetGraphic = hImg;
        return slider;
    }

    static Toggle T(GameObject p, string lbl, bool on)
    {
        var row = R(p, RH); var hlg = row.AddComponent<HorizontalLayoutGroup>(); hlg.spacing = 3; hlg.childControlWidth = hlg.childControlHeight = true; hlg.childForceExpandHeight = true;
        var lblGo = new GameObject("L"); lblGo.transform.SetParent(row.transform, false); lblGo.AddComponent<RectTransform>(); lblGo.AddComponent<LayoutElement>().preferredWidth = 100;
        var txt = lblGo.AddComponent<Text>(); txt.font = F(); txt.text = lbl; txt.fontSize = FS; txt.color = Color.white; txt.alignment = TextAnchor.MiddleLeft;
        var tgl = new GameObject("T"); tgl.transform.SetParent(row.transform, false); tgl.AddComponent<RectTransform>(); var le = tgl.AddComponent<LayoutElement>(); le.preferredWidth = le.preferredHeight = 20;
        var toggle = tgl.AddComponent<Toggle>(); toggle.isOn = on;
        var bgGo = new GameObject("BG"); bgGo.transform.SetParent(tgl.transform, false); var bgRt = bgGo.AddComponent<RectTransform>(); bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        var bgImg = bgGo.AddComponent<Image>(); bgImg.color = new Color(0.2f, 0.2f, 0.25f); toggle.targetGraphic = bgImg;
        var chk = new GameObject("X"); chk.transform.SetParent(bgGo.transform, false); var chkRt = chk.AddComponent<RectTransform>(); chkRt.anchorMin = new Vector2(0.2f, 0.2f); chkRt.anchorMax = new Vector2(0.8f, 0.8f); chkRt.offsetMin = chkRt.offsetMax = Vector2.zero;
        var chkImg = chk.AddComponent<Image>(); chkImg.color = new Color(0.3f, 0.65f, 1f); toggle.graphic = chkImg;
        return toggle;
    }

    static Button B(GameObject p, string lbl)
    {
        var go = R(p, RH); go.AddComponent<Image>().color = new Color(0.2f, 0.4f, 0.55f); var btn = go.AddComponent<Button>();
        var txtGo = new GameObject("T"); txtGo.transform.SetParent(go.transform, false); var txtRt = txtGo.AddComponent<RectTransform>(); txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;
        var txt = txtGo.AddComponent<Text>(); txt.font = F(); txt.text = lbl; txt.fontSize = FS; txt.fontStyle = FontStyle.Bold; txt.color = Color.white; txt.alignment = TextAnchor.MiddleCenter;
        return btn;
    }
    
    static Button BSmall(GameObject p, string lbl)
    {
        var go = new GameObject("Btn"); go.transform.SetParent(p.transform, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<LayoutElement>().flexibleWidth = 1;
        go.AddComponent<Image>().color = new Color(0.25f, 0.45f, 0.6f);
        var btn = go.AddComponent<Button>();
        var txtGo = new GameObject("T"); txtGo.transform.SetParent(go.transform, false);
        var txtRt = txtGo.AddComponent<RectTransform>(); 
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;
        var txt = txtGo.AddComponent<Text>(); txt.font = F(); txt.text = lbl; txt.fontSize = FS; txt.fontStyle = FontStyle.Bold; txt.color = Color.white; txt.alignment = TextAnchor.MiddleCenter;
        return btn;
    }
    
    static TMP_Dropdown D(GameObject p, string lbl, List<string> options)
    {
        var row = R(p, RH + 4); var hlg = row.AddComponent<HorizontalLayoutGroup>(); hlg.spacing = 3; hlg.childControlWidth = hlg.childControlHeight = true; hlg.childForceExpandHeight = true;
        
        var ddGo = new GameObject("Dropdown"); ddGo.transform.SetParent(row.transform, false);
        ddGo.AddComponent<RectTransform>();
        ddGo.AddComponent<LayoutElement>().flexibleWidth = 1;
        ddGo.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
        
        var dropdown = ddGo.AddComponent<TMP_Dropdown>();
        
        // Label
        var lblGo = new GameObject("Label"); lblGo.transform.SetParent(ddGo.transform, false);
        var lblRt = lblGo.AddComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = new Vector2(10, 0); lblRt.offsetMax = new Vector2(-25, 0);
        var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
        lblTmp.fontSize = FS; lblTmp.color = Color.white; lblTmp.alignment = TextAlignmentOptions.Left;
        dropdown.captionText = lblTmp;
        
        // Arrow
        var arrowGo = new GameObject("Arrow"); arrowGo.transform.SetParent(ddGo.transform, false);
        var arrowRt = arrowGo.AddComponent<RectTransform>();
        arrowRt.anchorMin = new Vector2(1, 0); arrowRt.anchorMax = Vector2.one;
        arrowRt.sizeDelta = new Vector2(20, 0); arrowRt.anchoredPosition = new Vector2(-10, 0);
        var arrowTxt = arrowGo.AddComponent<TextMeshProUGUI>();
        arrowTxt.text = "▼"; arrowTxt.fontSize = 10; arrowTxt.color = Color.white; arrowTxt.alignment = TextAlignmentOptions.Center;
        
        // Template
        var template = new GameObject("Template"); template.transform.SetParent(ddGo.transform, false);
        var tempRt = template.AddComponent<RectTransform>();
        tempRt.anchorMin = new Vector2(0, 0); tempRt.anchorMax = new Vector2(1, 0);
        tempRt.pivot = new Vector2(0.5f, 1); tempRt.sizeDelta = new Vector2(0, 150);
        template.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
        template.AddComponent<ScrollRect>();
        
        var viewport = new GameObject("Viewport"); viewport.transform.SetParent(template.transform, false);
        var vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one; vpRt.offsetMin = vpRt.offsetMax = Vector2.zero;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.AddComponent<Image>();
        
        var contentGo = new GameObject("Content"); contentGo.transform.SetParent(viewport.transform, false);
        var contRt = contentGo.AddComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0, 1); contRt.anchorMax = Vector2.one;
        contRt.pivot = new Vector2(0.5f, 1); contRt.sizeDelta = new Vector2(0, 28);
        
        var item = new GameObject("Item"); item.transform.SetParent(contentGo.transform, false);
        var itemRt = item.AddComponent<RectTransform>();
        itemRt.anchorMin = new Vector2(0, 0.5f); itemRt.anchorMax = new Vector2(1, 0.5f);
        itemRt.sizeDelta = new Vector2(0, 28); itemRt.anchoredPosition = Vector2.zero;
        var itemTgl = item.AddComponent<Toggle>();
        
        var itemBg = new GameObject("Background"); itemBg.transform.SetParent(item.transform, false);
        var itemBgRt = itemBg.AddComponent<RectTransform>();
        itemBgRt.anchorMin = Vector2.zero; itemBgRt.anchorMax = Vector2.one; itemBgRt.offsetMin = itemBgRt.offsetMax = Vector2.zero;
        itemBg.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f);
        
        var itemLbl = new GameObject("Label"); itemLbl.transform.SetParent(item.transform, false);
        var itemLblRt = itemLbl.AddComponent<RectTransform>();
        itemLblRt.anchorMin = Vector2.zero; itemLblRt.anchorMax = Vector2.one;
        itemLblRt.offsetMin = new Vector2(10, 0); itemLblRt.offsetMax = new Vector2(-10, 0);
        var itemTmp = itemLbl.AddComponent<TextMeshProUGUI>();
        itemTmp.fontSize = FS; itemTmp.color = Color.white; itemTmp.alignment = TextAlignmentOptions.Left;
        
        dropdown.template = tempRt;
        dropdown.itemText = itemTmp;
        itemTgl.targetGraphic = itemBg.GetComponent<Image>();
        
        template.SetActive(false);
        
        dropdown.ClearOptions();
        if (options.Count > 0)
            dropdown.AddOptions(options);
        else
            dropdown.AddOptions(new List<string> { "No presets" });
        
        return dropdown;
    }

    static GameObject R(GameObject p, float h) { var go = new GameObject("R"); go.transform.SetParent(p.transform, false); go.AddComponent<RectTransform>(); var le = go.AddComponent<LayoutElement>(); le.preferredHeight = le.minHeight = h; return go; }
}
#endif
