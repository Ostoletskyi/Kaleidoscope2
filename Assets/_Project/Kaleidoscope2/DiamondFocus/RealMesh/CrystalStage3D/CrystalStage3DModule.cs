using Kaleidoscope2.Core;
using Kaleidoscope2.DiamondFocus;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D
{
    public sealed class CrystalStage3DModule
    {
        private readonly CrystalStage3DSettings settings = new CrystalStage3DSettings();
        private readonly CrystalStage3DCameraRig cameraRig = new CrystalStage3DCameraRig();
        private readonly CrystalStage3DRenderer renderer = new CrystalStage3DRenderer();
        private readonly CrystalStage3DComposite composite = new CrystalStage3DComposite();
        private readonly CrystalStage3DLightRig lightRig = new CrystalStage3DLightRig();
        private readonly CrystalStage3DEnvironmentBinder environmentBinder = new CrystalStage3DEnvironmentBinder();

        private Transform owner;
        private GameObject root;
        private GameObject realMeshRoot;
        private int layer = 31;
        private bool visible;
        private string diagnosticsLabel = "CrystalStage3D not initialized";
        private float lastScreenCoverageEstimate;
        private float lastCameraCrystalDistance;
        private float lastCrystalBackgroundDistance;
        private Vector3 lastStageCameraLocalPosition;
        private Vector3 lastCrystalWorldPosition;
        private Vector3 lastCrystalLocalScale;
        private Vector3 lastBackgroundLocalPosition;
        private bool lastCrystalBetweenCameraAndBackground;
        private bool lastFullscreenCompositeDominates;

        public Texture OutputTexture
        {
            get { return composite.OutputTexture; }
        }

        public int ActiveLightCount
        {
            get { return lightRig.ActiveLightCount; }
        }

        public int MeshVertexCount
        {
            get { return renderer.MeshVertexCount; }
        }

        public int MeshTriangleCount
        {
            get { return renderer.MeshTriangleCount; }
        }

        public Vector3 MeshBoundsSize
        {
            get { return renderer.MeshBoundsSize; }
        }

        public bool HasVolume
        {
            get { return renderer.HasVolume; }
        }

        public bool SideFacesDetected
        {
            get { return renderer.SideFacesDetected; }
        }

        public string ActiveCrystalGameObjectName
        {
            get { return renderer.ActiveGameObjectName; }
        }

        public int RuntimeObjectCount
        {
            get
            {
                return (root != null ? 1 : 0)
                    + (realMeshRoot != null ? 1 : 0)
                    + cameraRig.RuntimeObjectCount
                    + renderer.RuntimeObjectCount
                    + composite.RuntimeObjectCount
                    + lightRig.RuntimeObjectCount
                    + environmentBinder.RuntimeObjectCount;
            }
        }

        public string DiagnosticsLabel
        {
            get
            {
                Vector3 bounds = MeshBoundsSize;
                Vector3 position = realMeshRoot != null ? realMeshRoot.transform.localPosition : Vector3.zero;
                return diagnosticsLabel
                    + ", debug " + settings.DebugModeLabel
                    + ", active crystal " + ActiveCrystalGameObjectName
                    + ", RealMesh3DRoot active " + (realMeshRoot != null && realMeshRoot.activeInHierarchy ? "yes" : "no")
                    + ", CrystalStage3D_Environment active " + (environmentBinder.IsActive ? "yes" : "no")
                    + ", scale " + settings.CrystalScale.ToString("0.00")
                    + ", centered " + (settings.RealMeshScreenCenter ? "true" : "false")
                    + ", target frame height " + (settings.TargetFrameHeight * 100f).ToString("0") + "%"
                    + ", position " + position.x.ToString("0.00") + "," + position.y.ToString("0.00") + "," + position.z.ToString("0.00")
                    + ", vertices " + MeshVertexCount.ToString()
                    + ", triangles " + MeshTriangleCount.ToString()
                    + ", bounds " + bounds.x.ToString("0.00") + "x" + bounds.y.ToString("0.00") + "x" + bounds.z.ToString("0.00")
                    + ", measured screen coverage " + CrystalSpatialDiagnostics.FormatPercent(lastScreenCoverageEstimate)
                    + ", stage camera position " + CrystalSpatialDiagnostics.FormatVector(lastStageCameraLocalPosition)
                    + ", crystal world position " + CrystalSpatialDiagnostics.FormatVector(lastCrystalWorldPosition)
                    + ", crystal world scale " + CrystalSpatialDiagnostics.FormatVector(lastCrystalLocalScale)
                    + ", camera-crystal distance " + lastCameraCrystalDistance.ToString("0.00")
                    + ", background plane position " + CrystalSpatialDiagnostics.FormatVector(lastBackgroundLocalPosition)
                    + ", crystal-background distance " + lastCrystalBackgroundDistance.ToString("0.00")
                    + ", crystal between camera/background " + (lastCrystalBetweenCameraAndBackground ? "true" : "false")
                    + ", fullscreen composite dominates " + (lastFullscreenCompositeDominates ? "true" : "false")
                    + ", hasVolume " + (HasVolume ? "true" : "false")
                    + ", sideFacesDetected " + (SideFacesDetected ? "true" : "false")
                    + ", camera orbit " + (settings.CameraOrbitEnabled ? "enabled" : "disabled")
                    + ", productionCameraOrbitEnabled " + (settings.ProductionCameraOrbitEnabled ? "true" : "false")
                    + ", " + cameraRig.DiagnosticsLabel
                    + ", crystal rotation " + (settings.CrystalRotationEnabled ? "enabled" : "disabled")
                    + ", lights " + ActiveLightCount.ToString() + "/" + settings.ActiveLightCount.ToString()
                    + ", objects " + RuntimeObjectCount.ToString();
            }
        }

        public void Initialize(Transform ownerTransform, int crystalLayer)
        {
            owner = ownerTransform;
            layer = Mathf.Clamp(crystalLayer, 0, 31);
            EnsureRoot();
            SetVisible(visible);
        }

        public void SetDebugMode(CrystalStage3DDebugMode debugMode)
        {
            settings.DebugMode = debugMode;
        }

        public bool Render(
            RenderTexture sourceTexture,
            CrystalSharedSettings sharedSettings,
            CrystalShape shape,
            CrystalMaterialMode materialMode,
            Vector3 rotation,
            float intensity,
            bool shouldBeVisible)
        {
            visible = shouldBeVisible;
            if (sourceTexture == null || !shouldBeVisible)
            {
                SetVisible(false);
                diagnosticsLabel = sourceTexture == null
                    ? "CrystalStage3D missing source texture"
                    : "CrystalStage3D hidden";
                return false;
            }

            EnsureRoot();
            if (root == null)
            {
                diagnosticsLabel = "CrystalStage3D root missing";
                return false;
            }

            settings.SyncFromShared(sharedSettings);
            settings.LightIntensity = intensity;
            SetVisible(true);
            cameraRig.Ensure(root.transform, layer);
            cameraRig.PrepareTexture(sourceTexture.width, sourceTexture.height);
            if (settings.ShowsBackground)
            {
                environmentBinder.Ensure(root.transform, layer);
                environmentBinder.Bind(sourceTexture, settings, true);
            }
            else
            {
                environmentBinder.SetVisible(false);
            }

            EnsureRealMeshRoot();
            ApplyRealMeshPlacement();
            renderer.Ensure(realMeshRoot.transform, layer);
            bool renderedGeometry = renderer.Render(settings, sharedSettings, shape, materialMode, rotation, intensity, true);

            if (!renderedGeometry)
            {
                diagnosticsLabel = "CrystalStage3D geometry validation failed";
                return false;
            }

            settings.SetCrystalWorldBounds(MeshBoundsSize);
            lightRig.Ensure(root.transform, layer);
            lightRig.Tick(
                Time.deltaTime,
                settings.LightIntensity,
                ResolveRotation01(rotation),
                settings.CrystalScale,
                settings.ActiveLightCount,
                settings.LightRigEnabled);

            bool renderedStage = cameraRig.Render(settings, Time.deltaTime);
            if (!renderedStage)
            {
                diagnosticsLabel = "CrystalStage3D camera render failed";
                return false;
            }

            MeasureSpatialState(settings);
            bool composited = composite.Composite(sourceTexture, cameraRig.StageTexture);
            diagnosticsLabel = composited
                ? "CrystalStage3D active independent 3D stage"
                : "CrystalStage3D composite failed";
            lastFullscreenCompositeDominates = composited;
            return composited;
        }

        public void SetVisible(bool value)
        {
            visible = value;
            if (root != null && root.activeSelf != value)
            {
                root.SetActive(value);
            }

            cameraRig.SetVisible(value);
            renderer.SetVisible(value);
            environmentBinder.SetVisible(value && settings.ShowsBackground);
            lightRig.SetVisible(value);
        }

        public void Shutdown()
        {
            visible = false;
            cameraRig.Shutdown();
            renderer.Shutdown();
            composite.Shutdown();
            lightRig.Shutdown();
            environmentBinder.Shutdown();
            DestroyRuntimeObject(root);
            root = null;
            realMeshRoot = null;
            owner = null;
            diagnosticsLabel = "CrystalStage3D not initialized";
        }

        private void EnsureRoot()
        {
            if (root != null)
            {
                root.layer = layer;
                return;
            }

            root = new GameObject("CrystalPresentationRoot")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            if (owner != null)
            {
                root.transform.SetParent(owner, false);
            }

            root.layer = layer;
            EnsureRealMeshRoot();
        }

        private void EnsureRealMeshRoot()
        {
            if (realMeshRoot != null)
            {
                realMeshRoot.layer = layer;
                return;
            }

            if (root == null)
            {
                return;
            }

            realMeshRoot = new GameObject("RealMesh3DRoot")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            realMeshRoot.transform.SetParent(root.transform, false);
            realMeshRoot.layer = layer;
        }

        private void ApplyRealMeshPlacement()
        {
            if (realMeshRoot == null)
            {
                return;
            }

            Vector2 offset = settings.CrystalScreenCenterOffset;
            realMeshRoot.transform.localPosition = settings.RealMeshScreenCenter
                ? Vector3.zero
                : new Vector3(offset.x, offset.y, 0f);
            realMeshRoot.transform.localRotation = Quaternion.identity;
            realMeshRoot.transform.localScale = Vector3.one;
        }

        private void MeasureSpatialState(CrystalStage3DSettings stageSettings)
        {
            if (stageSettings == null)
            {
                return;
            }

            Bounds worldBounds = renderer.CrystalWorldBounds;
            lastScreenCoverageEstimate = cameraRig.MeasureViewportHeightCoverage(worldBounds);
            if (lastScreenCoverageEstimate <= 0.0001f)
            {
                lastScreenCoverageEstimate = CrystalSpatialDiagnostics.PerspectiveHeightCoverage(
                    worldBounds.size.y,
                    Mathf.Max(0.0001f, cameraRig.MeasureDistanceTo(renderer.CrystalWorldPosition)),
                    cameraRig.LastFieldOfView);
            }

            lastStageCameraLocalPosition = cameraRig.LastLocalPosition;
            lastCrystalWorldPosition = renderer.CrystalWorldPosition;
            lastCrystalLocalScale = renderer.CrystalLocalScale;
            lastCameraCrystalDistance = cameraRig.MeasureDistanceTo(renderer.CrystalWorldPosition);
            lastBackgroundLocalPosition = environmentBinder.IsActive
                ? environmentBinder.LastBackgroundLocalPosition
                : new Vector3(0f, 0f, stageSettings.BackgroundDistance);
            lastCrystalBackgroundDistance = Mathf.Abs(lastBackgroundLocalPosition.z - renderer.CrystalLocalPosition.z);
            lastCrystalBetweenCameraAndBackground = environmentBinder.IsActive
                && lastStageCameraLocalPosition.z < renderer.CrystalLocalPosition.z
                && renderer.CrystalLocalPosition.z < lastBackgroundLocalPosition.z;
        }

        private static float ResolveRotation01(Vector3 rotation)
        {
            return Mathf.Clamp01(rotation.magnitude / 720f);
        }

        private static void DestroyRuntimeObject(Object instance)
        {
            if (instance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(instance);
            }
            else
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
