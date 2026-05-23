using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    internal static class KaelisMenuUiPrimitives
    {
        public static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        public static Image AddImage(RectTransform rect, Sprite sprite, Color color, bool raycastTarget)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static RawImage AddRawImage(RectTransform rect, Texture texture, Color color, bool raycastTarget)
        {
            RawImage image = rect.gameObject.AddComponent<RawImage>();
            image.texture = texture;
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static TMP_Text CreateText(RectTransform parent, string name, string value, float size, Color color, TextAlignmentOptions alignment, TMP_FontAsset font)
        {
            RectTransform rect = CreateRect(name, parent);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null)
            {
                text.font = font;
            }

            text.text = value;
            text.fontSize = size;
            text.fontStyle = FontStyles.Normal;
            text.color = color;
            text.alignment = alignment;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            Stretch(rect);
            return text;
        }

        public static void AddLayout(GameObject target, float preferredWidth, float preferredHeight)
        {
            LayoutElement element = target.AddComponent<LayoutElement>();
            if (preferredWidth >= 0f)
            {
                element.preferredWidth = preferredWidth;
            }

            if (preferredHeight >= 0f)
            {
                element.preferredHeight = preferredHeight;
            }
        }

        public static void AddSpacer(RectTransform parent, float height)
        {
            RectTransform spacer = CreateRect("Spacer", parent);
            AddLayout(spacer.gameObject, -1f, height);
        }

        public static void AddAmbientBand(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color, Sprite solidSprite)
        {
            RectTransform band = CreateRect(name, parent);
            band.anchorMin = anchorMin;
            band.anchorMax = anchorMax;
            band.offsetMin = Vector2.zero;
            band.offsetMax = Vector2.zero;
            AddImage(band, solidSprite, color, false);
            IgnoreLayout(band.gameObject);
        }

        public static void AddFrame(RectTransform target, Color primary, Color secondary, float thickness, Sprite solidSprite)
        {
            AddLine(target, "FrameTop", new Vector2(0f, 1f), new Vector2(1f, 1f), thickness, primary, solidSprite);
            AddLine(target, "FrameBottom", new Vector2(0f, 0f), new Vector2(1f, 0f), thickness, secondary, solidSprite);
            AddLine(target, "FrameLeft", new Vector2(0f, 0f), new Vector2(0f, 1f), thickness, secondary, solidSprite);
            AddLine(target, "FrameRight", new Vector2(1f, 0f), new Vector2(1f, 1f), thickness, primary, solidSprite);
        }

        public static void AddInsetFrame(RectTransform target, Color color, float inset, float thickness, Sprite solidSprite)
        {
            RectTransform insetFrame = CreateRect("InsetFrame", target);
            Stretch(insetFrame);
            insetFrame.offsetMin = new Vector2(inset, inset);
            insetFrame.offsetMax = new Vector2(-inset, -inset);
            AddFrame(insetFrame, color, new Color(color.r, color.g, color.b, color.a * 0.7f), thickness, solidSprite);
            IgnoreLayout(insetFrame.gameObject);
        }

        public static void AddCornerCuts(RectTransform target, Color color, float length, float thickness, Sprite solidSprite)
        {
            AddCornerCut(target, "CornerTopLeft", new Vector2(0f, 1f), new Vector2(length * 0.35f, -length * 0.35f), -45f, color, length, thickness, solidSprite);
            AddCornerCut(target, "CornerTopRight", new Vector2(1f, 1f), new Vector2(-length * 0.35f, -length * 0.35f), 45f, color, length, thickness, solidSprite);
            AddCornerCut(target, "CornerBottomLeft", new Vector2(0f, 0f), new Vector2(length * 0.35f, length * 0.35f), 45f, color, length, thickness, solidSprite);
            AddCornerCut(target, "CornerBottomRight", new Vector2(1f, 0f), new Vector2(-length * 0.35f, length * 0.35f), -45f, color, length, thickness, solidSprite);
        }

        public static RectTransform AddLine(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float thickness, Color color, Sprite solidSprite)
        {
            RectTransform line = CreateRect(name, parent);
            line.anchorMin = anchorMin;
            line.anchorMax = anchorMax;

            bool horizontal = Mathf.Approximately(anchorMin.y, anchorMax.y);
            if (horizontal)
            {
                line.pivot = new Vector2(0.5f, anchorMin.y);
                line.sizeDelta = new Vector2(0f, thickness);
            }
            else
            {
                line.pivot = new Vector2(anchorMin.x, 0.5f);
                line.sizeDelta = new Vector2(thickness, 0f);
            }

            line.anchoredPosition = Vector2.zero;
            AddImage(line, solidSprite, color, false);
            IgnoreLayout(line.gameObject);
            return line;
        }

        public static void AddIconGeometry(RectTransform root, KaelisMenuIconKind kind, Color color, float alpha, float thickness, Sprite solidSprite)
        {
            Color iconColor = new Color(color.r, color.g, color.b, alpha);

            switch (kind)
            {
                case KaelisMenuIconKind.Diamond:
                    AddIconLineBetween(root, "DiamondA", new Vector2(0f, 14f), new Vector2(15f, 2f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "DiamondB", new Vector2(15f, 2f), new Vector2(0f, -15f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "DiamondC", new Vector2(0f, -15f), new Vector2(-15f, 2f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "DiamondD", new Vector2(-15f, 2f), new Vector2(0f, 14f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "DiamondCore", new Vector2(0f, 14f), new Vector2(0f, -15f), new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * 0.65f), thickness * 0.7f, solidSprite);
                    break;
                case KaelisMenuIconKind.Monitor:
                    AddIconBox(root, "Screen", new Vector2(0f, 4f), new Vector2(24f, 15f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "Stand", new Vector2(0f, -4f), new Vector2(0f, -13f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "Base", new Vector2(-8f, -13f), new Vector2(8f, -13f), iconColor, thickness, solidSprite);
                    break;
                case KaelisMenuIconKind.Layers:
                    AddIconDiamond(root, "LayerA", new Vector2(0f, 8f), new Vector2(24f, 12f), iconColor, thickness, solidSprite);
                    AddIconDiamond(root, "LayerB", new Vector2(0f, 0f), new Vector2(24f, 12f), iconColor, thickness, solidSprite);
                    AddIconDiamond(root, "LayerC", new Vector2(0f, -8f), new Vector2(24f, 12f), iconColor, thickness, solidSprite);
                    break;
                case KaelisMenuIconKind.Optics:
                    AddIconBox(root, "OpticBox", Vector2.zero, new Vector2(23f, 23f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "OpticH", new Vector2(-15f, 0f), new Vector2(15f, 0f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "OpticV", new Vector2(0f, -15f), new Vector2(0f, 15f), iconColor, thickness, solidSprite);
                    AddIconBox(root, "OpticCore", Vector2.zero, new Vector2(9f, 9f), iconColor, thickness, solidSprite);
                    break;
                case KaelisMenuIconKind.Star:
                    AddIconLineBetween(root, "StarA", new Vector2(0f, 15f), new Vector2(0f, -15f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "StarB", new Vector2(-13f, 8f), new Vector2(13f, -8f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "StarC", new Vector2(13f, 8f), new Vector2(-13f, -8f), iconColor, thickness, solidSprite);
                    break;
                case KaelisMenuIconKind.Settings:
                    AddIconBox(root, "SettingsCore", Vector2.zero, new Vector2(13f, 13f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "SettingsH", new Vector2(-15f, 0f), new Vector2(15f, 0f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "SettingsV", new Vector2(0f, -15f), new Vector2(0f, 15f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "SettingsD1", new Vector2(-10f, -10f), new Vector2(10f, 10f), new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * 0.55f), thickness, solidSprite);
                    AddIconLineBetween(root, "SettingsD2", new Vector2(-10f, 10f), new Vector2(10f, -10f), new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * 0.55f), thickness, solidSprite);
                    break;
                case KaelisMenuIconKind.Exit:
                    AddIconLineBetween(root, "ExitStem", new Vector2(0f, 13f), new Vector2(0f, -2f), iconColor, thickness * 1.2f, solidSprite);
                    AddIconLineBetween(root, "ExitLeft", new Vector2(-11f, 5f), new Vector2(-11f, -12f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "ExitRight", new Vector2(11f, 5f), new Vector2(11f, -12f), iconColor, thickness, solidSprite);
                    AddIconLineBetween(root, "ExitBase", new Vector2(-11f, -12f), new Vector2(11f, -12f), iconColor, thickness, solidSprite);
                    break;
            }
        }

        public static void AddTextGlow(GameObject target, Color color, Vector2 distance)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = false;
        }

        private static void AddCornerCut(RectTransform target, string name, Vector2 anchor, Vector2 anchoredPosition, float rotation, Color color, float length, float thickness, Sprite solidSprite)
        {
            RectTransform line = CreateRect(name, target);
            line.anchorMin = anchor;
            line.anchorMax = anchor;
            line.pivot = new Vector2(0.5f, 0.5f);
            line.sizeDelta = new Vector2(length, thickness);
            line.anchoredPosition = anchoredPosition;
            line.localEulerAngles = new Vector3(0f, 0f, rotation);
            AddImage(line, solidSprite, color, false);
            IgnoreLayout(line.gameObject);
        }

        private static void AddIconBox(RectTransform root, string name, Vector2 center, Vector2 size, Color color, float thickness, Sprite solidSprite)
        {
            float halfWidth = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;
            AddIconLineBetween(root, name + "Top", center + new Vector2(-halfWidth, halfHeight), center + new Vector2(halfWidth, halfHeight), color, thickness, solidSprite);
            AddIconLineBetween(root, name + "Bottom", center + new Vector2(-halfWidth, -halfHeight), center + new Vector2(halfWidth, -halfHeight), color, thickness, solidSprite);
            AddIconLineBetween(root, name + "Left", center + new Vector2(-halfWidth, -halfHeight), center + new Vector2(-halfWidth, halfHeight), color, thickness, solidSprite);
            AddIconLineBetween(root, name + "Right", center + new Vector2(halfWidth, -halfHeight), center + new Vector2(halfWidth, halfHeight), color, thickness, solidSprite);
        }

        private static void AddIconDiamond(RectTransform root, string name, Vector2 center, Vector2 size, Color color, float thickness, Sprite solidSprite)
        {
            Vector2 top = center + new Vector2(0f, size.y * 0.5f);
            Vector2 right = center + new Vector2(size.x * 0.5f, 0f);
            Vector2 bottom = center + new Vector2(0f, -size.y * 0.5f);
            Vector2 left = center + new Vector2(-size.x * 0.5f, 0f);
            AddIconLineBetween(root, name + "A", top, right, color, thickness, solidSprite);
            AddIconLineBetween(root, name + "B", right, bottom, color, thickness, solidSprite);
            AddIconLineBetween(root, name + "C", bottom, left, color, thickness, solidSprite);
            AddIconLineBetween(root, name + "D", left, top, color, thickness, solidSprite);
        }

        private static void AddIconLineBetween(RectTransform root, string name, Vector2 start, Vector2 end, Color color, float thickness, Sprite solidSprite)
        {
            Vector2 delta = end - start;
            RectTransform line = CreateRect(name, root);
            line.anchorMin = new Vector2(0.5f, 0.5f);
            line.anchorMax = new Vector2(0.5f, 0.5f);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.sizeDelta = new Vector2(delta.magnitude, thickness);
            line.anchoredPosition = (start + end) * 0.5f;
            line.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            AddImage(line, solidSprite, color, false);
            IgnoreLayout(line.gameObject);
        }

        private static void IgnoreLayout(GameObject target)
        {
            LayoutElement layoutElement = target.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }
    }
}
