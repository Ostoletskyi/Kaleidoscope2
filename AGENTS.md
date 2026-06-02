# AGENTS.md - KAELIS Layered Visual Instrument Doctrine

## Purpose

KAELIS / Kaleidoscope2 is a visual instrument and visual synthesizer.

It is not a collection of unrelated visual switches. Every feature must fit a
layered control model with explicit ownership, truthful controls, and
deterministic routing.

The instrument may create strong visual fixation and motion aftereffects when
viewed continuously. Comfort and safety are therefore product capabilities,
not disclaimers or optional cleanup. Demonstration and benchmarking tools must
be transparent, reversible, and routed through the same public architecture as
ordinary performance controls.

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

Comfort/Safety is not a third rendering philosophy. It is an independently
enabled constraint and transition layer that may safely limit resolved motion
or visual intensity without secretly changing the selected philosophy,
capability, subclass, or authored preset.

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
- mouse-wheel visual scale: owned by shared wheel-scale state, resolved as
  Classic crystal overlay scale or Premium crystal scale through the public
  command route in every runtime mode where that crystal presentation is
  visible; mouse wheel must not alter mirror/kaleidoscope zoom, which remains
  owned by Arrow Up / Arrow Down
- rotation, motion speed, and atmospheric distortion

Rules:

- Parameters need a single state owner and a documented range.
- Menu and keyboard input dispatch parameter intent only.
- Renderers and shaders consume resolved values; they do not invent control
  ownership.
- Mouse-wheel scaling is a `Both`-scope Layer 4 crystal control. Classic
  resolves to bounded Diamond Focus crystal overlay scale, not mirror zoom or
  scene zoom; Premium resolves to bounded crystal scale. The menu toggle/step
  labels must remain truthful for both.
- Values outside a preferred `0..40` performance range require a stated
  artistic or legacy justification and a documented bound.

### Layer 5 - Comfort / Safety Constraints

Owns active viewing-comfort rules that mediate potentially fatiguing output
while preserving the user's selected visual intent wherever safe.

Responsibilities:

- reduce sustained central fixation, excessive motion speed, aggressive
  strobing, abrupt intensity changes, and prolonged forced one-direction
  motion;
- apply explicit comfort caps and smooth transition policies when a comfort
  session or reduced-motion preference is active;
- expose which safety rule is active and which resolved parameter it limits;
- provide a visible, reliable exit route from controlled sessions.

Ownership:

- `ComfortSafetyManager` owns comfort constraints and resolves capped/smoothed
  safety values; it does not own Render Philosophy or capability selection.
- `MeditationModeController` owns the Meditation session timeline.
- `CrystalSplitComfortController` owns the reversible detach/orbit/re-form
  anti-fixation presentation as a `CrystalFormationMode` /
  `SixCopyOrbitFormation` behavior.
- Feature owners retain the authored/requested state; safety owners may
  temporarily constrain the resolved state only while the safety mode applies.

Rules:

- Comfort rules must be deterministic, inspectable, and independently
  testable.
- An active guard modifies only unsafe resolved values; it must not silently
  replace a selected effect, shape, optical mode, or philosophy.
- Disabling or exiting a temporary safety-controlled session restores captured
  state through `SettingsRestoreService`.
- Normal Classic, Premium, and 3D behavior remains unchanged while comfort
  features are inactive.

### Session Tools - Not Visual State Layers

`Meditation Mode`, `Replay Demo`, and `Benchmark Demo` are temporary,
reversible sessions. They coordinate commands and constraints, but do not form
another renderer, state machine, or capability enum.

Every such session must:

- open a dedicated setup/preview panel first and issue no session-changing
  command until its visible `START` action is pressed;
- capture a full `SettingsSnapshot` before issuing changing commands;
- declare whether successful completion restores previous state or documented
  defaults;
- restore safely on cancellation, exception, disable, or failed startup;
- prevent overlapping sessions unless their interaction has been explicitly
  authored and validated;
- hide non-essential runtime/menu/help frames after successful start so the
  visual presentation remains primary;
- display only useful minimal active status and an exit instruction.

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


InputModule / RuntimeMenuController / Semantic Demo Source
    -> KaleidoscopeCommand
        -> KaleidoscopeDirector
            -> Feature Module / State Owner
                -> ComfortSafetyManager (active constraints only)
                -> Renderer / Material Binder
                    -> Shader


Responsibilities:

| Block | Responsibility |
| --- | --- |
| `InputModule` | Read physical input and emit commands only. |
| `RuntimeMenuController` and menu views | Display state, tooltip scope, and dispatch commands only. |
| `DemoPanel` / `DemoMenuController` | Display Demo tools/status and dispatch session commands only. |
| `VisualSessionUiController` | Hide non-essential UI after successful session start and display minimal Meditation/Replay exit status only. |
| `CleanViewController` | Toggle non-essential overlay visibility from `H` without altering visual or session state. |
| `KaleidoscopeCommand` | Describe intent with explicit payloads. |
| `KaleidoscopeDirector` | Route commands and coordinate state ownership only. |
| Feature modules/settings | Own capability logic, selected subclasses, guards, transitions, and resolved values. |
| `ComfortSafetyManager` | Resolve active comfort limits and safe easing without selecting visual modes. |
| Session controllers | Orchestrate Meditation, Replay, or Benchmark timelines using public commands. |
| `SettingsSnapshot` / `SettingsRestoreService` | Capture reversible session state and safely restore it. |
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
- Meditation, replay, or benchmark controllers directly mutating a renderer,
  material, shader, or mesh where a command/state-owner route exists.
- Any temporary session failing to restore prior/default state according to
  its declared completion and failure policy.
- A benchmark or comfort mode leaving extreme, capped, or split state active
  after it has exited.

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
| `Numpad 5` while Premium3D is active | Trigger Premium crystal brake / camera-facing align / 3-second freeze through `TriggerPremiumCrystalStabilization`; it must restore prior motion after release. | Premium |
| `Mouse Wheel` | Scale the visible crystal presentation through the shared wheel-scale owner: Classic crystal overlay or Premium crystal size. It must not alter mirror zoom, camera, tunnel, source image, or global scene scale. | Both |
| Top-row `1` | Cycle mirror presets `3 -> 4 -> 6 -> 4 -> 3 ...` through `TopRowMirrorCountCycle`. | Both |
| Top-row `2` | Cycle mirror presets `8 -> 10 -> 12 -> 10 -> 8 ...` through `TopRowMirrorCountCycle`. | Both |
| Top-row `3` | Cycle mirror presets `14 -> 16 -> 24 -> 16 -> 14 ...` through `TopRowMirrorCountCycle`. | Both |
| Top-row `4` | Cycle mirror presets `28 -> 36 -> 48 -> 36 -> 28 ...` through `TopRowMirrorCountCycle`. | Both |
| Top-row `5..9` | Keep the existing direct mirror counts `96 / 192 / 384 / 768 / 1536`. | Both |
| `F1` | Open or toggle the current hotkey/help view. | UI |
| `H` | Toggle clean view for non-essential menu/help/status/HUD overlays only. | UI |
| `Escape` | Return to the initial/root menu without resetting visual or crystal state. | UI |
| `F2..F12` | Local crystal modifiers/effects/optical operations only, truthfully documented per key. | Declared per key |
| Cursor-cluster keys | Adjust only their documented numeric parameters and ranges. | Declared per parameter |
| `Meditation Mode` menu button/tab | Open the Meditation setup panel only; do not alter visual/audio state. | UI |
| `Meditation Mode > START` | Capture state, hide non-essential UI, and enter the comfort-governed Meditation session. | Both |
| `Demo > Replay Demo` | Open the Replay setup panel only; do not start playback. | UI |
| `Replay Demo > START` | If actions exist, capture state, hide non-essential UI, and enter semantic action replay. | Both |
| `Demo > Benchmark Demo` | Open the dedicated visual-performance setup panel only. | UI |
| `Benchmark Demo > START` | Capture state, hide runtime panels, and run the declared 60-second benchmark/results flow. | Both |
| `Escape` / `Mouse Wheel Press` during a temporary visual session | Stop the active session and restore its captured visual/source/audio/UI state. | Meditation / Replay / Benchmark |

### Control Invariants

- Only `G` may switch Classic/Premium via keyboard.
- `Backspace` may not switch philosophy, apply a preset, change an effect, or
  reset optics.
- `Numpad Del` may not morph geometry unless `Geometry` is the selected class.
- `Numpad + / -` always target smooth curated geometry; they may not cycle
  optics, debug, effects, or render philosophy.
- Top-row mirror-count changes must route through state owners and render with
  a `1.0` second `MirrorCountTransitionState` crossfade between from/to
  mirror configurations; they must not jump by animating the integer count.
- `F2..F12` must never open file browsers, control slideshows, change image
  source, or switch Classic/Premium.
- Invisible states such as `CrystalOff` remain explicit diagnostic/API/menu
  choices only, never ordinary cycle outcomes.
- Help text and tooltips must be changed in the same task as any user-facing
  control change.
- `H` may hide or reveal overlay presentation only; it must not stop a
  session, alter renderer/source/audio state, or stop benchmark measurement.
- Outside an active temporary session, `Escape` retains its UI-only behavior.
  While Meditation, Replay Demo, or Benchmark Demo is active, the documented
  exit action may cancel that session and restore its captured state; this is
  session cleanup, not a general visual reset.
- Replay Demo may not intercept real user input invisibly; active replay and
  its stop controls must be visibly indicated.

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
- Premium material/effect cycling must not expose ugly white, empty, or cheap
  placeholder surfaces. Legacy weak names may be preserved as serialized enum
  values only when their labels and profiles resolve to authored premium glass,
  gem, mirror, stone, or metal looks.

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

## Settings Persistence Contract

Persistent settings store stable user preferences only. They are not an input
recording or replay log.

Ownership:

- `SettingsPersistenceService` owns loading, sanitizing, autosaving, manual
  saving, explicit reset, and corruption recovery for versioned
  `KaelisSettingsData`.
- The settings file lives under `Application.persistentDataPath`.
- Raw key/action history belongs only to Replay Demo / `InputRecorder`.

Rules:

- Missing settings files create safe defaults.
- Corrupted settings files must be backed up/ignored and replaced with safe
  defaults without crashing.
- Auto Save writes supported stable preferences after user-originated state
  changes only; temporary Meditation/Replay/Benchmark commands must not become
  startup defaults.
- Reset Settings / Factory Reset may clear or rewrite the settings file only
  after an explicit user reset action, never automatically on stop/restart.
- Menu labels must distinguish real persisted preferences from still-reserved
  display/audio/input services.

---

## Comfort And Safety Contract

Safety and comfort are first-class systems because prolonged centered,
high-motion visuals can cause fatigue or motion aftereffects. This contract
governs new comfort experiences and any future safety-limited preset.

### General Visual Safety Rules

Avoid:

- sudden full-screen flashes or aggressive strobing;
- instant split/merge geometry transitions;
- prolonged forced center fixation;
- uncontrolled speed spikes or direction reversals;
- benchmark extrema remaining active after the run.

Prefer:

- eased motion ramps and reversals;
- crossfades or continuous interpolation;
- reversible transitions driven by explicit session state;
- visible exit controls and status;
- documented comfort caps.

### Meditation Mode

The menu must expose a truthful `Meditation Mode` button/tab that opens a
setup/preview panel. Opening the panel is UI-only; the temporary
comfort-governed session begins only from its visible `START` control.

When enabled, `MeditationModeController` must:

- capture a full `SettingsSnapshot` before any state change;
- request automatic hiding of runtime frames/help only after startup succeeds,
  retaining a minimal exit instruction while the visuals play;
- request the default illustration folder through the normal source command
  route;
- request sequential, looping playback of the curated DemoContent audio
  playlist through the normal audio command route, continuing silently with a
  non-intrusive status if no valid track is available;
- enable applicable `ComfortSafetyManager` limits;
- cap kaleidoscope rotation speed at `1.5` rotations per second;
- run for `1` minute in one direction, then force `1` minute in the opposite
  direction, repeating while the mode remains active;
- smoothly cycle speed from `1.5` down to `0.25` rotations per second and
  back to `1.5` over each `10` second breathing period;
- generate gentle session-owned semantic W/A/S/D movements with occasional
  paired directions, rare `0.1` second Q/E-equivalent pulses, weighted
  mirror-count variation favoring lower digits, and a reliable semantic image
  reset every `40` seconds;
- request the preferred crystal presentation basis by routing the semantic
  equivalent of `Numpad 7` followed by `Numpad Del` during session setup;
- keep ordinary controls available unless a requested value violates active
  comfort limits;
- use easing for speed and directional transitions with no abrupt jumps;
- expose an immediate visible exit and restore the captured state on exit or
  failure.

Direction changes may be mediated through a smooth zero-crossing or equivalent
eased transition, but the minute-by-minute alternation must remain observable
and deterministic.

### Crystal Formation Comfort Pattern

`CrystalSplitComfortController` reduces continuous central fixation without
changing the selected crystal concept. This is a Crystal Formation Behavior,
not a shape, material, optical mode, debug effect, or input shortcut.

While its parent comfort session enables the pattern:

- Classic formation starts with one whole Classic crystal, performs one smooth
  pre-split self-rotation intent through the same semantic diamond-rotation
  route used by Numpad `4` / `6`, then reveals `6` full intact copies of the
  Classic crystal; it must never geometry-morph, cut, slice, stretch, squash,
  UV-rotate, distort, or replace the Classic crystal with fragments, wedges,
  shards, debris, or primitive placeholders;
- Premium formation starts from one solid Premium crystal and transforms into
  `6` full Premium crystal copies that share the selected Premium mesh and
  material/optics; controlled duplicates are preferred over mesh splitting
  when splitting would produce broken topology; it must never use broken mesh
  debris, random shards, invisible stand-ins, blobs, or white primitive
  placeholders;
- Premium six-copy mode is implemented as adaptive motion-design morphing with
  component separation: `PremiumComfortFormationMorphState` resolves
  detaching/orbiting/merging phase values, `PremiumComfortFormationLayout`
  computes size-aware viewport-safe placement, and
  `PremiumComfortFormationComponentAnimator` resolves per-copy transform,
  scale, alpha, and intensity interpolation for full-mesh components;
- Premium six-copy orbit exposes exactly `6` visible crystal copy renderers;
  the primary crystal is a split/merge bridge only and is hidden during the
  `Orbiting` phase so no residual center or seventh crystal remains;
- Premium crystal scale ceiling is `700%`, an additive `+100%` extension over
  the prior raised `600%` ceiling, and clamps must continue to reference the
  shared scale constant;
- Premium large-scale rendering uses bounds-based depth protection against the
  background/kaleidoscope plane: `PremiumCrystalDepthProtection` compares the
  back-most point of each primary/copy bounds to the protected plane, applies
  the configured safe clearance, and the final `SpatialCrystalStage3D`
  writer moves that object toward the camera after all Premium scale/layout
  writers and before `RenderStageCamera`; correction must be dynamic from
  actual penetration, not clipped to the legacy smoothing constant;
- Premium fullscreen-scale material safety keeps large non-Absolute-Mirror
  crystals glass-like by reducing wall-like alpha/brightness/specular energy
  while preserving transparent/refractive transmission through the visible
  kaleidoscope scene;
- `Numpad 5` Premium stabilization is a command-routed temporary state:
  `InputModule` emits `TriggerPremiumCrystalStabilization`,
  `DiamondFocusSettings` owns the align/freeze timer, `DiamondRotationController`
  brakes rotation while active, and the Premium renderer/stage consumes the
  state to freeze the six-copy formation and align full crystals to the screen;
- after detaching, the units complete one eased orbit near the outer
  composition; Premium copies must remain inside viewport `x/y` `0.15..0.85`
  by calculating orbit radius from current copy scale, estimated visual bounds,
  viewport aspect, and safe margin, then reducing copy scale when needed;
- after the complete revolution, they converge and re-form one coherent
  crystal;
- child units use authored orientation, phase, brightness, and drift
  differences while preserving full-copy readability;
- the authored rhythm should remain calm: about `2.5` seconds of Classic
  semantic pre-rotation, `2.5` seconds to detach, `5` seconds for one orbit,
  and `2.5` seconds to re-form;
- motion must read as a coherent unfolding/transformation, never as an
  explosion or gear-like clone array;
- split and merge must use continuous easing with no hard flicker or
  strobe-like frame transition;
- disabling, exiting, or failing the session returns to a coherent single
  crystal or the captured prior state.

The controller owns only the comfort presentation arrangement. It must not
overwrite selected Geometry, Optics, Debug, Experimental effect, or Render
Philosophy state.

---

## Demo And Benchmark Contract

The menu must expose a `Demo` tab/button containing two independent tools:
`Replay Demo` and `Benchmark Demo`. Neither tool may masquerade as normal user
input, modify shader/material state directly, or share mutable timeline state
with the other.

### Replay Demo

`InputRecorder` and `DemoReplayController` provide a reversible performance
replay:

- selecting Replay opens a dedicated setup panel without changing state;
- pressing `START` starts playback only when semantic actions exist, otherwise
  the panel remains visible with a truthful unavailable status;
- retain the last `500` user control actions in a bounded ring buffer;
- store semantic commands and payloads as the authoritative record, including
  action type, press duration where applicable, timestamp/delta timing,
  repetition frequency, and effect toggle state;
- raw key/button input may be retained only as optional debug metadata;
- if at least one but fewer than `500` recorded actions exists, duplicate or
  extend the chronological recorded sequence deterministically until the
  playback list contains `500` entries;
- if no semantic actions exist, Replay remains unavailable or uses a clearly
  labeled authored safe demo sequence; it must not silently invent a recording;
- loop playback like a music box until the user exits;
- stop by default on `Escape` or mouse wheel press;
- dispatch replayed semantic intent through the same public
  `KaleidoscopeCommand` / `KaleidoscopeDirector` route used by genuine input
  wherever that route exists;
- capture state before playback and restore it on stop or failure.

The recorder observes dispatched user-originated commands without changing
their behavior. It must distinguish playback-generated commands to prevent a
replay loop from recording itself.

### Benchmark Demo

`BenchmarkController`, `BenchmarkMetrics`, and `BenchmarkResultView` own a
transparent `60` second demonstration and measurement run:

- selecting Benchmark Demo first opens a dedicated setup panel; no visual
  state or session lease changes until the visible `START` action is pressed;
- capture a full `SettingsSnapshot` when `START` is pressed and before any
  benchmark-owned state change;
- automatically hide runtime menu frames during measured playback so the
  curated visuals remain the primary presentation;
- sequentially enable and disable all declared major modes/effects through
  public commands;
- sweep relevant settings from documented minimum to maximum using safe,
  visible transitions;
- display an upper-left live HUD with elapsed/total time, current phase,
  current FPS, running average FPS, peak FPS, and live `1% low FPS`;
- permit `H` to hide/show the live HUD during the active run without pausing
  collection or suppressing the final results state;
- collect average FPS, peak FPS, and `1% low FPS` / first-percentile FPS using
  a documented sampling method;
- on successful completion, restore the captured pre-benchmark settings,
  show the results screen, and allow saving the results to a timestamped file;
- on cancellation, startup failure, or runtime exception, restore the captured
  pre-benchmark state safely;
- never leave extreme swept settings active after completion or interruption.

Saved benchmark output must include date/time, Unity version if available,
resolution, average FPS, peak FPS, `1% low FPS`, and active rendering
mode/pipeline information if available.

### Snapshot / Restore Rule

`SettingsSnapshot` and `SettingsRestoreService` are shared infrastructure for
temporary sessions. The snapshot must include every setting a session may
modify, including where available:

- render philosophy/visual mode and active capability selections;
- geometry, optics, debug/experimental modes, active effects, and preset
  contributions;
- motion direction, rotation speed, comfort limits, transitions, and split
  presentation state;
- material and numeric visual parameters touched by a run;
- source mode, image folder/file/slideshow state, audio folder/file/playback
  state, and relevant UI session state.

Restore must be idempotent, safe during partial startup/failure, and routed
through state owners or public commands. A session may not start changing
state until snapshot capture succeeds.

### Required Module Isolation

Implement these as separate testable responsibilities, not as one giant
`MonoBehaviour`:

| Required Module | Sole Responsibility |
| --- | --- |
| `ComfortSafetyManager` | Active comfort constraints, caps, and safe easing policies. |
| `MeditationModeController` | Meditation session lifecycle and timed motion/audio/source intent. |
| `CrystalSplitComfortController` | Soft anti-fixation `SixCopyOrbitFormation`: Classic six full copies and Premium six full mesh copies detach/orbit/re-form. |
| `DemoPanel` / `DemoMenuController` | Demo UI, truthful state display, and session command dispatch. |
| `VisualSessionUiController` | Successful-session menu/help hiding and minimal Meditation/Replay status HUD. |
| `CleanViewController` | Central `H` visibility toggle for non-essential overlays and live HUD presentation. |
| `InputRecorder` | Last-500 semantic user-command ring buffer and timing metadata. |
| `DemoReplayController` | Build/replay/stop semantic playback sequences. |
| `BenchmarkController` | Execute the declared timed feature/sweep sequence and cleanup. |
| `BenchmarkMetrics` | Sample FPS and calculate required reported metrics. |
| `BenchmarkResultView` | Display live benchmark HUD/results and request result-file saving. |
| `SettingsSnapshot` / `SettingsRestoreService` | Capture and restore complete temporary-session state. |

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
- Menu background crystal interaction maps must use normalized coordinates
  owned by the menu FX controller. Moving light stripes may trigger prism/lens
  flare or camera glow only when they cross the declared crystal line/zone; the
  overlay must remain above the background, below UI, and non-raycasting.
- `Meditation Mode`, `Replay Demo`, and `Benchmark Demo` must show active,
  stopped, unavailable, or results status truthfully.
- Demo timer, live FPS, metrics definitions, stop controls, and result-save
  action must be visible whenever they are relevant.

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
- public input/command routing and semantic control ownership
- shader/material binding pipeline and existing serialized inspector links

High-risk areas include render pipeline/camera setup, scene-wide rendering,
OutputPreview internals, source/slideshow systems, baseline Classic shader
logic, broad `RuntimeMenuController` rewrites, and attempts to implement
comfort/demo sessions directly in renderer or input monoliths.

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
6. For Comfort, Meditation, Replay, or Benchmark work, identify the snapshot
   surface, restoration policy, safe exit path, and any currently reserved UI.

During implementation:

1. Add or repair command routes before visual/UI shortcuts.
2. Keep state ownership singular.
3. Preserve independent stackable layers.
4. Update help/tooltips for control changes.
5. Add diagnostics/tests proportional to risk.
6. Keep temporary sessions isolated, semantic-command-driven, and reversible.

After implementation:

1. Check compile/shader errors.
2. Exercise affected controls through their command route.
3. Test both render philosophies when a shared capability changed.
4. Test Absolute Mirror when transmission/material logic changed.
5. Test normal cycling for visible, curated outcomes.
6. Test protected neighboring systems relevant to the touched route.
7. Test comfort caps, easing, snapshot restore, and visible exit behavior for
   every changed temporary session.
8. Test Replay does not self-record and Benchmark cannot leave extreme state
   active.

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
10. Snapshot/restore coverage and exit/failure policy for temporary-session
    work.

For visual work also state:

- Whether Classic baseline behavior was modified.
- Whether Premium interpretation changed.
- Whether shared controls work in both philosophies where required.
- Whether Absolute Mirror remains opaque.
- Whether any effect/preset/control remains reserved rather than real.
- Whether comfort constraints were active and which resolved values they cap.
- Whether Meditation split/merge remains smooth and reduces fixed-center
  presentation without changing selected crystal ownership.
- Whether Demo/Benchmark restore policies were executed and verified.

---

## Final Principle

KAELIS must feel like a professional instrument:

- one layered visual language
- two deliberate render philosophies
- stackable capabilities
- explicit continuous control
- deterministic routing
- truthful UI
- responsible comfort constraints and reversible demonstration sessions
- expressive results without architectural chaos
