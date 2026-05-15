using Kaleidoscope2.Core;
using Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    [DisallowMultipleComponent]
    public sealed class CrystalPresentationModule : KaleidoscopeModuleBase, IKaleidoscopeTextureProcessor
    {
        private const string BillboardShaderName = "Kaleidoscope2/BillboardCrystal";
        private const string RealMeshShaderName = "Kaleidoscope2/RealCrystalOptics";

        [Header("Shaders")]
        [SerializeField] private Shader billboardShader;
        [SerializeField] private Shader realMeshShader;

        [Header("Crystal Presentation")]
        [SerializeField] private bool moduleEnabled = true;
        [SerializeField, Range(0, 31)] private int crystalLayer = 31;
        [SerializeField] private CrystalSharedSettings sharedSettings = new CrystalSharedSettings();

        [Header("RealMesh Diagnostics")]
        [SerializeField] private bool useRealMeshSolidGeometryValidationMaterial;
        [SerializeField] private CrystalStage3DDebugMode realMeshStageDebugMode = CrystalStage3DDebugMode.FinalPremiumComposite;

        private BillboardCrystalRenderer billboardRenderer;
        private RealMeshCrystalRenderer realMeshRenderer;
        private ICrystalRenderer activeRenderer;
        private CrystalRenderMode activeRenderMode = (CrystalRenderMode)(-1);
        private RenderTexture sourceCopyTexture;
        private Texture outputTexture;
        private bool realMeshScaleMigrationReported;

        public override string ModuleId
        {
            get { return "CrystalPresentation"; }
        }

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        public override void Validate()
        {
            if (billboardShader == null && Shader.Find(BillboardShaderName) == null)
            {
                ReportMissingReference("Shader (" + BillboardShaderName + ")");
            }

            if (realMeshShader == null && Shader.Find(RealMeshShaderName) == null)
            {
                ReportMissingReference("Shader (" + RealMeshShaderName + ")");
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            string mode = sharedSettings != null
                ? CrystalSharedSettings.GetRenderModeLabel(sharedSettings.RenderMode)
                : "Unknown";
            int activeLights = realMeshRenderer != null ? realMeshRenderer.ActiveLightCount : 0;
            string source = sharedSettings != null ? sharedSettings.SourceTextureStatus : "Unbound";
            string active = activeRenderer != null
                ? CrystalSharedSettings.GetRenderModeLabel(activeRenderMode)
                : "None";
            string billboardState = billboardRenderer != null ? billboardRenderer.LifecycleState : "not created";
            string realMeshState = realMeshRenderer != null ? realMeshRenderer.LifecycleState : "not created";
            string realMeshVolume = realMeshRenderer != null
                ? "hasVolume " + (realMeshRenderer.HasVolume ? "true" : "false")
                    + ", sideFacesDetected " + (realMeshRenderer.SideFacesDetected ? "true" : "false")
                : "hasVolume unknown";
            string realMeshPlacement = sharedSettings != null
                ? sharedSettings.RealMeshPlacementStatus
                : "scale unknown";
            string realMeshDiagnostics = realMeshRenderer != null
                ? realMeshRenderer.StageDiagnosticsLabel
                : "stage diagnostics not created";
            int runtimeObjects = 0;
            if (billboardRenderer != null)
            {
                runtimeObjects += billboardRenderer.RuntimeObjectCount;
            }

            if (realMeshRenderer != null)
            {
                runtimeObjects += realMeshRenderer.RuntimeObjectCount;
            }

            return CreateStatus((moduleEnabled ? "enabled" : "disabled")
                + ", mode " + mode
                + ", active renderer " + active
                + ", source " + source
                + ", billboard " + billboardState
                + ", real mesh " + realMeshState
                + ", " + realMeshVolume
                + ", " + realMeshPlacement
                + ", " + realMeshDiagnostics
                + ", runtime objects " + runtimeObjects.ToString()
                + ", real mesh lights " + activeLights.ToString() + ".");
        }

        public Texture Process(Texture sourceTexture, KaleidoscopeState runtimeState)
        {
            if (!moduleEnabled || runtimeState == null)
            {
                outputTexture = sourceTexture;
                DeactivateAllRenderers(false);
                return sourceTexture;
            }

            DiamondFocusSettings diamondSettings = runtimeState.DiamondFocusSettings;
            if (sharedSettings == null)
            {
                sharedSettings = new CrystalSharedSettings();
            }

            sharedSettings.SyncFromDiamond(diamondSettings);
            ReportRealMeshScaleMigrationIfNeeded();
            sharedSettings.SetSourceTextureStatus(sourceTexture != null);

            if (diamondSettings == null || !diamondSettings.Enabled || sourceTexture == null)
            {
                outputTexture = sourceTexture;
                DeactivateAllRenderers(false);
                return sourceTexture;
            }

            RenderTexture sourceRenderTexture = ResolveSourceRenderTexture(sourceTexture);
            if (sourceRenderTexture == null)
            {
                outputTexture = sourceTexture;
                DeactivateAllRenderers(false);
                return sourceTexture;
            }

            ICrystalRenderer renderer = ResolveRenderer(sharedSettings.RenderMode);
            if (renderer == null)
            {
                outputTexture = sourceTexture;
                return sourceTexture;
            }

            if (realMeshRenderer != null)
            {
                realMeshRenderer.SetGeometryValidationMaterialEnabled(useRealMeshSolidGeometryValidationMaterial || sharedSettings.UseSolidGeometryValidationMaterial);
                realMeshRenderer.SetStageDebugMode(realMeshStageDebugMode);
            }

            DeactivateAllRendererRoots();
            ApplyRendererState(renderer, sourceRenderTexture, sharedSettings);
            renderer.Render();
            outputTexture = ResolveOutputTexture(renderer, sourceTexture);
            return outputTexture;
        }

        private ICrystalRenderer ResolveRenderer(CrystalRenderMode renderMode)
        {
            ICrystalRenderer renderer = EnsureRenderer(renderMode);

            if (!ReferenceEquals(activeRenderer, renderer))
            {
                DeactivateRenderer(activeRenderer, true);
                activeRenderer = renderer;
                activeRenderMode = renderMode;
                activeRenderer.Initialize(sharedSettings);
            }

            return renderer;
        }

        private ICrystalRenderer EnsureRenderer(CrystalRenderMode renderMode)
        {
            if (renderMode == CrystalRenderMode.RealMesh3D)
            {
                if (realMeshRenderer == null)
                {
                    realMeshRenderer = new RealMeshCrystalRenderer(transform, realMeshShader, crystalLayer);
                }

                return realMeshRenderer;
            }

            if (billboardRenderer == null)
            {
                billboardRenderer = new BillboardCrystalRenderer(transform, billboardShader);
            }

            return billboardRenderer;
        }

        private void DeactivateAllRendererRoots()
        {
            if (billboardRenderer != null)
            {
                billboardRenderer.SetVisible(false);
            }

            if (realMeshRenderer != null)
            {
                realMeshRenderer.SetVisible(false);
            }
        }

        private void ReportRealMeshScaleMigrationIfNeeded()
        {
            if (sharedSettings == null)
            {
                return;
            }

            string migrationMessage;
            bool migrated = sharedSettings.EnsureRealMeshProductionDefaults(out migrationMessage);
            if (!migrated || realMeshScaleMigrationReported)
            {
                return;
            }

            realMeshScaleMigrationReported = true;
            Debug.Log("[CrystalPresentationModule] " + migrationMessage, this);
        }

        private static void ApplyRendererState(
            ICrystalRenderer renderer,
            RenderTexture sourceRenderTexture,
            CrystalSharedSettings settings)
        {
            renderer.SetSourceTexture(sourceRenderTexture);
            renderer.SetShape(settings.Shape);
            renderer.SetMaterialMode(settings.MaterialMode);
            renderer.SetRotation(settings.Rotation);
            renderer.SetIntensity(settings.Intensity);
            renderer.SetVisible(settings.Visible);
        }

        private Texture ResolveOutputTexture(ICrystalRenderer renderer, Texture fallback)
        {
            BillboardCrystalRenderer billboard = renderer as BillboardCrystalRenderer;
            if (billboard != null && billboard.OutputTexture != null)
            {
                return billboard.OutputTexture;
            }

            RealMeshCrystalRenderer realMesh = renderer as RealMeshCrystalRenderer;
            if (realMesh != null && realMesh.OutputTexture != null)
            {
                return realMesh.OutputTexture;
            }

            return fallback;
        }

        private RenderTexture ResolveSourceRenderTexture(Texture sourceTexture)
        {
            RenderTexture renderTexture = sourceTexture as RenderTexture;
            if (renderTexture != null)
            {
                return renderTexture;
            }

            int width = Mathf.Max(1, sourceTexture.width);
            int height = Mathf.Max(1, sourceTexture.height);
            if (sourceCopyTexture == null || sourceCopyTexture.width != width || sourceCopyTexture.height != height)
            {
                ReleaseSourceCopyTexture();
                RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0)
                {
                    msaaSamples = 1,
                    sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
                    useMipMap = false,
                    autoGenerateMips = false
                };
                sourceCopyTexture = new RenderTexture(descriptor)
                {
                    name = "Kaleidoscope2_CrystalPresentation_SourceCopy",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                sourceCopyTexture.Create();
            }

            Graphics.Blit(sourceTexture, sourceCopyTexture);
            return sourceCopyTexture;
        }

        private void DeactivateAllRenderers(bool shutdownRuntimeObjects)
        {
            DeactivateRenderer(billboardRenderer, shutdownRuntimeObjects);
            DeactivateRenderer(realMeshRenderer, shutdownRuntimeObjects);
            activeRenderer = null;
            activeRenderMode = (CrystalRenderMode)(-1);
        }

        private static void DeactivateRenderer(ICrystalRenderer renderer, bool shutdownRuntimeObjects)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.SetVisible(false);
            renderer.SetSourceTexture(null);
            if (shutdownRuntimeObjects)
            {
                renderer.Shutdown();
            }
        }

        private void OnDestroy()
        {
            DeactivateAllRenderers(true);
            billboardRenderer = null;
            realMeshRenderer = null;

            ReleaseSourceCopyTexture();
            outputTexture = null;
        }

        private void ReleaseSourceCopyTexture()
        {
            if (sourceCopyTexture == null)
            {
                return;
            }

            if (sourceCopyTexture.IsCreated())
            {
                sourceCopyTexture.Release();
            }

            if (Application.isPlaying)
            {
                Destroy(sourceCopyTexture);
            }
            else
            {
                DestroyImmediate(sourceCopyTexture);
            }

            sourceCopyTexture = null;
        }
    }
}
