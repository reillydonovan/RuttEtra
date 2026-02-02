using UnityEngine;

/// <summary>
/// Depth-based coloring for Rutt/Etra. Colors vertices based on their
/// Z displacement value, creating gradient effects.
/// </summary>
public class DepthColorizer : MonoBehaviour
{
    [Header("Enable")]
    public bool enableDepthColor = false;
    
    [Header("Color Mapping")]
    public ColorMode colorMode = ColorMode.Gradient;
    public Gradient depthGradient;
    
    [Header("Two-Color Mode")]
    public Color nearColor = Color.cyan;
    public Color farColor = Color.magenta;
    
    [Header("Rainbow Mode")]
    [Range(0.5f, 5f)] public float rainbowCycles = 1f;
    [Range(0f, 1f)] public float rainbowSaturation = 1f;
    [Range(0f, 1f)] public float rainbowValue = 1f;
    public bool animateRainbow = false;
    [Range(0f, 2f)] public float rainbowSpeed = 0.5f;
    
    [Header("Thermal Mode")]
    public bool thermalMode = false;
    
    [Header("Depth Range")]
    public bool autoRange = true;
    [Range(-5f, 0f)] public float minDepth = -2f;
    [Range(0f, 5f)] public float maxDepth = 2f;
    
    [Header("Audio Reactive")]
    public bool audioReactiveRange = false;
    public AudioReactive audioReactive;
    [Range(0f, 2f)] public float audioRangeMultiplier = 1f;
    
    public enum ColorMode
    {
        Gradient,
        TwoColor,
        Rainbow,
        Thermal,
        Bands
    }
    
    // Tracked depth range
    private float _trackedMinDepth = float.MaxValue;
    private float _trackedMaxDepth = float.MinValue;
    private float _rainbowOffset;
    
    private void Awake()
    {
        // Initialize default gradient if not set
        if (depthGradient == null || depthGradient.colorKeys.Length == 0)
        {
            depthGradient = new Gradient();
            depthGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.blue, 0f),
                    new GradientColorKey(Color.cyan, 0.25f),
                    new GradientColorKey(Color.green, 0.5f),
                    new GradientColorKey(Color.yellow, 0.75f),
                    new GradientColorKey(Color.red, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
        }
    }
    
    private void Start()
    {
        if (audioReactive == null)
        {
            audioReactive = FindFirstObjectByType<AudioReactive>();
        }
    }
    
    private void Update()
    {
        if (animateRainbow)
        {
            _rainbowOffset += rainbowSpeed * Time.deltaTime;
            if (_rainbowOffset > 1f) _rainbowOffset -= 1f;
        }
        
        // Audio reactive depth range
        if (audioReactiveRange && audioReactive != null && audioReactive.enableAudio)
        {
            float audioMod = audioReactive.bass * audioRangeMultiplier;
            // This would need to be applied in the color calculation
        }
    }
    
    /// <summary>
    /// Get color for a given depth value.
    /// Call this from the mesh generator when setting vertex colors.
    /// </summary>
    public Color GetColorForDepth(float depth)
    {
        if (!enableDepthColor)
            return Color.white;
        
        // Track depth range if auto
        if (autoRange)
        {
            if (depth < _trackedMinDepth) _trackedMinDepth = depth;
            if (depth > _trackedMaxDepth) _trackedMaxDepth = depth;
        }
        
        // Normalize depth to 0-1
        float min = autoRange ? _trackedMinDepth : minDepth;
        float max = autoRange ? _trackedMaxDepth : maxDepth;
        
        // Prevent division by zero
        if (Mathf.Approximately(max, min)) max = min + 0.001f;
        
        float t = Mathf.InverseLerp(min, max, depth);
        
        return colorMode switch
        {
            ColorMode.Gradient => depthGradient.Evaluate(t),
            ColorMode.TwoColor => Color.Lerp(nearColor, farColor, t),
            ColorMode.Rainbow => GetRainbowColor(t),
            ColorMode.Thermal => GetThermalColor(t),
            ColorMode.Bands => GetBandColor(t),
            _ => Color.white
        };
    }
    
    /// <summary>
    /// Apply depth colors to vertex color array
    /// </summary>
    public void ApplyDepthColors(Vector3[] vertices, Color[] colors, float displacementScale = 1f)
    {
        if (!enableDepthColor || vertices == null || colors == null) return;
        if (vertices.Length != colors.Length) return;
        
        // Reset tracking for this frame
        if (autoRange)
        {
            _trackedMinDepth = float.MaxValue;
            _trackedMaxDepth = float.MinValue;
            
            // First pass to determine range
            for (int i = 0; i < vertices.Length; i++)
            {
                float depth = vertices[i].z * displacementScale;
                if (depth < _trackedMinDepth) _trackedMinDepth = depth;
                if (depth > _trackedMaxDepth) _trackedMaxDepth = depth;
            }
        }
        
        // Second pass to apply colors
        for (int i = 0; i < vertices.Length; i++)
        {
            float depth = vertices[i].z * displacementScale;
            colors[i] = GetColorForDepth(depth);
        }
    }
    
    private Color GetRainbowColor(float t)
    {
        float hue = (t * rainbowCycles + _rainbowOffset) % 1f;
        return Color.HSVToRGB(hue, rainbowSaturation, rainbowValue);
    }
    
    private Color GetThermalColor(float t)
    {
        // Black -> Blue -> Purple -> Red -> Orange -> Yellow -> White
        if (t < 0.2f)
            return Color.Lerp(Color.black, Color.blue, t / 0.2f);
        else if (t < 0.4f)
            return Color.Lerp(Color.blue, new Color(0.5f, 0, 0.5f), (t - 0.2f) / 0.2f);
        else if (t < 0.6f)
            return Color.Lerp(new Color(0.5f, 0, 0.5f), Color.red, (t - 0.4f) / 0.2f);
        else if (t < 0.8f)
            return Color.Lerp(Color.red, new Color(1f, 0.5f, 0f), (t - 0.6f) / 0.2f);
        else
            return Color.Lerp(new Color(1f, 0.5f, 0f), Color.yellow, (t - 0.8f) / 0.2f);
    }
    
    private Color GetBandColor(float t)
    {
        // Discrete color bands
        int band = Mathf.FloorToInt(t * 5f);
        return band switch
        {
            0 => Color.blue,
            1 => Color.cyan,
            2 => Color.green,
            3 => Color.yellow,
            4 => Color.red,
            _ => Color.red
        };
    }
    
    /// <summary>
    /// Reset tracked depth range
    /// </summary>
    public void ResetDepthRange()
    {
        _trackedMinDepth = float.MaxValue;
        _trackedMaxDepth = float.MinValue;
    }
    
    /// <summary>
    /// Set a preset gradient
    /// </summary>
    public void SetPreset(string preset)
    {
        switch (preset.ToLower())
        {
            case "thermal":
                colorMode = ColorMode.Thermal;
                break;
                
            case "rainbow":
                colorMode = ColorMode.Rainbow;
                rainbowCycles = 1f;
                break;
                
            case "ocean":
                colorMode = ColorMode.TwoColor;
                nearColor = new Color(0f, 0.2f, 0.4f);
                farColor = new Color(0f, 0.8f, 1f);
                break;
                
            case "fire":
                colorMode = ColorMode.TwoColor;
                nearColor = new Color(0.3f, 0f, 0f);
                farColor = Color.yellow;
                break;
                
            case "matrix":
                colorMode = ColorMode.TwoColor;
                nearColor = new Color(0f, 0.2f, 0f);
                farColor = new Color(0f, 1f, 0.2f);
                break;
        }
    }
}
