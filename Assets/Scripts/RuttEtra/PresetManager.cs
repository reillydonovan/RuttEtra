using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Preset management system for saving, loading, and morphing between parameter states.
/// </summary>
public class PresetManager : MonoBehaviour
{
    [Header("References")]
    public RuttEtraSettings settings;
    
    [Header("Preset Storage")]
    public List<Preset> presets = new List<Preset>();
    public int currentPresetIndex = -1;
    
    [Header("Morphing")]
    public bool isMorphing = false;
    public float morphDuration = 2f;
    public AnimationCurve morphCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Auto-save")]
    public bool autoSaveOnQuit = true;
    public string presetFolder = "Presets";
    
    // Events
    public event Action<Preset> OnPresetLoaded;
    public event Action<Preset> OnPresetSaved;
    public event Action<float> OnMorphProgress;
    
    private Preset _morphFrom;
    private Preset _morphTo;
    private float _morphTime;
    private string _presetPath;
    
    [Serializable]
    public class Preset
    {
        public string name;
        public string description;
        public DateTime createdAt;
        
        // Input signal
        public float brightness;
        public float contrast;
        public float threshold;
        public float gamma;
        public bool edgeDetect;
        public int posterize;
        
        // Displacement
        public float displacementStrength;
        public float displacementSmoothing;
        public float displacementOffset;
        public bool invertDisplacement;
        public float zModulation;
        public float zModFrequency;
        
        // Position
        public float horizontalPosition;
        public float verticalPosition;
        
        // Scale
        public float horizontalScale;
        public float verticalScale;
        public float meshScale;
        
        // Rotation
        public float rotationX;
        public float rotationY;
        public float rotationZ;
        
        // Distortion
        public float keystoneH;
        public float keystoneV;
        public float barrelDistortion;
        
        // Scan lines
        public int scanLineSkip;
        public bool showHorizontalLines;
        public bool showVerticalLines;
        public bool interlace;
        
        // Wave
        public float horizontalWave;
        public float verticalWave;
        public float waveFrequency;
        public float waveSpeed;
        
        // Line style
        public float lineWidth;
        public float lineTaper;
        public float glowIntensity;
        
        // Colors
        public Color primaryColor;
        public Color secondaryColor;
        public float colorBlend;
        public bool useSourceColor;
        
        // Post effects
        public float noiseAmount;
        public float persistence;
        public float scanlineFlicker;
        public float bloom;
        
        public Preset() { }
        
        public Preset(string name)
        {
            this.name = name;
            this.createdAt = DateTime.Now;
        }
    }
    
    private void Start()
    {
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller) settings = controller.settings;
        }
        
        _presetPath = Path.Combine(Application.persistentDataPath, presetFolder);
        if (!Directory.Exists(_presetPath))
            Directory.CreateDirectory(_presetPath);
        
        LoadAllPresets();
        CreateDefaultPresets();
    }
    
    private void Update()
    {
        if (isMorphing)
        {
            _morphTime += Time.deltaTime / morphDuration;
            float t = morphCurve.Evaluate(Mathf.Clamp01(_morphTime));
            
            LerpPreset(_morphFrom, _morphTo, t);
            OnMorphProgress?.Invoke(t);
            
            if (_morphTime >= 1f)
            {
                isMorphing = false;
            }
        }
    }
    
    public Preset CaptureCurrentState(string name = "")
    {
        if (settings == null) return null;
        
        var preset = new Preset(string.IsNullOrEmpty(name) ? $"Preset_{DateTime.Now:HHmmss}" : name);
        
        // Input signal
        preset.brightness = settings.brightness;
        preset.contrast = settings.contrast;
        preset.threshold = settings.threshold;
        preset.gamma = settings.gamma;
        preset.edgeDetect = settings.edgeDetect;
        preset.posterize = settings.posterize;
        
        // Displacement
        preset.displacementStrength = settings.displacementStrength;
        preset.displacementSmoothing = settings.displacementSmoothing;
        preset.displacementOffset = settings.displacementOffset;
        preset.invertDisplacement = settings.invertDisplacement;
        preset.zModulation = settings.zModulation;
        preset.zModFrequency = settings.zModFrequency;
        
        // Position
        preset.horizontalPosition = settings.horizontalPosition;
        preset.verticalPosition = settings.verticalPosition;
        
        // Scale
        preset.horizontalScale = settings.horizontalScale;
        preset.verticalScale = settings.verticalScale;
        preset.meshScale = settings.meshScale;
        
        // Rotation
        preset.rotationX = settings.rotationX;
        preset.rotationY = settings.rotationY;
        preset.rotationZ = settings.rotationZ;
        
        // Distortion
        preset.keystoneH = settings.keystoneH;
        preset.keystoneV = settings.keystoneV;
        preset.barrelDistortion = settings.barrelDistortion;
        
        // Scan lines
        preset.scanLineSkip = settings.scanLineSkip;
        preset.showHorizontalLines = settings.showHorizontalLines;
        preset.showVerticalLines = settings.showVerticalLines;
        preset.interlace = settings.interlace;
        
        // Wave
        preset.horizontalWave = settings.horizontalWave;
        preset.verticalWave = settings.verticalWave;
        preset.waveFrequency = settings.waveFrequency;
        preset.waveSpeed = settings.waveSpeed;
        
        // Line style
        preset.lineWidth = settings.lineWidth;
        preset.lineTaper = settings.lineTaper;
        preset.glowIntensity = settings.glowIntensity;
        
        // Colors
        preset.primaryColor = settings.primaryColor;
        preset.secondaryColor = settings.secondaryColor;
        preset.colorBlend = settings.colorBlend;
        preset.useSourceColor = settings.useSourceColor;
        
        // Post effects
        preset.noiseAmount = settings.noiseAmount;
        preset.persistence = settings.persistence;
        preset.scanlineFlicker = settings.scanlineFlicker;
        preset.bloom = settings.bloom;
        
        return preset;
    }
    
    public void ApplyPreset(Preset preset, bool instant = true)
    {
        if (settings == null || preset == null) return;
        
        if (instant)
        {
            ApplyPresetImmediate(preset);
        }
        else
        {
            MorphToPreset(preset);
        }
        
        OnPresetLoaded?.Invoke(preset);
    }
    
    private void ApplyPresetImmediate(Preset preset)
    {
        // Input signal
        settings.brightness = preset.brightness;
        settings.contrast = preset.contrast;
        settings.threshold = preset.threshold;
        settings.gamma = preset.gamma;
        settings.edgeDetect = preset.edgeDetect;
        settings.posterize = preset.posterize;
        
        // Displacement
        settings.displacementStrength = preset.displacementStrength;
        settings.displacementSmoothing = preset.displacementSmoothing;
        settings.displacementOffset = preset.displacementOffset;
        settings.invertDisplacement = preset.invertDisplacement;
        settings.zModulation = preset.zModulation;
        settings.zModFrequency = preset.zModFrequency;
        
        // Position
        settings.horizontalPosition = preset.horizontalPosition;
        settings.verticalPosition = preset.verticalPosition;
        
        // Scale
        settings.horizontalScale = preset.horizontalScale;
        settings.verticalScale = preset.verticalScale;
        settings.meshScale = preset.meshScale;
        
        // Rotation
        settings.rotationX = preset.rotationX;
        settings.rotationY = preset.rotationY;
        settings.rotationZ = preset.rotationZ;
        
        // Distortion
        settings.keystoneH = preset.keystoneH;
        settings.keystoneV = preset.keystoneV;
        settings.barrelDistortion = preset.barrelDistortion;
        
        // Scan lines
        settings.scanLineSkip = preset.scanLineSkip;
        settings.showHorizontalLines = preset.showHorizontalLines;
        settings.showVerticalLines = preset.showVerticalLines;
        settings.interlace = preset.interlace;
        
        // Wave
        settings.horizontalWave = preset.horizontalWave;
        settings.verticalWave = preset.verticalWave;
        settings.waveFrequency = preset.waveFrequency;
        settings.waveSpeed = preset.waveSpeed;
        
        // Line style
        settings.lineWidth = preset.lineWidth;
        settings.lineTaper = preset.lineTaper;
        settings.glowIntensity = preset.glowIntensity;
        
        // Colors
        settings.primaryColor = preset.primaryColor;
        settings.secondaryColor = preset.secondaryColor;
        settings.colorBlend = preset.colorBlend;
        settings.useSourceColor = preset.useSourceColor;
        
        // Post effects
        settings.noiseAmount = preset.noiseAmount;
        settings.persistence = preset.persistence;
        settings.scanlineFlicker = preset.scanlineFlicker;
        settings.bloom = preset.bloom;
    }
    
    public void MorphToPreset(Preset target)
    {
        _morphFrom = CaptureCurrentState("_temp");
        _morphTo = target;
        _morphTime = 0f;
        isMorphing = true;
    }
    
    public void MorphBetweenPresets(Preset from, Preset to)
    {
        _morphFrom = from;
        _morphTo = to;
        _morphTime = 0f;
        isMorphing = true;
    }
    
    private void LerpPreset(Preset from, Preset to, float t)
    {
        if (settings == null) return;
        
        // Input signal
        settings.brightness = Mathf.Lerp(from.brightness, to.brightness, t);
        settings.contrast = Mathf.Lerp(from.contrast, to.contrast, t);
        settings.threshold = Mathf.Lerp(from.threshold, to.threshold, t);
        settings.gamma = Mathf.Lerp(from.gamma, to.gamma, t);
        
        // Displacement
        settings.displacementStrength = Mathf.Lerp(from.displacementStrength, to.displacementStrength, t);
        settings.displacementSmoothing = Mathf.Lerp(from.displacementSmoothing, to.displacementSmoothing, t);
        settings.displacementOffset = Mathf.Lerp(from.displacementOffset, to.displacementOffset, t);
        settings.zModulation = Mathf.Lerp(from.zModulation, to.zModulation, t);
        settings.zModFrequency = Mathf.Lerp(from.zModFrequency, to.zModFrequency, t);
        
        // Position
        settings.horizontalPosition = Mathf.Lerp(from.horizontalPosition, to.horizontalPosition, t);
        settings.verticalPosition = Mathf.Lerp(from.verticalPosition, to.verticalPosition, t);
        
        // Scale
        settings.horizontalScale = Mathf.Lerp(from.horizontalScale, to.horizontalScale, t);
        settings.verticalScale = Mathf.Lerp(from.verticalScale, to.verticalScale, t);
        settings.meshScale = Mathf.Lerp(from.meshScale, to.meshScale, t);
        
        // Rotation
        settings.rotationX = Mathf.Lerp(from.rotationX, to.rotationX, t);
        settings.rotationY = Mathf.Lerp(from.rotationY, to.rotationY, t);
        settings.rotationZ = Mathf.Lerp(from.rotationZ, to.rotationZ, t);
        
        // Distortion
        settings.keystoneH = Mathf.Lerp(from.keystoneH, to.keystoneH, t);
        settings.keystoneV = Mathf.Lerp(from.keystoneV, to.keystoneV, t);
        settings.barrelDistortion = Mathf.Lerp(from.barrelDistortion, to.barrelDistortion, t);
        
        // Wave
        settings.horizontalWave = Mathf.Lerp(from.horizontalWave, to.horizontalWave, t);
        settings.verticalWave = Mathf.Lerp(from.verticalWave, to.verticalWave, t);
        settings.waveFrequency = Mathf.Lerp(from.waveFrequency, to.waveFrequency, t);
        settings.waveSpeed = Mathf.Lerp(from.waveSpeed, to.waveSpeed, t);
        
        // Line style
        settings.lineWidth = Mathf.Lerp(from.lineWidth, to.lineWidth, t);
        settings.lineTaper = Mathf.Lerp(from.lineTaper, to.lineTaper, t);
        settings.glowIntensity = Mathf.Lerp(from.glowIntensity, to.glowIntensity, t);
        
        // Colors
        settings.primaryColor = Color.Lerp(from.primaryColor, to.primaryColor, t);
        settings.secondaryColor = Color.Lerp(from.secondaryColor, to.secondaryColor, t);
        settings.colorBlend = Mathf.Lerp(from.colorBlend, to.colorBlend, t);
        
        // Post effects
        settings.noiseAmount = Mathf.Lerp(from.noiseAmount, to.noiseAmount, t);
        settings.persistence = Mathf.Lerp(from.persistence, to.persistence, t);
        settings.scanlineFlicker = Mathf.Lerp(from.scanlineFlicker, to.scanlineFlicker, t);
        settings.bloom = Mathf.Lerp(from.bloom, to.bloom, t);
        
        // Boolean values switch at 0.5
        if (t >= 0.5f)
        {
            settings.edgeDetect = to.edgeDetect;
            settings.invertDisplacement = to.invertDisplacement;
            settings.showHorizontalLines = to.showHorizontalLines;
            settings.showVerticalLines = to.showVerticalLines;
            settings.interlace = to.interlace;
            settings.useSourceColor = to.useSourceColor;
            settings.posterize = to.posterize;
            settings.scanLineSkip = to.scanLineSkip;
        }
    }
    
    public void SavePreset(Preset preset)
    {
        if (!presets.Contains(preset))
            presets.Add(preset);
        
        SavePresetToFile(preset);
        OnPresetSaved?.Invoke(preset);
    }
    
    public void SaveCurrentAsPreset(string name)
    {
        var preset = CaptureCurrentState(name);
        SavePreset(preset);
    }
    
    private void SavePresetToFile(Preset preset)
    {
        string filename = SanitizeFilename(preset.name) + ".json";
        string path = Path.Combine(_presetPath, filename);
        string json = JsonUtility.ToJson(preset, true);
        File.WriteAllText(path, json);
        Debug.Log($"Preset saved: {path}");
    }
    
    public void LoadAllPresets()
    {
        if (!Directory.Exists(_presetPath)) return;
        
        presets.Clear();
        string[] files = Directory.GetFiles(_presetPath, "*.json");
        
        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                Preset preset = JsonUtility.FromJson<Preset>(json);
                presets.Add(preset);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load preset {file}: {e.Message}");
            }
        }
        
        Debug.Log($"Loaded {presets.Count} presets");
    }
    
    public void DeletePreset(Preset preset)
    {
        presets.Remove(preset);
        string filename = SanitizeFilename(preset.name) + ".json";
        string path = Path.Combine(_presetPath, filename);
        if (File.Exists(path))
            File.Delete(path);
    }
    
    public void LoadPresetByIndex(int index, bool morph = false)
    {
        if (index >= 0 && index < presets.Count)
        {
            currentPresetIndex = index;
            ApplyPreset(presets[index], !morph);
        }
    }
    
    public void LoadPresetByName(string name, bool morph = false)
    {
        var preset = presets.Find(p => p.name == name);
        if (preset != null)
        {
            currentPresetIndex = presets.IndexOf(preset);
            ApplyPreset(preset, !morph);
        }
    }
    
    public void NextPreset(bool morph = false)
    {
        if (presets.Count == 0) return;
        currentPresetIndex = (currentPresetIndex + 1) % presets.Count;
        ApplyPreset(presets[currentPresetIndex], !morph);
    }
    
    public void PreviousPreset(bool morph = false)
    {
        if (presets.Count == 0) return;
        currentPresetIndex = (currentPresetIndex - 1 + presets.Count) % presets.Count;
        ApplyPreset(presets[currentPresetIndex], !morph);
    }
    
    public void RandomPreset(bool morph = false)
    {
        if (presets.Count == 0) return;
        currentPresetIndex = UnityEngine.Random.Range(0, presets.Count);
        ApplyPreset(presets[currentPresetIndex], !morph);
    }
    
    private void CreateDefaultPresets()
    {
        if (presets.Count > 0) return;
        
        // Create some default presets
        var classic = new Preset("Classic Green")
        {
            brightness = 0, contrast = 1, threshold = 0, gamma = 1,
            displacementStrength = 1, displacementOffset = 0,
            horizontalWave = 0, verticalWave = 0,
            primaryColor = Color.green, secondaryColor = Color.cyan,
            showHorizontalLines = true, showVerticalLines = false,
            lineWidth = 0.01f, glowIntensity = 0.5f
        };
        
        var neon = new Preset("Neon Pink")
        {
            brightness = 0.2f, contrast = 1.5f, threshold = 0.1f, gamma = 0.8f,
            displacementStrength = 2f, displacementOffset = 0,
            horizontalWave = 0.3f, verticalWave = 0.2f, waveFrequency = 3,
            primaryColor = new Color(1, 0, 0.5f), secondaryColor = Color.yellow,
            showHorizontalLines = true, showVerticalLines = true,
            lineWidth = 0.015f, glowIntensity = 1.5f, bloom = 0.3f
        };
        
        var minimal = new Preset("Minimal White")
        {
            brightness = 0, contrast = 2, threshold = 0.3f, gamma = 1,
            displacementStrength = 0.5f, displacementOffset = 0,
            horizontalWave = 0, verticalWave = 0,
            primaryColor = Color.white, secondaryColor = Color.gray,
            showHorizontalLines = true, showVerticalLines = false,
            lineWidth = 0.005f, glowIntensity = 0.2f, scanLineSkip = 2
        };
        
        var psychedelic = new Preset("Psychedelic")
        {
            brightness = 0, contrast = 1.2f, threshold = 0, gamma = 1,
            displacementStrength = 3f, displacementOffset = 0,
            horizontalWave = 1f, verticalWave = 0.8f, waveFrequency = 5, waveSpeed = 2,
            primaryColor = Color.red, secondaryColor = Color.blue, colorBlend = 1,
            showHorizontalLines = true, showVerticalLines = true,
            lineWidth = 0.02f, glowIntensity = 1f,
            keystoneH = 0.2f, barrelDistortion = 0.1f
        };
        
        var crt = new Preset("Retro CRT")
        {
            brightness = -0.1f, contrast = 1.3f, threshold = 0, gamma = 1.2f,
            displacementStrength = 1.5f, displacementOffset = 0,
            horizontalWave = 0.05f, verticalWave = 0,
            primaryColor = new Color(0.3f, 1, 0.3f), secondaryColor = new Color(0, 0.5f, 0),
            showHorizontalLines = true, showVerticalLines = false,
            lineWidth = 0.012f, glowIntensity = 0.8f,
            noiseAmount = 0.05f, scanlineFlicker = 0.1f
        };
        
        presets.AddRange(new[] { classic, neon, minimal, psychedelic, crt });
        
        foreach (var preset in presets)
            SavePresetToFile(preset);
    }
    
    private string SanitizeFilename(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
    
    private void OnApplicationQuit()
    {
        if (autoSaveOnQuit && currentPresetIndex >= 0)
        {
            PlayerPrefs.SetInt("RuttEtra_LastPreset", currentPresetIndex);
            PlayerPrefs.Save();
        }
    }
}
