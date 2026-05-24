using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    public sealed class KaelisMenuInteractiveRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private Image surface;
        private CanvasGroup highlightGroup;
        private CanvasGroup flashGroup;
        private KaelisMenuTooltip tooltip;
        private RectTransform tooltipAnchor;
        private string tooltipTitle;
        private string tooltipBody;
        private string tooltipRange;
        private string tooltipCurrent;
        private string tooltipKeys;
        private Color baseColor;
        private Color hoverColor;
        private bool hovered;
        private bool pressed;
        private bool focused;
        private bool selected;
        private bool selectable = true;
        private float highlightAlpha;
        private float flashAlpha;

        public bool IsActiveForKeyboard
        {
            get { return hovered || focused || selected; }
        }

        public bool IsSelected
        {
            get { return selected; }
        }

        public bool HasTooltipData
        {
            get { return tooltip != null && !string.IsNullOrWhiteSpace(tooltipTitle) && tooltipAnchor != null; }
        }

        public string TooltipKeys
        {
            get { return tooltipKeys; }
        }

        internal void Configure(Image surface, CanvasGroup highlightGroup, CanvasGroup flashGroup, KaelisMenuTooltip tooltip, string title, string body, string range, string current, string keys)
        {
            this.surface = surface;
            this.highlightGroup = highlightGroup;
            this.flashGroup = flashGroup;
            this.tooltip = tooltip;
            tooltipAnchor = (RectTransform)transform;
            tooltipTitle = title;
            tooltipBody = body;
            tooltipRange = range;
            tooltipCurrent = current;
            tooltipKeys = keys;

            if (surface != null)
            {
                baseColor = surface.color;
                hoverColor = Color.Lerp(baseColor, new Color(1f, 0.72f, 0.28f, baseColor.a), 0.18f);
            }
        }

        public void SetSelected(bool value)
        {
            selected = value;
        }

        public void SetSelectableVisual(bool value)
        {
            selectable = value;
            if (surface != null)
            {
                surface.color = selectable ? baseColor : new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * 0.44f);
            }
        }

        public void Flash()
        {
            flashAlpha = 1f;
        }

        public void SetTooltipCurrent(string current)
        {
            tooltipCurrent = current;
            if (hovered && tooltip != null)
            {
                ShowTooltip();
            }
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
            if (tooltip != null)
            {
                tooltip.Hide();
            }
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
            focused = true;
            ShowTooltip();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            focused = false;
            pressed = false;
            if (!hovered && tooltip != null)
            {
                tooltip.Hide();
            }
        }

        private void Update()
        {
            float delta = Application.isPlaying ? Time.unscaledDeltaTime : 0.016f;
            float target = selected ? 1f : (pressed ? 0.92f : (hovered || focused ? 0.74f : 0f));
            if (!selectable)
            {
                target *= 0.42f;
            }

            highlightAlpha = Mathf.MoveTowards(highlightAlpha, target, delta * 8f);
            flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, delta * 5.5f);

            if (highlightGroup != null)
            {
                highlightGroup.alpha = highlightAlpha;
            }

            if (flashGroup != null)
            {
                flashGroup.alpha = flashAlpha;
            }

            if (surface != null && selectable)
            {
                Color targetColor = hovered || focused || selected || pressed ? hoverColor : baseColor;
                surface.color = Color.Lerp(surface.color, targetColor, delta * 10f);
            }
        }

        private void ShowTooltip()
        {
            if (tooltip != null)
            {
                tooltip.Show(tooltipAnchor, tooltipTitle, tooltipBody, tooltipRange, tooltipCurrent, tooltipKeys);
            }
        }
    }
}
