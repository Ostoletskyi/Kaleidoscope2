using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    public sealed class KaelisMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private RectTransform root;
        private RectTransform fillClip;
        private RectTransform fillContent;
        private CanvasGroup glowGroup;
        private CanvasGroup fillGroup;
        private CanvasGroup pressedGroup;
        private CanvasGroup flashGroup;
        private TMP_Text labelText;
        private KaelisMenuTooltip tooltip;
        private string tooltipTitle;
        private string tooltipBody;
        private string tooltipRange;
        private string tooltipCurrent;
        private string tooltipKeys;
        private KaelisButtonPalette palette;
        private bool hovered;
        private bool pressed;
        private bool selected;
        private float fillProgress;
        private float glowProgress;
        private float pressedProgress;
        private float flashProgress;
        private Coroutine flashRoutine;

        public Button Button { get; private set; }
        public Toggle Toggle { get; private set; }
        public bool HasTooltipData
        {
            get { return tooltip != null && !string.IsNullOrWhiteSpace(tooltipTitle) && root != null; }
        }

        public string TooltipKeys
        {
            get { return tooltipKeys; }
        }

        internal static KaelisMenuButton CreateButton(RectTransform parent, string name, string label, KaelisMenuButtonTone tone, KaelisMenuIconKind icon, KaelisMenuAssets assets, float height)
        {
            RectTransform rect = CreateRoot(parent, name, height);
            Image hitArea = KaelisMenuUiPrimitives.AddImage(rect, assets.SolidSprite, new Color(1f, 1f, 1f, 0.012f), true);
            Button button = rect.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = hitArea;

            KaelisMenuButton menuButton = rect.gameObject.AddComponent<KaelisMenuButton>();
            menuButton.Button = button;
            menuButton.Build(label, tone, icon, assets, height);
            return menuButton;
        }

        internal static KaelisMenuButton CreateToggle(RectTransform parent, string name, string label, KaelisMenuButtonTone tone, KaelisMenuIconKind icon, KaelisMenuAssets assets, float height)
        {
            RectTransform rect = CreateRoot(parent, name, height);
            Image hitArea = KaelisMenuUiPrimitives.AddImage(rect, assets.SolidSprite, new Color(1f, 1f, 1f, 0.012f), true);
            Toggle toggle = rect.gameObject.AddComponent<Toggle>();
            toggle.transition = Selectable.Transition.None;
            toggle.targetGraphic = hitArea;

            KaelisMenuButton menuButton = rect.gameObject.AddComponent<KaelisMenuButton>();
            menuButton.Toggle = toggle;
            menuButton.Build(label, tone, icon, assets, height);
            toggle.onValueChanged.AddListener(menuButton.SetSelectedVisual);
            return menuButton;
        }

        public void InvokeWithFlash(UnityAction action)
        {
            if (action == null)
            {
                return;
            }

            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                action.Invoke();
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashThenInvoke(action));
        }

        public void SetSelectedVisual(bool isSelected)
        {
            selected = isSelected;
        }

        internal void ConfigureTooltip(KaelisMenuTooltip tooltip, string title, string body, string range, string current, string keys)
        {
            this.tooltip = tooltip;
            tooltipTitle = title;
            tooltipBody = body;
            tooltipRange = range;
            tooltipCurrent = current;
            tooltipKeys = keys;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            pressed = false;
            HideTooltip();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            hovered = true;
            ShowTooltip();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            hovered = false;
            pressed = false;
            HideTooltip();
        }

        private static RectTransform CreateRoot(RectTransform parent, string name, float height)
        {
            RectTransform rect = KaelisMenuUiPrimitives.CreateRect(name, parent);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0f, height);
            KaelisMenuUiPrimitives.AddLayout(rect.gameObject, -1f, height);
            return rect;
        }

        private void Build(string label, KaelisMenuButtonTone tone, KaelisMenuIconKind icon, KaelisMenuAssets assets, float height)
        {
            root = (RectTransform)transform;
            palette = KaelisMenuStyle.GetButtonPalette(tone);
            KaelisMenuButtonTextures textures = assets.GetButtonTextures(tone);

            RectTransform glow = KaelisMenuUiPrimitives.CreateRect("GemGlow", root);
            KaelisMenuUiPrimitives.Stretch(glow);
            glow.offsetMin = new Vector2(-18f, -10f);
            glow.offsetMax = new Vector2(18f, 10f);
            KaelisMenuUiPrimitives.AddImage(glow, assets.SolidSprite, new Color(palette.Glow.r, palette.Glow.g, palette.Glow.b, 0.18f), false);
            glowGroup = glow.gameObject.AddComponent<CanvasGroup>();
            glowGroup.alpha = 0f;

            RectTransform baseLayer = KaelisMenuUiPrimitives.CreateRect("GemBase", root);
            KaelisMenuUiPrimitives.Stretch(baseLayer);
            AddGemTextureStrip(baseLayer, textures.BestNormal, Color.white, assets.SolidSprite);
            AddTextReadabilityBand(baseLayer, assets.SolidSprite);

            fillClip = KaelisMenuUiPrimitives.CreateRect("GemActivationClip", root);
            fillClip.anchorMin = new Vector2(0f, 0f);
            fillClip.anchorMax = new Vector2(0f, 1f);
            fillClip.pivot = new Vector2(0f, 0.5f);
            fillClip.anchoredPosition = Vector2.zero;
            fillClip.sizeDelta = new Vector2(0f, 0f);
            fillClip.gameObject.AddComponent<RectMask2D>();
            fillGroup = fillClip.gameObject.AddComponent<CanvasGroup>();
            fillGroup.alpha = 0f;

            fillContent = KaelisMenuUiPrimitives.CreateRect("GemActivationFullWidth", fillClip);
            fillContent.anchorMin = new Vector2(0f, 0f);
            fillContent.anchorMax = new Vector2(0f, 1f);
            fillContent.pivot = new Vector2(0f, 0.5f);
            fillContent.anchoredPosition = Vector2.zero;
            fillContent.sizeDelta = new Vector2(0f, 0f);
            AddGemTextureStrip(fillContent, textures.BestLit, new Color(1f, 1f, 1f, 0.95f), assets.SolidSprite);
            AddCleanActivationGlow(fillContent, palette.AccentSoft, assets.SolidSprite);

            RectTransform pressedLayer = KaelisMenuUiPrimitives.CreateRect("GemPressed", root);
            KaelisMenuUiPrimitives.Stretch(pressedLayer);
            AddGemTextureStrip(pressedLayer, textures.BestPressed, new Color(1f, 1f, 1f, 0.92f), assets.SolidSprite);
            AddCleanActivationGlow(pressedLayer, palette.AccentSoft, assets.SolidSprite);
            pressedGroup = pressedLayer.gameObject.AddComponent<CanvasGroup>();
            pressedGroup.alpha = 0f;

            RectTransform flash = KaelisMenuUiPrimitives.CreateRect("GemReleaseFlash", root);
            KaelisMenuUiPrimitives.Stretch(flash);
            KaelisMenuUiPrimitives.AddImage(flash, assets.SolidSprite, new Color(1f, 0.82f, 0.18f, 0.13f), false);
            flashGroup = flash.gameObject.AddComponent<CanvasGroup>();
            flashGroup.alpha = 0f;

            AddBroadActivationSurface(flash, new Color(1f, 0.84f, 0.24f, 0.34f), assets.SolidSprite);

            AddButtonIcon(root, icon, palette, assets.SolidSprite, height);
            AddButtonLabel(root, label, palette, assets.GetFont(KaelisMenuFontRole.Button), tone);
            ApplyVisuals();
        }

        private void Update()
        {
            float delta = Application.isPlaying ? Time.unscaledDeltaTime : 0.016f;
            float targetFill = hovered || pressed || selected ? 1f : 0f;
            float targetGlow = hovered || selected ? 1f : 0f;
            float targetPressed = pressed ? 1f : 0f;

            fillProgress = Mathf.MoveTowards(fillProgress, targetFill, delta * KaelisMenuStyle.ButtonFillSpeed);
            glowProgress = Mathf.MoveTowards(glowProgress, targetGlow, delta * KaelisMenuStyle.ButtonGlowSpeed);
            pressedProgress = Mathf.MoveTowards(pressedProgress, targetPressed, delta * KaelisMenuStyle.ButtonGlowSpeed);
            flashProgress = Mathf.MoveTowards(flashProgress, 0f, delta * 7f);
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (root == null)
            {
                return;
            }

            float width = Mathf.Max(1f, root.rect.width);
            fillClip.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * fillProgress);
            fillContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            float lit = Mathf.Clamp01(Mathf.Max(fillProgress, selected ? 0.66f : 0f));
            fillGroup.alpha = Mathf.Lerp(0f, 0.78f, lit);
            glowGroup.alpha = Mathf.Lerp(0f, 0.52f, Mathf.Max(glowProgress, flashProgress));
            pressedGroup.alpha = Mathf.Lerp(0f, 0.78f, pressedProgress);
            flashGroup.alpha = flashProgress;
            root.localScale = Vector3.one * (pressed ? 0.985f : 1f);

            if (labelText != null)
            {
            labelText.color = Color.Lerp(KaelisMenuStyle.TextSecondary, palette.Text, Mathf.Max(0.52f, lit));
            }

        }

        private void ShowTooltip()
        {
            if (tooltip != null)
            {
                tooltip.Show(root, tooltipTitle, tooltipBody, tooltipRange, tooltipCurrent, tooltipKeys);
            }
        }

        private void HideTooltip()
        {
            if (tooltip != null)
            {
                tooltip.Hide();
            }
        }

        private IEnumerator FlashThenInvoke(UnityAction action)
        {
            flashProgress = 1f;
            pressed = false;
            ApplyVisuals();
            yield return new WaitForSecondsRealtime(KaelisMenuStyle.ActivationFlashDelay);
            action.Invoke();
            flashRoutine = null;
        }

        private static void AddGemTextureStrip(RectTransform parent, Texture texture, Color color, Sprite fallbackSprite)
        {
            if (texture == null)
            {
                KaelisMenuUiPrimitives.AddImage(parent, fallbackSprite, color, false);
                return;
            }

            float capRatio = Mathf.Clamp(KaelisMenuStyle.ButtonCapWidth / Mathf.Max(1f, texture.width), 0.04f, 0.34f);

            RectTransform left = KaelisMenuUiPrimitives.CreateRect("LeftCap", parent);
            left.anchorMin = new Vector2(0f, 0f);
            left.anchorMax = new Vector2(0f, 1f);
            left.pivot = new Vector2(0f, 0.5f);
            left.sizeDelta = new Vector2(KaelisMenuStyle.ButtonCapWidth, 0f);
            left.anchoredPosition = Vector2.zero;
            RawImage leftImage = KaelisMenuUiPrimitives.AddRawImage(left, texture, color, false);
            leftImage.uvRect = new Rect(0f, 0f, capRatio, 1f);

            RectTransform center = KaelisMenuUiPrimitives.CreateRect("CenterStretch", parent);
            center.anchorMin = new Vector2(0f, 0f);
            center.anchorMax = new Vector2(1f, 1f);
            center.offsetMin = new Vector2(KaelisMenuStyle.ButtonCapWidth, 0f);
            center.offsetMax = new Vector2(-KaelisMenuStyle.ButtonCapWidth, 0f);
            RawImage centerImage = KaelisMenuUiPrimitives.AddRawImage(center, texture, color, false);
            centerImage.uvRect = new Rect(capRatio, 0f, 1f - (capRatio * 2f), 1f);

            RectTransform right = KaelisMenuUiPrimitives.CreateRect("RightCap", parent);
            right.anchorMin = new Vector2(1f, 0f);
            right.anchorMax = new Vector2(1f, 1f);
            right.pivot = new Vector2(1f, 0.5f);
            right.sizeDelta = new Vector2(KaelisMenuStyle.ButtonCapWidth, 0f);
            right.anchoredPosition = Vector2.zero;
            RawImage rightImage = KaelisMenuUiPrimitives.AddRawImage(right, texture, color, false);
            rightImage.uvRect = new Rect(1f - capRatio, 0f, capRatio, 1f);
        }

        private static void AddButtonIcon(RectTransform root, KaelisMenuIconKind icon, KaelisButtonPalette palette, Sprite solidSprite, float height)
        {
            RectTransform iconRoot = KaelisMenuUiPrimitives.CreateRect("Icon", root);
            iconRoot.anchorMin = new Vector2(0f, 0.5f);
            iconRoot.anchorMax = new Vector2(0f, 0.5f);
            iconRoot.pivot = new Vector2(0.5f, 0.5f);
            iconRoot.anchoredPosition = new Vector2(57f, 0f);
            iconRoot.sizeDelta = new Vector2(height * 0.62f, height * 0.62f);
            AddFilledDiamond(iconRoot, "IconGlow", height * 0.36f, new Color(palette.Glow.r, palette.Glow.g, palette.Glow.b, 0.16f), solidSprite);
            AddFilledDiamond(iconRoot, "IconGem", height * 0.22f, new Color(palette.Accent.r, palette.Accent.g, palette.Accent.b, 0.82f), solidSprite);
            AddFilledDiamond(iconRoot, "IconCore", height * 0.095f, new Color(1f, 0.96f, 0.78f, 0.92f), solidSprite);
        }

        private void AddButtonLabel(RectTransform root, string label, KaelisButtonPalette activePalette, TMP_FontAsset font, KaelisMenuButtonTone tone)
        {
            float maxFontSize = tone == KaelisMenuButtonTone.Primary ? 19f : 18f;
            labelText = KaelisMenuUiPrimitives.CreateText(root, "Label", label, maxFontSize, activePalette.Text, TextAlignmentOptions.Center, font);
            labelText.characterSpacing = tone == KaelisMenuButtonTone.Primary ? 4.5f : 4f;
            labelText.fontStyle = FontStyles.UpperCase;
            labelText.overflowMode = TextOverflowModes.Overflow;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = 14f;
            labelText.fontSizeMax = maxFontSize;
            RectTransform labelRect = (RectTransform)labelText.transform;
            labelRect.offsetMin = new Vector2(72f, 4f);
            labelRect.offsetMax = new Vector2(-48f, -4f);
            KaelisMenuUiPrimitives.AddTextGlow(labelText.gameObject, new Color(activePalette.Glow.r, activePalette.Glow.g, activePalette.Glow.b, 0.22f), new Vector2(0f, 0f));
        }

        private static void AddTextReadabilityBand(RectTransform parent, Sprite solidSprite)
        {
            RectTransform band = KaelisMenuUiPrimitives.CreateRect("TextReadabilityBand", parent);
            band.anchorMin = new Vector2(0.18f, 0.18f);
            band.anchorMax = new Vector2(0.84f, 0.82f);
            band.offsetMin = Vector2.zero;
            band.offsetMax = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(band, solidSprite, new Color(0f, 0.012f, 0.018f, 0.22f), false);
        }

        private static void AddCleanActivationGlow(RectTransform parent, Color color, Sprite solidSprite)
        {
            AddBroadActivationSurface(parent, color, solidSprite);
        }

        private static void AddBroadActivationSurface(RectTransform parent, Color color, Sprite solidSprite)
        {
            RectTransform surface = KaelisMenuUiPrimitives.CreateRect("ActivationSurface", parent);
            surface.anchorMin = new Vector2(0.06f, 0.18f);
            surface.anchorMax = new Vector2(0.94f, 0.82f);
            surface.pivot = new Vector2(0.5f, 0.5f);
            surface.offsetMin = Vector2.zero;
            surface.offsetMax = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(surface, solidSprite, new Color(color.r, color.g, color.b, Mathf.Min(color.a, 0.16f)), false);

            RectTransform bloom = KaelisMenuUiPrimitives.CreateRect("ActivationBloom", parent);
            bloom.anchorMin = new Vector2(0.12f, 0.30f);
            bloom.anchorMax = new Vector2(0.88f, 0.70f);
            bloom.pivot = new Vector2(0.5f, 0.5f);
            bloom.offsetMin = Vector2.zero;
            bloom.offsetMax = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(bloom, solidSprite, new Color(color.r, color.g, color.b, Mathf.Min(color.a, 0.24f)), false);
        }

        private static void AddFilledDiamond(RectTransform parent, string name, float size, Color color, Sprite solidSprite)
        {
            RectTransform diamond = KaelisMenuUiPrimitives.CreateRect(name, parent);
            diamond.anchorMin = new Vector2(0.5f, 0.5f);
            diamond.anchorMax = new Vector2(0.5f, 0.5f);
            diamond.pivot = new Vector2(0.5f, 0.5f);
            diamond.sizeDelta = new Vector2(size, size);
            diamond.anchoredPosition = Vector2.zero;
            diamond.localEulerAngles = new Vector3(0f, 0f, 45f);
            KaelisMenuUiPrimitives.AddImage(diamond, solidSprite, color, false);
        }
    }
}
