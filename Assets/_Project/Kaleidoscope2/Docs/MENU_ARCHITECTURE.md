# MENU_ARCHITECTURE.md

Project: Kaleidoscope2 / KAELIS

Product title: KAELIS - Optical Experience Engine

This document maps the intended KAELIS menu hierarchy to the current modular Unity runtime. It is a planning document only. It does not implement runtime menu UI, scene loading, effects, or refactors.

## Architecture Rule

Menu actions must preserve the project command flow:

```text
Menu UI / Input / Audio
        |
        v
KaleidoscopeDirector
        |
        v
Independent Modules
        |
        v
Final Render Output
```

The future menu must not directly edit shaders, cameras, source providers, recording internals, or scene objects. Menu buttons emit commands to `KaleidoscopeDirector`; modules own behavior.

## Status Definitions

| Status | Meaning |
|---|---|
| ACTIVE | Feature exists and can be connected to UI now. |
| PARTIAL | A module or prototype exists, but it needs cleanup, UI binding, polish, or production hardening. |
| PLACEHOLDER | A menu entry or state hook exists but should not perform real logic yet. |
| FUTURE | Not implemented yet; should remain disabled or marked as Coming Soon. |

## Current Code Evidence

| Area | Evidence | Classification |
|---|---|---|
| Core / Director | `Core/KaleidoscopeDirector.cs`, `KaleidoscopeState.cs`, `KaleidoscopeCommand.cs`, `KaleidoscopeBootstrap.cs` | ACTIVE |
| State | `KaleidoscopeState` stores visual mode, source mode, mirror, tunnel, 5D, 6D, 7D, diamond, diagnostics, recording, preset, and display flags | ACTIVE |
| SourceModule | `Source/SourceModule.cs` with procedural fallback plus image file/folder slideshow via legacy `Assets/_Project/Scripts/ImageSource` helpers | PARTIAL |
| MirrorModule | `Mirror/MirrorModule.cs` processes source texture into kaleidoscope output, mirror count, rotation, zoom, guides, motion/reanimation | ACTIVE |
| CameraModule | `Camera/CameraModule.cs` stores explicit camera roles and validates references | ACTIVE |
| DiagnosticsModule | `Diagnostics/DiagnosticsModule.cs`, `DebugHUD.cs` | ACTIVE |
| Runtime UI prototype | `Control/RuntimeMenuController.cs` builds a runtime control panel and file browser, not the final KAELIS main menu | PARTIAL |
| Input hotkey routing | `Input/InputModule.cs` emits commands for modes, mirrors, 4D/5D/6D/7D, audio, diamond, help, second display | PARTIAL |
| 4D prototype | `KaleidoscopeVisualMode.Hose`, `TunnelModule`, hose controls and chromatic aberration | PARTIAL |
| 5D prototype | `KaleidoscopeVisualMode.FiveD`, `TunnelModule` Mobius/flight controls, source image switching | PARTIAL |
| 6D optics prototype | `SixD/DepthWarpModule.cs`, `OpticalLookModule.cs`, `VolumetricIllusionModule.cs` | PARTIAL |
| 7D prototype | `SevenD/SevenDModule.cs` with strategy switching | PARTIAL |
| Diamond Focus prototype | `DiamondFocus/DiamondFocusModule.cs`, real 3D crystal pass, blur, shape/material controls | PARTIAL |
| Background blur replacement | `DiamondFocus/Shaders/DiamondBackgroundBlur.shader` and diamond composite path | PARTIAL |
| Second display output | `DisplayOutput/SecondDisplayOutputModule.cs`, F12 command/state | ACTIVE |
| AudioReactive | File/folder music playback exists; beat/reactive analysis is not implemented | PARTIAL |
| Presets | `Presets/PresetModule.cs` only stores active preset state/status | PLACEHOLDER |
| Recording | `Recording/RecordingModule.cs` only stores recording status and validates final texture | PLACEHOLDER |
| PhysicsChamber | `PhysicsChamber/PhysicsChamberModule.cs` status-only placeholder | PLACEHOLDER |
| Commercial KAELIS main menu | Art assets exist under `Menu/`, but no MenuModule or scene loading module exists | FUTURE |

## KAELIS Menu Hierarchy

### 1. ENTER EXPERIENCE

Purpose: starts or enters the main runtime scene/visual engine.

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| ENTER EXPERIENCE | Enter the runtime visual engine from the future commercial main menu. | MenuModule / SceneLoadingModule | FUTURE | `LoadRuntimeScene` | No MenuModule or async scene loading module exists yet. |
| Classic Mode | Start runtime in classic mirror kaleidoscope mode. | MirrorModule | ACTIVE | `SetVisualMode(Classic)` | Uses Source -> Mirror -> Final Output pipeline. |
| 4D Mode | Start runtime in 4D hose curvature mode. | TunnelModule / Hose state | PARTIAL | `SetVisualMode(Hose)` + `SetTunnelEnabled(true)` | Implemented as shader/post-process tunnel/hose prototype, not a separate production 4D module. |
| 5D Mode | Start runtime in Mobius/kaleidoscope flight mode. | TunnelModule / FiveDSettings | PARTIAL | `SetVisualMode(FiveD)` + `SetTunnelEnabled(true)` | Uses 5D flight time and source image switching; still prototype-level. |
| 6D Mode | Start runtime with pseudo-depth optical passes. | DepthWarpModule / OpticalLookModule / VolumetricIllusionModule | PARTIAL | `SetVisualMode(SixD)` | Downstream processors exist, but full 6D depth reconstruction remains future work. |
| Audio Reactive | Start runtime with music playback and future reactive visuals. | AudioReactiveModule | PARTIAL | `SetAudioFilePath`, `SetAudioFolderPath`, future `SetAudioReactiveEnabled` | Playback exists; beat detection and visual mapping are not present. |

Runtime behavior: a future menu scene should load `Kaleidoscope2_Main.unity` asynchronously, then dispatch the selected visual/source commands after bootstrap. Current runtime mode switching is handled by `RuntimeMenuController` and `InputModule`.

Input/command contract: menu buttons dispatch commands only through `KaleidoscopeDirector`. Scene loading should be owned by a future `SceneLoadingModule` or `MenuModule`.

Notes / risks: do not let a main menu button directly activate scene objects or edit module fields.

### 2. MODES

Purpose: selects the visual/rendering mode and source/display mode.

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| MODES | Open grouped visual/source/display selectors. | MenuModule / Control | FUTURE | None yet | Runtime control panel exists, but no KAELIS menu module. |
| Visual Modes | Container for visual mode choices. | Core state / MenuModule | PARTIAL | `SetVisualMode(...)` | Enum exists: Classic, Tunnel, Hose, FiveD, SixD, SevenD. |
| Visual Modes / Classic | Classic 2D radial mirror mode. | MirrorModule | ACTIVE | `SetVisualMode(Classic)` | Current default mode. |
| Visual Modes / 4D | Hose/curvature mode. | TunnelModule | PARTIAL | `SetVisualMode(Hose)` | Prototype with curvature, opening, wall profile, chromatic toggle. |
| Visual Modes / 5D | Mobius flight tunnel. | TunnelModule | PARTIAL | `SetVisualMode(FiveD)` | Prototype with flight speed and image shake/advance. |
| Visual Modes / 6D | Pseudo-depth optical pipeline. | SixD modules | PARTIAL | `SetVisualMode(SixD)` | DepthWarp, OpticalLook, VolumetricIllusion stages exist. |
| Visual Modes / Experimental | Experimental visual strategy mode. | SevenDModule | PARTIAL | `SetVisualMode(SevenD)` | Current experimental candidate is 7D strategy shader. Menu label needs final naming. |
| Source Modes | Container for source selection. | SourceModule | PARTIAL | `SetSourceMode(...)` | State enum exists; only procedural fallback and images are implemented. |
| Source Modes / Image Folder | Select an image folder and start slideshow. | Runtime file browser + SourceModule | ACTIVE | `SetImageFolderPath`, `SetSourceMode(ImageTexture)` | File browser and slideshow exist. |
| Source Modes / Video Source | Use video as source texture. | SourceModule | FUTURE | `SetSourceMode(VideoTexture)` | Enum value exists but no video loader. |
| Source Modes / Procedural | Use generated fallback texture. | SourceModule | ACTIVE | `SetSourceMode(ProceduralTexture)` | Fallback gradient texture exists. |
| Source Modes / Camera Feed | Use camera or live feed as source. | SourceModule / CameraModule | FUTURE | `SetSourceMode(ExternalRenderTexture)` or future command | Camera roles exist, but source feed wiring is not implemented. |
| Source Modes / Shader Generated | Use a shader-generated source. | SourceModule | FUTURE | Future `SetShaderSource` | No dedicated shader source module yet. |
| Display Modes | Container for window/output modes. | DisplayOutputModule / future Settings | PARTIAL | Mixed | Second display exists; fullscreen/window/VR/projection need modules. |
| Display Modes / Fullscreen | Switch app fullscreen. | SettingsModule | FUTURE | Future `SetFullscreen(true)` | No settings module currently owns Screen API. |
| Display Modes / Windowed | Switch app windowed mode. | SettingsModule | FUTURE | Future `SetFullscreen(false)` | No current command. |
| Display Modes / VR | Enable VR/XR display. | VRModule | FUTURE | Future `SetVrEnabled` | No XR module. |
| Display Modes / Projection | Output final render to external projection/display. | DisplayOutputModule | PARTIAL | `ToggleSecondDisplayOutput` / `SetSecondDisplayOutputEnabled` | F12 second monitor output exists; projection mapping is future. |

Runtime behavior: mode selection should only update state through commands. Source and display changes must remain owned by Source/Display modules.

Input/command contract: `SetVisualMode`, `SetSourceMode`, `SetImageFolderPath`, `ToggleSecondDisplayOutput`.

Notes / risks: `SourceModule` depends on helper code in `Assets/_Project/Scripts`, outside the preferred module folder layout. It works, but should be migrated later without changing behavior.

### 3. OPTICS

Purpose: controls optical post-processing and visual enhancement layers.

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| OPTICS | Open optical enhancement controls. | MenuModule / Control | FUTURE | None yet | Runtime panel has a few optical toggles, not a full optics panel. |
| Bloom | Control global bloom. | Future PostFXModule | FUTURE | Future `SetBloom` | Diamond shader has bloom-like highlights, but no global bloom module. |
| Chromatic Aberration | Toggle chromatic optics. | TunnelModule / DiamondFocus / future PostFX | PARTIAL | `ToggleTunnelHoseChromaticAberration`, future global command | 4D hose and diamond have CA; no unified global effect. |
| Depth Of Field / Background Blur | Blur background while keeping focus objects sharp. | DiamondFocusModule | PARTIAL | Driven by diamond rotation; future `SetBackgroundBlur` | Clean blur exists in Diamond Focus only. |
| Color Grading | Apply global color look. | Future PostFXModule | FUTURE | Future `SetColorGrade` | No global color grading module. |
| Refraction | Control optical refraction. | DiamondFocusModule / TunnelModule | PARTIAL | `AdjustDiamondRefractionIndex`, `SetDiamondRefractionIndex` | Diamond IOR exists; no global refraction panel. |
| Atmosphere | Haze/volumetric atmosphere. | VolumetricIllusionModule | PARTIAL | `SetVisualMode(SixD)` plus future per-parameter commands | 6D volumetric shader stage exists but has no UI binding. |
| Diamond Focus | Central 3D crystal, shape/material/rotation, background blur. | DiamondFocusModule | PARTIAL | `ToggleDiamondFocus`, `NextDiamondShape`, `PreviousDiamondShape`, `CycleDiamondMaterialMode`, diamond rotation/speed commands | Prototype is substantial, but still needs production UI and visual QA. |
| Depth Warp / 6D Optics | Pseudo-depth downstream optical layer. | DepthWarpModule / OpticalLookModule / VolumetricIllusionModule | PARTIAL | `SetVisualMode(SixD)`, future `Toggle6DDepthWarp` | Current processors exist; full AI depth projection is future. |

Runtime behavior: optics controls should act as downstream texture processors after the classic mirror output. Disabling any optics module must leave Classic mode functional.

Input/command contract: existing commands are mostly hotkey-driven. A future optics menu should add explicit parameter commands instead of editing shader material fields directly.

Notes / risks: avoid turning the optics panel into a direct shader editor. Each effect needs a module-owned API.

### 4. PRESETS

Purpose: loads/saves grouped visual configurations.

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| PRESETS | Open preset browser. | PresetModule | PLACEHOLDER | `SetActivePreset` | Module exists but stores only a preset name. |
| Cinematic | Built-in cinematic look. | PresetModule | FUTURE | `SetActivePreset("Cinematic")` | No preset asset/data. |
| Hypnotic | Built-in hypnotic motion look. | PresetModule | FUTURE | `SetActivePreset("Hypnotic")` | No preset asset/data. |
| Crystal | Built-in crystal/diamond look. | PresetModule | FUTURE | `SetActivePreset("Crystal")` | No preset asset/data. |
| Neon | Built-in neon look. | PresetModule | FUTURE | `SetActivePreset("Neon")` | No preset asset/data. |
| Deep Space | Built-in cosmic look. | PresetModule | FUTURE | `SetActivePreset("Deep Space")` | No preset asset/data. |
| Minimal | Built-in minimal look. | PresetModule | FUTURE | `SetActivePreset("Minimal")` | No preset asset/data. |
| Psychedelic | Built-in intense color look. | PresetModule | FUTURE | `SetActivePreset("Psychedelic")` | No preset asset/data. |
| User Presets | Container for user preset management. | PresetModule | FUTURE | Future preset commands | Persistence not implemented. |
| User Presets / Save Preset | Save current grouped settings. | PresetModule | FUTURE | Future `SavePreset` | Needs data schema and serialization. |
| User Presets / Load Preset | Load a user preset. | PresetModule | FUTURE | Future `LoadPreset` | Needs preset assets/files. |
| User Presets / Rename | Rename a user preset. | PresetModule | FUTURE | Future `RenamePreset` | Needs persistence model. |
| User Presets / Delete | Delete a user preset. | PresetModule | FUTURE | Future `DeletePreset` | Needs confirmation workflow. |

Runtime behavior: current placeholder can only display active preset state. Real preset application must dispatch grouped commands through Director.

Input/command contract: current `SetActivePreset` only changes state. Future commands should apply validated preset data through module APIs.

Notes / risks: do not hide behavior in preset data; preset application must remain explicit and observable.

### 5. RECORD & EXPORT

Purpose: captures final output as screenshots, image sequences, or video.

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| RECORD & EXPORT | Open capture/export workflow. | RecordingModule | PLACEHOLDER | `SetRecordingStatus(...)` | Module is status-only; no export pipeline. |
| Screenshot | Save current final output image. | RecordingModule | FUTURE | Future `CaptureScreenshot` | No screenshot writer. |
| PNG Sequence | Deterministic frame sequence export. | RecordingModule | FUTURE | Future `StartPngSequenceExport` | No frame loop/export path. |
| Video Export | Export video file. | RecordingModule | FUTURE | Future `StartVideoExport` | No ffmpeg assembly or audio sync. |
| GIF Export | Export GIF. | RecordingModule | FUTURE | Future `StartGifExport` | No GIF pipeline. |
| Resolution | Container for export size. | RecordingModule / SettingsModule | PLACEHOLDER | Future `SetExportResolution` | Render size exists in `CameraSettings`; export binding does not. |
| Resolution / 1080p | Export at 1920x1080. | RecordingModule | PLACEHOLDER | Future `SetExportResolution(1920,1080)` | Disabled until recording exists. |
| Resolution / 1440p | Export at 2560x1440. | RecordingModule | PLACEHOLDER | Future `SetExportResolution(2560,1440)` | Disabled until recording exists. |
| Resolution / 4K | Export at 3840x2160. | RecordingModule | PLACEHOLDER | Future `SetExportResolution(3840,2160)` | Disabled until recording exists. |
| Resolution / 8K | Export at 7680x4320. | RecordingModule | FUTURE | Future `SetExportResolution(7680,4320)` | Requires performance plan. |
| Frame Rate | Container for export FPS. | RecordingModule | PLACEHOLDER | Future `SetExportFrameRate` | No deterministic renderer yet. |
| Frame Rate / 30 FPS | Export at 30 FPS. | RecordingModule | PLACEHOLDER | Future `SetExportFrameRate(30)` | Disabled until recording exists. |
| Frame Rate / 60 FPS | Export at 60 FPS. | RecordingModule | PLACEHOLDER | Future `SetExportFrameRate(60)` | Disabled until recording exists. |
| Frame Rate / 120 FPS | Export at 120 FPS. | RecordingModule | FUTURE | Future `SetExportFrameRate(120)` | Requires performance validation. |
| Recording Quality | Container for quality profile. | Core state / RecordingModule | PLACEHOLDER | `SetQualityLevel(...)` | Quality enum exists; no export quality implementation. |
| Recording Quality / Preview | Preview quality target. | Core state | PLACEHOLDER | `SetQualityLevel(Preview)` | State only. |
| Recording Quality / High | High quality target. | Core state | PLACEHOLDER | `SetQualityLevel(High)` | State only. |
| Recording Quality / Ultra | Ultra quality target. | Core state | PLACEHOLDER | `SetQualityLevel(Ultra)` | State only. |
| Recording Quality / Cinematic | Offline/cinematic export target. | Core state | PLACEHOLDER | `SetQualityLevel(OfflineRender)` | Name should map to existing enum or add a new value later. |

Runtime behavior: all recording actions must consume Director final output only. No recording action may search for cameras or change visual modules directly.

Input/command contract: current command is `SetRecordingStatus`; future commands need deterministic export parameters.

Notes / risks: recording is intentionally not ready. Keep menu entries disabled until a real pipeline exists.

### 6. SETTINGS

Purpose: application-level configuration.

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| SETTINGS | Open application configuration. | Future SettingsModule | FUTURE | None yet | No SettingsModule exists. |
| Graphics | Graphics settings group. | Future SettingsModule | FUTURE | Future graphics commands | No central settings module. |
| Graphics / HDR | Toggle HDR rendering. | Future SettingsModule | FUTURE | Future `SetHdrEnabled` | No current command. |
| Graphics / Resolution Scale | Change render scale. | Future SettingsModule / modules | FUTURE | Future `SetResolutionScale` | Some modules have local renderScale fields, no central control. |
| Graphics / Anti-Aliasing | Configure AA. | Future SettingsModule | FUTURE | Future `SetAntiAliasing` | No current command. |
| Graphics / Reflections | Configure reflection quality. | DiamondFocus / future PostFX | FUTURE | Future `SetReflectionQuality` | Diamond has reflection-like shader response only. |
| Graphics / Post Processing | Enable/disable post stack. | Future PostFXModule | FUTURE | Future `SetPostProcessingEnabled` | No global post stack. |
| Audio | Audio settings group. | AudioReactiveModule / future SettingsModule | PARTIAL | Mixed | Playback exists; settings do not. |
| Audio / Master Volume | Set global volume. | Future AudioModule | FUTURE | Future `SetMasterVolume` | AudioSource volume is internal only. |
| Audio / UI Sounds | Toggle menu sounds. | MenuModule | FUTURE | Future `SetUiSoundsEnabled` | No UI sound system. |
| Audio / Reactive Audio | Enable beat/reactive analysis. | AudioReactiveModule | FUTURE | Future `SetAudioReactiveEnabled` | Playback exists, analysis does not. |
| Audio / Music Input | Select music file/folder. | Runtime file browser / AudioReactiveModule | PARTIAL | `SetAudioFilePath`, `SetAudioFolderPath` | Folder/file playback exists. |
| Controls | Input settings group. | InputModule / future RebindModule | PARTIAL | Existing hotkey commands | Input routing exists, but no rebind UI. |
| Controls / Keyboard | Keyboard hotkeys. | InputModule | ACTIVE | Existing commands | Physical hotkeys are implemented. |
| Controls / Mouse | Mouse control and menu toggle. | InputModule / Control | PARTIAL | `ToggleControlMenu` | Middle mouse menu toggle exists; no mouse settings panel. |
| Controls / Touch | Touch navigation. | Future InputModule extension | FUTURE | Future touch commands | Not implemented. |
| Controls / Gamepad | Gamepad navigation. | Future InputModule extension | FUTURE | Future gamepad commands | Not implemented. |
| Controls / Rebind Keys | Runtime key rebinding. | Future RebindModule | FUTURE | Future `SetKeyBinding` | Not implemented. |
| Interface | UI settings group. | Control / future SettingsModule | PARTIAL | Mixed | Runtime UI exists; no commercial settings. |
| Interface / UI Scale | Change UI scale. | Control / SettingsModule | PLACEHOLDER | Future `SetUiScale` | CanvasScaler exists but no exposed setting. |
| Interface / Minimal UI | Hide/show runtime controls. | RuntimeMenuController | PARTIAL | `ToggleControlMenu`, `SetControlMenuVisible` | Runtime panel visibility exists. |
| Interface / Diagnostics | Toggle diagnostics HUD. | DiagnosticsModule / DebugHUD | ACTIVE | `SetDiagnosticsVisible` | HUD and diagnostics state exist. |
| Interface / Tooltip Style | Configure tooltip behavior. | MenuModule | FUTURE | Future `SetTooltipStyle` | No tooltip system. |
| Performance | Performance settings group. | Future SettingsModule | FUTURE | Future performance commands | No performance module. |
| Performance / Dynamic Resolution | Enable dynamic resolution. | Future SettingsModule | FUTURE | Future `SetDynamicResolution` | Not implemented. |
| Performance / Upscaling | Enable upscaling. | Future SettingsModule | FUTURE | Future `SetUpscaling` | Not implemented. |
| Performance / GPU Limit | Limit GPU quality/load. | Future SettingsModule | FUTURE | Future `SetGpuLimit` | Not implemented. |
| Performance / Frame Limit | Limit frame rate. | Future SettingsModule | FUTURE | Future `SetFrameLimit` | Not implemented. |

Runtime behavior: settings must adjust state or module settings through explicit commands. Avoid direct calls to Unity `Screen`, `QualitySettings`, or `Application` from UI buttons unless owned by a SettingsModule.

Input/command contract: current controls are hotkey-only. Future SettingsModule should own application-level commands.

Notes / risks: settings are a common coupling trap. Keep them behind commands.

### 7. DIAGNOSTICS

Purpose: developer/operator visibility into runtime systems.

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| DIAGNOSTICS | Open diagnostics panel. | DiagnosticsModule / DebugHUD | ACTIVE | `SetDiagnosticsVisible`, `ValidateSystem`, `ClearDiagnostics` | Current HUD is developer-style. |
| FPS | Show smoothed FPS. | DiagnosticsModule | ACTIVE | Read `State.Diagnostics.FramesPerSecond` | Implemented. |
| GPU Usage | Show GPU usage/timing. | DiagnosticsModule | FUTURE | Future GPU profiler integration | Not implemented. |
| Render Pipeline | Show source/processor/final output chain. | KaleidoscopeDirector / Diagnostics | PARTIAL | Read module statuses and `FinalOutputTexture` | Final output exists; richer pipeline view is future. |
| Active Modules | Show registered modules and status. | KaleidoscopeDirector / DebugHUD | ACTIVE | Read `State.Diagnostics.ModuleStatuses` | Implemented. |
| Texture Memory | Show texture/RT memory usage. | DiagnosticsModule | FUTURE | Future memory diagnostics | Not implemented. |
| Optical Passes | Show active post/optical passes. | DiagnosticsModule / texture processors | PARTIAL | Read module statuses | Modules report status; no dedicated pass viewer. |
| Debug Views | Toggle debug visualizations. | Future DiagnosticsModule extension | FUTURE | Future debug view commands | Not implemented. |

Runtime behavior: diagnostics reads state and module statuses. It may issue explicit debug commands only when requested.

Input/command contract: `SetDiagnosticsVisible`, `ValidateSystem`, `ClearDiagnostics`.

Notes / risks: do not let diagnostics auto-repair runtime references.

### 8. ABOUT

Purpose: product identity and legal information.

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| ABOUT | Open product/legal information. | MenuModule | FUTURE | None | No About panel exists. |
| KAELIS | Product identity. | MenuModule | FUTURE | None | Menu art exists, not integrated. |
| Version | Display app version/build. | MenuModule / BuildInfoModule | FUTURE | Future `GetBuildInfo` | No version provider. |
| Credits | Display credits. | MenuModule | FUTURE | None | Not implemented. |
| Lex Nox Lab | Display studio identity. | MenuModule | FUTURE | None | Not implemented. |
| License | Display license/legal text. | MenuModule | FUTURE | None | Not implemented. |

Runtime behavior: read-only UI.

Input/command contract: none required except panel open/close commands.

Notes / risks: this should be data-driven later, not hardcoded across UI scripts.

### 9. EXIT

Purpose: quit workflow.

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| EXIT | Open quit workflow. | MenuModule / ApplicationLifecycleModule | FUTURE | Future `RequestExit` | No exit workflow exists. |
| Return To Desktop | Quit application. | ApplicationLifecycleModule | FUTURE | Future `QuitApplication` | Must be disabled or editor-safe in Play Mode. |
| Confirm Exit | Confirmation dialog. | MenuModule | FUTURE | Future `ConfirmExit` / `CancelExit` | No modal system exists. |

Runtime behavior: should show a confirmation before quitting.

Input/command contract: future application lifecycle commands.

Notes / risks: do not call `Application.Quit` directly from arbitrary buttons; route through a lifecycle owner.

## Required Technical Mapping

| Menu Item | Purpose | Owner Module | Status | Command | Notes |
|---|---|---|---|---|---|
| ENTER EXPERIENCE | Load or enter main runtime scene. | MenuModule / SceneLoadingModule | FUTURE | `LoadRuntimeScene` | Must use async loading later. |
| Classic Mode | Classic mirror kaleidoscope pipeline. | MirrorModule | ACTIVE | `SetVisualMode(Classic)` | Current default runtime path. |
| 4D Mode | Hose/curvature visual mode. | TunnelModule | PARTIAL | `SetVisualMode(Hose)` | Prototype in tunnel shader path. |
| 5D Mode | Mobius flight tunnel. | TunnelModule | PARTIAL | `SetVisualMode(FiveD)` | Prototype with image switching and flight speed. |
| 6D Mode | Pseudo-depth downstream optics. | DepthWarpModule / OpticalLookModule / VolumetricIllusionModule | PARTIAL | `SetVisualMode(SixD)` | Existing lightweight GPU passes; not full AI/depth pipeline. |
| 7D Experimental | Strategy-based kaleidoscope remapping. | SevenDModule | PARTIAL | `SetVisualMode(SevenD)`, `CycleSevenDStrategy` | Not in required top menu, but current code supports it. |
| Image Folder | Image slideshow source. | SourceModule | ACTIVE | `SetImageFolderPath`, `SetSourceMode(ImageTexture)` | Uses runtime file browser and slideshow helper. |
| Procedural | Generated fallback source. | SourceModule | ACTIVE | `SetSourceMode(ProceduralTexture)` | Currently a generated fallback gradient. |
| Video Source | Video source mode. | SourceModule | FUTURE | `SetSourceMode(VideoTexture)` | Enum only. |
| Camera Feed | External camera/source feed. | SourceModule / CameraModule | FUTURE | Future source command | Explicit cameras exist; feed is not wired. |
| Shader Generated | Shader source mode. | SourceModule | FUTURE | Future shader source command | No source shader module. |
| Projection | Second display/projection output. | DisplayOutputModule | PARTIAL | `ToggleSecondDisplayOutput` | F12 second monitor works; projection mapping is future. |
| Diamond Focus | Central crystal with blur/focus optics. | DiamondFocusModule | PARTIAL | `ToggleDiamondFocus`, shape/material/rotation commands | Substantial prototype, still needs production UI and polish. |
| Depth Warp / 6D Optics | Pseudo-3D depth-based optical layer. | DepthWarpModule / SixD modules | PARTIAL | Future `Toggle6DDepthWarp` | Current enablement is mode/state driven. |
| Recording | Export final output. | RecordingModule | PLACEHOLDER | `SetRecordingStatus`, future `StartRecording` / `StopRecording` | No export implementation. |
| Diagnostics | Runtime debug visibility. | DiagnosticsModule / DebugHUD | ACTIVE | `SetDiagnosticsVisible`, `ValidateSystem`, `ClearDiagnostics` | HUD and module statuses exist. |
| Presets | Load/save visual configurations. | PresetModule | PLACEHOLDER | `SetActivePreset` | Name-only placeholder. |
| Settings | Application-level configuration. | Future SettingsModule | FUTURE | Future settings commands | Not implemented. |
| About | Product/legal information. | MenuModule | FUTURE | None | Not implemented. |
| Exit | Quit workflow. | Future ApplicationLifecycleModule | FUTURE | Future quit commands | Not implemented. |

## Current vs Future Classification

### Current / Already Present

- Core / Director: ACTIVE.
- State: ACTIVE.
- SourceModule: PARTIAL overall; image folder and procedural fallback are ACTIVE.
- MirrorModule: ACTIVE.
- CameraModule: ACTIVE.
- DiagnosticsModule and DebugHUD: ACTIVE.
- Runtime UI prototype: PARTIAL.
- 4D prototype: PARTIAL.
- Diamond Focus prototype: PARTIAL.
- Background blur replacement: PARTIAL, present inside Diamond Focus.
- Second display output: ACTIVE.

### Partial

- Presets: PLACEHOLDER rather than functional, but command/state hooks exist.
- Recording: PLACEHOLDER rather than functional, but status hooks exist.
- AudioReactive: PARTIAL, because playback exists but reactive analysis does not.
- Tunnel / 5D: PARTIAL.
- 6D modules: PARTIAL.
- 7D module: PARTIAL.
- Menu system: PARTIAL only as runtime control panel assets/code, not final KAELIS main menu.
- Input hotkey routing: PARTIAL, broad command routing exists but no rebindable controls.

### Future

- Full 6D depth reconstruction and AI depth projection.
- True high-quality bokeh.
- Commercial main menu scene.
- Full rebindable controls.
- Real export pipeline.
- VR/projection mapping modes.
- Global settings module.
- Global post-processing stack.
- About/legal screens.
- Exit confirmation workflow.

## Recommended Implementation Order

1. MenuModule Foundation.
2. Main Menu Scene.
3. One perfect interactive button.
4. Async loading into runtime scene.
5. Mode selection binding.
6. Optics panel binding.
7. Presets panel.
8. Recording/export panel.
9. Settings panel.
10. Diagnostics integration.
11. Commercial polish pass.

Why: the menu must grow from one working vertical slice, not from a giant static UI mockup. The first slice should prove the full path: button focus/hover/click -> command -> director -> state/module response -> visual feedback. After that, the same contract can safely expand to modes, optics, presets, recording, settings, diagnostics, and polish without breaking the block architecture.

## Architecture Risks

- The current runtime control panel is useful but is not the final KAELIS main menu. Treat it as an operator panel prototype.
- Image source and file browser helpers live under `Assets/_Project/Scripts`, outside the preferred `Assets/_Project/Kaleidoscope2` module structure.
- Recording and presets are placeholders; their menu entries should be disabled until data/export pipelines exist.
- AudioReactive currently means music playback, not beat-driven visuals.
- 4D/5D are implemented through the Tunnel module. That is acceptable for the prototype, but future production docs should clarify whether they remain Tunnel-owned or become separate mode modules.
- 6D has real modular processors, but full depth reconstruction and AI depth are future work.
- Menu settings can easily become direct Unity API calls. Add a SettingsModule before wiring graphics/audio/performance controls.
