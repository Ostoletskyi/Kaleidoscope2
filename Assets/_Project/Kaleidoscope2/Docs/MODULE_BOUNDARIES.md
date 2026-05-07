# MODULE_BOUNDARIES.md

This document is a strict architecture contract for Kaleidoscope2. Read it before implementing or modifying any runtime system.

## System Flow

All runtime behavior must preserve this direction:

```text
UI / Input / Audio
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

Modules may expose small public APIs, but they must not control another module's internals.

## Core

Responsibilities:

- Own `KaleidoscopeDirector`, `KaleidoscopeState`, commands, module registration, lifecycle, command routing, and validation.
- Hold the authoritative runtime state.
- Route commands to registered modules through `IKaleidoscopeModule`.

Allowed dependencies:

- Unity runtime APIs required for lifecycle.
- Module interfaces, command objects, and serialized module references.

Forbidden dependencies:

- Scene-object name searches as production logic.
- Direct shader, camera, recording, tunnel, audio, or physics implementation details.
- Global mutable production state outside `KaleidoscopeState`.

## Control

Responsibilities:

- Runtime UI, editor controls, sliders, buttons, toggles, presets, recording controls, debug controls.

Allowed dependencies:

- `KaleidoscopeDirector`
- `KaleidoscopeCommand`
- Read-only access to `KaleidoscopeState`

Forbidden dependencies:

- Direct shader writes.
- Direct camera manipulation.
- Direct physics, tunnel, source, mirror, recording, or audio internals.

## Input

Responsibilities:

- Convert keyboard, mouse, gamepad, and hotkey activity into commands.

Allowed dependencies:

- `KaleidoscopeDirector`
- `KaleidoscopeCommand`

Forbidden dependencies:

- Direct movement of cameras, source objects, physics objects, tunnel objects, or shader values.

## Source

Responsibilities:

- Produce or expose the source texture.
- Track source mode and source status.

Allowed dependencies:

- Core state and command contracts.
- Own source assets and source RenderTexture references.

Forbidden dependencies:

- Mirror, tunnel, recording, audio, UI, or camera internals.

## PhysicsChamber

Responsibilities:

- Physical visual content: gems, particles, rigidbodies, collisions, chamber movement, shake, material presets.

Allowed dependencies:

- Core state and command contracts.
- Own physical scene references.

Forbidden dependencies:

- Mirror, tunnel, recording, or UI internals.

## Mirror

Responsibilities:

- Classic kaleidoscope processing.
- Own mirror shader parameters and output RenderTexture generation.

Allowed dependencies:

- Core state and command contracts.
- Source texture input through explicit render pipeline calls.
- Own materials, shaders, and temporary resources.

Forbidden dependencies:

- Recording internals.
- Tunnel internals.
- UI or input direct access.
- Camera discovery through `Camera.main`.

## Camera

Responsibilities:

- Centralized camera roles: SourceCamera, ViewerCamera, RenderCamera, OfflineCamera, and future TunnelCamera when needed.
- Explicit camera references and RenderTexture assignment.

Allowed dependencies:

- Core state and command contracts.
- Serialized or bootstrap-injected camera references.

Forbidden dependencies:

- `Camera.main` for production logic.
- Hidden camera creation without registration.
- Multiple unrelated scripts enabling or disabling cameras independently.

## AudioReactive

Responsibilities:

- Audio analysis, beat detection, and visual command generation.

Allowed dependencies:

- `KaleidoscopeDirector`
- `KaleidoscopeCommand`
- Own audio source and analysis data.

Forbidden dependencies:

- Direct shader, camera, physics, tunnel, recording, or source mutation.

## Tunnel

Responsibilities:

- Optional downstream 3D projection of final kaleidoscope texture.

Allowed dependencies:

- Core state and command contracts.
- Final kaleidoscope texture provided explicitly by the render pipeline.
- Own tunnel mesh, material, camera if registered through Camera/Core rules.

Forbidden dependencies:

- Replacing the classic pipeline.
- Reading PhysicsChamber internals.
- Pulling source or mirror internals directly.

## Recording

Responsibilities:

- Deterministic recording/export of final output only.

Allowed dependencies:

- Core state and command contracts.
- Final output RenderTexture assigned explicitly.

Forbidden dependencies:

- `Camera.main`.
- Camera searches.
- Mirror, tunnel, source, or physics internals.
- Changing visual architecture to support export.

## Presets

Responsibilities:

- Data profiles for source, mirror, camera, audio, tunnel, quality, and recording settings.

Allowed dependencies:

- Core commands and state.

Forbidden dependencies:

- Applying settings directly to modules without Director routing.
- Hidden control logic inside preset data.

## Diagnostics

Responsibilities:

- Read current state, FPS, active modules, warnings, missing references, errors, mode, tunnel state, and recording state.

Allowed dependencies:

- Read-only state access.
- Explicit debug commands through Director when mutation is intentional.

Forbidden dependencies:

- Mutating production state from display code.
- Repairing missing references automatically.
- Searching the scene every frame.

## Render Pipeline Ownership

Future rendering must preserve this pipeline:

```text
Source Content
    |
    v
Source RenderTexture
    |
    v
MirrorModule
    |
    v
Kaleidoscope RenderTexture
    |
    v
Final Output RenderTexture
    |
    v
Display / Tunnel / Recording
```

Ownership rules:

- Source owns source texture generation.
- Mirror owns classic kaleidoscope processing.
- Tunnel consumes final kaleidoscope texture downstream.
- Recording consumes final output only.
- Camera owns camera roles and RenderTexture routing.
- Director orchestrates connections and commands; it does not implement shader, camera, tunnel, audio, physics, or recording internals.

## Communication Rules

Allowed communication:

- UI/Input/Audio send `KaleidoscopeCommand` objects to `KaleidoscopeDirector`.
- Director routes commands to registered `IKaleidoscopeModule` implementations.
- Modules synchronize user-facing values through `KaleidoscopeState`.
- Modules report warnings, errors, and missing references through state diagnostics.
- Bootstrap assigns serialized references explicitly.

Forbidden communication:

- `FindObjectOfType` in runtime loops.
- `GameObject.Find` for production logic.
- Scene object names as production dependencies.
- Singleton dumping grounds.
- Module-to-module hidden references.
- Static mutable state for runtime behavior.

## Safe Module Extension Rules

When adding a feature:

1. Identify the owning module.
2. Add state fields only if the value is user-facing or cross-system status.
3. Add commands when UI, input, audio, presets, or external control must request behavior.
4. Route commands through Director.
5. Keep direct references serialized, injected, or registered through Bootstrap.
6. Validate missing dependencies explicitly.
7. Add diagnostics for status and failure modes.
8. Do not implement a forbidden dependency as a workaround.

If a feature requires a forbidden dependency, stop and report the architecture violation before writing code.
