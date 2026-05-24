# AGENTS.md — KAELIS Crystal + Menu Integration Mode

## 0. Main Mission

KAELIS is now entering a crystal-control integration phase.

The goal is not to add more decorative placeholders. The goal is to make every menu setting lead to a real, visible, meaningful visual result.

Main objective:


Every menu control must map to a real parameter, command, or explicitly marked RESERVED future binding.
Premium3D crystals must become expressive, physically volumetric versions of the Classic2D crystal/form language.


The user wants dramatic, visible, sometimes surprising image transformation:


“so that the image can be admired and be surprising — like: was this even possible?”


Tiny parameter changes are not enough.

---

## 1. Protected Baseline

The Classic2D mode is the visual quality baseline and must not be broken.

Classic2D rules:

- do not degrade Classic2D visuals;
- do not rewrite Classic2D shader logic unless explicitly approved;
- do not remove existing Classic2D shape/form behavior;
- use Classic2D as reference for shape language and visual richness.

Premium3D rules:

- Premium3D is the active improvement zone;
- Premium3D may be refactored if needed;
- Premium3D must receive the same shape/form family as Classic2D, but as real 3D volumetric crystals;
- Premium3D optics must be improved until controls produce clear visible results.

Menu rules:

- menu must expose meaningful controls;
- menu controls must not lie;
- controls without real binding must be marked RESERVED;
- controls with real binding must visibly affect the result.

---

## 2. Allowed Work Areas

Codex may work in these areas for this phase:


Assets/_Project/Kaleidoscope2/Menu/**
Assets/_Project/Kaleidoscope2/DiamondFocus/**
Assets/_Project/Kaleidoscope2/CrystalStage3D/**
Assets/_Project/Kaleidoscope2/Diagnostics/**


Codex may also inspect, but not blindly rewrite:


Classic2D-related code
Mirror/**
Input/**
KaleidoscopeDirector.cs
KaleidoscopeCommand.cs
CrystalPresentationModule.cs
RealCrystalVolumetricMeshFactory.cs
RealCrystalOptics.shader
SpatialCrystalStage3D.cs
CrystalSharedSettings.cs


If a safe public command/API is missing, Codex may propose or add a minimal clean bridge, but must report it.

---

## 3. Forbidden / High-Risk Areas

Do not casually modify:


Source/**
OutputPreview internals
RuntimeMenuController internals
camera/render pipeline logic
global render settings
scene-wide camera setup
Classic2D shader internals


Exception:
If the planned fix absolutely requires a change outside the allowed zone, Codex must first report:

- why it is required;
- what file must change;
- what behavior is protected;
- expected risk;
- validation plan.

---

## 4. Parameter Truth Rule

Every menu setting must be audited and classified:


REAL_BINDING
PARTIAL_BINDING
RESERVED
DEAD_CONTROL
BROKEN_BINDING


Definitions:

### REAL_BINDING
The UI control changes a real runtime parameter and the visual result is observable.

### PARTIAL_BINDING
The UI control changes something, but the visual result is weak, incomplete, or only affects one mode.

### RESERVED
The UI control is intentionally future-facing and clearly labeled as unavailable.

### DEAD_CONTROL
The UI control exists but leads nowhere.

### BROKEN_BINDING
The UI control tries to call something but fails, does nothing, or changes the wrong parameter.

Dead controls are not allowed to remain silently.

---

## 5. Required Audit For Every Setting

For each menu setting, Codex must answer:


Control name:
Panel:
Current UI range:
Current default:
Current binding target:
Affected runtime file/class:
Affected shader/material property if any:
Affected modes:
Expected visual effect:
Actual observed/measured effect:
Is range strong enough?
Recommended range:
Status: REAL / PARTIAL / RESERVED / DEAD / BROKEN
Required fix:


The audit must include:

- Modes panel;
- Optics panel;
- Presets panel;
- Settings panel;
- Production/Recording controls if present;
- Enter Experience content-selection flow;
- mouse wheel crystal scaling;
- crystal material mode toggles;
- mirror/refraction/reflection controls.

---

## 6. Premium3D Crystal Requirements

Premium3D crystals must become real visual objects, not flat transparent overlays.

### 6.1 Opacity / transparency
Audit whether transparency is fully controlled.

Required:

- no unwanted direct see-through center;
- no “soap bubble” look;
- transparency must be controllable;
- Direct Transparency must be a real setting;
- default should hide direct background enough to feel like a gemstone.

### 6.2 Mouse wheel scaling
All Premium3D crystals and all Premium3D crystal shapes must support mouse-wheel scaling.

Required:


Mouse wheel up/down changes crystal size.
Range must be large and expressive.
Suggested size range: 20% – 300%.
Default: 100%.


No shape may ignore scaling.

### 6.3 Absolute mirror mode
There must be a real absolute mirror / polished mirror mode.

Required:

- crystal facets become mirror-polished;
- reflection dominates;
- direct transparency is minimized;
- hidden reflection/background environment is visible through facets;
- should look like luxury mirror/prism material.

If not currently implemented, mark as BROKEN/PARTIAL and plan a fix.

### 6.4 Facet refraction
Facet-based refraction must be real and visible.

Required:

- different facets bend background differently;
- refraction direction depends on facet normals;
- image distortion must be significant at high values;
- prism/dispersion must split light/color near facets;
- result must not be just a smooth lens/bubble.

### 6.5 Internal reflections
At high values, crystal must show deeper internal echo/reflection layers.

Required:

- stronger internal bounce feeling;
- more depth;
- no flat glass disc look.

### 6.6 Premium shape language
Premium3D must inherit the shape/form language of Classic2D.

Meaning:


Classic2D remains 2D as before.
Premium3D gets corresponding volumetric 3D crystal versions of those shapes.


The target is the same family of visual shapes/forms, but with depth, facets, thickness, and physical crystal volume.

---

## 7. Shape Transfer Rule: Classic2D -> Premium3D

Codex must inspect the Classic2D shape/template system and identify:

- what forms/templates exist;
- how they are named;
- what parameters define them;
- which are user-facing;
- which are internal.

Then map each Classic2D form to a Premium3D volumetric counterpart.

Example mapping format:


Classic2D shape: Star / radial shard / diamond / polygon / mandala / ...
Premium3D shape: volumetric star-cut gem / faceted diamond / prism object / ...
Mesh requirement: real side faces, front/back depth, non-flat thickness.
Status: existing / needs new mesh / needs factory method.


Required:
- no fake 2D extrusion only;
- must have real volume;
- must preserve recognizable silhouette from Classic2D;
- must work with material/optics controls;
- must support mouse-wheel scaling.

---

## 8. Menu Binding Requirements

Menu controls must be connected through a safe binding architecture:


Menu UI
  -> KaelisMenuActionRouter
  -> KaelisMenuCommandBridge
  -> safe runtime command/settings object
  -> render/crystal module


Do not wire sliders by random `FindObjectOfType` calls unless no safer path exists and it is reported.

Preferred:

- shared settings object;
- explicit command;
- public method on module;
- central dispatcher.

Every real binding must have:
- clamp;
- default;
- min/max;
- reset;
- tooltip;
- visible value;
- validation.

---

## 9. Range Philosophy

Ranges must be large enough to produce dramatic visual variation.

But they must not produce:
- NaN;
- white screen;
- black screen;
- broken mesh;
- invisible crystal;
- permanent overexposure;
- camera clipping;
- GPU errors.

For each range, Codex must decide:


Safe default range
Creative extended range
Hard clamp range


Example:


Refraction Strength:
default 1.0
UI range 0.0 – 5.0
hard clamp 0.0 – 8.0


---

## 10. Diagnostics Requirement

Codex must add or extend menu/crystal diagnostics where useful.

Diagnostics should report:

- active crystal shape;
- active crystal material;
- crystal scale percent;
- transparency/direct transmission;
- mirror strength;
- refraction strength;
- dispersion strength;
- whether parameter bindings are real;
- whether current menu slider changed runtime state;
- whether physical stage RT is active;
- whether hidden reflection/background texture is assigned.

---

## 11. Planning Before Implementation

For this phase, Codex must not jump straight into code.

Required workflow:


Stage 1: Audit and map existing controls/parameters.
Stage 2: Plan fixes and shape transfer.
Stage 3: Implement only after approval.


The audit must be concrete and file-based.

No vague statements like:
- “improved optics”
- “enhanced crystal”
- “made it better”

Use exact file, class, method, property names.

---

## 12. Validation

Every implementation pass must include:

- Unity compile result;
- Editor compile result;
- smoke test if available;
- Play Mode manual checklist;
- `git diff --name-only`;
- protected path check;
- before/after report for each affected control;
- report of real vs reserved controls.

Required visual checks:

- all Premium3D shapes scale with mouse wheel;
- direct see-through is reduced/controlled;
- absolute mirror mode visibly works;
- refraction produces strong facet-based distortion at high values;
- prism dispersion is visible at high values;
- Classic2D is unchanged;
- menu still functions.

---

## 13. Final Principle

KAELIS must not have decorative controls.

If a slider exists, it must either:


1. visibly control something real,
or
2. clearly say RESERVED.


Premium3D must become the volumetric, physical, expressive continuation of the Classic2D shape language.
