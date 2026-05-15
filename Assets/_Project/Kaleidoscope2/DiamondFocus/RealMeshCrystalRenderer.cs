using Kaleidoscope2.Core;
using Kaleidoscope2.DiamondFocus.RealMesh;
using Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public sealed class RealMeshCrystalRenderer : ICrystalRenderer
    {
        private const bool EnableStage04LightRig = false;

        private readonly Transform owner;
        private readonly Shader crystalShader;
        private readonly int layer;
        private readonly RealCrystalMeshController meshController = new RealCrystalMeshController();
        private readonly RealCrystalMaterialBinder materialBinder;
        private readonly RealMesh.CrystalLightRigModule lightRig = new RealMesh.CrystalLightRigModule();
        private readonly CrystalStage3DModule stage3D = new CrystalStage3DModule();

        private CrystalSharedSettings settings;
        private RenderTexture sourceTexture;
        private RenderTexture outputTexture;
        private GameObject root;
        private GameObject cameraObject;
        private Camera renderCamera;
        private CrystalShape shape;
        private CrystalMaterialMode materialMode;
        private Vector3 rotation;
        private float intensity;
        private bool visible;
        private bool geometryValidationMaterialEnabled;
        private CrystalStage3DDebugMode stageDebugMode = CrystalStage3DDebugMode.FinalPremiumComposite;

        public RealMeshCrystalRenderer(Transform ownerTransform, Shader shader, int crystalLayer)
        {
            owner = ownerTransform;
            crystalShader = shader;
            layer = Mathf.Clamp(crystalLayer, 0, 31);
            materialBinder = new RealCrystalMaterialBinder(crystalShader);
        }

        public Texture OutputTexture
        {
            get
            {
                Texture stageOutput = stage3D.OutputTexture;
                if (stageOutput != null)
                {
                    return stageOutput;
                }

                return outputTexture != null ? outputTexture : sourceTexture;
            }
        }

        public int ActiveLightCount
        {
            get
            {
                int stageLights = stage3D.ActiveLightCount;
                return stageLights > 0 ? stageLights : lightRig.ActiveLightCount;
            }
        }

        public string RendererType
        {
            get { return "MeshFilter + MeshRenderer"; }
        }

        public int MeshVertexCount
        {
            get { return stage3D.MeshVertexCount > 0 ? stage3D.MeshVertexCount : meshController.MeshVertexCount; }
        }

        public int MeshTriangleCount
        {
            get { return stage3D.MeshTriangleCount > 0 ? stage3D.MeshTriangleCount : meshController.MeshTriangleCount; }
        }

        public bool HasThickness
        {
            get { return stage3D.MeshVertexCount > 0 ? stage3D.MeshBoundsSize.z > RealCrystalVolumetricMeshFactory.MinimumValidDepth : meshController.HasThickness; }
        }

        public bool SideFacesDetected
        {
            get { return stage3D.MeshVertexCount > 0 ? stage3D.SideFacesDetected : meshController.SideFacesDetected; }
        }

        public bool HasVolume
        {
            get { return stage3D.MeshVertexCount > 0 ? stage3D.HasVolume : meshController.HasVolume; }
        }

        public string MeshDiagnosticsLabel
        {
            get
            {
                return stage3D.MeshVertexCount > 0
                    ? stage3D.DiagnosticsLabel
                    : meshController.MeshDiagnosticsLabel;
            }
        }

        public bool IsVisible
        {
            get { return visible; }
        }

        public int RuntimeObjectCount
        {
            get
            {
                int count = 0;
                if (root != null)
                {
                    count++;
                }

                if (cameraObject != null)
                {
                    count++;
                }

                if (outputTexture != null)
                {
                    count++;
                }

                count += meshController.RuntimeObjectCount;
                count += lightRig.RuntimeObjectCount;
                count += stage3D.RuntimeObjectCount;
                return count;
            }
        }

        public string LifecycleState
        {
            get
            {
                return (visible ? "visible" : "inactive")
                    + ", runtime objects " + RuntimeObjectCount.ToString()
                    + ", renderer " + RendererType
                    + ", fallback root " + ResolveFallbackRootState()
                    + ", geometry debug " + (geometryValidationMaterialEnabled ? "on" : "off")
                    + ", " + MeshDiagnosticsLabel
                    + ", " + materialBinder.DiagnosticsLabel
                    + ", lights " + ActiveLightCount.ToString();
            }
        }

        public string StageDiagnosticsLabel
        {
            get { return stage3D.DiagnosticsLabel; }
        }

        public void Initialize(CrystalSharedSettings sharedSettings)
        {
            settings = sharedSettings;
            stage3D.Initialize(owner, layer);
            ApplyRuntimeVisibility(visible);
        }

        public void PrepareOutput(int width, int height, bool linear)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            if (outputTexture != null && outputTexture.width == width && outputTexture.height == height)
            {
                return;
            }

            ReleaseOutput();
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 24)
            {
                msaaSamples = 1,
                sRGB = linear,
                useMipMap = false,
                autoGenerateMips = false
            };

            outputTexture = new RenderTexture(descriptor)
            {
                name = "Kaleidoscope2_CrystalPresentation_RealMeshOutput",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            outputTexture.Create();
        }

        public void SetSourceTexture(RenderTexture texture)
        {
            sourceTexture = texture;
        }

        public void SetShape(CrystalShape value)
        {
            shape = value;
        }

        public void SetMaterialMode(CrystalMaterialMode value)
        {
            materialMode = value;
        }

        public void SetRotation(Vector3 value)
        {
            rotation = value;
        }

        public void SetIntensity(float value)
        {
            intensity = Mathf.Clamp(value, 0f, 20f);
        }

        public void SetVisible(bool value)
        {
            visible = value;
            ApplyRuntimeVisibility(value);
        }

        public void SetGeometryValidationMaterialEnabled(bool value)
        {
            geometryValidationMaterialEnabled = value;
        }

        public void SetStageDebugMode(CrystalStage3DDebugMode value)
        {
            stageDebugMode = value;
        }

        public void Render()
        {
            if (sourceTexture == null)
            {
                ApplyRuntimeVisibility(false);
                ReleaseOutput();
                return;
            }

            if (!visible)
            {
                ApplyRuntimeVisibility(false);
                return;
            }

            stage3D.SetDebugMode(geometryValidationMaterialEnabled
                ? CrystalStage3DDebugMode.SolidLitGeometry
                : stageDebugMode);
            if (stage3D.Render(sourceTexture, settings, shape, materialMode, rotation, intensity, visible))
            {
                ReleaseOutput();
                ApplyFallbackRuntimeVisibility(false);
                return;
            }

            stage3D.SetVisible(false);
            ApplyFallbackRuntimeVisibility(false);
            ReleaseOutput();
        }

        public void Shutdown()
        {
            visible = false;
            stage3D.Shutdown();
            ReleaseOutput();
            meshController.Shutdown();
            lightRig.Shutdown();
            materialBinder.Shutdown();
            DestroyRuntimeObject(root);
            root = null;
            cameraObject = null;
            renderCamera = null;
            sourceTexture = null;
            settings = null;
        }

        private void ApplyRuntimeVisibility(bool value)
        {
            stage3D.SetVisible(value);
            ApplyFallbackRuntimeVisibility(false);
        }

        private void ApplyFallbackRuntimeVisibility(bool value)
        {
            if (root != null && root.activeSelf != value)
            {
                root.SetActive(value);
            }

            if (cameraObject != null && cameraObject.activeSelf != value)
            {
                cameraObject.SetActive(value);
            }

            meshController.SetVisible(value);
            lightRig.SetEnabled(value);

            if (renderCamera != null)
            {
                renderCamera.enabled = false;
            }
        }

        private void EnsureRuntimeObjects()
        {
            if (root == null)
            {
                root = new GameObject("Kaleidoscope2_CrystalPresentation_RealMeshRuntime")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                if (owner != null)
                {
                    root.transform.SetParent(owner, false);
                }
            }

            if (cameraObject == null)
            {
                cameraObject = new GameObject("RealMeshCrystalCamera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                cameraObject.transform.SetParent(root.transform, false);
                renderCamera = cameraObject.AddComponent<Camera>();
            }

            if (renderCamera == null)
            {
                renderCamera = cameraObject.GetComponent<Camera>();
                if (renderCamera == null)
                {
                    renderCamera = cameraObject.AddComponent<Camera>();
                }
            }

            root.layer = layer;
            cameraObject.layer = layer;
            renderCamera.enabled = false;
            renderCamera.clearFlags = CameraClearFlags.Depth;
            renderCamera.orthographic = true;
            renderCamera.orthographicSize = 1.25f;
            renderCamera.nearClipPlane = 0.01f;
            renderCamera.farClipPlane = 8f;
            renderCamera.allowHDR = false;
            renderCamera.allowMSAA = false;
            renderCamera.cullingMask = 1 << layer;
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -3.2f);
            cameraObject.transform.localRotation = Quaternion.identity;
        }

        private string ResolveFallbackRootState()
        {
            if (root == null)
            {
                return "not created";
            }

            return root.activeSelf ? "active" : "inactive";
        }

        private float ResolveRotation01()
        {
            float magnitude = rotation.magnitude;
            return Mathf.Clamp01(magnitude / 720f);
        }

        private void ReleaseOutput()
        {
            if (outputTexture == null)
            {
                return;
            }

            if (outputTexture.IsCreated())
            {
                outputTexture.Release();
            }

            DestroyRuntimeObject(outputTexture);
            outputTexture = null;
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
