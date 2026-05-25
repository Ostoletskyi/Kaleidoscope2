using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    [DisallowMultipleComponent]
    public sealed class DiamondFocusModule : KaleidoscopeModuleBase, IKaleidoscopeTextureProcessor
    {
        private const string CompositeShaderName = "Kaleidoscope2/DiamondComposite";
        private const string CrystalShaderName = "Kaleidoscope2/DiamondCrystal3D";
        private const string BlurShaderName = "Kaleidoscope2/DiamondBackgroundBlur";
        private const string LegacyHybridCompositeModeLabel = "Legacy DiamondFocus Hybrid Composite";

        [Header("Shaders")]
        [Tooltip("Composite shader that blends the 3D crystal render over the current kaleidoscope output.")]
        [SerializeField] private Shader diamondShader;
        [SerializeField] private Material diamondMaterial;
        [Tooltip("3D glass/crystal shader used by the module-owned offscreen crystal renderer.")]
        [SerializeField] private Shader crystalShader;
        [SerializeField] private Material crystalMaterial;
        [Tooltip("Separable Gaussian blur shader used only for the background layer.")]
        [SerializeField] private Shader blurShader;
        [SerializeField] private Material blurMaterial;
        [SerializeField, Range(0.5f, 1f)] private float renderScale = 1f;

        [Header("Background Blur")]
        [SerializeField, Range(0f, 1f)] private float blurStrength;
        [SerializeField, Range(0f, 24f)] private float maxBlurRadius = 6f;
        [SerializeField, Range(1, 6)] private int blurIterations = 2;
        [SerializeField, Range(1, 4)] private int blurDownsample = 1;

        [Header("3D Renderer")]
        [SerializeField, Range(0, 31)] private int crystalLayer = 31;
        [SerializeField, Range(0.25f, 2f)] private float orthographicSize = 0.78f;
        [SerializeField, Range(1f, 12f)] private float cameraDistance = 4f;
        [SerializeField, Range(0.25f, 3f)] private float crystalWorldScale = 1.85f;

        [Header("Crystal Source Binding")]
        [Tooltip("When enabled, the crystal shader receives the current processed kaleidoscope texture through _KaleidoscopeTex.")]
        [SerializeField] private bool bindKaleidoscopeTexture = true;
        [SerializeField] private Color missingKaleidoscopeWarningColor = new Color(1f, 0f, 0.85f, 1f);

        [Header("Module")]
        [SerializeField] private bool moduleEnabled = true;
        [SerializeField] private bool onlyIn4DMode;

        private readonly DiamondRotationController rotationController = new DiamondRotationController();
        private readonly DiamondShapeController shapeController = new DiamondShapeController();
        private readonly DiamondMaterialModeController materialModeController = new DiamondMaterialModeController();
        private readonly DiamondOpticsController opticsController = new DiamondOpticsController();
        private readonly CrystalMaterialBinder materialBinder = new CrystalMaterialBinder();

        private Material runtimeCompositeMaterial;
        private Material runtimeCrystalMaterial;
        private Material runtimeBlurMaterial;
        private RenderTexture outputTexture;
        private RenderTexture crystalTexture;
        private RenderTexture blurPingTexture;
        private RenderTexture blurPongTexture;
        private GameObject renderRoot;
        private GameObject cameraObject;
        private GameObject crystalObject;
        private Camera renderCamera;
        private MeshFilter crystalMeshFilter;
        private MeshRenderer crystalMeshRenderer;
        private DiamondFocusShape renderedShape = (DiamondFocusShape)(-1);
        private Texture2D fallbackKaleidoscopeTexture;
        private Color fallbackKaleidoscopeTextureColor = Color.clear;
        private bool missingKaleidoscopeTextureReported;
        private float lastReportedRefractionCoefficient = float.MinValue;
        private float lastRefractionReportTime = -10f;
        private float lastReportedDirectedLightIntensity = float.MinValue;
        private float lastLightReportTime = -10f;
        private float lastReportedCrystalLightRigIntensity = float.MinValue;
        private int lastReportedCrystalLightRigCount = int.MinValue;
        private float lastCrystalLightRigReportTime = -10f;
        private float lastReportedPremiumCrystalScalePercent = float.MinValue;
        private float lastPremiumCrystalScaleReportTime = -10f;
        private float lastEffectiveCrystalScale;
        private Vector3 lastCrystalLocalPosition;
        private Bounds lastCrystalBounds;
        private bool hasLastCrystalBounds;
        private float lastScreenCoverageEstimate;
        private float lastCameraCrystalDistance;
        private float lastCrystalBackgroundDistance;
        private Vector3 lastCrystalCameraLocalPosition;
        private bool lastCrystalBetweenCameraAndBackground;
        private bool lastFullscreenCompositeDominates;
        private string lastRuntimeHierarchyPath = "not rendered";

        public override string ModuleId
        {
            get { return "DiamondFocus"; }
        }

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        public override void Tick(float deltaTime)
        {
            DiamondFocusSettings settings = State != null ? State.DiamondFocusSettings : null;
            if (settings == null || !settings.Enabled || !moduleEnabled)
            {
                return;
            }

            if (onlyIn4DMode && State.ActiveVisualMode != KaleidoscopeVisualMode.Hose)
            {
                return;
            }

            shapeController.Tick(settings, deltaTime);
            rotationController.Tick(settings, deltaTime);
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Type == KaleidoscopeCommandType.SetDiamondRotationDirection
                || command.Type == KaleidoscopeCommandType.AdjustDiamondRotationSpeed
                || command.Type == KaleidoscopeCommandType.SetDiamondRotationSpeed
                || command.Type == KaleidoscopeCommandType.IncreaseDiamondRotationSpeed
                || command.Type == KaleidoscopeCommandType.DecreaseDiamondRotationSpeed
                || command.Type == KaleidoscopeCommandType.NextDiamondShape
                || command.Type == KaleidoscopeCommandType.PreviousDiamondShape
                || command.Type == KaleidoscopeCommandType.CycleDiamondMaterialMode
                || command.Type == KaleidoscopeCommandType.SetDiamondMaterialMode
                || command.Type == KaleidoscopeCommandType.CycleCrystalDebugMode
                || command.Type == KaleidoscopeCommandType.SetCrystalDebugMode
                || command.Type == KaleidoscopeCommandType.CycleCrystalDebugEffect
                || command.Type == KaleidoscopeCommandType.SetCrystalDebugEffect
                || command.Type == KaleidoscopeCommandType.AdjustDiamondRefractionIndex
                || command.Type == KaleidoscopeCommandType.SetDiamondRefractionIndex
                || command.Type == KaleidoscopeCommandType.AdjustDiamondDirectedLightIntensity
                || command.Type == KaleidoscopeCommandType.SetDiamondDirectedLightIntensity
                || command.Type == KaleidoscopeCommandType.ToggleCrystalLightRig
                || command.Type == KaleidoscopeCommandType.SetCrystalLightRigEnabled
                || command.Type == KaleidoscopeCommandType.AdjustCrystalLightRigIntensity
                || command.Type == KaleidoscopeCommandType.SetCrystalLightRigIntensity
                || command.Type == KaleidoscopeCommandType.SetCrystalLightRigActiveLightCount
                || command.Type == KaleidoscopeCommandType.AdjustPremiumCrystalScalePercent
                || command.Type == KaleidoscopeCommandType.SetPremiumCrystalScalePercent
                || command.Type == KaleidoscopeCommandType.TogglePremiumCrystalEffect
                || command.Type == KaleidoscopeCommandType.ResetPremiumCrystalOpticalControls;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            DiamondFocusSettings settings = State != null ? State.DiamondFocusSettings : null;
            if (command == null || settings == null)
            {
                return;
            }

            switch (command.Type)
            {
                case KaleidoscopeCommandType.SetDiamondRotationDirection:
                    settings.SetTargetRotationDirection(command.Vector2Value);
                    break;

                case KaleidoscopeCommandType.AdjustDiamondRotationSpeed:
                    settings.AdjustTargetRotationSpeed(command.FloatValue);
                    break;

                case KaleidoscopeCommandType.SetDiamondRotationSpeed:
                    settings.SetTargetRotationSpeed(command.FloatValue);
                    settings.SetCurrentRotationSpeed(command.FloatValue);
                    break;

                case KaleidoscopeCommandType.IncreaseDiamondRotationSpeed:
                    settings.AdjustTargetRotationSpeed(Mathf.Abs(command.FloatValue));
                    break;

                case KaleidoscopeCommandType.DecreaseDiamondRotationSpeed:
                    settings.AdjustTargetRotationSpeed(-Mathf.Abs(command.FloatValue));
                    break;

                case KaleidoscopeCommandType.NextDiamondShape:
                    shapeController.NextShape(settings);
                    break;

                case KaleidoscopeCommandType.PreviousDiamondShape:
                    shapeController.PreviousShape(settings);
                    break;

                case KaleidoscopeCommandType.CycleDiamondMaterialMode:
                    materialModeController.NextMode(settings);
                    break;

                case KaleidoscopeCommandType.SetDiamondMaterialMode:
                    materialModeController.SetMode(settings, (DiamondCrystalMaterialMode)command.IntValue);
                    break;

                case KaleidoscopeCommandType.CycleCrystalDebugMode:
                    settings.CycleDebugMode(command.IntValue);
                    ReportDebugMode(settings);
                    break;

                case KaleidoscopeCommandType.SetCrystalDebugMode:
                    settings.SetDebugMode((DiamondCrystalDebugMode)Mathf.Clamp(command.IntValue, 0, 7));
                    ReportDebugMode(settings);
                    break;

                case KaleidoscopeCommandType.CycleCrystalDebugEffect:
                case KaleidoscopeCommandType.SetCrystalDebugEffect:
                    ReportCrystalDebugEffect(settings);
                    break;

                case KaleidoscopeCommandType.AdjustDiamondRefractionIndex:
                    settings.AdjustRefractionIndex(command.FloatValue);
                    ReportRefractionCoefficient(settings);
                    break;

                case KaleidoscopeCommandType.SetDiamondRefractionIndex:
                    settings.SetRefractionIndex(command.FloatValue);
                    ReportRefractionCoefficient(settings);
                    break;

                case KaleidoscopeCommandType.AdjustDiamondDirectedLightIntensity:
                    settings.AdjustDirectedLightIntensity(command.FloatValue);
                    ReportDirectedLightIntensity(settings);
                    break;

                case KaleidoscopeCommandType.SetDiamondDirectedLightIntensity:
                    settings.SetDirectedLightIntensity(command.FloatValue);
                    ReportDirectedLightIntensity(settings);
                    break;

                case KaleidoscopeCommandType.ToggleCrystalLightRig:
                case KaleidoscopeCommandType.SetCrystalLightRigEnabled:
                case KaleidoscopeCommandType.AdjustCrystalLightRigIntensity:
                case KaleidoscopeCommandType.SetCrystalLightRigIntensity:
                case KaleidoscopeCommandType.SetCrystalLightRigActiveLightCount:
                    ReportCrystalLightRig(settings);
                    break;

                case KaleidoscopeCommandType.AdjustPremiumCrystalScalePercent:
                case KaleidoscopeCommandType.SetPremiumCrystalScalePercent:
                    ReportPremiumCrystalScale(settings);
                    break;

                case KaleidoscopeCommandType.TogglePremiumCrystalEffect:
                    settings.TogglePremiumCrystalEffect((PremiumCrystalEffectToggle)command.IntValue);
                    ReportPremiumCrystalEffects(settings);
                    break;

                case KaleidoscopeCommandType.ResetPremiumCrystalOpticalControls:
                    settings.ResetPremiumCrystalOpticalControls();
                    ReportPremiumCrystalEffects(settings);
                    break;
            }
        }

        public Texture Process(Texture sourceTexture, KaleidoscopeState runtimeState)
        {
            if (runtimeState == null)
            {
                SetRuntimeRendererVisible(false);
                return sourceTexture;
            }

            DiamondFocusSettings settings = runtimeState.DiamondFocusSettings;
            if (!moduleEnabled
                || settings == null
                || !settings.Enabled
                || (onlyIn4DMode && runtimeState.ActiveVisualMode != KaleidoscopeVisualMode.Hose))
            {
                SetRuntimeRendererVisible(false);
                lastFullscreenCompositeDominates = false;
                return sourceTexture;
            }

            if (settings.CrystalSimulationMode == CrystalRenderMode.RealMesh3D)
            {
                SetRuntimeRendererVisible(false);
                lastFullscreenCompositeDominates = false;
                return sourceTexture;
            }

            bool kaleidoscopeTextureValid;
            Texture kaleidoscopeTexture = ResolveKaleidoscopeTexture(sourceTexture, out kaleidoscopeTextureValid);
            settings.SetKaleidoscopeTexBindingStatus(kaleidoscopeTextureValid);
            settings.RefreshInspectorLabels();

            Material compositeMaterial = EnsureCompositeMaterial();
            Material glassMaterial = EnsureCrystalMaterial();
            Material activeBlurMaterial = EnsureBlurMaterial();
            if (compositeMaterial == null || glassMaterial == null || activeBlurMaterial == null)
            {
                return sourceTexture != null ? sourceTexture : kaleidoscopeTexture;
            }

            EnsureOutputTexture(runtimeState);
            if (outputTexture == null || !RenderCrystal(kaleidoscopeTexture, kaleidoscopeTextureValid, settings, glassMaterial))
            {
                return sourceTexture != null ? sourceTexture : kaleidoscopeTexture;
            }

            float backgroundBlurAmount = ResolveBackgroundBlurAmount(settings);
            Texture backgroundTexture = kaleidoscopeTextureValid
                ? RenderBackgroundBlur(kaleidoscopeTexture, backgroundBlurAmount, activeBlurMaterial)
                : kaleidoscopeTexture;
            opticsController.ConfigureCompositeMaterial(compositeMaterial, settings, kaleidoscopeTexture, backgroundTexture, crystalTexture, backgroundBlurAmount);
            Graphics.Blit(kaleidoscopeTexture, outputTexture, compositeMaterial);
            return outputTexture;
        }

        public override void Validate()
        {
            if (diamondShader == null && Shader.Find(CompositeShaderName) == null)
            {
                ReportMissingReference("Shader (" + CompositeShaderName + ")");
            }

            if (crystalShader == null && Shader.Find(CrystalShaderName) == null)
            {
                ReportMissingReference("Shader (" + CrystalShaderName + ")");
            }

            if (blurShader == null && Shader.Find(BlurShaderName) == null)
            {
                ReportMissingReference("Shader (" + BlurShaderName + ")");
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            DiamondFocusSettings settings = State != null ? State.DiamondFocusSettings : null;
            if (!moduleEnabled)
            {
                return CreateStatus("Disabled by module switch.");
            }

            if (settings == null)
            {
                return CreateStatus("Waiting for diamond settings.");
            }

            string enabled = settings.Enabled ? "enabled" : "disabled";
            string mode = onlyIn4DMode
                ? State != null && State.ActiveVisualMode == KaleidoscopeVisualMode.Hose ? "active in 4D" : "pass-through outside 4D"
                : "active in all visual modes";
            string direction = settings.CurrentRotationDirection.ToString("0.00");
            string speed = settings.CurrentRotationSpeed.ToString("0");
            string normalized = settings.NormalizedRotationSpeed.ToString("0.00");
            string backgroundBlur = opticsController.BackgroundBlur.ToString("0.00");
            string sourceBinding = settings.KaleidoscopeTexBindingStatus;
            CrystalLightRigSettings lightRig = settings.CrystalLightRigSettings;
            string profileName = materialBinder.LastProfile.Surface.ModeName;
            if (string.IsNullOrEmpty(profileName))
            {
                profileName = materialModeController.GetModeLabel(settings);
            }

            string opticsStatus = (diamondMaterial != null || crystalMaterial != null || blurMaterial != null)
                ? "assigned material"
                : (runtimeCompositeMaterial != null && runtimeCrystalMaterial != null && runtimeBlurMaterial != null)
                    ? "runtime 3D materials"
                    : (diamondShader != null || crystalShader != null || blurShader != null)
                        ? "assigned shader"
                        : "shader lookup";
            string crystalPosition = lastCrystalLocalPosition.ToString("F2");
            string crystalBounds = hasLastCrystalBounds
                ? "center " + lastCrystalBounds.center.ToString("F2") + ", size " + lastCrystalBounds.size.ToString("F2")
                : "not rendered";
            string spatialStatus = "render mode " + LegacyHybridCompositeModeLabel
                + ", legacy composite active " + (settings.CrystalSimulationMode != CrystalRenderMode.RealMesh3D ? "true" : "false")
                + ", crystal stage active false"
                + ", screen coverage " + CrystalSpatialDiagnostics.FormatPercent(lastScreenCoverageEstimate)
                + ", effective scale " + lastEffectiveCrystalScale.ToString("0.00")
                + ", configured world scale " + Mathf.Max(0.05f, crystalWorldScale).ToString("0.00")
                + ", crystal local position " + crystalPosition
                + ", camera local position " + CrystalSpatialDiagnostics.FormatVector(lastCrystalCameraLocalPosition)
                + ", camera-crystal distance " + lastCameraCrystalDistance.ToString("0.00")
                + ", crystal-background distance " + (lastCrystalBackgroundDistance > 0f ? lastCrystalBackgroundDistance.ToString("0.00") : "none")
                + ", background spatial false"
                + ", crystal between camera/background " + (lastCrystalBetweenCameraAndBackground ? "true" : "false")
                + ", fullscreen composite dominates " + (lastFullscreenCompositeDominates ? "true" : "false")
                + ", hierarchy " + lastRuntimeHierarchyPath
                + ", bounds " + crystalBounds;

            return CreateStatus(enabled
                + ", " + mode
                + ", shape " + shapeController.GetShapeLabel(settings)
                + ", material " + materialModeController.GetModeLabel(settings)
                + ", profile " + profileName
                + ", simulation " + settings.CrystalSimulationModeLabel
                + ", Premium3D controls " + settings.PremiumCrystalControlStatus
                + ", Premium3D effects " + settings.PremiumCrystalEffectStatus
                + ", dir " + direction
                + ", speed " + speed
                + ", RefractionCoefficient " + settings.RefractionCoefficient.ToString("0.00")
                + ", DirectedLightIntensity " + settings.DirectedLightIntensity.ToString("0.00")
                + ", LightRig " + (lightRig.RigEnabled ? "on" : "off")
                + " " + lightRig.LightIntensity.ToString("0.00") + "/" + settings.ActiveCrystalBrightnessMax.ToString("0.00")
                + " glint " + lightRig.ResolvedGlintIntensity.ToString("0.00")
                + " spectral " + lightRig.ResolvedSpectralIntensity.ToString("0.00")
                + ", normalized " + normalized
                + ", Background Blur " + backgroundBlur + " (radius " + maxBlurRadius.ToString("0.0") + ", iterations " + Mathf.Max(1, blurIterations) + ", downsample " + Mathf.Max(1, blurDownsample) + ")"
                + ", cinematic optics " + settings.CinematicCrystalOptics.ToString("0.00")
                + ", caustics " + settings.HighEnergyCaustics.ToString("0.00")
                + ", debug " + settings.DebugModeLabel
                + ", source " + sourceBinding
                + ", variants " + (settings.EnableRandomVariants ? "on" : "off")
                + ", preserve classic " + (settings.PreserveClassicMode ? "on" : "off")
                + ", " + spatialStatus
                + ", optics " + opticsStatus + ".");
        }

        private void OnDestroy()
        {
            ReleaseOutputTexture();
            ReleaseCrystalTexture();
            ReleaseBlurTextures();
            shapeController.Release();
            DestroyRuntimeObject(renderRoot);
            DestroyRuntimeObject(runtimeCompositeMaterial);
            DestroyRuntimeObject(runtimeCrystalMaterial);
            DestroyRuntimeObject(runtimeBlurMaterial);
            DestroyRuntimeObject(fallbackKaleidoscopeTexture);

            renderRoot = null;
            cameraObject = null;
            crystalObject = null;
            renderCamera = null;
            crystalMeshFilter = null;
            crystalMeshRenderer = null;
            runtimeCompositeMaterial = null;
            runtimeCrystalMaterial = null;
            runtimeBlurMaterial = null;
            fallbackKaleidoscopeTexture = null;
        }

        private Material EnsureCompositeMaterial()
        {
            if (diamondMaterial != null)
            {
                return diamondMaterial;
            }

            if (runtimeCompositeMaterial != null)
            {
                return runtimeCompositeMaterial;
            }

            Shader shader = diamondShader != null ? diamondShader : Shader.Find(CompositeShaderName);
            if (shader == null)
            {
                ReportMissingReference("Shader (" + CompositeShaderName + ")");
                return null;
            }

            runtimeCompositeMaterial = new Material(shader)
            {
                name = "Kaleidoscope2_DiamondFocus_CompositeMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };

            return runtimeCompositeMaterial;
        }

        private Material EnsureCrystalMaterial()
        {
            if (crystalMaterial != null)
            {
                return crystalMaterial;
            }

            if (runtimeCrystalMaterial != null)
            {
                return runtimeCrystalMaterial;
            }

            Shader shader = crystalShader != null ? crystalShader : Shader.Find(CrystalShaderName);
            if (shader == null)
            {
                ReportMissingReference("Shader (" + CrystalShaderName + ")");
                return null;
            }

            runtimeCrystalMaterial = new Material(shader)
            {
                name = "Kaleidoscope2_DiamondFocus_CrystalMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };

            return runtimeCrystalMaterial;
        }

        private Material EnsureBlurMaterial()
        {
            if (blurMaterial != null)
            {
                return blurMaterial;
            }

            if (runtimeBlurMaterial != null)
            {
                return runtimeBlurMaterial;
            }

            Shader shader = blurShader != null ? blurShader : Shader.Find(BlurShaderName);
            if (shader == null)
            {
                ReportMissingReference("Shader (" + BlurShaderName + ")");
                return null;
            }

            runtimeBlurMaterial = new Material(shader)
            {
                name = "Kaleidoscope2_DiamondFocus_BackgroundBlurMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };

            return runtimeBlurMaterial;
        }

        private void EnsureOutputTexture(KaleidoscopeState runtimeState)
        {
            CameraSettings settings = runtimeState.CameraSettings;
            int baseWidth = settings != null ? settings.RenderWidth : 1920;
            int baseHeight = settings != null ? settings.RenderHeight : 1080;
            float scale = Mathf.Clamp(renderScale, 0.5f, 1f);
            int width = Mathf.Max(1, Mathf.RoundToInt(baseWidth * scale));
            int height = Mathf.Max(1, Mathf.RoundToInt(baseHeight * scale));

            if (outputTexture != null && outputTexture.width == width && outputTexture.height == height)
            {
                return;
            }

            ReleaseOutputTexture();

            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0)
            {
                msaaSamples = 1,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
                useMipMap = false,
                autoGenerateMips = false
            };

            outputTexture = new RenderTexture(descriptor)
            {
                name = "Kaleidoscope2_DiamondFocus_Output",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            outputTexture.Create();
        }

        private void EnsureCrystalTexture()
        {
            if (outputTexture == null)
            {
                ReleaseCrystalTexture();
                return;
            }

            if (crystalTexture != null && crystalTexture.width == outputTexture.width && crystalTexture.height == outputTexture.height)
            {
                return;
            }

            ReleaseCrystalTexture();

            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(outputTexture.width, outputTexture.height, RenderTextureFormat.ARGB32, 24)
            {
                msaaSamples = 1,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
                useMipMap = false,
                autoGenerateMips = false
            };

            crystalTexture = new RenderTexture(descriptor)
            {
                name = "Kaleidoscope2_DiamondFocus_3DCrystal",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            crystalTexture.Create();
        }

        private void EnsureRenderer()
        {
            int layer = Mathf.Clamp(crystalLayer, 0, 31);

            if (renderRoot == null)
            {
                renderRoot = new GameObject("Kaleidoscope2_DiamondFocus_RuntimeRenderer")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                renderRoot.transform.SetParent(transform, false);
            }

            if (cameraObject == null)
            {
                cameraObject = new GameObject("CrystalRenderCamera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                cameraObject.transform.SetParent(renderRoot.transform, false);
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

            if (renderCamera != null)
            {
                renderCamera.enabled = false;
                renderCamera.clearFlags = CameraClearFlags.SolidColor;
                renderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                renderCamera.orthographic = true;
                renderCamera.nearClipPlane = 0.01f;
                renderCamera.allowHDR = false;
                renderCamera.allowMSAA = false;
            }

            if (crystalObject == null)
            {
                crystalObject = new GameObject("VolumetricCrystalMesh")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                crystalObject.transform.SetParent(renderRoot.transform, false);
                crystalMeshFilter = crystalObject.AddComponent<MeshFilter>();
                crystalMeshRenderer = crystalObject.AddComponent<MeshRenderer>();
            }

            if (crystalMeshFilter == null)
            {
                crystalMeshFilter = crystalObject.GetComponent<MeshFilter>();
                if (crystalMeshFilter == null)
                {
                    crystalMeshFilter = crystalObject.AddComponent<MeshFilter>();
                }
            }

            if (crystalMeshRenderer == null)
            {
                crystalMeshRenderer = crystalObject.GetComponent<MeshRenderer>();
                if (crystalMeshRenderer == null)
                {
                    crystalMeshRenderer = crystalObject.AddComponent<MeshRenderer>();
                }
            }

            if (crystalMeshRenderer != null)
            {
                crystalMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                crystalMeshRenderer.receiveShadows = false;
            }

            renderRoot.layer = layer;
            cameraObject.layer = layer;
            crystalObject.layer = layer;

            renderCamera.cullingMask = 1 << layer;
            renderCamera.orthographicSize = Mathf.Max(0.05f, orthographicSize);
            renderCamera.farClipPlane = Mathf.Max(2f, cameraDistance + 4f);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -Mathf.Max(1f, cameraDistance));
            cameraObject.transform.localRotation = Quaternion.identity;
            SetRuntimeRendererVisible(false);
        }

        private bool RenderCrystal(Texture kaleidoscopeTexture, bool kaleidoscopeTextureValid, DiamondFocusSettings settings, Material material)
        {
            if (kaleidoscopeTexture == null || settings == null || material == null)
            {
                return false;
            }

            EnsureCrystalTexture();
            EnsureRenderer();

            if (crystalTexture == null || renderCamera == null || crystalMeshFilter == null || crystalMeshRenderer == null)
            {
                SetRuntimeRendererVisible(false);
                return false;
            }

            Mesh mesh = shapeController.GetMesh(settings);
            if (mesh == null)
            {
                SetRuntimeRendererVisible(false);
                return false;
            }

            if (renderedShape != settings.Shape || crystalMeshFilter.sharedMesh != mesh)
            {
                crystalMeshFilter.sharedMesh = mesh;
                renderedShape = settings.Shape;
            }

            float desiredCoverage = CrystalSpatialDiagnostics.ClampRealMeshCoverage(settings.ScreenScale);
            float targetHalfHeight = Mathf.Max(0.05f, renderCamera.orthographicSize * desiredCoverage);

            Bounds meshBounds = mesh.bounds;
            float verticalExtent = Mathf.Max(0.001f, meshBounds.extents.y);
            float scale = Mathf.Clamp(targetHalfHeight / verticalExtent, 0.05f, 8f);

            crystalObject.transform.localPosition = Vector3.zero;
            crystalObject.transform.localRotation = Quaternion.Euler(settings.RotationEuler);
            crystalObject.transform.localScale = new Vector3(scale, scale, scale);
            if (renderCamera != null)
            {
                renderCamera.transform.LookAt(renderRoot.transform.position, Vector3.up);
            }

            lastEffectiveCrystalScale = scale;
            lastCrystalLocalPosition = crystalObject.transform.localPosition;
            Vector3 scaledSize = Vector3.Scale(meshBounds.size, crystalObject.transform.localScale);
            lastCrystalBounds = new Bounds(crystalObject.transform.localPosition + Vector3.Scale(meshBounds.center, crystalObject.transform.localScale), scaledSize);
            hasLastCrystalBounds = true;
            lastScreenCoverageEstimate = CrystalSpatialDiagnostics.OrthographicHeightCoverage(scaledSize.y, renderCamera.orthographicSize);
            lastCrystalCameraLocalPosition = cameraObject != null ? cameraObject.transform.localPosition : Vector3.zero;
            lastCameraCrystalDistance = renderCamera != null
                ? Vector3.Distance(renderCamera.transform.position, crystalObject.transform.position)
                : 0f;
            lastCrystalBackgroundDistance = 0f;
            lastCrystalBetweenCameraAndBackground = false;
            lastFullscreenCompositeDominates = true;
            lastRuntimeHierarchyPath = CrystalSpatialDiagnostics.GetHierarchyPath(crystalObject.transform);
            crystalMeshRenderer.sharedMaterial = material;
            materialBinder.ConfigureCrystalMaterial(material, settings, kaleidoscopeTexture, kaleidoscopeTextureValid, missingKaleidoscopeWarningColor);

            SetRuntimeRendererVisible(true);
            RenderTexture previousTarget = renderCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                renderCamera.targetTexture = crystalTexture;
                renderCamera.Render();
            }
            finally
            {
                renderCamera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                SetRuntimeRendererVisible(false);
            }

            return true;
        }

        private void SetRuntimeRendererVisible(bool visible)
        {
            if (renderRoot != null && renderRoot.activeSelf != visible)
            {
                renderRoot.SetActive(visible);
            }

            if (cameraObject != null && cameraObject.activeSelf != visible)
            {
                cameraObject.SetActive(visible);
            }

            if (crystalObject != null && crystalObject.activeSelf != visible)
            {
                crystalObject.SetActive(visible);
            }

            if (renderCamera != null)
            {
                renderCamera.enabled = false;
            }
        }

        private void ReportRefractionCoefficient(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            float value = settings.RefractionCoefficient;
            float now = Application.isPlaying ? Time.unscaledTime : 0f;
            if (Mathf.Abs(value - lastReportedRefractionCoefficient) < 0.05f && now - lastRefractionReportTime < 0.5f)
            {
                return;
            }

            lastReportedRefractionCoefficient = value;
            lastRefractionReportTime = now;
            Debug.Log("[DiamondFocusModule] RefractionCoefficient " + value.ToString("0.00") + " / 10 mapped to safe crystal shader optics.", this);
        }

        private void ReportDirectedLightIntensity(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            float value = settings.DirectedLightIntensity;
            float now = Application.isPlaying ? Time.unscaledTime : 0f;
            if (Mathf.Abs(value - lastReportedDirectedLightIntensity) < 0.05f && now - lastLightReportTime < 0.5f)
            {
                return;
            }

            lastReportedDirectedLightIntensity = value;
            lastLightReportTime = now;
            Debug.Log("[DiamondFocusModule] DirectedLightIntensity " + value.ToString("0.00") + " / +/-10 applied to crystal glints and internal reflection.", this);
        }

        private void ReportCrystalLightRig(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            CrystalLightRigSettings lightRig = settings.CrystalLightRigSettings;
            float value = lightRig.LightIntensity;
            int activeLightLimit = lightRig.ActiveLightCountLimit;
            float now = Application.isPlaying ? Time.unscaledTime : 0f;
            if (Mathf.Abs(value - lastReportedCrystalLightRigIntensity) < 0.05f
                && activeLightLimit == lastReportedCrystalLightRigCount
                && now - lastCrystalLightRigReportTime < 0.5f)
            {
                return;
            }

            lastReportedCrystalLightRigIntensity = value;
            lastReportedCrystalLightRigCount = activeLightLimit;
            lastCrystalLightRigReportTime = now;
            Debug.Log("[DiamondFocusModule] CrystalLightRig "
                + (lightRig.RigEnabled ? "enabled" : "disabled")
                + ", light intensity " + value.ToString("0.00") + " / " + settings.ActiveCrystalBrightnessMax.ToString("0.00")
                + ", old brightness min/max " + DiamondFocusSettings.OldCrystalBrightnessMin.ToString("0.00") + "/" + DiamondFocusSettings.OldCrystalBrightnessMax.ToString("0.00")
                + ", new brightness min/max " + DiamondFocusSettings.PremiumCrystalBrightnessMin.ToString("0.00") + "/" + DiamondFocusSettings.PremiumCrystalBrightnessMax.ToString("0.00")
                + ", active light limit " + activeLightLimit.ToString() + ".", this);
        }

        private void ReportPremiumCrystalScale(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            float value = settings.PremiumCrystalScalePercent;
            float now = Application.isPlaying ? Time.unscaledTime : 0f;
            if (Mathf.Abs(value - lastReportedPremiumCrystalScalePercent) < 0.05f && now - lastPremiumCrystalScaleReportTime < 0.5f)
            {
                return;
            }

            lastReportedPremiumCrystalScalePercent = value;
            lastPremiumCrystalScaleReportTime = now;
            Debug.Log("[DiamondFocusModule] Premium3D crystal scale "
                + value.ToString("0") + "%, range "
                + DiamondFocusSettings.PremiumCrystalScalePercentMin.ToString("0") + "-"
                + DiamondFocusSettings.PremiumCrystalScalePercentMax.ToString("0") + "%.", this);
        }

        private void ReportPremiumCrystalEffects(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            Debug.Log("[DiamondFocusModule] Premium3D effects " + settings.PremiumCrystalEffectStatus + ".", this);
        }

        private void ReportDebugMode(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            Debug.Log("[DiamondFocusModule] Diamond Focus debug mode " + settings.DebugModeLabel + " (" + settings.DebugMode + ").", this);
        }

        private void ReportCrystalDebugEffect(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            Debug.Log("[DiamondFocusModule] Shared crystal debug effect " + settings.CrystalDebugEffectLabel + ".", this);
        }

        private Texture ResolveKaleidoscopeTexture(Texture sourceTexture, out bool textureValid)
        {
            textureValid = bindKaleidoscopeTexture && sourceTexture != null;
            if (textureValid)
            {
                missingKaleidoscopeTextureReported = false;
                return sourceTexture;
            }

            if (!missingKaleidoscopeTextureReported)
            {
                ReportWarning("Missing or disabled _KaleidoscopeTex. Rendering crystal fallback warning color.");
                missingKaleidoscopeTextureReported = true;
            }

            return EnsureFallbackKaleidoscopeTexture();
        }

        private Texture2D EnsureFallbackKaleidoscopeTexture()
        {
            if (fallbackKaleidoscopeTexture == null)
            {
                fallbackKaleidoscopeTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    name = "Kaleidoscope2_DiamondFocus_MissingKaleidoscopeTex",
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
            }

            if (fallbackKaleidoscopeTextureColor != missingKaleidoscopeWarningColor)
            {
                Color dark = new Color(0.03f, 0f, 0.04f, 1f);
                fallbackKaleidoscopeTexture.SetPixels(new[]
                {
                    missingKaleidoscopeWarningColor,
                    dark,
                    dark,
                    missingKaleidoscopeWarningColor
                });
                fallbackKaleidoscopeTexture.Apply(false, false);
                fallbackKaleidoscopeTextureColor = missingKaleidoscopeWarningColor;
            }

            return fallbackKaleidoscopeTexture;
        }

        private Texture RenderBackgroundBlur(Texture sourceTexture, float backgroundBlurAmount, Material material)
        {
            if (sourceTexture == null || material == null || backgroundBlurAmount <= 0.001f)
            {
                return sourceTexture;
            }

            EnsureBlurTextures(sourceTexture);
            if (blurPingTexture == null || blurPongTexture == null)
            {
                return sourceTexture;
            }

            int iterations = Mathf.Max(1, blurIterations);
            float radius = Mathf.Max(0f, maxBlurRadius) * backgroundBlurAmount;
            if (radius <= 0.001f)
            {
                return sourceTexture;
            }

            Texture current = sourceTexture;
            float iterationRadius = radius / iterations;
            for (int index = 0; index < iterations; index++)
            {
                float passRadius = iterationRadius * (index + 1f);
                opticsController.ConfigureBlurMaterial(material, current, Vector2.right, passRadius);
                Graphics.Blit(current, blurPingTexture, material, 0);

                opticsController.ConfigureBlurMaterial(material, blurPingTexture, Vector2.up, passRadius);
                Graphics.Blit(blurPingTexture, blurPongTexture, material, 0);
                current = blurPongTexture;
            }

            return current;
        }

        private float ResolveBackgroundBlurAmount(DiamondFocusSettings settings)
        {
            if (settings == null || maxBlurRadius <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(settings.NormalizedRotationSpeed + Mathf.Clamp01(blurStrength));
        }

        private void ReleaseOutputTexture()
        {
            ReleaseRenderTexture(ref outputTexture);
        }

        private void ReleaseCrystalTexture()
        {
            ReleaseRenderTexture(ref crystalTexture);
        }

        private void EnsureBlurTextures(Texture sourceTexture)
        {
            if (sourceTexture == null)
            {
                ReleaseBlurTextures();
                return;
            }

            int downsample = Mathf.Max(1, blurDownsample);
            int width = Mathf.Max(1, sourceTexture.width / downsample);
            int height = Mathf.Max(1, sourceTexture.height / downsample);

            if (blurPingTexture != null
                && blurPongTexture != null
                && blurPingTexture.width == width
                && blurPingTexture.height == height
                && blurPongTexture.width == width
                && blurPongTexture.height == height)
            {
                return;
            }

            ReleaseBlurTextures();

            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0)
            {
                msaaSamples = 1,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
                useMipMap = false,
                autoGenerateMips = false
            };

            blurPingTexture = CreateBlurTexture(descriptor, "Kaleidoscope2_DiamondFocus_BlurPing");
            blurPongTexture = CreateBlurTexture(descriptor, "Kaleidoscope2_DiamondFocus_BlurPong");
        }

        private RenderTexture CreateBlurTexture(RenderTextureDescriptor descriptor, string textureName)
        {
            RenderTexture texture = new RenderTexture(descriptor)
            {
                name = textureName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.Create();
            return texture;
        }

        private void ReleaseBlurTextures()
        {
            ReleaseRenderTexture(ref blurPingTexture);
            ReleaseRenderTexture(ref blurPongTexture);
        }

        private void ReleaseRenderTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            if (texture.IsCreated())
            {
                texture.Release();
            }

            DestroyRuntimeObject(texture);
            texture = null;
        }

        private void DestroyRuntimeObject(Object instance)
        {
            if (instance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(instance);
            }
            else
            {
                DestroyImmediate(instance);
            }
        }
    }
}
