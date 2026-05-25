# AGENTS.md — KAELIS / Kaleidoscope2 Control Architecture Cleanup

## Mission

You are working on KAELIS / Kaleidoscope2.

Your task is not to add random effects.
Your task is to audit, detect, and fix architectural inconsistencies, broken control logic, duplicated commands, fake UI bindings, accidental mode switches, and unclear effect ownership.

The current problem:
Effects, debug modes, premium modes, classic modes, presets, and input bindings have become chaotic.
Some buttons enable/disable unrelated effects.
Some keys switch modes in ways not intended by the control logic.
Some UI rows may claim one action but dispatch another.
Some systems overlap and mutate the same state directly.

Your job:
Bring order.

You have full permission to refactor, rename, move, delete obsolete wiring, and reorganize systems if needed.

But preserve useful visual behavior whenever possible.

---

## Hard Rules

### 1. Classic/Premium mode switching

Only the `G` key may switch between Classic and Premium crystal mode.

No other key may switch Classic <-> Premium.

Forbidden:
- Numpad 1 switching Classic/Premium
- Numpad 3 switching Classic/Premium
- Numpad 7 switching Classic/Premium
- Numpad 9 switching Classic/Premium
- F-keys switching Classic/Premium
- Debug effects secretly switching Classic/Premium

---

### 2. Function keys

Function keys are reserved for local crystal effects only.

Scope:
- Classic crystal effects
- Premium crystal effects

Function keys must not:
- switch Classic/Premium mode;
- change visual source mode;
- open/close file browser;
- change unrelated global systems;
- trigger hidden mode jumps.

They may only affect local crystal-related effects.

---

### 3. Keys above cursor

Keys above the cursor cluster must operate within value ranges from 0 to 40.

Their logic must be explicit, documented, and reflected in the menu.

Examples:
- increase/decrease intensity;
- adjust effect index;
- adjust strength;
- adjust speed;
- adjust density.

But every such action must clamp to:
0..40

No hidden values outside this range unless explicitly documented and justified.

---

### 4. Numpad module selection

Numpad keys 1, 3, 7, 9 do NOT switch Classic/Premium mode.

They select crystal capability classes.

Required mapping:

Numpad 1
→ Select Class 1

Numpad 3
→ Select Class 2

Numpad 7
→ Select Class 3

Numpad 9
→ Select Class 4

The selected class becomes the active control context.

Numpad Del / "." cycles subclasses inside the selected class.

Example:

Class 1 selected:
Numpad Del cycles subclasses of Class 1.

Class 2 selected:
Numpad Del cycles subclasses of Class 2.

Class 3 selected:
Numpad Del cycles subclasses of Class 3.

Class 4 selected:
Numpad Del cycles subclasses of Class 4.

Numpad Del must never randomly cycle a different module.

---

### 5. Suggested class ownership

Use this mapping unless project audit discovers a better one:

Class 1:
Premium Crystal Shapes

Class 2:
Premium Optical Modes

Class 3:
Crystal Debug Modes

Class 4:
Crystal Debug Effects / Experimental Crystal Effects

If this mapping is changed, document the reason.

---

### 6. Menu visibility

Every hotkey binding must be visible in the menu.

Every control must have:
- current value;
- hotkey hint;
- short tooltip;
- scope: Classic, Premium, or Both.

No invisible “secret” runtime behavior.

---

### 7. Block architecture

Strict block architecture is mandatory.

InputModule:
- reads keys only;
- emits commands only;
- never directly changes shader/material/mesh state.

KaleidoscopeCommand:
- describes intent only.

KaleidoscopeDirector:
- routes commands to responsible modules.

Each feature must have an owning module.

RuntimeMenuController:
- displays state;
- dispatches commands;
- never directly mutates shader/material internals.

Shaders:
- render only;
- do not own gameplay/control logic.

Forbidden:
- UI directly changes shader floats;
- Input directly changes material;
- Debug Mode secretly changes Premium mode;
- Preset directly bypasses Director;
- multiple systems writing the same state without ownership.

---

## Audit Requirements

Before changing code, audit:

1. All keyboard input bindings.
2. All menu buttons and their dispatched commands.
3. All commands in KaleidoscopeCommand.
4. All Director routing.
5. All state fields related to:
   - Classic/Premium mode
   - Premium shapes
   - Premium optical modes
   - Debug modes
   - Debug effects
   - Experimental presets
   - Backspace crystal toggle
6. All shader/material mutation paths.
7. All duplicated or conflicting control paths.

Produce a short internal report:
- Found conflict
- File/method
- What it currently does
- What it should do
- Fix applied

---

## Control Map Target

Final control logic must be deterministic:

G:
Switch Classic/Premium only.

Backspace:
Enable/disable crystal only.

Numpad 1:
Select capability Class 1.

Numpad 3:
Select capability Class 2.

Numpad 7:
Select capability Class 3.

Numpad 9:
Select capability Class 4.

Numpad Del / ".":
Cycle subclass inside currently selected capability class.

Function keys:
Local crystal effects only.

Cursor-cluster keys:
Adjust assigned values in range 0..40 only.

Plus/Minus:
May cycle crystal shape only if already assigned and documented.
Must not conflict with Numpad class logic.

---

## Output Requirements

Every Codex response must include:

1. What was audited.
2. What inconsistencies were found.
3. What was changed.
4. Why the change was needed.
5. Files changed.
6. Validation performed.
7. Remaining risks.
8. Final runtime control table.

---

## Validation

Required validation:

- G switches Classic/Premium.
- No other key switches Classic/Premium.
- Backspace toggles crystal only.
- Numpad 1/3/7/9 select classes only.
- Numpad Del cycles only selected class.
- Function keys affect only local crystal effects.
- Cursor-cluster values clamp to 0..40.
- Menu shows all bindings.
- Tooltips exist.
- Classic mode works.
- Premium mode works.
- Absolute Mirror remains opaque.
- Experimental effects preserved.
- File browser untouched.
- Slideshow untouched.
- No compile errors.