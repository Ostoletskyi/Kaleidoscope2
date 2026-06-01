using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.FX
{
    [DisallowMultipleComponent]
    public sealed class PremiumMenuMotionController : MonoBehaviour
    {
        private const int PremiumStripeCount = 8;
        private const float StripeLengthMultiplier = 2.95f;
        private const int GradientResolution = 64;
        internal const float MinimumVisibleBeamWidth = 0.105f;
        internal const float MaximumVisibleBeamWidth = 0.245f;

        [SerializeField] private PremiumMenuStripeSettings[] stripeSettings = PremiumMenuStripeSettings.CreateDefaults();

        private RectTransform root;
        private PremiumMenuStripe[] stripes;
        private Sprite[] generatedStripeSprites;
        private Sprite fallbackSprite;

        internal int BeamCount
        {
            get { return stripes != null ? stripes.Length : 0; }
        }

        internal static PremiumMenuMotionController Ensure(GameObject host)
        {
            if (host == null)
            {
                return null;
            }

            PremiumMenuMotionController controller = host.GetComponent<PremiumMenuMotionController>();
            if (controller == null)
            {
                controller = host.AddComponent<PremiumMenuMotionController>();
            }

            return controller;
        }

        internal void Build(RectTransform canvasRoot, Sprite sprite)
        {
            if (canvasRoot == null || sprite == null)
            {
                return;
            }

            fallbackSprite = sprite;
            EnsurePremiumSettings();

            if (root == null)
            {
                root = CreateStretchedRect("PremiumMenuMotionStripes", canvasRoot);
            }

            EnsureStripes();
        }

        private void Update()
        {
            if (root == null || stripes == null)
            {
                return;
            }

            Rect rect = root.rect;
            float time = Time.unscaledTime;
            for (int index = 0; index < stripes.Length; index++)
            {
                if (stripes[index] != null)
                {
                    stripes[index].Tick(rect, time);
                }
            }
        }

        internal bool TryGetBeamSample(int index, float time, out PremiumMenuBeamSample sample)
        {
            sample = default(PremiumMenuBeamSample);
            if (root == null || stripes == null || index < 0 || index >= stripes.Length || stripes[index] == null)
            {
                return false;
            }

            return stripes[index].TrySample(root.rect, time, out sample);
        }

        private void OnDestroy()
        {
            DestroyGeneratedSprites();
        }

        private void EnsurePremiumSettings()
        {
            if (stripeSettings == null || stripeSettings.Length != PremiumStripeCount)
            {
                stripeSettings = PremiumMenuStripeSettings.CreateDefaults();
            }
        }

        private void EnsureStripes()
        {
            if (root == null || fallbackSprite == null)
            {
                return;
            }

            EnsurePremiumSettings();
            if (stripes == null || stripes.Length != PremiumStripeCount)
            {
                RemoveExistingStripeObjects();
                DestroyGeneratedSprites();
                stripes = new PremiumMenuStripe[PremiumStripeCount];
                generatedStripeSprites = new Sprite[PremiumStripeCount];
            }

            for (int index = 0; index < PremiumStripeCount; index++)
            {
                if (stripes[index] == null)
                {
                    RectTransform rect = CreateStripeRect(index, root);
                    Image image = rect.gameObject.AddComponent<Image>();
                    generatedStripeSprites[index] = CreateSoftGradientSprite(stripeSettings[index].Softness);
                    image.sprite = generatedStripeSprites[index] != null ? generatedStripeSprites[index] : fallbackSprite;
                    image.type = Image.Type.Simple;
                    image.raycastTarget = false;
                    stripes[index] = new PremiumMenuStripe(rect, image);
                }

                stripes[index].Configure(stripeSettings[index]);
            }
        }

        private void RemoveExistingStripeObjects()
        {
            if (root == null)
            {
                return;
            }

            for (int index = root.childCount - 1; index >= 0; index--)
            {
                Transform child = root.GetChild(index);
                if (child != null && child.name.StartsWith("PremiumLightStripe_", StringComparison.Ordinal))
                {
                    DestroyOwnedObject(child.gameObject);
                }
            }
        }

        private void DestroyGeneratedSprites()
        {
            if (generatedStripeSprites == null)
            {
                return;
            }

            for (int index = 0; index < generatedStripeSprites.Length; index++)
            {
                Sprite sprite = generatedStripeSprites[index];
                if (sprite == null)
                {
                    continue;
                }

                Texture texture = sprite.texture;
                DestroyOwnedObject(sprite);
                DestroyOwnedObject(texture);
            }

            generatedStripeSprites = null;
        }

        private static Sprite CreateSoftGradientSprite(float softness)
        {
            Texture2D texture = new Texture2D(2, GradientResolution, TextureFormat.RGBA32, false)
            {
                name = "KAELIS_PremiumStripe_Gradient",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };

            float controlledSoftness = Mathf.Clamp(softness, 0.35f, 0.65f);
            float fullIntensityRadius = Mathf.Lerp(0.12f, 0.30f, controlledSoftness);
            for (int y = 0; y < GradientResolution; y++)
            {
                float distanceFromCenter = Mathf.Abs((y / (GradientResolution - 1f)) * 2f - 1f);
                float alpha = 1f - Mathf.SmoothStep(fullIntensityRadius, 1f, distanceFromCenter);
                Color pixel = new Color(1f, 1f, 1f, alpha);
                texture.SetPixel(0, y, pixel);
                texture.SetPixel(1, y, pixel);
            }

            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "KAELIS_PremiumStripe_SoftGradient";
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
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

        private static RectTransform CreateStripeRect(int index, Transform parent)
        {
            GameObject go = new GameObject("PremiumLightStripe_" + (index + 1).ToString(), typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rectTransform = (RectTransform)go.transform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;
            return rectTransform;
        }

        private static void DestroyOwnedObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        internal const float MinimumStripeLengthMultiplier = 1.35f;
        internal static float ConfiguredStripeLengthMultiplier { get { return StripeLengthMultiplier; } }
    }

    [Serializable]
    public sealed class PremiumMenuStripeSettings
    {
        [SerializeField] private Vector2 direction = new Vector2(0.72f, -0.69f);
        [SerializeField, Range(PremiumMenuMotionController.MinimumVisibleBeamWidth, PremiumMenuMotionController.MaximumVisibleBeamWidth)] private float width = 0.14f;
        [SerializeField, Range(0.001f, 0.08f)] private float speed = 0.011f;
        [SerializeField, Range(0f, 0.2f)] private float opacity = 0.07f;
        [SerializeField, Range(-90f, 110f)] private float angle = 22f;
        [SerializeField, Range(0.35f, 0.65f)] private float softness = 0.54f;
        [SerializeField, Range(-0.5f, 0.5f)] private float spacing;
        [SerializeField, Range(0f, 1f)] private float startOffset = 0.42f;
        [SerializeField] private Color color = new Color(0.62f, 0.96f, 1f, 1f);

        public Vector2 Direction { get { return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right; } }
        public float Width { get { return Mathf.Clamp(width, PremiumMenuMotionController.MinimumVisibleBeamWidth, PremiumMenuMotionController.MaximumVisibleBeamWidth); } }
        public float Speed { get { return Mathf.Clamp(speed, 0.001f, 0.08f); } }
        public float Opacity { get { return Mathf.Clamp(opacity, 0f, 0.2f); } }
        public float Angle { get { return Mathf.Clamp(angle, -90f, 110f); } }
        public float Softness { get { return Mathf.Clamp(softness, 0.35f, 0.65f); } }
        public float Spacing { get { return Mathf.Clamp(spacing, -0.5f, 0.5f); } }
        public float StartOffset { get { return Mathf.Repeat(startOffset, 1f); } }
        public Color Color { get { return color; } }

        public PremiumMenuStripeSettings()
        {
        }

        private PremiumMenuStripeSettings(Vector2 direction, float width, float speed, float opacity, float angle, float softness, float spacing, float startOffset, Color color)
        {
            this.direction = direction;
            this.width = width;
            this.speed = speed;
            this.opacity = opacity;
            this.angle = angle;
            this.softness = softness;
            this.spacing = spacing;
            this.startOffset = startOffset;
            this.color = color;
        }

        internal static PremiumMenuStripeSettings[] CreateDefaults()
        {
            return new[]
            {
                new PremiumMenuStripeSettings(new Vector2(0.375f, -0.927f), 0.230f, 0.0105f, 0.150f, 20f, 0.58f, -0.06f, 0.03f, new Color(0.62f, 0.96f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(-0.309f, -0.951f), 0.150f, 0.0120f, 0.135f, -18f, 0.53f, -0.24f, 0.16f, new Color(0.74f, 0.93f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(0.574f, -0.819f), 0.115f, 0.0135f, 0.165f, 35f, 0.45f, 0.22f, 0.28f, new Color(0.82f, 0.98f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(0.990f, -0.139f), 0.245f, 0.0096f, 0.120f, 80f, 0.62f, -0.34f, 0.40f, new Color(0.55f, 0.88f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(0.122f, -0.993f), 0.190f, 0.0114f, 0.128f, 7f, 0.59f, 0.04f, 0.52f, new Color(1f, 0.84f, 0.52f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(-0.530f, -0.848f), 0.120f, 0.0140f, 0.155f, -32f, 0.44f, 0.30f, 0.64f, new Color(0.72f, 1f, 0.94f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(0.705f, -0.709f), 0.170f, 0.0108f, 0.132f, 48f, 0.55f, -0.12f, 0.76f, new Color(0.90f, 0.98f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(-0.150f, -0.989f), 0.108f, 0.0152f, 0.145f, -44f, 0.43f, 0.18f, 0.88f, new Color(1f, 0.92f, 0.68f, 1f))
            };
        }
    }

    internal struct PremiumMenuBeamSample
    {
        public Vector2 Center;
        public Vector2 Direction;
        public Vector2 Normal;
        public float Thickness;
        public float Length;
        public float Opacity;
    }

    internal sealed class PremiumMenuStripe
    {
        private readonly RectTransform transform;
        private readonly Image image;
        private PremiumMenuStripeSettings settings;

        public PremiumMenuStripe(RectTransform transform, Image image)
        {
            this.transform = transform;
            this.image = image;
        }

        public void Configure(PremiumMenuStripeSettings value)
        {
            settings = value;
            if (image != null && settings != null)
            {
                Color color = settings.Color;
                color.a = settings.Opacity;
                image.color = color;
                image.raycastTarget = false;
            }
        }

        public void Tick(Rect bounds, float time)
        {
            PremiumMenuBeamSample sample;
            if (transform == null || !TrySample(bounds, time, out sample))
            {
                return;
            }

            transform.sizeDelta = new Vector2(sample.Length, sample.Thickness);
            transform.localEulerAngles = new Vector3(0f, 0f, settings.Angle);
            transform.anchoredPosition = sample.Center;
        }

        internal bool TrySample(Rect bounds, float time, out PremiumMenuBeamSample sample)
        {
            sample = default(PremiumMenuBeamSample);
            if (settings == null)
            {
                return false;
            }

            float width = Mathf.Max(1f, bounds.width);
            float height = Mathf.Max(1f, bounds.height);
            float diagonal = Mathf.Sqrt(width * width + height * height);
            float thickness = Mathf.Max(8f, Mathf.Min(width, height) * settings.Width);
            float stripeLength = Mathf.Max(
                diagonal * PremiumMenuMotionController.MinimumStripeLengthMultiplier,
                diagonal * PremiumMenuMotionController.ConfiguredStripeLengthMultiplier);

            float radians = settings.Angle * Mathf.Deg2Rad;
            Vector2 stripeAxis = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            Vector2 stripeAcross = new Vector2(-stripeAxis.y, stripeAxis.x);
            Vector2 direction = settings.Direction;
            Vector2 laneOffset = stripeAxis * diagonal * settings.Spacing;

            float halfScreenProjection = Mathf.Abs(direction.x) * width * 0.5f + Mathf.Abs(direction.y) * height * 0.5f;
            float halfStripeProjection =
                Mathf.Abs(Vector2.Dot(stripeAxis, direction)) * stripeLength * 0.5f
                + Mathf.Abs(Vector2.Dot(stripeAcross, direction)) * thickness * 0.5f;
            float offsetProjection = Mathf.Abs(Vector2.Dot(laneOffset, direction));
            float offscreenDistance = halfScreenProjection + halfStripeProjection + offsetProjection + thickness * 0.12f;

            float phase = Mathf.Repeat(settings.StartOffset + time * settings.Speed, 1f);
            float distance = Mathf.Lerp(-offscreenDistance, offscreenDistance, phase);
            sample.Center = direction * distance + laneOffset;
            sample.Direction = direction;
            sample.Normal = stripeAcross;
            sample.Thickness = thickness;
            sample.Length = stripeLength;
            sample.Opacity = settings.Opacity;
            return true;
        }
    }
}
