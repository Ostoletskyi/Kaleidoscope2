# AGENTS.md — KAELIS Menu Freedom Mode

## 0. Main Decision

The startup menu is now an active creative development zone.

Codex is allowed to redesign and rebuild the KAELIS startup menu until it matches the approved visual direction.

The approved direction is:

- luxury crystal interface;
- noble blue / cyan atmosphere;
- warm gold accents;
- gemstone buttons;
- cinematic optical depth;
- bright, premium, commercial look;
- real menu UI, not a debug overlay.

The current menu implementation is not sacred.

If the current runtime-built menu architecture prevents reaching the desired commercial visual result, Codex may replace or refactor it.

Do not keep weak menu code just because it already exists.

---

## 1. Protected Systems

The following systems are protected and must not be changed during menu work:

- Classic2D rendering;
- Premium3D crystal rendering;
- DiamondFocus;
- crystal shaders;
- Mirror/**;
- Source/**;
- RuntimeMenuController;
- OutputPreview;
- camera/render pipelines;
- gameplay/render logic outside startup menu.

Menu work must not damage the working kaleidoscope modes.

Classic2D and Premium3D rendering are separate from menu development.

---

## 2. What Codex May Freely Change

Codex may freely change, refactor, replace, or rebuild:

- startup menu UI;
- menu controller;
- menu view hierarchy;
- menu animation scripts;
- menu asset preparation scripts;
- generated menu sprites;
- generated TMP font assets;
- menu-only materials;
- menu-only VFX;
- menu-only layout logic;
- menu-only input handling;
- menu-only transition logic.

Codex may create new files under:

```text
Assets/_Project/Kaleidoscope2/Menu/
Assets/_Project/Kaleidoscope2/Menu/UI/
Assets/_Project/Kaleidoscope2/Menu/Runtime/
Assets/_Project/Kaleidoscope2/Menu/Editor/
Assets/_Project/Kaleidoscope2/Menu/VFX/
Assets/_Project/Kaleidoscope2/Menu/Generated/
```

Codex may remove obsolete menu-only code and menu-only generated assets if they are replaced by a better menu implementation.

---

## 3. Approved Visual Target

The selected reference image is the art direction source.

Codex must move the real Unity menu toward this visual target:

- bright noble blue/cyan atmosphere;
- polished crystal panels;
- luminous gemstone buttons;
- gold primary action;
- blue/cyan secondary buttons;
- ruby red exit button;
- soft optical haze;
- cinematic preview panel;
- luxury sci-fi typography;
- clean commercial composition.

Do not darken the menu into a dull technical interface.

Do not invent a new style unless the current menu cannot technically reproduce the reference.

If the current result differs from the reference, the reference wins.

---

## 4. Menu Quality Bar

The menu must feel like:

- commercial software;
- premium visual application;
- optical experience engine;
- luxury crystal dashboard;
- polished startup screen.

The menu must not feel like:

- Unity debug UI;
- raw prototype;
- programmer layout;
- accidental overlay;
- technical placeholder;
- low-contrast dark screen;
- cheap neon arcade UI.

---

## 5. Layout Direction

The preferred layout remains:

```text
Left panel      — KAELIS title, tagline, buttons
Right panel     — live preview / hero visual
Bottom bar      — system/demo/mode status
Background      — soft optical crystal atmosphere
```

Codex may adjust proportions, spacing, panel sizes, padding, and visual balance if it improves similarity to the approved reference.

Codex may redesign internal hierarchy of the startup menu if needed.

---

## 6. Typography Direction

Use real TextMeshPro text.

Do not rely on text baked into images.

Preferred roles:

- KAELIS title: Cinzel or best luxury serif available;
- tagline/subtitle: Cinzel Regular or refined serif with letter spacing;
- button labels: Cinzel SemiBold or best readable luxury style;
- secondary UI/status: Inter / Manrope / clean modern sans-serif;
- decorative accent: Cormorant Garamond only if readable and appropriate.

If a font does not work visually, Codex may choose a better font from the available menu fonts and explain why.

Readability has priority over strict font assignment.

---

## 7. Button Direction

Buttons are central to the menu identity.

They should look like carved gemstone UI capsules.

Required button family:

- Enter Experience: gold / amber / primary;
- Demo Mode: cyan / teal;
- Modes / Optics / Presets / Settings: blue/cyan gemstone;
- Exit: ruby / red.

Button states:

```text
Normal  — dark gemstone body, readable text, calm glow
Hover   — activation line/fill reaches the opposite edge consistently
Pressed — fully lit gemstone state
Release — brief hot yellow/gold flash, then action
Active  — stable selected state
Exit    — ruby destructive state
```

Important:

The activation line/fill must travel consistently to the opposite edge.
No partial broken line.
No different random fill lengths per button.
No cropped or broken highlights.
No baked labels inside button sprites.

Codex may replace the current button implementation if the existing sliced sprites or transition logic cannot produce the desired behavior.

---

## 8. Background Direction

The far background should be:

- blue/cyan;
- luminous;
- soft;
- cinematic;
- optical;
- atmospheric;
- supportive of menu readability.

It should not be:

- visible frozen kaleidoscope;
- muddy black;
- too dark;
- too noisy;
- too sharp;
- visually competing with preview and buttons.

Codex may replace the current menu background asset or create a new menu-only background if it improves the result.

---

## 9. Preview Panel Direction

The preview panel should feel like a premium hero display.

It should have:

- clear frame hierarchy;
- soft cyan/gold accents;
- rich visual content;
- enough brightness;
- polished depth;
- no placeholder feeling.

The preview image may remain a menu art/placeholder until live preview is explicitly requested, but it must look intentional.

---

## 10. Animation and Interaction

Codex may add menu-only animation:

- intro fade;
- staggered button reveal;
- hover glow;
- press compression;
- release flash;
- subtle shimmer;
- soft panel glow;
- slow background drift;
- non-invasive menu ambience.

Do not add aggressive animation.
Do not affect rendering modules.

Actions may be delayed briefly after release flash, but interaction must still feel responsive.

---

## 11. Demo Mode Preparation

The menu should visually support Demo Mode.

Demo Mode may remain a stored UI state unless a task explicitly asks to wire audio/images.

Future demo content lives under:

```text
Assets/_Project/Kaleidoscope2/DemoContent/
    Audio/
    Images/
    Presets/
```

Do not wire demo playback unless the task explicitly says so.

---

## 12. Asset Policy

Allowed:

- generate menu-only sprites;
- generate TMP font assets;
- create editor helper scripts for deterministic asset preparation;
- use Resources or serialized references if reliable;
- replace bad generated assets with better ones;
- delete obsolete menu-only generated assets.

Required:

- keep generated assets under Menu/**;
- do not reintroduce deleted old reference files;
- do not use stale crop coordinates;
- rescan current asset files when assets change;
- do not use baked text from concept images as UI text.

---

## 13. Validation

Every menu task must report:

- files changed;
- assets generated;
- protected systems untouched;
- smoke test / compile result;
- visual goal achieved or not;
- remaining issues.

Also report:

```text
git diff --name-only
```

Expected changes should normally be limited to:

```text
Assets/_Project/Kaleidoscope2/Menu/**
Assets/_Project/Kaleidoscope2/Menu/UI/**
Assets/_Project/Kaleidoscope2/Menu/Editor/**
```

---

## 14. Final Rule

Protect the kaleidoscope engine.

Free the menu.

Codex is allowed to be bold inside the menu subsystem until the startup screen looks like the approved premium KAELIS reference.
