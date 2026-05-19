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
        private const string CrystalOpticsShaderName = "Kaleidoscope2/RealCrystalOptics";
        private const string PremiumBackgroundShaderName = "Kaleidoscope2/CrystalStage3D/PremiumOpticalBackground";
        private const float CameraDistance = 10f;
        private const float BackgroundDistance = 3.5f;
        private const float CameraFieldOfView = 35f;
        private const float TargetCrystalScreenCoverage = 0.55f;
        private const float MinimumCrystalScreenCoverage = 0.52f;
        private const float MaximumCrystalScreenCoverage = 0.58f;
        private const float DioramaCrystalScale = 0.72f;
        private const float BackgroundViewFillMargin = 1.08f;
        private const float MinimumStageViewScale = 0.2f;
        private const float MaximumStageViewScale = 16f;
        private const float MinimumMeshDimension = 0.001f;
        private const float StableValueTolerance = 0.0005f;
        private const bool BackgroundPulseEnabled = false;
        private const bool RadialWavesEnabled = false;
        private const bool CrystalScalePulseEnabled = false;
        private const bool CameraBreathingEnabled = false;
        private const bool AnimatedCoverageCorrectionEnabled = false;
        private static readonly int StageMainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int StageViewAspectId = Shader.PropertyToID("_ViewAspect");
        private static readonly int StageTextureAspectId = Shader.PropertyToID("_TextureAspect");
        private static readonly int CrystalKaleidoscopeTexId = Shader.PropertyToID("_KaleidoscopeTex");
        private static readonly int CrystalTintId = Shader.PropertyToID("_Tint");
        private static readonly int CrystalIntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int CrystalAlphaId = Shader.PropertyToID("_Alpha");
        private static readonly int CrystalMetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int CrystalSmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int CrystalTransparencyId = Shader.PropertyToID("_Transparency");
        private static readonly int CrystalRefractionStrengthId = Shader.PropertyToID("_RefractionStrength");
        private static readonly int CrystalFresnelPowerId = Shader.PropertyToID("_FresnelPower");
        private static readonly int CrystalReflectionStrengthId = Shader.PropertyToID("_ReflectionStrength");
        private static readonly int CrystalInternalBrightnessId = Shader.PropertyToID("_InternalBrightness");
        private static readonly int CrystalMinimumTransmissionId = Shader.PropertyToID("_MinimumTransmission");
        private static readonly int CrystalSpecularStrengthId = Shader.PropertyToID("_SpecularStrength");
        private static readonly int CrystalDispersionStrengthId = Shader.PropertyToID("_DispersionStrength");
        private static readonly int CrystalRimStrengthId = Shader.PropertyToID("_RimStrength");
        private static readonly int CrystalBrightnessFloorId = Shader.PropertyToID("_BrightnessFloor");
        private static readonly int CrystalGlintStrengthId = Shader.PropertyToID("_GlintStrength");
        private static readonly int CrystalFacetContrastId = Shader.PropertyToID("_FacetContrast");
        private static readonly int CrystalInternalScatterId = Shader.PropertyToID("_InternalScatter");

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
        private Light glintLightA;
        private Light glintLightB;
        private Light glintLightC;
        private Light glintLightD;
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
        private string opticalStageDiagnostics = "premium optical layers inactive";
        private string sourceTextureDiagnostics = "source texture none";
        private string activePremiumShapeLabel = "none";
        private float validationOrbitPhase;
        private float stageViewScale = 1f;
        private float finalVisibleScreenCoverage;
        private string finalVisibleCoverageDiagnostics = "final visible coverage not measured";
        private bool stabilityBaselineCaptured;
        private int stabilitySampleCount;
        private Vector3 baselineDioramaScale;
        private Vector3 baselineCrystalLocalScale;
        private Vector3 baselineBackgroundLocalScale;
        private Vector3 baselineStageCameraLocalPosition;
        private float baselineStageViewScale;
        private float baselineStageCameraFieldOfView;
        private float baselineStageCameraOrthographicSize;
        private float baselineStageCameraDistance;
        private float minFinalVisibleCoverage;
        private float maxFinalVisibleCoverage;
        private float minBackgroundApparentScale;
        private float maxBackgroundApparentScale;
        private string staticBaselineDiagnostics = "static baseline not sampled";
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
            get { return crystalMesh != null && crystalMesh.subMeshCount > 0 ? (int)(crystalMesh.GetIndexCount(0) / 3) : 0; }
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
                opticalStageDiagnostics = "premium optical layers inactive";
                finalVisibleScreenCoverage = 0f;
                finalVisibleCoverageDiagnostics = "final visible coverage not measured, stage hidden";
                ResetStaticBaselineTracking("stage hidden");
                sourceTextureDiagnostics = "source texture none";
                UpdateCameraDiagnostics();
                UpdateDiagnostics(null, null, null, 0f, 0f, 0f, 0f, false, debugMode, false);
                return false;
            }

            EnsureRuntimeObjects();
            ConfigureDirectViewCameras();
            EnsureOutputTexture(sourceTexture.width, sourceTexture.height);
            EnsureCrystalMesh(shape);
            ConfigureStage(sourceTexture, settings, materialMode, rotation, intensity, solidGeometryValidation, debugMode);
            RenderStageCamera();

            Bounds crystalBounds = crystalMesh != null && crystalObject != null
                ? CrystalSpatialDiagnostics.TransformBounds(crystalMesh.bounds, crystalObject.transform)
                : crystalMeshRenderer != null ? crystalMeshRenderer.bounds : new Bounds();
            Bounds backgroundBounds = backgroundMesh != null && backgroundObject != null
                ? CrystalSpatialDiagnostics.TransformBounds(backgroundMesh.bounds, backgroundObject.transform)
                : backgroundMeshRenderer != null ? backgroundMeshRenderer.bounds : new Bounds();
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
                backgroundBounds,
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
            glintLightA = null;
            glintLightB = null;
            glintLightC = null;
            glintLightD = null;
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
            opticalStageDiagnostics = "premium optical layers inactive";
            finalVisibleScreenCoverage = 0f;
            finalVisibleCoverageDiagnostics = "final visible coverage not measured";
            ResetStaticBaselineTracking("shutdown");
            sourceTextureDiagnostics = "source texture none";
            activePremiumShapeLabel = "none";
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
                glintLightA = CreateLight("GlintLightA", LightType.Point, Vector3.zero);
                glintLightB = CreateLight("GlintLightB", LightType.Point, Vector3.zero);
                glintLightC = CreateLight("GlintLightC", LightType.Point, Vector3.zero);
                glintLightD = CreateLight("GlintLightD", LightType.Point, Vector3.zero);
            }
            lightRigObject.transform.SetParent(dioramaObject.transform, false);
            if (glintLightA == null)
            {
                glintLightA = CreateLight("GlintLightA", LightType.Point, Vector3.zero);
            }

            if (glintLightB == null)
            {
                glintLightB = CreateLight("GlintLightB", LightType.Point, Vector3.zero);
            }

            if (glintLightC == null)
            {
                glintLightC = CreateLight("GlintLightC", LightType.Point, Vector3.zero);
            }

            if (glintLightD == null)
            {
                glintLightD = CreateLight("GlintLightD", LightType.Point, Vector3.zero);
            }

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
            backgroundObject.transform.SetParent(root.transform, false);

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
            backgroundObject.transform.SetSiblingIndex(2);
            diagnosticsObject.transform.SetSiblingIndex(3);

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
            light.shadows = type == LightType.Point ? LightShadows.None : LightShadows.Soft;
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
            ResetStaticBaselineTracking("output texture resized");
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
            CrystalShape premiumShape = ResolvePremiumShape(shape);
            if (crystalMesh != null && activeShape == premiumShape)
            {
                return;
            }

            DestroyRuntimeObject(crystalMesh);
            crystalMesh = RealCrystalShapeLibrary.CreateMesh(premiumShape);
            activeShape = premiumShape;
            activePremiumShapeLabel = ResolvePremiumShapeLabel(shape, premiumShape);
            ResetStaticBaselineTracking("crystal mesh changed");
            if (crystalMeshFilter != null)
            {
                crystalMeshFilter.sharedMesh = crystalMesh;
            }
        }

        private static string ResolvePremiumShapeLabel(CrystalShape requestedShape, CrystalShape premiumShape)
        {
            if (premiumShape == CrystalShape.BrilliantCut)
            {
                return "Classic Brilliant / Premium Diamond (" + requestedShape.ToString() + " -> " + premiumShape.ToString() + ")";
            }

            return requestedShape.ToString() + " -> " + premiumShape.ToString();
        }

        private static CrystalShape ResolvePremiumShape(CrystalShape shape)
        {
            switch (shape)
            {
                case CrystalShape.ClassicDiamond:
                    return CrystalShape.BrilliantCut;
                case CrystalShape.FacetedCube:
                    return CrystalShape.PrincessCut;
                case CrystalShape.DiscoBall:
                    return CrystalShape.CushionCut;
                case CrystalShape.TetrahedralCrystal:
                    return CrystalShape.PearCut;
                case CrystalShape.RhombicCrystal:
                    return CrystalShape.MarquiseCut;
                case CrystalShape.OvalRingGem:
                    return CrystalShape.EmeraldCut;
                default:
                    return shape;
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
            dioramaObject.transform.localPosition = Vector3.zero;
            dioramaObject.transform.localRotation = Quaternion.identity;

            Camera framingCamera = ResolveFinalVisibleCamera();
            float viewHeightAtCrystal = ResolveViewHeightForFramingCamera(framingCamera, root.transform.TransformPoint(Vector3.zero), CameraDistance);
            float targetCoverage = ClampFinalVisibleCoverage(TargetCrystalScreenCoverage);
            float desiredCrystalWorldHeight = viewHeightAtCrystal * targetCoverage;
            float meshHeight = crystalMesh != null ? Mathf.Max(MinimumMeshDimension, crystalMesh.bounds.size.y) : 1f;
            stageViewScale = Mathf.Clamp(
                desiredCrystalWorldHeight / Mathf.Max(MinimumMeshDimension, meshHeight * DioramaCrystalScale),
                MinimumStageViewScale,
                MaximumStageViewScale);
            dioramaObject.transform.localScale = Vector3.one * stageViewScale;

            Vector2 centerOffset = settings != null ? settings.CrystalScreenCenterOffset : Vector2.zero;
            float framingAspect = framingCamera != null && framingCamera.aspect > 0f ? framingCamera.aspect : aspect;
            float viewWidthAtCrystal = viewHeightAtCrystal * Mathf.Max(0.1f, framingAspect);
            crystalObject.transform.localPosition = new Vector3(
                centerOffset.x * viewWidthAtCrystal * 0.25f,
                centerOffset.y * viewHeightAtCrystal * 0.25f,
                0f);
            crystalObject.transform.localScale = Vector3.one * DioramaCrystalScale;
            crystalObject.transform.localRotation = Quaternion.Euler(rotation);

            ApplyDeterministicFinalVisibleScaleCorrection(framingCamera, targetCoverage);

            backgroundObject.transform.localPosition = new Vector3(0f, 0f, BackgroundDistance);
            backgroundObject.transform.localRotation = Quaternion.identity;
            Vector2 backgroundWorldSize = ResolveBackgroundWorldSize(aspect) * BackgroundViewFillMargin;
            backgroundObject.transform.localScale = new Vector3(
                backgroundWorldSize.x,
                backgroundWorldSize.y,
                1f);
            ApplyFinalVisibleCentering(framingCamera);
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

        private static float ClampFinalVisibleCoverage(float value)
        {
            return Mathf.Clamp(value, MinimumCrystalScreenCoverage, MaximumCrystalScreenCoverage);
        }

        private float ResolveViewHeightForFramingCamera(Camera framingCamera, Vector3 crystalWorldPosition, float fallbackDistance)
        {
            if (framingCamera == null)
            {
                return ResolveViewHeight(fallbackDistance);
            }

            float distance;
            if (!TryResolveCameraDepth(framingCamera, crystalWorldPosition, out distance))
            {
                return ResolveViewHeight(fallbackDistance);
            }

            return ResolveCameraViewHeight(framingCamera, distance);
        }

        private void ApplyDeterministicFinalVisibleScaleCorrection(Camera framingCamera, float targetCoverage)
        {
            if (framingCamera == null || crystalMesh == null || crystalObject == null || dioramaObject == null)
            {
                return;
            }

            for (int pass = 0; pass < 2; pass++)
            {
                Bounds bounds = CrystalSpatialDiagnostics.TransformBounds(crystalMesh.bounds, crystalObject.transform);
                float currentCoverage = CrystalSpatialDiagnostics.ViewportHeightCoverage(framingCamera, bounds);
                if (currentCoverage <= 0.0001f)
                {
                    return;
                }

                float correction = targetCoverage / currentCoverage;
                if (Mathf.Abs(correction - 1f) <= StableValueTolerance)
                {
                    return;
                }

                stageViewScale = Mathf.Clamp(stageViewScale * correction, MinimumStageViewScale, MaximumStageViewScale);
                dioramaObject.transform.localScale = Vector3.one * stageViewScale;
            }
        }

        private void ApplyFinalVisibleCentering(Camera framingCamera)
        {
            if (framingCamera == null || crystalObject == null || dioramaObject == null)
            {
                return;
            }

            Vector3 crystalWorld = crystalMesh != null
                ? CrystalSpatialDiagnostics.TransformBounds(crystalMesh.bounds, crystalObject.transform).center
                : crystalObject.transform.position;
            Vector3 viewport = framingCamera.WorldToViewportPoint(crystalWorld);
            if (viewport.z <= framingCamera.nearClipPlane)
            {
                return;
            }

            Vector3 desiredWorld = framingCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, viewport.z));
            Vector3 worldDelta = desiredWorld - crystalWorld;
            if (!IsFinite(worldDelta) || worldDelta.sqrMagnitude > 10000f)
            {
                return;
            }

            dioramaObject.transform.position += worldDelta;
        }

        private Vector2 ResolveBackgroundWorldSize(float stageAspect)
        {
            float backgroundCameraDistance = CameraDistance + BackgroundDistance;
            float requiredHeight = ResolveViewHeight(backgroundCameraDistance);
            float requiredWidth = requiredHeight * Mathf.Max(0.1f, stageAspect);
            int cameraCount = Camera.allCamerasCount;
            EnsureCameraCacheCapacity(cameraCount);
            int resolvedCount = Camera.GetAllCameras(cameraCache);
            Vector3 backgroundWorld = backgroundObject != null ? backgroundObject.transform.position : Vector3.zero;
            for (int index = 0; index < resolvedCount; index++)
            {
                Camera camera = cameraCache[index];
                if (camera == null || ReferenceEquals(camera, stageCamera) || !IsDirectViewCamera(camera))
                {
                    continue;
                }

                if ((camera.cullingMask & stageLayerMask) == 0)
                {
                    continue;
                }

                float distance = Vector3.Dot(camera.transform.forward, backgroundWorld - camera.transform.position);
                if (distance <= camera.nearClipPlane)
                {
                    continue;
                }

                float viewHeight = ResolveCameraViewHeight(camera, distance);
                requiredHeight = Mathf.Max(requiredHeight, viewHeight);
                requiredWidth = Mathf.Max(requiredWidth, viewHeight * Mathf.Max(0.1f, camera.aspect));
            }

            return new Vector2(requiredWidth, requiredHeight);
        }

        private static float ResolveCameraViewHeight(Camera camera, float distance)
        {
            if (camera == null)
            {
                return 0f;
            }

            if (camera.orthographic)
            {
                return Mathf.Max(MinimumMeshDimension, camera.orthographicSize * 2f);
            }

            float halfFovRadians = Mathf.Clamp(camera.fieldOfView, 1f, 179f) * 0.5f * Mathf.Deg2Rad;
            return Mathf.Tan(halfFovRadians) * Mathf.Max(MinimumMeshDimension, distance) * 2f;
        }

        private void ConfigureLights(CrystalSharedSettings settings, float intensity)
        {
            bool enabled = settings == null || settings.CrystalLightRigEnabled;
            int activeCount = enabled && settings != null ? settings.RealMeshLightCount : 3;
            float resolvedIntensity = Mathf.Clamp(intensity, 0f, 20f);
            float time = Time.time;
            Vector3 glintPositionA = new Vector3(
                Mathf.Sin(time * 0.82f) * 2.35f,
                1.35f + Mathf.Sin(time * 1.12f) * 0.42f,
                -1.35f + Mathf.Cos(time * 0.67f) * 0.45f);
            Vector3 glintPositionB = new Vector3(
                Mathf.Cos(time * 0.71f + 1.4f) * 2.65f,
                -0.85f + Mathf.Sin(time * 0.94f + 0.6f) * 0.35f,
                -1.05f + Mathf.Sin(time * 0.53f) * 0.5f);
            Vector3 glintPositionC = new Vector3(
                Mathf.Sin(time * 1.08f + 2.1f) * 2.1f,
                0.25f + Mathf.Cos(time * 0.88f) * 1.35f,
                -1.75f + Mathf.Sin(time * 0.79f + 1.2f) * 0.38f);
            Vector3 glintPositionD = new Vector3(
                Mathf.Cos(time * 1.22f + 3.4f) * 2.85f,
                0.85f + Mathf.Sin(time * 1.17f + 2.4f) * 0.72f,
                -0.72f + Mathf.Cos(time * 0.58f + 0.9f) * 0.62f);

            ConfigureLight(keyLight, enabled && activeCount >= 1, resolvedIntensity * 1.32f, new Color(1f, 0.94f, 0.78f, 1f), new Vector3(-2.15f, 2.75f, -3.45f));
            ConfigureLight(rimLight, enabled && activeCount >= 2, resolvedIntensity * 0.98f, new Color(0.5f, 0.78f, 1f, 1f), new Vector3(2.55f, 2.08f, -1.35f));
            ConfigureLight(fillLight, enabled && activeCount >= 3, resolvedIntensity * 0.52f, new Color(0.78f, 0.94f, 1f, 1f), new Vector3(0f, -1.28f, -2.35f));
            ConfigureLight(glintLightA, enabled && activeCount >= 4, resolvedIntensity * 0.34f, new Color(1f, 0.78f, 0.36f, 1f), glintPositionA);
            ConfigureLight(glintLightB, enabled && activeCount >= 5, resolvedIntensity * 0.28f, new Color(0.38f, 0.92f, 1f, 1f), glintPositionB);
            ConfigureLight(glintLightC, enabled && activeCount >= 6, resolvedIntensity * 0.24f, new Color(0.86f, 0.46f, 1f, 1f), glintPositionC);
            ConfigureLight(glintLightD, enabled && activeCount >= 7, resolvedIntensity * 0.22f, new Color(1f, 0.32f, 0.22f, 1f), glintPositionD);
            lastActiveLightCount = CountActiveLights();
        }

        private static void ConfigureLight(Light light, bool active, float intensity, Color color, Vector3 localPosition)
        {
            if (light == null)
            {
                return;
            }

            light.gameObject.SetActive(active);
            light.intensity = Mathf.Max(0f, intensity);
            light.color = color;
            light.range = light.type == LightType.Point ? 7.5f : 18f;
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

        private Camera ResolveFinalVisibleCamera()
        {
            int cameraCount = Camera.allCamerasCount;
            EnsureCameraCacheCapacity(cameraCount);
            int resolvedCount = Camera.GetAllCameras(cameraCache);
            Camera bestCamera = null;
            float bestDepth = float.NegativeInfinity;
            for (int index = 0; index < resolvedCount; index++)
            {
                Camera camera = cameraCache[index];
                if (camera == null || ReferenceEquals(camera, stageCamera) || !camera.enabled || !IsDirectViewCamera(camera))
                {
                    continue;
                }

                if ((camera.cullingMask & stageLayerMask) == 0)
                {
                    continue;
                }

                if (bestCamera == null || camera.depth >= bestDepth)
                {
                    bestCamera = camera;
                    bestDepth = camera.depth;
                }
            }

            return bestCamera;
        }

        private static bool TryResolveCameraDepth(Camera camera, Vector3 worldPosition, out float depth)
        {
            depth = 0f;
            if (camera == null)
            {
                return false;
            }

            depth = Vector3.Dot(camera.transform.forward, worldPosition - camera.transform.position);
            return depth > camera.nearClipPlane;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
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
                Shader shader = Shader.Find(PremiumBackgroundShaderName);
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Texture");
                }

                backgroundMaterial = new Material(shader)
                {
                    name = "Kaleidoscope2_CrystalStage3D_BackgroundMaterial",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            else
            {
                Shader premiumShader = Shader.Find(PremiumBackgroundShaderName);
                if (premiumShader != null && backgroundMaterial.shader != premiumShader)
                {
                    backgroundMaterial.shader = premiumShader;
                }
            }

            backgroundMaterial.mainTexture = sourceTexture;
            ConfigurePremiumBackgroundMaterial(backgroundMaterial, sourceTexture);
            backgroundMeshRenderer.sharedMaterial = backgroundMaterial;
            backgroundMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            backgroundMeshRenderer.receiveShadows = false;

            Material crystalMaterial = solidGeometryValidation
                ? EnsureSolidCrystalMaterial()
                : EnsureTransparentCrystalMaterial(sourceTexture, settings, materialMode, intensity);
            crystalMeshRenderer.sharedMaterial = crystalMaterial;
            crystalMeshRenderer.shadowCastingMode = ShadowCastingMode.On;
            crystalMeshRenderer.receiveShadows = true;
        }

        private void ConfigurePremiumBackgroundMaterial(Material material, RenderTexture sourceTexture)
        {
            if (material == null)
            {
                opticalStageDiagnostics = "premium optical layers inactive, background material missing";
                sourceTextureDiagnostics = "source texture none";
                return;
            }

            sourceTextureDiagnostics = sourceTexture != null
                ? "source texture " + FormatTexture(sourceTexture)
                : "source texture none";
            float geometryAspect = backgroundObject != null && Mathf.Abs(backgroundObject.transform.localScale.y) > MinimumMeshDimension
                ? Mathf.Abs(backgroundObject.transform.localScale.x / backgroundObject.transform.localScale.y)
                : 0f;
            float viewAspect = geometryAspect > 0f
                ? geometryAspect
                : stageCamera != null && stageCamera.aspect > 0f
                ? stageCamera.aspect
                : (sourceTexture != null && sourceTexture.height > 0 ? sourceTexture.width / (float)sourceTexture.height : 1f);
            float textureAspect = sourceTexture != null && sourceTexture.height > 0
                ? sourceTexture.width / (float)sourceTexture.height
                : viewAspect;

            SetMaterialTextureIfPresent(material, StageMainTexId, sourceTexture);
            SetMaterialFloatIfPresent(material, StageViewAspectId, viewAspect);
            SetMaterialFloatIfPresent(material, StageTextureAspectId, textureAspect);
            opticalStageDiagnostics = material.shader != null && material.shader.name == PremiumBackgroundShaderName
                ? "premium background clean, selected source fill-cover true, background fills camera view true, pulsing false, rings false, radial waves false, caustics false, prism false"
                : "premium optical layers fallback shader";
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
            RenderTexture sourceTexture,
            CrystalSharedSettings settings,
            CrystalMaterialMode materialMode,
            float intensity)
        {
            if (transparentCrystalMaterial == null)
            {
                Shader shader = Shader.Find(CrystalOpticsShaderName);
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                transparentCrystalMaterial = new Material(shader)
                {
                    name = "Kaleidoscope2_CrystalStage3D_TransparentCrystalMaterial",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            else
            {
                Shader opticsShader = Shader.Find(CrystalOpticsShaderName);
                if (opticsShader != null && transparentCrystalMaterial.shader != opticsShader)
                {
                    transparentCrystalMaterial.shader = opticsShader;
                }
            }

            Color color = ResolveCrystalColor(materialMode);
            float alpha = settings != null ? settings.RealMeshAlpha : 0.58f;
            color.a = Mathf.Clamp(alpha + 0.08f + Mathf.Clamp01(intensity / 20f) * 0.16f, 0.38f, 0.92f);
            transparentCrystalMaterial.color = color;
            if (transparentCrystalMaterial.shader != null && transparentCrystalMaterial.shader.name == CrystalOpticsShaderName)
            {
                ConfigurePremiumCrystalMaterial(transparentCrystalMaterial, sourceTexture, settings, color, materialMode, intensity);
            }
            else
            {
                ConfigureStandardTransparent(transparentCrystalMaterial);
            }

            return transparentCrystalMaterial;
        }

        private static void ConfigurePremiumCrystalMaterial(
            Material material,
            RenderTexture sourceTexture,
            CrystalSharedSettings settings,
            Color color,
            CrystalMaterialMode materialMode,
            float intensity)
        {
            float intensity01 = Mathf.Clamp01(intensity / 20f);
            float transparency = settings != null ? settings.Transparency : 0.08f;
            float refractionStrength = settings != null ? settings.RefractionStrength : 0.085f;
            float fresnelPower = settings != null ? settings.FresnelPower : 2.7f;
            float reflectionStrength = settings != null ? settings.ReflectionStrength : 0.72f;
            float internalBrightness = settings != null ? settings.InternalBrightness : 1.22f;
            float minimumTransmission = settings != null ? settings.MinimumTransmission : 0.18f;
            float specularStrength = settings != null ? settings.SpecularStrength : 0.86f;
            float metallic;
            float smoothness;
            ResolveCrystalSurface(materialMode, out metallic, out smoothness);

            SetMaterialTextureIfPresent(material, CrystalKaleidoscopeTexId, sourceTexture);
            SetMaterialColorIfPresent(material, CrystalTintId, color);
            SetMaterialFloatIfPresent(material, CrystalIntensityId, Mathf.Clamp(intensity, 0f, 20f));
            SetMaterialFloatIfPresent(material, CrystalAlphaId, Mathf.Clamp(color.a, 0.42f, 0.94f));
            SetMaterialFloatIfPresent(material, CrystalMetallicId, metallic);
            SetMaterialFloatIfPresent(material, CrystalSmoothnessId, Mathf.Clamp01(smoothness + intensity01 * 0.04f));
            SetMaterialFloatIfPresent(material, CrystalTransparencyId, Mathf.Clamp01(transparency * 0.72f));
            SetMaterialFloatIfPresent(material, CrystalRefractionStrengthId, Mathf.Clamp(refractionStrength + 0.03f + intensity01 * 0.018f, 0.065f, 0.12f));
            SetMaterialFloatIfPresent(material, CrystalFresnelPowerId, Mathf.Clamp(fresnelPower * 0.82f, 1.1f, 4.6f));
            SetMaterialFloatIfPresent(material, CrystalReflectionStrengthId, Mathf.Clamp01(reflectionStrength + 0.18f));
            SetMaterialFloatIfPresent(material, CrystalInternalBrightnessId, Mathf.Clamp(internalBrightness + 0.42f + intensity01 * 0.38f, 1.15f, 2.8f));
            SetMaterialFloatIfPresent(material, CrystalMinimumTransmissionId, Mathf.Clamp01(minimumTransmission + 0.16f));
            SetMaterialFloatIfPresent(material, CrystalSpecularStrengthId, Mathf.Clamp01(specularStrength + 0.18f));
            SetMaterialFloatIfPresent(material, CrystalDispersionStrengthId, 0.072f + intensity01 * 0.032f);
            SetMaterialFloatIfPresent(material, CrystalRimStrengthId, 1.02f + intensity01 * 0.28f);
            SetMaterialFloatIfPresent(material, CrystalBrightnessFloorId, 0.13f + intensity01 * 0.045f);
            SetMaterialFloatIfPresent(material, CrystalGlintStrengthId, 0.92f + intensity01 * 0.32f);
            SetMaterialFloatIfPresent(material, CrystalFacetContrastId, 0.62f + intensity01 * 0.2f);
            SetMaterialFloatIfPresent(material, CrystalInternalScatterId, 0.44f + intensity01 * 0.2f);
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        private static void ResolveCrystalSurface(CrystalMaterialMode materialMode, out float metallic, out float smoothness)
        {
            switch (materialMode)
            {
                case CrystalMaterialMode.Metal:
                    metallic = 0.32f;
                    smoothness = 0.94f;
                    break;
                case CrystalMaterialMode.FuturisticPlastic:
                    metallic = 0f;
                    smoothness = 0.86f;
                    break;
                default:
                    metallic = 0.02f;
                    smoothness = 0.97f;
                    break;
            }
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

        private static void SetMaterialTextureIfPresent(Material material, int propertyId, Texture texture)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetTexture(propertyId, texture);
            }
        }

        private static void SetMaterialFloatIfPresent(Material material, int propertyId, float value)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetFloat(propertyId, value);
            }
        }

        private static void SetMaterialColorIfPresent(Material material, int propertyId, Color value)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetColor(propertyId, value);
            }
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

            if (glintLightA != null && glintLightA.gameObject.activeSelf)
            {
                glintLightA.cullingMask = stageLayerMask;
                count++;
            }

            if (glintLightB != null && glintLightB.gameObject.activeSelf)
            {
                glintLightB.cullingMask = stageLayerMask;
                count++;
            }

            if (glintLightC != null && glintLightC.gameObject.activeSelf)
            {
                glintLightC.cullingMask = stageLayerMask;
                count++;
            }

            if (glintLightD != null && glintLightD.gameObject.activeSelf)
            {
                glintLightD.cullingMask = stageLayerMask;
                count++;
            }

            return count;
        }

        private bool ResolveValidationOrbit(CrystalSharedSettings settings, CrystalStage3DDebugMode debugMode)
        {
            return CameraBreathingEnabled
                && settings != null
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

            return TexturePropertyReferencesOutput(material, "_MainTex")
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

        private void UpdateFinalVisibleCoverageDiagnostics(Bounds? crystalBounds)
        {
            finalVisibleScreenCoverage = 0f;
            if (!crystalBounds.HasValue || crystalBounds.Value.size == Vector3.zero)
            {
                finalVisibleCoverageDiagnostics = "final visible coverage measured false, crystal bounds unavailable";
                return;
            }

            Camera finalCamera = ResolveFinalVisibleCamera();
            if (finalCamera == null)
            {
                finalVisibleCoverageDiagnostics = "final visible coverage measured false, no enabled Main/View screen camera";
                return;
            }

            Rect viewportBounds;
            if (!TryMeasureViewportBounds(finalCamera, crystalBounds.Value, out viewportBounds))
            {
                finalVisibleCoverageDiagnostics = "final visible coverage measured false, crystal outside " + finalCamera.name;
                return;
            }

            finalVisibleScreenCoverage = Mathf.Clamp01(viewportBounds.height);
            Vector2 center = viewportBounds.center;
            float centerOffset = Vector2.Distance(center, new Vector2(0.5f, 0.5f));
            bool coverageInRange = finalVisibleScreenCoverage >= MinimumCrystalScreenCoverage
                && finalVisibleScreenCoverage <= MaximumCrystalScreenCoverage;
            bool centered = centerOffset <= 0.035f;
            finalVisibleCoverageDiagnostics = "final visible coverage measured true"
                + ", final camera " + finalCamera.name
                + ", final visible screen coverage " + CrystalSpatialDiagnostics.FormatPercent(finalVisibleScreenCoverage)
                + ", target " + FormatPercentUnclamped(TargetCrystalScreenCoverage)
                + ", allowed " + FormatPercentUnclamped(MinimumCrystalScreenCoverage) + "-" + FormatPercentUnclamped(MaximumCrystalScreenCoverage)
                + ", coverage in range " + (coverageInRange ? "true" : "false")
                + ", viewport center " + center.x.ToString("0.000") + "," + center.y.ToString("0.000")
                + ", centered " + (centered ? "true" : "false");
        }

        private void ResetStaticBaselineTracking(string reason)
        {
            stabilityBaselineCaptured = false;
            stabilitySampleCount = 0;
            baselineDioramaScale = Vector3.zero;
            baselineCrystalLocalScale = Vector3.zero;
            baselineBackgroundLocalScale = Vector3.zero;
            baselineStageCameraLocalPosition = Vector3.zero;
            baselineStageViewScale = 0f;
            baselineStageCameraFieldOfView = 0f;
            baselineStageCameraOrthographicSize = 0f;
            baselineStageCameraDistance = 0f;
            minFinalVisibleCoverage = float.PositiveInfinity;
            maxFinalVisibleCoverage = float.NegativeInfinity;
            minBackgroundApparentScale = float.PositiveInfinity;
            maxBackgroundApparentScale = float.NegativeInfinity;
            staticBaselineDiagnostics = "static baseline tracking reset, reason " + reason;
        }

        private void UpdateStaticBaselineDiagnostics(Bounds? backgroundBounds, Camera camera)
        {
            if (dioramaObject == null || crystalObject == null || backgroundObject == null || camera == null)
            {
                staticBaselineDiagnostics = "static baseline sampled false, missing stage object or camera";
                return;
            }

            float cameraDistance = Vector3.Distance(camera.transform.position, root != null ? root.transform.TransformPoint(Vector3.zero) : Vector3.zero);
            if (!stabilityBaselineCaptured)
            {
                stabilityBaselineCaptured = true;
                baselineStageViewScale = stageViewScale;
                baselineDioramaScale = dioramaObject.transform.localScale;
                baselineCrystalLocalScale = crystalObject.transform.localScale;
                baselineBackgroundLocalScale = backgroundObject.transform.localScale;
                baselineStageCameraLocalPosition = camera.transform.localPosition;
                baselineStageCameraFieldOfView = camera.fieldOfView;
                baselineStageCameraOrthographicSize = camera.orthographicSize;
                baselineStageCameraDistance = cameraDistance;
            }

            float backgroundApparentScale = MeasureFinalCameraViewportHeight(backgroundBounds);
            if (finalVisibleScreenCoverage > 0f)
            {
                ExpandRange(ref minFinalVisibleCoverage, ref maxFinalVisibleCoverage, finalVisibleScreenCoverage);
            }

            if (backgroundApparentScale > 0f)
            {
                ExpandRange(ref minBackgroundApparentScale, ref maxBackgroundApparentScale, backgroundApparentScale);
            }

            stabilitySampleCount++;

            bool dioramaScaleStable = NearlyEqual(dioramaObject.transform.localScale, baselineDioramaScale);
            bool crystalLocalScaleStable = NearlyEqual(crystalObject.transform.localScale, baselineCrystalLocalScale);
            bool backgroundLocalScaleStable = NearlyEqual(backgroundObject.transform.localScale, baselineBackgroundLocalScale);
            bool stageViewScaleStable = NearlyEqual(stageViewScale, baselineStageViewScale);
            bool cameraPositionStable = NearlyEqual(camera.transform.localPosition, baselineStageCameraLocalPosition);
            bool cameraFovStable = NearlyEqual(camera.fieldOfView, baselineStageCameraFieldOfView);
            bool cameraOrthoStable = NearlyEqual(camera.orthographicSize, baselineStageCameraOrthographicSize);
            bool cameraDistanceStable = NearlyEqual(cameraDistance, baselineStageCameraDistance);
            bool crystalScaleStable = dioramaScaleStable && crystalLocalScaleStable && stageViewScaleStable && !CrystalScalePulseEnabled;
            bool backgroundScaleStable = backgroundLocalScaleStable && !BackgroundPulseEnabled && !RadialWavesEnabled;
            bool cameraFramingStable = cameraPositionStable && cameraFovStable && cameraOrthoStable && cameraDistanceStable && !CameraBreathingEnabled;

            staticBaselineDiagnostics = "static baseline locked true"
                + ", static samples " + stabilitySampleCount.ToString()
                + ", background scale stable " + (backgroundScaleStable ? "true" : "false")
                + ", crystal scale stable " + (crystalScaleStable ? "true" : "false")
                + ", camera framing stable " + (cameraFramingStable ? "true" : "false")
                + ", backgroundPulseEnabled " + (BackgroundPulseEnabled ? "true" : "false")
                + ", radialWavesEnabled " + (RadialWavesEnabled ? "true" : "false")
                + ", crystalScalePulseEnabled " + (CrystalScalePulseEnabled ? "true" : "false")
                + ", cameraBreathingEnabled " + (CameraBreathingEnabled ? "true" : "false")
                + ", animatedCoverageCorrectionEnabled " + (AnimatedCoverageCorrectionEnabled ? "true" : "false")
                + ", final coverage min/max " + FormatPercentRange(minFinalVisibleCoverage, maxFinalVisibleCoverage, stabilitySampleCount)
                + ", background apparent scale min/max " + FormatPercentRange(minBackgroundApparentScale, maxBackgroundApparentScale, stabilitySampleCount);
        }

        private float MeasureFinalCameraViewportHeight(Bounds? bounds)
        {
            if (!bounds.HasValue || bounds.Value.size == Vector3.zero)
            {
                return 0f;
            }

            Camera finalCamera = ResolveFinalVisibleCamera();
            if (finalCamera == null)
            {
                return 0f;
            }

            Rect viewportBounds;
            return TryMeasureViewportBounds(finalCamera, bounds.Value, out viewportBounds)
                ? Mathf.Max(0f, viewportBounds.height)
                : 0f;
        }

        private static void ExpandRange(ref float min, ref float max, float value)
        {
            min = Mathf.Min(min, value);
            max = Mathf.Max(max, value);
        }

        private static bool NearlyEqual(Vector3 a, Vector3 b)
        {
            return NearlyEqual(a.x, b.x) && NearlyEqual(a.y, b.y) && NearlyEqual(a.z, b.z);
        }

        private static bool NearlyEqual(float a, float b)
        {
            return Mathf.Abs(a - b) <= StableValueTolerance;
        }

        private static string FormatPercentRange(float min, float max, int sampleCount)
        {
            if (sampleCount <= 0 || float.IsInfinity(min) || float.IsInfinity(max))
            {
                return "none";
            }

            return FormatPercentUnclamped(min) + "-" + FormatPercentUnclamped(max);
        }

        private static string FormatPercentUnclamped(float value)
        {
            return (Mathf.Max(0f, value) * 100f).ToString("0.0") + "%";
        }

        private static bool TryMeasureViewportBounds(Camera camera, Bounds worldBounds, out Rect viewportBounds)
        {
            viewportBounds = new Rect(0f, 0f, 0f, 0f);
            if (camera == null || worldBounds.size == Vector3.zero)
            {
                return false;
            }

            Vector3 center = worldBounds.center;
            Vector3 extents = worldBounds.extents;
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            bool hasVisibleCorner = false;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                        Vector3 viewport = camera.WorldToViewportPoint(corner);
                        if (viewport.z <= camera.nearClipPlane)
                        {
                            continue;
                        }

                        hasVisibleCorner = true;
                        minX = Mathf.Min(minX, viewport.x);
                        maxX = Mathf.Max(maxX, viewport.x);
                        minY = Mathf.Min(minY, viewport.y);
                        maxY = Mathf.Max(maxY, viewport.y);
                    }
                }
            }

            if (!hasVisibleCorner)
            {
                return false;
            }

            viewportBounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return true;
        }

        private void UpdateDiagnostics(
            Bounds? crystalBounds,
            Bounds? backgroundBounds,
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
            string meshStats = "mesh vertices " + MeshVertexCount.ToString()
                + ", mesh triangles " + MeshTriangleCount.ToString()
                + ", hasVolume " + (HasVolume ? "true" : "false")
                + ", sideFaces " + (SideFacesDetected ? "true" : "false");
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
            string cameraOrthoSize = camera != null ? camera.orthographicSize.ToString("0.00") : "none";
            string cameraDistanceFromStage = camera != null && root != null
                ? Vector3.Distance(camera.transform.position, root.transform.TransformPoint(Vector3.zero)).ToString("0.00")
                : "none";
            string stageMask = FormatMask(stageLayerMask);
            string crystalCameraMask = camera != null ? FormatMask(camera.cullingMask) : "none";
            bool stageVisibleToNonStageCamera = IsPhysicalStageVisibleToNonStageCamera();
            bool directViewCameraCanRenderStageLayer = CanAnyDirectViewCameraRenderStageLayer();
            bool nonViewCameraCanRenderStageLayer = CanAnyNonViewCameraRenderStageLayer();
            bool crystalVisibleToDirectView = IsRendererVisibleToDirectView(crystalMeshRenderer);
            bool backgroundVisibleToDirectView = IsRendererVisibleToDirectView(backgroundMeshRenderer);
            string diagnosticsActive = diagnosticsObject != null && diagnosticsObject.activeInHierarchy ? "true" : "false";
            UpdateFinalVisibleCoverageDiagnostics(crystalBounds);
            UpdateStaticBaselineDiagnostics(backgroundBounds, camera);

            diagnosticsLabel = StageModeLabel
                + ", mode " + debugMode.ToString()
                + ", legacy composite active false"
                + ", active view path DirectPhysicalPremium3D"
                + ", SpatialCrystalStage3D active " + (stageRenderedThisFrame ? "true" : "false")
                + ", wrong background plane active false"
                + ", runtime stage scene visible " + (root != null && root.activeSelf ? "true" : "false")
                + ", writes diagnostic/offline RT " + (stageRenderedThisFrame && outputTexture != null ? "true" : "false")
                + ", stage RT " + stageTextureSize
                + ", " + sourceTextureDiagnostics
                + ", " + opticalStageDiagnostics
                + ", StageDiagnostics active " + diagnosticsActive
                + ", active cameras " + activeCameraDiagnostics
                + ", camera projection " + cameraProjection
                + ", camera FOV " + cameraFov
                + ", camera ortho size " + cameraOrthoSize
                + ", camera distance " + cameraDistanceFromStage
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
                + ", premium shape " + activePremiumShapeLabel
                + ", crystal bounds " + boundsSize
                + ", " + meshStats
                + ", screen coverage " + CrystalSpatialDiagnostics.FormatPercent(screenCoverage)
                + ", " + finalVisibleCoverageDiagnostics
                + ", " + staticBaselineDiagnostics
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
