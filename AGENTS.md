# AGENTS.md — KAELIS / Kaleidoscope2

## 0. Main Instruction

This project has two protected zones:

```text
Classic2D / Layer 1  — protected, do not break.
Premium3D / Layer 2 — active development zone, can be refactored until it reaches the target quality.
```

Classic2D already works well and is visually strong.

Premium3D is not finished. It may contain legacy experiments, temporary bridges, duplicated paths, weak optics, and old failed attempts. Codex is allowed to remove or replace old Premium3D code when it is clearly blocking progress.

Do not preserve broken Premium3D code just because it exists.

---

## 1. Project Goal

KAELIS is a modular Unity cinematic kaleidoscope application with:

- a polished Classic2D kaleidoscope mode;
- a premium RealMesh3D crystal mode;
- modern application menu;
- optional demo/presentation mode;
- built-in default images and music for first launch / showcase;
- file loading for user images later.

Architecture idea:

```text
Input / Menu / Demo Mode / Audio
        ↓
KaleidoscopeDirector
        ↓
Layer 1 Classic2D OR Layer 2 Premium3D
        ↓
Final Output
```

---

## 2. Layer 1 — Classic2D / Protected Mode

Classic2D is the stable reference mode.

Pipeline:

```text
Source
    ↓
Mirror
    ↓
Classic output
    ↓
OutputPreview
```

Classic2D must stay visually unchanged unless the task explicitly says otherwise.

Forbidden during Premium3D work:

- changing MirrorModule behavior;
- changing SourceModule behavior;
- changing RuntimeMenuController / OutputPreview binding;
- changing Classic2D shaders;
- changing Billboard2D behavior;
- changing existing classic hotkeys globally;
- changing Layer 1 visual output to solve Premium3D problems.

Classic2D is the baseline. Do not “fix” it while working on Premium3D.

---

## 3. Layer 2 — Premium3D / Active Development Mode

Premium3D is allowed to evolve aggressively.

Goal:

A large, beautiful, convincing, faceted, volumetric crystal that:

- occupies a strong part of the screen;
- has real 3D mesh geometry;
- has readable front/back depth;
- has mirror-polished facets;
- refracts and reflects the kaleidoscope background;
- does not look like a flat transparent bubble;
- does not show the kaleidoscope center as a clean window;
- has rich gemstone colors;
- supports shape variety;
- supports smooth shape transitions;
- supports controlled optical effects.

Premium3D may be refactored if needed.

Allowed in Premium3D:

- replace weak shaders;
- remove failed optical experiments;
- remove dead legacy bridges;
- replace fake fullscreen tricks with spatial rendering;
- introduce new clean classes;
- split large classes if the split is local and improves clarity;
- add diagnostics;
- add debug modes;
- add material presets;
- add hidden reflection background;
- add reflection camera / reflection texture;
- add shape morphing;
- add mouse wheel size control;
- add gated F-key effect toggles.

Do not be afraid to remove Premium3D code that is proven wrong.

---

## 4. Premium3D Hard Requirements

Premium3D crystal must not be:

- a plane;
- a quad;
- a RawImage;
- a flat billboard;
- a fullscreen fake projection;
- a transparent bubble;
- a clean window to the background center.

Premium3D crystal must be:

- MeshFilter + MeshRenderer;
- volumetric;
- centered;
- large enough;
- stable in size;
- rich in material response;
- reflective and refractive;
- visually stronger than a simple tint.

If the user says “the crystal is still too small / dull / white / flat / bubble-like”, treat that as a valid visual failure even if diagnostics claim success.

Human visual result has priority over internal coverage numbers.

---

## 5. Premium3D Size and Scale Policy

Mouse wheel must control Premium3D crystal scale when crystal mode is active.

Required range:

```text
20% → 300%
```

Rules:

- mouse wheel scaling is active only when Premium3D crystal is visible/active;
- when crystal is not visible, mouse wheel keeps original behavior;
- scale must not pulse or breathe;
- rotation must not change scale;
- shape switching must not reset user scale unless explicitly requested.

If automatic coverage calculation conflicts with user scale, user scale wins.

---

## 6. Premium3D Optical Policy

The crystal must not behave like a direct transparent window.

Bad:

```text
Background → direct clean screenUv → user
```

Good:

```text
Background → refraction / reflection / internal echo / facet split → user
```

Required optical direction:

- reduce direct clean transmission;
- block clean center see-through;
- use internal reflection layers;
- use hidden reflection background;
- use mirror-like facets;
- use gemstone absorption;
- use Fresnel;
- use controlled dispersion;
- keep highlights readable, not blown out.

Diamond must not become a white unreadable blob.

Ruby must feel deep red and expensive, not pink plastic.

Opal must feel milky/rainbow/internal, not a flat tint.

---

## 7. Hidden Reflection Background

Premium3D should support a hidden reflection environment.

Concept:

```text
Visible kaleidoscope background
        ↓
seen behind crystal

Hidden mirrored background behind camera
        ↓
not directly visible
        ↓
reflected only in mirror-polished crystal facets
```

Rules:

- hidden reflection background must not be directly visible in Game View;
- it must be visible only to reflection camera / reflection texture / crystal shader;
- it must not cause screen-inside-screen;
- it must not create a second visible wall;
- it must not affect Classic2D;
- diagnostics must prove whether reflection texture is valid and assigned.

Specular light highlight is not proof of hidden background reflection.

Proof mode is required:

```text
HiddenReflectionOnlyOnCrystal
```

In this mode, hidden background must be unmistakably visible on facets.

---

## 8. Premium3D Refactor Permission

Codex is explicitly allowed to remove old Premium3D code if it is:

- unused;
- duplicated;
- blocking the desired result;
- responsible for pulsing/breathing;
- responsible for fullscreen fake projection;
- responsible for white overexposure;
- responsible for clean center see-through;
- responsible for conflicting scale correction;
- a failed experimental path.

Before removal:

1. Identify the file/class.
2. Explain why it is Premium3D-only.
3. Confirm it is not used by Classic2D.
4. Remove or isolate it.
5. Compile.
6. Report the change.

Do not remove Classic2D code.

Do not remove shared code unless it is proven safe or replaced with a compatible path.

---

## 9. Input / Hotkey Safety

Do not steal existing hotkeys globally.

Crystal-specific controls are active only when:

```text
Premium3D crystal is visible/active
```

If Premium3D crystal is not active, old project behavior must remain.

Required known controls:

```text
Mouse Wheel — Premium3D crystal scale, 20% to 300%, only when crystal is active
Insert/Delete — Premium3D crystal brightness range, safer lowered bounds
F1 — Help, never steal
F2–F12 — Premium3D effect toggles, only when crystal is active
```

Suggested F-key map:

```text
F2  Hidden reflection background
F3  Mirror facets
F4  Internal reflections
F5  Dispersion / spectral split
F6  Refraction distortion
F7  Opal iridescence
F8  Facet highlights
F9  Shape morphing
F10 Optical diagnostics
F11 Cycle gem material preset
F12 Reset Premium3D optical controls
```

---

## 10. Modern Menu Direction

KAELIS needs a modern application menu.

Menu goals:

- premium look;
- clean structure;
- no debug-panel feeling;
- readable typography;
- keyboard/mouse support;
- clear mode switching;
- modern polished visual style.

Menu must include, eventually:

- Classic2D / Premium3D mode switch;
- crystal material selection;
- crystal shape selection;
- crystal scale display/control;
- optical effects toggles;
- demo mode checkbox;
- image source selection;
- audio/demo controls;
- diagnostics panel optional.

Menu must not directly mutate low-level shaders or cameras.

Preferred flow:

```text
Menu UI
    ↓
MenuDirector / command layer
    ↓
KaleidoscopeDirector / settings
    ↓
Modules
```

---

## 11. Demo Mode

Add a separate application demo mode.

Demo mode is controlled by a checkbox in the modern menu.

When Demo Mode is ON:

- play a default bundled track;
- use bundled default illustration/image set;
- auto-cycle images;
- optionally auto-cycle shapes/materials;
- show the application in a polished presentation state.

When Demo Mode is OFF:

- user-controlled mode remains available;
- user-selected images/music should be used when implemented;
- normal controls remain.

Demo mode must not depend on external user files.

Default assets will be chosen by the user and then included in the project.

Expected structure may be:

```text
Assets/_Project/Kaleidoscope2/DemoContent/
    Audio/
    Images/
    Presets/
```

Demo mode must be deterministic and safe for showcasing.

---

## 12. Built-In Illustrations / Default Content

KAELIS should include a curated built-in image set.

Purpose:

- first launch looks good;
- demo mode works without file browser;
- Premium3D has rich backgrounds to reflect/refract;
- Classic2D has good showcase material.

Rules:

- built-in images are content assets, not code hacks;
- do not hardcode one image into shaders;
- use a content provider / source list;
- allow user images later without breaking demo defaults.

---

## 13. Diagnostics

Premium3D diagnostics should report:

- active mode;
- active shape;
- active material;
- crystal scale percent;
- hidden reflection texture valid;
- hidden reflection texture assigned;
- hidden reflection visible to main camera false;
- hidden reflection visible to reflection camera true;
- direct transmission value;
- brightness/intensity;
- pulsing/breathing flags false;
- active effect toggles.

Diagnostics are allowed to be technical. They are for development confidence.

---

## 14. Stage Discipline

Work in focused stages.

Do not combine menu rewrite, optical shader rewrite, demo mode, asset import, shape morphing, and input remap unless the task explicitly asks for a combined pass.

However, within Premium3D, Codex may refactor old broken code when necessary to complete the requested stage.

---

## 15. Validation Requirements

Every task report must include:

- what changed;
- files touched;
- forbidden files check;
- compile result;
- what was not touched;
- visual acceptance notes if relevant;
- remaining issues.

For Premium3D tasks, also report:

- whether Classic2D was untouched;
- whether pulsing/breathing returned;
- whether hidden reflection is actually visible in debug mode if relevant;
- whether mouse wheel scale works if relevant;
- whether the crystal still shows clean center directly.

---

## 16. Final Rule

Classic2D is protected.

Premium3D is allowed to be rebuilt until it becomes beautiful.

Do not keep broken Premium3D code for safety theater.

Safety means protecting the working Classic2D mode and moving Premium3D forward with clear, testable changes.
