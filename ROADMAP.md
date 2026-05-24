# ROADMAP.md — KAELIS Crystal + Menu Binding Roadmap

## Strategy

KAELIS is moving from visual menu shell to real crystal-control integration.

The user wants three stages:


1. New AGENTS.md and ROADMAP.md for crystals and menu binding.
2. Careful Codex planning.
3. Implementation of the approved plan.


This roadmap defines the work from parameter audit to Premium3D crystal improvement.

---

# PHASE 0 — Safety Checkpoint

Goal:
Start from a safe state.

Tasks:

- run `git status --short`;
- confirm branch;
- identify dirty files;
- commit or stash current stable menu work if needed;
- read AGENTS.md;
- read ROADMAP.md;
- compile current state before audit if possible.

Acceptance:

- current baseline is known;
- no accidental broad rewrite begins.

---

# PHASE 1 — Full Menu Parameter Binding Audit

Goal:
Determine exactly what every menu control does.

Audit all controls in:


Modes
Optics
Presets
Settings
Exit
Enter Experience
Production / Showcase / Recording if present


For each control report:


Control name
Panel
UI type: row / button / slider / toggle
Current range
Default value
Tooltip/hotkeys
Binding target
File/class/method/property
Affected mode(s)
Expected visual effect
Actual result
Binding status: REAL / PARTIAL / RESERVED / DEAD / BROKEN
Recommended fix


Acceptance:

- no menu control remains unknown;
- dead controls are identified;
- fake/reserved controls are clearly separated from real controls.

---

# PHASE 2 — Optics Parameter Effect Audit

Goal:
Analyze whether optics ranges are strong enough and whether they lead to real visual change.

Controls to audit:


Brightness
Contrast
Bloom / Glow
Facet Highlights
Refraction Strength
Reflection Strength
Internal Reflections
Background Distortion
Direct Transparency
Prism Dispersion
Chromatic Aberration
Rainbow Edge
Spectral Split
Caustics
Spotlight Shadow
Mirror Backdrop
Crystal Depth
Absolute Mirror / Mirror Material


For each:


Current shader/material/property target
Current range
Default
What it should visually do
What it actually does
Whether effect is strong enough
Proposed extended range
Safety clamp
Required implementation fix


Acceptance:

- all optics controls either work or are marked RESERVED/BROKEN;
- ranges are proposed for dramatic visible effect.

---

# PHASE 3 — Premium3D Crystal Transparency / Mirror / Refraction Audit

Goal:
Answer the user’s core questions.

Questions:


Is transparency fully removed/controlled?
Can the center still show direct background incorrectly?
Does mouse wheel scale every crystal shape?
Does absolute mirror mode really work?
Does facet refraction distort light correctly?
Does dispersion visibly split light?
Do internal reflections create depth?


Tasks:

- inspect RealCrystalOptics shader/material path;
- inspect CrystalSharedSettings;
- inspect SpatialCrystalStage3D;
- inspect RealCrystalVolumetricMeshFactory;
- inspect input handling for mouse wheel scaling;
- inspect material presets and gem modes;
- inspect hidden reflection/background setup.

Acceptance:

- each question has a concrete answer;
- broken/partial items have a fix plan.

---

# PHASE 4 — Classic2D Shape Template Audit

Goal:
Identify the Classic2D shape/form language that must be transferred to Premium3D.

Tasks:

- inspect Classic2D mode shape/template code;
- list all available shapes/templates/forms;
- identify user-facing shape names;
- identify parameters defining the shapes;
- identify morph/transition behavior if any;
- identify how these shapes are selected.

Output table:


Classic2D shape name
File/class
Parameters
Visual description
User-facing?
Premium3D equivalent needed?
Current Premium3D equivalent exists?
Implementation difficulty


Acceptance:

- Classic2D shape vocabulary is fully known;
- no guesswork before Premium3D mesh work.

---

# PHASE 5 — Premium3D Shape Transfer Plan

Goal:
Plan volumetric 3D equivalents of Classic2D shapes.

For each Classic2D shape:


Premium3D mesh type
Silhouette preservation strategy
Depth/thickness strategy
Facet generation strategy
Front/back/side face strategy
UV/reflection/refraction compatibility
Mouse wheel scaling support
Material compatibility
Morph compatibility
Risk level


Required:

- Premium3D shape must be physically volumetric;
- not just a flat billboard;
- must have real side faces and depth;
- must support optical material controls.

Acceptance:

- approved list of Premium3D shape tasks exists.

---

# PHASE 6 — Menu Binding Architecture Plan

Goal:
Plan how menu controls connect to runtime safely.

Required architecture:


Menu slider/toggle
 -> KaelisMenuActionRouter
 -> KaelisMenuCommandBridge
 -> KaelisCrystalSettings / command / module API
 -> crystal runtime module / material / shader property


Tasks:

- identify existing safe APIs;
- identify missing APIs;
- propose minimal new APIs;
- avoid random direct object poking;
- define data model for optics settings.

Potential classes:


KaelisCrystalOpticsSettings
KaelisCrystalShapeSettings
KaelisCrystalMaterialSettings
KaelisPremiumCrystalController
KaelisMenuCrystalBindingBridge


Acceptance:

- every real menu control has a planned binding target;
- no dead sliders remain.

---

# PHASE 7 — Implementation Stage A: Binding Truth

Goal:
Make menu controls truthful before deep visual changes.

Tasks:

- mark dead controls as RESERVED or bind them;
- ensure tooltips show real binding status;
- ensure sliders update actual settings object;
- add diagnostics showing value changes;
- do not yet rewrite mesh system.

Acceptance:

- menu values are no longer fake;
- diagnostics show control -> setting flow.

---

# PHASE 8 — Implementation Stage B: Mouse Wheel Scaling

Goal:
All Premium3D crystal shapes support mouse-wheel scaling.

Requirements:


Range: 20% – 300%
Default: 100%
Works for every Premium3D shape
No pulsing/breathing
No background counter-scaling
No shape-specific failure


Tasks:

- find current scale control path;
- centralize Premium3D crystal scale;
- bind mouse wheel and Settings control if applicable;
- update diagnostics.

Acceptance:

- all Premium3D shapes scale consistently.

---

# PHASE 9 — Implementation Stage C: Transparency / Mirror / Refraction

Goal:
Fix the core optical behavior.

Tasks:

- reduce unwanted direct transparency;
- make Direct Transparency control real;
- make Absolute Mirror mode real;
- strengthen reflection/background facet sampling;
- strengthen facet-normal-based refraction;
- strengthen prism dispersion;
- add safe clamps;
- update presets.

Acceptance:

- crystal no longer feels like soap bubble;
- high refraction visibly distorts background;
- absolute mirror clearly reflects environment;
- prism split is visible.

---

# PHASE 10 — Implementation Stage D: Premium3D Shape Transfer

Goal:
Create 3D volumetric versions of Classic2D crystal forms.

Tasks:

- implement selected shape factory methods;
- preserve Classic2D silhouettes;
- add real volume/depth;
- support material/optics controls;
- support mouse-wheel scaling;
- support shape switching/morph if feasible.

Acceptance:

- Premium3D offers recognizable counterparts to Classic2D shapes;
- Classic2D remains unchanged.

---

# PHASE 11 — Presets Integration

Goal:
Connect presets to real crystal/menu parameters.

Tasks:

- define factory presets;
- set shape/material/optics values;
- clamp ranges;
- apply through command bridge;
- mark user presets RESERVED if persistence not ready.

Factory presets:


Diamond Palace
Blue Ice
Golden Prism
Ruby Night
Emerald Depth
Opal Dream
Cosmic Glass
Dark Luxury
Absolute Mirror


Acceptance:

- applying a preset visibly changes Premium3D output.

---

# PHASE 12 — Validation / Regression Pass

Goal:
Prove the work did not break core modes.

Checklist:


Classic2D unchanged
Premium3D shows volumetric crystal
All Premium3D shapes scale 20–300%
Direct transparency controlled
Absolute mirror works
Refraction strong at high values
Dispersion visible at high values
Menu controls real or RESERVED
No dead sliders
No pulsing/breathing regression
No duplicate physical/RT output regression
Compile passes
Smoke tests pass


Acceptance:

- user can test and confirm visual improvement.

---

# Current Active Task For Codex

After replacing AGENTS.md and ROADMAP.md, run:


PLAN ONLY:
Perform a full audit and planning pass for KAELIS crystal/menu parameter bindings.
Do not implement yet.
Answer exactly which menu controls lead to which runtime parameters, which are dead, which are reserved, whether ranges are strong enough, whether Premium3D transparency/mirror/refraction/scaling work, and how Classic2D shapes should be transferred to volumetric Premium3D forms.


---

# Final Principle

Do not add more fake controls.

Make the crystal respond.
Make Premium3D physically expressive.
Use Classic2D as shape inspiration, not as something to break.
