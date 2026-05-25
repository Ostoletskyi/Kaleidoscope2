# AGENTS.md - KAELIS Layered Visual Instrument Doctrine

## Purpose

KAELIS / Kaleidoscope2 is a visual instrument and visual synthesizer.

It is not a collection of unrelated visual switches. Every feature must fit a
layered control model with explicit ownership, truthful controls, and
deterministic routing.

Audit discipline is permanent. Before adding or repairing behavior, Codex must
identify which layer owns it, which command represents it, which feature
module interprets it, and how the result will be validated.

---

## Role Of Codex

Codex acts as lead architect and disciplined implementer.

For every task Codex must:

1. Understand the artistic and technical goal.
2. Identify affected layers, owners, commands, renderers, and risks.
3. Audit the current route before changing it.
4. Make the smallest coherent architectural change that satisfies the goal.
5. Validate behavior, control truthfulness, and protected systems.

Codex may improve weak implementation and repair obsolete wiring, but must not
replace clear architecture with shortcuts or silently erase valuable visual
behavior.

---

## Core Doctrine

Classic and Premium are two render philosophies interpreting one control
architecture.

- `Classic`: mathematical mirror symmetry, planar kaleidoscopic form language,
  and elegant mathematical morphing.
- `Premium`: physical optical materiality, volumetric forms, facets,
  reflection, refraction, dispersion, and gemstone presence.

They are not competing state machines. A shared user intent may be interpreted
differently by each renderer while preserving its philosophy.

Example:

- Geometry morph intent in Classic selects a curated mathematical form.
- Geometry morph intent in Premium selects a curated volumetric crystal form.
- Debug Effect intent may drive different shader expressions in each renderer,
  but the selected effect and command ownership remain shared.

---

## Layered Model

### Layer 1 - Render Philosophy

Owns which rendering philosophy interprets crystal capability state.

Values:

- `Classic`
- `Premium`

Ownership:

- A dedicated render-philosophy state owner stores this selection.
- Only explicit render-philosophy commands may change it.
- No debug, optical, preset-detail, geometry-cycle, or visibility action may
  change it incidentally.

### Layer 2 - Capability Classes

Owns the kind of crystal concept the user is controlling.

Classes:

1. `Geometry`
2. `Optics`
3. `Debug`
4. `Experimental / Debug Effects`

These classes are independent and stackable. Selecting a class chooses the
context for subclass cycling; it does not change render philosophy or apply a
hidden visual state.

### Layer 3 - Subclasses

Owns selectable modes within one capability class.

Examples:

- Geometry: curated Classic forms or Premium symmetric crystal forms.
- Optics: High-Purity Diamond, Prism Dispersion, Mirror Facets, Internal
  Reflection, Absolute Mirror, and explicitly integrated optical modes.
- Debug: existing diagnostic views, with invisible diagnostic states excluded
  from ordinary cycling.
- Experimental / Debug Effects: Perfect Mirror Boost, Sea-Frosted Broken
  Bottle Glass, Negative, Halo, Ancient Stone, Facet Chromatic Aberration,
  Glimmer + Lens Flare, Rainbow / Prism Fire, Mirage / Atmospheric Heat Haze,
  and deliberately authored experimental states.

Rules:

- Subclasses belonging to different classes must not be collapsed into one
  enum or one ambiguous cycle route.
- Normal cycling exposes only valid, visible, user-facing states.
- Diagnostic-only or artifact states may remain accessible through explicit
  menu/API routes and must be labeled as such.
- Experimental artifact behavior must remain expressive; do not normalize it
  merely to make implementation easier.

### Layer 4 - Continuous Parameters

Owns numeric expression inside a selected state.

Examples:

- intensity
- roughness
- glow and bloom
- brightness and contrast
- reflection and refraction
- dispersion and chromatic amount
- transparency and direct transmission
- curvature and scale
- rotation, motion speed, and atmospheric distortion

Rules:

- Parameters need a single state owner and a documented range.
- Menu and keyboard input dispatch parameter intent only.
- Renderers and shaders consume resolved values; they do not invent control
  ownership.
- Values outside a preferred `0..40` performance range require a stated
  artistic or legacy justification and a documented bound.

---

## Stackability Contract

The architecture must permit independent combinations such as:

`Premium + Star Prism + Absolute Mirror + Halo + Experimental Artifact`

or:

`Classic + Curated Form + Rainbow / Prism Fire + Debug Reflection View`

When a combination requires a guard, the guard modifies only the unsafe
parameter, not unrelated selected states.

Example:

- In Absolute Mirror, a Frosted effect may add roughness and edge abrasion.
- Absolute Mirror must still force zero direct transmission and zero direct
  transparency.
- The selected Frosted effect remains selected; it is not silently replaced.

---

## Strict Block Architecture

Required runtime flow:


InputModule / RuntimeMenuController
    -> KaleidoscopeCommand
        -> KaleidoscopeDirector
            -> Feature Module / State Owner
                -> Renderer / Material Binder
                    -> Shader


Responsibilities:

| Block | Responsibility |
| --- | --- |
| `InputModule` | Read physical input and emit commands only. |
| `RuntimeMenuController` and menu views | Display state, tooltip scope, and dispatch commands only. |
| `KaleidoscopeCommand` | Describe intent with explicit payloads. |
| `KaleidoscopeDirector` | Route commands and coordinate state ownership only. |
| Feature modules/settings | Own capability logic, selected subclasses, guards, transitions, and resolved values. |
| Preset module/state service | Apply authored state through command/state routing. |
| Renderers/material binders | Translate resolved state into renderer/material inputs. |
| Shaders | Render received values only. |
| Diagnostics/tests | Verify routes, invariants, visibility, and regression safety. |

Forbidden:

- Input directly mutating mesh, shader, or material values.
- Menu code directly mutating mesh, shader, or material values.
- Debug effects secretly switching Classic/Premium.
- Presets bypassing the Director/state route.
- Multiple modules owning the same selected state or parameter.
- UI labels that do not correspond to actual runtime behavior.
- Hidden input side effects.
- Renderer fallback behavior that replaces a selected user-facing geometry
  with an ugly primitive after a transition.

---

## Runtime Control Contract

Every visible control must have one meaning, one owner, one command route, one
menu/help description, one tooltip, and a declared scope: `Classic`,
`Premium`, or `Both`.

| Control | Required Meaning | Scope |
| --- | --- | --- |
| `G` | Switch Render Philosophy only: Classic / Premium. | Both |
| `Backspace` | Toggle crystal visibility only. | Both |
| `Numpad 1` | Select the `Geometry` capability class. | Both; Premium labels must name Premium forms |
| `Numpad 3` | Select the `Optics` capability class. | Premium or explicitly supported Classic optics |
| `Numpad 7` | Select the `Debug` capability class. | Both |
| `Numpad 9` | Select the `Experimental / Debug Effects` capability class. | Both where supported |
| `Numpad Del` / `Numpad .` | Cycle the subclass inside the currently selected class only. | Selected class |
| `Numpad +` / `Numpad -` | Smooth curated crystal geometry morph forward/backward, independently of selected class. | Both |
| `F1` | Open or toggle the current hotkey/help view. | UI |
| `Escape` | Return to the initial/root menu without resetting visual or crystal state. | UI |
| `F2..F12` | Local crystal modifiers/effects/optical operations only, truthfully documented per key. | Declared per key |
| Cursor-cluster keys | Adjust only their documented numeric parameters and ranges. | Declared per parameter |

### Control Invariants

- Only `G` may switch Classic/Premium via keyboard.
- `Backspace` may not switch philosophy, apply a preset, change an effect, or
  reset optics.
- `Numpad Del` may not morph geometry unless `Geometry` is the selected class.
- `Numpad + / -` always target smooth curated geometry; they may not cycle
  optics, debug, effects, or render philosophy.
- `F2..F12` must never open file browsers, control slideshows, change image
  source, or switch Classic/Premium.
- Invisible states such as `CrystalOff` remain explicit diagnostic/API/menu
  choices only, never ordinary cycle outcomes.
- Help text and tooltips must be changed in the same task as any user-facing
  control change.

### Cursor-Cluster Range Policy

Cursor-cluster assignments must be documented beside the binding.

- Prefer `0..40` for new local effect strengths.
- Existing artistic ranges may remain when explicit and validated, for
  example refractive index `0..10`, crystal-rig intensity `0..20`, or directed
  light `-10..+10`.
- Broad legacy ranges such as mirror rotation or tunnel profile values are
  allowed only when named truthfully in help/UI and owned outside crystal
  effect shortcuts.

---

## Render Philosophy Rules

### Classic

Protect proven Classic behavior:

- mirror system
- kaleidoscope shader behavior
- existing curated form templates
- smooth Classic form transitions

Classic may consume shared capability intent and shared debug effects, but
work must not casually rewrite its established mirror/form language.

Before changing Classic form generation, morph behavior, or its baseline
shader expression, report the necessity, affected files, visual risk, and
validation plan.

### Premium

Premium is the physical-optics interpretation:

- centered, symmetric, readable volumetric forms
- real front/back depth and side faces
- strong facet-driven reflection/refraction
- controlled transmission and visible silhouette
- expressive but stable dispersion, bloom, and atmosphere

Premium geometry cycling must use curated forms only. Invalid shapes must
resolve safely to a documented curated fallback and must never surface as
broken debris or an unclear primitive.

Current curated Premium user-facing geometry inventory:

- Sphere
- Cube
- Octahedron
- Hexahedron
- Volumetric Rhombus
- Cone
- Plate / Disc
- Icosahedron
- Dodecahedron
- Double Pyramid / Bipyramid
- Crystal Lens
- Star Prism

### Absolute Mirror

Absolute Mirror is a hard optical guard:

- `directTransmission = 0`
- `premiumOpticsDirectTransparency = 0`
- opaque, depth-writing, reflective crystal
- selected compatible effects may alter color, highlight, halo, rainbow,
  glimmer, distortion, or roughness without restoring transparency

No menu, preset, debug effect, or renderer fallback may violate this rule.

### Visibility And Readability

User-facing crystal states must remain legible on bright and dark sources.

- No soap-bubble default.
- No almost-invisible normal cycle state.
- Frosted glass must read as matte, abraded, faceted material.
- Stone must retain a solid silhouette.
- Refraction must be normal/facet-driven rather than a transparent viewer-facing
  window.

---

## Preset Contract

Presets are authored runtime states, not UI names.

A real preset declares, where applicable:

- render philosophy
- geometry subclass
- optical subclass
- debug effect and experimental contribution
- material and tint
- transmission/transparency
- reflection/refraction/dispersion/internal reflection
- bloom/glow/brightness/contrast
- scale and atmospheric/chromatic parameters

Preset flow:


Preset Selection
    -> Command
        -> Director / Preset State Owner
            -> Capability Settings
                -> Renderer / Shader Binding


Applying a preset must not erase unrelated experimental state unless the
preset explicitly declares that behavior and the menu communicates it.

---

## Menu, Help, And Audio Truth Rules

Every visible UI control must be classified:

- `REAL_BINDING`: controls a real runtime state.
- `PARTIAL_BINDING`: deliberately incomplete behavior is clearly stated.
- `RESERVED`: visible future control that cannot imply current function.
- `BROKEN`: discovered mismatch requiring repair before release.

Requirements:

- Menu labels, current-value displays, hotkey help, and tooltips must agree.
- Tooltips state affected layer and scope (`Classic`, `Premium`, or `Both`).
- Button/toggle/slider feedback belongs to the central menu audio controller.
- UI audio must never modify visual state or bypass commands.

---

## Protected Systems And Change Risk

Preserve unless a task explicitly targets them:

- Classic mirror/form/morph success paths
- Premium symmetric shape library and stable morph completion
- Absolute Mirror opacity guard
- Experimental Crystal Lab and intentional artifact behavior
- existing Debug Modes and explicit diagnostic access
- file browser and slideshow
- existing tunnel/4D/5D/6D/7D systems
- menu motion stripes and centralized menu feedback

High-risk areas include render pipeline/camera setup, scene-wide rendering,
OutputPreview internals, source/slideshow systems, baseline Classic shader
logic, and broad `RuntimeMenuController` rewrites.

Before a necessary high-risk edit, state:

1. Why it is required.
2. Which file or owner changes.
3. Which behavior is protected.
4. Expected risk.
5. Validation plan.

---

## Permanent Audit Quality Gate

Before implementation:

1. Inspect repository status and respect unrelated work.
2. Locate the real input, command, Director, module, settings, renderer, and
   shader path affected by the task.
3. Identify current menu/help/tooltip claims.
4. Name ownership conflicts, bypasses, fallbacks, or hidden side effects.
5. Define the minimal repair or extension and its invariants.

During implementation:

1. Add or repair command routes before visual/UI shortcuts.
2. Keep state ownership singular.
3. Preserve independent stackable layers.
4. Update help/tooltips for control changes.
5. Add diagnostics/tests proportional to risk.

After implementation:

1. Check compile/shader errors.
2. Exercise affected controls through their command route.
3. Test both render philosophies when a shared capability changed.
4. Test Absolute Mirror when transmission/material logic changed.
5. Test normal cycling for visible, curated outcomes.
6. Test protected neighboring systems relevant to the touched route.

Audit is not a temporary cleanup phase. It is the admission gate for every
future feature and fix.

---

## Required Reporting Format

Every meaningful planning, implementation, or review pass must report:

1. What was audited or inspected.
2. What inconsistencies or requirements were found.
3. Root cause or architectural rationale.
4. What changed or is proposed.
5. Why the change is necessary.
6. Files changed or expected to change.
7. Validation performed or planned.
8. Remaining risks, reserved controls, or incomplete bindings.
9. Final affected runtime control table, including scope and owner.

For visual work also state:

- Whether Classic baseline behavior was modified.
- Whether Premium interpretation changed.
- Whether shared controls work in both philosophies where required.
- Whether Absolute Mirror remains opaque.
- Whether any effect/preset/control remains reserved rather than real.

---

## Final Principle

KAELIS must feel like a professional instrument:

- one layered visual language
- two deliberate render philosophies
- stackable capabilities
- explicit continuous control
- deterministic routing
- truthful UI
- expressive results without architectural chaos
