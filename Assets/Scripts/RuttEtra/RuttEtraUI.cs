using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class RuttEtraUI : MonoBehaviour
{
    [Header("References")]
    public RuttEtraController controller;
    public RuttEtraSettings settings;
    
    [Header("UI Panels")]
    public GameObject controlPanel;
    public Key toggleUIKey = Key.H;
    
    [Header("Displacement Controls")]
    public Slider displacementSlider;
    public Slider smoothingSlider;
    public Toggle invertToggle;
    
    [Header("Visual Controls")]
    public Slider lineWidthSlider;
    public Slider glowSlider;
    public Toggle sourceColorToggle;
    public Slider colorBlendSlider;
    
    [Header("Resolution Controls")]
    public Slider horizontalResSlider;
    public Slider verticalResSlider;
    public TMP_Text resolutionText;
    
    [Header("Scan Line Controls")]
    public Slider scanLineSkipSlider;
    public Toggle horizontalLinesToggle;
    public Toggle verticalLinesToggle;
    
    [Header("Color Pickers")]
    public Slider primaryHueSlider;
    public Slider secondaryHueSlider;
    
    private bool _panelVisible = true;
    
    private void Start()
    {
        InitializeUI();
        BindEvents();
    }
    
    private void InitializeUI()
    {
        if (settings == null) return;
        
        // Set initial values
        if (displacementSlider) displacementSlider.value = settings.displacementStrength;
        if (smoothingSlider) smoothingSlider.value = settings.displacementSmoothing;
        if (invertToggle) invertToggle.isOn = settings.invertDisplacement;
        
        if (lineWidthSlider) lineWidthSlider.value = settings.lineWidth;
        if (glowSlider) glowSlider.value = settings.glowIntensity;
        if (sourceColorToggle) sourceColorToggle.isOn = settings.useSourceColor;
        if (colorBlendSlider) colorBlendSlider.value = settings.colorBlend;
        
        if (horizontalResSlider) horizontalResSlider.value = settings.horizontalResolution;
        if (verticalResSlider) verticalResSlider.value = settings.verticalResolution;
        
        if (scanLineSkipSlider) scanLineSkipSlider.value = settings.scanLineSkip;
        if (horizontalLinesToggle) horizontalLinesToggle.isOn = settings.showHorizontalLines;
        if (verticalLinesToggle) verticalLinesToggle.isOn = settings.showVerticalLines;
        
        UpdateResolutionText();
    }
    
    private void BindEvents()
    {
        // Displacement
        displacementSlider?.onValueChanged.AddListener(v => {
            settings.displacementStrength = v;
        });
        
        smoothingSlider?.onValueChanged.AddListener(v => {
            settings.displacementSmoothing = v;
        });
        
        invertToggle?.onValueChanged.AddListener(v => {
            settings.invertDisplacement = v;
        });
        
        // Visual
        lineWidthSlider?.onValueChanged.AddListener(v => {
            settings.lineWidth = v;
        });
        
        glowSlider?.onValueChanged.AddListener(v => {
            settings.glowIntensity = v;
        });
        
        sourceColorToggle?.onValueChanged.AddListener(v => {
            settings.useSourceColor = v;
        });
        
        colorBlendSlider?.onValueChanged.AddListener(v => {
            settings.colorBlend = v;
        });
        
        // Resolution
        horizontalResSlider?.onValueChanged.AddListener(v => {
            settings.horizontalResolution = Mathf.RoundToInt(v);
            UpdateResolutionText();
            controller?.meshGenerator?.RefreshMesh();
        });
        
        verticalResSlider?.onValueChanged.AddListener(v => {
            settings.verticalResolution = Mathf.RoundToInt(v);
            UpdateResolutionText();
            controller?.meshGenerator?.RefreshMesh();
        });
        
        // Scan lines
        scanLineSkipSlider?.onValueChanged.AddListener(v => {
            settings.scanLineSkip = Mathf.RoundToInt(v);
            controller?.meshGenerator?.RefreshMesh();
        });
        
        horizontalLinesToggle?.onValueChanged.AddListener(v => {
            settings.showHorizontalLines = v;
            controller?.meshGenerator?.RefreshMesh();
        });
        
        verticalLinesToggle?.onValueChanged.AddListener(v => {
            settings.showVerticalLines = v;
            controller?.meshGenerator?.RefreshMesh();
        });
        
        // Colors
        primaryHueSlider?.onValueChanged.AddListener(v => {
            settings.primaryColor = Color.HSVToRGB(v, 1f, 1f);
        });
        
        secondaryHueSlider?.onValueChanged.AddListener(v => {
            settings.secondaryColor = Color.HSVToRGB(v, 1f, 1f);
        });
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[toggleUIKey].wasPressedThisFrame)
        {
            _panelVisible = !_panelVisible;
            controlPanel?.SetActive(_panelVisible);
        }
    }
    
    private void UpdateResolutionText()
    {
        if (resolutionText != null && settings != null)
        {
            resolutionText.text = $"{settings.horizontalResolution} x {settings.verticalResolution}";
        }
    }
}

