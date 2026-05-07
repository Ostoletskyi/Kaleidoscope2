# AGENTS.md — Kaleidoscope 2

## 1. Project Goal

Kaleidoscope 2 is a modular Unity project for building a cinematic, music-reactive kaleidoscope engine.

The project must be developed as a block-based architecture where every system is isolated, testable, replaceable, and controlled through a centralized director.

This is not a collection of random MonoBehaviour scripts.  
This is a structured visual engine.

---

## 2. Core Architecture Rule

All systems must follow this flow:

UI / Input / Audio
        ↓
KaleidoscopeDirector
        ↓
Independent Modules
        ↓
Final Render Output

No module may directly control another module’s internal implementation.

Allowed:

- UI sends commands to Director.
- Input sends commands to Director.
- Audio sends events to Director.
- Director routes commands to modules.
- Modules expose public methods through clear contracts.
- Modules report status/events back through defined interfaces or event bus.

Forbidden:

- UI directly changes shader parameters.
- UI directly changes cameras.
- Audio module directly changes MirrorModule internals.
- Recording module searches for Camera.main.
- Tunnel module rewrites classic kaleidoscope pipeline.
- Any module uses FindObjectOfType in Update.
- Any runtime system depends on scene object names as production logic.

---

## 3. Required Top-Level Modules

The project must be organized around these modules:

### Core

Responsible for:

- KaleidoscopeDirector
- KaleidoscopeState
- module registration
- command routing
- lifecycle management
- global validation
- event bus

Main files:

- `KaleidoscopeDirector.cs`
- `KaleidoscopeState.cs`
- `KaleidoscopeBootstrap.cs`
- `IKaleidoscopeModule.cs`
- `KaleidoscopeCommand.cs`

---

### Control

Responsible for:

- runtime UI
- editor control panel
- sliders/buttons/toggles
- preset selection
- recording buttons
- debug controls

Control must never directly modify shader, camera, physics, recording, or tunnel internals.

Control talks only to:

- `KaleidoscopeDirector`
- command API

---

### Input

Responsible for:

- keyboard
- mouse
- gamepad
- hotkeys
- WASD center movement
- mouse wheel zoom
- Space shake
- arrows rotation/twist

Input must convert physical user input into commands.

Input must not directly move cameras, physics objects, shaders, or tunnel objects.

---

### Source

Responsible for producing the source texture.

Supported source modes:

- physics chamber camera
- image texture
- video texture
- procedural texture
- external RenderTexture

The Source module exposes:

- current source texture
- source mode
- source status

It must not know how mirror rendering, recording, tunnel, or audio systems work.

---

### PhysicsChamber

Responsible for:

- gems
- particles
- Rigidbody simulation
- collision behavior
- rotating chamber
- shake/avalanche
- material presets
- physical visual entropy

PhysicsChamber produces visual content only.

It must not directly control:

- MirrorModule
- TunnelModule
- RecordingModule
- UI

---

### Mirror

Responsible for the classic kaleidoscope effect.

Responsibilities:

- radial symmetry
- mirror count
- segment angle
- rotation
- center offset
- zoom
- seam blending
- chromatic aberration
- vignette
- shader parameter application

MirrorModule receives source texture and outputs kaleidoscope RenderTexture.

It must not know whether the output is displayed, recorded, or projected into tunnel mode.

---

### Camera

Responsible for:

- source camera
- viewer camera
- render camera
- offline render camera
- camera rig setup
- RenderTexture assignment
- safe camera activation/deactivation

Do not use `Camera.main` for production logic.

Camera references must be explicit, serialized, injected, or registered through Bootstrap/Director.

---

### AudioReactive

Responsible for:

- audio analysis
- beat detection
- kick/snare/drop/build/break/silence events
- mapping audio events to visual commands

AudioReactive must emit events or commands.

It must not directly edit shader values, camera transforms, or physics objects unless routed through Director.

---

### Tunnel

Responsible for experimental 3D tunnel mode.

Responsibilities:

- tunnel mesh
- tunnel projection shader
- final kaleidoscope texture projection
- end cap
- seam feathering
- depth shading
- tunnel camera if needed

Tunnel mode must use the final kaleidoscope texture as input.

Tunnel mode must not break or rewrite the classic kaleidoscope pipeline.

Classic mode must remain fully functional when TunnelModule is disabled.

---

### Recording

Responsible for:

- preview recording
- offline frame export
- PNG sequence export
- ffmpeg assembly
- audio sync
- fixed FPS rendering
- final video output

Recording must consume final RenderTexture only.

Recording must not decide how the visual scene works.

Recording must not rely on `Camera.main`.

---

### Presets

Responsible for:

- saving/loading preset data
- global visual profiles
- mirror settings
- color settings
- camera settings
- audio-reactive intensity
- tunnel settings
- recording profiles

Presets are data, not control logic.

Preset application must go through Director.

---

### Diagnostics

Responsible for:

- debug HUD
- FPS
- memory usage
- RenderTexture status
- active modules
- current mode
- warnings
- validation reports
- missing references

Diagnostics may read state and module status.

Diagnostics must not mutate production state except through explicit debug commands.

---

## 4. Required Folder Structure

Use this structure unless there is a strong reason not to:

Assets/_Project/Kaleidoscope2/
│
├── Core/
├── Control/
├── Input/
├── Source/
├── PhysicsChamber/
├── Mirror/
├── Camera/
├── AudioReactive/
├── Tunnel/
├── Recording/
├── Presets/
├── Diagnostics/
├── Shaders/
├── Materials/
├── Textures/
├── Scenes/
└── Docs/

Do not place unrelated systems in the same folder.

Do not create one giant Scripts folder.

---

## 5. Dependency Direction

Dependencies must flow downward:

Control/Input/Audio
        ↓
Core/Director
        ↓
Modules
        ↓
Rendering/Output

Allowed dependency examples:

- Control → Core
- Input → Core
- AudioReactive → Core
- Core → Module interfaces
- Module → its own internal helpers
- Recording → final output interface

Forbidden dependency examples:

- Mirror → Recording
- Recording → Mirror internals
- Tunnel → PhysicsChamber internals
- AudioReactive → Mirror shader directly
- UI → Camera directly
- PhysicsChamber → UI
- Source → Recording

---

## 6. Communication Rules

All cross-module communication must use one of these:

1. `KaleidoscopeDirector`
2. command objects
3. event bus
4. explicit interfaces
5. serialized references assigned by Bootstrap

Avoid hidden communication.

Forbidden:

- global static state for production logic
- `FindObjectOfType` in runtime loops
- `GameObject.Find` for core systems
- magic scene names
- hidden dependencies through singleton abuse

Singletons are allowed only for Bootstrap-level access and must not become dumping grounds.

---

## 7. State Management

There must be one authoritative state object:

`KaleidoscopeState`

It stores:

- active source mode
- active visual mode
- mirror settings
- camera settings
- tunnel enabled/disabled
- recording state
- active preset
- runtime quality level
- current warnings/errors

Modules may keep internal cache, but user-facing state must be synchronized with `KaleidoscopeState`.

UI must read from State, not guess values.

---

## 8. Rendering Pipeline

The required rendering flow:

Source Content
    ↓
Source RenderTexture
    ↓
MirrorModule
    ↓
Kaleidoscope RenderTexture
    ↓
Final Output RenderTexture
    ↓
Display / Tunnel / Recording

Tunnel mode must be downstream from final kaleidoscope output.

Recording must record final output, not random scene cameras.

---

## 9. Mode Rules

The project must support multiple modes without breaking the core pipeline:

- Classic Kaleidoscope Mode
- Physics Chamber Source Mode
- Image Source Mode
- Procedural Source Mode
- Audio Reactive Mode
- Experimental Tunnel Mode
- Offline Recording Mode

Modes must be composable where possible.

Example:

Classic + Audio Reactive + Recording  
Classic + Tunnel + Recording  
Physics Source + Classic + Audio Reactive

Do not hardcode one mode in a way that disables all others.

---

## 10. Shader Rules

Shader parameters must be controlled through dedicated modules.

Allowed:

- MirrorModule sets mirror shader parameters.
- TunnelModule sets tunnel shader parameters.
- PostFX module sets post-processing parameters.

Forbidden:

- UI directly calls material.SetFloat.
- Input directly edits shader values.
- AudioReactive directly edits material parameters.

Shader property names must be constants.

Example:

public static class MirrorShaderIds
{
    public static readonly int MirrorCount = Shader.PropertyToID("_MirrorCount");
    public static readonly int CenterOffset = Shader.PropertyToID("_CenterOffset");
}

---

## 11. Camera Rules

Camera logic must be centralized in CameraModule.

Forbidden:

- production logic using `Camera.main`
- recording logic guessing active camera
- multiple scripts enabling/disabling cameras independently
- hidden camera creation without registration

Every production camera must have a clear role:

- SourceCamera
- ViewerCamera
- RenderCamera
- OfflineCamera
- TunnelCamera if needed

---

## 12. Recording Rules

Recording must be deterministic where possible.

Required principles:

- fixed target FPS
- stable frame count
- no dependency on editor preview timing
- offline frame export must not rely on variable Update timing
- ffmpeg assembly must be separated from frame rendering

Recording pipeline:

1. prepare scene
2. validate cameras and RenderTextures
3. render frame sequence
4. export PNG sequence
5. assemble final video with ffmpeg
6. report result

Recording must not change visual architecture.

---

## 13. Performance Rules

Avoid garbage generation in Update.

Forbidden in Update/LateUpdate/FixedUpdate:

- `FindObjectOfType`
- `GameObject.Find`
- LINQ allocations
- repeated material instantiation
- repeated RenderTexture allocation
- string-heavy logging every frame

RenderTextures must be reused where possible.

Temporary resources must be released safely.

---

## 14. Unity Serialization Rules

Preserve serialized fields.

Do not rename serialized fields without migration.

Do not break prefab or scene references.

When renaming classes/files:

- keep Unity file/class naming consistent
- check references
- document migration risk

---

## 15. Validation Rules

After every implementation step, provide a report with:

### What changed
Short explanation.

### Why
Reason for the change.

### Files touched
List of changed files.

### Architecture impact
Explain whether module boundaries were preserved.

### Validation
Mention what was checked.

### Risks / Follow-up
Mention remaining risks.

---

## 16. Development Stage Rules

Do not rewrite the entire project in one step.

Use staged development.

Recommended stages:

### Stage 00 — Architecture Audit

- inspect current project
- map dependencies
- find direct cross-module calls
- find Camera.main usage
- find shader writes outside modules
- produce report only
- do not modify code

### Stage 01 — Core + Director

- create Core folder
- add Director
- add State
- add command structure
- add module interface

### Stage 02 — Input Routing

- route keyboard/mouse/gamepad through InputModule
- remove direct input-to-shader/camera/physics writes

### Stage 03 — Source + Mirror Separation

- isolate source texture generation
- isolate mirror rendering
- define source-to-mirror pipeline

### Stage 04 — Camera Module

- centralize cameras
- remove Camera.main dependencies
- validate RenderTexture assignments

### Stage 05 — Physics Chamber Module

- isolate physical gem chamber
- expose only source texture/status/shake commands

### Stage 06 — Audio Reactive Module

- convert audio analysis to events/commands
- no direct visual mutation

### Stage 07 — Tunnel Module

- add tunnel as downstream projection mode
- preserve classic mode

### Stage 08 — Recording Module

- consume final output RenderTexture
- add deterministic offline export

### Stage 09 — Presets + Diagnostics

- add preset system
- add debug HUD
- add validation reports

---

## 17. Coding Style

Use clear class names.

Prefer:

- `MirrorModule`
- `CameraModule`
- `RecordingModule`
- `KaleidoscopeDirector`
- `KaleidoscopeState`

Avoid vague names:

- `Manager`
- `Controller2`
- `NewScript`
- `Helper`
- `TempFix`
- `MainLogic`

A class name must explain its responsibility.

---

## 18. Public API Rules

Each module must expose a small public API.

Example:

MirrorModule may expose:

- `SetMirrorCount(int count)`
- `SetRotationSpeed(float speed)`
- `SetCenterOffset(Vector2 offset)`
- `SetZoom(float zoom)`
- `Render(RenderTexture source, RenderTexture target)`

MirrorModule must not expose all internal materials, cameras, and temporary objects unless necessary.

---

## 19. Error Handling

Modules must fail clearly.

If a dependency is missing:

- log a clear warning/error
- report to Diagnostics
- do not silently fail
- do not create hidden fallback behavior unless documented

Example:

`[MirrorModule] Missing source RenderTexture. Mirror rendering skipped.`

---

## 20. Documentation Rules

Every module folder should contain short documentation if the module becomes complex.

Recommended docs:

- `Docs/ARCHITECTURE.md`
- `Docs/RENDER_PIPELINE.md`
- `Docs/MODULE_BOUNDARIES.md`
- `Docs/RECORDING_PIPELINE.md`
- `Docs/TUNNEL_MODE.md`

Documentation must explain:

- responsibility
- inputs
- outputs
- dependencies
- forbidden interactions

---

## 21. Absolute Architecture Prohibitions

Never do these without explicit approval:

- rewrite the full project at once
- merge modules into one large controller
- let UI directly control shader/camera/physics
- let AudioReactive directly control shader/camera/physics
- let Recording search for cameras dynamically
- make Tunnel Mode replace the classic pipeline
- hide production logic in editor-only scripts
- create circular dependencies
- introduce global mutable state without Director/State

---

## 22. Preferred Mental Model

Think of the project as a hardware rack:

- Control Panel = operator panel
- Director = central controller / dispatcher
- State = system status memory
- Source = signal generator
- Mirror = image processor
- Camera = optical routing
- AudioReactive = signal analyzer
- Tunnel = optional projection device
- Recording = output recorder
- Diagnostics = monitoring panel

Each block has cables, but blocks are not soldered into each other.

---

## 23. Final Rule

When in doubt, preserve modularity.

A quick fix that breaks architecture is not acceptable.

A slower fix that keeps module boundaries clear is preferred.

The project must remain understandable, expandable, and safe to modify.