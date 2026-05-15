# AGENTS.md — Kaleidoscope2 / KAELIS

## 1. Project Goal

Kaleidoscope2 / KAELIS is a modular Unity project for building a cinematic, optical, music-reactive kaleidoscope engine.

The project must be developed as a block-based architecture where every system is isolated, testable, replaceable, and controlled through a centralized director.

This is not a collection of random MonoBehaviour scripts.  
This is a structured visual engine.

---

## 2. Core Architecture Rule

All systems must follow this flow:

```text
UI / Input / Audio
        ↓
KaleidoscopeDirector
        ↓
Independent Modules
        ↓
Final Render Output
```

No module may directly control another module’s internal implementation.

Allowed:

- UI sends commands to Director.
- Input sends commands to Director.
- Audio sends events to Director.
- Director routes commands to modules.
- Modules expose public methods through clear contracts.
- Modules report status/events back through defined interfaces or event bus.

Forbidden:

- UI directly changes shader parameters.
- UI directly changes cameras.
- Audio module directly changes MirrorModule internals.
- Recording module searches for Camera.main.
- Tunnel module rewrites classic kaleidoscope pipeline.
- Any module uses FindObjectOfType in Update.
- Any runtime system depends on scene object names as production logic.

---

## 3. Layer Separation Rule

Kaleidoscope2 has two different visual layers.

```text
Layer 1 — Kaleidoscope Display Layer
Layer 2 — Crystal3D Stage Layer
```

These layers must not be confused.

---

### 3.1 Layer 1 — Kaleidoscope Display Layer

Purpose:

Render and display the classic kaleidoscope image.

Pipeline:

```text
SourceModule
        ↓
MirrorModule
        ↓
FinalOutputTexture
        ↓
OutputPreview / Display Surface
```

Layer 1 is the stable base layer.

For RealMesh3D / Premium Crystal tasks, Layer 1 is read-only.

Forbidden during RealMesh3D tasks:

- modifying MirrorModule;
- modifying SourceModule;
- modifying OutputPreview behavior;
- modifying the classic display pipeline;
- using Layer 1 display objects as crystal geometry;
- turning the display plane into the crystal;
- treating FinalOutput / RawImage / Display Plane as RealMesh3D.

Layer 1 may remain visible as the background behind the crystal, but it must not become the crystal.

---

### 3.2 Layer 2 — Crystal3D Stage Layer

Purpose:

Render a true premium 3D crystal as a separate volumetric object.

Correct visual composition:

```text
Viewer / Stage Camera
        ↓
RealMesh3D Crystal
        ↓
Kaleidoscope background behind crystal
```

Layer 2 must contain:

- real volumetric faceted MeshFilter + MeshRenderer crystal;
- dedicated CrystalStage3D root;
- dedicated CrystalStage3D camera if needed;
- dedicated CrystalLightRig;
- explicit positioning and scaling;
- clean composite over or with Layer 1;
- one active crystal renderer at a time.

Forbidden for Layer 2:

- Plane as crystal;
- Quad as crystal;
- Billboard as crystal;
- RawImage as crystal;
- Sprite as crystal;
- screen-space-only crystal;
- single flat surface mesh;
- using FinalKaleidoscopeTexture as direct albedo painted over the whole crystal;
- large projection panel pretending to be Premium 3D;
- camera-facing geometry tricks pretending to be volume.

If a flat projection plane is visible as the main crystal in RealMesh3D mode, the implementation is failed.

---

## 4. Crystal Rendering Rule

The project supports two different crystal rendering strategies:

```text
Billboard2D / Performance Crystal
RealMesh3D / Premium Crystal
```

Both strategies may receive the same final kaleidoscope texture, but they must render it differently and remain isolated from each other.

---

### 4.1 Billboard2D / Performance Crystal

Billboard2D is a fast fake crystal simulation for weak machines.

Allowed:

- Plane;
- Quad;
- Billboard;
- screen-space shader;
- fake facets;
- UV distortion;
- fake reflection/refraction;
- low-cost performance mode.

This mode is valid only as a performance-oriented 2D simulation.

If the implementation is flat, it belongs to Billboard2D.

Billboard2D must remain available, but RealMesh3D tasks must not modify Billboard2D unless the user explicitly approves it.

---

### 4.2 RealMesh3D / Premium Crystal

RealMesh3D is a true 3D faceted crystal.

Hard requirements:

- Must use MeshFilter + MeshRenderer.
- Must have real volumetric geometry.
- Must have thickness.
- Must have visible side faces.
- Must be visible as a 3D object from side view in Scene View.
- Must rotate as a real object in 3D.
- Must receive light from CrystalLightRig or equivalent explicit light rig.
- Must support transparent optical/crystal material.
- Must be separate from the kaleidoscope background display plane.
- Must exist as a real object even with all optical shaders disabled.

Forbidden for RealMesh3D:

- Plane;
- Quad;
- Sprite;
- RawImage;
- Billboard;
- single flat surface mesh;
- UI-only fake crystal;
- screen-space-only implementation;
- camera-facing illusion;
- projection panel as crystal.

If it is flat, it is not RealMesh3D.  
If it looks like a sheet from the side, the task is failed.

---

### 4.3 RealMesh3D Validation Rule

RealMesh3D is considered valid only if:

1. The object visibly has depth in Scene View side angle.
2. The object silhouette changes while orbiting around it.
3. Front and back faces are spatially separated.
4. Side faces are visible.
5. Mesh bounds depth is non-zero and meaningful.
6. The object casts/receives light as a volumetric object.
7. The object remains visibly volumetric without any shader effects enabled.
8. Disabling the crystal optical material still leaves a real faceted 3D object.

If disabling the shader leaves only a flat plane, the implementation is invalid.

Validation by a static frontal camera is not sufficient.

---

### 4.4 Crystal Renderer Strategy Rule

Crystal rendering must be separated through a strategy-like architecture:

```text
FinalKaleidoscopeTexture
        ↓
DiamondFocusModule / CrystalPresentationModule
        ↓
ICrystalRenderer
        ├── BillboardCrystalRenderer
        └── RealMeshCrystalRenderer
```

Billboard2D and RealMesh3D must not directly depend on each other.

Allowed:

- DiamondFocusModule selects active crystal renderer.
- CrystalPresentationModule routes shared settings to the active renderer.
- Both renderers can consume the same FinalKaleidoscopeTexture.
- Inactive renderer must be disabled cleanly.

Forbidden:

- Mixing Billboard2D and RealMesh3D logic in one uncontrolled class.
- Calling Billboard2D internals from RealMesh3D.
- Calling RealMesh3D internals from Billboard2D.
- Duplicating MirrorModule or SourceModule for crystal rendering.
- Replacing the classic kaleidoscope pipeline with crystal-specific logic.

---

## 5. Premium Menu / Commercial UI Rule

The KAELIS menu is not a temporary debug UI.

It must be treated as a premium commercial product interface.

The menu must feel like:

- a cinematic optical device;
- a high-end visual engine;
- a polished commercial application;
- not a Unity debug prototype;
- not a scattered set of buttons;
- not a raw developer panel.

---

### 5.1 Menu Module Location

Codex/AI agents must inspect this folder before implementing or changing any menu-related feature:

```text
C:\Projects\CLIPS\Kaleidoskop2\Assets\_Project\Kaleidoscope2\Menu
```

If the folder exists, inspect:

- all scripts;
- prefabs;
- scenes;
- UI assets;
- sprites;
- materials;
- animations;
- documentation;
- existing hierarchy;
- current menu flow.

If the folder does not exist, report that the Menu module is missing and propose the required folder structure before creating anything.

---

### 5.2 Required Menu Architecture

The menu must be implemented as a separate module:

```text
Assets/_Project/Kaleidoscope2/Menu/
│
├── Core/
│   ├── MenuDirector.cs
│   ├── MenuState.cs
│   ├── MenuCommand.cs
│   └── IMenuPanel.cs
│
├── Input/
│   ├── MenuInputRouter.cs
│   └── MenuNavigationController.cs
│
├── Panels/
│   ├── MainMenuPanel.cs
│   ├── ModesPanel.cs
│   ├── OpticsPanel.cs
│   ├── PresetsPanel.cs
│   ├── RecordExportPanel.cs
│   ├── SettingsPanel.cs
│   ├── DiagnosticsPanel.cs
│   └── AboutPanel.cs
│
├── Loading/
│   ├── LoadingTransitionController.cs
│   └── AsyncSceneLoader.cs
│
├── UI/
│   ├── MenuButtonView.cs
│   ├── MenuButtonAnimator.cs
│   ├── MenuToggleView.cs
│   ├── MenuSliderView.cs
│   └── MenuDropdownView.cs
│
├── Theme/
│   ├── MenuThemeSettings.cs
│   ├── MenuTypographySettings.cs
│   └── MenuAudioFeedback.cs
│
├── Assets/
│   ├── Backgrounds/
│   ├── Logos/
│   ├── Buttons/
│   ├── Icons/
│   ├── VFX/
│   └── Fonts/
│
└── Docs/
    └── MENU_ARCHITECTURE.md
```

The exact structure may be adjusted if the existing project already has a better organized Menu module, but responsibilities must remain separated.

---

### 5.3 Menu Responsibility

The Menu module is responsible for:

- commercial start screen;
- main menu navigation;
- premium visual presentation;
- button states;
- keyboard navigation;
- mouse interaction;
- touch interaction;
- async loading;
- transitions;
- menu sound feedback;
- UI scale handling;
- connection to KaleidoscopeDirector through commands only.

The Menu module must not directly modify:

- MirrorModule internals;
- SourceModule internals;
- DiamondFocus internals;
- RealMesh3D internals;
- shader parameters;
- camera internals;
- recording internals;
- physics objects.

Menu actions must be routed through:

```text
Menu UI
        ↓
MenuDirector / MenuInputRouter
        ↓
KaleidoscopeDirector
        ↓
Target Module
```

---

### 5.4 Premium UI Quality Bar

The menu must not be accepted as complete if it looks like a prototype.

Minimum quality requirements:

- clean layout;
- no overlapping elements;
- no blurry text;
- no broken typography;
- no inconsistent button sizes;
- no stretched sprites;
- no baked text inside background images;
- no misspelled title text;
- no random placeholder labels in final-looking UI;
- no debug HUD mixed into commercial menu;
- no unreadable low-contrast labels.

The UI must use:

- real TextMeshPro text;
- clear hierarchy;
- consistent spacing;
- consistent alignment;
- premium dark/glass/optical styling;
- separated background, logo, button, icon, and text layers;
- proper Canvas scaling;
- proper layout groups where appropriate;
- 9-sliced button frames where stretchable UI is used.

---

### 5.5 Required Button States

Every production menu button must support:

- Normal;
- Hover;
- Pressed;
- Selected;
- Disabled.

Each state should provide visual feedback:

- glow;
- scale change;
- opacity change;
- highlight frame;
- sound feedback if available.

Touch input must behave consistently with mouse click.

Keyboard/gamepad selection must behave consistently with hover/selected state.

---

### 5.6 Required Main Menu Hierarchy

The menu must support or document these top-level entries:

```text
ENTER EXPERIENCE
MODES
OPTICS
PRESETS
RECORD & EXPORT
SETTINGS
DIAGNOSTICS
ABOUT
EXIT
```

A menu item may be:

- ACTIVE;
- PARTIAL;
- PLACEHOLDER;
- FUTURE.

Codex must not pretend that a placeholder is an active feature.

---

### 5.7 Required Menu Audit

Before implementing premium menu changes, Codex must create or update:

```text
Assets/_Project/Kaleidoscope2/Menu/Docs/MENU_AUDIT.md
```

The audit must include:

1. Existing files in Menu folder.
2. Existing scenes/prefabs/assets.
3. Existing scripts and their responsibilities.
4. Existing menu flow.
5. Current visual quality issues.
6. Missing architecture pieces.
7. Missing asset pieces.
8. Missing interaction states.
9. Missing loading/transition logic.
10. Missing keyboard/mouse/touch support.
11. What is ACTIVE.
12. What is PARTIAL.
13. What is PLACEHOLDER.
14. What is FUTURE.
15. Recommended next implementation order.

This audit must be done before large menu implementation changes.

---

### 5.8 Menu Implementation Rule

Do not build the entire menu at once.

Use vertical slices.

Recommended order:

1. Audit existing Menu folder.
2. Document missing pieces.
3. Create MenuDirector + MenuState + MenuCommand if missing.
4. Create MainMenu scene/panel skeleton.
5. Implement one perfect production-quality button.
6. Add hover/click/keyboard/touch behavior to that one button.
7. Add async loading placeholder.
8. Duplicate the proven button pattern to other entries.
9. Add panels one by one.
10. Add polish, animation, sound, and transitions.

If one button is not polished, do not scale the system to ten buttons.

---

### 5.9 Text and Branding Rules

The application name must be written exactly as:

```text
KAELIS
```

Optional subtitle:

```text
BEYOND THE REFLECTION
```

Studio/copyright text must be written exactly as:

```text
© 2026 Lex Nox Lab. All rights reserved.
```

Do not generate corrupted text, pseudo-text, fake words, random glyphs, or misspelled branding in production UI.

Generated background art should not contain baked menu text unless explicitly requested.

Prefer live TextMeshPro text in Unity for all UI labels.

---

### 5.10 Menu Acceptance Criteria

A menu task is not complete until:

- Menu folder has been inspected.
- Missing pieces have been listed.
- Menu architecture is documented.
- UI does not overlap.
- Text is sharp and readable.
- Background art is separated from live UI text.
- Buttons have interaction states.
- Mouse, keyboard, and touch behavior are considered.
- Placeholders are visibly marked as placeholders.
- Active features are not mislabeled.
- No menu code directly mutates rendering modules.
- Unity compiles.
- Play Mode smoke test is reported.

---

## 6. Required Top-Level Modules

The project must be organized around these modules.

---

### Menu

Responsible for:

- commercial start screen;
- main navigation;
- transitions;
- loading flow;
- panel hierarchy;
- premium UI presentation;
- keyboard/mouse/touch navigation;
- menu animation;
- menu sounds;
- UI theme system.

Menu must never directly modify:

- MirrorModule internals;
- shader parameters;
- camera internals;
- DiamondFocus internals;
- Recording internals.

Menu communicates only through:

- MenuDirector;
- KaleidoscopeDirector;
- command routing.

---

### Core

Responsible for:

- KaleidoscopeDirector;
- KaleidoscopeState;
- module registration;
- command routing;
- lifecycle management;
- global validation;
- event bus.

Main files:

- `KaleidoscopeDirector.cs`;
- `KaleidoscopeState.cs`;
- `KaleidoscopeBootstrap.cs`;
- `IKaleidoscopeModule.cs`;
- `KaleidoscopeCommand.cs`.

---

### Control

Responsible for:

- runtime UI;
- editor control panel;
- sliders/buttons/toggles;
- preset selection;
- recording buttons;
- debug controls.

Control must never directly modify shader, camera, physics, recording, tunnel, or crystal internals.

Control talks only to:

- `KaleidoscopeDirector`;
- command API.

---

### Input

Responsible for:

- keyboard;
- mouse;
- gamepad;
- touch input;
- hotkeys;
- WASD center movement;
- mouse wheel zoom;
- Space shake;
- arrows rotation/twist.

Input must convert physical user input into commands.

Input must not directly move cameras, physics objects, shaders, tunnel objects, or crystal objects.

---

### Source

Responsible for producing the source texture.

Supported source modes:

- physics chamber camera;
- image texture;
- video texture;
- procedural texture;
- external RenderTexture.

The Source module exposes:

- current source texture;
- source mode;
- source status.

It must not know how mirror rendering, recording, tunnel, crystal, or audio systems work.

---

### PhysicsChamber

Responsible for:

- gems;
- particles;
- Rigidbody simulation;
- collision behavior;
- rotating chamber;
- shake/avalanche;
- material presets;
- physical visual entropy.

PhysicsChamber produces visual content only.

It must not directly control:

- MirrorModule;
- TunnelModule;
- RecordingModule;
- DiamondFocusModule;
- UI.

---

### Mirror

Responsible for the classic kaleidoscope effect.

Responsibilities:

- radial symmetry;
- mirror count;
- segment angle;
- rotation;
- center offset;
- zoom;
- seam blending;
- chromatic aberration;
- vignette;
- shader parameter application.

MirrorModule receives source texture and outputs kaleidoscope RenderTexture.

It must not know whether the output is displayed, recorded, projected into tunnel mode, or used by crystal rendering.

---

### Camera

Responsible for:

- source camera;
- viewer camera;
- render camera;
- offline render camera;
- camera rig setup;
- RenderTexture assignment;
- safe camera activation/deactivation.

Do not use `Camera.main` for production logic.

Camera references must be explicit, serialized, injected, or registered through Bootstrap/Director.

---

### AudioReactive

Responsible for:

- audio analysis;
- beat detection;
- kick/snare/drop/build/break/silence events;
- mapping audio events to visual commands.

AudioReactive must emit events or commands.

It must not directly edit shader values, camera transforms, physics objects, or crystal objects unless routed through Director.

---

### Tunnel

Responsible for experimental 3D tunnel mode.

Responsibilities:

- tunnel mesh;
- tunnel projection shader;
- final kaleidoscope texture projection;
- end cap;
- seam feathering;
- depth shading;
- tunnel camera if needed.

Tunnel mode must use the final kaleidoscope texture as input.

Tunnel mode must not break or rewrite the classic kaleidoscope pipeline.

Classic mode must remain fully functional when TunnelModule is disabled.

---

### DiamondFocus / CrystalPresentation

Responsible for crystal rendering and crystal optical presentation.

Responsibilities:

- crystal visibility;
- crystal simulation mode selection;
- Billboard2D performance renderer;
- RealMesh3D premium renderer;
- CrystalStage3D layer;
- crystal material/profile binding;
- crystal shape switching;
- crystal rotation;
- crystal optics profile;
- crystal diagnostics;
- optional crystal light rig.

DiamondFocus must remain downstream of the kaleidoscope output.

DiamondFocus must not rewrite MirrorModule, SourceModule, TunnelModule, or RecordingModule.

RealMesh3D must be treated as Layer 2, not as a modification of Layer 1.

---

### Recording

Responsible for:

- preview recording;
- offline frame export;
- PNG sequence export;
- ffmpeg assembly;
- audio sync;
- fixed FPS rendering;
- final video output.

Recording must consume final RenderTexture only.

Recording must not decide how the visual scene works.

Recording must not rely on `Camera.main`.

---

### Presets

Responsible for:

- saving/loading preset data;
- global visual profiles;
- mirror settings;
- color settings;
- camera settings;
- audio-reactive intensity;
- tunnel settings;
- crystal settings;
- recording profiles.

Presets are data, not control logic.

Preset application must go through Director.

---

### Diagnostics

Responsible for:

- debug HUD;
- FPS;
- memory usage;
- RenderTexture status;
- active modules;
- current mode;
- crystal simulation mode;
- Layer 1 / Layer 2 state;
- warnings;
- validation reports;
- missing references.

Diagnostics may read state and module status.

Diagnostics must not mutate production state except through explicit debug commands.

---

## 7. Required Folder Structure

Use this structure unless there is a strong reason not to:

```text
Assets/_Project/Kaleidoscope2/
│
├── Menu/
├── Core/
├── Control/
├── Input/
├── Source/
├── PhysicsChamber/
├── Mirror/
├── Camera/
├── AudioReactive/
├── Tunnel/
├── DiamondFocus/
│   ├── Billboard/
│   ├── RealMesh/
│   │   └── CrystalStage3D/
│   ├── Lighting/
│   ├── Shaders/
│   └── Docs/
├── Recording/
├── Presets/
├── Diagnostics/
├── Shaders/
├── Materials/
├── Textures/
├── Scenes/
└── Docs/
```

Do not place unrelated systems in the same folder.

Do not create one giant Scripts folder.

---

## 8. Dependency Direction

Dependencies must flow downward:

```text
Control/Input/Audio/Menu
        ↓
Core/Director
        ↓
Modules
        ↓
Rendering/Output
```

Allowed dependency examples:

- Control → Core;
- Menu → Core;
- Input → Core;
- AudioReactive → Core;
- Core → Module interfaces;
- Module → its own internal helpers;
- Recording → final output interface;
- DiamondFocus → crystal renderer interfaces;
- CrystalPresentation → active crystal renderer.

Forbidden dependency examples:

- Mirror → Recording;
- Recording → Mirror internals;
- Tunnel → PhysicsChamber internals;
- AudioReactive → Mirror shader directly;
- UI → Camera directly;
- UI → Crystal material directly;
- PhysicsChamber → UI;
- Source → Recording;
- BillboardCrystalRenderer → RealMeshCrystalRenderer;
- RealMeshCrystalRenderer → BillboardCrystalRenderer.

---

## 9. Communication Rules

All cross-module communication must use one of these:

1. KaleidoscopeDirector;
2. command objects;
3. event bus;
4. explicit interfaces;
5. serialized references assigned by Bootstrap.

Avoid hidden communication.

Forbidden:

- global static state for production logic;
- FindObjectOfType in runtime loops;
- GameObject.Find for core systems;
- magic scene names;
- hidden dependencies through singleton abuse.

Singletons are allowed only for Bootstrap-level access and must not become dumping grounds.

---

## 10. State Management

There must be one authoritative state object:

```text
KaleidoscopeState
```

It stores:

- active source mode;
- active visual mode;
- mirror settings;
- camera settings;
- tunnel enabled/disabled;
- crystal simulation mode;
- Layer 1 / Layer 2 status;
- recording state;
- active preset;
- runtime quality level;
- current warnings/errors.

Modules may keep internal cache, but user-facing state must be synchronized with KaleidoscopeState.

UI must read from State, not guess values.

---

## 11. Rendering Pipeline

The required rendering flow:

```text
Layer 1:
Source Content
    ↓
Source RenderTexture
    ↓
MirrorModule
    ↓
Kaleidoscope RenderTexture
    ↓
Final Output RenderTexture
    ↓
OutputPreview / Display

Layer 2:
CrystalStage3D
    ↓
RealMesh3D Crystal
    ↓
Crystal Camera / LightRig
    ↓
Crystal Composite
    ↓
Final Visual Composition
```

Layer 1 is the stable base display layer.

Layer 2 is the premium 3D crystal stage.

Tunnel mode must be downstream from final kaleidoscope output.

Recording must record final output, not random scene cameras.

RealMesh3D must not be implemented by turning Layer 1 into a crystal.

---

## 12. Mode Rules

The project must support multiple modes without breaking the core pipeline:

- Classic Kaleidoscope Mode;
- Physics Chamber Source Mode;
- Image Source Mode;
- Procedural Source Mode;
- Audio Reactive Mode;
- Experimental Tunnel Mode;
- Billboard2D Crystal Mode;
- RealMesh3D Crystal Mode;
- Offline Recording Mode.

Modes must be composable where possible.

Examples:

- Classic + Crystal Billboard2D;
- Classic + Crystal RealMesh3D;
- Classic + Audio Reactive + Recording;
- Classic + Tunnel + Recording;
- Physics Source + Classic + Crystal RealMesh3D.

Do not hardcode one mode in a way that disables all others.

---

## 13. Shader Rules

Shader parameters must be controlled through dedicated modules.

Allowed:

- MirrorModule sets mirror shader parameters.
- TunnelModule sets tunnel shader parameters.
- DiamondFocus/Crystal renderer sets crystal shader parameters.
- PostFX module sets post-processing parameters.

Forbidden:

- UI directly calls material.SetFloat.
- Input directly edits shader values.
- AudioReactive directly edits material parameters.
- RuntimeMenuController directly edits crystal material values.

Shader property names must be constants.

Example:

```csharp
public static class MirrorShaderIds
{
    public static readonly int MirrorCount = Shader.PropertyToID("_MirrorCount");
    public static readonly int CenterOffset = Shader.PropertyToID("_CenterOffset");
}
```

---

## 14. Camera Rules

Camera logic must be centralized in CameraModule or explicitly owned by the module that needs the camera.

Forbidden:

- production logic using Camera.main;
- recording logic guessing active camera;
- multiple scripts enabling/disabling cameras independently;
- hidden camera creation without registration.

Every production camera must have a clear role:

- SourceCamera;
- ViewerCamera;
- RenderCamera;
- OfflineCamera;
- TunnelCamera if needed;
- CrystalCamera only if explicitly owned/registered by CrystalStage3D or CameraModule.

---

## 15. Recording Rules

Recording must be deterministic where possible.

Required principles:

- fixed target FPS;
- stable frame count;
- no dependency on editor preview timing;
- offline frame export must not rely on variable Update timing;
- ffmpeg assembly must be separated from frame rendering.

Recording pipeline:

1. prepare scene;
2. validate cameras and RenderTextures;
3. render frame sequence;
4. export PNG sequence;
5. assemble final video with ffmpeg;
6. report result.

Recording must not change visual architecture.

---

## 16. Performance Rules

Avoid garbage generation in Update.

Forbidden in Update/LateUpdate/FixedUpdate:

- FindObjectOfType;
- GameObject.Find;
- LINQ allocations;
- repeated material instantiation;
- repeated RenderTexture allocation;
- string-heavy logging every frame.

RenderTextures must be reused where possible.

Temporary resources must be released safely.

Billboard2D exists specifically as a performance-friendly crystal mode.

RealMesh3D may be more expensive, but must still avoid uncontrolled lights, shadows, allocations, or excessive material instances.

---

## 17. Unity Serialization Rules

Preserve serialized fields.

Do not rename serialized fields without migration.

Do not break prefab or scene references.

When renaming classes/files:

- keep Unity file/class naming consistent;
- check references;
- document migration risk.

Enums used in serialized Unity components must be extended carefully:

- append new values where possible;
- avoid reordering existing values;
- document migration risk.

---

## 18. Validation Rules

After every implementation step, provide a report with:

### What changed

Short explanation.

### Why

Reason for the change.

### Files touched

List of changed files.

### Architecture impact

Explain whether module boundaries were preserved.

### Validation

Mention what was checked.

### Risks / Follow-up

Mention remaining risks.

For RealMesh3D tasks, the validation report must explicitly confirm:

- mesh is not a plane;
- mesh has thickness;
- side faces are visible;
- MeshFilter + MeshRenderer are used;
- object is visible as volumetric from side view;
- Billboard2D still works separately;
- Layer 1 was not modified unless explicitly approved;
- no projection panel is pretending to be the crystal.

---

## 19. Development Stage Rules

Do not rewrite the entire project in one step.

Use staged development.

Recommended stages:

### Stage 00 — Architecture Audit

- inspect current project;
- map dependencies;
- find direct cross-module calls;
- find Camera.main usage;
- find shader writes outside modules;
- produce report only;
- do not modify code.

### Stage 01 — Core + Director

- create Core folder;
- add Director;
- add State;
- add command structure;
- add module interface.

### Stage 02 — Input Routing

- route keyboard/mouse/gamepad through InputModule;
- remove direct input-to-shader/camera/physics writes.

### Stage 03 — Source + Mirror Separation

- isolate source texture generation;
- isolate mirror rendering;
- define source-to-mirror pipeline.

### Stage 04 — Camera Module

- centralize cameras;
- remove Camera.main dependencies;
- validate RenderTexture assignments.

### Stage 05 — Physics Chamber Module

- isolate physical gem chamber;
- expose only source texture/status/shake commands.

### Stage 06 — Audio Reactive Module

- convert audio analysis to events/commands;
- no direct visual mutation.

### Stage 07 — Tunnel Module

- add tunnel as downstream projection mode;
- preserve classic mode.

### Stage 08 — Recording Module

- consume final output RenderTexture;
- add deterministic offline export.

### Stage 09 — Presets + Diagnostics

- add preset system;
- add debug HUD;
- add validation reports.

### Stage 10 — Layer 2 Crystal3D Stage

- protect Layer 1 as read-only;
- preserve Billboard2D performance mode;
- rebuild RealMesh3D as true Layer 2 volumetric object;
- remove projection-plane composition from Premium Crystal;
- add crystal diagnostics.

---

## 20. Coding Style

Use clear class names.

Prefer:

- MirrorModule;
- CameraModule;
- RecordingModule;
- KaleidoscopeDirector;
- KaleidoscopeState;
- DiamondFocusModule;
- CrystalPresentationModule;
- BillboardCrystalRenderer;
- RealMeshCrystalRenderer;
- CrystalStage3DModule;
- CrystalLightRigModule.

Avoid vague names:

- Manager;
- Controller2;
- NewScript;
- Helper;
- TempFix;
- MainLogic.

A class name must explain its responsibility.

---

## 21. Public API Rules

Each module must expose a small public API.

Example:

MirrorModule may expose:

- SetMirrorCount(int count);
- SetRotationSpeed(float speed);
- SetCenterOffset(Vector2 offset);
- SetZoom(float zoom);
- Render(RenderTexture source, RenderTexture target).

Crystal renderer interface may expose:

- Initialize(...);
- SetSourceTexture(RenderTexture texture);
- SetShape(...);
- SetMaterialMode(...);
- SetRotation(...);
- SetVisible(bool visible);
- Tick(float deltaTime);
- Shutdown().

MirrorModule must not expose all internal materials, cameras, and temporary objects unless necessary.

Crystal renderers must not expose unrelated internal scene objects unless necessary.

---

## 22. Error Handling

Modules must fail clearly.

If a dependency is missing:

- log a clear warning/error;
- report to Diagnostics;
- do not silently fail;
- do not create hidden fallback behavior unless documented.

Example:

```text
[MirrorModule] Missing source RenderTexture. Mirror rendering skipped.
```

Example:

```text
[RealMeshCrystalRenderer] Invalid mesh: flat plane detected. RealMesh3D validation failed.
```

---

## 23. Documentation Rules

Every module folder should contain short documentation if the module becomes complex.

Recommended docs:

- Docs/ARCHITECTURE.md;
- Docs/RENDER_PIPELINE.md;
- Docs/MODULE_BOUNDARIES.md;
- Docs/RECORDING_PIPELINE.md;
- Docs/TUNNEL_MODE.md;
- DiamondFocus/Docs/CRYSTAL_PRESENTATION_ARCHITECTURE.md;
- Menu/Docs/MENU_ARCHITECTURE.md;
- Menu/Docs/MENU_AUDIT.md.

Documentation must explain:

- responsibility;
- inputs;
- outputs;
- dependencies;
- forbidden interactions;
- validation criteria.

---

## 24. Absolute Architecture Prohibitions

Never do these without explicit approval:

- rewrite the full project at once;
- merge modules into one large controller;
- let UI directly control shader/camera/physics;
- let UI directly control crystal material internals;
- let AudioReactive directly control shader/camera/physics/crystal;
- let Recording search for cameras dynamically;
- make Tunnel Mode replace the classic pipeline;
- make Crystal Mode replace the classic pipeline;
- make RealMesh3D mutate Layer 1;
- hide production logic in editor-only scripts;
- create circular dependencies;
- introduce global mutable state without Director/State;
- call a flat plane, quad, billboard, sprite, or RawImage a RealMesh3D crystal;
- use a large projection panel as Premium Crystal.

---

## 25. Preferred Mental Model

Think of the project as a hardware rack:

- Menu = commercial front panel;
- Control Panel = operator/developer panel;
- Director = central controller / dispatcher;
- State = system status memory;
- Source = signal generator;
- Mirror = image processor;
- Layer 1 = display monitor;
- Layer 2 = separate crystal optical stage;
- Camera = optical routing;
- AudioReactive = signal analyzer;
- Tunnel = optional projection device;
- DiamondFocus / CrystalPresentation = optional optical crystal device;
- Billboard2D = low-cost crystal display adapter;
- RealMesh3D = premium volumetric crystal device;
- Recording = output recorder;
- Diagnostics = monitoring panel.

Each block has cables, but blocks are not soldered into each other.

---

## 26. Final Rule

When in doubt, preserve modularity.

A quick fix that breaks architecture is not acceptable.

A slower fix that keeps module boundaries clear is preferred.

The project must remain understandable, expandable, and safe to modify.

If a crystal implementation is flat, classify it as Billboard2D.  
If a task requires RealMesh3D, it must be a true volumetric faceted mesh.  
If a RealMesh3D implementation uses a projection plane as the main crystal, the task is failed.
