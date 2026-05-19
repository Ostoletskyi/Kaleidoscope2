# AGENTS.md — KAELIS

## 1. Project Goal

KAELIS is a modular Unity-based cinematic kaleidoscope engine.

Core flow:

```text
UI / Input / Audio
        ↓
KaleidoscopeDirector
        ↓
Independent Modules
        ↓
Final Output
```

Every module must remain isolated, replaceable, and testable.

Do not build a chaotic MonoBehaviour jungle.

---

## 2. Core Architecture Rules

Allowed:

- UI / Menu / Input / Audio send commands to KaleidoscopeDirector.
- KaleidoscopeDirector routes commands to modules.
- Modules communicate through commands, interfaces, or event bus.
- Modules expose small public APIs.
- Diagnostics may read state and report problems.

Forbidden:

- UI directly edits shaders, cameras, materials, or crystal internals.
- One module directly mutates another module’s internals.
- `FindObjectOfType` in runtime loops.
- `GameObject.Find` as production architecture.
- `Camera.main` as production dependency.
- hidden singleton logic.
- circular dependencies.
- unrelated “cleanup” during feature work.

---

## 3. Layer Separation

KAELIS has two different visual layers.

```text
Layer 1 — Kaleidoscope Display Layer
Layer 2 — Premium3D Crystal Stage Layer
```

These layers must not be confused.

### 3.1 Layer 1 — Kaleidoscope Display Layer

Purpose: render and display the classic kaleidoscope image.

Pipeline:

```text
Source
    ↓
Mirror
    ↓
FinalOutputTexture
    ↓
OutputPreview
```

Layer 1 is stable and must remain protected.

During Premium3D / RealMesh3D work, Layer 1 is READ-ONLY.

Do NOT:

- modify MirrorModule;
- modify SourceModule;
- modify OutputPreview;
- convert the display plane into a crystal;
- treat fullscreen output as RealMesh3D;
- break Classic2D / Billboard2D behavior.

### 3.2 Layer 2 — Premium3D Crystal Stage Layer

Purpose: render a real volumetric crystal as a premium 3D optical stage.

Correct composition:

```text
Camera
    ↓
RealMeshCrystal
    ↓
BackgroundGeometry
```

Incorrect composition:

```text
Camera
    ↓
Fullscreen plane
    ↓
Tiny crystal
```

Layer 2 must use:

- real MeshFilter + MeshRenderer crystal;
- real depth;
- side faces;
- front/back separation;
- explicit background geometry;
- explicit camera/stage framing;
- explicit light rig when lighting work is requested.

Forbidden for RealMesh3D:

- Plane as crystal;
- Quad as crystal;
- Billboard as crystal;
- RawImage as crystal;
- fullscreen fake projection as crystal;
- screen-space-only fake depth;
- huge projection wall pretending to be Premium3D.

If the crystal is flat, it is not RealMesh3D.

---

## 4. Current Repository State

Current `DiamondFocusModule` is a LEGACY HYBRID compositor.

It may contain:

- real mesh;
- optics;
- RenderTexture compositing;
- compatibility rendering.

But it is NOT final True CrystalStage3D.

Important:

```text
A volumetric mesh may exist,
while the final system still behaves like a 2D compositor.
```

Do not keep trying to solve True Premium3D purely through fullscreen compositing tricks.

Legacy DiamondFocus may remain as compatibility/fallback.

True Premium3D work must focus on the CrystalStage3D spatial path.

---

## 5. Crystal Modes

KAELIS supports two crystal strategies.

```text
Billboard2D — performance/fake crystal mode
RealMesh3D — premium volumetric crystal mode
```

### 5.1 Billboard2D

Billboard2D is intentionally fake and performance-friendly.

Allowed:

- quad;
- billboard;
- screen-space shader;
- fake facets;
- fake reflection/refraction.

Do not modify Billboard2D during RealMesh3D work unless explicitly requested.

### 5.2 RealMesh3D

RealMesh3D is the premium mode.

Hard requirements:

- volumetric mesh;
- visible side faces;
- meaningful depth;
- visible from side angle;
- centered subject;
- separate from background plane;
- remains volumetric with optical shaders disabled.

Validation:

If disabling shaders leaves only a flat surface, the implementation fails.

---

## 6. Premium3D Quality Gate — Size and Shape First

Before any Premium3D optical effects are allowed, the crystal must pass the Size & Shape Gate.

Required before effects:

- crystal occupies 42–48% of final visible Game View height;
- crystal is centered;
- no tiny crystal;
- no corner crystal;
- no duplicate crystal;
- default shape is visually accepted as premium;
- shape looks intentional, symmetrical, gemstone-like;
- shape has readable facets;
- Layer 1 remains unchanged.

Forbidden until this gate passes:

- caustics;
- prism/rainbow projection;
- radial waves;
- background pulsing;
- spotlight polish;
- sparkle/glint systems;
- presets;
- extra post-processing;
- new beauty effects.

If the crystal is still too small or ugly, fix size and shape first.

Do not add new effects to compensate for a weak crystal.

---

## 7. Premium3D Effects Rule

Effects are allowed only after Size & Shape Gate passes.

When effects are allowed:

- background must remain beautiful and full-strength;
- effects must enhance the crystal, not hide it;
- prism/rainbow/caustics must be crystal-driven, not random fullscreen noise;
- spotlight must support the crystal, not replace the scene;
- no pulsing/radial waves unless explicitly requested.

Effects must never be used to disguise broken scale, ugly geometry, or bad framing.

---

## 8. Input / Hotkey Safety

Do not steal existing hotkeys globally.

Crystal-specific hotkeys are active only when:

```text
Backspace has enabled crystal visibility
AND
Crystal visible == true
```

If the crystal is hidden, all keys must keep their original project behavior.

Crystal hotkeys must be gated through InputModule / command routing.

---

## 9. Menu Rules

Menu is a premium commercial UI, not a debug panel.

Before menu tasks, inspect:

```text
Assets/_Project/Kaleidoscope2/Menu/
```

Menu architecture:

```text
Menu
    ↓
MenuDirector
    ↓
KaleidoscopeDirector
```

Menu must not directly mutate:

- shaders;
- cameras;
- crystal internals;
- recording internals.

Menu quality requirements:

- TextMeshPro;
- sharp readable text;
- hover/pressed/selected states;
- keyboard/mouse/touch support;
- consistent spacing;
- premium presentation;
- no baked broken text in background art.

---

## 10. Required Top-Level Modules

Required modules:

- Menu
- Core
- Control
- Input
- Source
- Mirror
- Camera
- PhysicsChamber
- AudioReactive
- Tunnel
- DiamondFocus
- Recording
- Presets
- Diagnostics

---

## 11. DiamondFocus Responsibility

DiamondFocus currently acts as:

```text
Legacy Hybrid Crystal Compatibility Layer
```

Responsibilities:

- Billboard2D compatibility;
- temporary RealMesh integration;
- diagnostics;
- compatibility with existing pipeline.

DiamondFocus is NOT final True CrystalStage3D.

Future preferred direction:

```text
DiamondFocus
    ↓
ICrystalRenderer
    ├── BillboardCrystalRenderer
    └── RealMeshCrystalRenderer / CrystalStage3D
```

---

## 12. Performance Rules

Forbidden in Update / LateUpdate / FixedUpdate:

- allocations;
- LINQ allocations;
- FindObjectOfType;
- GameObject.Find;
- repeated material recreation;
- repeated RenderTexture recreation;
- heavy string logging every frame.

Reuse resources.

---

## 13. Validation Rules

Every task report must include:

- What changed
- Why
- Files touched
- Validation
- Risks / Follow-up

For RealMesh3D tasks additionally confirm:

- mesh is volumetric;
- side faces are visible;
- not a plane/quad/billboard;
- final visible screen coverage is reported;
- Layer 1 was not modified;
- Billboard2D still works.

For Premium3D Size & Shape tasks additionally report:

- final visible crystal coverage %;
- internal Stage RT crystal coverage % if applicable;
- active shape name;
- mesh bounds;
- vertex count;
- triangle count;
- hasVolume true/false.

---

## 14. Stage Discipline

Only work on the requested stage.

Do NOT:

- implement future stages early;
- rewrite project globally;
- improve unrelated things;
- combine size/shape fixes with optical effects;
- combine cleanup with new features unless explicitly asked.

Small isolated commits only.

---

## 15. Mental Model

Think like a hardware rack:

- Menu = front panel
- Director = controller
- Source = signal generator
- Mirror = image processor
- Layer 1 = display monitor
- Layer 2 = crystal optical stage
- Recording = recorder
- Diagnostics = monitoring panel

Modules are connected, not fused.

---

## 16. Final Rule

Protect modularity.

A quick hack that destroys architecture is failure.

A slower clean solution is preferred.

If the crystal is small or ugly, fix size and shape before adding effects.
