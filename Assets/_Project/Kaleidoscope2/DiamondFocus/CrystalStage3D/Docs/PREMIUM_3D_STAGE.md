# Premium 3D Crystal Stage

Premium3D means a separate `CrystalStage3D` spatial scene that renders to its own RenderTexture.

Runtime composition:

```text
CrystalCamera
    -> RealMesh3D Crystal
    -> BackgroundGeometry
    -> CrystalStage3D RenderTexture
```

This mode is not the legacy DiamondFocus compositor, not Billboard2D, not a fullscreen projection, and not screen-space fake depth. The legacy DiamondFocus hybrid path remains available as the Classic2D/Billboard compatibility path.

Layer 1 remains read-only: Source, Mirror, OutputPreview, RuntimeMenuController, Billboard2D, and Menu do not control the physical CrystalStage3D objects.
