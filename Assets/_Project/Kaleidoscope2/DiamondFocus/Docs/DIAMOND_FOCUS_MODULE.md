# Diamond Focus Module

Stage: DIAMOND-FOCUS-02

## Responsibility

`DiamondFocusModule` is a reusable downstream optical layer. It consumes the already-rendered kaleidoscope texture from any scene that registers it and adds a central transparent faceted diamond/crystal plus a lightweight background blur pass.

## Pipeline

Source -> Mirror/Mode Processors -> DiamondFocus -> Final Output

The module is pass-through when disabled. It is disabled by default so existing scenes keep their current controls until the user presses `Backspace`. The project prefab defaults to all visual modes (`onlyIn4DMode = false`), so the same crystal control layer can be reused in 2D, 3D, 4D, 5D, 6D, and 7D.

## Scene Reuse

Use `Assets/_Project/Kaleidoscope2/DiamondFocus/Prefabs/DiamondFocusModule.prefab` in any scene that has a `KaleidoscopeBootstrap`.

Register the prefab instance in `KaleidoscopeBootstrap.moduleRegistrations` with:

- `moduleArea`: `DiamondFocus`
- `moduleBehaviour`: the prefab instance's `DiamondFocusModule`
- `required`: `false`

## Boundaries

Allowed:

- Input emits Diamond commands through `KaleidoscopeDirector`.
- `KaleidoscopeDirector` owns global enable/disable state for Diamond Focus.
- `DiamondFocusModule` owns its real 3D crystal render pass, shape switching, rotation smoothing, and optics parameters.

Forbidden:

- Diamond code does not modify `MirrorModule`, `SourceModule`, or `RecordingModule` internals.
- Input and UI do not edit diamond material parameters directly.
- No production `Camera.main` or runtime object-name discovery.

## Controls

- `Backspace`: enable/disable Diamond Focus.
- `Numpad 8`: accelerate pitch upward.
- `Numpad 2`: accelerate pitch downward.
- `Numpad 4`: accelerate yaw left.
- `Numpad 6`: accelerate yaw right.
- `Numpad 7 / 9 / 1 / 3`: accelerate diagonal rotation.
- `Numpad +`: next diamond shape.
- `Numpad -`: previous diamond shape.
- `Numpad . / Delete`: next crystal material mode.

When Diamond Focus is off, numpad controls return to the rest of the project. When it is on, the numpad rotation keys are reserved for the crystal in every visual mode.

## Shapes

1. Classic diamond
2. Faceted cube
3. 12-facet crystal
4. Tetrahedral crystal
5. 96-facet diamond

## Crystal Material Modes

`Numpad . / Delete` cycles the crystal material behavior independently from shape:

1. Absolute mirror.
2. Diamond glass.
3. Generated glow with a fresh color tint every time the mode is selected.
4. Optical object approximation using IOR, caustics, dispersion, and total internal reflection controls.
5. Generated material: one of wood, metal, plastic, or stone is picked every time the mode is selected.

## 3D Rendering

The current implementation renders actual Unity meshes into a module-owned transparent RenderTexture with an explicit internal offscreen camera. The glass shader samples the already-rendered kaleidoscope texture to approximate refraction and reflection. A separate separable Gaussian pass blurs only the background, then the composite shader blends the sharp crystal over that blurred background.

Background blur is speed-driven:

- `blurStrength`: baseline blur amount, default `0`.
- `maxBlurRadius`: maximum blur radius, default `6`.
- `blurIterations`: separable blur iterations, default `2`.
- `blurDownsample`: blur buffer downsample, default `1`.

The old artifact-prone inline sampling path is disabled; composite shaders no longer generate blur themselves.

The offscreen camera is created and owned by `DiamondFocusModule`; it does not use `Camera.main`, scene-name lookup, or `FindObjectOfType`. The renderer uses a configurable layer mask (`crystalLayer`, default 31) so the crystal pass does not render unrelated scene objects.
