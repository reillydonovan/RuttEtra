using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Color palette system for Rutt/Etra. Provides preset color themes
/// and smooth transitions between palettes.
/// </summary>
public class ColorPaletteSystem : MonoBehaviour
{
    [Header("Palette Settings")]
    public bool enablePalette = true;
    public int currentPaletteIndex = 0;
    public bool autoTransition = false;
    [Range(5f, 60f)] public float transitionInterval = 15f;
    [Range(0.5f, 5f)] public float transitionDuration = 2f;
    
    [Header("Depth Coloring")]
    public bool enableDepthColor = false;
    public bool useGradient = true;
    
    [Header("References")]
    public RuttEtraSettings settings;
    
    // Events
    public event Action<int> OnPaletteChanged;
    public event Action<string> OnPaletteNameChanged;
    
    // Built-in palettes
    private List<ColorPalette> _palettes;
    private float _transitionTimer;
    private float _autoTimer;
    private ColorPalette _fromPalette;
    private ColorPalette _toPalette;
    private bool _isTransitioning;
    
    [System.Serializable]
    public class ColorPalette
    {
        public string name;
        public Color primaryColor;
        public Color secondaryColor;
        public Color backgroundColor;
        public Color glowColor;
        public float glowIntensity;
        public float saturation;
        
        public ColorPalette(string name, Color primary, Color secondary, Color bg, Color glow, float glowInt = 0.5f, float sat = 1f)
        {
            this.name = name;
            primaryColor = primary;
            secondaryColor = secondary;
            backgroundColor = bg;
            glowColor = glow;
            glowIntensity = glowInt;
            saturation = sat;
        }
    }
    
    private void Awake()
    {
        InitializePalettes();
    }
    
    private void Start()
    {
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller != null) settings = controller.settings;
        }
        
        if (enablePalette && _palettes.Count > 0)
        {
            ApplyPalette(currentPaletteIndex, instant: true);
        }
    }
    
    private void InitializePalettes()
    {
        _palettes = new List<ColorPalette>
        {
            // Classic CRT Green
            new ColorPalette(
                "CRT Green",
                new Color(0.2f, 1f, 0.3f),      // Bright green
                new Color(0f, 0.5f, 0.1f),      // Dark green
                Color.black,
                new Color(0.3f, 1f, 0.4f),
                0.6f, 1f
            ),
            
            // Vaporwave
            new ColorPalette(
                "Vaporwave",
                new Color(1f, 0.4f, 0.8f),      // Hot pink
                new Color(0.4f, 0.8f, 1f),      // Cyan
                new Color(0.1f, 0f, 0.2f),      // Deep purple
                new Color(1f, 0.5f, 0.9f),
                0.8f, 1.2f
            ),
            
            // Cyberpunk
            new ColorPalette(
                "Cyberpunk",
                new Color(1f, 0.9f, 0f),        // Yellow
                new Color(1f, 0f, 0.5f),        // Magenta
                new Color(0.05f, 0f, 0.1f),
                new Color(1f, 0.8f, 0f),
                1f, 1.1f
            ),
            
            // Synthwave Sunset
            new ColorPalette(
                "Synthwave",
                new Color(1f, 0.3f, 0.1f),      // Orange
                new Color(0.8f, 0f, 0.6f),      // Purple
                new Color(0.1f, 0f, 0.15f),
                new Color(1f, 0.4f, 0.2f),
                0.9f, 1f
            ),
            
            // Ice Blue
            new ColorPalette(
                "Ice Blue",
                new Color(0.5f, 0.9f, 1f),      // Light cyan
                new Color(0.2f, 0.4f, 0.8f),    // Deep blue
                new Color(0f, 0.02f, 0.05f),
                new Color(0.6f, 0.95f, 1f),
                0.7f, 0.9f
            ),
            
            // Fire
            new ColorPalette(
                "Fire",
                new Color(1f, 0.6f, 0f),        // Orange
                new Color(1f, 0.1f, 0f),        // Red
                Color.black,
                new Color(1f, 0.7f, 0.2f),
                1.2f, 1f
            ),
            
            // Matrix
            new ColorPalette(
                "Matrix",
                new Color(0f, 1f, 0.2f),        // Bright green
                new Color(0f, 0.3f, 0f),        // Dark green
                Color.black,
                new Color(0.2f, 1f, 0.3f),
                0.5f, 0.8f
            ),
            
            // Monochrome
            new ColorPalette(
                "Monochrome",
                Color.white,
                new Color(0.5f, 0.5f, 0.5f),
                Color.black,
                Color.white,
                0.3f, 0f
            ),
            
            // Neon Pink
            new ColorPalette(
                "Neon Pink",
                new Color(1f, 0f, 0.6f),        // Hot pink
                new Color(0.6f, 0f, 1f),        // Purple
                new Color(0.05f, 0f, 0.05f),
                new Color(1f, 0.2f, 0.7f),
                1f, 1.2f
            ),
            
            // Retro Amber
            new ColorPalette(
                "Retro Amber",
                new Color(1f, 0.7f, 0f),        // Amber
                new Color(0.6f, 0.3f, 0f),      // Brown
                Color.black,
                new Color(1f, 0.8f, 0.2f),
                0.5f, 0.9f
            ),
            
            // Ocean
            new ColorPalette(
                "Ocean",
                new Color(0f, 0.8f, 0.9f),      // Turquoise
                new Color(0f, 0.3f, 0.6f),      // Deep blue
                new Color(0f, 0.05f, 0.1f),
                new Color(0.2f, 0.9f, 1f),
                0.6f, 1f
            ),
            
            // Toxic
            new ColorPalette(
                "Toxic",
                new Color(0.5f, 1f, 0f),        // Lime
                new Color(0f, 0.6f, 0f),        // Green
                Color.black,
                new Color(0.6f, 1f, 0.2f),
                0.8f, 1.1f
            ),
            
            // Plasma
            new ColorPalette(
                "Plasma",
                new Color(0.8f, 0.2f, 1f),      // Purple
                new Color(0.2f, 0.8f, 1f),      // Cyan
                new Color(0.05f, 0f, 0.1f),
                new Color(0.9f, 0.3f, 1f),
                1f, 1.2f
            ),
            
            // Sunset
            new ColorPalette(
                "Sunset",
                new Color(1f, 0.5f, 0.2f),      // Orange
                new Color(1f, 0.2f, 0.4f),      // Red-pink
                new Color(0.1f, 0.02f, 0.05f),
                new Color(1f, 0.6f, 0.3f),
                0.7f, 1f
            ),
            
            // Arctic
            new ColorPalette(
                "Arctic",
                new Color(0.9f, 0.95f, 1f),     // White-blue
                new Color(0.4f, 0.6f, 0.9f),    // Light blue
                new Color(0.02f, 0.03f, 0.05f),
                new Color(0.8f, 0.9f, 1f),
                0.4f, 0.7f
            )
        };
    }
    
    private void Update()
    {
        if (!enablePalette || settings == null) return;
        
        // Handle auto transition
        if (autoTransition)
        {
            _autoTimer += Time.deltaTime;
            if (_autoTimer >= transitionInterval)
            {
                _autoTimer = 0f;
                NextPalette();
            }
        }
        
        // Handle transition animation
        if (_isTransitioning)
        {
            _transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_transitionTimer / transitionDuration);
            t = Mathf.SmoothStep(0f, 1f, t); // Ease in/out
            
            LerpPalette(_fromPalette, _toPalette, t);
            
            if (t >= 1f)
            {
                _isTransitioning = false;
            }
        }
    }
    
    /// <summary>
    /// Apply a palette by index
    /// </summary>
    public void ApplyPalette(int index, bool instant = false)
    {
        if (index < 0 || index >= _palettes.Count) 
        {
            Debug.LogWarning($"[ColorPalette] Invalid palette index: {index}, palettes count: {_palettes?.Count ?? 0}");
            return;
        }
        
        currentPaletteIndex = index;
        Debug.Log($"[ColorPalette] Applying palette '{_palettes[index].name}' (index {index}), instant={instant}");
        
        if (instant || transitionDuration <= 0)
        {
            ApplyPaletteImmediate(_palettes[index]);
        }
        else
        {
            StartTransition(_palettes[index]);
        }
        
        OnPaletteChanged?.Invoke(index);
        OnPaletteNameChanged?.Invoke(_palettes[index].name);
    }
    
    /// <summary>
    /// Apply palette by name
    /// </summary>
    public void ApplyPalette(string name, bool instant = false)
    {
        for (int i = 0; i < _palettes.Count; i++)
        {
            if (_palettes[i].name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                ApplyPalette(i, instant);
                return;
            }
        }
    }
    
    /// <summary>
    /// Apply palette by index (alias for dropdown binding)
    /// </summary>
    public void ApplyPaletteByIndex(int index)
    {
        ApplyPalette(index, false);
    }
    
    /// <summary>
    /// Go to next palette
    /// </summary>
    public void NextPalette()
    {
        int next = (currentPaletteIndex + 1) % _palettes.Count;
        ApplyPalette(next);
    }
    
    /// <summary>
    /// Go to previous palette
    /// </summary>
    public void PreviousPalette()
    {
        int prev = currentPaletteIndex - 1;
        if (prev < 0) prev = _palettes.Count - 1;
        ApplyPalette(prev);
    }
    
    /// <summary>
    /// Apply a random palette
    /// </summary>
    public void RandomPalette()
    {
        int random = UnityEngine.Random.Range(0, _palettes.Count);
        ApplyPalette(random);
    }
    
    private void StartTransition(ColorPalette target)
    {
        _fromPalette = GetCurrentPaletteFromSettings();
        _toPalette = target;
        _transitionTimer = 0f;
        _isTransitioning = true;
    }
    
    private ColorPalette GetCurrentPaletteFromSettings()
    {
        return new ColorPalette(
            "Current",
            settings.primaryColor,
            settings.secondaryColor,
            Camera.main != null ? Camera.main.backgroundColor : Color.black,
            settings.primaryColor,
            settings.glowIntensity,
            1f
        );
    }
    
    private void ApplyPaletteImmediate(ColorPalette palette)
    {
        settings.primaryColor = palette.primaryColor;
        settings.secondaryColor = palette.secondaryColor;
        settings.glowIntensity = palette.glowIntensity;
        
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = palette.backgroundColor;
        }
    }
    
    private void LerpPalette(ColorPalette from, ColorPalette to, float t)
    {
        settings.primaryColor = Color.Lerp(from.primaryColor, to.primaryColor, t);
        settings.secondaryColor = Color.Lerp(from.secondaryColor, to.secondaryColor, t);
        settings.glowIntensity = Mathf.Lerp(from.glowIntensity, to.glowIntensity, t);
        
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = Color.Lerp(from.backgroundColor, to.backgroundColor, t);
        }
    }
    
    /// <summary>
    /// Get list of palette names
    /// </summary>
    public List<string> GetPaletteNames()
    {
        var names = new List<string>();
        if (_palettes != null)
        {
            foreach (var p in _palettes)
                names.Add(p.name);
        }
        return names;
    }
    
    /// <summary>
    /// Get current palette name
    /// </summary>
    public string CurrentPaletteName => _palettes != null && currentPaletteIndex < _palettes.Count 
        ? _palettes[currentPaletteIndex].name 
        : "None";
    
    /// <summary>
    /// Get total number of palettes
    /// </summary>
    public int PaletteCount => _palettes?.Count ?? 0;
    
    /// <summary>
    /// Check if currently transitioning
    /// </summary>
    public bool IsTransitioning => _isTransitioning;
}
