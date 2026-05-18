# AGENTS.md — KAELIS

# 1. Project Goal

KAELIS is a modular Unity-based cinematic kaleidoscope engine.

Core principle:

UI / Input / Audio
        ↓
KaleidoscopeDirector
        ↓
Independent Modules
        ↓
Final Output

Every module must remain isolated, replaceable, and testable.

Do not build a chaotic MonoBehaviour jungle.

2. Core Architecture Rules

Allowed:

UI → Director
Input → Director
Audio → Director
Director → Modules
Modules communicate through:
commands
interfaces
event bus

Forbidden:

UI directly edits shaders/cameras/materials
Modules directly mutate other modules
FindObjectOfType in runtime loops
GameObject.Find as production architecture
Camera.main as production dependency
hidden singleton logic
circular dependencies

3. Layer Separation (CRITICAL)

KAELIS has TWO different rendering layers.

Layer 1 — Kaleidoscope Display Layer

Pipeline:

Source
    ↓
Mirror
    ↓
FinalOutputTexture
    ↓
OutputPreview

Purpose:
stable kaleidoscope display pipeline.

Layer 1 is READ-ONLY during RealMesh3D tasks.

Do NOT:

modify MirrorModule
modify SourceModule
modify OutputPreview
convert display plane into crystal
treat fullscreen output as 3D crystal
Layer 2 — Crystal3D Stage Layer

Purpose:
real volumetric crystal rendering.

Correct composition:

Camera
    ↓
Crystal
    ↓
Background

NOT:

Camera
    ↓
Fullscreen plane
    ↓
Tiny crystal

Required:

real MeshFilter + MeshRenderer
visible depth
side faces
real lighting
explicit background geometry
explicit camera
explicit light rig

Forbidden:

Plane as crystal
Quad as crystal
Billboard as crystal
RawImage as crystal
fullscreen fake projection
screen-space fake depth

If the crystal is flat:
it is NOT RealMesh3D.

4. Current Repository State

IMPORTANT:

Current DiamondFocusModule is a LEGACY HYBRID compositor.

It may contain:

real mesh
optics
compositing

BUT:

it is NOT True CrystalStage3D.

Fullscreen composite ≠ true 3D stage.

Do not keep trying to solve true spatial 3D purely through fullscreen compositing tricks.

5. Crystal Modes

Two supported crystal strategies:

Billboard2D
RealMesh3D
Billboard2D

Performance mode.

Allowed:

quad
billboard
fake optics
screen-space effects

This is intentionally fake.

RealMesh3D

Premium mode.

Hard requirements:

volumetric mesh
real side faces
real depth
visible from side angle
separate from background plane
real lighting interaction

Validation:

If disabling shaders leaves only a flat surface:
FAIL.

6. Menu Rules

Menu is a PREMIUM COMMERCIAL UI.

Not a debug panel.

Before menu tasks inspect:

Assets/_Project/Kaleidoscope2/Menu/

Menu architecture:

Menu
    ↓
MenuDirector
    ↓
KaleidoscopeDirector

Menu must NOT directly mutate:

shaders
cameras
crystal internals
recording internals

Required quality:

clean layout
TextMeshPro
hover states
keyboard support
touch support
consistent spacing
premium presentation

7. Top-Level Modules

Required modules:

Menu
Core
Control
Input
Source
Mirror
Camera
PhysicsChamber
AudioReactive
Tunnel
DiamondFocus
Recording
Presets
Diagnostics

8. DiamondFocus Responsibility

DiamondFocus currently acts as:

Legacy Hybrid Crystal Compatibility Layer

Responsibilities:

Billboard2D
temporary RealMesh integration
diagnostics
compatibility

NOT final True CrystalStage3D.

Future true architecture:

DiamondFocus
    ↓
ICrystalRenderer
    ├── BillboardCrystalRenderer
    └── RealMeshCrystalRenderer
9. Performance Rules

Forbidden in Update:

allocations
LINQ
FindObjectOfType
GameObject.Find
material recreation
RenderTexture recreation

Reuse resources.

10. Validation Rules

Every task report must include:

What changed
Why
Files touched
Validation
Risks

For RealMesh3D tasks additionally confirm:

mesh is volumetric
side faces visible
not a plane
Layer 1 untouched
Billboard2D still works
11. Stage Discipline

Only work on requested stage.

Do NOT:

implement future stages
rewrite project globally
“improve unrelated things”

Small isolated commits only.

12. Mental Model

Think like hardware rack:

Menu = front panel
Director = controller
Source = signal generator
Mirror = image processor
Layer 1 = display monitor
Layer 2 = crystal optical stage
Recording = recorder
Diagnostics = monitoring panel

Modules are connected.
Not fused together.

13. Final Rule

Protect modularity.

A quick hack that destroys architecture is failure.

A slower clean solution is preferred.