# ROADMAP.md - KAELIS Layered Visual Instrument Migration

## Destination

KAELIS / Kaleidoscope2 becomes a layered visual synthesizer in which Classic
and Premium interpret one explicit control system:


Render Philosophy
    -> Capability Class
        -> Selected Subclass
            -> Continuous Parameters
                -> Renderer Interpretation


- `Classic` expresses mathematical mirror symmetry.
- `Premium` expresses physical optical materiality.

The migration must not trade artistic richness for architecture. It must give
every valuable behavior a clear owner, command route, menu truth status, and
regression test.

Audit and cleanup are no longer a temporary project phase. They are the
quality gate for every stage below.

---

## Non-Negotiable Invariants

These rules hold throughout the roadmap:

- `G` switches Render Philosophy only.
- `Backspace` toggles crystal visibility only.
- `Numpad 1 / 3 / 7 / 9` select capability classes only.
- `Numpad Del` / `Numpad .` cycles subclasses in the selected class only.
- `Numpad + / -` smoothly morphs curated geometry in Classic and Premium.
- `F1` opens current hotkey/help.
- `Escape` returns to the initial menu without resetting visual state.
- `F2..F12` operate on local crystal modifiers/effects only.
- Cursor-cluster controls mutate documented numeric values only.
- Input and menu UI dispatch commands; they never edit materials directly.
- Presets apply actual state through the Director/state owners.
- Normal cycling never enters invisible, invalid, or placeholder geometry.
- Absolute Mirror remains opaque with zero direct transmission and direct
  transparency.
- Experimental Crystal Lab and intentional artifact character remain
  available.
- File browser, slideshow, tunnel, and higher-dimensional systems remain
  protected from crystal-control side effects.

---

## Deliverable Discipline For Every Stage

Before work:

- create or confirm a rollback point;
- list modified/untracked files and avoid disturbing unrelated work;
- trace input -> command -> Director -> feature owner -> renderer/shader;
- classify affected controls as `REAL_BINDING`, `PARTIAL_BINDING`,
  `RESERVED`, or `BROKEN`.

After work:

- report root causes and ownership decisions;
- list files changed;
- validate compilation and relevant runtime behavior;
- update menu/help/tooltips and the final control table when control truth
  changes;
- document remaining risks and reserved capabilities.

---

## Stage 00 - Safety Snapshot

### Goal

Make experimentation reversible and establish a known baseline.

### Work

- Inspect branch, working tree, recent history, and existing validation logs.
- Identify user-authored or generated changes that must not be overwritten.
- Create a safety branch or checkpoint before broad architectural/rendering
  changes when no appropriate rollback point exists.
- Record protected systems and current known regressions.

### Outputs

- baseline status report;
- protected-behavior list;
- rollback reference;
- initial validation command list.

### Exit Gate

- Dirty state is understood.
- Classic success paths and all protected neighbor systems are named.
- Work can be reversed without guesswork.

---

## Stage 01 - Current Control Audit

### Goal

Produce a truthful map of the instrument as it exists, including remaining
cleanup debt.

### Audit Targets

- all keyboard and mouse bindings;
- menu buttons, selectors, sliders, toggles, help text, and tooltips;
- command enum/factories and command consumers;
- Director state routes;
- feature module ownership;
- material/shader mutation paths;
- preset payload paths;
- geometry transition completion and fallback behavior;
- debug/experimental overlaps;
- file browser/slideshow and unrelated-system side effects.

### Required Findings Table

For each visible control record:

| Control | Claimed Meaning | Actual Command | Actual Owner | Scope | Truth Status | Conflict / Fix |
| --- | --- | --- | --- | --- | --- | --- |

### Exit Gate

- Hidden philosophy switches, bypasses, fake labels, and duplicate owners are
  either fixed or entered as scheduled migration debt.
- A current runtime control table exists.

---

## Stage 02 - Layered Control Model

### Goal

Make the four-layer model explicit in shared state and documentation.

### Model

| Layer | Meaning | Required Owner |
| --- | --- | --- |
| 1. Render Philosophy | `Classic` or `Premium` interpretation | Render philosophy settings/module |
| 2. Capability Class | Geometry, Optics, Debug, Experimental / Debug Effects | Runtime control context owner |
| 3. Subclass | Selected form/mode/effect/preset within a class | Capability-specific settings/module |
| 4. Continuous Parameters | Strength and expression values | Parameter/preset state owners |

### Work

- Confirm or introduce explicit state for active capability class.
- Keep subclass selections independent and stackable.
- Define which concepts are shared intents and which interpretations remain
  philosophy-specific.
- Define guards such as Absolute Mirror without coupling unrelated layers.
- Establish scope metadata for menu and help: `Classic`, `Premium`, `Both`.

### Exit Gate

- No proposal collapses geometry, optics, debug, and experimental effects into
  one ambiguous selection.
- Valid stacked combinations can be represented without hidden state changes.

---

## Stage 03 - Command Routing Cleanup

### Goal

Enforce the block route for all crystal controls and remove control ambiguity.

### Canonical Flow


InputModule / Menu
    -> KaleidoscopeCommand
        -> KaleidoscopeDirector
            -> Feature Module / State Owner
                -> Renderer / Shader


### Work

- Enforce the runtime mapping for `G`, `Backspace`, Numpad controls, `F1`,
  `Escape`, `F2..F12`, and cursor-cluster parameters.
- Route direct geometry shortcuts through smooth morph commands.
- Remove input/menu direct shader/material mutation.
- Remove duplicate state writers and secret cross-layer changes.
- Keep diagnostic-only invisible states out of ordinary cycles.
- Add command-route diagnostics/tests for affected controls.

### Exit Gate

- Each control has exactly one intent and owner.
- No cycling route produces an invalid/invisible user-facing state.
- Plus/minus geometry completion retains its selected final geometry.

---

## Stage 04 - Menu And Help Truth Pass

### Goal

Make the interface an accurate instrument panel rather than a promise.

### Work

- Display the current Render Philosophy and active Capability Class.
- Display selected Geometry, Optics, Debug, and Experimental/Debug Effect
  subclasses independently.
- Ensure every action dispatches its actual command.
- Add scope to relevant labels/tooltips: `Classic`, `Premium`, or `Both`.
- Keep `REAL_BINDING`, `PARTIAL_BINDING`, `RESERVED`, and `BROKEN`
  classifications visible where needed.
- Keep hotkey help synchronized with actual current bindings and documented
  numeric ranges.
- Ensure `F1` and `Escape` remain UI-only routes.

### Exit Gate

- No visible dead button or misleading selector.
- Help, menu, tooltip, command, and runtime result agree.

---

## Stage 05 - Classic/Premium Interpretation Rules

### Goal

Define one instrument language with two deliberate render expressions.

### Classic Interpretation

- Preserve established mathematical mirror system, form templates, and smooth
  morphing.
- Interpret shared geometry/effect commands in its planar symmetry language.
- Avoid casual rewrites of proven baseline Classic shader behavior.

### Premium Interpretation

- Express shared intent through volumetric symmetric geometry and facet-based
  optics.
- Preserve the curated Premium symmetric inventory unless a superior authored
  replacement is deliberately migrated: Sphere, Cube, Octahedron,
  Hexahedron, Volumetric Rhombus, Cone, Plate / Disc, Icosahedron,
  Dodecahedron, Double Pyramid / Bipyramid, Crystal Lens, and Star Prism.
- Require stable steady-state geometry after morph completion.
- Prohibit crude primitive or debris-looking normal-cycle outcomes.

### Shared Material Rules

- Effects selected in shared state must be consumed in both philosophies where
  designated `Both`.
- Weak crystal states require silhouette and facet readability floors.
- Premium default transmission must be gemstone-like rather than a clear
  window.
- Absolute Mirror forces opacity and zero direct transmission regardless of
  stacked compatible effects.

### Exit Gate

- Interpretation differences are intentional and documented.
- Shared commands no longer imply identical rendering code.
- Protected Classic behavior is verified whenever shared rendering changes.

---

## Stage 06 - Capability Class Modules

### Goal

Give each capability class singular ownership and testable behavior.

### Geometry Module

- Own curated shape/form selection and smooth morph transitions.
- Support direct `Numpad + / -` geometry morphing in both philosophies.
- Validate transition and final steady-state mesh/form consistency.

### Optics Module

- Own optical mode selection and optical parameter guards.
- Include High-Purity Diamond, Prism Dispersion, Mirror Facets, Internal
  Reflection, Absolute Mirror, and explicitly authored extensions.
- Own Absolute Mirror enforcement.

### Debug Module

- Preserve existing diagnostics.
- Separate diagnostic inspection from visual philosophy and optical selection.
- Keep invisible states explicit-only.

### Experimental / Debug Effects Module

- Own shared expressive effects and experimental selection behavior.
- Preserve valuable artifact behavior without normalization.
- State clearly whether experimental presets stack with or replace parameters.

### Continuous Parameter Owners

- Assign each exposed slider/hotkey numeric value to exactly one settings
  owner.
- Document range, default, scope, and preset participation.

### Exit Gate

- Each class can be tested without invoking unrelated classes.
- Selected subclasses remain stackable.

---

## Stage 07 - Preset System As Real Authored States

### Goal

Make presets reproducible performances of the layered engine.

### Required Presets

- Diamond Palace
- Blue Ice
- Golden Prism
- Ruby Night
- Emerald Depth
- Opal Dream
- Cosmic Glass
- Dark Luxury
- Absolute Mirror

### Authored Payload Requirements

Each preset declares applicable:

- render philosophy;
- geometry;
- optics;
- debug effect / experimental contribution;
- material/tint;
- transparency and direct transmission;
- reflection, refraction, dispersion, and internal reflections;
- bloom/glow, brightness, contrast, chromatic and atmospheric effects;
- scale.

### Work

- Route preset application through commands and state owners.
- Preserve or explicitly override stackable layers; never overwrite silently.
- Give every preset a meaningful visual delta.
- Make Absolute Mirror preset satisfy the hard opacity guard.

### Exit Gate

- Applying a preset changes actual runtime state, not only UI text.
- Preset state is inspectable, reproducible, and validated.

---

## Stage 08 - Visual Tuning And Readability

### Goal

Tune the instrument for expressive, readable output rather than weak or
invisible materials.

### Work

- Improve Frosted Broken Bottle Glass as matte, sea-worn, faceted material.
- Ensure stone/mineral states remain opaque and readable.
- Strengthen facet-driven refraction, reflection, dispersion, rim response,
  and internal light where appropriate.
- Establish minimum readability behavior for user-facing weak effects.
- Test bright, dark, detailed, and low-contrast source imagery.
- Tune Premium optics boldly while treating Classic baseline behavior as a
  protected reference.

### Visual Acceptance

- No normal user-facing state nearly disappears.
- No Premium default reads as soap-bubble glass.
- High optical controls create visible, stable artistic response.
- Absolute Mirror is unmistakably opaque and reflective.

---

## Stage 09 - UI Audio And Menu Polish

### Goal

Make interaction feel authored without mixing UI presentation with visual
ownership.

### Work

- Keep premium light stripes continuous, subtle, full-screen, softly edged,
  and indefinitely looping.
- Keep button panels readable and clickable above motion layers.
- Centralize button, toggle, selector, and directional slider sound feedback.
- Provide configurable UI audio volume, mild optional pitch variation, and
  anti-spam slider cooldown.
- Ensure missing clips warn gracefully without stopping menu interaction.
- Keep menu motion/audio controllers separate from visual capability state.

### Exit Gate

- UI polish adds feedback only; it does not mutate crystal rendering outside
  dispatched commands.
- Menu remains elegant, responsive, and stable over prolonged display.

---

## Stage 10 - Regression Validation And Final Control Table

### Goal

Certify KAELIS as a deterministic layered instrument.

### Automated/Structural Validation

- No compile or shader errors.
- All affected commands have correct owners/consumers.
- No direct input/menu shader mutation.
- No duplicate owner for selected state or exposed parameter.
- Presets apply real payloads.
- Geometry transitions finish on their requested curated result.

### Runtime Validation Matrix

| Area | Required Checks |
| --- | --- |
| Philosophy | `G` alone switches Classic/Premium; visual state remains coherent. |
| Visibility | `Backspace` changes visibility only. |
| Classes | `Numpad 1 / 3 / 7 / 9` select their declared classes. |
| Subclass cycle | `Numpad Del / .` cycles only the selected class; no invisible outcome. |
| Geometry shortcut | `Numpad + / -` smoothly morphs curated geometry in Classic and Premium. |
| Help/navigation | `F1` opens current help; `Escape` returns to root without state reset. |
| Local modifiers | `F2..F12` affect only documented crystal-local functions. |
| Parameters | Cursor-cluster values remain within documented ranges. |
| Premium shapes | All symmetric user-facing shapes remain visible and stable. |
| Effects | Shared effects operate in both philosophies where scoped `Both`. |
| Absolute Mirror | Transparency and direct transmission remain zero under stacked effects/presets. |
| Experimental | Crystal Lab and intentional artifact expression remain available. |
| Neighbors | File browser, slideshow, tunnel/4D/5D/6D/7D remain intact. |
| Menu polish | Stripes loop continuously; centralized UI audio responds correctly without spam. |

### Final Control Table Template

| Control | Layer | Command | Owner | Scope | Menu/Tooltip Location | Validation Status |
| --- | --- | --- | --- | --- | --- | --- |
| `G` | Render Philosophy | TBD/current audited command | Philosophy owner | Both | Help + mode status | Pending/Pass |
| `Backspace` | Visibility | TBD/current audited command | Visibility owner | Both | Help + crystal status | Pending/Pass |
| `Numpad 1` | Capability Class | TBD/current audited command | Control-context owner | Both | Help/HUD | Pending/Pass |
| `Numpad 3` | Capability Class | TBD/current audited command | Control-context owner | Declared | Help/HUD | Pending/Pass |
| `Numpad 7` | Capability Class | TBD/current audited command | Control-context owner | Both | Help/HUD | Pending/Pass |
| `Numpad 9` | Capability Class | TBD/current audited command | Control-context owner | Both | Help/HUD | Pending/Pass |
| `Numpad Del / .` | Subclass | TBD/current audited command | Selected class owner | Selected | Help/HUD | Pending/Pass |
| `Numpad + / -` | Geometry | TBD/current audited command | Geometry owner | Both | Help/geometry row | Pending/Pass |
| `F1` | UI | TBD/current audited command | Menu navigation owner | UI | Help hint | Pending/Pass |
| `Escape` | UI | TBD/current audited command | Menu navigation owner | UI | Help hint | Pending/Pass |

### Exit Gate

- Final runtime control table is filled with real command names and pass
  results.
- Known risks are explicit.
- No architectural-quality failure is waived silently.

---

## Ongoing Rule After Migration

New features enter KAELIS only through this question sequence:

1. Which layer does this belong to?
2. Who owns its state?
3. Which command carries intent?
4. How do Classic and Premium interpret it?
5. What menu/help/tooltip truth changes?
6. What invariant and regression test prove it did not create chaos?

If these questions cannot be answered clearly, the feature is not ready to be
implemented.
