# RuttEtra - Unity Video Synthesizer

A real-time recreation of the classic **Rutt/Etra video synthesizer** (1970s analog video art tool) in Unity 6 with URP. Transforms webcam input into 3D wireframe visualizations where brightness controls depth displacement.

![RuttEtra Preview](preview.png)

## Quick Start

1. Open the project in **Unity 6** (uses Universal Render Pipeline)
2. Run `RuttEtra > Setup Scene` from the menu to create the base scene objects
3. Run `RuttEtra > Add All Features` to add audio reactive, presets, recording, etc.
4. Run `RuttEtra > Create UI` to generate the full control panel
5. Run `RuttEtra > Add Orbit Camera` to add mouse camera controls
6. Press **Play** - the webcam will start and you'll see the visualization
7. Press **F1** for keyboard shortcuts help

---

## Features Overview

### Core Rutt/Etra Controls
- **Input Signal Processing**: Brightness, contrast, threshold, gamma, edge detection, posterize
- **Z-Axis Displacement**: Strength, smoothing, offset, invert, modulation
- **Raster Controls**: Position, scale, rotation (X/Y/Z)
- **Distortion**: Keystone (H/V), barrel distortion
- **Scan Lines**: Skip, horizontal/vertical toggle, interlace
- **Deflection Wave**: Horizontal/vertical wave, frequency, speed
- **Line Style**: Width, taper, glow intensity
- **Colors**: Primary/secondary hue & saturation, blend, source color mode
- **Resolution**: Configurable horizontal/vertical mesh resolution
- **Post Effects**: Noise, persistence, flicker, bloom

### Audio Reactive System
Real-time audio analysis with microphone input:
- **Audio device selection**: Choose from any connected microphone, webcam mic, or audio input
- **FFT frequency bands**: Bass, Low-Mid, Mid, High-Mid, Treble
- **Beat detection** with adjustable threshold and cooldown
- **Parameter modulation**: Map any audio band to:
  - Displacement strength
  - Wave amplitude
  - Hue shift
  - Scale
  - Rotation
  - Glow intensity
- **Beat effects**: Flash and pulse on beat detection
- **Real-time level display** in UI showing bass/mid/treble levels

### Analog Emulation (Post-Processing)
Classic video effects with full UI control. Uses URP-compatible render pipeline callbacks.

**Note:** These effects are applied to the Main Camera. Enable them in the UI or Inspector.

**CRT Simulation:**
- Scanline intensity and count
- Phosphor glow
- Screen curvature
- Vignette

**VHS Artifacts:**
- Tracking noise
- Color bleed
- Tape noise
- Horizontal jitter

**Additional Effects:**
- Chromatic aberration
- Horizontal/vertical hold drift
- Static noise and snow
- Saturation, hue shift, gamma

### Visual Feedback Effect
Classic video feedback loops:
- **Feedback amount** (persistence)
- **Transform**: Zoom, rotation, X/Y offset
- **Color modification**: Hue shift, saturation decay, brightness decay
- **Clear button** to reset feedback buffer

### Preset System
Save and recall parameter states:
- **Dropdown selector** for quick preset switching
- **Save/Load presets** to JSON files
- **Preset morphing**: Smooth animated transitions between states
- **Navigation buttons**: Previous, Next, Random
- **Default presets**: Classic Green, Neon Pink, Minimal White, Psychedelic, Retro CRT
- Storage: `[PersistentDataPath]/Presets/`

### Video Recording
Capture your creations:
- **Screenshot button** (also F12 key)
- **Recording toggle** with status display
- **Target FPS** slider (15-60)
- Output formats: PNG sequence, JPG sequence
- Storage: `[PersistentDataPath]/Recordings/`

### Video File Input
Process pre-recorded video instead of webcam:
- Support for VideoClip assets and URLs
- Playback controls: play, pause, seek, loop
- Variable playback speed

### OSC Control
Network control via Open Sound Control:
- **Enable toggle** in UI
- **Log messages** toggle for debugging
- **Status display** showing port number
- Default port: **9000**
- Works with TouchDesigner, Max/MSP, Resolume, VJ software

### MIDI Control
Hardware controller support:
- **Enable toggle** in UI
- **Learn mode** for easy mapping
- **CC mapping**: Knobs/faders to continuous parameters
- **Note mapping**: Buttons/pads to toggles
- Default mappings included for common controllers

### LFO Animation
Built-in parameter animation:
- **Auto-rotate**: Continuous Y-axis rotation
- **Hue cycle**: Rainbow color cycling
- **Wave animate**: Oscillating wave distortion
- **Z pulse**: Breathing displacement
- **Scale breathe**: Pulsing scale
- **Distortion animate**: Cycling distortion effects
- Individual speed controls for each

### Performance Controller
Live performance features:
- **Comprehensive keyboard shortcuts**
- **Fullscreen mode** (F11)
- **Hide/show UI** (H)
- **Number keys** for instant preset recall (1-9)
- **Quick toggles** for all features

---

## UI Control Panel

The UI panel (toggled with **H** key) contains all controls organized in sections:

| Section | Controls |
|---------|----------|
| **Input Signal** | Brightness, Contrast, Threshold, Gamma, Edge Detect, Posterize |
| **Z-Axis Displacement** | Strength, Smoothing, Offset, Invert, Z Modulation |
| **Raster Position** | H/V Position |
| **Raster Scale** | H/V Scale, Overall Scale |
| **Raster Rotation** | X/Y/Z Rotation |
| **Distortion** | Keystone H/V, Barrel |
| **Scan Lines** | Line Skip, Horizontal, Vertical, Interlace |
| **Deflection Wave** | H/V Wave, Frequency, Speed |
| **Line Style** | Width, Taper, Glow |
| **Colors** | Primary/Secondary Hue & Sat, Blend, Source Color |
| **Resolution** | Horizontal, Vertical |
| **Feedback (Settings)** | Amount, Zoom, Rotation |
| **Post Effects** | Noise, Persistence, Flicker, Bloom |
| **Webcam** | Mirror H/V |
| **Animation (LFO)** | Rotate, Hue Cycle, Wave, Z Pulse, Breathe, Distortion |
| **Audio Reactive** | Enable, Device selector, Gain, Smoothing, Modulation toggles & amounts, Beat detection |
| **CRT Effects** | Enable, Scanlines, Phosphor, Curvature, Vignette |
| **VHS Effects** | Enable, Tracking, Color Bleed, Tape Noise, Jitter |
| **Analog Color** | Chromatic, Hold Drift, Signal Noise, Saturation, Hue Shift |
| **Visual Feedback** | Enable, Amount, Zoom, Rotation, Offset, Color mods, Clear |
| **Presets** | Dropdown, Save, Prev/Next/Random, Morph toggle & duration |
| **Recording** | Record toggle, Screenshot, FPS, Status |
| **OSC Control** | Enable, Log Messages, Status |
| **MIDI Control** | Enable, Learn Mode, Status |
| **Actions** | Reset Camera, Reset All |

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **F1** | Show help |
| **F11** | Toggle fullscreen |
| **F12** | Screenshot |
| **H** | Hide/show UI |
| **[ ]** | Previous/Next preset |
| **\\** | Random preset |
| **P** | Save current preset |
| **1-9** | Load preset by number |
| **Ctrl+R** | Start/Stop recording |
| **I** | Invert displacement |
| **C** | Source color mode |
| **V** | Vertical lines |
| **L** | Interlace |
| **E** | Edge detect |
| **A** | Audio reactive |
| **F** | Feedback effect |
| **T** | CRT effect |
| **Y** | VHS effect |
| **+/-** | Displacement strength |
| **< >** | Glow intensity |
| **Shift+Arrows** | Rotation |
| **Backspace** | Reset to defaults |

---

## Project Architecture

### Core Scripts (`Assets/Scripts/RuttEtra/`)

| File | Purpose |
|------|---------|
| `RuttEtraController.cs` | Main controller, holds references |
| `RuttEtraSettings.cs` | ScriptableObject with all parameters |
| `RuttEtraMeshGenerator.cs` | Generates wireframe mesh from luminance |
| `WebcamCapture.cs` | Webcam initialization and capture |
| `RuttEtraUI.cs` | Runtime UI controller with all bindings |
| `RuttEtraAnimator.cs` | LFO-based parameter animation |
| `OrbitCamera.cs` | Mouse-controlled orbit camera |

### Feature Scripts

| File | Purpose |
|------|---------|
| `AudioReactive.cs` | Microphone input, FFT analysis, beat detection, parameter modulation |
| `AnalogEffects.cs` | CRT/VHS post-processing controller |
| `AnalogEffectsFeature.cs` | URP Renderer Feature for analog effects |
| `FeedbackEffect.cs` | Visual feedback loops with transform and color |
| `PresetManager.cs` | Preset save/load/morph with JSON storage |
| `VideoRecorder.cs` | Screenshot and video recording to image sequences |
| `VideoFileInput.cs` | Video file playback as alternative to webcam |
| `OSCReceiver.cs` | OSC network input with full parameter mapping |
| `MIDIInput.cs` | MIDI controller support with learn mode |
| `PerformanceController.cs` | Keyboard shortcuts and fullscreen |

### Editor Scripts (`Assets/Scripts/RuttEtra/Editor/`)

| File | Purpose |
|------|---------|
| `RuttEtraSceneSetup.cs` | Menu item to setup base scene |
| `RuttEtraUICreator.cs` | Programmatically creates full UI panel |
| `RuttEtraAdvancedSetup.cs` | Menu items to add feature components |

### Shaders (`Assets/Shaders/`)

| File | Purpose |
|------|---------|
| `RuttEtraScanLine.shader` | Wireframe rendering with geometry shader for line width |
| `WebcamProcess.shader` | Webcam texture processing |
| `AnalogEffects.shader` | CRT/VHS/chromatic post-processing |
| `FeedbackEffect.shader` | Video feedback with transform and color |

---

## OSC Address Reference

### Main Parameters
```
/rutt/displacement [0-5]
/rutt/offset [-1 to 1]
/rutt/brightness [-1 to 1]
/rutt/contrast [0.1-3]
/rutt/threshold [0-1]
/rutt/gamma [0.1-3]
/rutt/invert [0/1]
```

### Transform
```
/rutt/rotationx [-180 to 180]
/rutt/rotationy [-180 to 180]
/rutt/rotationz [-180 to 180]
/rutt/scale [0.1-3]
/rutt/hscale [0.1-3]
/rutt/vscale [0.1-3]
/rutt/hposition [-10 to 10]
/rutt/vposition [-10 to 10]
```

### Wave
```
/rutt/hwave [0-2]
/rutt/vwave [0-2]
/rutt/wavefrequency [0-10]
/rutt/wavespeed [0-5]
```

### Line Style
```
/rutt/linewidth [0.001-0.05]
/rutt/glow [0-2]
/rutt/linetaper [0-1]
```

### Distortion
```
/rutt/keystoneh [-1 to 1]
/rutt/keystonev [-1 to 1]
/rutt/barrel [-0.5 to 0.5]
```

### Colors
```
/rutt/colorblend [0-1]
/rutt/sourcecolor [0/1]
/rutt/primaryhue [0-1]
/rutt/secondaryhue [0-1]
```

### Post Effects
```
/rutt/noise [0-1]
/rutt/persistence [0-1]
/rutt/flicker [0-1]
/rutt/bloom [0-1]
```

### Audio Reactive
```
/rutt/audio/enable [0/1]
/rutt/audio/gain [0.1-10]
/rutt/audio/displacement [0/1]
/rutt/audio/wave [0/1]
/rutt/audio/hue [0/1]
```

### Analog Effects
```
/rutt/crt/enable [0/1]
/rutt/crt/scanlines [0-1]
/rutt/vhs/enable [0/1]
/rutt/chromatic [0-0.1]
```

---

## MIDI Default Mappings

### CC Mappings (Knobs/Faders)
| CC | Parameter | Range |
|----|-----------|-------|
| 1 | Displacement | 0-5 |
| 2 | Brightness | -1 to 1 |
| 3 | Contrast | 0.1-3 |
| 4 | H Wave | 0-2 |
| 5 | V Wave | 0-2 |
| 6 | Rotation Y | -180 to 180 |
| 7 | Scale | 0.1-3 |
| 8 | Glow | 0-2 |
| 9 | Line Width | 0.001-0.05 |
| 10 | Color Blend | 0-1 |
| 11 | Noise | 0-1 |
| 12 | Keystone H | -1 to 1 |

### Note Mappings (Pads/Buttons)
| Note | Action |
|------|--------|
| 36 | Invert |
| 37 | Source Color |
| 38 | H Lines |
| 39 | V Lines |
| 40 | Interlace |
| 41 | Audio Displacement |
| 42 | Audio Wave |
| 43 | Audio Hue |

---

## VFX Graph Renderer (Alternative)

See the `RuttEtraUI_VFXGraph` scene for a VFX Graph-based alternative renderer using GPU particles.

---

## Dependencies

- **Unity 6** (2023.x or later)
- **Universal Render Pipeline** (URP)
- **TextMeshPro**
- **Input System** (new input system)
- **Visual Effect Graph** (optional, for VFX renderer)

---

## For AI Assistants / Future Sessions

### Quick Context Recovery
This is a Unity 6 URP project recreating the Rutt/Etra video synthesizer with:
- Webcam input → luminance → 3D wireframe mesh
- Comprehensive scrollable UI panel with all controls
- Audio reactive modulation (microphone FFT + beat detection)
- Analog video emulation (CRT/VHS post-processing)
- Visual feedback effect (recursive zoom/rotate/color)
- OSC/MIDI external control
- Preset save/load with morphing transitions
- Video recording/screenshots
- Keyboard shortcuts for live performance

### Key Files
- `RuttEtraSettings.cs` - All parameters as ScriptableObject
- `RuttEtraMeshGenerator.cs` - Core mesh generation from luminance
- `RuttEtraScanLine.shader` - Wireframe rendering with geometry shader
- `RuttEtraUI.cs` - All UI bindings for all features
- `RuttEtraUICreator.cs` - Programmatic UI creation

### Common Tasks

**Add new parameter:**
1. Add field to `RuttEtraSettings.cs`
2. Add UI field to `RuttEtraUI.cs`
3. Add binding in `BindEvents()` or `BindNewFeatureEvents()`
4. Add UI creation in `RuttEtraUICreator.cs`
5. Use in `RuttEtraMeshGenerator.cs` or shader

**Recreate UI:**
```
RuttEtra > Create UI
```

**Add all features to scene:**
```
RuttEtra > Setup Scene (if new)
RuttEtra > Add All Features
RuttEtra > Create UI
```

**Add individual features:**
```
RuttEtra > Add Audio Reactive Only
RuttEtra > Add Analog Effects Only
RuttEtra > Add Preset System Only
```

### File Locations
- Settings: `Assets/Settings/RuttEtraSettings.asset`
- Main scene: `Assets/Scenes/SampleScene.unity`
- Presets: `[PersistentDataPath]/Presets/`
- Recordings: `[PersistentDataPath]/Recordings/`

---

## Original Rutt/Etra Reference

The original Rutt/Etra (1973) was an analog video synthesizer that:
- Took NTSC video input
- Used luminance to deflect electron beam in Z-axis
- Created iconic "3D wireframe" video art aesthetic
- Was used by artists like Nam June Paik, Steina & Woody Vasulka

This digital recreation captures that aesthetic with modern real-time capabilities, extensive control options, and integration with modern tools like OSC, MIDI, and audio reactivity.

---

## Development Status

### Stable Features (Tested & Working)
- **Core Rutt/Etra visualization** - Webcam to 3D wireframe mesh
- **All raster controls** - Position, scale, rotation, distortion
- **Deflection wave effects** - Horizontal/vertical wave modulation
- **Line style controls** - Width, taper, glow
- **Color controls** - Hue, saturation, blend, source color
- **Resolution adjustment** - Configurable mesh density
- **UI system** - Full control panel with all parameters
- **Orbit camera** - Mouse-controlled camera movement
- **Preset system** - Save/load/morph presets (JSON storage)
- **LFO animation** - Auto-rotate, hue cycle, wave animate, etc.
- **Screenshot capture** - F12 key or UI button
- **Keyboard shortcuts** - Performance controller

### In Development (Requires Testing)
The following features have been implemented but require testing to verify functionality:

**Audio Reactive System:**
- Microphone capture and FFT analysis implemented
- Parameter modulation (displacement, wave, hue, scale) implemented
- Audio device selection dropdown in UI
- **Needs testing:** Different audio devices, beat detection sensitivity tuning
- **Setup:** Enable `enableAudio`, select device, enable modulation toggles

**Analog Effects (CRT/VHS):**
- Uses Unity 6 RenderGraph API via `AnalogEffectsFeature`
- Implements `RecordRenderGraph()` with `AddUnsafePass` and `Blitter.BlitCameraTexture`
- Uses `CommandBufferHelpers.GetNativeCommandBuffer()` for Unity 6 compatibility
- **Needs testing:** Visual verification of CRT scanlines, VHS artifacts, chromatic aberration
- **Setup:** Run `RuttEtra > Setup Analog Effects Renderer`, enable effects on Main Camera

**Visual Feedback Effect:**
- Currently uses legacy `OnRenderImage` approach
- **Not functional in pure URP** - requires conversion to RenderGraph API
- Planned for future update

**MIDI Control:**
- Implementation complete with learn mode
- **Needs testing:** Requires MIDI hardware device
- Default mappings provided for common controllers

**OSC Control:**
- Implementation complete with full parameter mapping
- **Needs testing:** Requires external OSC software (TouchDesigner, Max/MSP, etc.)
- Default port: 9000

**Video Recording:**
- Screenshot capture (F12) implemented
- Video sequence recording implemented
- **Needs testing:** Performance impact, output file verification

**Video File Input:**
- Implementation complete
- **Needs testing:** VideoClip asset playback

### Known Issues

1. **Feedback Effect**: Not functional in URP. Requires conversion to RenderGraph API (planned).

2. **Audio Device Permissions**: On some systems, microphone permissions must be granted. The device list refreshes automatically every few seconds.

3. **Post-Processing Performance**: CRT/VHS effects add GPU overhead. Disable if frame rate drops.

### Technical Notes for Developers

**Unity 6 RenderGraph API for Post-Processing:**

The `AnalogEffectsFeature` demonstrates the correct pattern for Unity 6 URP:

```csharp
public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
{
    var resourceData = frameData.Get<UniversalResourceData>();
    if (resourceData.isActiveTargetBackBuffer) return;
    
    var source = resourceData.activeColorTexture;
    var destination = renderGraph.CreateTexture(renderGraph.GetTextureDesc(source));
    
    using (var builder = renderGraph.AddUnsafePass<PassData>("EffectName", out var passData))
    {
        passData.source = source;
        passData.destination = destination;
        passData.material = _material;
        
        builder.UseTexture(source, AccessFlags.ReadWrite);
        builder.UseTexture(destination, AccessFlags.ReadWrite);
        
        builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
        {
            // Convert UnsafeCommandBuffer to CommandBuffer for Blitter
            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
            Blitter.BlitCameraTexture(cmd, data.source, data.destination, data.material, 0);
            Blitter.BlitCameraTexture(cmd, data.destination, data.source);
        });
    }
}
```

**Key Points:**
- Use `AddUnsafePass` for custom blitting operations
- Convert `UnsafeCommandBuffer` via `CommandBufferHelpers.GetNativeCommandBuffer()`
- `Blitter.BlitCameraTexture()` handles UV coordinate conventions
- Set `requiresIntermediateTexture = true` in constructor
- Skip if `resourceData.isActiveTargetBackBuffer` is true

---

## Troubleshooting

### Effects Not Working

**Audio Reactive not responding:**
1. Check that `enableAudio` is ON in the AudioReactive component
2. Select an audio device from the dropdown (webcam mic, microphone, etc.)
3. Enable at least one modulation toggle (e.g., "Mod Displacement")
4. Increase the Input Gain if levels are too low
5. Check Unity console for "No microphone found" errors
6. Grant microphone permissions if prompted by OS

**CRT/VHS effects not visible:**
1. Run `RuttEtra > Setup Analog Effects Renderer` from the menu
2. Check that `AnalogEffectsFeature` is added to your URP Renderer asset (e.g., `PC_Renderer`)
3. Select Main Camera in hierarchy
4. Enable `enableEffects` on the AnalogEffects component
5. Enable specific effects: `enableCRT`, `enableVHS`, etc.
6. Increase intensity values (e.g., `scanlineIntensity` to 0.5+)

**Feedback effect not working:**
- **Note:** Feedback effect is currently not functional in URP (in development)
- This feature requires conversion to RenderGraph API

### UI Not Controlling Parameters

1. Make sure you ran `RuttEtra > Create UI` to generate the UI
2. The UI auto-finds components at Start - if components were added after UI, recreate the UI
3. Check Console for errors about missing references

### Performance Issues

- Reduce mesh resolution (horizontalResolution, verticalResolution)
- Disable unused effects (CRT, VHS, Feedback)
- Disable audio reactive if not needed
- Lower the recording FPS if capturing video

---

## License

[Add your license here]

## Credits

Digital recreation by [Your Name]
Original Rutt/Etra designed by Steve Rutt and Bill Etra (1973)
