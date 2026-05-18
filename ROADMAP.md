# ROADMAP — KAELIS Premium 3D Crystal Mode

## Цель

Создать премиальный 3D-режим кристалла в стиле демонстрационного оптического стенда:

Camera
↓
RealMesh3D Crystal
↓
Stage Background / Optical Hall
↓
Crystal Light Rig

Главная идея:
кристалл — главный объект сцены, а не эффект поверх картинки.

---

## STAGE 01 — Концептуальная фиксация режима

Цель:
Зафиксировать, что Premium 3D Crystal Mode — это отдельная витрина, а не Billboard2D и не fullscreen composite.

Нужно:
- описать режим как отдельный CrystalStage3D;
- сохранить Legacy DiamondFocus как fallback;
- запретить использовать fullscreen plane как главный визуальный объект;
- зафиксировать композицию: камера → кристалл → фон.

Критерий готовности:
Codex понимает, что Premium 3D = stage scene, а не shader overlay.

---

## STAGE 02 — Чистая пространственная сцена

Цель:
Собрать правильную сценическую основу.

Нужно создать:

CrystalStage3DRoot
├── CrystalCamera
├── RealMeshCrystal
├── BackgroundGeometry / OpticalHall
├── CrystalLightRig
└── StageDiagnostics

Требования:
- кристалл в центре;
- фон позади;
- камера смотрит через кристалл;
- физические объекты stage не видны Main Camera напрямую;
- Main output получает только Stage RenderTexture.

Критерий готовности:
в Scene View сбоку видно: Camera → Crystal → Background.

---

## STAGE 03 — Геометрия кристалла

Цель:
Сделать набор настоящих объёмных форм.

Формы:

1. Classic Diamond
2. Octagon
3. Hexagon
4. Drop / Pear
5. Marquise
6. Cushion

Требования:
- MeshFilter + MeshRenderer;
- настоящая толщина;
- side faces;
- front/back separation;
- silhouette меняется при вращении;
- форма не является plane/quad/billboard.

Критерий готовности:
каждая форма выглядит объёмной даже с простым solid material.

---

## STAGE 04 — Размер и кадрирование

Цель:
Сделать кристалл главным объектом кадра.

Требования:
- кристалл занимает 35–55% высоты кадра;
- центрирован;
- не обрезается;
- не превращается в миниатюру;
- background не становится гигантской простынёй;
- масштабируется вся stage-diorama, а не отдельная плоскость.

Критерий готовности:
кристалл читается как центральный premium-объект.

---

## STAGE 05 — Crystal Light Rig

Цель:
Создать сценический свет как на глянцевом рендере.

Нужно:
- key light;
- rim lights;
- fill light;
- small moving glint lights;
- optional circular light rig;
- выключаемые режимы света: 1 / 2 / 4 / 8 sources.

Правила:
- свет должен работать на кристалл;
- не создавать хаотичные пересветы;
- не ломать фон;
- не включать дорогие realtime shadows по умолчанию.

Критерий готовности:
грани ловят свет, видны блики и рёбра.

---

## STAGE 06 — Оптический материал

Цель:
Создать материал “дорогого стекла / бриллианта”.

Свойства:
- transparency;
- Fresnel;
- specular highlights;
- internal reflection;
- controlled refraction;
- chromatic dispersion;
- edge glints;
- minimum visibility floor, чтобы кристалл не проваливался в чёрное.

Важно:
не рисовать калейдоскоп как albedo по всей поверхности.

Правильно:
кристалл не “показывает картинку”,
а “преломляет и отражает окружение”.

Критерий готовности:
даже без подписей понятно, что это стекло/кристалл, а не тёмный камень.

---

## STAGE 07 — Kaleidoscope Texture Inside Crystal

Цель:
Подключить итоговую текстуру калейдоскопа как оптический источник.

Использовать FinalKaleidoscopeTexture как:
- environment/refraction input;
- internal optical texture;
- background behind crystal;
- controlled reflection source.

Запрещено:
- клеить texture как плоскую картинку на грани;
- превращать кристалл в экран;
- делать fullscreen projection.

Критерий готовности:
картинка видна внутри/через кристалл, но кристалл остаётся объёмным объектом.

---

## STAGE 08 — Background / Optical Hall

Цель:
Сделать фон как премиальную сцену-витрину.

Варианты:
- dark optical hall;
- cinematic lab;
- abstract black glass room;
- circular pedestal;
- light columns;
- subtle volumetric haze.

Требования:
- фон позади кристалла;
- не спорит с кристаллом;
- не выглядит как Unity test plane;
- можно заменить на простую тёмную сцену для performance mode.

Критерий готовности:
сцена выглядит как коммерческий premium render, а не debug preview.

---

## STAGE 09 — Поворот и управление

Цель:
Сделать управление кристаллом.

Управление:
- П / G — переключение Classic2D ↔ Premium3D;
- Numpad +/- — переключение формы;
- отдельные клавиши — скорость вращения;
- F5/F6/F7/F8 — количество источников света;
- Reset — вернуть сцену к дефолту.

Требования:
- вращается кристалл, не камера;
- камера остаётся стабильной;
- debug orbit только для проверки.

Критерий готовности:
режимом можно управлять без разрушения сцены.

---

## STAGE 10 — UI Overlay / Информационная панель

Цель:
Добавить подписи как на концепте, но без вмешательства в rendering core.

Блоки:
- Объёмная гранёная геометрия;
- Оптический материал;
- Kaleidoscope Texture;
- Crystal Light Rig;
- Поворот кристалла;
- Варианты форм.

Правила:
- UI отдельным Canvas/Panel;
- TextMeshPro;
- не baked text на фоне;
- можно выключить overlay;
- UI не меняет shader/camera напрямую.

Критерий готовности:
режим можно показать как презентационный экран.

---

## STAGE 11 — Shape Selector Strip

Цель:
Сделать нижнюю панель выбора форм.

Нужно:
- 6 thumbnails форм;
- active selected state;
- hover/pressed state;
- подписи:
  - Classic Diamond
  - Octagon
  - Hexagon
  - Drop
  - Marquise
  - Cushion

Критерий готовности:
форма переключается визуально и понятно.

---

## STAGE 12 — Diagnostics & Proof

Цель:
Не потерять контроль над архитектурой.

Диагностика должна показывать:
- active mode: Classic2D / Premium3D;
- active crystal shape;
- crystal screen coverage;
- camera → crystal distance;
- crystal → background distance;
- stage RT size;
- physical stage visible to Main Camera: false;
- Layer 1 modified: false.

Критерий готовности:
если что-то снова станет “простынёй”, это видно сразу.

---

## STAGE 13 — Performance Profiles

Цель:
Разделить качество.

Профили:
- Preview;
- High;
- Ultra;
- Offline Render.

Preview:
- меньше lights;
- проще material;
- без дорогих эффектов.

Ultra:
- больше glints;
- сильнее dispersion;
- higher RT;
- cinematic background.

Критерий готовности:
режим можно запустить не только на сильной машине.

---

## STAGE 14 — Commercial Polish

Цель:
Довести до уровня “глянцевого журнала”.

Добавить:
- мягкую камеру;
- controlled bloom;
- subtle lens flare;
- vignette;
- тонкую хроматическую аберрацию;
- cinematic color grading;
- intro transition.

Критерий готовности:
скриншот режима можно использовать как промо-материал.

---

## STAGE 15 — Freeze Stable Baseline

Цель:
Зафиксировать рабочий Premium3D.

Нужно:
- Unity compile;
- Play Mode test;
- переключение Classic2D ↔ Premium3D;
- проверка всех форм;
- проверка lights;
- git commit;
- больше не трогать spatial core без отдельной причины.

Критерий готовности:
Premium3D стал стабильной базой для дальнейшей оптики.