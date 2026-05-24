using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    internal sealed class KaelisMenuTooltip : MonoBehaviour
    {
        private const float PreferredWidth = 430f;
        private const float MinHeight = 148f;
        private const float MaxHeight = 238f;
        private const float EdgePadding = 18f;
        private const float AnchorGap = 14f;
        private const float FadeInDuration = 0.5f;
        private const float FadeOutDuration = 0.2f;
        private const float RiseOffset = 10f;

        private RectTransform canvasRoot;
        private RectTransform root;
        private CanvasGroup group;
        private TMP_Text titleText;
        private TMP_Text bodyText;
        private TMP_Text detailText;
        private RectTransform currentAnchor;
        private readonly Vector3[] worldCorners = new Vector3[4];
        private Vector2 targetPosition;
        private Vector2 hiddenPosition;
        private float targetAlpha;
        private bool hiding;

        internal float FadeInSeconds
        {
            get { return FadeInDuration; }
        }

        internal float FadeOutSeconds
        {
            get { return FadeOutDuration; }
        }

        internal bool UsesAnchorPlacement
        {
            get { return true; }
        }

        public static KaelisMenuTooltip Create(RectTransform parent, KaelisMenuAssets assets)
        {
            RectTransform root = KaelisMenuUiPrimitives.CreateRect("MenuTooltip", parent);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0f, 1f);
            root.sizeDelta = new Vector2(PreferredWidth, 172f);
            KaelisMenuUiPrimitives.AddImage(root, assets.SolidSprite, new Color(0.005f, 0.030f, 0.045f, 0.92f), false);
            KaelisMenuUiPrimitives.AddFrame(root, new Color(1f, 0.74f, 0.30f, 0.48f), new Color(0.18f, 0.82f, 0.94f, 0.28f), 1.05f, assets.SolidSprite);
            KaelisMenuUiPrimitives.AddCornerCuts(root, new Color(1f, 0.76f, 0.36f, 0.36f), 22f, 1.1f, assets.SolidSprite);

            RectTransform glow = KaelisMenuUiPrimitives.CreateRect("TooltipGoldBloom", root);
            glow.anchorMin = new Vector2(0f, 0.58f);
            glow.anchorMax = new Vector2(1f, 1f);
            glow.offsetMin = Vector2.zero;
            glow.offsetMax = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(glow, assets.SolidSprite, new Color(1f, 0.68f, 0.22f, 0.075f), false);

            KaelisMenuTooltip tooltip = root.gameObject.AddComponent<KaelisMenuTooltip>();
            tooltip.canvasRoot = parent;
            tooltip.root = root;
            tooltip.group = root.gameObject.AddComponent<CanvasGroup>();
            tooltip.group.alpha = 0f;
            tooltip.group.interactable = false;
            tooltip.group.blocksRaycasts = false;

            tooltip.titleText = KaelisMenuUiPrimitives.CreateText(root, "TooltipTitle", string.Empty, 15f, KaelisMenuStyle.GoldSoft, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Button));
            tooltip.titleText.characterSpacing = 3f;
            RectTransform titleRect = (RectTransform)tooltip.titleText.transform;
            titleRect.offsetMin = new Vector2(18f, 128f);
            titleRect.offsetMax = new Vector2(-18f, -12f);

            tooltip.bodyText = KaelisMenuUiPrimitives.CreateText(root, "TooltipBody", string.Empty, 12.5f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            tooltip.bodyText.enableWordWrapping = true;
            RectTransform bodyRect = (RectTransform)tooltip.bodyText.transform;
            bodyRect.offsetMin = new Vector2(18f, 54f);
            bodyRect.offsetMax = new Vector2(-18f, -48f);

            tooltip.detailText = KaelisMenuUiPrimitives.CreateText(root, "TooltipDetails", string.Empty, 11.5f, new Color(0.66f, 0.94f, 1f, 0.92f), TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            tooltip.detailText.enableWordWrapping = true;
            RectTransform detailRect = (RectTransform)tooltip.detailText.transform;
            detailRect.offsetMin = new Vector2(18f, 14f);
            detailRect.offsetMax = new Vector2(-18f, -116f);

            tooltip.HideImmediate();
            return tooltip;
        }

        public void Show(RectTransform anchor, string title, string body, string range, string current, string keys)
        {
            bool wasShowing = gameObject.activeSelf && group != null && group.alpha > 0.01f && !hiding;
            currentAnchor = anchor;
            titleText.text = KaelisMenuLocalizationService.Translate(title);
            bodyText.text = KaelisMenuLocalizationService.Translate(body);
            detailText.text = ComposeDetails(range, current, keys);
            UpdateDynamicSize();
            targetPosition = ComputeAnchoredPosition(currentAnchor);
            hiddenPosition = targetPosition + new Vector2(0f, -RiseOffset);
            if (!wasShowing)
            {
                root.anchoredPosition = hiddenPosition;
            }

            targetAlpha = 1f;
            hiding = false;
            gameObject.SetActive(true);
            root.SetAsLastSibling();
        }

        public void Show(string title, string body, string range, string current, string keys)
        {
            Show(null, title, body, range, current, keys);
        }

        public void Hide()
        {
            targetAlpha = 0f;
            hiding = true;
        }

        private void HideImmediate()
        {
            targetAlpha = 0f;
            hiding = false;
            currentAnchor = null;
            if (group != null)
            {
                group.alpha = 0f;
            }

            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (root == null || group == null)
            {
                return;
            }

            float delta = Application.isPlaying ? Time.unscaledDeltaTime : 0.016f;
            if (targetAlpha > 0f)
            {
                targetPosition = ComputeAnchoredPosition(currentAnchor);
                float alphaStep = delta / FadeInDuration;
                group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, alphaStep);
                root.anchoredPosition = Vector2.Lerp(root.anchoredPosition, targetPosition, delta * 12f);
            }
            else
            {
                float alphaStep = delta / FadeOutDuration;
                group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, alphaStep);
                root.anchoredPosition = Vector2.Lerp(root.anchoredPosition, hiddenPosition, delta * 10f);
                if (hiding && group.alpha <= 0.001f)
                {
                    HideImmediate();
                }
            }
        }

        private void UpdateDynamicSize()
        {
            if (canvasRoot == null || root == null)
            {
                return;
            }

            float availableHeight = Mathf.Max(MinHeight, canvasRoot.rect.height - (EdgePadding * 2f));
            float maxHeight = Mathf.Min(MaxHeight, availableHeight);
            float contentWidth = PreferredWidth - 36f;
            float titleHeight = Mathf.Max(24f, titleText.GetPreferredValues(titleText.text, contentWidth, 1000f).y);
            float bodyHeight = Mathf.Max(44f, bodyText.GetPreferredValues(bodyText.text, contentWidth, 1000f).y);
            float detailHeight = string.IsNullOrWhiteSpace(detailText.text) ? 0f : Mathf.Max(38f, detailText.GetPreferredValues(detailText.text, contentWidth, 1000f).y);
            float height = Mathf.Clamp(38f + titleHeight + bodyHeight + detailHeight, MinHeight, maxHeight);
            root.sizeDelta = new Vector2(PreferredWidth, height);

            RectTransform titleRect = (RectTransform)titleText.transform;
            RectTransform bodyRect = (RectTransform)bodyText.transform;
            RectTransform detailRect = (RectTransform)detailText.transform;
            float top = height - 16f;
            titleRect.offsetMin = new Vector2(18f, top - titleHeight);
            titleRect.offsetMax = new Vector2(-18f, -16f);
            bodyRect.offsetMin = new Vector2(18f, Mathf.Max(48f, top - titleHeight - bodyHeight - 12f));
            bodyRect.offsetMax = new Vector2(-18f, -(height - top + titleHeight + 8f));
            detailRect.offsetMin = new Vector2(18f, 14f);
            detailRect.offsetMax = new Vector2(-18f, -(height - Mathf.Max(16f, detailHeight + 14f)));
        }

        private Vector2 ComputeAnchoredPosition(RectTransform anchor)
        {
            Rect canvasRect = canvasRoot != null ? canvasRoot.rect : new Rect(-960f, -540f, 1920f, 1080f);
            if (anchor == null)
            {
                return ClampInside(canvasRect, new Vector2(canvasRect.center.x - (root.rect.width * 0.5f), canvasRect.center.y + (root.rect.height * 0.5f)));
            }

            Rect anchorRect = GetAnchorRect(anchor);
            Vector2 center = anchorRect.center;
            float width = root.rect.width;
            float height = root.rect.height;

            Vector2 above = new Vector2(center.x - (width * 0.5f), anchorRect.yMax + AnchorGap + height);
            if (Fits(canvasRect, above, width, height))
            {
                return above;
            }

            Vector2 below = new Vector2(center.x - (width * 0.5f), anchorRect.yMin - AnchorGap);
            if (Fits(canvasRect, below, width, height))
            {
                return below;
            }

            Vector2 right = new Vector2(anchorRect.xMax + AnchorGap, center.y + (height * 0.5f));
            if (Fits(canvasRect, right, width, height))
            {
                return right;
            }

            Vector2 left = new Vector2(anchorRect.xMin - AnchorGap - width, center.y + (height * 0.5f));
            if (Fits(canvasRect, left, width, height))
            {
                return left;
            }

            return ClampInside(canvasRect, above);
        }

        private Rect GetAnchorRect(RectTransform anchor)
        {
            anchor.GetWorldCorners(worldCorners);

            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int index = 0; index < worldCorners.Length; index++)
            {
                Vector2 local = canvasRoot.InverseTransformPoint(worldCorners[index]);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private bool Fits(Rect canvasRect, Vector2 topLeft, float width, float height)
        {
            return topLeft.x >= canvasRect.xMin + EdgePadding
                   && topLeft.x + width <= canvasRect.xMax - EdgePadding
                   && topLeft.y <= canvasRect.yMax - EdgePadding
                   && topLeft.y - height >= canvasRect.yMin + EdgePadding;
        }

        private Vector2 ClampInside(Rect canvasRect, Vector2 topLeft)
        {
            float width = root.rect.width;
            float height = root.rect.height;
            topLeft.x = Mathf.Clamp(topLeft.x, canvasRect.xMin + EdgePadding, canvasRect.xMax - width - EdgePadding);
            topLeft.y = Mathf.Clamp(topLeft.y, canvasRect.yMin + height + EdgePadding, canvasRect.yMax - EdgePadding);
            return topLeft;
        }

        private static string ComposeDetails(string range, string current, string keys)
        {
            string detail = string.Empty;
            if (!string.IsNullOrWhiteSpace(range))
            {
                detail += KaelisMenuLocalizationService.Translate("Range") + ": " + KaelisMenuLocalizationService.Translate(range);
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                detail += (detail.Length > 0 ? "\n" : string.Empty) + KaelisMenuLocalizationService.Translate("Current") + ": " + KaelisMenuLocalizationService.Translate(current);
            }

            if (!string.IsNullOrWhiteSpace(keys))
            {
                detail += (detail.Length > 0 ? "\n" : string.Empty) + KaelisMenuLocalizationService.Translate("Keys") + ": " + KaelisMenuLocalizationService.Translate(keys);
            }

            return detail;
        }
    }
}
