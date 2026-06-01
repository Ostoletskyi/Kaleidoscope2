using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    public sealed class KaelisMenuSliderControl : MonoBehaviour
    {
        private Slider slider;
        private TMP_Text valueText;
        private KaelisMenuInteractiveRow row;
        private float defaultValue;
        private string[] options;
        private string suffix;
        private Action<float> changedHandler;

        public float Value
        {
            get { return slider != null ? slider.value : defaultValue; }
        }

        public float MinValue
        {
            get { return slider != null ? slider.minValue : 0f; }
        }

        public float MaxValue
        {
            get { return slider != null ? slider.maxValue : 0f; }
        }

        internal static KaelisMenuSliderControl Create(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip, string title, string description, float min, float max, float defaultValue, string suffix, bool reserved)
        {
            return Create(parent, assets, tooltip, title, description, min, max, defaultValue, suffix, reserved ? KaelisMenuBindingStatus.Reserved : KaelisMenuBindingStatus.RealBinding, null, null);
        }

        internal static KaelisMenuSliderControl Create(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip, string title, string description, float min, float max, float defaultValue, string suffix, bool reserved, string unavailableReason)
        {
            return Create(parent, assets, tooltip, title, description, min, max, defaultValue, suffix, reserved ? KaelisMenuBindingStatus.Reserved : KaelisMenuBindingStatus.RealBinding, null, unavailableReason);
        }

        internal static KaelisMenuSliderControl Create(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip, string title, string description, float min, float max, float defaultValue, string suffix, bool reserved, string[] options)
        {
            return Create(parent, assets, tooltip, title, description, min, max, defaultValue, suffix, reserved ? KaelisMenuBindingStatus.Reserved : KaelisMenuBindingStatus.RealBinding, options);
        }

        internal static KaelisMenuSliderControl Create(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip, string title, string description, float min, float max, float defaultValue, string suffix, bool reserved, string[] options, string unavailableReason)
        {
            return Create(parent, assets, tooltip, title, description, min, max, defaultValue, suffix, reserved ? KaelisMenuBindingStatus.Reserved : KaelisMenuBindingStatus.RealBinding, options, unavailableReason);
        }

        internal static KaelisMenuSliderControl Create(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip, string title, string description, float min, float max, float defaultValue, string suffix, KaelisMenuBindingStatus status)
        {
            return Create(parent, assets, tooltip, title, description, min, max, defaultValue, suffix, status, null);
        }

        internal static KaelisMenuSliderControl Create(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip, string title, string description, float min, float max, float defaultValue, string suffix, KaelisMenuBindingStatus status, string[] options)
        {
            return Create(parent, assets, tooltip, title, description, min, max, defaultValue, suffix, status, options, null);
        }

        internal static KaelisMenuSliderControl Create(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip, string title, string description, float min, float max, float defaultValue, string suffix, KaelisMenuBindingStatus status, string[] options, string unavailableReason)
        {
            RectTransform root = KaelisMenuUiPrimitives.CreateRect(ToObjectName(title) + "Slider", parent);
            KaelisMenuUiPrimitives.AddLayout(root.gameObject, -1f, 76f);
            Image surface = KaelisMenuUiPrimitives.AddImage(root, assets.SolidSprite, new Color(0.010f, 0.060f, 0.080f, 0.58f), true);
            AddWideHighlight(root, assets, out CanvasGroup highlightGroup, out CanvasGroup flashGroup);

            KaelisMenuSliderControl control = root.gameObject.AddComponent<KaelisMenuSliderControl>();
            control.defaultValue = defaultValue;
            control.suffix = suffix;
            control.options = options;

            string rangeText = options != null ? string.Join(" / ", options) : Format(min, suffix) + " - " + Format(max, suffix);
            string statusText = GetStatusLabel(status, unavailableReason);
            string statusLine = status == KaelisMenuBindingStatus.Reserved && !string.IsNullOrWhiteSpace(unavailableReason)
                ? unavailableReason
                : "Status: " + statusText;
            control.row = root.gameObject.AddComponent<KaelisMenuInteractiveRow>();
            control.row.Configure(surface, highlightGroup, flashGroup, tooltip, title, description + "\n" + statusLine, rangeText, control.FormatValue(defaultValue), KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Slider));

            TMP_Text label = KaelisMenuUiPrimitives.CreateText(root, "Label", title.ToUpperInvariant(), 14.5f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Button));
            label.characterSpacing = 3f;
            RectTransform labelRect = (RectTransform)label.transform;
            labelRect.offsetMin = new Vector2(20f, 44f);
            labelRect.offsetMax = new Vector2(-270f, -5f);

            TMP_Text badge = KaelisMenuUiPrimitives.CreateText(root, "BindingBadge", statusText, 11f, GetStatusColor(status), TextAlignmentOptions.Right, assets.GetFont(KaelisMenuFontRole.Status));
            badge.characterSpacing = 2f;
            RectTransform badgeRect = (RectTransform)badge.transform;
            badgeRect.offsetMin = new Vector2(0f, 44f);
            badgeRect.offsetMax = new Vector2(-18f, -5f);

            TMP_Text range = KaelisMenuUiPrimitives.CreateText(root, "Range", rangeText, 11.5f, KaelisMenuStyle.TextMuted, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            RectTransform rangeRect = (RectTransform)range.transform;
            rangeRect.offsetMin = new Vector2(20f, 9f);
            rangeRect.offsetMax = new Vector2(-600f, -45f);

            control.valueText = KaelisMenuUiPrimitives.CreateText(root, "Value", string.Empty, 13f, KaelisMenuStyle.GoldSoft, TextAlignmentOptions.Right, assets.GetFont(KaelisMenuFontRole.Status));
            RectTransform valueRect = (RectTransform)control.valueText.transform;
            valueRect.offsetMin = new Vector2(0f, 8f);
            valueRect.offsetMax = new Vector2(-18f, -44f);

            RectTransform sliderRoot = KaelisMenuUiPrimitives.CreateRect("Slider", root);
            sliderRoot.anchorMin = new Vector2(0f, 0f);
            sliderRoot.anchorMax = new Vector2(1f, 0f);
            sliderRoot.offsetMin = new Vector2(210f, 18f);
            sliderRoot.offsetMax = new Vector2(-150f, 42f);

            control.slider = sliderRoot.gameObject.AddComponent<Slider>();
            control.slider.transition = Selectable.Transition.None;
            control.slider.minValue = min;
            control.slider.maxValue = max;
            control.slider.wholeNumbers = options != null;
            control.slider.interactable = status != KaelisMenuBindingStatus.Reserved;

            RectTransform rail = KaelisMenuUiPrimitives.CreateRect("GlassRail", sliderRoot);
            rail.anchorMin = new Vector2(0f, 0.5f);
            rail.anchorMax = new Vector2(1f, 0.5f);
            rail.sizeDelta = new Vector2(0f, 8f);
            rail.anchoredPosition = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(rail, assets.SolidSprite, new Color(0.08f, 0.42f, 0.52f, 0.60f), false);

            RectTransform fillArea = KaelisMenuUiPrimitives.CreateRect("FillArea", sliderRoot);
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = new Vector2(0f, 7f);
            fillArea.offsetMax = new Vector2(0f, -7f);

            RectTransform fill = KaelisMenuUiPrimitives.CreateRect("GoldFill", fillArea);
            KaelisMenuUiPrimitives.Stretch(fill);
            KaelisMenuUiPrimitives.AddImage(fill, assets.SolidSprite, new Color(1f, 0.70f, 0.24f, 0.82f), false);
            control.slider.fillRect = fill;

            RectTransform handleArea = KaelisMenuUiPrimitives.CreateRect("HandleSlideArea", sliderRoot);
            KaelisMenuUiPrimitives.Stretch(handleArea);
            handleArea.offsetMin = new Vector2(8f, 0f);
            handleArea.offsetMax = new Vector2(-8f, 0f);

            RectTransform handle = KaelisMenuUiPrimitives.CreateRect("GemKnob", handleArea);
            handle.sizeDelta = new Vector2(20f, 20f);
            handle.localEulerAngles = new Vector3(0f, 0f, 45f);
            Image handleImage = KaelisMenuUiPrimitives.AddImage(handle, assets.SolidSprite, new Color(1f, 0.82f, 0.32f, 0.96f), true);
            control.slider.handleRect = handle;
            control.slider.targetGraphic = handleImage;

            control.slider.value = Mathf.Clamp(defaultValue, min, max);
            control.slider.onValueChanged.AddListener(control.SetValueVisual);
            control.SetValueVisual(control.slider.value);
            MenuAudioFeedbackController.BindSlider(control.slider);
            return control;
        }

        internal void SetChangedHandler(Action<float> handler)
        {
            changedHandler = handler;
        }

        private void SetValueVisual(float value)
        {
            string formatted = FormatValue(value);
            if (valueText != null)
            {
                valueText.text = formatted;
            }

            if (row != null)
            {
                row.SetTooltipCurrent(formatted);
            }

            if (changedHandler != null)
            {
                changedHandler(value);
            }
        }

        private string FormatValue(float value)
        {
            if (options != null && options.Length > 0)
            {
                int index = Mathf.Clamp(Mathf.RoundToInt(value), 0, options.Length - 1);
                return options[index];
            }

            return Format(value, suffix);
        }

        private static string Format(float value, string suffix)
        {
            return value.ToString(suffix == "%" ? "0" : "0.00") + suffix;
        }

        private static string GetStatusLabel(KaelisMenuBindingStatus status, string unavailableReason = null)
        {
            switch (status)
            {
                case KaelisMenuBindingStatus.PartialBinding:
                    return "PARTIAL";
                case KaelisMenuBindingStatus.Reserved:
                    return string.IsNullOrWhiteSpace(unavailableReason) ? "RESERVED" : "SERVICE MISSING";
                default:
                    return "REAL";
            }
        }

        private static Color GetStatusColor(KaelisMenuBindingStatus status)
        {
            switch (status)
            {
                case KaelisMenuBindingStatus.PartialBinding:
                    return KaelisMenuStyle.Cyan;
                case KaelisMenuBindingStatus.Reserved:
                    return KaelisMenuStyle.TextMuted;
                default:
                    return KaelisMenuStyle.GoldSoft;
            }
        }

        internal static void AddWideHighlight(RectTransform root, KaelisMenuAssets assets, out CanvasGroup highlightGroup, out CanvasGroup flashGroup)
        {
            RectTransform highlight = KaelisMenuUiPrimitives.CreateRect("GoldHoverHighlight", root);
            KaelisMenuUiPrimitives.Stretch(highlight);
            KaelisMenuUiPrimitives.AddImage(highlight, assets.SolidSprite, new Color(1f, 0.68f, 0.22f, 0.09f), false);
            AddBand(highlight, "TopGold", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 6f), assets);
            AddBand(highlight, "BottomGold", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 6f), assets);
            AddBand(highlight, "LeftGold", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(6f, 0f), assets);
            AddBand(highlight, "RightGold", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(6f, 0f), assets);
            highlightGroup = highlight.gameObject.AddComponent<CanvasGroup>();
            highlightGroup.alpha = 0f;

            RectTransform flash = KaelisMenuUiPrimitives.CreateRect("ActivationFlash", root);
            KaelisMenuUiPrimitives.Stretch(flash);
            KaelisMenuUiPrimitives.AddImage(flash, assets.SolidSprite, new Color(1f, 0.80f, 0.24f, 0.20f), false);
            flashGroup = flash.gameObject.AddComponent<CanvasGroup>();
            flashGroup.alpha = 0f;
        }

        private static void AddBand(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, KaelisMenuAssets assets)
        {
            RectTransform band = KaelisMenuUiPrimitives.CreateRect(name, parent);
            band.anchorMin = anchorMin;
            band.anchorMax = anchorMax;
            band.sizeDelta = size;
            band.anchoredPosition = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(band, assets.SolidSprite, new Color(1f, 0.72f, 0.28f, 0.62f), false);
        }

        private static string ToObjectName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "Slider";
            }

            char[] characters = value.ToCharArray();
            int writeIndex = 0;
            for (int index = 0; index < characters.Length; index++)
            {
                if (char.IsLetterOrDigit(characters[index]))
                {
                    characters[writeIndex] = characters[index];
                    writeIndex++;
                }
            }

            return writeIndex > 0 ? new string(characters, 0, writeIndex) : "Slider";
        }
    }
}
