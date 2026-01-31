# RuttEtra

A real-time recreation of the legendary **Rutt/Etra Video Synthesizer** in Unity. Transform your webcam feed into mesmerizing 3D wireframe landscapes where image brightness controls depth displacement.

![Unity](https://img.shields.io/badge/Unity-6000.2+-black?logo=unity)
![License](https://img.shields.io/badge/license-MIT-blue)

## About

The original Rutt/Etra was an analog video synthesizer created by Steve Rutt and Bill Etra in the early 1970s. It became iconic for its ability to transform standard video into three-dimensional wireframe representations, where the luminance (brightness) of each scan line controlled the vertical displacement of that line in 3D space.

This project brings that classic effect to modern hardware using Unity's Universal Render Pipeline, allowing real-time manipulation of webcam feeds with customizable parameters.

## Features

- **Real-time webcam processing** with configurable resolution
- **Luminance-based displacement** — brighter areas push forward, darker areas recede
- **Horizontal & vertical scan lines** with adjustable density
- **Custom color modes** — use source video colors or stylized gradients
- **Glow and noise effects** for authentic CRT aesthetics
- **Orbital camera controls** for viewing the 3D effect from any angle
- **Scriptable settings** for easy preset management

## Requirements

- **Unity 6** (6000.2.14f1 or later)
- **Universal Render Pipeline (URP)**
- A webcam

## Quick Start

1. **Clone or download** this repository
2. **Open the project** in Unity 6
3. Go to **RuttEtra → Setup Scene** in the menu bar
4. **Press Play** — the effect will begin using your default webcam

## Controls

| Input | Action |
|-------|--------|
| `W` / `↑` | Tilt camera up |
| `S` / `↓` | Tilt camera down |
| `A` / `←` | Orbit camera left |
| `D` / `→` | Orbit camera right |
| `Mouse Scroll` | Zoom in/out |
| `Space` | Toggle displacement inversion |
| `Tab` | Toggle source colors |
| `V` | Toggle vertical scan lines |
| `R` | Reset camera position |
| `H` | Toggle UI panel (if set up) |

## Configuration

All visual parameters are stored in a `RuttEtraSettings` ScriptableObject located at `Assets/Settings/RuttEtraSettings.asset`. You can adjust these in the Inspector or create multiple presets.

### Mesh Resolution
| Parameter | Range | Description |
|-----------|-------|-------------|
| `Horizontal Resolution` | 16–512 | Number of vertices per scan line |
| `Vertical Resolution` | 8–256 | Number of scan lines |

### Displacement
| Parameter | Range | Description |
|-----------|-------|-------------|
| `Displacement Strength` | 0–5 | How far vertices displace based on luminance |
| `Displacement Smoothing` | 0–1 | Temporal smoothing to reduce jitter |
| `Invert Displacement` | Toggle | Swap bright/dark displacement direction |

### Visual Style
| Parameter | Range | Description |
|-----------|-------|-------------|
| `Line Width` | 0.001–0.05 | Thickness of scan lines |
| `Primary Color` | Color | Base color for low luminance areas |
| `Secondary Color` | Color | Color for high luminance areas |
| `Color Blend` | 0–1 | How much luminance affects color gradient |
| `Use Source Color` | Toggle | Use original webcam colors instead |

### Scan Lines
| Parameter | Range | Description |
|-----------|-------|-------------|
| `Scan Line Skip` | 1–8 | Draw every Nth line (higher = fewer lines) |
| `Show Horizontal Lines` | Toggle | Enable horizontal scan lines |
| `Show Vertical Lines` | Toggle | Enable vertical scan lines |

### Post Effects
| Parameter | Range | Description |
|-----------|-------|-------------|
| `Glow Intensity` | 0–2 | Additive glow on lines |
| `Noise Amount` | 0–1 | Analog noise/jitter effect |

## Project Structure

```
Assets/
├── Scripts/
│   └── RuttEtra/
│       ├── RuttEtraController.cs    # Main orchestrator & camera controls
│       ├── RuttEtraMeshGenerator.cs # Mesh generation & displacement
│       ├── RuttEtraSettings.cs      # ScriptableObject settings
│       ├── RuttEtraUI.cs            # UI bindings
│       ├── WebcamCapture.cs         # Webcam input handling
│       └── Editor/
│           └── RuttEtraSceneSetup.cs # Editor tooling
├── Shaders/
│   ├── RuttEtraScanLine.shader      # Line rendering with glow/noise
│   └── WebcamProcess.shader         # Webcam mirroring/processing
└── Settings/
    └── RuttEtraSettings.asset       # Default configuration
```

## How It Works

1. **WebcamCapture** grabs frames from your webcam and optionally mirrors them
2. **RuttEtraMeshGenerator** creates a grid of vertices matching the configured resolution
3. Each frame, the webcam texture is sampled and converted to luminance values
4. Vertex positions are displaced along the Z-axis based on their corresponding luminance
5. The mesh is rendered as lines (not triangles) using `MeshTopology.Lines`
6. A custom shader adds glow and noise effects with additive blending

## Tips

- **Lower resolution** (64×32) gives a more authentic, chunky retro look
- **Higher resolution** (256×128) creates smoother, more detailed surfaces
- **Increase Scan Line Skip** for the classic spaced-line aesthetic
- **Enable vertical lines** along with horizontal for a grid/mesh effect
- **Use dark backgrounds** in your webcam view for dramatic displacement contrast
- **Invert displacement** to make dark areas pop forward instead of bright

## License

MIT License — feel free to use, modify, and distribute.

## Acknowledgments

- **Steve Rutt & Bill Etra** for creating the original video synthesizer
- The analog video art community for keeping these techniques alive
