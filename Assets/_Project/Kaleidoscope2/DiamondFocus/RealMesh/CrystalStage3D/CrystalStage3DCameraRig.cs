using Kaleidoscope2.DiamondFocus;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D
{
    public sealed class CrystalStage3DCameraRig
    {
        private GameObject cameraObject;
        private Camera camera;
        private RenderTexture stageTexture;
        private float orbitPhase;
        private string diagnosticsLabel = "camera rig not created";
        private Vector3 lastLocalPosition;
        private float lastCameraDistance;
        private float lastFieldOfView;

        public RenderTexture StageTexture
        {
            get { return stageTexture; }
        }

        public int RuntimeObjectCount
        {
            get { return (cameraObject != null ? 1 : 0) + (stageTexture != null ? 1 : 0); }
        }

        public string DiagnosticsLabel
        {
            get { return diagnosticsLabel; }
        }

        public bool IsActive
        {
            get { return cameraObject != null && cameraObject.activeInHierarchy; }
        }

        public Vector3 LastLocalPosition
        {
            get { return lastLocalPosition; }
        }

        public float LastCameraDistance
        {
            get { return lastCameraDistance; }
        }

        public float LastFieldOfView
        {
            get { return lastFieldOfView; }
        }

        public void Ensure(Transform parent, int layer)
        {
            if (cameraObject == null)
            {
                cameraObject = new GameObject("CrystalStage3D_CrystalCamera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                cameraObject.transform.SetParent(parent, false);
                camera = cameraObject.AddComponent<Camera>();
            }

            if (camera == null)
            {
                camera = cameraObject.GetComponent<Camera>();
                if (camera == null)
                {
                    camera = cameraObject.AddComponent<Camera>();
                }
            }

            int safeLayer = Mathf.Clamp(layer, 0, 31);
            cameraObject.layer = safeLayer;
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.orthographic = false;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 20f;
            camera.allowHDR = true;
            camera.allowMSAA = false;
            camera.cullingMask = 1 << safeLayer;
        }

        public void PrepareTexture(int width, int height)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            if (stageTexture != null && stageTexture.width == width && stageTexture.height == height)
            {
                return;
            }

            ReleaseTexture();
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 24)
            {
                msaaSamples = 1,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
                useMipMap = false,
                autoGenerateMips = false
            };

            stageTexture = new RenderTexture(descriptor)
            {
                name = "Kaleidoscope2_CrystalStage3D_RenderTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            stageTexture.Create();
        }

        public bool Render(CrystalStage3DSettings settings, float deltaTime)
        {
            if (camera == null || stageTexture == null || settings == null)
            {
                diagnosticsLabel = "camera rig missing camera or render texture";
                return false;
            }

            if (settings.CameraOrbitEnabled)
            {
                orbitPhase += Mathf.Max(0f, deltaTime) * settings.CameraOrbitSpeed;
            }
            else
            {
                orbitPhase = 0f;
            }

            float radius = settings.CameraOrbitRadius;
            Vector2 offset = settings.CrystalScreenCenterOffset;
            Vector3 target = settings.RealMeshScreenCenter ? Vector3.zero : new Vector3(offset.x, offset.y, 0f);
            Vector3 position = new Vector3(
                Mathf.Sin(orbitPhase) * radius,
                settings.CameraHeight,
                -settings.CameraDistance + (Mathf.Cos(orbitPhase) - 1f) * radius * 0.35f);

            cameraObject.transform.localPosition = position;
            LookAtLocal(target);
            camera.fieldOfView = settings.CameraFieldOfView;
            camera.farClipPlane = Mathf.Max(8f, settings.CameraDistance + settings.BackgroundDistance + 4f);
            lastLocalPosition = position;
            lastCameraDistance = settings.CameraDistance;
            lastFieldOfView = camera.fieldOfView;

            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            camera.targetTexture = stageTexture;
            camera.Render();
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            diagnosticsLabel = "stable perspective camera rendered stage, camera orbit "
                + (settings.CameraOrbitEnabled ? "enabled" : "disabled")
                + ", productionCameraOrbitEnabled " + (settings.ProductionCameraOrbitEnabled ? "true" : "false")
                + ", position " + position.x.ToString("0.00") + "," + position.y.ToString("0.00") + "," + position.z.ToString("0.00")
                + ", fov " + camera.fieldOfView.ToString("0.00")
                + ", distance " + settings.CameraDistance.ToString("0.00")
                + ", base distance " + settings.BaseCameraDistance.ToString("0.00")
                + ", target frame " + (settings.TargetFrameHeight * 100f).ToString("0") + "%";
            return true;
        }

        public float MeasureViewportHeightCoverage(Bounds worldBounds)
        {
            return CrystalSpatialDiagnostics.ViewportHeightCoverage(camera, worldBounds);
        }

        public float MeasureDistanceTo(Vector3 worldPosition)
        {
            return cameraObject != null
                ? Vector3.Distance(cameraObject.transform.position, worldPosition)
                : 0f;
        }

        public void SetVisible(bool visible)
        {
            if (cameraObject != null && cameraObject.activeSelf != visible)
            {
                cameraObject.SetActive(visible);
            }

            if (camera != null)
            {
                camera.enabled = false;
            }
        }

        public void Shutdown()
        {
            ReleaseTexture();
            DestroyRuntimeObject(cameraObject);
            cameraObject = null;
            camera = null;
            orbitPhase = 0f;
            diagnosticsLabel = "camera rig not created";
        }

        private void LookAtLocal(Vector3 target)
        {
            Vector3 worldTarget = cameraObject.transform.parent != null
                ? cameraObject.transform.parent.TransformPoint(target)
                : target;
            Vector3 direction = worldTarget - cameraObject.transform.position;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                cameraObject.transform.localRotation = Quaternion.identity;
                return;
            }

            cameraObject.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void ReleaseTexture()
        {
            if (stageTexture == null)
            {
                return;
            }

            if (stageTexture.IsCreated())
            {
                stageTexture.Release();
            }

            DestroyRuntimeObject(stageTexture);
            stageTexture = null;
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
