# ROADMAP.md — Kaleidoscope2

## Главная цель

Создать модульный Unity-проект Kaleidoscope2 с блочной архитектурой:

UI / Input / Audio
        ↓
KaleidoscopeDirector
        ↓
Independent Modules
        ↓
Final Render Output

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

Assets/_Project/Kaleidoscope2/
├── Core/
├── Control/
├── Input/
├── Source/
├── PhysicsChamber/
├── Mirror/
├── Camera/
├── AudioReactive/
├── Tunnel/
├── Recording/
├── Presets/
├── Diagnostics/
├── Shaders/
├── Materials/
├── Scenes/
└── Docs/

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

`RenderTexture GetSourceTexture()`

## Критерий готовности

Источник генерирует RenderTexture без участия Mirror, Camera, Recording или Tunnel.

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
  - OfflineCamera.
- Запретить использование `Camera.main` в production logic.
- Все камеры регистрируются явно.

## Критерий готовности

Камеры управляются только CameraModule.  
Нет хаотичного включения/выключения камер из разных скриптов.

---

# STAGE 06 — Control Panel

## Цель

Создать единый пульт управления.

## Задачи

- Создать Runtime UI или Editor Window.
- Добавить управление:
  - mirror count;
  - zoom;
  - rotation speed;
  - center offset;
  - source mode;
  - tunnel mode;
  - recording;
  - presets.
- UI отправляет команды только в Director.

## Критерий готовности

Пользователь управляет системой через один пульт.  
UI не трогает shader/camera/physics напрямую.

---

# STAGE 07 — Input Module

## Цель

Отделить физический ввод от логики.

## Задачи

- Создать `InputModule`.
- Поддержать:
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
  - Tunnel Preview.
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

Музыка влияет на визуал через команды, а не прямые изменения shader/camera.

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

## Критерий готовности

Проект работает стабильно без постоянного мусора и просадок.

---

# STAGE 14 — Documentation

## Цель

Зафиксировать архитектуру.

## Задачи

Создать:

- `Docs/ARCHITECTURE.md`
- `Docs/RENDER_PIPELINE.md`
- `Docs/MODULE_BOUNDARIES.md`
- `Docs/RECORDING_PIPELINE.md`
- `Docs/TUNNEL_MODE.md`

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
- Позже подключить Physics, Audio, Tunnel, Recording.

## Критерий готовности

Есть одна главная сцена, где система работает как единый движок.

---

# Development Rules

## Нельзя

- переписывать весь проект одним коммитом;
- смешивать модули;
- делать прямые связи между UI и shader;
- использовать `Camera.main` как основу архитектуры;
- искать объекты через `FindObjectOfType` в Update;
- добавлять Tunnel Mode так, чтобы он ломал Classic Mode;
- добавлять Recording так, чтобы он управлял сценой напрямую.

## Нужно

- двигаться маленькими Stage;
- после каждого Stage делать отчёт;
- сохранять модульные границы;
- проверять Unity compile;
- документировать риски.

---

# Required Report Format

После каждого этапа агент должен отвечать так:

## Stage ID

Например: STAGE 03 — Source Module

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

Начинать строго с:

1. STAGE 00 — Project Foundation
2. STAGE 01 — Core Architecture
3. STAGE 02 — Diagnostics First
4. STAGE 03 — Source Module
5. STAGE 04 — Mirror Module

Не переходить к Tunnel, AudioReactive и Recording до появления стабильного Source → Mirror → Final Output pipeline.