#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public static class RuttEtraAdvancedSetup
{
    [MenuItem("RuttEtra/Add All Features")]
    public static void AddAllFeatures()
    {
        var controller = Object.FindFirstObjectByType<RuttEtraController>();
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Error", "Run 'RuttEtra > Setup Scene' first", "OK");
            return;
        }
        
        var settings = controller.settings;
        var webcam = controller.webcamCapture;
        
        // Add AudioReactive
        var audio = controller.GetComponent<AudioReactive>();
        if (audio == null)
        {
            audio = controller.gameObject.AddComponent<AudioReactive>();
            audio.settings = settings;
        }
        
        // Add OSC Receiver
        var osc = controller.GetComponent<OSCReceiver>();
        if (osc == null)
        {
            osc = controller.gameObject.AddComponent<OSCReceiver>();
            osc.settings = settings;
            osc.audioReactive = audio;
        }
        
        // Add MIDI Input
        var midi = controller.GetComponent<MIDIInput>();
        if (midi == null)
        {
            midi = controller.gameObject.AddComponent<MIDIInput>();
            midi.settings = settings;
            midi.audioReactive = audio;
        }
        
        // Add Preset Manager
        var presets = controller.GetComponent<PresetManager>();
        if (presets == null)
        {
            presets = controller.gameObject.AddComponent<PresetManager>();
            presets.settings = settings;
        }
        
        // Add Video Recorder
        var recorder = controller.GetComponent<VideoRecorder>();
        if (recorder == null)
        {
            recorder = controller.gameObject.AddComponent<VideoRecorder>();
        }
        
        // Add Video File Input (disabled by default)
        var videoInput = controller.GetComponent<VideoFileInput>();
        if (videoInput == null)
        {
            videoInput = controller.gameObject.AddComponent<VideoFileInput>();
            videoInput.enabled = false;
        }
        
        // Add Analog Effects to camera
        var cam = Camera.main;
        if (cam != null)
        {
            var analog = cam.GetComponent<AnalogEffects>();
            if (analog == null)
            {
                analog = cam.gameObject.AddComponent<AnalogEffects>();
            }
            
            // Add Feedback Effect
            var feedback = cam.GetComponent<FeedbackEffect>();
            if (feedback == null)
            {
                feedback = cam.gameObject.AddComponent<FeedbackEffect>();
                feedback.settings = settings;
            }
            
            // Update OSC reference
            if (osc != null)
                osc.analogEffects = analog;
        }
        
        // Add Performance Controller
        var perf = controller.GetComponent<PerformanceController>();
        if (perf == null)
        {
            perf = controller.gameObject.AddComponent<PerformanceController>();
            perf.settings = settings;
            perf.presetManager = presets;
            perf.audioReactive = audio;
            perf.videoRecorder = recorder;
            perf.analogEffects = cam?.GetComponent<AnalogEffects>();
            perf.feedbackEffect = cam?.GetComponent<FeedbackEffect>();
        }
        
        EditorUtility.SetDirty(controller.gameObject);
        if (cam != null) EditorUtility.SetDirty(cam.gameObject);
        
        // Add renderer feature
        AddAnalogEffectsRendererFeature();
        
        Debug.Log("All advanced features added!");
        
        string info = @"
=== Features Added ===

On Controller:
- AudioReactive: Microphone input & beat detection
- OSCReceiver: Network control (port 9000)
- MIDIInput: Hardware controller support
- PresetManager: Save/load presets
- VideoRecorder: Screenshot & recording
- VideoFileInput: Video file playback (disabled)
- PerformanceController: Keyboard shortcuts

On Camera:
- AnalogEffects: CRT/VHS post-processing
- FeedbackEffect: Video feedback loops

URP Renderer:
- AnalogEffectsFeature added to render pipeline

Press F1 in Play mode for keyboard shortcuts.
Presets saved to: " + Application.persistentDataPath + @"/Presets/
";
        EditorUtility.DisplayDialog("Features Added", info, "OK");
    }
    
    [MenuItem("RuttEtra/Setup Analog Effects Renderer")]
    public static void AddAnalogEffectsRendererFeature()
    {
        // Find the current URP asset
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("No URP asset found in Graphics Settings");
            return;
        }
        
        // Get the renderer data using reflection
        var rendererDataListField = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
        if (rendererDataListField == null)
        {
            Debug.LogError("Could not find renderer data list field");
            return;
        }
        
        var rendererDataArray = rendererDataListField.GetValue(urpAsset) as ScriptableRendererData[];
        if (rendererDataArray == null || rendererDataArray.Length == 0)
        {
            Debug.LogError("No renderer data found");
            return;
        }
        
        // Add feature to each renderer
        foreach (var rendererData in rendererDataArray)
        {
            if (rendererData == null) continue;
            
            // Check if feature already exists
            bool hasFeature = false;
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is AnalogEffectsFeature)
                {
                    hasFeature = true;
                    break;
                }
            }
            
            if (!hasFeature)
            {
                // Create and add the feature
                var feature = ScriptableObject.CreateInstance<AnalogEffectsFeature>();
                feature.name = "AnalogEffectsFeature";
                
                // Add to renderer features list using reflection
                var featuresField = typeof(ScriptableRendererData).GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
                if (featuresField != null)
                {
                    var features = featuresField.GetValue(rendererData) as System.Collections.Generic.List<ScriptableRendererFeature>;
                    if (features != null)
                    {
                        features.Add(feature);
                        
                        // Also need to add to m_RendererFeatureMap
                        var mapField = typeof(ScriptableRendererData).GetField("m_RendererFeatureMap", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (mapField != null)
                        {
                            var map = mapField.GetValue(rendererData) as System.Collections.Generic.List<long>;
                            if (map != null)
                            {
                                map.Add(feature.GetInstanceID());
                            }
                        }
                        
                        EditorUtility.SetDirty(rendererData);
                        AssetDatabase.AddObjectToAsset(feature, rendererData);
                        AssetDatabase.SaveAssets();
                        Debug.Log($"Added AnalogEffectsFeature to {rendererData.name}");
                    }
                }
            }
            else
            {
                Debug.Log($"AnalogEffectsFeature already exists on {rendererData.name}");
            }
        }
        
        AssetDatabase.Refresh();
    }
    
    [MenuItem("RuttEtra/Add Audio Reactive Only")]
    public static void AddAudioReactive()
    {
        var controller = Object.FindFirstObjectByType<RuttEtraController>();
        if (controller == null) return;
        
        var audio = controller.GetComponent<AudioReactive>();
        if (audio == null)
        {
            audio = controller.gameObject.AddComponent<AudioReactive>();
            audio.settings = controller.settings;
        }
        
        Selection.activeGameObject = controller.gameObject;
        Debug.Log("AudioReactive added. Enable modulation options in Inspector.");
    }
    
    [MenuItem("RuttEtra/Add Analog Effects Only")]
    public static void AddAnalogEffects()
    {
        var cam = Camera.main;
        if (cam == null) return;
        
        var analog = cam.GetComponent<AnalogEffects>();
        if (analog == null)
        {
            analog = cam.gameObject.AddComponent<AnalogEffects>();
        }
        
        // Also add the renderer feature
        AddAnalogEffectsRendererFeature();
        
        Selection.activeGameObject = cam.gameObject;
        Debug.Log("AnalogEffects added. Enable CRT/VHS in Inspector.");
    }
    
    [MenuItem("RuttEtra/Add Preset System Only")]
    public static void AddPresetSystem()
    {
        var controller = Object.FindFirstObjectByType<RuttEtraController>();
        if (controller == null) return;
        
        var presets = controller.GetComponent<PresetManager>();
        if (presets == null)
        {
            presets = controller.gameObject.AddComponent<PresetManager>();
            presets.settings = controller.settings;
        }
        
        Selection.activeGameObject = controller.gameObject;
        Debug.Log("PresetManager added. Use [ ] keys to cycle presets.");
    }
}
#endif
