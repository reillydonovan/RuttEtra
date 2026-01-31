#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class RuttEtraUICreator
{
    const float PW = 340f, RH = 26f;
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

        // FEEDBACK
        H(content, "FEEDBACK");
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
        H(content, "ANIMATION");
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

        // ACTIONS
        H(content, "ACTIONS");
        ui.resetCameraButton = B(content, "Reset Camera");
        ui.resetAllButton = B(content, "Reset All");
        L(content, "Press H to hide");

        Undo.RegisterCreatedObjectUndo(canvas, "Create UI");
        Selection.activeGameObject = uiGo;
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

    static void H(GameObject p, string t) { var go = R(p, RH + 2); var txt = go.AddComponent<Text>(); txt.font = F(); txt.text = t; txt.fontSize = FS; txt.fontStyle = FontStyle.Bold; txt.color = new Color(0.5f, 0.8f, 1f); txt.alignment = TextAnchor.LowerLeft; }
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

    static GameObject R(GameObject p, float h) { var go = new GameObject("R"); go.transform.SetParent(p.transform, false); go.AddComponent<RectTransform>(); var le = go.AddComponent<LayoutElement>(); le.preferredHeight = le.minHeight = h; return go; }
}
#endif
