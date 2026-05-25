using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.FX
{
    [DisallowMultipleComponent]
    public sealed class MenuAtmosphereFXController : MonoBehaviour
    {
        internal const string ShaderName = "Kaleidoscope2/Menu/AtmosphereOverlay";

        [Header("Master")]
        [SerializeField] private bool EnableAtmosphereFX = true;

        [Header("Light Bands")]
        [SerializeField] private float BandSpeed = 0.018f;
        [SerializeField] private float BandOpacity = 0f;
        [SerializeField] private float BandSoftness = 0.075f;
        [SerializeField] private float WideBandSpeed = 0.010f;
        [SerializeField] private float WideBandOpacity = 0f;
        [SerializeField] private Vector2 BandDirection = new Vector2(0.82f, 0.57f);
        [SerializeField] private Vector2 WideBandDirection = new Vector2(0.62f, 0.78f);
        [SerializeField] private Color CyanTint = new Color(0.58f, 0.95f, 1f, 1f);
        [SerializeField] private Color GoldTint = new Color(1f, 0.72f, 0.32f, 1f);
        [SerializeField] private bool AdditiveBlend = true;

        [Header("Caustics")]
        [SerializeField] private float CausticIntensity = 0.065f;
        [SerializeField] private float NoiseDistortion = 0.012f;
        [SerializeField] private float IntensityBreathing = 0.25f;
        [SerializeField] private Vector2 CausticDrift = new Vector2(0.018f, 0.011f);

        [Header("Dispersion Dust")]
        [SerializeField] private float DustDensity = 0.46f;
        [SerializeField] private float DustSpeed = 0.55f;
        [SerializeField] private float DustColorVariation = 0.62f;

        [Header("Crystal Shimmer")]
        [SerializeField] private float CrystalShimmerIntensity = 0.64f;
        [SerializeField] private float SparkleIntensity = 0.70f;
        [SerializeField] private float SparkleSpeed = 0.45f;

        private RectTransform fxRoot;
        private MenuLightBandAnimator lightBandAnimator;
        private MenuDispersionDustController dustController;
        private MenuCrystalShimmerController shimmerController;
        private Material lightBandMaterial;
        private Material dustMaterial;
        private Material shimmerMaterial;
        private Shader overlayShader;

        internal bool AtmosphereEnabled { get { return EnableAtmosphereFX; } }
        internal float NarrowBandSpeed { get { return BandSpeed; } }
        internal float NarrowBandOpacity { get { return BandOpacity; } }
        internal float LightBandSoftness { get { return BandSoftness; } }
        internal float SoftWideBandSpeed { get { return WideBandSpeed; } }
        internal float SoftWideBandOpacity { get { return WideBandOpacity; } }
        internal Vector2 NarrowBandDirection { get { return BandDirection; } }
        internal Vector2 SoftWideBandDirection { get { return WideBandDirection; } }
        internal Color CoolTint { get { return CyanTint; } }
        internal Color WarmTint { get { return GoldTint; } }
        internal bool UsesAdditiveBlend { get { return AdditiveBlend; } }
        internal float CausticAmount { get { return CausticIntensity; } }
        internal float NoiseAmount { get { return NoiseDistortion; } }
        internal float BreathingAmount { get { return IntensityBreathing; } }
        internal Vector2 Drift { get { return CausticDrift; } }

        internal static MenuAtmosphereFXController Ensure(GameObject host)
        {
            if (host == null)
            {
                return null;
            }

            MenuAtmosphereFXController controller = host.GetComponent<MenuAtmosphereFXController>();
            if (controller == null)
            {
                controller = host.AddComponent<MenuAtmosphereFXController>();
            }

            return controller;
        }

        internal void Build(RectTransform canvasRoot, Sprite solidSprite)
        {
            if (canvasRoot == null || solidSprite == null || ResolveShader() == null)
            {
                return;
            }

            if (fxRoot == null)
            {
                fxRoot = CreateStretchedRect("MenuAtmosphereFX", canvasRoot);
            }

            if (lightBandAnimator == null)
            {
                RectTransform overlayRect = CreateStretchedRect("LightBandCausticOverlay", fxRoot);
                Image overlayImage = overlayRect.gameObject.AddComponent<Image>();
                overlayImage.sprite = solidSprite;
                overlayImage.color = Color.white;
                overlayImage.raycastTarget = false;

                lightBandMaterial = CreateRuntimeMaterial("KAELIS_Menu_LightBands", 0f, AdditiveBlend);
                overlayImage.material = lightBandMaterial;
                lightBandAnimator = overlayRect.gameObject.AddComponent<MenuLightBandAnimator>();
                lightBandAnimator.Initialize(lightBandMaterial);
            }

            if (dustController == null)
            {
                RectTransform dustRect = CreateStretchedRect("DispersionDust", fxRoot);
                dustMaterial = CreateRuntimeMaterial("KAELIS_Menu_DispersionDust", 2f, true);
                dustController = dustRect.gameObject.AddComponent<MenuDispersionDustController>();
                dustController.AssignMaterial(dustMaterial);
            }

            ApplySettings();
        }

        internal void BindCrystalShimmerTarget(RectTransform previewRoot)
        {
            if (previewRoot == null || ResolveShader() == null || shimmerController != null)
            {
                return;
            }

            RectTransform shimmerRect = CreateStretchedRect("CrystalShimmerHighlights", previewRoot);
            shimmerMaterial = CreateRuntimeMaterial("KAELIS_Menu_CrystalShimmer", 1f, true);
            shimmerController = shimmerRect.gameObject.AddComponent<MenuCrystalShimmerController>();
            shimmerController.AssignMaterial(shimmerMaterial);
            ApplySettings();
        }

        private void Update()
        {
            ClampSettings();
            ApplySettings();
        }

        private void OnValidate()
        {
            ClampSettings();
            ApplySettings();
        }

        private void OnDestroy()
        {
            DestroyRuntimeMaterial(lightBandMaterial);
            DestroyRuntimeMaterial(dustMaterial);
            DestroyRuntimeMaterial(shimmerMaterial);
            lightBandMaterial = null;
            dustMaterial = null;
            shimmerMaterial = null;
        }

        private void ApplySettings()
        {
            bool active = EnableAtmosphereFX;
            if (fxRoot != null && fxRoot.gameObject.activeSelf != active)
            {
                fxRoot.gameObject.SetActive(active);
            }

            if (lightBandAnimator != null)
            {
                lightBandAnimator.ApplySettings(this);
            }

            if (dustController != null)
            {
                dustController.gameObject.SetActive(active && DustDensity > 0.001f);
                dustController.Configure(DustDensity, DustSpeed, DustColorVariation);
            }

            if (shimmerController != null)
            {
                shimmerController.gameObject.SetActive(active && CrystalShimmerIntensity > 0.001f);
                shimmerController.Configure(CrystalShimmerIntensity, SparkleIntensity, SparkleSpeed, CyanTint, GoldTint);
            }
        }

        private Shader ResolveShader()
        {
            if (overlayShader == null)
            {
                overlayShader = Shader.Find(ShaderName);
            }

            return overlayShader;
        }

        private Material CreateRuntimeMaterial(string materialName, float effectMode, bool additive)
        {
            Shader shader = ResolveShader();
            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader)
            {
                name = materialName,
                hideFlags = HideFlags.DontSave
            };

            material.SetFloat(MenuAtmosphereShaderIds.EffectMode, effectMode);
            material.SetInt(MenuAtmosphereShaderIds.SrcBlend, (int)BlendMode.SrcAlpha);
            material.SetInt(MenuAtmosphereShaderIds.DstBlend, additive ? (int)BlendMode.One : (int)BlendMode.OneMinusSrcAlpha);
            return material;
        }

        private void ClampSettings()
        {
            BandSpeed = Mathf.Clamp(BandSpeed, 0f, 0.25f);
            BandOpacity = Mathf.Clamp01(BandOpacity);
            BandSoftness = Mathf.Clamp(BandSoftness, 0.005f, 0.45f);
            WideBandSpeed = Mathf.Clamp(WideBandSpeed, 0f, 0.25f);
            WideBandOpacity = Mathf.Clamp01(WideBandOpacity);
            CausticIntensity = Mathf.Clamp01(CausticIntensity);
            NoiseDistortion = Mathf.Clamp(NoiseDistortion, 0f, 0.10f);
            IntensityBreathing = Mathf.Clamp01(IntensityBreathing);
            DustDensity = Mathf.Clamp01(DustDensity);
            DustSpeed = Mathf.Clamp(DustSpeed, 0f, 2f);
            DustColorVariation = Mathf.Clamp01(DustColorVariation);
            CrystalShimmerIntensity = Mathf.Clamp(CrystalShimmerIntensity, 0f, 2f);
            SparkleIntensity = Mathf.Clamp(SparkleIntensity, 0f, 2f);
            SparkleSpeed = Mathf.Clamp(SparkleSpeed, 0.01f, 2.5f);

            if (BandDirection.sqrMagnitude < 0.0001f)
            {
                BandDirection = new Vector2(0.82f, 0.57f);
            }

            if (WideBandDirection.sqrMagnitude < 0.0001f)
            {
                WideBandDirection = new Vector2(0.62f, 0.78f);
            }
        }

        private static RectTransform CreateStretchedRect(string objectName, Transform parent)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rectTransform = (RectTransform)go.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            return rectTransform;
        }

        private static void DestroyRuntimeMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
        }
    }

    internal static class MenuAtmosphereShaderIds
    {
        public static readonly int MenuTime = Shader.PropertyToID("_MenuTime");
        public static readonly int EffectMode = Shader.PropertyToID("_EffectMode");
        public static readonly int NarrowBandColor = Shader.PropertyToID("_NarrowBandColor");
        public static readonly int WideBandColor = Shader.PropertyToID("_WideBandColor");
        public static readonly int NarrowBandParams = Shader.PropertyToID("_NarrowBandParams");
        public static readonly int WideBandParams = Shader.PropertyToID("_WideBandParams");
        public static readonly int BandDirection = Shader.PropertyToID("_BandDirection");
        public static readonly int AtmosphereParams = Shader.PropertyToID("_AtmosphereParams");
        public static readonly int CausticParams = Shader.PropertyToID("_CausticParams");
        public static readonly int ShimmerParams = Shader.PropertyToID("_ShimmerParams");
        public static readonly int ShimmerTintA = Shader.PropertyToID("_ShimmerTintA");
        public static readonly int ShimmerTintB = Shader.PropertyToID("_ShimmerTintB");
        public static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        public static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    }
}
