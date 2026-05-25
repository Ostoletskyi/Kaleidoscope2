using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.FX
{
    [Serializable]
    public sealed class PremiumMenuPrismReactionSettings
    {
        [SerializeField] private Rect crystalRectNormalized = new Rect(0.47f, 0.22f, 0.36f, 0.61f);
        [SerializeField, Range(0f, 0.45f)] private float prismIntensity = 0.30f;
        [SerializeField, Range(0.05f, 4f)] private float prismFadeInSpeed = 1.70f;
        [SerializeField, Range(0.05f, 4f)] private float prismFadeOutSpeed = 0.86f;
        [SerializeField, Range(0f, 0.6f)] private float prismOffset = 0.22f;
        [SerializeField, Range(0.02f, 0.5f)] private float prismSpread = 0.24f;
        [SerializeField, Range(0.35f, 0.65f)] private float prismSoftness = 0.50f;
        [SerializeField, Range(0.001f, 0.14f)] private float prismColorSeparation = 0.052f;
        [SerializeField, Range(0.05f, 1f)] private float prismLifetimeSmoothing = 0.30f;

        public Rect CrystalRectNormalized
        {
            get
            {
                float width = Mathf.Clamp(crystalRectNormalized.width, 0.02f, 1f);
                float height = Mathf.Clamp(crystalRectNormalized.height, 0.02f, 1f);
                return new Rect(
                    Mathf.Clamp(crystalRectNormalized.x, 0f, 1f - width),
                    Mathf.Clamp(crystalRectNormalized.y, 0f, 1f - height),
                    width,
                    height);
            }
        }

        public float PrismIntensity { get { return Mathf.Clamp(prismIntensity, 0f, 0.45f); } }
        public float PrismFadeInSpeed { get { return Mathf.Clamp(prismFadeInSpeed, 0.05f, 4f); } }
        public float PrismFadeOutSpeed { get { return Mathf.Clamp(prismFadeOutSpeed, 0.05f, 4f); } }
        public float PrismOffset { get { return Mathf.Clamp(prismOffset, 0f, 0.6f); } }
        public float PrismSpread { get { return Mathf.Clamp(prismSpread, 0.02f, 0.5f); } }
        public float PrismSoftness { get { return Mathf.Clamp(prismSoftness, 0.35f, 0.65f); } }
        public float PrismColorSeparation { get { return Mathf.Clamp(prismColorSeparation, 0.001f, 0.14f); } }
        public float PrismLifetimeSmoothing { get { return Mathf.Clamp(prismLifetimeSmoothing, 0.05f, 1f); } }
    }

    [DisallowMultipleComponent]
    public sealed class PremiumMenuPrismReactionController : MaskableGraphic
    {
        private PremiumMenuMotionController beamSource;
        private PremiumMenuPrismReactionSettings settings;
        private Material runtimeMaterial;
        private PremiumMenuPrismReactionController mirroredSource;
        private float mirroredIntensityScale = 1f;
        private float visibleIntensity;
        private Vector2 refractedCenterNormalized = new Vector2(0.65f, 0.48f);
        private Vector2 refractedDirection = Vector2.right;

        public override Texture mainTexture
        {
            get { return s_WhiteTexture; }
        }

        internal PremiumMenuPrismReactionSettings Settings { get { return settings; } }
        internal float VisibleIntensity { get { return visibleIntensity; } }

        internal void AssignMaterial(Material materialInstance)
        {
            runtimeMaterial = materialInstance;
            material = runtimeMaterial;
            color = Color.white;
            raycastTarget = false;
            SetMaterialDirty();
        }

        internal void Configure(PremiumMenuMotionController source, PremiumMenuPrismReactionSettings newSettings)
        {
            beamSource = source;
            mirroredSource = null;
            settings = newSettings ?? new PremiumMenuPrismReactionSettings();
            raycastTarget = false;
            UpdateMaterialProperties(Application.isPlaying ? Time.unscaledTime : 0f);
        }

        internal void MirrorFrom(PremiumMenuPrismReactionController source, PremiumMenuPrismReactionSettings newSettings, float intensityScale)
        {
            beamSource = null;
            mirroredSource = source;
            mirroredIntensityScale = Mathf.Max(0f, intensityScale);
            settings = newSettings ?? new PremiumMenuPrismReactionSettings();
            raycastTarget = false;
            UpdateMirroredProperties(Application.isPlaying ? Time.unscaledTime : 0f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            raycastTarget = false;
        }

        private void Update()
        {
            if (mirroredSource != null)
            {
                UpdateMirroredProperties(Time.unscaledTime);
                return;
            }

            float deltaTime = Application.isPlaying ? Time.unscaledDeltaTime : 0.016f;
            EvaluateAtTime(Time.unscaledTime, deltaTime);
        }

        internal void EvaluateAtTime(float time, float deltaTime)
        {
            if (beamSource == null || settings == null)
            {
                return;
            }

            Rect bounds = rectTransform.rect;
            Rect normalizedRect = settings.CrystalRectNormalized;
            Vector2 crystalCenter = NormalizedToLocal(normalizedRect.center, bounds);
            Vector2 crystalSize = new Vector2(normalizedRect.width * bounds.width, normalizedRect.height * bounds.height);
            float crystalRadius = Mathf.Max(1f, Mathf.Min(crystalSize.x, crystalSize.y) * 0.48f);

            float strongestInfluence = 0f;
            PremiumMenuBeamSample strongestBeam = default(PremiumMenuBeamSample);
            for (int index = 0; index < beamSource.BeamCount; index++)
            {
                PremiumMenuBeamSample beam;
                if (!beamSource.TryGetBeamSample(index, time, out beam))
                {
                    continue;
                }

                float beamDistance = Mathf.Abs(Vector2.Dot(crystalCenter - beam.Center, beam.Normal));
                float intersectionRange = crystalRadius + beam.Thickness * 0.5f;
                float normalizedDistance = Mathf.InverseLerp(intersectionRange * 0.28f, intersectionRange, beamDistance);
                float influence = 1f - Mathf.SmoothStep(0f, 1f, normalizedDistance);
                influence *= Mathf.Lerp(0.72f, 1f, Mathf.InverseLerp(0.03f, 0.08f, beam.Opacity));
                if (influence > strongestInfluence)
                {
                    strongestInfluence = influence;
                    strongestBeam = beam;
                }
            }

            float breathing = 0.92f + Mathf.Sin(time * 0.31f) * 0.08f;
            float targetIntensity = strongestInfluence * settings.PrismIntensity * breathing;
            float responseSpeed = targetIntensity > visibleIntensity ? settings.PrismFadeInSpeed : settings.PrismFadeOutSpeed;
            float smoothing = Mathf.Lerp(1f, 0.34f, settings.PrismLifetimeSmoothing);
            visibleIntensity = Mathf.MoveTowards(visibleIntensity, targetIntensity, responseSpeed * smoothing * Mathf.Max(0f, deltaTime));

            if (strongestInfluence > 0.0001f)
            {
                Vector2 outgoingDirection = strongestBeam.Direction.sqrMagnitude > 0.0001f
                    ? strongestBeam.Direction.normalized
                    : Vector2.right;
                Vector2 closestBeamPoint = strongestBeam.Center
                    + strongestBeam.Normal * Vector2.Dot(crystalCenter - strongestBeam.Center, strongestBeam.Normal);
                Vector2 reactionPoint = Vector2.Lerp(crystalCenter, closestBeamPoint, 0.34f)
                    + outgoingDirection * crystalRadius * settings.PrismOffset;
                refractedCenterNormalized = LocalToNormalized(reactionPoint, bounds);
                refractedDirection = outgoingDirection;
            }

            UpdateMaterialProperties(time);
        }

        private void UpdateMirroredProperties(float time)
        {
            if (mirroredSource == null)
            {
                return;
            }

            visibleIntensity = Mathf.Min(settings != null ? settings.PrismIntensity * 1.18f : 1f, mirroredSource.visibleIntensity * mirroredIntensityScale);
            refractedCenterNormalized = mirroredSource.refractedCenterNormalized;
            refractedDirection = mirroredSource.refractedDirection;
            UpdateMaterialProperties(time);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = rectTransform.rect;
            Color32 vertexColor = color;
            vh.AddVert(new Vector3(rect.xMin, rect.yMin, 0f), vertexColor, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(rect.xMin, rect.yMax, 0f), vertexColor, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(rect.xMax, rect.yMax, 0f), vertexColor, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(rect.xMax, rect.yMin, 0f), vertexColor, new Vector2(1f, 0f));
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
        }

        private void UpdateMaterialProperties(float time)
        {
            if (runtimeMaterial == null || settings == null)
            {
                return;
            }

            Rect crystalRect = settings.CrystalRectNormalized;
            runtimeMaterial.SetFloat(MenuAtmosphereShaderIds.MenuTime, time);
            runtimeMaterial.SetVector(
                MenuAtmosphereShaderIds.PrismCrystalRect,
                new Vector4(crystalRect.x, crystalRect.y, crystalRect.width, crystalRect.height));
            runtimeMaterial.SetVector(
                MenuAtmosphereShaderIds.PrismReaction,
                new Vector4(refractedCenterNormalized.x, refractedCenterNormalized.y, visibleIntensity, settings.PrismSoftness));
            runtimeMaterial.SetVector(
                MenuAtmosphereShaderIds.PrismDirection,
                new Vector4(refractedDirection.x, refractedDirection.y, settings.PrismOffset, settings.PrismSpread));
            runtimeMaterial.SetVector(
                MenuAtmosphereShaderIds.PrismOptics,
                new Vector4(settings.PrismColorSeparation, settings.PrismLifetimeSmoothing, settings.PrismFadeInSpeed, settings.PrismFadeOutSpeed));
        }

        private static Vector2 NormalizedToLocal(Vector2 point, Rect bounds)
        {
            return new Vector2(
                bounds.xMin + point.x * bounds.width,
                bounds.yMin + point.y * bounds.height);
        }

        private static Vector2 LocalToNormalized(Vector2 point, Rect bounds)
        {
            return new Vector2(
                bounds.width > 0.001f ? Mathf.InverseLerp(bounds.xMin, bounds.xMax, point.x) : 0.5f,
                bounds.height > 0.001f ? Mathf.InverseLerp(bounds.yMin, bounds.yMax, point.y) : 0.5f);
        }
    }
}
