# ROADMAP.md - KAELIS Comfort, Meditation, Demo, And Benchmark Expansion

## Destination

KAELIS / Kaleidoscope2 remains a layered visual instrument in which Classic
and Premium/3D interpret explicit visual controls. It now also becomes a
responsible long-viewing experience with isolated Comfort/Safety, Meditation,
Replay Demo, and Benchmark Demo systems.

The new systems must reduce avoidable fatigue and central fixation, demonstrate
features transparently, and restore state predictably. They must not rewrite
the core renderer, bypass the command route, or destabilize existing artistic
behavior.

Canonical runtime direction:


User Input / Menu / Semantic Replay or Benchmark Source
    -> KaleidoscopeCommand
        -> KaleidoscopeDirector
            -> Existing Feature State Owners
                -> ComfortSafetyManager (active resolved-value limits only)
                    -> Existing Renderer / Material Binder / Shader


Temporary-session lifecycle:


Start Meditation / Replay / Benchmark
    -> Capture SettingsSnapshot
        -> Dispatch Public Commands
            -> Run Isolated Timeline
                -> Restore Previous Or Documented Default State
                    -> Show Stopped / Results Status


---

## Existing Integration Candidates To Audit

These are known candidate integration surfaces discovered during documentation
preparation. Stage 00 must verify their current behavior before implementation;
this list is not permission to modify them broadly.

| Concern | Candidate Existing Surface | Planned Relationship |
| --- | --- | --- |
| Physical input | `Assets/_Project/Kaleidoscope2/Input/InputModule.cs` | Keep physical input dispatching commands; observe semantic user commands for recording. |
| Command and routing | `Assets/_Project/Kaleidoscope2/Core/KaleidoscopeCommand.cs`, `KaleidoscopeDirector.cs` | Extend public intents only as required; route all session-issued visual actions here. |
| Shared runtime state | `Assets/_Project/Kaleidoscope2/Core/KaleidoscopeState.cs` | Audit snapshot scope and existing motion state before defining full restore. |
| Runtime menu | `Assets/_Project/Kaleidoscope2/Control/RuntimeMenuController.cs` | Add truthful session controls only through menu-command routing. |
| Startup/menu bridge | `Assets/_Project/Kaleidoscope2/Menu/KaelisStartupMenuController.cs`, `Menu/Runtime/KaelisMenuCommandBridge.cs` | Replace the currently reserved Demo affordance only when real demo behavior exists. |
| Image source/folder | `Assets/_Project/Kaleidoscope2/Source/SourceModule.cs` | Meditation requests default illustrations through existing source commands. |
| Audio loading/playback | `Assets/_Project/Kaleidoscope2/AudioReactive/AudioReactiveModule.cs` | Meditation requests an ordered, looping curated playlist through existing audio commands; Benchmark remains silent. |
| FPS/status | `Assets/_Project/Kaleidoscope2/Diagnostics/DiagnosticsModule.cs`, `DebugHUD.cs` | Reuse or extend measurement/display ownership for benchmark reporting. |
| Crystal state/effects | Existing DiamondFocus/settings modules | Snapshot touched visual state; keep Classic/Premium/3D behavior owned by existing modules. |

---

## Non-Negotiable Invariants

These rules remain in force throughout all stages:

- Classic, Premium, and existing 3D kaleidoscope behavior remain valid and
  independently controllable when new sessions are inactive.
- File browser, slideshow/source selection, audio routing, command routing,
  shader/material binding, serialized inspector links, and existing visual
  controls are protected.
- `G` switches Render Philosophy only; `Backspace` controls visibility only.
- Existing Numpad, `F1`, `F2..F12`, cursor-cluster, and geometry morph
  contracts in `AGENTS.md` remain authoritative.
- Menu/input/session code dispatches public semantic commands; it does not
  mutate mesh, material, shader, or renderer state directly when an owner
  route exists.
- Comfort/Safety constraints mediate only unsafe resolved output while active;
  they do not silently replace user selections or renderer philosophy.
- No comfort or demo implementation may introduce sudden flashes, aggressive
  strobing, instant split/merge, uncontrolled speed spikes, or forced
  prolonged center fixation.
- Meditation, Replay Demo, and Benchmark Demo capture a complete
  `SettingsSnapshot` before making temporary changes.
- Cancellation and failure always restore the captured state safely.
- Benchmark success restores its documented default baseline before results
  are shown; early exit/failure restores pre-run state.
- Absolute Mirror remains opaque with zero direct transmission and direct
  transparency whenever relevant visual behavior is exercised.
- Existing experimental/artifact behavior remains available unless an active,
  disclosed safety constraint limits only an unsafe resolved parameter.

---

## Required New Module Boundaries

Implementation must use separated, testable responsibilities and preserve
existing serialized fields and inspector links.

| Module | Responsibility | Must Not Own |
| --- | --- | --- |
| `ComfortSafetyManager` | Comfort caps, reduced-motion constraints, safe eased transition policies. | Render philosophy, visual subclass selection, renderer internals. |
| `MeditationModeController` | Session start/stop and timed source/audio/motion requests. | Audio decoding, image loading, shader values. |
| `CrystalSplitComfortController` | Crystal Formation Behavior owner for `SixCopyOrbitFormation`: Classic full-copy choreography and Premium full-mesh copy transformation. | Core geometry selection, optical mode, material mode, debug effect, or input ownership. |
| `DemoPanel` / `DemoMenuController` | UI display and session command dispatch. | Playback timelines or visual state. |
| `VisualSessionUiController` | Shared setup-success presentation hiding and minimal Meditation/Replay exit HUD. | Session lifecycle, renderer, source, or audio state. |
| `CleanViewController` | Global `H` clean-view visibility for non-essential overlays. | Visual, source, audio, session, or benchmark metric state. |
| `InputRecorder` | Last-500 semantic user-action ring buffer and timing metadata. | Replaying actions or changing state. |
| `DemoReplayController` | Construct, run, loop, and cancel semantic replay. | Raw-input-only recording or renderer mutation. |
| `BenchmarkController` | Orchestrate 60-second feature sequence, cleanup, and result transition. | Metric mathematics or UI rendering. |
| `BenchmarkMetrics` | FPS samples, average, peak, and 1% low calculation. | Feature toggles or visual control. |
| `BenchmarkResultView` | Results presentation and save request/status. | Benchmark state mutation. |
| `SettingsSnapshot` / `SettingsRestoreService` | Full reversible capture and idempotent restoration. | New visual behaviors. |
| `SettingsPersistenceService` / `KaelisSettingsData` | Versioned persistent user preferences in `Application.persistentDataPath`; load, autosave, manual save, explicit reset, and corruption recovery. | Raw input logs, Replay recording, temporary session snapshots, or unrelated display/audio services. |

---

## Deliverable Discipline For Every Stage

Before implementation:

- inspect `git status` and preserve unrelated work;
- identify input, menu, command, Director, state-owner, renderer/binder, and
  shader implications for the stage;
- identify visible menu/help/tooltip claims and any `RESERVED` controls being
  made real;
- state planned integration points, snapshot scope, exit/failure policy, and
  protected systems;
- prefer additive modules and minimally extended public commands over broad
  controller or renderer rewrites.

After implementation:

- report what was audited, the root architectural reason, and files changed;
- report command routes and owning modules for all newly real controls;
- validate compile/runtime behavior, user exit, and state restoration;
- validate both render philosophies where a shared behavior is touched;
- validate Absolute Mirror when optical/material state can be encountered;
- validate protected neighbors relevant to the change;
- update menu text/tooltips and the affected runtime control table;
- identify remaining reserved controls, deferred work, and risks.

---

## Stage 00 - Audit Existing Architecture

### Goal

Map the existing integration points before any feature code changes.

### Work

- Read existing menu, startup menu, input, command, Director, visual
  controller/state/settings, renderer/binder, and shader paths relevant to
  temporary visual sessions.
- Read source-folder/image-loading and audio-loading/playback ownership for
  Meditation defaults.
- Read FPS/status display and sampling behavior for Benchmark reuse.
- Locate any current Demo/Meditation UI and classify each item as
  `REAL_BINDING`, `PARTIAL_BINDING`, `RESERVED`, or `BROKEN`.
- Locate existing snapshot-like behavior and identify missing state coverage.
- Check how Classic, Premium, and 3D modes expose motion speed/direction and
  how Absolute Mirror is enforced.

### Output

- list of files/classes involved and safest integration points;
- current command-route map for source, audio, visual parameters, and FPS;
- snapshot coverage matrix and ownership conflicts;
- protected-system and regression-risk list;
- proposed command additions, if any, without code changes.

### Exit Gate

- No code changes have been made in this stage.
- A minimal implementation boundary for each new module is documented.
- Any high-risk edit has a specific necessity and validation plan.

---

## Stage 01 - Settings Snapshot/Restore Foundation

### Goal

Make all temporary sessions safely reversible before they can change the
instrument.

### Work

- Create `SettingsSnapshot` and `SettingsRestoreService` as dedicated
  infrastructure.
- Capture every state that Meditation, Replay, or Benchmark can modify:
  render/visual mode, capability selections, active effects/presets, motion
  direction and speed, numeric parameters, source folder/file/mode, audio
  folder/file/playback state, comfort state, and relevant session UI state
  where available.
- Restore through public commands or owning settings services in a safe,
  deterministic order.
- Define completion policies: Meditation and Replay restore the previous
  snapshot; Benchmark success restores documented defaults; Benchmark
  cancellation/failure restores its prior snapshot.
- Make restore idempotent and safe after partial startup.

### Validation

- Apply temporary representative changes, restore them, and confirm no visual,
  source, audio, motion, effect, or UI-session artifacts remain.
- Exercise restore during simulated early cancellation/failure.
- Confirm existing visual control operation is unchanged when no session runs.

### Exit Gate

- No temporary mode can begin state changes unless a snapshot was captured.
- Restore coverage and omissions, if any, are explicit.

---

## Stage 02 - Comfort/Safety Manager

### Goal

Introduce centralized, inspectable comfort limits without stealing visual
ownership from existing modules.

### Work

- Add `ComfortSafetyManager`.
- Own an active max rotation speed cap, smooth speed-change policy, optional
  reduced-motion constraints, and reusable safe transition/easing helpers.
- Resolve unsafe requested values only when a relevant comfort policy is
  active; keep original selected modes/effects intact.
- Expose active cap/status to UI and diagnostics where useful.
- Define a comfort cap suitable for Meditation at a maximum of `1.5`
  rotations per second.

### Validation

- With comfort disabled, existing controls retain their current resolved
  behavior.
- With comfort enabled, unsafe speed requests clamp cleanly and smoothly.
- No cap operation switches Classic/Premium/3D mode, geometry, optics, debug,
  effect, source, or audio state.

### Exit Gate

- Comfort rules have one owner and are independently testable.
- No renderer or shader is made a hidden safety state owner.

---

## Stage 03 - Meditation Mode

### Goal

Deliver a user-facing guided visual session governed by comfort limits.

### Work

- Add the truthful `Meditation Mode` menu button/tab and
  `MeditationModeController`.
- Open a dedicated Meditation setup/preview panel first; dispatch session
  start only from a clear `START` action.
- Capture a `SettingsSnapshot` before session startup.
- Load curated illustration assets and start the ordered, looping DemoContent
  audio playlist through existing source/audio command owners.
- Enable comfort limits with rotation speed capped at `1.5` rotations per
  second.
- Run `1` minute in direction A, then `1` minute in direction B, repeating
  while active.
- During each repeating `10` second breathing cycle, smoothly ramp speed
  `1.5 -> 0.25 -> 1.5` rotations per second.
- Emit gentle semantic W/A/S/D motion activity, paired movements where
  authored, rare exact `0.1` second Q/E-equivalent pulses, weighted
  low-digit-favoring mirror variation, and semantic image reanimation every
  `40` seconds.
- Apply the preferred session-safe crystal basis through semantic
  `Numpad 7` then `Numpad Del` command equivalents.
- Use eased directional reversals and smooth speed transitions with no abrupt
  jumps.
- Keep ordinary functions available unless they violate the active comfort
  cap.
- Show active status and an obvious exit; restore prior state on stop or
  failure.
- Hide menu/help frames after successful START and retain only a minimal
  session exit label while the visual session runs.

### Validation

- Session loads default source/audio using public routes and can be exited
  cleanly.
- Direction and breathing-cycle timing are deterministic.
- Speed remains in the breathing envelope of `0.25..1.5` rotations per second
  away from eased direction zero crossings.
- Exit and failure restore the state captured before startup.

### Exit Gate

- The menu describes actual behavior and comfort scope.
- Shared behavior is checked in Classic and Premium/3D paths that support it.

---

## Stage 04 - Crystal Split Comfort Pattern

### Goal

Reduce sustained center fixation through a soft, rhythmic crystal
presentation.

### Work

- Add `CrystalSplitComfortController` as the isolated Crystal Formation
  Behavior owner enabled by the applicable comfort session.
- Classic path: keep one whole Classic crystal visible, request the pre-split
  self-rotation through the semantic diamond-rotation route, then reveal `6`
  full intact Classic crystal copies. Never geometry-morph, UV-rotate, deform,
  slice, stretch, squash, fragment, or replace Classic with debris.
- Premium path: transform the one solid Premium crystal into `6` full Premium
  mesh copies sharing the selected mesh/material/optics, orbit once, then
  rejoin. Use controlled duplicates instead of ugly mesh splitting; never use
  primitives, blobs, broken topology, or placeholder copies.
- Keep the calm choreography near `2.5` seconds pre-rotation, `2.5` seconds
  detach, `5` seconds orbit, and `2.5` seconds rejoin.
- Keep Premium copies inside the visible safe area `x/y = 0.15..0.85` by
  calculating orbit radius from camera/viewport bounds, current visual copy
  scale, estimated bounds, aspect ratio, and six-copy spacing; reduce copy
  scale when a larger orbit would violate the safe margin.
- Give full copies phase/orientation offsets and subtle drift while preserving
  readable full-crystal identity.
- Author the transition as a coherent unfolding and re-formation without a
  flash, strobe, instant spawn, or explosive movement.
- Restore a coherent single crystal/prior captured state on session exit or
  failure.

### Validation

- No hard flash or instantaneous split/merge is visible.
- Copy variation is subtle, equal-sized, and does not create six unrelated
  effects.
- Existing selected geometry, optics, debug/effect states, Classic/Premium/3D
  mode, and Absolute Mirror invariants remain owned and valid.

### Exit Gate

- The split pattern is reversible and isolated from core geometry selection.

---

## Stage 05 - Demo Replay Recorder

### Goal

Record real performance intent without altering normal interaction.

### Work

- Add `InputRecorder` with a bounded last-`500` action ring buffer.
- Record dispatched user-originated semantic commands and payloads, including
  action type, time/delta timing, press duration where meaningful, frequency,
  and effect toggle state.
- Allow raw key/button information only as non-authoritative debug metadata.
- Tag command origin so replay-generated commands cannot record themselves.

### Validation

- User actions still dispatch exactly once and behave as before.
- The buffer retains the newest `500` semantic actions in chronological
  playback order.
- Payloads and timing metadata can reproduce toggles and held/adjusted input.

### Exit Gate

- Recorder behavior is observational and command-semantic, not raw-key-only.

---

## Stage 06 - Demo Replay Playback

### Goal

Provide transparent looping replay using normal public command routing.

### Work

- Add `DemoPanel` / `DemoMenuController` Replay entry and
  `DemoReplayController`.
- Open a dedicated Replay setup/preview panel first; do not dispatch playback
  until the visible `START` action is pressed.
- Capture a `SettingsSnapshot` before replay begins.
- If `1..499` actions are present, deterministically repeat/extend the ordered
  sequence until exactly `500` playback entries are available.
- If zero actions exist, keep Replay unavailable or require a clearly labeled
  authored safe demo sequence; do not pretend it is a recording.
- If START is pressed with zero actions, keep the setup panel visible and
  report the unavailable reason truthfully.
- Replay semantic commands through `KaleidoscopeDirector` in timing order and
  loop like a music box until stopped.
- Expose active status and default stop controls: `Escape` and mouse wheel
  press.
- Restore the captured state on stop or failure.

### Validation

- Replay reaches existing feature owners through public commands.
- Replay does not self-record or mutate low-level visual state directly.
- Both default stop actions exit promptly and restore prior state.

### Exit Gate

- Demo Replay is a real, truthful UI binding with deterministic playback.

---

## Stage 07 - Benchmark Demo

### Goal

Run a transparent 60-second feature demonstration and measure performance.

### Work

- Add `BenchmarkController`, `BenchmarkMetrics`, and `BenchmarkResultView`.
- Open a dedicated Benchmark setup panel first; dispatch the run only from a
  clear `START` action.
- Capture a `SettingsSnapshot` after `START` is pressed and before benchmark
  playback changes any runtime state.
- Define a deterministic `60` second sequence that toggles declared major
  modes/effects on and off and sweeps relevant documented settings from
  minimum to maximum through public commands.
- Apply smooth transitions and safety constraints needed to avoid flashes,
  strobing, uncontrolled speed, or unreadable presentation.
- Hide runtime menu panels during playback and display an upper-left HUD with
  elapsed/total time, phase name, live current/average/peak FPS, and live
  `1% low FPS`.
- Collect average FPS, peak FPS, and `1% low FPS` / first-percentile FPS using
  a documented frame-sampling calculation.
- On success, restore documented default settings before presenting results.
- On cancellation or failure, restore the pre-run snapshot safely.

### Validation

- Sequence duration, command sequence, and sampled metrics are reproducible.
- UI timer/FPS updates through its declared diagnostics/result ownership.
- Completion and cancellation cannot leave maximum/extreme settings active.
- Relevant Classic, Premium/3D, source/browser, and Absolute Mirror behavior
  remains coherent.

### Exit Gate

- Results are shown only after safe cleanup has completed.

---

## Stage 08 - Save Benchmark Result

### Goal

Make measurement results portable and honest.

### Work

- Let `BenchmarkResultView` request saving a timestamped result file through a
  dedicated save path/service selected by the implementation audit.
- Include date/time, Unity version if available, resolution, average FPS, peak
  FPS, `1% low FPS`, and active rendering mode/pipeline information if
  available.
- Include sequence/version identity and note any unavailable fields rather
  than guessing.
- Report save success or failure in the results view without altering visual
  state.

### Validation

- A completed result can be saved and read back with the required fields.
- File-save failure leaves results visible and application state stable.

### Exit Gate

- Saved metrics correspond to the completed cleaned-up benchmark run.

---

## Stage 09 - Polish And Documentation

### Goal

Finish the feature set with truthful messaging and regression confidence.

### Work

- Add user-facing descriptions for Meditation Mode purpose, comfort limits,
  Replay Demo behavior and stop keys, Benchmark purpose/metrics, and the
  safety/comfort note.
- Update menu labels, help, tooltips, localization, current-state indicators,
  and reserved/real classifications together.
- Keep shared mouse-wheel visual scale truthful for both Classic crystal
  overlay size and Premium crystal size through the existing command/state
  owner route in every runtime mode where the target crystal is visible.
  Classic wheel scale must not reuse mirror zoom, and Arrow Up / Down remain
  the mirror/kaleidoscope zoom controls.
- Persist stable user preferences through versioned JSON under
  `Application.persistentDataPath`; do not mix Replay/InputRecorder raw action
  history into settings, and reset the file only through explicit reset.
- Keep Premium material/effect cycling free of ugly white, empty, or cheap
  placeholder surfaces by resolving weak legacy enum values to authored
  premium glass, mirror, gem, stone, or metal profiles.
- Maintain the startup-menu crystal interaction map as normalized rect plus
  trigger line/zone data. Moving light stripes may trigger lens flare/camera
  glow only through the non-raycasting menu FX controller and shader overlay.
- Add short developer notes for module boundaries, snapshot coverage, command
  origin tracking, metric definitions, and restoration policies.
- Validate the full regression matrix and record any still-reserved behavior.

### Validation Matrix

| Area | Required Checks |
| --- | --- |
| Core control contract | Existing keyboard/menu controls retain their documented single meanings. |
| Mouse-wheel visual scale | Shared wheel toggle/step updates `KaleidoscopeState`; Classic wheel resolves to bounded Diamond Focus crystal overlay size in any runtime mode where the Classic overlay is visible, Premium wheel resolves to bounded crystal size, Arrow Up / Down still own mirror zoom, and neither route steals the other's active philosophy. |
| Protected visuals | Classic baseline and Premium/3D interpretations remain coherent while sessions are inactive. |
| Premium surface quality | `Opal Prism Glass`, `Liquid Mercury Mirror`, `Brushed Steel Mirror`, `Chrome Facet Mirror`, `Blackened Iron Facets`, and `Polished Brass Prism` remain visible, authored, and non-empty; Absolute Mirror opacity remains hard-guarded. |
| Meditation | Selection opens setup without state change; START begins curated image/playlist route and hides frames; `0.25 <-> 1.5` breathing cycle, semantic gentle activity, and minute direction reversal are smooth; exit restores state/UI. |
| Comfort cap | Active unsafe speed requests are clamped; inactive mode does not alter normal control resolution. |
| Crystal formation | Classic uses six medium full copies with no fragments; Premium transforms one whole crystal into six full mesh copies, sizes orbit radius from copy bounds/scale, avoids crowding, stays inside the 15% viewport margin, and rejoins smoothly. |
| Settings persistence | `SettingsPersistenceService` creates/loads versioned JSON, persists stable visual/crystal/menu preferences, survives restart, backs up corrupt files, ignores temporary-session commands, and never writes raw Replay/InputRecorder history. |
| Menu crystal interaction map | Startup-menu background crystal map uses normalized rect plus trigger line/zone; stripe crossing produces visible lens/camera glow through a non-raycasting overlay above the background and below UI. |
| Replay recording | Latest 500 semantic actions and timing metadata are captured without behavior changes or self-recording. |
| Replay playback | Selection opens setup without state change; START uses public command routing only when data exists; zero-action behavior is truthful; `Escape` and mouse wheel press stop and restore UI/state. |
| Benchmark | Existing setup/START flow uses shared presentation hiding; 60-second sequence shows timer/FPS, collects average, peak, and 1% low, and restores before results. |
| Clean view | `H` hides/shows non-essential overlays in normal and active-session presentation; Benchmark metrics continue collecting while HUD is hidden. |
| Benchmark saving | Timestamped file contains required available fields and handles failure safely. |
| Optics guard | Absolute Mirror remains opaque if touched by a session. |
| Neighbor systems | Browser/slideshow, audio, input, shader pipeline, existing visual controls, and serialized links remain intact. |

### Exit Gate

- All new visible functionality is `REAL_BINDING` or clearly labeled
  unavailable/reserved.
- Validation results, remaining risks, and deferred capabilities are written
  down.

---

## Affected Runtime Control Table

This is the required final reporting surface for the new work. Command names
are intentionally pending until Stage 00 audits the existing route and each
implementation stage supplies real commands.

| Control / Surface | Layer | Intended Command Route | Owner | Scope | Status Before Implementation |
| --- | --- | --- | --- | --- | --- |
| `Meditation Mode` button/tab | Setup selection | Menu -> dedicated setup panel, no session command | `DemoPanel` / menu controller | UI | Implemented |
| `Meditation Mode > START` | Comfort session | Menu -> command -> Director -> `MeditationModeController` | `MeditationModeController` | Both where motion is supported | Implemented |
| Meditation motion limit | Comfort constraint | Session intent -> `ComfortSafetyManager` -> resolved speed | `ComfortSafetyManager` | Active Meditation | Implemented |
| Meditation `SixCopyOrbitFormation` | Crystal Formation Behavior | Session intent -> formation controller -> existing render presentation boundary | `CrystalSplitComfortController` | Active Meditation/declared comfort use | Implemented / corrected |
| `Demo` tab/button | UI/session selection | Menu -> setup selections only | `DemoPanel` / `DemoMenuController` | UI | Implemented |
| `Replay Demo` | Demo setup | Menu -> dedicated replay setup panel, no session command | `DemoPanel` / menu controller | UI | Implemented |
| `Replay Demo > START` | Demo session | Menu -> command -> `DemoReplayController` -> public semantic commands | `DemoReplayController` | Both | Implemented |
| `Escape` during active Replay | Session exit | Input -> stop-session command -> restore service | `DemoReplayController` / `SettingsRestoreService` | Replay only | Planned exception to normal UI navigation |
| Mouse wheel press during Replay | Session exit | Input -> stop-session command -> restore service | `DemoReplayController` / `SettingsRestoreService` | Replay only | Planned |
| `Benchmark Demo` | Demo setup | Menu -> dedicated benchmark panel, no session start | `DemoPanel` / menu controller | UI | Implemented |
| `Benchmark Demo > START` | Demo session | Menu -> command -> `BenchmarkController` -> public commands | `BenchmarkController` | Both where features are declared | Implemented |
| Visual-session hiding/minimal HUD | Presentation UI | Successful start -> public UI visibility commands / minimal status | `VisualSessionUiController` | Meditation / Replay / Benchmark hiding | Implemented |
| Benchmark live HUD/results | Diagnostics/result UI | Metrics/display update route | `BenchmarkMetrics` / `BenchmarkResultView` | Benchmark only | Implemented |
| `H` clean view | UI visibility | Input -> `ToggleCleanView` -> Director -> overlay consumers | `CleanViewController` | Normal/Meditation/Replay/Benchmark | Implemented |
| Mouse wheel over visual output | Layer 4 crystal scale | Input -> `AdjustClassicCrystalScalePercent` (Classic) / `AdjustPremiumCrystalScalePercent` (Premium) | `KaleidoscopeState` plus `DiamondFocusSettings` | Both | Implemented / updated |
| Auto Save Settings | Persistent preferences | User command -> `SettingsPersistenceService` -> versioned JSON in `Application.persistentDataPath` | `SettingsPersistenceService` / `KaelisSettingsData` | Stable user settings | Implemented / foundation |
| Reset Settings | Persistent preferences | Explicit reset command only; menu remains disabled until confirmation exists | `SettingsPersistenceService` / menu confirmation owner | Stable user settings | Service implemented, UI confirmation reserved |
| Premium material/effect cycling | Layer 3/4 material interpretation | Material mode -> shared settings/profile library -> renderer/material binder | `DiamondFocusSettings` / `CrystalSharedSettings` | Premium | Implemented / updated |
| Menu background light stripe crossing crystal map | UI FX | `PremiumMenuMotionController` beam sample -> normalized map in `PremiumMenuPrismReactionController` -> shader flare | `PremiumMenuPrismReactionController` | UI | Implemented / updated |
| Save benchmark result | Result action | Results UI -> save service | `BenchmarkResultView` | Benchmark results | Planned |

Existing `G`, `Backspace`, Numpad, `F1`, `F2..F12`, cursor-cluster, and
ordinary `Escape` control invariants remain governed by `AGENTS.md` and must
be included in regression reporting when touched.

---

## Ongoing Admission Rule

Each implementation pass must answer:

1. Which existing owner and public command route does this integrate with?
2. Which safety or session module owns the new responsibility?
3. What complete state is captured before temporary change?
4. What is restored on successful completion, cancellation, and failure?
5. How is central fixation, abrupt motion, strobing, or extreme state
   prevented?
6. Which Classic, Premium/3D, Absolute Mirror, browser, input, audio, and
   shader-pipeline regression checks prove the addition is controlled?

If those answers are incomplete, the implementation is not ready to begin.
