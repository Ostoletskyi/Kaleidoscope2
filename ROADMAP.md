# ROADMAP.md — Kaleidoscope2 / KAELIS

## Главная цель

Создать модульный Unity-проект Kaleidoscope2 / KAELIS с блочной архитектурой:

```text
UI / Input / Audio
        ↓
KaleidoscopeDirector
        ↓
Independent Modules
        ↓
Final Render Output
```

Главный принцип: каждый модуль отвечает только за свою область и не управляет внутренностями других модулей.

---

# STAGE 00 — Project Foundation

## Цель

Подготовить чистый фундамент проекта.

## Задачи

- Создать Unity-проект.
- Настроить Git.
- Добавить `.gitignore` для Unity.
- Добавить `AGENTS.md`.
- Добавить `ROADMAP.md`.
- Создать базовую структуру папок:

```text
Assets/_Project/Kaleidoscope2/
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
├── Recording/
├── Presets/
├── Diagnostics/
├── Shaders/
├── Materials/
├── Textures/
├── Scenes/
└── Docs/
```

## Критерий готовности

Проект открывается в Unity без ошибок.  
Git отслеживает только нужные файлы.

---

# STAGE 01 — Core Architecture

## Цель

Создать центральный управляющий слой.

## Задачи

- Создать `KaleidoscopeDirector`.
- Создать `KaleidoscopeState`.
- Создать `IKaleidoscopeModule`.
- Создать `KaleidoscopeCommand`.
- Создать базовый Event Bus / Command Bus.
- Добавить регистрацию модулей через Director.

## Критерий готовности

Все будущие модули могут подключаться к Director.  
Нет прямых связей между модулями.

---

# STAGE 02 — Diagnostics First

## Цель

Сразу видеть состояние системы.

## Задачи

- Создать `DiagnosticsModule`.
- Показывать:
  - активный режим;
  - FPS;
  - текущий source mode;
  - mirror count;
  - tunnel enabled/disabled;
  - crystal simulation mode;
  - Layer 1 / Layer 2 state;
  - recording status;
  - предупреждения;
  - ошибки ссылок.

## Критерий готовности

В сцене есть Debug HUD.  
Можно понять, что работает, а что сломано.

---

# STAGE 03 — Source Module

## Цель

Создать независимый источник изображения.

## Задачи

- Создать `SourceModule`.
- Поддержать базовые source modes:
  - color test texture;
  - image texture;
  - procedural texture;
  - RenderTexture input.
- SourceModule должен отдавать:

```csharp
RenderTexture GetSourceTexture()
```

## Критерий готовности

Источник генерирует RenderTexture без участия Mirror, Camera, Recording, Tunnel или DiamondFocus.

---

# STAGE 04 — Mirror Module

## Цель

Создать классический калейдоскоп.

## Задачи

- Создать `MirrorModule`.
- Создать/подключить `KaleidoscopeMirror.shader`.
- Реализовать:
  - mirror count;
  - rotation;
  - zoom;
  - center offset;
  - seam blending;
  - vignette;
  - chromatic aberration.

## Критерий готовности

Source RenderTexture превращается в Kaleidoscope RenderTexture.

---

# STAGE 05 — Camera Module

## Цель

Централизовать камеры.

## Задачи

- Создать `CameraModule`.
- Ввести роли камер:
  - SourceCamera;
  - ViewerCamera;
  - RenderCamera;
  - OfflineCamera;
  - TunnelCamera if needed;
  - CrystalCamera only if explicitly needed.
- Запретить использование `Camera.main` в production logic.
- Все камеры регистрируются явно.

## Критерий готовности

Камеры управляются только CameraModule.  
Нет хаотичного включения/выключения камер из разных скриптов.

---

# STAGE 06 — Control Panel

## Цель

Создать единый рабочий пульт управления для разработки и настройки.

Важно: `Control Panel` — это developer/operator UI.  
Он не равен коммерческому `Premium Menu`.

## Задачи

- Создать Runtime UI или Editor Window.
- Добавить управление:
  - mirror count;
  - zoom;
  - rotation speed;
  - center offset;
  - source mode;
  - tunnel mode;
  - crystal mode;
  - recording;
  - presets.
- UI отправляет команды только в Director.

## Критерий готовности

Пользователь управляет системой через один рабочий пульт.  
UI не трогает shader/camera/physics/crystal напрямую.

---

# STAGE 07 — Input Module

## Цель

Отделить физический ввод от логики.

## Задачи

- Создать `InputModule`.
- Поддержать:
  - keyboard;
  - mouse;
  - touch;
  - gamepad;
  - WASD для center offset;
  - mouse wheel для zoom;
  - arrows для rotation/twist;
  - Space для shake;
  - hotkeys для mode switching.
- InputModule создаёт команды и отправляет их Director.

## Критерий готовности

Ввод работает, но не управляет модулями напрямую.

---

# STAGE 08 — Physics Chamber Module

## Цель

Создать живой физический источник.

## Задачи

- Создать `PhysicsChamberModule`.
- Добавить:
  - камни;
  - particles;
  - Rigidbody;
  - chamber rotation;
  - shake;
  - avalanche;
  - material presets.
- Камера снимает физическую сцену в Source RenderTexture.

## Критерий готовности

Физическая колба может быть выбрана как Source Mode.

---

# STAGE 09 — Preset Module

## Цель

Сделать настройки управляемыми данными.

## Задачи

- Создать `KaleidoscopePreset`.
- Создать `PresetModule`.
- Поддержать пресеты:
  - Classic;
  - Jewelry;
  - Cosmic;
  - Aggressive Color;
  - Soft;
  - Audio Reactive;
  - Tunnel Preview;
  - Crystal Preview.
- Пресеты применяются только через Director.

## Критерий готовности

Можно переключать визуальные наборы без изменения кода.

---

# STAGE 10 — Audio Reactive Module

## Цель

Связать музыку с визуальными событиями.

## Задачи

- Создать `AudioReactiveModule`.
- Добавить:
  - audio analysis;
  - beat detection;
  - kick/snare/drop/build/break events.
- AudioReactiveModule генерирует команды:
  - zoom pulse;
  - brightness pulse;
  - rotation tick;
  - segment burst;
  - color pressure.

## Критерий готовности

Музыка влияет на визуал через команды, а не прямые изменения shader/camera/physics/crystal.

---

# STAGE 11 — Tunnel Module

## Цель

Добавить экспериментальный 3D tunnel mode.

## Задачи

- Создать `TunnelModule`.
- Добавить:
  - tunnel mesh;
  - projection shader;
  - end cap;
  - seam feather;
  - depth shading.
- Tunnel получает final kaleidoscope texture.

## Критерий готовности

Tunnel Mode включается/выключается без поломки Classic Mode.

---

# STAGE 12 — Recording Module

## Цель

Сделать стабильный экспорт результата.

## Задачи

- Создать `RecordingModule`.
- Реализовать:
  - preview recording;
  - offline PNG sequence export;
  - fixed FPS;
  - ffmpeg assembly;
  - audio sync.
- Recording берёт только Final Output RenderTexture.

## Критерий готовности

Можно получить видеофайл без зависимости от хаотичного Editor timing.

---

# STAGE 13 — Quality & Performance Pass

## Цель

Стабилизировать производительность.

## Задачи

- Убрать лишние allocations в Update.
- Проверить RenderTexture lifecycle.
- Проверить shader/material instances.
- Добавить quality levels:
  - Preview;
  - High;
  - Ultra;
  - Offline Render.
- Разделить производительные и премиальные режимы.

## Критерий готовности

Проект работает стабильно без постоянного мусора и просадок.

---

# STAGE 14 — Documentation

## Цель

Зафиксировать архитектуру.

## Задачи

Создать/обновить:

- `Docs/ARCHITECTURE.md`;
- `Docs/RENDER_PIPELINE.md`;
- `Docs/MODULE_BOUNDARIES.md`;
- `Docs/RECORDING_PIPELINE.md`;
- `Docs/TUNNEL_MODE.md`;
- `DiamondFocus/Docs/CRYSTAL_PRESENTATION_ARCHITECTURE.md`;
- `Menu/Docs/MENU_ARCHITECTURE.md`;
- `Menu/Docs/MENU_AUDIT.md`.

## Критерий готовности

Новый разработчик или AI-агент понимает проект без гадания.

---

# STAGE 15 — Production Scene

## Цель

Собрать рабочую демо-сцену.

## Задачи

- Создать `Kaleidoscope2_Main.unity`.
- Подключить:
  - Director;
  - State;
  - Control Panel;
  - Source;
  - Mirror;
  - Camera;
  - Diagnostics.
- Позже подключить:
  - Physics;
  - Audio;
  - Tunnel;
  - Recording;
  - DiamondFocus;
  - Menu.

## Критерий готовности

Есть одна главная сцена, где система работает как единый движок.

---

# STAGE 16 — Layer 2 Crystal3D Stage Rebuild

## Цель

Кардинально отделить премиальный 3D-кристалл от первого слоя калейдоскопа.

```text
Layer 1:
Source → Mirror → FinalOutput → OutputPreview

Layer 2:
CrystalStage3D → RealMesh3D Crystal → Crystal Camera/LightRig → Final Composite
```

Главный принцип:

```text
Layer 1 is the display/background layer.
Layer 2 is the separate real 3D crystal stage.
```

RealMesh3D Premium не является:

- billboard;
- projection plane;
- RawImage;
- UI layer;
- screen-space effect;
- quad with texture;
- large projected replica.

RealMesh3D Premium является:

- отдельным объёмным 3D-объектом;
- перед фоном;
- в центре кадра;
- с собственным светом;
- с собственной композицией;
- отдельным вторым слоем.

---

## Layer 1 — Read-Only Rule

Во всех задачах Stage 16 первый слой считается read-only.

Запрещено менять:

- `MirrorModule`;
- `SourceModule`;
- `OutputPreview`;
- `Billboard2D`;
- финальную 2D-плоскость отображения.

Если для исправления RealMesh3D кажется необходимым изменить Layer 1 — остановиться и доложить причину.

---

## Layer 2 — Allowed Work Area

Разрешено менять:

- `DiamondFocus/RealMesh/**`;
- `DiamondFocus/Lighting/**`;
- `CrystalStage3D/**`;
- `CrystalPresentationModule`;
- `RealMeshCrystalRenderer`;
- `CrystalLightRig`;
- диагностику второго слоя;
- минимальные команды Input/Core только если они нужны для управления вторым слоем.

---

## Required RealMesh3D Composition

Правильная композиция:

```text
Camera
        ↓
RealMesh3D Crystal — centered, large, volumetric
        ↓
Background / Kaleidoscope image behind crystal
```

Неправильная композиция:

```text
Camera
        ↓
Large flat projection plane
        ↓
Small crystal somewhere near the plane
```

---

## Required RealMesh3D Behavior

В режиме RealMesh3D Premium:

- виден один крупный объёмный кристалл в центре кадра;
- нет большой плоской реплики;
- нет маленького кристалла в углу;
- кристалл занимает примерно 25–40% высоты кадра;
- фон находится позади кристалла;
- кристалл виден как объёмный объект;
- кристалл вращается сам;
- камера не используется как основной способ доказать 3D;
- Billboard2D остаётся нетронутым.

---

## Required Validation

Validation camera must orbit around the crystal during proof/testing.

The crystal must visibly preserve:

- thickness;
- side faces;
- depth silhouette;
- volumetric presence.

A static frontal camera is not sufficient validation.

RealMesh3D must not rely on:

- screen-space-only distortion;
- single-plane projection;
- fake depth through UV;
- billboard illusion;
- camera-facing geometry tricks.

---

## Acceptance Criteria

В RealMesh3D Premium:

- нет большой плоской реплики;
- нет маленького кристалла в углу;
- есть один крупный кристалл в центре;
- кристалл занимает 25–40% высоты кадра;
- фон находится позади кристалла;
- кристалл виден как объёмный объект;
- Billboard2D остаётся нетронутым;
- MirrorModule и SourceModule остаются нетронутыми;
- диагностика показывает активное состояние Layer 2.

---

# STAGE 17 — Premium Menu Architecture

## Цель

Создать премиальную коммерческую систему меню для KAELIS.

Меню должно ощущаться как:

- cinematic optical workstation;
- premium visual engine;
- polished commercial application;

а не как:

- debug HUD;
- temporary Unity prototype;
- scattered developer buttons.

---

## Required Menu Architecture

```text
Menu UI
        ↓
MenuDirector
        ↓
KaleidoscopeDirector
        ↓
Modules
```

Menu system must remain isolated from rendering internals.

Menu must never directly mutate:

- shader parameters;
- camera internals;
- crystal internals;
- recording internals;
- physics internals.

Menu communicates through:

- MenuDirector;
- commands;
- KaleidoscopeDirector.

---

## Required Folder Structure

```text
Assets/_Project/Kaleidoscope2/Menu/
│
├── Core/
├── Input/
├── Panels/
├── Loading/
├── UI/
├── Theme/
├── Assets/
└── Docs/
```

---

## Required Responsibilities

Menu subsystem is responsible for:

- commercial start screen;
- premium presentation;
- panel hierarchy;
- button interaction;
- keyboard navigation;
- mouse navigation;
- touch navigation;
- loading transitions;
- async loading;
- menu sounds;
- visual theme;
- placeholder handling;
- menu diagnostics.

---

## Required Main Menu Structure

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

Each item must be classified as:

- ACTIVE;
- PARTIAL;
- PLACEHOLDER;
- FUTURE.

Codex must not pretend that a placeholder is an active feature.

---

## Required Menu Audit

Before implementing menu changes, inspect:

```text
Assets/_Project/Kaleidoscope2/Menu/
```

Then create/update:

```text
Assets/_Project/Kaleidoscope2/Menu/Docs/MENU_AUDIT.md
```

The audit must include:

- existing scripts;
- existing prefabs;
- existing scenes;
- current visual quality;
- missing systems;
- missing interactions;
- missing transitions;
- missing loading logic;
- missing button states;
- missing keyboard/mouse/touch support;
- missing premium polish;
- recommended implementation order.

---

## Required UI Quality

The menu must not look like:

- prototype UI;
- debug HUD;
- temporary developer overlay;
- stretched sprites;
- blurry text;
- random buttons;
- inconsistent spacing.

Required:

- TextMeshPro;
- clean typography;
- sharp UI;
- proper spacing;
- layered composition;
- premium dark/glass/optical visual style;
- proper hover/pressed/selected states;
- async loading transitions.

---

## Acceptance Criteria

- Menu folder audited.
- Missing systems documented.
- One production-quality button implemented before scaling to all buttons.
- Hover/click/keyboard/touch interactions work.
- Background art separated from UI text.
- No direct module mutation from menu code.
- UI visually readable and consistent.
- Menu behaves like a premium commercial application.

---

# Development Rules

## Нельзя

- переписывать весь проект одним коммитом;
- смешивать модули;
- делать прямые связи между UI и shader;
- использовать `Camera.main` как основу архитектуры;
- искать объекты через `FindObjectOfType` в Update;
- добавлять Tunnel Mode так, чтобы он ломал Classic Mode;
- добавлять Recording так, чтобы он управлял сценой напрямую;
- смешивать Billboard2D и RealMesh3D без интерфейса;
- называть плоскость, quad или billboard настоящим 3D-кристаллом;
- строить коммерческое меню как debug overlay;
- изменять Layer 1 при задачах Stage 16 без явного разрешения.

## Нужно

- двигаться маленькими Stage;
- после каждого Stage делать отчёт;
- сохранять модульные границы;
- проверять Unity compile;
- документировать риски;
- делать Git checkpoint перед крупными экспериментами;
- отделять production UI от developer/control UI;
- при задачах Stage 16 защищать Layer 1 и работать только со вторым слоем.

---

# Stage Lock Rule

Only the requested Stage is active.

All other stages are context, not permission to implement.

Examples:

- If the user asks for STAGE 16, do not implement STAGE 17.
- If the user asks for Menu Audit, do not build the full menu.
- If the user asks for RealMesh3D prototype, do not rewrite Billboard2D.
- If the user asks for shader fix, do not redesign the module system.

---

# Execution Mode

Codex must not implement multiple stages in one task.

For every task:

1. Read `AGENTS.md` and `ROADMAP.md`.
2. Run `git status` first.
3. Work only on explicitly requested Stage.
4. Do not touch unrelated modules.
5. Do not add future features early.
6. If the task needs changes outside the requested Stage, stop and report why.
7. Prefer audit/report before implementation.
8. Create small commits.

---

# Required Report Format

После каждого этапа агент должен отвечать так:

## Stage ID

Например:

```text
STAGE 16 — Layer 2 Crystal3D Stage Rebuild
```

## What changed

Что было добавлено или изменено.

## Why

Зачем это было сделано.

## Files touched

Список файлов.

## Architecture impact

Как это влияет на блочную архитектуру.

## Validation

Что проверено.

## Risks / Follow-up

Что осталось проверить или улучшить.

---

# Current Priority

Текущий приоритет:

1. Сохранять модульную архитектуру.
2. Стабилизировать `Source → Mirror → Final Output` pipeline.
3. Защитить Layer 1 как стабильный display/background слой.
4. Кардинально отделить RealMesh3D как Layer 2.
5. Сохранить Billboard2D как performance mode.
6. Реализовать RealMesh3D как настоящий объёмный кристалл, а не projection plane.
7. Провести аудит существующей папки Menu.
8. Построить premium commercial-grade menu subsystem.
9. Улучшать polish и optical quality перед добавлением крупных новых систем.

Текущий фокус:

- modularity;
- Layer 1 / Layer 2 separation;
- premium presentation;
- stable rendering pipeline;
- diagnostics;
- clean UI architecture;
- true volumetric crystal rendering;
- controlled Codex changes through documented stages.

Запрещено сейчас:

- rewriting the entire project;
- merging modules together;
- replacing the classic pipeline;
- treating flat planes as RealMesh3D;
- using a large projection panel as Premium Crystal;
- building chaotic prototype UI;
- direct UI-to-render-module mutation;
- adding large new feature groups before stabilizing current systems.
