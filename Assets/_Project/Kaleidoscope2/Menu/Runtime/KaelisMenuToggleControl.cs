using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    public sealed class KaelisMenuToggleControl : MonoBehaviour
    {
        private KaelisMenuInteractiveRow row;
        private RectTransform knob;
        private TMP_Text valueText;
        private Button button;
        private Action<bool> changedHandler;
        private bool value;

        public bool Value
        {
            get { return value; }
        }

        public Button Button
        {
            get { return button; }
        }

        internal static KaelisMenuToggleControl Create(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip, string title, string description, bool defaultValue, bool reserved)
        {
            return Create(parent, assets, tooltip, title, description, defaultValue, reserved ? KaelisMenuBindingStatus.Reserved : KaelisMenuBindingStatus.RealBinding);
        }

        internal static KaelisMenuToggleControl Create(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip, string title, string description, bool defaultValue, KaelisMenuBindingStatus status)
        {
            RectTransform root = KaelisMenuUiPrimitives.CreateRect(ToObjectName(title) + "Toggle", parent);
            KaelisMenuUiPrimitives.AddLayout(root.gameObject, -1f, 62f);
            Image surface = KaelisMenuUiPrimitives.AddImage(root, assets.SolidSprite, new Color(0.010f, 0.060f, 0.080f, 0.58f), true);
            KaelisMenuSliderControl.AddWideHighlight(root, assets, out CanvasGroup highlightGroup, out CanvasGroup flashGroup);

            KaelisMenuToggleControl control = root.gameObject.AddComponent<KaelisMenuToggleControl>();
            control.row = root.gameObject.AddComponent<KaelisMenuInteractiveRow>();
            string statusText = GetStatusLabel(status);
            control.row.Configure(surface, highlightGroup, flashGroup, tooltip, title, description + "\nStatus: " + statusText, "OFF / ON", defaultValue ? "ON" : "OFF", "Click to toggle");

            control.button = root.gameObject.AddComponent<Button>();
            control.button.transition = Selectable.Transition.None;
            control.button.targetGraphic = surface;
            control.button.interactable = status != KaelisMenuBindingStatus.Reserved;
            control.button.onClick.AddListener(control.Toggle);
            MenuAudioFeedbackController.BindToggleButton(control.button, () => control.Value);

            TMP_Text label = KaelisMenuUiPrimitives.CreateText(root, "Label", title.ToUpperInvariant(), 14f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Button));
            label.characterSpacing = 3f;
            RectTransform labelRect = (RectTransform)label.transform;
            labelRect.offsetMin = new Vector2(20f, 28f);
            labelRect.offsetMax = new Vector2(-260f, -4f);

            TMP_Text badge = KaelisMenuUiPrimitives.CreateText(root, "BindingBadge", statusText, 11f, GetStatusColor(status), TextAlignmentOptions.Right, assets.GetFont(KaelisMenuFontRole.Status));
            RectTransform badgeRect = (RectTransform)badge.transform;
            badgeRect.offsetMin = new Vector2(0f, 28f);
            badgeRect.offsetMax = new Vector2(-18f, -4f);

            TMP_Text body = KaelisMenuUiPrimitives.CreateText(root, "Description", description, 11.5f, KaelisMenuStyle.TextMuted, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            body.enableWordWrapping = true;
            RectTransform bodyRect = (RectTransform)body.transform;
            bodyRect.offsetMin = new Vector2(20f, 6f);
            bodyRect.offsetMax = new Vector2(-260f, -34f);

            RectTransform track = KaelisMenuUiPrimitives.CreateRect("GlassToggleTrack", root);
            track.anchorMin = new Vector2(1f, 0.5f);
            track.anchorMax = new Vector2(1f, 0.5f);
            track.pivot = new Vector2(1f, 0.5f);
            track.sizeDelta = new Vector2(92f, 28f);
            track.anchoredPosition = new Vector2(-102f, -4f);
            KaelisMenuUiPrimitives.AddImage(track, assets.SolidSprite, new Color(0.07f, 0.38f, 0.48f, 0.60f), false);

            control.knob = KaelisMenuUiPrimitives.CreateRect("GemToggleKnob", track);
            control.knob.anchorMin = new Vector2(0f, 0.5f);
            control.knob.anchorMax = new Vector2(0f, 0.5f);
            control.knob.pivot = new Vector2(0.5f, 0.5f);
            control.knob.sizeDelta = new Vector2(22f, 22f);
            control.knob.localEulerAngles = new Vector3(0f, 0f, 45f);
            KaelisMenuUiPrimitives.AddImage(control.knob, assets.SolidSprite, KaelisMenuStyle.GoldSoft, false);

            control.valueText = KaelisMenuUiPrimitives.CreateText(root, "Value", string.Empty, 13f, KaelisMenuStyle.GoldSoft, TextAlignmentOptions.Right, assets.GetFont(KaelisMenuFontRole.Status));
            RectTransform valueRect = (RectTransform)control.valueText.transform;
            valueRect.offsetMin = new Vector2(0f, 0f);
            valueRect.offsetMax = new Vector2(-18f, -34f);

            control.value = defaultValue;
            control.ApplyVisual();
            return control;
        }

        internal void SetChangedHandler(Action<bool> handler)
        {
            changedHandler = handler;
        }

        internal void SetValueWithoutNotify(bool newValue)
        {
            value = newValue;
            if (row != null)
            {
                row.SetSelected(value);
                row.SetTooltipCurrent(value ? "ON" : "OFF");
            }

            ApplyVisual();
        }

        internal void SetTooltipCurrent(string current)
        {
            if (row != null)
            {
                row.SetTooltipCurrent(current);
            }
        }

        internal void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }

            if (row != null)
            {
                row.SetSelectableVisual(interactable);
            }
        }

        private void Toggle()
        {
            if (button != null && !button.interactable)
            {
                return;
            }

            value = !value;
            if (row != null)
            {
                row.SetSelected(value);
                row.SetTooltipCurrent(value ? "ON" : "OFF");
                row.Flash();
            }

            ApplyVisual();
            if (changedHandler != null)
            {
                changedHandler(value);
            }
        }

        private void ApplyVisual()
        {
            if (knob != null)
            {
                knob.anchoredPosition = new Vector2(value ? 74f : 18f, 0f);
            }

            if (valueText != null)
            {
                KaelisMenuLocalizationService.SetText(valueText, value ? "ON" : "OFF");
            }
        }

        private static string ToObjectName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "Toggle";
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

            return writeIndex > 0 ? new string(characters, 0, writeIndex) : "Toggle";
        }

        private static string GetStatusLabel(KaelisMenuBindingStatus status)
        {
            switch (status)
            {
                case KaelisMenuBindingStatus.PartialBinding:
                    return "PARTIAL";
                case KaelisMenuBindingStatus.Reserved:
                    return "RESERVED";
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
    }
}
