using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.FX
{
    [DisallowMultipleComponent]
    public sealed class PremiumMenuMotionController : MonoBehaviour
    {
        private const int PremiumStripeCount = 5;
        private const float StripeLengthMultiplier = 2.65f;
        private const int GradientResolution = 64;

        [SerializeField] private PremiumMenuStripeSettings[] stripeSettings = PremiumMenuStripeSettings.CreateDefaults();

        private RectTransform root;
        private PremiumMenuStripe[] stripes;
        private Sprite[] generatedStripeSprites;
        private Sprite fallbackSprite;

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
        [SerializeField, Range(0.01f, 0.45f)] private float width = 0.22f;
        [SerializeField, Range(0.001f, 0.08f)] private float speed = 0.008f;
        [SerializeField, Range(0f, 0.2f)] private float opacity = 0.07f;
        [SerializeField, Range(-90f, 110f)] private float angle = 22f;
        [SerializeField, Range(0.35f, 0.65f)] private float softness = 0.54f;
        [SerializeField, Range(-0.5f, 0.5f)] private float spacing;
        [SerializeField, Range(0f, 1f)] private float startOffset = 0.42f;
        [SerializeField] private Color color = new Color(0.62f, 0.96f, 1f, 1f);

        public Vector2 Direction { get { return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right; } }
        public float Width { get { return Mathf.Clamp(width, 0.01f, 0.45f); } }
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
                new PremiumMenuStripeSettings(new Vector2(0.72f, -0.69f), 0.22f, 0.0074f, 0.070f, 22f, 0.56f, -0.18f, 0.42f, new Color(0.62f, 0.96f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(-0.48f, -0.88f), 0.145f, 0.0066f, 0.055f, -18f, 0.53f, 0.16f, 0.57f, new Color(0.74f, 0.93f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(0.61f, -0.79f), 0.076f, 0.0090f, 0.045f, 35f, 0.48f, 0.31f, 0.48f, new Color(0.80f, 0.97f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(1f, -0.08f), 0.30f, 0.0046f, 0.035f, 8f, 0.62f, -0.35f, 0.32f, new Color(1f, 0.84f, 0.52f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(-0.57f, -0.82f), 0.046f, 0.0078f, 0.050f, -32f, 0.42f, -0.04f, 0.68f, new Color(0.72f, 1f, 0.94f, 1f))
            };
        }
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
            if (transform == null || settings == null)
            {
                return;
            }

            float width = Mathf.Max(1f, bounds.width);
            float height = Mathf.Max(1f, bounds.height);
            float diagonal = Mathf.Sqrt(width * width + height * height);
            float thickness = Mathf.Max(8f, Mathf.Min(width, height) * settings.Width);
            float stripeLength = Mathf.Max(
                diagonal * PremiumMenuMotionController.MinimumStripeLengthMultiplier,
                diagonal * PremiumMenuMotionController.ConfiguredStripeLengthMultiplier);

            transform.sizeDelta = new Vector2(stripeLength, thickness);
            transform.localEulerAngles = new Vector3(0f, 0f, settings.Angle);

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
            float offscreenDistance = halfScreenProjection + halfStripeProjection + offsetProjection + thickness;

            float phase = Mathf.Repeat(settings.StartOffset + time * settings.Speed, 1f);
            float distance = Mathf.Lerp(-offscreenDistance, offscreenDistance, phase);
            transform.anchoredPosition = direction * distance + laneOffset;
        }
    }
}
