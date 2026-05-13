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
├── DiamondFocusModule (optional downstream optical layer)
└── RuntimeMenuController
```

The runtime menu uses a Canvas under `RuntimeMenuController`. It builds three presentation surfaces:

- `MainControlPanel`: always-visible runtime controls.
- `DiagnosticsOverlay`: a separate read-only diagnostics panel toggled by middle mouse click.
- `HotkeysHelpPanel`: a separate help overlay toggled by `F1`.

`DiagnosticsModule` owns the diagnostic data, while the visible panels only read `KaleidoscopeState` and route mutations through `KaleidoscopeDirector`. The legacy `DebugHUD` component remains disabled in this scene to avoid duplicate overlays.

## Control Rules

- Runtime UI sends `KaleidoscopeCommand` objects to `KaleidoscopeDirector`.
- Runtime UI reads `KaleidoscopeState` for labels and control values.
- Runtime UI reads `KaleidoscopeDirector.FinalOutputTexture` for display.
- Runtime UI does not mutate shader, camera, tunnel, recording, physics, source, or mirror internals.
- Middle mouse click toggles diagnostics visibility through `SetDiagnosticsVisible`.
- `F1` routes `ToggleHotkeysHelp` through `KaleidoscopeDirector`; the visible help overlay is owned by `RuntimeMenuController`.
- `Backspace` routes `ToggleDiamondFocus` through `KaleidoscopeDirector`; the 3D crystal render/composite pass remains inside `DiamondFocusModule`.
- `Esc` hides an open overlay without changing module internals.

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
