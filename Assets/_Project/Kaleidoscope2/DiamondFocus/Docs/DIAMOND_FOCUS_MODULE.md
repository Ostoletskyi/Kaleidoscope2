# Diamond Focus Module

Stage: DIAMOND-FOCUS-02

## Responsibility

`DiamondFocusModule` is a reusable downstream optical layer. It consumes the already-rendered kaleidoscope texture from any scene that registers it and adds a central solid faceted diamond/crystal plus a lightweight background blur pass.

## Pipeline

Source -> Mirror/Mode Processors -> DiamondFocus / CrystalPresentation -> Final Output

The module is pass-through when disabled. It is disabled by default so existing scenes keep their current controls until the user presses `Backspace`. The project prefab defaults to all visual modes (`onlyIn4DMode = false`), so the same crystal control layer can be reused in 2D, 3D, 4D, 5D, 6D, and 7D.

## Scene Reuse

Use `Assets/_Project/Kaleidoscope2/DiamondFocus/Prefabs/DiamondFocusModule.prefab` in any scene that has a `KaleidoscopeBootstrap`.

Register the prefab instance in `KaleidoscopeBootstrap.moduleRegistrations` with:

- `moduleArea`: `DiamondFocus`
- `moduleBehaviour`: the prefab instance's `DiamondFocusModule`
- `required`: `false`

The optional premium highlight rig lives in `Assets/_Project/Kaleidoscope2/DiamondFocus/Lighting/CrystalLightRigModule.cs`. Add it as a separate scene object and register it with:

- `moduleArea`: `CrystalLightRig`
- `moduleBehaviour`: the scene object's `CrystalLightRigModule`
- `required`: `false`

The presentation strategy layer lives in `Assets/_Project/Kaleidoscope2/DiamondFocus/CrystalPresentationModule.cs`. Register it after `DiamondFocusModule` with:

- `moduleArea`: `CrystalPresentation`
- `moduleBehaviour`: the scene object's `CrystalPresentationModule`
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
- `Numpad . / Numpad Del`: next crystal material mode.
- `Home / End`: increase / decrease refraction coefficient in the range `0..10`.
- `PageUp / PageDown`: increase / decrease directed crystal light intensity in the range `-10..+10`, where `0` is normal.
- `M`: enable / disable the optional Crystal Light Rig.
- `Insert / Delete`: increase / decrease Crystal Light Rig intensity in the range `0..20`.
- `G` on an English keyboard / `П` on a Russian keyboard: switch Crystal Simulation between `2D Performance` and `3D Premium`.

When Diamond Focus is off, numpad controls return to the rest of the project. When it is on, the numpad rotation keys are reserved for the crystal in every visual mode.

## Shapes

1. Classic diamond
2. Faceted cube
3. Disco-ball style faceted sphere
4. Triangular crystal
5. Rhombic crystal
6. Oval ring gem

## Crystal Material Modes

`Numpad . / Numpad Del` cycles the crystal material behavior independently from shape:

1. Absolute Mirror + Prism.
2. High-Purity Diamond.
3. Emerald, Topaz, Ruby, Sapphire, Amethyst, Aquamarine, and Garnet.
4. Futuristic Plastic.
5. Mercury, Stainless Steel, Chrome, Cast Iron, and Polished Brass.

## Cinematic Crystal Optics

The crystal shader exposes an artistic optical stack for a stronger high-end look:

- `_KaleidoscopeTex`: explicit texture input bound by `DiamondFocusModule` from the current processed kaleidoscope output.
- `refractionStrength`: screen-space bending amount for `_KaleidoscopeTex`.
- `dispersionStrength`: global scale for RGB split/refraction fire.
- `reflectionStrength`: reflected samples are taken from `_KaleidoscopeTex`, not skybox, probes, grab pass, or camera opaque texture.
- `fresnelPower`: controls the edge-weighted optical response.
- `internalBrightness`: scales bloom-compatible internal highlights, caustics, and glow.
- `noiseDistortionStrength`: procedural patterns are limited to subtle distortion/intensity variation.
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
- `opticalIOR`: runtime refraction coefficient, controlled by `Home / End` and clamped to `0..10`.
- `directedLightIntensity`: runtime key-light intensity for Fresnel return, glints, and internal reflection, controlled by `PageUp / PageDown` and clamped to `-10..+10`.

Glints are micro highlights on facet/light alignment. They are computed inside the crystal shader from facet normals, Fresnel, and a directed key light, then colored with thin `_KaleidoscopeTex` dispersion samples so the flashes stay tied to the current kaleidoscope output.

## Crystal Light Rig

`CrystalLightRigModule` is a separate optional DiamondFocus lighting block. It creates one soft warm-white key light, two cool rim lights, six orbit glint lights, and three subtle spectral accent lights. The lights are unshadowed point lights with a crystal-layer culling mask by default, so they stay cheap and do not dominate the background.

The crystal shader also receives explicit rig parameters through `CrystalMaterialBinder`:

- `_CrystalLightRigEnabled`
- `_CrystalRigLightIntensity`
- `_CrystalRigGlintIntensity`
- `_CrystalRigRimIntensity`
- `_CrystalRigSpectralIntensity`
- `_CrystalRigPulse`
- `_CrystalRigOrbitPhase`

The rig intensity is stored in `CrystalLightRigSettings` and clamped to `0..20`. Crystal rotation speed increases the resolved glint and spectral intensity, and the optional pulse rides on top of that speed response. This keeps highlights alive when the crystal spins faster without coupling the rig to `MirrorModule` or `SourceModule`.

## Crystal Presentation Strategy

The crystal presentation layer uses a strategy interface:

```csharp
public interface ICrystalRenderer
{
    void Initialize(CrystalSharedSettings settings);
    void SetSourceTexture(RenderTexture texture);
    void SetShape(CrystalShape shape);
    void SetMaterialMode(CrystalMaterialMode mode);
    void SetRotation(Vector3 rotation);
    void SetIntensity(float value);
    void SetVisible(bool visible);
    void Render();
    void Shutdown();
}
```

`CrystalPresentationModule` owns the strategy choice and passes the same final kaleidoscope texture to either:

- `BillboardCrystalRenderer`: the 2D Performance branch. During this migration step it acts as an adapter around the existing fast DiamondFocus composite, so the current look is preserved.
- `RealMeshCrystalRenderer`: the 3D Premium prototype. It creates a real `MeshRenderer`, faceted placeholder mesh, transparent material, internal render camera, and `RealMesh/CrystalLightRigModule` placeholder lights.

The two renderers do not know about each other. Both receive only `CrystalSharedSettings`, source texture, visibility, shape, material, rotation, and intensity. `DiamondFocusModule` becomes pass-through when `CrystalSimulationMode` is `RealMesh3D`, so the new 3D branch does not duplicate the old billboard composite.

Debug mode is held in `DiamondFocusSettings` and routed by the module to `_CrystalDebugMode`:

1. Final crystal composite.
2. Raw `_KaleidoscopeTex`.
3. Refraction only.
4. Reflection only.
5. Dispersion only.
6. Surface / normal only.
7. Crystal off.
8. Artifact stress test.

If `_KaleidoscopeTex` is missing or deliberately disabled on the module, the crystal renders a magenta warning fallback and reports a module warning instead of sampling unrelated scene or environment data.

## 3D Rendering

The current implementation renders actual Unity meshes into a module-owned RenderTexture with an explicit internal offscreen camera. The crystal shader writes a solid alpha mask, suppresses direct transmission, and uses refracted/reflected samples from the already-rendered kaleidoscope texture as indirect internal light. Diamond mode is dominated by total internal reflection, dark facet depth, cold white-blue return light, and spectral fire rather than simple background visibility. A separate separable Gaussian pass blurs only the background, then the composite shader places the sharp crystal over that blurred background.

Background blur is speed-driven:

- `blurStrength`: baseline blur amount, default `0`.
- `maxBlurRadius`: maximum blur radius, default `6`.
- `blurIterations`: separable blur iterations, default `2`.
- `blurDownsample`: blur buffer downsample, default `1`.

The old artifact-prone inline sampling path is disabled; composite shaders no longer generate blur themselves.

The offscreen camera is created and owned by `DiamondFocusModule`; it does not use `Camera.main`, scene-name lookup, or `FindObjectOfType`. The renderer uses a configurable layer mask (`crystalLayer`, default 31) so the crystal pass does not render unrelated scene objects.
