# ROADMAP.md — KAELIS Premium3D Recovery

## Main Goal

Build a stable, modular cinematic kaleidoscope engine with:

- protected Classic2D / Layer 1;
- real Premium3D / Layer 2;
- strong crystal size and shape;
- optical effects only after the crystal itself is accepted;
- premium UI later, not during core rendering fixes.

---

## Current Priority

The current priority is NOT caustics, prism effects, spotlight polish, or menu work.

Current priority:

```text
Premium3D Size & Shape Gate
```

The crystal must become:

- large enough;
- centered;
- beautiful;
- premium-looking;
- not a deformed small low-poly object.

Until this passes, no new visual effects are allowed.

---

# STAGE 00 — Safety Checkpoint

Goal: freeze current state before changes.

Tasks:

- run `git status`;
- commit or stash current work;
- push if needed;
- verify working tree state.

Acceptance:

There is a safe rollback point.

---

# STAGE 01 — Regression Cleanup

Goal:

Remove accidental visual regressions introduced by premature effects.

Remove/disable if present:

- background pulsing;
- expanding rings;
- unwanted radial waves;
- accidental global distortion;
- premature caustics/prism overlays;
- effects that were added before Size & Shape Gate passed.

Rules:

- do not modify MirrorModule;
- do not modify SourceModule;
- do not change Classic2D behavior;
- do not add new effects.

Acceptance:

Premium3D returns to a clean baseline without unwanted pulsing/rings/waves.

---

# STAGE 02 — Crystal Size Gate

Goal:

Make Premium3D crystal visibly large enough.

Target:

```text
42–48% of final visible Game View height
```

Preferred target:

```text
45%
```

Rules:

- measure final visible Game View coverage;
- do not rely only on internal RenderTexture coverage;
- do not scale only the background;
- do not hide size problems with effects;
- keep crystal centered;
- no tiny/corner/duplicate crystal.

Acceptance:

- final visible coverage is reported;
- crystal is centered;
- crystal is visibly large enough;
- Classic2D unchanged.

---

# STAGE 03 — Crystal Shape Gate

Goal:

Replace/refine the default Premium3D crystal shape.

Default required shape:

```text
Classic Brilliant / Premium Diamond
```

Shape requirements:

- clear crown;
- clear girdle;
- clear pavilion;
- readable table facet;
- symmetrical silhouette;
- strong facet structure;
- attractive gemstone-like form;
- no broken shard look;
- no weak random low-poly object.

Optional shape library preparation:

- Classic Brilliant
- Octagon
- Emerald Cut
- Marquise
- Pear / Drop
- Cushion

But only Classic Brilliant must be visually correct in this stage.

Acceptance:

- default shape is visually acceptable;
- mesh remains volumetric;
- side faces visible;
- vertex/triangle/bounds reported.

---

# STAGE 04 — Freeze Size & Shape Baseline

Goal:

Lock the first good Premium3D crystal baseline.

Tasks:

- Unity compile;
- Play Mode check;
- Classic2D ↔ Premium3D switch check;
- commit;
- push.

Acceptance:

There is a stable baseline where the Premium3D crystal is large and has an acceptable default shape.

No effects work starts before this stage is accepted.

---

# STAGE 05 — Crystal Spotlight Baseline

Goal:

Add controlled spotlight only after crystal size and shape are accepted.

Rules:

- crystal hotkeys active only when Backspace has enabled crystal visibility;
- when crystal is hidden, old project hotkeys keep their original behavior;
- spotlight must not globally steal keys;
- spotlight must not replace the background;
- no caustics/rainbow yet.

Acceptance:

- spotlight helps reveal facets;
- existing controls remain safe;
- background remains beautiful.

---

# STAGE 06 — Shadow / Light Receiver

Goal:

Make BackgroundGeometry react to crystal lighting.

Tasks:

- use background as receiver;
- add basic controlled light spot or shadow;
- keep selected/kaleidoscope background visible and attractive;
- avoid making the scene black.

Acceptance:

Light feels connected to crystal/background.

---

# STAGE 07 — Caustics Prototype

Goal:

Add a simple controlled caustics layer.

Rules:

- caustics must be crystal-driven;
- no fullscreen random noise;
- no pulsing/radial waves unless explicitly requested;
- effect must be optional/toggleable.

Acceptance:

Background receives beautiful light patterns that appear caused by the crystal.

---

# STAGE 08 — Prism / Rainbow Projection

Goal:

Add controlled spectral/prismatic effects.

Rules:

- prism/rainbow must attach visually to crystal, facets, or light direction;
- no acidic fullscreen rainbow;
- no background destruction;
- effect must be optional/toggleable.

Acceptance:

Crystal produces visible prism/rainbow beauty without overwhelming the scene.

---

# STAGE 09 — Visual Balance Pass

Goal:

Make Premium3D feel like a valuable mode, not a weak copy of Classic2D.

Check:

- crystal is still 42–48%;
- shape still reads as premium;
- background remains beautiful;
- effects support the crystal;
- no pulsing/ring regressions;
- no duplicates;
- no screen-inside-screen;
- Classic2D unchanged.

Acceptance:

Premium3D is visually pleasing and stable.

---

# STAGE 10 — Premium3D Shape Library Expansion

Goal:

Add polished shape variety after the default shape works.

Shapes:

- Classic Brilliant;
- Octagon;
- Emerald Cut;
- Marquise;
- Pear / Drop;
- Cushion.

Acceptance:

Each shape is volumetric, attractive, and switchable without breaking framing.

---

# STAGE 11 — Presets

Goal:

Create curated Premium3D looks.

Examples:

- Soft Jewel;
- Strong Prism;
- Dark Hall Spotlight;
- Rainbow Caustics;
- Clean Product Shot.

Acceptance:

Presets change Premium3D character without breaking size/shape.

---

# STAGE 12 — Premium Menu

Goal:

Build commercial-grade menu later.

Before menu work, inspect:

```text
Assets/_Project/Kaleidoscope2/Menu/
```

Rules:

- do not build the whole menu at once;
- implement one polished button/panel first;
- UI must not directly mutate rendering internals.

Acceptance:

Menu feels premium, not debug/prototype.

---

# Development Rules

Forbidden:

- changing Layer 1 during Premium3D work without explicit approval;
- calling a flat plane a 3D crystal;
- adding effects before Size & Shape Gate passes;
- using effects to hide bad crystal geometry;
- giant rewrites;
- unrelated refactoring;
- global hotkey stealing.

Required:

- small stages;
- small commits;
- Unity compile validation;
- report measured values;
- protect Classic2D;
- protect Billboard2D.

---

# Execution Rules For Codex

Before every task:

1. Read `AGENTS.md`.
2. Read `ROADMAP.md`.
3. Run `git status`.
4. Work only on the requested stage.
5. Do not touch unrelated modules.
6. Stop and report if a requested change requires forbidden files.
7. Do not proceed to effects unless Size & Shape Gate is accepted.

---

# Current Active Stage

```text
STAGE 01 → STAGE 03
Regression Cleanup + Crystal Size Gate + Crystal Shape Gate
```

Do not implement spotlight, caustics, prism, rainbow, presets, menu, or polish until the crystal is large and visually accepted.
