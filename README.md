# Kaleidoscope2

Modular cinematic kaleidoscope engine for Unity with centralized orchestration, isolated runtime modules, diagnostics-first development, and a deterministic render pipeline target.

## Architecture

```text
UI / Input / Audio
        |
        v
KaleidoscopeDirector
        |
        v
Independent Modules
        |
        v
Final Render Output
```

## Current Stage

- STAGE 00 - Project Foundation
- STAGE 01 - Core Architecture
- STAGE 02 - Diagnostics First

Future visual systems must preserve the contracts in `AGENTS.md`, `ROADMAP.md`, and `Assets/_Project/Kaleidoscope2/Docs/MODULE_BOUNDARIES.md`.
