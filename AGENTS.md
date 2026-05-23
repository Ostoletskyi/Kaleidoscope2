# AGENTS.md — KAELIS Menu Actions Mode

## 0. Main Mission

The startup menu visuals are now close enough to continue with interaction architecture.

The new priority is:

```text
Turn KAELIS menu buttons into real, safe, well-structured actions.
```

The menu must stop being only a beautiful shell. Each button must open or trigger the correct menu section/action through a clean command layer.

This document replaces the previous “visual-only” menu focus.

---

## 1. Non-Negotiable Protected Systems

Do not modify these systems directly during menu action work:

```text
Classic2D rendering
Premium3D rendering
DiamondFocus
crystal shaders
Mirror/**
Source/**
RuntimeMenuController internals
OutputPreview internals
camera/render pipeline logic
kaleidoscope render modules
```

Exception: a menu button may call an already existing public command/method if that method is clearly intended for user interaction.

Do not duplicate render logic inside menu code.

---

## 2. Menu Is Allowed To Change

Codex may freely modify:

```text
Assets/_Project/Kaleidoscope2/Menu/**
```

Allowed inside Menu/**:

- menu section panels;
- button callbacks;
- menu navigation;
- UI state management;
- menu-only overlays;
- menu-only animations;
- menu-only command adapters;
- menu-only placeholder panels;
- menu-only tests;
- menu-only asset helpers.

Codex may create new menu-only files such as:

```text
KaelisMenuActionRouter.cs
KaelisMenuSectionController.cs
KaelisMenuSectionPanel.cs
KaelisModesPanel.cs
KaelisOpticsPanel.cs
KaelisPresetsPanel.cs
KaelisSettingsPanel.cs
KaelisExitPanel.cs
KaelisMenuCommandBridge.cs
KaelisMenuActionSmokeTest.cs
```

---

## 3. Architecture Rule

Menu buttons must not directly poke random runtime objects.

Preferred architecture:

```text
Button click
    -> KaelisMenuActionRouter
        -> SectionController or CommandBridge
            -> safe public runtime command OR placeholder
```

Do not let button callbacks contain large business logic.

Bad:

```csharp
button.onClick.AddListener(() => {
    FindObjectOfType<SomeModule>().someField = 10;
    FindObjectOfType<AnotherModule>().Reset();
    Camera.main.enabled = false;
});
```

Good:

```csharp
button.onClick.AddListener(() =>
    actionRouter.OpenSection(KaelisMenuSection.Modes));
```

or:

```csharp
button.onClick.AddListener(() =>
    actionRouter.TriggerEnterExperience());
```

---

## 4. Button Responsibilities

### ENTER EXPERIENCE

Purpose: start the main user interaction.

Required behavior:
- trigger the same underlying action as middle mouse click, if that is the current primary runtime menu/action;
- do not create a parallel fake implementation;
- reuse the same safe public command/method;
- hide or transition the startup menu only if that is part of the same intended behavior.

If the middle mouse click currently opens/toggles the runtime control menu, Enter Experience must do the same.

### DEMO MODE

Demo Mode is intentionally excluded from this action wiring stage.

Do not implement full Demo Mode yet.

Allowed:
- keep current toggle state;
- show placeholder status;
- prepare future hook only.

Do not wire audio/images/presets until a dedicated Demo Mode task.

### MODES

Purpose: open the Modes section panel.

This section should contain mode choices such as:

```text
Classic 2D
Premium 3D Crystal
4D Tunnel / Funnel
5D Endless Flight
Experimental / Coming Soon
```

Only bind to real mode switching if a safe public command already exists.

Otherwise use clear placeholders and logs.

### OPTICS

Purpose: open the Optics section panel.

This section contains visual/optical controls such as:

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

Do not directly modify protected shaders or render modules unless a safe public parameter API already exists.

If safe binding is unclear, create placeholder controls.

### PRESETS

Purpose: open the Presets section panel.

This section contains:

```text
Factory Presets
User Presets
Apply
Save Current
Rename
Delete
Reset Factory
```

Do not invent persistence unless the project already has a safe preset system.

Placeholders are acceptable.

### SETTINGS

Purpose: open the Settings section panel.

This section contains application/system settings:

```text
Display
Audio
Controls
System
Diagnostics
```

Settings must not become a dumping ground for visual optics. Visual controls belong in Optics.

### EXIT

Purpose: open an exit confirmation panel.

Required:
- first click on Exit opens confirmation;
- do not immediately quit;
- panel shows Cancel and Exit Application;
- Cancel returns to previous/neutral menu state;
- Exit Application calls Application.Quit in build and logs in Editor.

---

## 5. Section System Rule

Only one section panel may be open at a time.

Expected behavior:

```text
Click MODES    -> Modes panel opens
Click OPTICS   -> Optics panel replaces Modes panel
Click PRESETS  -> Presets panel replaces current panel
Click SETTINGS -> Settings panel replaces current panel
Click EXIT     -> Exit confirmation replaces current panel or overlays it
ESC            -> follows existing startup menu behavior
```

The right preview panel should remain visually present whenever possible.

Section panels should feel like premium glass overlays, not debug UI.

---

## 6. Real Binding vs Placeholder Rule

Codex must clearly distinguish:

```text
REAL BINDING
PLACEHOLDER
UNSAFE / NEEDS FUTURE TASK
```

Do not pretend a placeholder changes the engine.

If a button only logs intent, report it honestly.

Example report:

```text
MODES / Classic2D: placeholder only; no safe public command found.
OPTICS / Bloom: placeholder only; real binding deferred.
ENTER EXPERIENCE: real binding; calls same path as middle mouse click.
```

---

## 7. Visual Direction For Panels

New section panels must match current KAELIS style:

- blue/cyan glass;
- gold accents;
- gemstone luxury;
- clear typography;
- no Unity-default controls;
- no debug wireframe;
- no flat grey panels;
- no noisy technical clutter.

Controls should be visually premium even when placeholders.

---

## 8. Safety Rules

Before implementation:

- run git status;
- read AGENTS.md and ROADMAP.md;
- inspect current menu action code;
- trace middle mouse click action before wiring Enter Experience.

During implementation:

- keep changes inside Menu/** if possible;
- do not touch protected rendering systems;
- do not use GameObject.Find or Camera.main unless already project-approved;
- avoid FindObjectOfType unless there is no safer existing reference;
- prefer serialized references or explicit command bridge;
- keep menu tests updated.

After implementation:

- run compile;
- run menu smoke test;
- run git diff --name-only;
- report protected path check;
- report real bindings vs placeholders.

---

## 9. Validation Requirements

A task is not done unless:

- all main menu buttons have explicit behavior;
- Demo Mode is left intentionally unchanged except safe state display;
- Enter Experience reuses the same action path as middle mouse click;
- section switching works;
- Exit confirmation works;
- only one section panel is open at a time;
- no protected systems changed;
- compile passes;
- smoke test covers button actions.

---

## 10. Final Principle

Do not build a beautiful dead menu.

Do not wire buttons by hacking render modules.

Build a clean action layer between the beautiful KAELIS menu and the engine.
