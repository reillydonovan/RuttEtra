# RuttEtra - Unity Video Synthesizer

A real-time recreation of the classic **Rutt/Etra video synthesizer** (1970s analog video art tool) in Unity 6 with URP. Transforms webcam input into 3D wireframe visualizations where brightness controls depth displacement.

## Quick Start

1. Open the project in **Unity 6** (uses Universal Render Pipeline)
2. Run `RuttEtra > Setup Scene` from the menu to create the base scene objects
3. Run `RuttEtra > Create UI` to generate the control panel
4. Run `RuttEtra > Add Orbit Camera` to add mouse camera controls
5. Press **Play** - the webcam will start and you'll see the visualization
6. Use the right-side panel to adjust parameters
7. Press **H** to hide/show the UI panel
8. **Right-click + drag** to orbit camera, **scroll wheel** to zoom

---

## Project Architecture

### Core Scripts (`Assets/Scripts/RuttEtra/`)

| File | Purpose |
|------|---------|
| `RuttEtraController.cs` | Main controller, holds references to settings/webcam/mesh |
| `RuttEtraSettings.cs` | ScriptableObject with all configurable parameters |
| `RuttEtraMeshGenerator.cs` | Generates and updates the wireframe mesh from webcam luminance |
| `WebcamCapture.cs` | Handles webcam initialization and frame capture |
| `RuttEtraUI.cs` | Runtime UI controller, binds sliders/toggles to settings |
| `RuttEtraAnimator.cs` | LFO-based parameter animation system |
| `OrbitCamera.cs` | Mouse-controlled orbit camera |

### Editor Scripts (`Assets/Scripts/RuttEtra/Editor/`)

| File | Purpose |
|------|---------|
| `RuttEtraSceneSetup.cs` | Menu item to create scene hierarchy |
| `RuttEtraUICreator.cs` | Programmatically builds the UI Canvas and controls |

### Shaders (`Assets/Shaders/`)

| File | Purpose |
|------|---------|
| `RuttEtraScanLine.shader` | URP shader with geometry shader for variable line width, glow, noise |
| `WebcamProcess.shader` | (Optional) Webcam preprocessing |

### Settings Asset

- `Assets/Settings/RuttEtraSettings.asset` - The ScriptableObject instance holding all parameters

---

## Feature Overview

### Input Signal Processing
- **Brightness/Contrast/Threshold/Gamma** - Standard image adjustments
- **Edge Detect** - Use edges instead of luminance for displacement
- **Posterize** - Quantize depth levels (1-8)

### Z-Axis Displacement
- **Strength** - How much luminance affects depth
- **Smoothing** - Temporal smoothing between frames
- **Offset** - Base Z offset
- **Invert** - Flip bright/dark displacement
- **Z Modulation** - Sine wave modulation on Z

### Raster Controls
- **Position** - H/V offset of the mesh (applied to vertices)
- **Scale** - H/V/Overall scale
- **Rotation** - X/Y/Z euler rotation
- **Distortion** - Keystone H/V, Barrel/Pincushion

### Scan Lines
- **Line Skip** - Draw every Nth line
- **H/V Lines** - Toggle horizontal/vertical lines
- **Interlace** - Alternating field display

### Deflection Wave
- **H/V Wave** - Amplitude of sine wave deflection
- **Frequency/Speed** - Wave parameters

### Line Style
- **Width** - Line thickness (uses geometry shader)
- **Taper** - Edge fading
- **Glow** - Intensity boost

### Colors
- **Primary/Secondary** - Two colors for depth-based blending
- **Blend** - How much to blend based on luminance
- **Source Color** - Use original webcam colors

### Post Effects
- **Noise** - Per-pixel noise
- **Persistence** - Temporal glow trails
- **Flicker** - Random brightness variation
- **Bloom** - Soft glow effect

### Animation System
Quick toggles for animated effects:
- **Rotate** - Auto Y-axis rotation
- **Hue Cycle** - Color spectrum cycling
- **Wave** - Animated wave deflection
- **Z Pulse** - Displacement pulsing
- **Breathe** - Scale breathing
- **Distortion** - Animated keystone/barrel

Advanced LFOs available on `RuttEtraAnimator` component for per-parameter animation with waveform selection (Sine, Triangle, Square, Sawtooth, Random).

---

## Technical Details

### Mesh Generation (`RuttEtraMeshGenerator.cs`)
- Creates a grid of vertices based on `horizontalResolution` x `verticalResolution`
- Uses `MeshTopology.Lines` for wireframe rendering
- Updates vertex positions each frame based on luminance buffer
- Applies all transformations (position, scale, rotation, distortion) directly to vertices

### Shader (`RuttEtraScanLine.shader`)
- Uses **geometry shader** to expand lines into quads for variable width
- Additive blending (`Blend SrcAlpha One`) for glow effect
- Passes: vertex -> geometry (line to quad) -> fragment
- Properties: `_LineWidth`, `_GlowIntensity`, `_NoiseAmount`, `_LineTaper`, `_Bloom`

### UI System
- Built programmatically via `RuttEtraUICreator.cs`
- Uses Unity's built-in `Text` for most labels, `TextMeshPro` for resolution display
- `RectMask2D` for scrolling (not `Mask` - avoids stencil buffer issues)
- Requires `InputSystemUIInputModule` for new Input System compatibility

### Camera (`OrbitCamera.cs`)
- Right-click + drag to orbit
- Scroll wheel to zoom
- Continues updating position when hovering over UI (only skips input capture)

---

## Scene Hierarchy

```
RuttEtra_Controller
├── Webcam_Capture (WebcamCapture)
└── RuttEtra_Mesh (MeshFilter, MeshRenderer, RuttEtraMeshGenerator)

Main Camera (OrbitCamera)

EventSystem (InputSystemUIInputModule)

RuttEtra_Canvas
├── ControlPanel
│   └── Scroll
│       └── Content (all UI controls)
└── RuttEtra_UI (RuttEtraUI component)
```

---

## Known Issues / Notes

1. **Line Width** - Requires geometry shader support (Shader Model 4.0+)
2. **Webcam** - First available webcam is used; set `preferredDeviceName` on WebcamCapture to specify
3. **Resolution changes** - Changing H/V resolution triggers mesh regeneration
4. **Feedback** - Settings exist but visual feedback effect not fully implemented yet
5. **Performance** - High resolutions (256x128+) may impact framerate

---

## Dependencies

- **Unity 6** (2023.x or later)
- **Universal Render Pipeline** (URP)
- **TextMeshPro** (for resolution text display)
- **Input System** (new input system, not legacy)

Packages in `Packages/manifest.json`:
```json
"com.unity.inputsystem": "1.11.2",
"com.unity.render-pipelines.universal": "17.0.3",
"com.unity.textmeshpro": "3.0.6"
```

---

## For AI Assistants / Future Sessions

### MCP Integration
This project uses Unity MCP for AI-assisted development. Key tools:
- `recompile_scripts` - Recompile after code changes
- `execute_menu_item` - Run editor menu items like `RuttEtra/Create UI`
- `save_scene` - Save current scene
- `update_component` - Modify component properties
- `get_gameobject` - Inspect scene objects

### Common Tasks

**Recreate UI after code changes:**
```
1. recompile_scripts
2. execute_menu_item: "RuttEtra/Create UI"
3. save_scene
```

**Add new setting:**
1. Add field to `RuttEtraSettings.cs`
2. Add UI field to `RuttEtraUI.cs`
3. Add binding in `RuttEtraUI.BindEvents()`
4. Add UI creation in `RuttEtraUICreator.cs`
5. Use setting in `RuttEtraMeshGenerator.cs` or shader

**Add new animation:**
1. Add quick toggle/speed to `RuttEtraAnimator.cs`
2. Add UI fields to `RuttEtraUI.cs`
3. Add creation in `RuttEtraUICreator.cs` under ANIMATION section
4. Add binding to animator method in `RuttEtraUI.BindEvents()`

### File Locations
- Settings asset: `Assets/Settings/RuttEtraSettings.asset`
- Main scene: `Assets/Scenes/RuttEtraUI.unity`
- Shader: `Assets/Shaders/RuttEtraScanLine.shader`

### Architecture Decisions
- **Settings as ScriptableObject** - Allows persistence and easy inspector editing
- **Programmatic UI** - Created via code to avoid manual UI setup and ensure consistency
- **Vertex-based transforms** - Position/scale/rotation applied to vertices, not Transform, for better control
- **Geometry shader for lines** - Enables variable line width (Unity's line topology is always 1px)
- **Auto-find references** - Components find each other via `FindFirstObjectByType` to reduce manual wiring

---

## Original Rutt/Etra Reference

The original Rutt/Etra (1973) was an analog video synthesizer that:
- Took NTSC video input
- Used luminance to deflect electron beam in Z-axis
- Created iconic "3D wireframe" video art aesthetic
- Was used by artists like Nam June Paik, Steina & Woody Vasulka

This digital recreation aims to capture that aesthetic with modern real-time capabilities.

---

## License

[Add your license here]

## Credits

Digital recreation by [Your Name]
Original Rutt/Etra designed by Steve Rutt and Bill Etra (1973)
