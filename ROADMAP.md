# ROADMAP.md — KAELIS Development Roadmap

## Current Strategy

KAELIS has one stable visual reference and one active experimental target.

```text
Classic2D — protected reference mode.
Premium3D — active development mode.
```

Classic2D must remain untouched unless explicitly requested.

Premium3D may be refactored, cleaned, simplified, or rebuilt until it reaches the desired result.

---

## Desired Result

The application should become:

- visually impressive immediately after launch;
- usable as a demo/showcase without manual setup;
- able to show beautiful Classic2D kaleidoscope visuals;
- able to show a premium 3D crystal that reflects/refracts the background convincingly;
- controlled through a modern menu.

---

# PHASE 0 — Safety Baseline

Goal:

Create a safe checkpoint before larger Premium3D/menu/demo changes.

Tasks:

- run `git status`;
- commit current working state;
- push if needed;
- confirm current branch;
- confirm Classic2D works.

Acceptance:

- clean rollback point exists;
- Classic2D current behavior is known.

---

# PHASE 1 — Premium3D Stabilization and Cleanup

Goal:

Stop protecting failed Premium3D experiments.

Tasks:

- identify old Premium3D code paths;
- identify duplicated renderers;
- identify unused legacy bridges;
- identify code causing:
  - pulsing;
  - breathing;
  - white overexposure;
  - fake fullscreen projection;
  - clean center see-through;
  - reflection pipeline failure;
- remove or isolate Premium3D-only broken code.

Rules:

- do not touch Classic2D;
- do not touch Mirror/Source/OutputPreview unless explicitly approved;
- removal is allowed if the code is Premium3D-only and blocking progress.

Acceptance:

- Premium3D code path is clearer;
- no obvious duplicate crystal path;
- no accidental fullscreen fake wall;
- no hidden broken renderer kept “just in case”.

---

# PHASE 2 — Premium3D Scale and Controls

Goal:

Give the user direct control over Premium3D crystal size.

Tasks:

- mouse wheel controls Premium3D crystal scale;
- scale range: 20% to 300%;
- only active when Premium3D crystal is visible/active;
- when inactive, mouse wheel keeps old behavior;
- scale remains stable during rotation;
- scale remains stable during shape switching.

Also:

- Insert/Delete brightness range should be safer;
- upper brightness limit lowered by 40%;
- lower brightness limit lowered by 40%;
- prevent Diamond white overexposure.

Acceptance:

- user can make crystal huge or small manually;
- no pulsing/breathing;
- Classic2D unchanged.

---

# PHASE 3 — Premium3D Hidden Reflection Pipeline

Goal:

Make crystal facets reflect rich hidden environment detail.

Tasks:

- create hidden mirrored background behind the camera;
- render it to hidden reflection texture;
- make it invisible to main/user camera;
- assign it to crystal material;
- implement/verify HiddenReflectionOnlyOnCrystal debug mode;
- make mirror-polished facets visibly reflect it.

Acceptance:

- hidden reflection texture is non-empty;
- hidden reflection texture is assigned to material;
- user cannot see second background directly;
- crystal facets visibly show hidden reflection fragments;
- specular light spot is not mistaken for reflection proof.

---

# PHASE 4 — Premium3D Optical Material Pass

Goal:

Make the crystal feel like a gemstone, not a transparent bubble.

Tasks:

- reduce direct center see-through;
- reduce clean screenUv window behavior;
- improve internal reflection;
- improve facet-based refraction;
- improve gemstone absorption;
- improve Fresnel/edge response;
- prevent white overexposure;
- tune Diamond/Ruby/Emerald/Sapphire/Opal profiles.

Acceptance:

- Diamond is bright but not a white blob;
- Ruby is deep red, not pink plastic;
- Opal has internal milky/rainbow character;
- center of kaleidoscope is not visible as a clean direct hole;
- background is transformed through the crystal.

---

# PHASE 5 — Premium3D Shape Library and Morphing

Goal:

Make Premium3D shapes as visually exciting as Classic2D.

Tasks:

- improve weak shapes;
- remove or deprioritize ugly forms;
- add/clean shapes:
  - Classic Brilliant;
  - Octagon;
  - Cushion;
  - Marquise;
  - Pear / Drop;
  - Emerald / Step Cut;
  - Princess / Square;
  - Hexagon variation;
- implement safe shape morphing;
- avoid hard popping;
- keep scale stable.

Acceptance:

- shape switching is visually satisfying;
- forms are not random broken shards;
- crystal remains volumetric and attractive.

---

# PHASE 6 — F-Key Premium3D Effect Toggles

Goal:

Make Premium3D effects controllable.

Rules:

- F1 remains Help;
- F2–F12 are active only when Premium3D crystal is visible/active;
- when Premium3D is inactive, old key behavior remains.

Suggested mapping:

```text
F2  Hidden reflection background
F3  Mirror facets
F4  Internal reflections
F5  Dispersion
F6  Refraction distortion
F7  Opal iridescence
F8  Facet highlights
F9  Shape morphing
F10 Optical diagnostics
F11 Cycle gem material
F12 Reset Premium3D optical controls
```

Acceptance:

- toggles work;
- diagnostics report state;
- no global hotkey stealing.

---

# PHASE 7 — Modern Menu Foundation

Goal:

Build a modern application menu.

Tasks:

- inspect existing menu files;
- design clean menu architecture;
- add mode switch Classic2D / Premium3D;
- add Demo Mode checkbox;
- add basic crystal controls;
- add visual style foundation.

Rules:

- menu must not directly mutate shaders/cameras;
- use command/settings layer;
- keep it polished, not debug-like.

Acceptance:

- menu looks modern;
- Demo Mode checkbox exists;
- Classic/Premium switching is clear;
- no rendering logic is hardcoded in UI.

---

# PHASE 8 — Demo Mode

Goal:

Create a separate showcase mode.

Demo Mode ON:

- play default bundled track;
- use bundled illustration/image set;
- auto-cycle content;
- optionally auto-cycle Premium3D shapes/materials;
- show polished visual experience.

Demo Mode OFF:

- normal user control;
- no forced audio/images.

Tasks:

- add DemoModeSettings;
- add DemoModeController;
- add content provider for bundled images;
- add default audio hook;
- add menu checkbox;
- make it deterministic.

Expected asset structure:

```text
Assets/_Project/Kaleidoscope2/DemoContent/
    Audio/
    Images/
    Presets/
```

Acceptance:

- application can run a good-looking demo without external files;
- user can disable demo mode;
- default assets are replaceable by project content.

---

# PHASE 9 — Built-In Illustration Source

Goal:

Bundle curated images for first launch/demo.

Tasks:

- add built-in image list;
- make Source module able to receive built-in texture sequence without breaking user file loading;
- support auto-cycle;
- keep future file browser compatibility.

Acceptance:

- default built-in illustrations work;
- selected images can be changed later;
- Classic2D and Premium3D both benefit.

---

# PHASE 10 — Presentation Polish

Goal:

Make KAELIS feel like a finished visual application.

Tasks:

- menu polish;
- loading/splash polish;
- demo transitions;
- preset names;
- diagnostics toggle;
- stable defaults;
- performance check.

Acceptance:

- app opens into a strong visual state;
- demo mode showcases the project;
- Classic2D remains strong;
- Premium3D feels valuable, not experimental.

---

## Forbidden During All Phases

Unless explicitly requested:

- do not modify Classic2D visual output;
- do not modify MirrorModule behavior;
- do not modify SourceModule behavior for Premium3D-only fixes;
- do not break RuntimeMenuController / OutputPreview binding;
- do not remove working Classic2D code;
- do not steal global hotkeys.

---

## Allowed During Premium3D Phases

Codex may:

- delete failed Premium3D-only code;
- replace weak Premium3D shaders;
- replace weak Premium3D renderers;
- remove duplicated Premium3D paths;
- create clean Premium3D modules;
- add diagnostics/debug modes;
- refactor Premium3D until visual target is met.

This permission is intentional.

Premium3D is not protected legacy. It is the active build area.

---

## Current Active Priority

```text
1. Premium3D cleanup
2. Mouse wheel scale control
3. Hidden reflection proof
4. Optical material correction
5. Modern menu + Demo Mode foundation
```

Do not start full menu/demo work until the current codebase has a safe checkpoint.
