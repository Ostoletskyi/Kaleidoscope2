using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.FX
{
    [DisallowMultipleComponent]
    public sealed class PremiumMenuMotionController : MonoBehaviour
    {
        [SerializeField] private PremiumMenuStripeSettings[] stripeSettings = PremiumMenuStripeSettings.CreateDefaults();

        private RectTransform root;
        private PremiumMenuStripe[] stripes;
        private Sprite solidSprite;

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

            solidSprite = sprite;
            if (stripeSettings == null || stripeSettings.Length == 0)
            {
                stripeSettings = PremiumMenuStripeSettings.CreateDefaults();
            }

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

        private void EnsureStripes()
        {
            if (root == null || solidSprite == null)
            {
                return;
            }

            int count = Mathf.Max(0, stripeSettings.Length);
            if (stripes == null || stripes.Length != count)
            {
                stripes = new PremiumMenuStripe[count];
            }

            for (int index = 0; index < count; index++)
            {
                if (stripes[index] == null)
                {
                    RectTransform rect = CreateStripeRect(index, root);
                    Image image = rect.gameObject.AddComponent<Image>();
                    image.sprite = solidSprite;
                    image.raycastTarget = false;
                    stripes[index] = new PremiumMenuStripe(rect, image);
                }

                stripes[index].Configure(stripeSettings[index]);
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
    }

    [Serializable]
    public sealed class PremiumMenuStripeSettings
    {
        [SerializeField] private Vector2 direction = new Vector2(0.72f, -0.52f);
        [SerializeField, Range(0.01f, 0.45f)] private float width = 0.22f;
        [SerializeField, Range(0.001f, 0.08f)] private float speed = 0.012f;
        [SerializeField, Range(0f, 0.2f)] private float opacity = 0.08f;
        [SerializeField, Range(-90f, 90f)] private float angle = -18f;
        [SerializeField, Range(0f, 1f)] private float startOffset;
        [SerializeField] private Color color = new Color(0.68f, 0.96f, 1f, 1f);

        public Vector2 Direction { get { return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right; } }
        public float Width { get { return Mathf.Clamp(width, 0.01f, 0.45f); } }
        public float Speed { get { return Mathf.Clamp(speed, 0.001f, 0.08f); } }
        public float Opacity { get { return Mathf.Clamp(opacity, 0f, 0.2f); } }
        public float Angle { get { return Mathf.Clamp(angle, -90f, 90f); } }
        public float StartOffset { get { return Mathf.Repeat(startOffset, 1f); } }
        public Color Color { get { return color; } }

        public PremiumMenuStripeSettings()
        {
        }

        private PremiumMenuStripeSettings(Vector2 direction, float width, float speed, float opacity, float angle, float startOffset, Color color)
        {
            this.direction = direction;
            this.width = width;
            this.speed = speed;
            this.opacity = opacity;
            this.angle = angle;
            this.startOffset = startOffset;
            this.color = color;
        }

        internal static PremiumMenuStripeSettings[] CreateDefaults()
        {
            return new[]
            {
                new PremiumMenuStripeSettings(new Vector2(0.72f, -0.52f), 0.26f, 0.010f, 0.10f, -18f, 0.00f, new Color(0.62f, 0.96f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(0f, -1f), 0.075f, 0.006f, 0.06f, 0f, 0.20f, new Color(0.90f, 0.98f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(0.68f, 0.48f), 0.16f, 0.009f, 0.08f, 16f, 0.40f, new Color(0.76f, 0.92f, 1f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(1f, 0f), 0.34f, 0.004f, 0.05f, 0f, 0.62f, new Color(1f, 0.82f, 0.48f, 1f)),
                new PremiumMenuStripeSettings(new Vector2(-0.62f, -0.54f), 0.045f, 0.014f, 0.07f, 24f, 0.82f, new Color(0.78f, 1f, 0.94f, 1f))
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
            transform.sizeDelta = new Vector2(diagonal * 1.55f, thickness);
            transform.localEulerAngles = new Vector3(0f, 0f, settings.Angle);

            Vector2 direction = settings.Direction;
            float travel = diagonal * 0.82f + thickness;
            float phase = Mathf.Repeat(settings.StartOffset + time * settings.Speed, 1f);
            float distance = Mathf.Lerp(-travel, travel, phase);
            transform.anchoredPosition = direction * distance;
        }
    }
}
