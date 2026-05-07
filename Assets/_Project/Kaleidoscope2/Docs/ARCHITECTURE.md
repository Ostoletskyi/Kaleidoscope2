# ARCHITECTURE.md

Kaleidoscope2 is a modular Unity visual engine. The project is organized as replaceable blocks controlled by a central director, not as a stack of unrelated scene scripts.

## Pipeline Overview

Runtime control follows one direction:

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

The director owns orchestration. Modules own implementation details. Diagnostics reads state and reports health.

## Module Responsibilities

Core:

- Owns `KaleidoscopeDirector`, `KaleidoscopeState`, `KaleidoscopeCommand`, `IKaleidoscopeModule`, and `KaleidoscopeBootstrap`.
- Registers modules, routes commands, ticks modules, synchronizes state, and validates global wiring.

Control:

- Owns runtime or editor control surfaces.
- Sends commands to the director and reads state for display.

Input:

- Converts physical input into commands.
- Does not move cameras, objects, shaders, or module internals directly.

Source:

- Owns source texture production and source mode status.
- Does not know how mirror, tunnel, or recording systems consume the result.

PhysicsChamber:

- Owns future physical content generation.
- Produces visual source content only.

Mirror:

- Owns classic kaleidoscope processing and mirror shader parameters.
- Consumes a source texture and produces a kaleidoscope texture.

Camera:

- Owns explicit camera roles: Source, Viewer, Render, Offline.
- Does not rely on `Camera.main` or scene object names.

AudioReactive:

- Owns audio analysis and emits commands/events through the director.
- Does not mutate visual modules directly.

Tunnel:

- Owns optional downstream 3D projection.
- Consumes final kaleidoscope texture and must not replace the classic pipeline.

Recording:

- Owns deterministic output capture.
- Consumes final output only and does not search for cameras.

Presets:

- Owns preset data and preset selection.
- Applies settings through director commands.

Diagnostics:

- Reads `KaleidoscopeState`, module status, FPS, warnings, missing references, and errors.
- Does not mutate production state except through explicit debug commands.

## Render Flow

The target rendering flow is:

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

Current implementation is foundation-only. Source generation, mirror rendering, tunnel rendering, and recording export are intentionally placeholders until their stages are implemented.

## Ownership Rules

- `KaleidoscopeState` is the authoritative user-facing runtime state.
- `KaleidoscopeDirector` is the only command router.
- `KaleidoscopeBootstrap` wires serialized module slots into the director.
- Modules expose small public APIs and report diagnostics through state.
- Camera references are owned by `CameraModule`; production logic must not call `Camera.main`.
- Shader property IDs belong to the module that owns the shader.
- Recording consumes final output only.
- Tunnel consumes final kaleidoscope texture downstream.

## Bootstrap Structure

Bootstrap registration is module-based:

```text
KaleidoscopeBootstrap
    |
    v
Module registration slots
    |
    v
KaleidoscopeDirector.RegisterModule(IKaleidoscopeModule)
```

Each slot declares the module area it represents and the serialized module component assigned to that slot. This keeps startup wiring explicit without making modules discover each other.

Legacy flat bootstrap arrays are supported only as a migration fallback. New scenes should use module registration slots.

## Forbidden Coupling

Do not introduce:

- UI directly editing shaders, cameras, physics, recording, tunnel, or source internals.
- AudioReactive directly editing visual modules.
- Recording using `Camera.main` or camera searches.
- Tunnel replacing Source to Mirror to Final Output flow.
- Runtime loops using `FindObjectOfType` or `GameObject.Find`.
- Scene object names as production dependencies.
- Global mutable production state outside `KaleidoscopeState`.

If a feature seems to require forbidden coupling, stop and redesign the boundary before implementation.
