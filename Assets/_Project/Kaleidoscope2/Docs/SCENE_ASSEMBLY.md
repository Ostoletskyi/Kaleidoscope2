# SCENE_ASSEMBLY.md

`Kaleidoscope2_Main.unity` is the first visible runtime assembly scene.

## Purpose

The scene proves the foundation loop without implementing production visual systems:

```text
KaleidoscopeBootstrap
    |
    v
KaleidoscopeDirector
    |
    v
SourceModule -> MirrorModule placeholder -> Director.FinalOutputTexture
    |
    v
RuntimeMenuController RawImage
```

## Root Hierarchy

```text
KaleidoscopeBootstrapRoot
├── KaleidoscopeBootstrap
├── KaleidoscopeDirector
├── SourceModule
├── MirrorModule
├── CameraModule
│   ├── SourceCamera
│   ├── ViewerCamera
│   ├── RenderCamera
│   └── OfflineCamera
├── DiagnosticsModule
└── RuntimeMenuController
```

The runtime menu uses a Canvas under `RuntimeMenuController`. The diagnostics HUD is owned by `DiagnosticsModule`.

## Control Rules

- Runtime UI sends `KaleidoscopeCommand` objects to `KaleidoscopeDirector`.
- Runtime UI reads `KaleidoscopeState` for labels and control values.
- Runtime UI reads `KaleidoscopeDirector.FinalOutputTexture` for display.
- Runtime UI does not mutate shader, camera, tunnel, recording, physics, source, or mirror internals.

## Visible Output

`SourceModule` generates a small placeholder gradient texture. `MirrorModule` currently passes it through unchanged. This is intentional and must not be mistaken for the final kaleidoscope shader stage.

## Placeholder Limits

The scene does not implement:

- advanced kaleidoscope shaders;
- tunnel rendering;
- recording export;
- audio reactive behavior;
- source media import;
- production camera routing.
