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

        [Header("Module")]
        [SerializeField] private bool moduleEnabled = true;
        [SerializeField] private bool onlyIn4DMode;

        private readonly DiamondRotationController rotationController = new DiamondRotationController();
        private readonly DiamondShapeController shapeController = new DiamondShapeController();
        private readonly DiamondMaterialModeController materialModeController = new DiamondMaterialModeController();
        private readonly DiamondOpticsController opticsController = new DiamondOpticsController();

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
                || command.Type == KaleidoscopeCommandType.SetDiamondShape
                || command.Type == KaleidoscopeCommandType.CycleDiamondMaterialMode
                || command.Type == KaleidoscopeCommandType.SetDiamondMaterialMode;
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

                case KaleidoscopeCommandType.SetDiamondShape:
                    shapeController.SetShape(settings, (DiamondFocusShape)Mathf.Clamp(command.IntValue, 0, DiamondFocusSettings.ShapeCount - 1));
                    break;

                case KaleidoscopeCommandType.CycleDiamondMaterialMode:
                    materialModeController.NextMode(settings);
                    break;

                case KaleidoscopeCommandType.SetDiamondMaterialMode:
                    materialModeController.SetMode(settings, (DiamondCrystalMaterialMode)Mathf.Clamp(command.IntValue, 0, DiamondFocusSettings.MaterialModeCount - 1));
                    break;
            }
        }

        public Texture Process(Texture sourceTexture, KaleidoscopeState runtimeState)
        {
            if (sourceTexture == null || runtimeState == null)
            {
                return sourceTexture;
            }

            DiamondFocusSettings settings = runtimeState.DiamondFocusSettings;
            if (!moduleEnabled
                || settings == null
                || !settings.Enabled
                || (onlyIn4DMode && runtimeState.ActiveVisualMode != KaleidoscopeVisualMode.Hose))
            {
                return sourceTexture;
            }

            Material compositeMaterial = EnsureCompositeMaterial();
            Material glassMaterial = EnsureCrystalMaterial();
            Material activeBlurMaterial = EnsureBlurMaterial();
            if (compositeMaterial == null || glassMaterial == null || activeBlurMaterial == null)
            {
                return sourceTexture;
            }

            EnsureOutputTexture(runtimeState);
            if (outputTexture == null || !RenderCrystal(sourceTexture, settings, glassMaterial))
            {
                return sourceTexture;
            }

            float backgroundBlurAmount = ResolveBackgroundBlurAmount(settings);
            Texture backgroundTexture = RenderBackgroundBlur(sourceTexture, backgroundBlurAmount, activeBlurMaterial);
            opticsController.ConfigureCompositeMaterial(compositeMaterial, settings, sourceTexture, backgroundTexture, crystalTexture, backgroundBlurAmount);
            Graphics.Blit(sourceTexture, outputTexture, compositeMaterial);
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
            string opticsStatus = (diamondMaterial != null || crystalMaterial != null || blurMaterial != null)
                ? "assigned material"
                : (runtimeCompositeMaterial != null && runtimeCrystalMaterial != null && runtimeBlurMaterial != null)
                    ? "runtime 3D materials"
                    : (diamondShader != null || crystalShader != null || blurShader != null)
                        ? "assigned shader"
                        : "shader lookup";

            return CreateStatus(enabled
                + ", " + mode
                + ", shape " + shapeController.GetShapeLabel(settings)
                + ", material " + materialModeController.GetModeLabel(settings)
                + ", dir " + direction
                + ", speed " + speed
                + ", normalized " + normalized
                + ", Background Blur " + backgroundBlur + " (radius " + maxBlurRadius.ToString("0.0") + ", iterations " + Mathf.Max(1, blurIterations) + ", downsample " + Mathf.Max(1, blurDownsample) + ")"
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

            renderRoot = null;
            cameraObject = null;
            crystalObject = null;
            renderCamera = null;
            crystalMeshFilter = null;
            crystalMeshRenderer = null;
            runtimeCompositeMaterial = null;
            runtimeCrystalMaterial = null;
            runtimeBlurMaterial = null;
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
        }

        private bool RenderCrystal(Texture sourceTexture, DiamondFocusSettings settings, Material material)
        {
            if (sourceTexture == null || settings == null || material == null)
            {
                return false;
            }

            EnsureCrystalTexture();
            EnsureRenderer();

            if (crystalTexture == null || renderCamera == null || crystalMeshFilter == null || crystalMeshRenderer == null)
            {
                return false;
            }

            Mesh mesh = shapeController.GetMesh(settings.Shape);
            if (mesh == null)
            {
                return false;
            }

            if (renderedShape != settings.Shape || crystalMeshFilter.sharedMesh != mesh)
            {
                crystalMeshFilter.sharedMesh = mesh;
                renderedShape = settings.Shape;
            }

            float scale = Mathf.Max(0.05f, crystalWorldScale * settings.ScreenScale);
            crystalObject.transform.localPosition = Vector3.zero;
            crystalObject.transform.localRotation = Quaternion.Euler(settings.RotationEuler);
            crystalObject.transform.localScale = new Vector3(scale, scale, scale);
            crystalMeshRenderer.sharedMaterial = material;
            opticsController.ConfigureCrystalMaterial(material, settings, sourceTexture);

            RenderTexture previousTarget = renderCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            renderCamera.targetTexture = crystalTexture;
            renderCamera.Render();
            renderCamera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            return true;
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
