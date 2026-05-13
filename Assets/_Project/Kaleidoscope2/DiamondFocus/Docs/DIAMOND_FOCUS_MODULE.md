# Diamond Focus Module

Stage: DIAMOND-FOCUS-02

## Responsibility

`DiamondFocusModule` is a reusable downstream optical layer. It consumes the already-rendered kaleidoscope texture from any scene that registers it and adds a central solid faceted diamond/crystal plus a lightweight background blur pass.

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
- `Numpad +`: next diamond shape with a 2 second morph.
- `Numpad -`: previous diamond shape with a 2 second morph.
- `Numpad . / Delete`: next crystal material mode.
- `Home / End`: increase / decrease refraction index in the range `0..10`.

When Diamond Focus is off, numpad controls return to the rest of the project. When it is on, the numpad rotation keys are reserved for the crystal in every visual mode.

## Shapes

1. Classic diamond
2. Faceted cube
3. Disco-ball style faceted sphere
4. Triangular crystal
5. Rhombic crystal
6. Oval ring gem

## Crystal Material Modes

`Numpad . / Delete` cycles the crystal material behavior independently from shape:

1. Absolute mirror.
2. Diamond glass.
3. Generated glow with a fresh color tint every time the mode is selected.
4. Optical object approximation using IOR, caustics, dispersion, and total internal reflection controls.
5. Generated material: one of wood, metal, plastic, or stone is picked every time the mode is selected.

## Cinematic Crystal Optics

The crystal shader exposes an artistic optical stack for a stronger high-end look:

- `diamondLikeRefraction`: stronger diamond-style bending of the kaleidoscope image.
- `spectralDispersion`: wider RGB splitting and spectral fire on facets.
- `highEnergyCaustics`: bright caustic streaks and flashes on internal facet intersections.
- `multiBounceInternalReflections`: extra internal reflection samples before final compositing.
- `cinematicCrystalOptics`: global polish/glow shaping for a more filmic crystal response.
- `physicallyBasedRefraction`: screen-space approximation driven by IOR and refracted vectors.
- `deepVolumetricLightScattering`: soft internal light volume, kept inside the crystal layer.
- `crystalSolidity`: keeps the composite in an opaque real-stone presence. Default is fully solid.
- `blueWhitePlasmaEnergy`: boosts cold white-blue internal caustics and high-energy facet flashes.
- `directTransmission`: limits how much raw background can pass straight through the crystal. Default is very low, so the object reads as a dense diamond instead of cheap glass.
- `totalInternalReturn`: boosts total-internal-reflection style light return toward the viewer.
- `spectralFireIntensity`: controls rainbow fire from dispersion and caustic bands.
- `facetDepthContrast`: deepens dark/bright facet separation so the cut feels dimensional.
- `opticalIOR`: runtime refraction index, controlled by `Home / End` and clamped to `0..10`.

## 3D Rendering

The current implementation renders actual Unity meshes into a module-owned RenderTexture with an explicit internal offscreen camera. The crystal shader writes a solid alpha mask, suppresses direct transmission, and uses refracted/reflected samples from the already-rendered kaleidoscope texture as indirect internal light. Diamond mode is dominated by total internal reflection, dark facet depth, cold white-blue return light, and spectral fire rather than simple background visibility. A separate separable Gaussian pass blurs only the background, then the composite shader places the sharp crystal over that blurred background.

Background blur is speed-driven:

- `blurStrength`: baseline blur amount, default `0`.
- `maxBlurRadius`: maximum blur radius, default `6`.
- `blurIterations`: separable blur iterations, default `2`.
- `blurDownsample`: blur buffer downsample, default `1`.

The old artifact-prone inline sampling path is disabled; composite shaders no longer generate blur themselves.

The offscreen camera is created and owned by `DiamondFocusModule`; it does not use `Camera.main`, scene-name lookup, or `FindObjectOfType`. The renderer uses a configurable layer mask (`crystalLayer`, default 31) so the crystal pass does not render unrelated scene objects.
