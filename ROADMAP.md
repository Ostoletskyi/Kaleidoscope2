# ROADMAP.md — KAELIS Control Cleanup Roadmap

## Goal

Restore order in the control/effect system.

Current issue:
The crystal/effect system has grown powerful but chaotic.
Modes, effects, debug controls, premium options, and presets overlap.
Some controls trigger unexpected behavior.

Target:
A clean modular control architecture where every key, menu row, and effect has one clear owner.

---

## Stage 00 — Safety Snapshot

Before editing:

- Run git status.
- Create safety branch or commit.
- Do not delete working visual effects.
- Preserve current experimental/artifact behavior.

Required:
git status
git branch backup-before-control-cleanup

---

## Stage 01 — Full Control Audit

Audit all input/control paths.

Search for:
- KeyCode.G
- KeyCode.Backspace
- Keypad1
- Keypad3
- Keypad7
- Keypad9
- KeypadPeriod
- KeyCode.Delete
- F1..F12
- Plus
- Minus
- Insert
- Home
- End
- PageUp
- PageDown
- Arrow keys

For each binding document:
- file
- method
- current action
- intended action
- conflict yes/no

No functional changes yet unless compile is broken.

---

## Stage 02 — Command Ownership Cleanup

Review KaleidoscopeCommand.

Remove or deprecate unclear commands.
Add missing explicit commands if needed.

Required commands should be clear:

- ToggleClassicPremiumMode
- ToggleCrystalEnabled
- SelectCrystalControlClass
- CycleActiveCrystalSubclass
- CyclePremiumShape
- CyclePremiumOpticalMode
- CycleCrystalDebugMode
- CycleCrystalDebugEffect
- SetCrystalDebugEffect
- SetPremiumShape
- SetPremiumOpticalMode

No command should have hidden side effects.

---

## Stage 03 — Numpad Control Context System

Implement or repair active control context.

Required:

Numpad 1:
Select Class 1.

Numpad 3:
Select Class 2.

Numpad 7:
Select Class 3.

Numpad 9:
Select Class 4.

Numpad Del / ".":
Cycle currently selected class.

Suggested class mapping:

Class 1:
Premium Crystal Shapes.

Class 2:
Premium Optical Modes.

Class 3:
Crystal Debug Modes.

Class 4:
Crystal Debug Effects / Experimental Effects.

Rules:
- Class selection must not change the effect immediately unless explicitly intended.
- Class selection only changes active context.
- Numpad Del performs the actual cycling.
- Menu must display active class.

---

## Stage 04 — Classic/Premium Mode Lockdown

Only G may switch Classic/Premium.

Audit and remove all other Classic/Premium switching paths.

Validation:
- Pressing G switches Classic/Premium.
- Pressing Numpad 1/3/7/9 does not switch Classic/Premium.
- Pressing F keys does not switch Classic/Premium.
- Menu buttons for Classic/Premium must clearly state their role if they exist.

---

## Stage 05 — Function Key Cleanup

Function keys are reserved for local crystal effects only.

Audit F1..F12.

Allowed:
- local crystal effect toggles;
- local crystal visual parameters;
- debug/effect actions directly related to crystal.

Forbidden:
- global mode switching;
- file browser;
- source mode;
- Classic/Premium switching;
- hidden unrelated state changes.

Document final F-key map in menu.

---

## Stage 06 — Cursor-Cluster Range System

Keys above cursor must operate in range 0..40.

Audit:
- Insert
- Delete
- Home
- End
- PageUp
- PageDown
- arrow-adjacent cluster if used

Required:
- Each controlled value clamps to 0..40.
- Menu displays current value.
- Tooltip explains what the value controls.
- No hidden values outside the documented range.

---

## Stage 07 — Menu Truth Pass

Every menu row must match real command behavior.

For each menu control:
- label must be accurate;
- current value must be real;
- hotkey hint must be correct;
- tooltip must explain scope;
- button must dispatch command only;
- no direct shader/material mutation.

Add sections:

Controls:
- Mode
- Crystal Toggle
- Active Numpad Class
- Premium Shape
- Premium Optical Mode
- Debug Mode
- Debug Effect
- Experimental Preset
- Function Keys
- Cursor Range Controls

---

## Stage 08 — Remove Fake / Duplicate / Conflicting Bindings

Search for:
- direct material mutations from UI;
- direct state mutations from InputModule;
- duplicate debug cycling;
- multiple handlers for the same key;
- menu-only fake labels;
- unused commands;
- obsolete fallback behavior.

Fix or remove.

If removal is risky:
- mark obsolete;
- disconnect from runtime;
- document.

---

## Stage 09 — Effect System Stabilization

Ensure:
- Premium shapes remain symmetric.
- Morphs keep final shape.
- Absolute Mirror remains opaque.
- Debug effects do not switch Classic/Premium.
- Experimental presets do not override mode unless explicitly designed.
- CrystalOff debug state is not reached through normal cycling.

---

## Stage 10 — Final Runtime Control Table

Produce final table:

Control | Action | Scope | Module | Notes

Required rows:
G
Backspace
Numpad 1
Numpad 3
Numpad 7
Numpad 9
Numpad Del / "."
F1..F12
Plus
Minus
Insert/Delete/Home/End/PageUp/PageDown

This table must also appear in runtime menu/help.

---

## Stage 11 — Validation

Automated validation:
- Unity compile passes.
- Existing smoke tests pass.
- Add tests for command routing where possible.

Manual validation:
1. G switches Classic/Premium.
2. Backspace toggles crystal only.
3. Numpad 1 selects Class 1.
4. Numpad 3 selects Class 2.
5. Numpad 7 selects Class 3.
6. Numpad 9 selects Class 4.
7. Numpad Del cycles selected class only.
8. F keys affect only local crystal effects.
9. Cursor cluster values remain 0..40.
10. Menu shows correct hotkeys.
11. Classic works.
12. Premium works.
13. Experimental effects work.
14. File browser still works.
15. Slideshow still works.
16. No invisible crystal state unless explicitly selected from diagnostics.

---

## Final Principle

KAELIS must behave like a visual instrument, not a chaotic debug panel.

Every control must have:
- one meaning;
- one owner;
- one visible menu entry;
- one command route;
- one documented scope.