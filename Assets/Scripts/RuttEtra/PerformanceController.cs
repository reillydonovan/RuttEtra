using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Comprehensive performance controller with keyboard shortcuts, fullscreen toggle,
/// and quick access to common features for live performance.
/// Uses the new Input System.
/// </summary>
public class PerformanceController : MonoBehaviour
{
    [Header("References")]
    public RuttEtraSettings settings;
    public RuttEtraUI ui;
    public PresetManager presetManager;
    public AudioReactive audioReactive;
    public VideoRecorder videoRecorder;
    public AnalogEffects analogEffects;
    public FeedbackEffect feedbackEffect;
    
    [Header("Settings")]
    public bool startFullscreen = false;
    public bool morphPresets = true;
    public bool numberKeysForPresets = true;
    public float adjustmentSpeed = 0.5f;
    
    // Events
    public event Action OnFullscreenToggled;
    public event Action OnUIToggled;
    
    private bool _isFullscreen;
    private bool _uiVisible = true;
    private Keyboard _keyboard;
    
    private void Start()
    {
        _keyboard = Keyboard.current;
        
        // Auto-find references
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller) settings = controller.settings;
        }
        
        if (ui == null) ui = FindFirstObjectByType<RuttEtraUI>();
        if (presetManager == null) presetManager = FindFirstObjectByType<PresetManager>();
        if (audioReactive == null) audioReactive = FindFirstObjectByType<AudioReactive>();
        if (videoRecorder == null) videoRecorder = FindFirstObjectByType<VideoRecorder>();
        if (analogEffects == null) analogEffects = FindFirstObjectByType<AnalogEffects>();
        if (feedbackEffect == null) feedbackEffect = FindFirstObjectByType<FeedbackEffect>();
        
        if (startFullscreen)
            SetFullscreen(true);
        
        Debug.Log("Performance Controller initialized. Press F1 for help.");
    }
    
    private void Update()
    {
        if (_keyboard == null) _keyboard = Keyboard.current;
        if (_keyboard == null) return;
        
        // Help
        if (_keyboard[Key.F1].wasPressedThisFrame)
            ShowHelp();
        
        // Display
        if (_keyboard[Key.F11].wasPressedThisFrame)
            ToggleFullscreen();
        
        // Note: H key is handled by RuttEtraUI
        
        // Presets
        if (_keyboard[Key.RightBracket].wasPressedThisFrame)
            presetManager?.NextPreset(morphPresets);
        
        if (_keyboard[Key.LeftBracket].wasPressedThisFrame)
            presetManager?.PreviousPreset(morphPresets);
        
        if (_keyboard[Key.Backslash].wasPressedThisFrame)
            presetManager?.RandomPreset(morphPresets);
        
        if (_keyboard[Key.P].wasPressedThisFrame)
            SaveCurrentPreset();
        
        // Number keys for presets (1-9)
        if (numberKeysForPresets)
        {
            for (int i = 1; i <= 9; i++)
            {
                Key key = (Key)((int)Key.Digit1 + i - 1);
                if (_keyboard[key].wasPressedThisFrame)
                {
                    presetManager?.LoadPresetByIndex(i - 1, morphPresets);
                }
            }
        }
        
        // Recording
        if (_keyboard[Key.R].wasPressedThisFrame && _keyboard[Key.LeftCtrl].isPressed)
            videoRecorder?.ToggleRecording();
        
        if (_keyboard[Key.F12].wasPressedThisFrame)
            videoRecorder?.TakeScreenshot();
        
        // Quick toggles (only if settings exists)
        if (settings != null)
        {
            if (_keyboard[Key.I].wasPressedThisFrame)
                settings.invertDisplacement = !settings.invertDisplacement;
            
            if (_keyboard[Key.C].wasPressedThisFrame)
                settings.useSourceColor = !settings.useSourceColor;
            
            if (_keyboard[Key.V].wasPressedThisFrame)
                settings.showVerticalLines = !settings.showVerticalLines;
            
            if (_keyboard[Key.L].wasPressedThisFrame)
                settings.interlace = !settings.interlace;
            
            if (_keyboard[Key.E].wasPressedThisFrame)
                settings.edgeDetect = !settings.edgeDetect;
        }
        
        if (_keyboard[Key.A].wasPressedThisFrame && audioReactive != null)
            audioReactive.SetEnabled(!audioReactive.enableAudio);
        
        if (_keyboard[Key.F].wasPressedThisFrame && feedbackEffect != null)
            feedbackEffect.enableFeedback = !feedbackEffect.enableFeedback;
        
        if (_keyboard[Key.T].wasPressedThisFrame && analogEffects != null)
            analogEffects.enableCRT = !analogEffects.enableCRT;
        
        if (_keyboard[Key.Y].wasPressedThisFrame && analogEffects != null)
            analogEffects.enableVHS = !analogEffects.enableVHS;
        
        // Value adjustments (hold to adjust)
        if (settings != null)
        {
            if (_keyboard[Key.Equals].isPressed)
                settings.displacementStrength += adjustmentSpeed * Time.deltaTime;
            
            if (_keyboard[Key.Minus].isPressed)
                settings.displacementStrength = Mathf.Max(0, settings.displacementStrength - adjustmentSpeed * Time.deltaTime);
            
            if (_keyboard[Key.Period].isPressed)
                settings.glowIntensity += adjustmentSpeed * Time.deltaTime;
            
            if (_keyboard[Key.Comma].isPressed)
                settings.glowIntensity = Mathf.Max(0, settings.glowIntensity - adjustmentSpeed * Time.deltaTime);
            
            // Arrow keys for rotation when holding shift
            if (_keyboard[Key.LeftShift].isPressed)
            {
                if (_keyboard[Key.LeftArrow].isPressed)
                    settings.rotationY -= 30 * Time.deltaTime;
                if (_keyboard[Key.RightArrow].isPressed)
                    settings.rotationY += 30 * Time.deltaTime;
                if (_keyboard[Key.UpArrow].isPressed)
                    settings.rotationX -= 30 * Time.deltaTime;
                if (_keyboard[Key.DownArrow].isPressed)
                    settings.rotationX += 30 * Time.deltaTime;
            }
        }
        
        // Reset
        if (_keyboard[Key.Backspace].wasPressedThisFrame)
            ResetToDefaults();
    }
    
    public void ToggleFullscreen()
    {
        SetFullscreen(!_isFullscreen);
    }
    
    public void SetFullscreen(bool fullscreen)
    {
        _isFullscreen = fullscreen;
        
        if (fullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
        }
        
        OnFullscreenToggled?.Invoke();
    }
    
    public void ToggleUI()
    {
        _uiVisible = !_uiVisible;
        
        if (ui != null && ui.controlPanel != null)
            ui.controlPanel.SetActive(_uiVisible);
        
        // Also hide cursor in fullscreen without UI
        Cursor.visible = _uiVisible || !_isFullscreen;
        
        OnUIToggled?.Invoke();
    }
    
    private void SaveCurrentPreset()
    {
        if (presetManager == null) return;
        
        string name = $"Preset_{DateTime.Now:HHmmss}";
        presetManager.SaveCurrentAsPreset(name);
        Debug.Log($"Saved preset: {name}");
    }
    
    private void ResetToDefaults()
    {
        if (settings == null) return;
        
        settings.brightness = 0;
        settings.contrast = 1;
        settings.threshold = 0;
        settings.gamma = 1;
        settings.displacementStrength = 1;
        settings.displacementOffset = 0;
        settings.invertDisplacement = false;
        settings.horizontalWave = 0;
        settings.verticalWave = 0;
        settings.rotationX = 0;
        settings.rotationY = 0;
        settings.rotationZ = 0;
        settings.meshScale = 1;
        settings.keystoneH = 0;
        settings.keystoneV = 0;
        settings.barrelDistortion = 0;
        settings.glowIntensity = 0.5f;
        settings.lineWidth = 0.01f;
        settings.primaryColor = Color.green;
        settings.secondaryColor = Color.cyan;
        
        if (feedbackEffect != null)
            feedbackEffect.ClearFeedback();
        
        audioReactive?.RecaptureBaseValues();
        
        Debug.Log("Reset to defaults");
    }
    
    private void ShowHelp()
    {
        string help = @"
=== RuttEtra Performance Controls ===

DISPLAY:
  F11      - Toggle fullscreen
  H        - Toggle UI panel

PRESETS:
  [ ]      - Previous/Next preset
  \        - Random preset
  P        - Save current as preset
  1-9      - Load preset by number

RECORDING:
  Ctrl+R   - Start/Stop recording
  F12      - Screenshot

TOGGLES:
  I        - Invert displacement
  C        - Source color mode
  V        - Vertical lines
  L        - Interlace
  E        - Edge detect
  A        - Audio reactive
  F        - Feedback effect
  T        - CRT effect
  Y        - VHS effect

ADJUSTMENTS:
  +/-      - Displacement strength
  , .      - Glow intensity
  Shift+Arrows - Rotation

OTHER:
  Backspace - Reset to defaults
  F1       - Show this help
";
        Debug.Log(help);
    }
}
