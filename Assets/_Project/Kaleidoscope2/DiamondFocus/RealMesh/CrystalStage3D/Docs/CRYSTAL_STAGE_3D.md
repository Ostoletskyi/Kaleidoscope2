# CrystalStage3D

CrystalStage3D is the premium RealMesh3D rendering stage for DiamondFocus.

It is intentionally separate from Billboard2D and from the existing RealMesh3D fallback renderer. Its job is to render a miniature 3D scene:

```text
FinalKaleidoscopeTexture
    -> CrystalStage3DComposite background input
    -> real faceted MeshFilter + MeshRenderer crystal
    -> dedicated CrystalStage3D camera
    -> CrystalStageRenderTexture
    -> CrystalStage3DComposite
```

## Responsibility

- Own the production hierarchy `CrystalPresentationRoot/RealMesh3DRoot/RealCrystalMesh`.
- Render the crystal through a stable perspective `CrystalStage3D_CrystalCamera`.
- Keep the kaleidoscope texture off the crystal albedo.
- Composite the crystal over the kaleidoscope output as a second layer.
- Use the kaleidoscope texture as an environment layer only in explicit reflection/refraction debug views.
- Keep dedicated stage lights isolated from MirrorModule, SourceModule, and Billboard2D.
- Composite the stage render texture downstream over the final kaleidoscope output.

## Forbidden

- Do not use Plane/Quad/Billboard/Sprite/RawImage as the RealMesh3D crystal.
- Do not paint `FinalKaleidoscopeTexture` directly onto the crystal body.
- Do not rotate the production camera for normal crystal rotation.
- Do not call MirrorModule or SourceModule internals.
- Do not let Billboard2D know this stage exists.
- Do not use `Camera.main` or scene-object-name lookup as production logic.

## Debug Modes

- Solid Lit Geometry: blue lit material, no kaleidoscope texture.
- Transparent Glass Only: transparent lit glass, no kaleidoscope texture.
- Reflection Environment Only: reflective glass with stage environment visible.
- Refraction Environment Only: transparent glass in front of background.
- Final Premium Composite: full stage composite path.

## Validation

RealMesh3D is valid only if:

- the crystal uses `MeshFilter + MeshRenderer`;
- the mesh has non-zero depth;
- side faces are present;
- the silhouette changes when the crystal rotates or the Scene View orbits around it;
- disabling kaleidoscope sampling on the material still leaves a visible 3D object;
- disabling optical effects still leaves real volumetric geometry.
