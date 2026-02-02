using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

/// <summary>
/// MIDI input controller for hardware control of RuttEtra parameters.
/// Uses Unity's Input System MIDI support.
/// 
/// Supports:
/// - CC (Control Change) messages for continuous parameters
/// - Note On/Off for toggles and triggers
/// - Learn mode for mapping controls
/// </summary>
public class MIDIInput : MonoBehaviour
{
    [Header("Enable")]
    public bool enableMIDI = true;
    
    [Header("References")]
    public RuttEtraSettings settings;
    public AudioReactive audioReactive;
    
    [Header("CC Mappings (Controller Number -> Parameter)")]
    public List<CCMapping> ccMappings = new List<CCMapping>();
    
    [Header("Note Mappings (Note Number -> Toggle/Action)")]
    public List<NoteMapping> noteMappings = new List<NoteMapping>();
    
    [Header("Learn Mode")]
    public bool learnMode = false;
    public string learnTarget = "";
    
    // Events
    public event Action<int, float> OnCCReceived;
    public event Action<int, bool> OnNoteReceived;
    
    [Serializable]
    public class CCMapping
    {
        public int ccNumber;
        public string parameter;
        public float minValue = 0f;
        public float maxValue = 1f;
        public bool inverted = false;
    }
    
    [Serializable]
    public class NoteMapping
    {
        public int noteNumber;
        public string action;
        public bool toggle = true;
    }
    
    private InputAction _midiCCAction;
    private InputAction _midiNoteAction;
    private Dictionary<string, Action<float>> _parameterSetters;
    private Dictionary<string, Action> _actionTriggers;
    private Dictionary<string, bool> _toggleStates;
    
    private void Start()
    {
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller) settings = controller.settings;
        }
        
        if (audioReactive == null)
            audioReactive = FindFirstObjectByType<AudioReactive>();
        
        SetupDefaultMappings();
        SetupParameterSetters();
        SetupMIDIInput();
    }
    
    private void SetupDefaultMappings()
    {
        if (ccMappings.Count == 0)
        {
            // Default CC mappings for common MIDI controllers
            ccMappings.Add(new CCMapping { ccNumber = 1, parameter = "displacement", minValue = 0, maxValue = 5 });
            ccMappings.Add(new CCMapping { ccNumber = 2, parameter = "brightness", minValue = -1, maxValue = 1 });
            ccMappings.Add(new CCMapping { ccNumber = 3, parameter = "contrast", minValue = 0.1f, maxValue = 3 });
            ccMappings.Add(new CCMapping { ccNumber = 4, parameter = "hwave", minValue = 0, maxValue = 2 });
            ccMappings.Add(new CCMapping { ccNumber = 5, parameter = "vwave", minValue = 0, maxValue = 2 });
            ccMappings.Add(new CCMapping { ccNumber = 6, parameter = "rotationY", minValue = -180, maxValue = 180 });
            ccMappings.Add(new CCMapping { ccNumber = 7, parameter = "scale", minValue = 0.1f, maxValue = 3 });
            ccMappings.Add(new CCMapping { ccNumber = 8, parameter = "glow", minValue = 0, maxValue = 2 });
            ccMappings.Add(new CCMapping { ccNumber = 9, parameter = "linewidth", minValue = 0.001f, maxValue = 0.05f });
            ccMappings.Add(new CCMapping { ccNumber = 10, parameter = "colorblend", minValue = 0, maxValue = 1 });
            ccMappings.Add(new CCMapping { ccNumber = 11, parameter = "noise", minValue = 0, maxValue = 1 });
            ccMappings.Add(new CCMapping { ccNumber = 12, parameter = "keystoneH", minValue = -1, maxValue = 1 });
        }
        
        if (noteMappings.Count == 0)
        {
            // Default note mappings
            noteMappings.Add(new NoteMapping { noteNumber = 36, action = "invert", toggle = true });
            noteMappings.Add(new NoteMapping { noteNumber = 37, action = "sourcecolor", toggle = true });
            noteMappings.Add(new NoteMapping { noteNumber = 38, action = "hlines", toggle = true });
            noteMappings.Add(new NoteMapping { noteNumber = 39, action = "vlines", toggle = true });
            noteMappings.Add(new NoteMapping { noteNumber = 40, action = "interlace", toggle = true });
            noteMappings.Add(new NoteMapping { noteNumber = 41, action = "audio_displacement", toggle = true });
            noteMappings.Add(new NoteMapping { noteNumber = 42, action = "audio_wave", toggle = true });
            noteMappings.Add(new NoteMapping { noteNumber = 43, action = "audio_hue", toggle = true });
        }
    }
    
    private void SetupParameterSetters()
    {
        _parameterSetters = new Dictionary<string, Action<float>>();
        _actionTriggers = new Dictionary<string, Action>();
        _toggleStates = new Dictionary<string, bool>();
        
        if (settings != null)
        {
            // Continuous parameters
            _parameterSetters["displacement"] = v => settings.displacementStrength = v;
            _parameterSetters["offset"] = v => settings.displacementOffset = v;
            _parameterSetters["brightness"] = v => settings.brightness = v;
            _parameterSetters["contrast"] = v => settings.contrast = v;
            _parameterSetters["threshold"] = v => settings.threshold = v;
            _parameterSetters["gamma"] = v => settings.gamma = v;
            _parameterSetters["rotationx"] = v => settings.rotationX = v;
            _parameterSetters["rotationy"] = v => settings.rotationY = v;
            _parameterSetters["rotationz"] = v => settings.rotationZ = v;
            _parameterSetters["scale"] = v => settings.meshScale = v;
            _parameterSetters["hscale"] = v => settings.horizontalScale = v;
            _parameterSetters["vscale"] = v => settings.verticalScale = v;
            _parameterSetters["hposition"] = v => settings.horizontalPosition = v;
            _parameterSetters["vposition"] = v => settings.verticalPosition = v;
            _parameterSetters["hwave"] = v => settings.horizontalWave = v;
            _parameterSetters["vwave"] = v => settings.verticalWave = v;
            _parameterSetters["wavefrequency"] = v => settings.waveFrequency = v;
            _parameterSetters["wavespeed"] = v => settings.waveSpeed = v;
            _parameterSetters["linewidth"] = v => settings.lineWidth = v;
            _parameterSetters["glow"] = v => settings.glowIntensity = v;
            _parameterSetters["linetaper"] = v => settings.lineTaper = v;
            _parameterSetters["keystoneh"] = v => settings.keystoneH = v;
            _parameterSetters["keystonev"] = v => settings.keystoneV = v;
            _parameterSetters["barrel"] = v => settings.barrelDistortion = v;
            _parameterSetters["colorblend"] = v => settings.colorBlend = v;
            _parameterSetters["noise"] = v => settings.noiseAmount = v;
            _parameterSetters["persistence"] = v => settings.persistence = v;
            _parameterSetters["flicker"] = v => settings.scanlineFlicker = v;
            _parameterSetters["bloom"] = v => settings.bloom = v;
            
            // Toggle actions
            _actionTriggers["invert"] = () => settings.invertDisplacement = !settings.invertDisplacement;
            _actionTriggers["sourcecolor"] = () => settings.useSourceColor = !settings.useSourceColor;
            _actionTriggers["hlines"] = () => settings.showHorizontalLines = !settings.showHorizontalLines;
            _actionTriggers["vlines"] = () => settings.showVerticalLines = !settings.showVerticalLines;
            _actionTriggers["interlace"] = () => settings.interlace = !settings.interlace;
            _actionTriggers["edgedetect"] = () => settings.edgeDetect = !settings.edgeDetect;
        }
        
        if (audioReactive != null)
        {
            _actionTriggers["audio_enable"] = () => audioReactive.enableAudio = !audioReactive.enableAudio;
            _actionTriggers["audio_displacement"] = () => audioReactive.modulateDisplacement = !audioReactive.modulateDisplacement;
            _actionTriggers["audio_wave"] = () => audioReactive.modulateWave = !audioReactive.modulateWave;
            _actionTriggers["audio_hue"] = () => audioReactive.modulateHue = !audioReactive.modulateHue;
            _actionTriggers["audio_scale"] = () => audioReactive.modulateScale = !audioReactive.modulateScale;
            _actionTriggers["audio_rotation"] = () => audioReactive.modulateRotation = !audioReactive.modulateRotation;
            _actionTriggers["audio_glow"] = () => audioReactive.modulateGlow = !audioReactive.modulateGlow;
            _actionTriggers["beat_flash"] = () => audioReactive.flashOnBeat = !audioReactive.flashOnBeat;
            _actionTriggers["beat_pulse"] = () => audioReactive.pulseOnBeat = !audioReactive.pulseOnBeat;
        }
    }
    
    private void SetupMIDIInput()
    {
        // Note: Unity's MIDI support requires the Input System package
        // and proper MIDI device setup. This provides the framework.
        
        // Try to find MIDI devices
        var midiDevice = InputSystem.GetDevice("MIDIDevice");
        if (midiDevice != null)
        {
            Debug.Log($"MIDI: Found device: {midiDevice.name}");
        }
        else
        {
            Debug.Log("MIDI: No MIDI device found. Connect a MIDI controller and restart.");
        }
    }
    
    private void Update()
    {
        if (!enableMIDI) return;
        
        // Poll for MIDI messages using Input System
        // Note: Full MIDI implementation requires platform-specific handling
        // This provides the mapping infrastructure
        
        // For testing, you can simulate MIDI with keyboard
        #if UNITY_EDITOR
        SimulateMIDIWithKeyboard();
        #endif
    }
    
    #if UNITY_EDITOR
    private void SimulateMIDIWithKeyboard()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        
        // Number keys 1-9 simulate CC 1-9
        for (int i = 1; i <= 9; i++)
        {
            var key = (Key)((int)Key.Digit1 + i - 1);
            if (kb[key].wasPressedThisFrame)
            {
                // Simulate CC with incrementing value
                float value = (Time.time % 1f);
                ProcessCC(i, value);
            }
        }
        
        // F1-F8 simulate notes 36-43
        for (int i = 0; i < 8; i++)
        {
            var key = (Key)((int)Key.F1 + i);
            if (kb[key].wasPressedThisFrame)
            {
                ProcessNote(36 + i, true);
            }
        }
    }
    #endif
    
    public void ProcessCC(int ccNumber, float normalizedValue)
    {
        if (learnMode && !string.IsNullOrEmpty(learnTarget))
        {
            // Learn mode: assign this CC to the target parameter
            var mapping = ccMappings.Find(m => m.parameter == learnTarget);
            if (mapping != null)
            {
                mapping.ccNumber = ccNumber;
            }
            else
            {
                ccMappings.Add(new CCMapping { ccNumber = ccNumber, parameter = learnTarget });
            }
            Debug.Log($"MIDI Learn: CC{ccNumber} -> {learnTarget}");
            learnMode = false;
            learnTarget = "";
            return;
        }
        
        // Find mapping
        var ccMapping = ccMappings.Find(m => m.ccNumber == ccNumber);
        if (ccMapping != null)
        {
            float value = ccMapping.inverted ? 1f - normalizedValue : normalizedValue;
            value = Mathf.Lerp(ccMapping.minValue, ccMapping.maxValue, value);
            
            if (_parameterSetters.TryGetValue(ccMapping.parameter.ToLower(), out var setter))
            {
                setter(value);
            }
        }
        
        OnCCReceived?.Invoke(ccNumber, normalizedValue);
    }
    
    public void ProcessNote(int noteNumber, bool noteOn)
    {
        if (learnMode && !string.IsNullOrEmpty(learnTarget) && noteOn)
        {
            var mapping = noteMappings.Find(m => m.action == learnTarget);
            if (mapping != null)
            {
                mapping.noteNumber = noteNumber;
            }
            else
            {
                noteMappings.Add(new NoteMapping { noteNumber = noteNumber, action = learnTarget });
            }
            Debug.Log($"MIDI Learn: Note {noteNumber} -> {learnTarget}");
            learnMode = false;
            learnTarget = "";
            return;
        }
        
        if (!noteOn) return;
        
        var noteMapping = noteMappings.Find(m => m.noteNumber == noteNumber);
        if (noteMapping != null)
        {
            if (_actionTriggers.TryGetValue(noteMapping.action.ToLower(), out var trigger))
            {
                trigger();
            }
        }
        
        OnNoteReceived?.Invoke(noteNumber, noteOn);
    }
    
    public void StartLearnMode(string targetParameter)
    {
        learnMode = true;
        learnTarget = targetParameter;
        Debug.Log($"MIDI Learn: Move a control to assign to '{targetParameter}'");
    }
    
    public void ClearMapping(int ccNumber)
    {
        ccMappings.RemoveAll(m => m.ccNumber == ccNumber);
    }
    
    public void ClearNoteMapping(int noteNumber)
    {
        noteMappings.RemoveAll(m => m.noteNumber == noteNumber);
    }
}
