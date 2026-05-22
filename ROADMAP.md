# ROADMAP.md — KAELIS Menu Commercial Redesign Roadmap

## Strategy

The current priority is the startup menu.

The previous cautious menu implementation did not reach the desired visual quality. Therefore, menu development is now allowed to be more ambitious.

The rule:

```text
Classic2D and Premium3D rendering are protected.
Startup Menu is free to redesign.
```

Codex may refactor or rebuild the menu subsystem to reach the approved commercial reference.

---

# PHASE 0 — Menu Safety Checkpoint

Goal:
Create a safe rollback point before aggressive menu work.

Tasks:

- run git status;
- commit current state if not committed;
- confirm current branch;
- verify Classic2D and Premium3D still run;
- confirm current menu assets exist.

Acceptance:

- safe rollback point exists;
- protected systems are known-good.

---

# PHASE 1 — Current Menu Audit

Goal:
Understand why the current menu does not match the reference.

Tasks:

- inspect current menu hierarchy;
- inspect KaelisStartupMenuController;
- inspect generated button assets;
- inspect font assets;
- inspect current menu background;
- inspect hover/pressed/activation transitions;
- compare current result against approved reference.

Report:

- what is too dark;
- what is too dull;
- what is too small;
- what differs from reference;
- what current code prevents.

Acceptance:

- clear list of visual blockers;
- clear list of code/assets to replace or keep.

---

# PHASE 2 — Menu Architecture Freedom Pass

Goal:
Decide whether current runtime-built architecture is still useful.

Allowed actions:

- keep current architecture if sufficient;
- split controller into view/style/animation classes;
- create a proper prefab-like runtime hierarchy;
- create new menu-only scripts;
- replace weak code;
- remove obsolete menu-only code.

Suggested structure:

```text
Menu/Runtime/
    KaelisStartupMenuController.cs
    KaelisStartupMenuView.cs
    KaelisMenuStyle.cs
    KaelisMenuButton.cs
    KaelisMenuAnimator.cs
    KaelisMenuAssets.cs

Menu/Editor/
    KaelisMenuAssetPreparation.cs
```

Acceptance:

- menu code becomes easier to polish;
- current visual limitations are reduced;
- protected systems untouched.

---

# PHASE 3 — Reference-Locked Visual Reconstruction

Goal:
Make the real menu match the selected reference.

Tasks:

- rebuild background treatment;
- improve left panel;
- improve right preview panel;
- improve bottom status bar;
- improve frame/corner accents;
- restore bright blue/cyan/gold luxury palette;
- remove dull/underlit look.

Target:

- brighter;
- cleaner;
- more luminous;
- more premium;
- more blue/cyan;
- less muddy;
- closer to approved reference.

Acceptance:

- visual comparison clearly moves toward reference;
- no new unrelated style invented.

---

# PHASE 4 — Button System Rebuild

Goal:
Fix gemstone buttons completely.

Tasks:

- remove current broken/weak button state behavior if needed;
- rebuild button visuals as layered UI elements;
- ensure activation line/fill reaches the opposite edge consistently;
- ensure hover is consistent on every button;
- ensure pressed state is full and readable;
- ensure release flash is brief and premium;
- ensure text remains TMP, not baked into image;
- ensure gem corners do not stretch.

Implementation options:

- sliced sprites;
- left/middle/right fragments;
- overlay masks;
- shader-like UI material only if menu-only;
- procedural fill line independent from sprite texture.

Acceptance:

- no broken partial activity line;
- all buttons respond consistently;
- Enter is dominant;
- Exit is ruby;
- secondary buttons are blue/cyan;
- button text is readable.

---

# PHASE 5 — Typography Pass

Goal:
Make text feel premium and readable.

Tasks:

- choose final font roles from available fonts;
- generate TMP font assets if needed;
- tune title size/tracking;
- tune button label size/tracking;
- tune status/micro text;
- fix hierarchy:
  - KAELIS title;
  - tagline;
  - primary CTA;
  - secondary buttons;
  - preview labels;
  - status bar.

Acceptance:

- KAELIS title feels premium;
- labels are readable;
- text no longer looks like placeholder;
- typography matches reference.

---

# PHASE 6 — Background and Preview Polish

Goal:
Make the menu feel commercial at first glance.

Tasks:

- replace dark/frozen background with soft optical atmosphere;
- improve preview panel content and brightness;
- add subtle prism/bokeh/caustic ambience;
- keep UI readable;
- avoid noisy kaleidoscope background.

Acceptance:

- far background supports menu;
- preview feels like a hero showcase;
- overall menu is bright and rich.

---

# PHASE 7 — Interaction and Animation

Goal:
Make menu feel alive.

Tasks:

- intro reveal;
- staggered button appearance;
- hover shimmer;
- press compression;
- release flash;
- soft glow transitions;
- optional slow background drift;
- optional subtle preview shimmer.

Acceptance:

- interactions feel premium;
- no aggressive animation;
- actions remain responsive.

---

# PHASE 8 — Demo Mode UI Preparation

Goal:
Make Demo Mode visually ready.

Tasks:

- improve Demo Mode toggle;
- show ON/OFF clearly;
- prepare UI text for demo status;
- do not wire audio/images yet unless requested.

Acceptance:

- Demo Mode control looks integrated;
- future wiring is easy.

---

# PHASE 9 — Asset Cleanup

Goal:
Remove old menu clutter.

Tasks:

- delete obsolete menu-only generated assets if replaced;
- remove stale reference files if not needed;
- keep final reference images in a clear References folder;
- keep generated assets in Generated/Resources folders;
- ensure no old deleted assets are reintroduced.

Acceptance:

- menu folders are clean;
- no confusing stale files;
- deterministic asset preparation exists if needed.

---

# PHASE 10 — Commercial Acceptance Pass

Goal:
Judge the menu like a product.

Checklist:

- Does it match approved reference?
- Is it bright enough?
- Is it blue/cyan/gold enough?
- Is KAELIS title strong?
- Is Enter obvious?
- Are buttons beautiful and consistent?
- Is the preview panel rich?
- Is the background supportive?
- Is text readable?
- Does it avoid debug look?
- Are protected systems untouched?

Acceptance:

- menu is presentation-ready;
- user confirms visual quality;
- then commit.

---

## Forbidden Throughout

Do not modify:

- Classic2D;
- Premium3D rendering;
- DiamondFocus;
- crystal shaders;
- Mirror/**;
- Source/**;
- RuntimeMenuController;
- OutputPreview;
- cameras/render pipeline.

---

## Allowed Throughout

Inside Menu/**, Codex may:

- redesign;
- refactor;
- replace;
- delete obsolete menu code;
- generate assets;
- create editor helpers;
- create new menu scripts;
- create better menu materials;
- change layout;
- change animation;
- change button state system.

---

## Current Active Task

The next task should be:

```text
Menu Phase 1–4:
Audit the current menu against the approved reference, then rebuild the menu visual/button system as needed so it actually matches the selected bright blue-gold crystal reference.
```

Do not continue tiny superficial tweaks if the current implementation cannot reach the reference.
