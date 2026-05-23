# ROADMAP.md — KAELIS Menu Actions Roadmap

## Strategy

The visual foundation of the KAELIS startup menu is now good enough to begin interaction work.

The next goal is:

```text
Make the menu functional without damaging the rendering engine.
```

The menu must become an interaction shell with clean section panels and safe command routing.

---

# PHASE 0 — Safety Checkpoint

Goal: start from a safe state.

Tasks:

- run git status;
- confirm current branch;
- commit current visual menu state if approved;
- verify changed files are limited to Menu/**;
- read AGENTS.md;
- read ROADMAP.md.

Acceptance:

- rollback point exists;
- no protected systems are dirty.

---

# PHASE 1 — Action Audit

Goal: understand current input/action paths before wiring buttons.

Tasks:

- inspect startup menu button callbacks;
- inspect middle mouse click handling;
- identify what action middle mouse click currently triggers;
- identify existing public command methods;
- identify whether RuntimeMenuController has safe public methods;
- identify any existing mode switch API;
- identify existing settings/preset APIs if present.

Report:

```text
Button/action path found
Safe public methods available
Unsafe direct module access to avoid
Real bindings possible now
Placeholders required
```

Acceptance:

- Enter Experience target action is known;
- safe command boundary is clear.

---

# PHASE 2 — Menu Action Router

Goal: create a clean button action layer.

Create menu-only class:

```text
KaelisMenuActionRouter
```

Responsibilities:

- receive button actions;
- open/close sections;
- route Enter Experience;
- route Exit confirmation;
- expose DemoMode state without implementing Demo Mode;
- keep one active section at a time;
- call command bridge when real runtime action is safe.

Acceptance:

- menu buttons no longer contain large inline logic;
- action flow is centralized.

---

# PHASE 3 — Command Bridge

Goal: safely connect menu UI to existing runtime actions.

Create menu-only class if needed:

```text
KaelisMenuCommandBridge
```

Responsibilities:

- call existing safe public runtime commands;
- provide clear placeholder logs when safe binding does not exist;
- avoid direct manipulation of protected modules.

Important:

Do not invent duplicate render behavior.

Acceptance:

- Enter Experience uses same action as middle mouse click where possible;
- unsafe bindings are not faked.

---

# PHASE 4 — Section Controller

Goal: implement section navigation.

Create:

```text
KaelisMenuSectionController
```

Responsibilities:

- register section panels;
- show one active section at a time;
- hide previous section;
- handle neutral/closed state;
- provide soft fade/slide transitions.

Sections:

```text
Modes
Optics
Presets
Settings
Exit
```

Demo Mode is excluded from section implementation for now.

Acceptance:

- clicking menu buttons opens corresponding panels;
- only one panel visible;
- transitions feel premium.

---

# PHASE 5 — Modes Panel

Goal: create functional Modes UI shell.

Panel content:

```text
Classic 2D
Premium 3D Crystal
4D Tunnel / Funnel
5D Endless Flight
Experimental / Coming Soon
```

Each entry should include:

- title;
- short description;
- selected/active state;
- disabled state if not implemented.

Binding rule:

- bind only if safe public command exists;
- otherwise placeholder log.

Acceptance:

- Modes button opens Modes panel;
- cards look premium;
- no rendering modules directly modified.

---

# PHASE 6 — Optics Panel

Goal: create visual optics controls shell.

Controls:

```text
Refraction
Reflection
Prism Dispersion
Facet Highlights
Bloom / Glow
Caustics
Background Distortion
Crystal Transparency
```

Control types:

- premium sliders;
- toggles;
- small info labels;
- disabled state when unsupported.

Binding rule:

- bind only to safe public APIs;
- otherwise placeholder log.

Acceptance:

- Optics panel exists;
- controls are visually consistent;
- no shader/render hack.

---

# PHASE 7 — Presets Panel

Goal: create presets shell.

Content:

```text
Factory Presets
User Presets
Apply
Save Current
Rename
Delete
Reset Factory
```

Initial presets may be placeholders:

```text
Diamond Palace
Blue Ice
Golden Prism
Ruby Night
Emerald Depth
Opal Dream
Dark Luxury
Cosmic Glass
```

Binding rule:

- do not implement persistence unless safe preset system exists;
- placeholders acceptable.

Acceptance:

- Presets button opens panel;
- cards/list look premium;
- no fake persistence claim.

---

# PHASE 8 — Settings Panel

Goal: create system settings shell.

Groups:

```text
DISPLAY
- Resolution
- Fullscreen
- VSync
- Target FPS

AUDIO
- Master Volume
- Menu Volume
- Demo Volume placeholder

CONTROLS
- Mouse Wheel Crystal Scale
- Hotkeys
- UI Scale

SYSTEM
- Show FPS
- Show Diagnostics
- Reset Settings
```

Binding rule:

- safe settings may be bound;
- otherwise placeholders.

Acceptance:

- Settings panel exists;
- categories are clear;
- no visual optics mixed into Settings.

---

# PHASE 9 — Exit Confirmation

Goal: prevent accidental exit.

Behavior:

- Exit button opens confirmation panel;
- panel says “Exit KAELIS?”;
- buttons:
  - Cancel;
  - Exit Application.
- Cancel closes panel;
- Exit Application quits in build and logs in editor.

Acceptance:

- no instant quit from first click;
- confirmation looks premium.

---

# PHASE 10 — Demo Mode Reserved

Goal: keep Demo Mode untouched until dedicated task.

Allowed:

- toggle state visual;
- status text update;
- placeholder log.

Forbidden for now:

- audio playback wiring;
- image cycling;
- preset demo choreography;
- demo timeline;
- demo content automation.

Acceptance:

- Demo Mode not accidentally implemented halfway.

---

# PHASE 11 — Smoke Tests

Goal: protect menu action system.

Update/create:

```text
KaelisMenuActionSmokeTest
```

Test:

- Enter Experience callback exists;
- Modes opens Modes panel;
- Optics opens Optics panel;
- Presets opens Presets panel;
- Settings opens Settings panel;
- Exit opens confirmation;
- Cancel closes Exit panel;
- only one section visible at a time;
- Demo Mode remains state-only;
- menu hierarchy builds.

Acceptance:

- smoke test passes.

---

# PHASE 12 — Visual Interaction Polish

Goal: make action panels feel part of KAELIS.

Tasks:

- panel fade/slide transitions;
- hover states;
- selected section indicator;
- glass overlay styling;
- clear section titles;
- back/close affordance if needed.

Acceptance:

- section panels look premium;
- no debug UI;
- no ugly default Unity controls.

---

# PHASE 13 — Report And Commit

Goal: finish safely.

Required final report:

```text
Files changed
Section architecture
Button actions
Real bindings vs placeholders
Protected systems untouched
Compile result
Smoke test result
git diff --name-only
```

Commit message suggestion:

```text
Add KAELIS startup menu action sections
```

---

## Current Active Task

Next Codex task:

```text
Plan and implement menu action routing and section panels for Modes, Optics, Presets, Settings, and Exit. Keep Demo Mode reserved. Make Enter Experience call the same action as middle mouse click.
```

---

## Forbidden Throughout

Do not touch:

```text
Classic2D
Premium3D rendering
DiamondFocus
crystal shaders
Mirror/**
Source/**
RuntimeMenuController internals
OutputPreview internals
cameras
render pipelines
```

---

## Allowed Throughout

Inside Menu/**, Codex may:

```text
create action router
create section controller
create section panels
create command bridge
create menu smoke tests
adjust menu layout for panels
add menu-only transitions
add placeholder controls
```

---

## Final Principle

First make the menu safe and functional.

Then bind the real engine controls one by one in separate tasks.
