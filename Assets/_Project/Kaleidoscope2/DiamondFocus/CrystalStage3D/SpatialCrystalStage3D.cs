using Kaleidoscope2.Core;
using Kaleidoscope2.DiamondFocus.RealMesh;
using Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kaleidoscope2.DiamondFocus.CrystalStage3D
{
    public sealed class SpatialCrystalStage3D
    {
        private const string SpatialStageLayerName = "SpatialStage3D";
        private const string StageLayerSemanticName = "CrystalStage3D";
        private const string StageModeLabel = "Premium3D CrystalStage3D spatial baseline";
        private const float CameraDistance = 10f;
        private const float BackgroundDistance = 3.5f;
        private const float CameraFieldOfView = 35f;
        private const float TargetCrystalScreenCoverage = CrystalSpatialDiagnostics.RealMeshTargetCoverage;
        private const float DioramaCrystalScale = 0.72f;
        private const float DioramaBackgroundHeight = 2.75f;
        private const float MinimumStageViewScale = 0.2f;
        private const float MaximumStageViewScale = 8f;
        private const float MinimumMeshDimension = 0.001f;

        private Transform owner;
        private GameObject root;
        private GameObject dioramaObject;
        private GameObject cameraObject;
        private GameObject lightRigObject;
        private GameObject crystalObject;
        private GameObject backgroundObject;
        private GameObject diagnosticsObject;
        private Camera stageCamera;
        private Light keyLight;
        private Light rimLight;
        private Light fillLight;
        private MeshFilter crystalMeshFilter;
        private MeshRenderer crystalMeshRenderer;
        private MeshFilter backgroundMeshFilter;
        private MeshRenderer backgroundMeshRenderer;
        private Mesh crystalMesh;
        private Mesh backgroundMesh;
        private Material transparentCrystalMaterial;
        private Material solidCrystalMaterial;
        private Material backgroundMaterial;
        private RenderTexture outputTexture;
        private Camera[] cameraCache = new Camera[8];
        private CrystalShape activeShape = (CrystalShape)(-1);
        private int layer;
        private int stageLayerMask;
        private int lastActiveLightCount;
        private int protectedCameraCount;
        private string protectedCameraNames = "none";
        private int directViewCameraCount;
        private string directViewCameraNames = "none";
        private string activeCameraDiagnostics = "none";
        private string crystalCameraTargetTextureStatus = "none";
        private float validationOrbitPhase;
        private float stageViewScale = 1f;
        private bool stageRenderedThisFrame;
        private bool physicalStageUsesStageOutputTexture;
        private string diagnosticsLabel = StageModeLabel + ": not initialized";

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        public int ActiveLightCount
        {
            get
            {
                return lastActiveLightCount;
            }
        }

        public int MeshVertexCount
        {
            get { return crystalMesh != null ? crystalMesh.vertexCount : 0; }
        }

        public int MeshTriangleCount
        {
            get { return crystalMesh != null && crystalMesh.triangles != null ? crystalMesh.triangles.Length / 3 : 0; }
        }

        public Vector3 MeshBoundsSize
        {
            get { return crystalMesh != null ? crystalMesh.bounds.size : Vector3.zero; }
        }

        public bool HasVolume
        {
            get { return RealCrystalVolumetricMeshFactory.HasVolume(crystalMesh); }
        }

        public bool SideFacesDetected
        {
            get { return RealCrystalShapeLibrary.HasSideFaces(crystalMesh); }
        }

        public string DiagnosticsLabel
        {
            get { return diagnosticsLabel; }
        }

        public int RuntimeObjectCount
        {
            get
            {
                int count = 0;
                count += root != null ? 1 : 0;
                count += dioramaObject != null ? 1 : 0;
                count += cameraObject != null ? 1 : 0;
                count += lightRigObject != null ? 1 : 0;
                count += crystalObject != null ? 1 : 0;
                count += backgroundObject != null ? 1 : 0;
                count += diagnosticsObject != null ? 1 : 0;
                count += outputTexture != null ? 1 : 0;
                count += crystalMesh != null ? 1 : 0;
                count += backgroundMesh != null ? 1 : 0;
                count += transparentCrystalMaterial != null ? 1 : 0;
                count += solidCrystalMaterial != null ? 1 : 0;
                count += backgroundMaterial != null ? 1 : 0;
                return count;
            }
        }

        public void Initialize(Transform ownerTransform, int crystalLayer)
        {
            owner = ownerTransform;
            layer = ResolveSpatialStageLayer(crystalLayer);
            stageLayerMask = 1 << layer;
            EnsureRuntimeObjects();
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (root != null && root.activeSelf != visible)
            {
                root.SetActive(visible);
            }

            if (stageCamera != null)
            {
                stageCamera.enabled = false;
            }
        }

        public bool Render(
            RenderTexture sourceTexture,
            CrystalSharedSettings settings,
            CrystalShape shape,
            CrystalMaterialMode materialMode,
            Vector3 rotation,
            float intensity,
            bool visible,
            bool solidGeometryValidation,
            CrystalStage3DDebugMode debugMode)
        {
            if (!visible || sourceTexture == null)
            {
                stageRenderedThisFrame = false;
                lastActiveLightCount = 0;
                SetVisible(false);
                physicalStageUsesStageOutputTexture = false;
                crystalCameraTargetTextureStatus = "none";
                UpdateCameraDiagnostics();
                UpdateDiagnostics(null, null, 0f, 0f, 0f, 0f, false, debugMode, false);
                return false;
            }

            EnsureRuntimeObjects();
            ConfigureDirectViewCameras();
            EnsureOutputTexture(sourceTexture.width, sourceTexture.height);
            EnsureCrystalMesh(shape);
            ConfigureStage(sourceTexture, settings, materialMode, rotation, intensity, solidGeometryValidation, debugMode);
            RenderStageCamera();

            Bounds crystalBounds = crystalMeshRenderer != null ? crystalMeshRenderer.bounds : new Bounds();
            Bounds backgroundBounds = backgroundMeshRenderer != null ? backgroundMeshRenderer.bounds : new Bounds();
            float screenCoverage = CrystalSpatialDiagnostics.ViewportHeightCoverage(stageCamera, crystalBounds);
            float backgroundCoverage = CrystalSpatialDiagnostics.ViewportHeightCoverage(stageCamera, backgroundBounds);
            float cameraCrystalDistance = stageCamera != null && crystalObject != null
                ? Vector3.Distance(stageCamera.transform.position, crystalObject.transform.position)
                : 0f;
            float crystalBackgroundDistance = crystalObject != null && backgroundObject != null
                ? Vector3.Distance(crystalObject.transform.position, backgroundObject.transform.position)
                : 0f;
            bool crystalBetweenCameraAndBackground = IsCrystalBetweenCameraAndBackground();
            bool validationOrbitActive = ResolveValidationOrbit(settings, debugMode);
            stageRenderedThisFrame = outputTexture != null;
            physicalStageUsesStageOutputTexture = PhysicalStageUsesOutputTexture();
            UpdateCameraDiagnostics();
            UpdateDiagnostics(
                crystalBounds,
                stageCamera,
                screenCoverage,
                backgroundCoverage,
                cameraCrystalDistance,
                crystalBackgroundDistance,
                crystalBetweenCameraAndBackground,
                debugMode,
                validationOrbitActive);
            return outputTexture != null;
        }

        public void Shutdown()
        {
            ReleaseOutputTexture();
            DestroyRuntimeObject(root);
            DestroyRuntimeObject(crystalMesh);
            DestroyRuntimeObject(backgroundMesh);
            DestroyRuntimeObject(transparentCrystalMaterial);
            DestroyRuntimeObject(solidCrystalMaterial);
            DestroyRuntimeObject(backgroundMaterial);

            root = null;
            dioramaObject = null;
            cameraObject = null;
            lightRigObject = null;
            crystalObject = null;
            backgroundObject = null;
            diagnosticsObject = null;
            stageCamera = null;
            keyLight = null;
            rimLight = null;
            fillLight = null;
            crystalMeshFilter = null;
            crystalMeshRenderer = null;
            backgroundMeshFilter = null;
            backgroundMeshRenderer = null;
            crystalMesh = null;
            backgroundMesh = null;
            transparentCrystalMaterial = null;
            solidCrystalMaterial = null;
            backgroundMaterial = null;
            lastActiveLightCount = 0;
            activeShape = (CrystalShape)(-1);
            stageRenderedThisFrame = false;
            physicalStageUsesStageOutputTexture = false;
            diagnosticsLabel = StageModeLabel + ": shutdown";
        }

        private void EnsureRuntimeObjects()
        {
            if (root == null)
            {
                root = new GameObject("CrystalStage3DRoot")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            root.transform.SetParent(owner, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            if (dioramaObject == null)
            {
                dioramaObject = new GameObject("StageDiorama")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            dioramaObject.transform.SetParent(root.transform, false);
            dioramaObject.transform.localPosition = Vector3.zero;
            dioramaObject.transform.localRotation = Quaternion.identity;

            if (cameraObject == null)
            {
                cameraObject = new GameObject("CrystalCamera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                cameraObject.transform.SetParent(root.transform, false);
                stageCamera = cameraObject.AddComponent<Camera>();
            }

            if (stageCamera == null)
            {
                stageCamera = cameraObject.GetComponent<Camera>();
                if (stageCamera == null)
                {
                    stageCamera = cameraObject.AddComponent<Camera>();
                }
            }

            if (lightRigObject == null)
            {
                lightRigObject = new GameObject("CrystalLightRig")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                keyLight = CreateLight("KeyLight", LightType.Directional, new Vector3(34f, -28f, 0f));
                rimLight = CreateLight("RimLight", LightType.Directional, new Vector3(-22f, 146f, 0f));
                fillLight = CreateLight("FillLight", LightType.Point, Vector3.zero);
            }
            lightRigObject.transform.SetParent(dioramaObject.transform, false);

            if (crystalObject == null)
            {
                crystalObject = new GameObject("RealMeshCrystal")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                crystalMeshFilter = crystalObject.AddComponent<MeshFilter>();
                crystalMeshRenderer = crystalObject.AddComponent<MeshRenderer>();
            }
            crystalObject.transform.SetParent(dioramaObject.transform, false);

            if (backgroundObject == null)
            {
                backgroundObject = new GameObject("BackgroundGeometry")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                backgroundMeshFilter = backgroundObject.AddComponent<MeshFilter>();
                backgroundMeshRenderer = backgroundObject.AddComponent<MeshRenderer>();
            }
            backgroundObject.transform.SetParent(dioramaObject.transform, false);

            if (diagnosticsObject == null)
            {
                diagnosticsObject = new GameObject("StageDiagnostics")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            diagnosticsObject.transform.SetParent(root.transform, false);
            diagnosticsObject.transform.localPosition = Vector3.zero;
            diagnosticsObject.transform.localRotation = Quaternion.identity;
            diagnosticsObject.transform.localScale = Vector3.one;

            cameraObject.transform.SetSiblingIndex(0);
            dioramaObject.transform.SetSiblingIndex(1);
            diagnosticsObject.transform.SetSiblingIndex(2);

            AssignLayerRecursive(root.transform);
            ConfigureCamera();
            ConfigureDirectViewCameras();
            EnsureBackgroundMesh();
        }

        private Light CreateLight(string name, LightType type, Vector3 eulerAngles)
        {
            GameObject lightObject = new GameObject(name)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            lightObject.transform.SetParent(lightRigObject.transform, false);
            lightObject.transform.localRotation = Quaternion.Euler(eulerAngles);
            Light light = lightObject.AddComponent<Light>();
            light.type = type;
            light.shadows = LightShadows.Soft;
            light.cullingMask = stageLayerMask;
            return light;
        }

        private void ConfigureCamera()
        {
            stageCamera.enabled = false;
            stageCamera.clearFlags = CameraClearFlags.SolidColor;
            stageCamera.backgroundColor = Color.black;
            stageCamera.orthographic = false;
            stageCamera.fieldOfView = CameraFieldOfView;
            stageCamera.nearClipPlane = 0.05f;
            stageCamera.farClipPlane = 80f;
            stageCamera.allowHDR = false;
            stageCamera.allowMSAA = false;
            stageCamera.cullingMask = stageLayerMask;
            stageCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }

        private void EnsureOutputTexture(int width, int height)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            if (outputTexture != null && outputTexture.width == width && outputTexture.height == height)
            {
                if (stageCamera != null)
                {
                    stageCamera.aspect = width / (float)height;
                }

                return;
            }

            ReleaseOutputTexture();
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 24)
            {
                msaaSamples = 1,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
                useMipMap = false,
                autoGenerateMips = false
            };
            outputTexture = new RenderTexture(descriptor)
            {
                name = "Kaleidoscope2_CrystalStage3D_SpatialOutput",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            outputTexture.Create();
            if (stageCamera != null)
            {
                stageCamera.aspect = width / (float)height;
            }
        }

        private void EnsureCrystalMesh(CrystalShape shape)
        {
            if (crystalMesh != null && activeShape == shape)
            {
                return;
            }

            DestroyRuntimeObject(crystalMesh);
            crystalMesh = RealCrystalShapeLibrary.CreateMesh(shape);
            activeShape = shape;
            if (crystalMeshFilter != null)
            {
                crystalMeshFilter.sharedMesh = crystalMesh;
            }
        }

        private void EnsureBackgroundMesh()
        {
            if (backgroundMesh == null)
            {
                backgroundMesh = new Mesh
                {
                    name = "Kaleidoscope2_CrystalStage3D_BackgroundPlane",
                    hideFlags = HideFlags.HideAndDontSave
                };
                backgroundMesh.vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, 0f),
                    new Vector3(0.5f, -0.5f, 0f),
                    new Vector3(0.5f, 0.5f, 0f),
                    new Vector3(-0.5f, 0.5f, 0f)
                };
                backgroundMesh.uv = new[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(1f, 1f),
                    new Vector2(0f, 1f)
                };
                backgroundMesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
                backgroundMesh.RecalculateBounds();
                backgroundMesh.RecalculateNormals();
            }

            if (backgroundMeshFilter != null)
            {
                backgroundMeshFilter.sharedMesh = backgroundMesh;
            }
        }

        private void ConfigureStage(
            RenderTexture sourceTexture,
            CrystalSharedSettings settings,
            CrystalMaterialMode materialMode,
            Vector3 rotation,
            float intensity,
            bool solidGeometryValidation,
            CrystalStage3DDebugMode debugMode)
        {
            SetVisible(true);
            ConfigureTransforms(sourceTexture, settings, rotation, debugMode);
            ConfigureLights(settings, intensity);
            ConfigureMaterials(
                sourceTexture,
                settings,
                materialMode,
                intensity,
                solidGeometryValidation || debugMode == CrystalStage3DDebugMode.SolidLitGeometry);
        }

        private void ConfigureTransforms(
            RenderTexture sourceTexture,
            CrystalSharedSettings settings,
            Vector3 rotation,
            CrystalStage3DDebugMode debugMode)
        {
            float aspect = sourceTexture.height > 0 ? sourceTexture.width / (float)sourceTexture.height : 1f;
            bool validationOrbit = ResolveValidationOrbit(settings, debugMode);
            if (validationOrbit)
            {
                validationOrbitPhase += Mathf.Max(0f, Time.deltaTime) * 0.45f;
            }
            else
            {
                validationOrbitPhase = 0f;
            }

            Vector3 cameraPosition = ResolveCameraPosition(validationOrbit);
            cameraObject.transform.localPosition = cameraPosition;
            cameraObject.transform.LookAt(root.transform.TransformPoint(Vector3.zero), Vector3.up);

            float viewHeightAtCrystal = ResolveViewHeight(CameraDistance);
            float targetCoverage = CrystalSpatialDiagnostics.ClampRealMeshCoverage(TargetCrystalScreenCoverage);
            float desiredCrystalWorldHeight = viewHeightAtCrystal * targetCoverage;
            float meshHeight = crystalMesh != null ? Mathf.Max(MinimumMeshDimension, crystalMesh.bounds.size.y) : 1f;
            stageViewScale = Mathf.Clamp(
                desiredCrystalWorldHeight / Mathf.Max(MinimumMeshDimension, meshHeight * DioramaCrystalScale),
                MinimumStageViewScale,
                MaximumStageViewScale);
            dioramaObject.transform.localScale = Vector3.one * stageViewScale;

            Vector2 centerOffset = settings != null ? settings.CrystalScreenCenterOffset : Vector2.zero;
            float viewWidthAtCrystal = viewHeightAtCrystal * aspect;
            crystalObject.transform.localPosition = new Vector3(
                centerOffset.x * viewWidthAtCrystal * 0.25f,
                centerOffset.y * viewHeightAtCrystal * 0.25f,
                0f);
            crystalObject.transform.localScale = Vector3.one * DioramaCrystalScale;
            crystalObject.transform.localRotation = Quaternion.Euler(rotation);

            backgroundObject.transform.localPosition = new Vector3(0f, 0f, BackgroundDistance);
            backgroundObject.transform.localRotation = Quaternion.identity;
            backgroundObject.transform.localScale = new Vector3(
                DioramaBackgroundHeight * aspect,
                DioramaBackgroundHeight,
                1f);
        }

        private Vector3 ResolveCameraPosition(bool validationOrbit)
        {
            if (!validationOrbit)
            {
                return new Vector3(0f, 0f, -CameraDistance);
            }

            float sideOffset = Mathf.Sin(validationOrbitPhase) * CameraDistance * 0.42f;
            float depth = -CameraDistance + Mathf.Abs(Mathf.Sin(validationOrbitPhase)) * CameraDistance * 0.18f;
            return new Vector3(sideOffset, 0.55f, depth);
        }

        private static float ResolveViewHeight(float distance)
        {
            float halfFovRadians = CameraFieldOfView * 0.5f * Mathf.Deg2Rad;
            return Mathf.Tan(halfFovRadians) * Mathf.Max(MinimumMeshDimension, distance) * 2f;
        }

        private void ConfigureLights(CrystalSharedSettings settings, float intensity)
        {
            bool enabled = settings == null || settings.CrystalLightRigEnabled;
            int activeCount = enabled && settings != null ? settings.RealMeshLightCount : 3;
            float resolvedIntensity = Mathf.Clamp(intensity, 0f, 20f);

            ConfigureLight(keyLight, enabled && activeCount >= 1, resolvedIntensity * 0.9f, new Vector3(-1.8f, 2.4f, -3.2f));
            ConfigureLight(rimLight, enabled && activeCount >= 2, resolvedIntensity * 0.58f, new Vector3(2.2f, 1.8f, -1.4f));
            ConfigureLight(fillLight, enabled && activeCount >= 3, resolvedIntensity * 0.38f, new Vector3(0f, -1.2f, -2f));
            lastActiveLightCount = CountActiveLights();
        }

        private static void ConfigureLight(Light light, bool active, float intensity, Vector3 localPosition)
        {
            if (light == null)
            {
                return;
            }

            light.gameObject.SetActive(active);
            light.intensity = Mathf.Max(0f, intensity);
            light.range = 18f;
            light.transform.localPosition = localPosition;
        }

        private void ConfigureDirectViewCameras()
        {
            int cameraCount = Camera.allCamerasCount;
            EnsureCameraCacheCapacity(cameraCount);

            int resolvedCount = Camera.GetAllCameras(cameraCache);
            protectedCameraCount = 0;
            protectedCameraNames = "none";
            directViewCameraCount = 0;
            directViewCameraNames = "none";
            for (int index = 0; index < resolvedCount; index++)
            {
                Camera camera = cameraCache[index];
                if (camera == null || ReferenceEquals(camera, stageCamera))
                {
                    continue;
                }

                if (IsDirectViewCamera(camera))
                {
                    if ((camera.cullingMask & stageLayerMask) == 0)
                    {
                        camera.cullingMask |= stageLayerMask;
                    }

                    directViewCameraNames = directViewCameraCount == 0
                        ? camera.name
                        : directViewCameraNames + "|" + camera.name;
                    directViewCameraCount++;
                    continue;
                }

                if ((camera.cullingMask & stageLayerMask) == 0)
                {
                    continue;
                }

                camera.cullingMask &= ~stageLayerMask;
                protectedCameraNames = protectedCameraCount == 0
                    ? camera.name
                    : protectedCameraNames + "|" + camera.name;
                protectedCameraCount++;
            }
        }

        private static bool IsDirectViewCamera(Camera camera)
        {
            if (camera == null)
            {
                return false;
            }

            string cameraName = camera.name;
            return camera.targetTexture == null
                && (cameraName.IndexOf("Viewer", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || cameraName.IndexOf("View", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || cameraName.IndexOf("Main", System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void EnsureCameraCacheCapacity(int cameraCount)
        {
            if (cameraCache == null || cameraCache.Length < cameraCount)
            {
                cameraCache = new Camera[Mathf.NextPowerOfTwo(Mathf.Max(1, cameraCount))];
            }
        }

        private void ConfigureMaterials(
            RenderTexture sourceTexture,
            CrystalSharedSettings settings,
            CrystalMaterialMode materialMode,
            float intensity,
            bool solidGeometryValidation)
        {
            if (backgroundMaterial == null)
            {
                Shader shader = Shader.Find("Unlit/Texture");
                backgroundMaterial = new Material(shader)
                {
                    name = "Kaleidoscope2_CrystalStage3D_BackgroundMaterial",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            backgroundMaterial.mainTexture = sourceTexture;
            backgroundMeshRenderer.sharedMaterial = backgroundMaterial;
            backgroundMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            backgroundMeshRenderer.receiveShadows = false;

            Material crystalMaterial = solidGeometryValidation
                ? EnsureSolidCrystalMaterial()
                : EnsureTransparentCrystalMaterial(settings, materialMode, intensity);
            crystalMeshRenderer.sharedMaterial = crystalMaterial;
            crystalMeshRenderer.shadowCastingMode = ShadowCastingMode.On;
            crystalMeshRenderer.receiveShadows = true;
        }

        private Material EnsureSolidCrystalMaterial()
        {
            if (solidCrystalMaterial == null)
            {
                Shader shader = Shader.Find("Standard");
                solidCrystalMaterial = new Material(shader)
                {
                    name = "Kaleidoscope2_CrystalStage3D_SolidGeometryMaterial",
                    hideFlags = HideFlags.HideAndDontSave
                };
                solidCrystalMaterial.color = new Color(0.2f, 0.82f, 1f, 1f);
            }

            ConfigureStandardOpaque(solidCrystalMaterial);
            return solidCrystalMaterial;
        }

        private Material EnsureTransparentCrystalMaterial(
            CrystalSharedSettings settings,
            CrystalMaterialMode materialMode,
            float intensity)
        {
            if (transparentCrystalMaterial == null)
            {
                Shader shader = Shader.Find("Standard");
                transparentCrystalMaterial = new Material(shader)
                {
                    name = "Kaleidoscope2_CrystalStage3D_TransparentCrystalMaterial",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            Color color = ResolveCrystalColor(materialMode);
            float alpha = settings != null ? settings.RealMeshAlpha : 0.58f;
            color.a = Mathf.Clamp(alpha + Mathf.Clamp01(intensity / 20f) * 0.12f, 0.22f, 0.82f);
            transparentCrystalMaterial.color = color;
            ConfigureStandardTransparent(transparentCrystalMaterial);
            return transparentCrystalMaterial;
        }

        private static Color ResolveCrystalColor(CrystalMaterialMode materialMode)
        {
            switch (materialMode)
            {
                case CrystalMaterialMode.Gemstone:
                    return new Color(0.22f, 0.95f, 0.76f, 0.62f);
                case CrystalMaterialMode.FuturisticPlastic:
                    return new Color(0.78f, 0.58f, 1f, 0.56f);
                case CrystalMaterialMode.Metal:
                    return new Color(0.82f, 0.84f, 0.9f, 0.5f);
                case CrystalMaterialMode.AbsoluteMirror:
                    return new Color(0.92f, 0.96f, 1f, 0.48f);
                default:
                    return new Color(0.56f, 0.86f, 1f, 0.58f);
            }
        }

        private static void ConfigureStandardOpaque(Material material)
        {
            if (material == null)
            {
                return;
            }

            material.SetFloat("_Mode", 0f);
            material.SetInt("_SrcBlend", (int)BlendMode.One);
            material.SetInt("_DstBlend", (int)BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = -1;
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Glossiness", 0.82f);
        }

        private static void ConfigureStandardTransparent(Material material)
        {
            if (material == null)
            {
                return;
            }

            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Glossiness", 0.96f);
        }

        private void RenderStageCamera()
        {
            if (stageCamera == null || outputTexture == null)
            {
                crystalCameraTargetTextureStatus = "none";
                return;
            }

            RenderTexture previousTarget = stageCamera.targetTexture;
            crystalCameraTargetTextureStatus = FormatTexture(outputTexture);
            try
            {
                stageCamera.enabled = false;
                stageCamera.targetTexture = outputTexture;
                stageCamera.Render();
            }
            finally
            {
                stageCamera.targetTexture = previousTarget;
                stageCamera.enabled = false;
            }
        }

        private int CountActiveLights()
        {
            int count = 0;
            if (keyLight != null && keyLight.gameObject.activeSelf)
            {
                keyLight.cullingMask = stageLayerMask;
                count++;
            }

            if (rimLight != null && rimLight.gameObject.activeSelf)
            {
                rimLight.cullingMask = stageLayerMask;
                count++;
            }

            if (fillLight != null && fillLight.gameObject.activeSelf)
            {
                fillLight.cullingMask = stageLayerMask;
                count++;
            }

            return count;
        }

        private bool ResolveValidationOrbit(CrystalSharedSettings settings, CrystalStage3DDebugMode debugMode)
        {
            return settings != null
                && settings.CameraOrbitEnabled
                && debugMode != CrystalStage3DDebugMode.FinalPremiumComposite;
        }

        private bool IsCrystalBetweenCameraAndBackground()
        {
            if (stageCamera == null || crystalObject == null || backgroundObject == null)
            {
                return false;
            }

            Vector3 cameraPosition = stageCamera.transform.position;
            Vector3 cameraForward = stageCamera.transform.forward;
            float crystalDepth = Vector3.Dot(cameraForward, crystalObject.transform.position - cameraPosition);
            float backgroundDepth = Vector3.Dot(cameraForward, backgroundObject.transform.position - cameraPosition);
            return crystalDepth > 0f && backgroundDepth > crystalDepth;
        }

        private bool IsPhysicalStageVisibleToNonStageCamera()
        {
            return root != null
                && root.activeInHierarchy
                && CanAnyDirectViewCameraRenderStageLayer()
                && IsAnyPhysicalStageRendererEnabled();
        }

        private bool CanAnyDirectViewCameraRenderStageLayer()
        {
            int cameraCount = Camera.allCamerasCount;
            EnsureCameraCacheCapacity(cameraCount);
            int resolvedCount = Camera.GetAllCameras(cameraCache);
            for (int index = 0; index < resolvedCount; index++)
            {
                Camera camera = cameraCache[index];
                if (camera == null || ReferenceEquals(camera, stageCamera) || !IsDirectViewCamera(camera))
                {
                    continue;
                }

                if ((camera.cullingMask & stageLayerMask) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanAnyNonViewCameraRenderStageLayer()
        {
            int cameraCount = Camera.allCamerasCount;
            EnsureCameraCacheCapacity(cameraCount);
            int resolvedCount = Camera.GetAllCameras(cameraCache);
            for (int index = 0; index < resolvedCount; index++)
            {
                Camera camera = cameraCache[index];
                if (camera == null || ReferenceEquals(camera, stageCamera) || IsDirectViewCamera(camera))
                {
                    continue;
                }

                if ((camera.cullingMask & stageLayerMask) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsRendererVisibleToDirectView(MeshRenderer renderer)
        {
            return root != null
                && root.activeInHierarchy
                && renderer != null
                && renderer.enabled
                && CanAnyDirectViewCameraRenderStageLayer();
        }

        private bool IsAnyPhysicalStageRendererEnabled()
        {
            return (crystalMeshRenderer != null && crystalMeshRenderer.enabled)
                || (backgroundMeshRenderer != null && backgroundMeshRenderer.enabled);
        }

        private bool PhysicalStageUsesOutputTexture()
        {
            return MaterialUsesOutputTexture(crystalMeshRenderer)
                || MaterialUsesOutputTexture(backgroundMeshRenderer);
        }

        private bool MaterialUsesOutputTexture(MeshRenderer renderer)
        {
            if (renderer == null || outputTexture == null)
            {
                return false;
            }

            Material material = renderer.sharedMaterial;
            if (material == null)
            {
                return false;
            }

            return ReferenceEquals(material.mainTexture, outputTexture)
                || TexturePropertyReferencesOutput(material, "_MainTex")
                || TexturePropertyReferencesOutput(material, "_BaseMap")
                || TexturePropertyReferencesOutput(material, "_KaleidoscopeTex");
        }

        private bool TexturePropertyReferencesOutput(Material material, string propertyName)
        {
            return material != null
                && material.HasProperty(propertyName)
                && ReferenceEquals(material.GetTexture(propertyName), outputTexture);
        }

        private void UpdateCameraDiagnostics()
        {
            int cameraCount = Camera.allCamerasCount;
            EnsureCameraCacheCapacity(cameraCount);
            int resolvedCount = Camera.GetAllCameras(cameraCache);
            activeCameraDiagnostics = "none";
            for (int index = 0; index < resolvedCount; index++)
            {
                Camera camera = cameraCache[index];
                if (camera == null)
                {
                    continue;
                }

                string cameraStatus = camera.name
                    + "[enabled=" + (camera.enabled ? "true" : "false")
                    + ",clear=" + camera.clearFlags.ToString()
                    + ",mask=" + FormatMask(camera.cullingMask)
                    + ",target=" + FormatTexture(camera.targetTexture)
                    + ",depth=" + camera.depth.ToString("0.0")
                    + ",role=" + (camera.targetTexture != null ? "RT" : "Screen")
                    + "]";
                activeCameraDiagnostics = activeCameraDiagnostics == "none"
                    ? cameraStatus
                    : activeCameraDiagnostics + "|" + cameraStatus;
            }
        }

        private void UpdateDiagnostics(
            Bounds? crystalBounds,
            Camera camera,
            float screenCoverage,
            float backgroundCoverage,
            float cameraCrystalDistance,
            float crystalBackgroundDistance,
            bool crystalBetweenCameraAndBackground,
            CrystalStage3DDebugMode debugMode,
            bool validationOrbitActive)
        {
            string crystalPosition = crystalObject != null
                ? CrystalSpatialDiagnostics.FormatVector(crystalObject.transform.position)
                : "none";
            string backgroundPosition = backgroundObject != null
                ? CrystalSpatialDiagnostics.FormatVector(backgroundObject.transform.position)
                : "none";
            string backgroundLocalPosition = backgroundObject != null
                ? CrystalSpatialDiagnostics.FormatVector(backgroundObject.transform.localPosition)
                : "none";
            string crystalLocalPosition = crystalObject != null
                ? CrystalSpatialDiagnostics.FormatVector(crystalObject.transform.localPosition)
                : "none";
            string crystalScale = crystalObject != null
                ? CrystalSpatialDiagnostics.FormatVector(crystalObject.transform.localScale)
                : "none";
            string stageRootScale = root != null
                ? CrystalSpatialDiagnostics.FormatVector(root.transform.localScale)
                : "none";
            string dioramaScale = dioramaObject != null
                ? CrystalSpatialDiagnostics.FormatVector(dioramaObject.transform.localScale)
                : "none";
            string backgroundScale = backgroundObject != null
                ? CrystalSpatialDiagnostics.FormatVector(backgroundObject.transform.localScale)
                : "none";
            string cameraPosition = camera != null
                ? CrystalSpatialDiagnostics.FormatVector(camera.transform.position)
                : "none";
            string cameraLocalPosition = camera != null
                ? CrystalSpatialDiagnostics.FormatVector(camera.transform.localPosition)
                : "none";
            string boundsSize = crystalBounds.HasValue
                ? CrystalSpatialDiagnostics.FormatVector(crystalBounds.Value.size)
                : "none";
            string hierarchy = root != null
                ? CrystalSpatialDiagnostics.GetHierarchyPath(root.transform)
                : "none";
            string ownerName = owner != null ? owner.name : "none";
            string stageTextureSize = outputTexture != null
                ? outputTexture.width.ToString() + "x" + outputTexture.height.ToString()
                : "none";
            string cameraProjection = camera != null
                ? (camera.orthographic ? "orthographic" : "perspective")
                : "none";
            string cameraFov = camera != null ? camera.fieldOfView.ToString("0.0") : "none";
            string stageMask = FormatMask(stageLayerMask);
            string crystalCameraMask = camera != null ? FormatMask(camera.cullingMask) : "none";
            bool stageVisibleToNonStageCamera = IsPhysicalStageVisibleToNonStageCamera();
            bool directViewCameraCanRenderStageLayer = CanAnyDirectViewCameraRenderStageLayer();
            bool nonViewCameraCanRenderStageLayer = CanAnyNonViewCameraRenderStageLayer();
            bool crystalVisibleToDirectView = IsRendererVisibleToDirectView(crystalMeshRenderer);
            bool backgroundVisibleToDirectView = IsRendererVisibleToDirectView(backgroundMeshRenderer);
            string diagnosticsActive = diagnosticsObject != null && diagnosticsObject.activeInHierarchy ? "true" : "false";

            diagnosticsLabel = StageModeLabel
                + ", mode " + debugMode.ToString()
                + ", legacy composite active false"
                + ", active view path DirectPhysicalPremium3D"
                + ", SpatialCrystalStage3D active " + (stageRenderedThisFrame ? "true" : "false")
                + ", wrong background plane active false"
                + ", runtime stage scene visible " + (root != null && root.activeSelf ? "true" : "false")
                + ", writes diagnostic/offline RT " + (stageRenderedThisFrame && outputTexture != null ? "true" : "false")
                + ", stage RT " + stageTextureSize
                + ", StageDiagnostics active " + diagnosticsActive
                + ", active cameras " + activeCameraDiagnostics
                + ", camera projection " + cameraProjection
                + ", camera FOV " + cameraFov
                + ", stage layer " + StageLayerSemanticName + " physical layer " + SpatialStageLayerName + "(" + layer.ToString() + ")"
                + ", CrystalCamera culling mask " + crystalCameraMask
                + ", CrystalCamera targetTexture " + crystalCameraTargetTextureStatus
                + ", direct view cameras " + directViewCameraCount.ToString()
                + ", direct view camera names " + directViewCameraNames
                + ", protected non-view cameras " + protectedCameraCount.ToString()
                + ", protected non-view camera names " + protectedCameraNames
                + ", stage layer mask " + stageMask
                + ", stage root scale " + stageRootScale
                + ", stageViewScale " + stageViewScale.ToString("0.00")
                + ", diorama scale " + dioramaScale
                + ", crystal world " + crystalPosition
                + ", crystal local " + crystalLocalPosition
                + ", crystal scale " + crystalScale
                + ", background world " + backgroundPosition
                + ", background local " + backgroundLocalPosition
                + ", background scale " + backgroundScale
                + ", camera world " + cameraPosition
                + ", camera local " + cameraLocalPosition
                + ", camera-crystal distance " + cameraCrystalDistance.ToString("0.00")
                + ", crystal-background distance " + crystalBackgroundDistance.ToString("0.00")
                + ", crystal bounds " + boundsSize
                + ", screen coverage " + CrystalSpatialDiagnostics.FormatPercent(screenCoverage)
                + ", background screen coverage " + CrystalSpatialDiagnostics.FormatPercent(backgroundCoverage)
                + ", crystalBetweenCameraAndBackground " + (crystalBetweenCameraAndBackground ? "true" : "false")
                + ", validation orbit " + (validationOrbitActive ? "on" : "off")
                + ", fullscreen composite dominates false"
                + ", physical stage visible to Main/View Camera " + (stageVisibleToNonStageCamera ? "true" : "false")
                + ", Main/View cameras can render SpatialStage3D " + (directViewCameraCanRenderStageLayer ? "true" : "false")
                + ", non-view cameras can render SpatialStage3D " + (nonViewCameraCanRenderStageLayer ? "true" : "false")
                + ", RealMeshCrystal visible to Main/View Camera " + (crystalVisibleToDirectView ? "true" : "false")
                + ", BackgroundGeometry visible to Main/View Camera " + (backgroundVisibleToDirectView ? "true" : "false")
                + ", physical material uses stage RT " + (physicalStageUsesStageOutputTexture ? "true" : "false")
                + ", owner " + ownerName
                + ", hierarchy " + hierarchy;
        }

        private void AssignLayerRecursive(Transform current)
        {
            if (current == null)
            {
                return;
            }

            current.gameObject.layer = layer;
            for (int index = 0; index < current.childCount; index++)
            {
                AssignLayerRecursive(current.GetChild(index));
            }
        }

        private static int ResolveSpatialStageLayer(int fallbackLayer)
        {
            int namedLayer = LayerMask.NameToLayer(SpatialStageLayerName);
            return namedLayer >= 0 ? namedLayer : Mathf.Clamp(fallbackLayer, 0, 31);
        }

        private static string FormatMask(int mask)
        {
            return "0x" + unchecked((uint)mask).ToString("X8");
        }

        private static string FormatTexture(Texture texture)
        {
            return texture != null
                ? texture.name + "(" + texture.width.ToString() + "x" + texture.height.ToString() + ")"
                : "none";
        }

        private void ReleaseOutputTexture()
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
            crystalCameraTargetTextureStatus = "none";
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
