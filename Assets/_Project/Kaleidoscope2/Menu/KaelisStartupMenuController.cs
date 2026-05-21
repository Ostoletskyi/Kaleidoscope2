using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class KaelisStartupMenuController : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private static readonly Color BackgroundTint = new Color(0.04f, 0.05f, 0.07f, 1f);
        private static readonly Color Scrim = new Color(0f, 0f, 0f, 0.54f);
        private static readonly Color Panel = new Color(0.025f, 0.035f, 0.05f, 0.78f);
        private static readonly Color PanelStrong = new Color(0.035f, 0.045f, 0.06f, 0.88f);
        private static readonly Color ButtonNormal = new Color(0.055f, 0.07f, 0.085f, 0.92f);
        private static readonly Color ButtonHover = new Color(0.08f, 0.13f, 0.145f, 0.96f);
        private static readonly Color ButtonPressed = new Color(0.68f, 0.14f, 0.09f, 1f);
        private static readonly Color ButtonSelected = new Color(0.13f, 0.18f, 0.19f, 1f);
        private static readonly Color TextPrimary = new Color(0.95f, 0.98f, 1f, 1f);
        private static readonly Color TextSecondary = new Color(0.66f, 0.76f, 0.82f, 1f);
        private static readonly Color Gold = new Color(1f, 0.72f, 0.36f, 1f);
        private static readonly Color Cyan = new Color(0.22f, 0.86f, 1f, 1f);
        private static readonly Color RedGlow = new Color(1f, 0.18f, 0.12f, 1f);

        [Header("Startup")]
        [SerializeField] private bool startVisible = true;

        [Header("Concept References")]
        [SerializeField] private Texture backgroundTexture;
        [SerializeField] private Texture previewTexture;
        [SerializeField] private Texture fallbackConceptTexture;

        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private RectTransform canvasRoot;
        private GameObject menuRoot;
        private TMP_Text statusText;
        private TMP_Text demoStateText;
        private Toggle demoToggle;
        private TMP_FontAsset runtimeFontAsset;
        private bool runtimeFontAssetGenerated;
        private Texture2D solidTexture;
        private Sprite solidSprite;
        private bool demoModeEnabled;
        private bool visible;

        public bool DemoModeEnabled
        {
            get { return demoModeEnabled; }
        }

        private void Awake()
        {
            EnsureGeneratedAssets();
            EnsureEventSystem();
            BuildMenu();
            SetVisible(startVisible);
            UpdateDemoState(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetVisible(!visible);
            }
        }

        private void OnDestroy()
        {
            if (runtimeFontAssetGenerated)
            {
                DestroyGeneratedAsset(runtimeFontAsset);
            }

            DestroyGeneratedAsset(solidSprite);
            DestroyGeneratedAsset(solidTexture);
        }

        private void BuildMenu()
        {
            menuRoot = new GameObject("MainMenuCanvas", typeof(RectTransform));
            menuRoot.transform.SetParent(transform, false);

            canvasRoot = (RectTransform)menuRoot.transform;
            Stretch(canvasRoot);

            canvas = menuRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = menuRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            menuRoot.AddComponent<GraphicRaycaster>();
            canvasGroup = menuRoot.AddComponent<CanvasGroup>();

            CreateBackground(canvasRoot);

            RectTransform layoutRoot = CreateRect("SafeFrame", canvasRoot);
            layoutRoot.anchorMin = Vector2.zero;
            layoutRoot.anchorMax = Vector2.one;
            layoutRoot.offsetMin = new Vector2(64f, 56f);
            layoutRoot.offsetMax = new Vector2(-64f, -72f);

            RectTransform leftPanel = CreateGlassPanel("LeftControlPanel", layoutRoot, new Vector2(0f, 0f), new Vector2(0f, 1f));
            leftPanel.pivot = new Vector2(0f, 0.5f);
            leftPanel.sizeDelta = new Vector2(450f, 0f);
            leftPanel.offsetMin = new Vector2(0f, 0f);
            leftPanel.offsetMax = new Vector2(450f, 0f);

            RectTransform previewPanel = CreatePreviewPanel(layoutRoot);
            BuildLeftPanel(leftPanel);
            BuildBottomStatus(canvasRoot);
            BuildPreviewPanel(previewPanel);
        }

        private void CreateBackground(RectTransform parent)
        {
            Texture selectedTexture = backgroundTexture != null ? backgroundTexture : fallbackConceptTexture;

            RawImage background = CreateRawImage("CinematicBackground", parent, selectedTexture);
            Stretch(background.rectTransform);
            background.color = selectedTexture != null ? Color.white : BackgroundTint;
            background.raycastTarget = false;

            if (selectedTexture == null)
            {
                Image fallback = background.gameObject.AddComponent<Image>();
                fallback.sprite = solidSprite;
                fallback.color = BackgroundTint;
                fallback.raycastTarget = false;
            }

            RawImage vignette = CreateRawImage("DarkVignette", parent, null);
            Stretch(vignette.rectTransform);
            vignette.texture = solidTexture;
            vignette.color = Scrim;
            vignette.raycastTarget = false;

            RectTransform redWash = CreateRect("RedCyanAtmosphere", parent);
            Stretch(redWash);
            Image washImage = redWash.gameObject.AddComponent<Image>();
            washImage.sprite = solidSprite;
            washImage.color = new Color(0.12f, 0.02f, 0.025f, 0.32f);
            washImage.raycastTarget = false;
        }

        private void BuildLeftPanel(RectTransform panel)
        {
            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(34, 34, 34, 28);
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TMP_Text eyebrow = CreateText(panel, "Eyebrow", "OPTICAL EXPERIENCE ENGINE", 17f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left);
            eyebrow.characterSpacing = 18f;
            AddLayout(eyebrow.gameObject, -1f, 28f);

            TMP_Text title = CreateText(panel, "Title", "KAELIS", 68f, FontStyles.Normal, TextPrimary, TextAlignmentOptions.Left);
            title.characterSpacing = 6f;
            AddLayout(title.gameObject, -1f, 82f);

            TMP_Text subtitle = CreateText(panel, "Subtitle", "BEYOND THE REFLECTION", 19f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left);
            subtitle.characterSpacing = 8f;
            AddLayout(subtitle.gameObject, -1f, 34f);

            AddSpacer(panel, 18f);

            Button enterButton = CreateMenuButton(panel, "EnterExperienceButton", "ENTER EXPERIENCE", Gold);
            enterButton.onClick.AddListener(EnterExperience);

            demoToggle = CreateDemoToggle(panel);
            demoToggle.onValueChanged.AddListener(UpdateDemoState);

            Button modesButton = CreateMenuButton(panel, "ModesButton", "MODES", Cyan);
            modesButton.onClick.AddListener(() => LogPlaceholder("Modes"));

            Button opticsButton = CreateMenuButton(panel, "OpticsButton", "OPTICS", Cyan);
            opticsButton.onClick.AddListener(() => LogPlaceholder("Optics"));

            Button presetsButton = CreateMenuButton(panel, "PresetsButton", "PRESETS", Cyan);
            presetsButton.onClick.AddListener(() => LogPlaceholder("Presets"));

            Button settingsButton = CreateMenuButton(panel, "SettingsButton", "SETTINGS", Gold);
            settingsButton.onClick.AddListener(() => LogPlaceholder("Settings"));

            Button exitButton = CreateMenuButton(panel, "ExitButton", "EXIT", RedGlow);
            exitButton.onClick.AddListener(ExitApplication);

            AddSpacer(panel, 10f);
            demoStateText = CreateText(panel, "DemoState", "DEMO MODE  OFF", 14f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left);
            demoStateText.characterSpacing = 6f;
            AddLayout(demoStateText.gameObject, -1f, 24f);
        }

        private RectTransform CreatePreviewPanel(RectTransform parent)
        {
            RectTransform panel = CreateGlassPanel("PreviewPanel", parent, new Vector2(0f, 0f), new Vector2(1f, 1f));
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.offsetMin = new Vector2(506f, 0f);
            panel.offsetMax = Vector2.zero;
            return panel;
        }

        private void BuildPreviewPanel(RectTransform panel)
        {
            RectTransform header = CreateRect("PreviewHeader", panel);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.offsetMin = new Vector2(28f, -74f);
            header.offsetMax = new Vector2(-28f, -26f);

            TMP_Text title = CreateText(header, "Label", "LIVE PREVIEW", 18f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left);
            title.characterSpacing = 10f;
            Stretch(title.rectTransform);

            RectTransform previewFrame = CreateRect("RawImagePreviewFrame", panel);
            previewFrame.anchorMin = new Vector2(0f, 0f);
            previewFrame.anchorMax = new Vector2(1f, 1f);
            previewFrame.offsetMin = new Vector2(32f, 34f);
            previewFrame.offsetMax = new Vector2(-32f, -98f);

            Image frame = previewFrame.gameObject.AddComponent<Image>();
            frame.sprite = solidSprite;
            frame.color = new Color(0.015f, 0.02f, 0.03f, 0.5f);
            frame.raycastTarget = false;
            AddFrame(previewFrame, Cyan, Gold, 2f);

            RawImage preview = CreateRawImage("PreviewRawImage", previewFrame, previewTexture != null ? previewTexture : backgroundTexture);
            Stretch(preview.rectTransform);
            preview.rectTransform.offsetMin = new Vector2(10f, 10f);
            preview.rectTransform.offsetMax = new Vector2(-10f, -10f);
            preview.color = new Color(1f, 1f, 1f, 0.36f);
            preview.raycastTarget = false;

            RectTransform overlay = CreateRect("PreviewPlaceholderOverlay", previewFrame);
            Stretch(overlay);
            overlay.offsetMin = new Vector2(10f, 10f);
            overlay.offsetMax = new Vector2(-10f, -10f);
            Image overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.sprite = solidSprite;
            overlayImage.color = new Color(0.02f, 0.025f, 0.035f, 0.42f);
            overlayImage.raycastTarget = false;

            TMP_Text placeholder = CreateText(overlay, "PlaceholderText", "PREVIEW PANEL", 28f, FontStyles.Normal, TextPrimary, TextAlignmentOptions.Center);
            placeholder.characterSpacing = 8f;
            Stretch(placeholder.rectTransform);

            TMP_Text sub = CreateText(overlay, "PlaceholderSubtitle", "RAWIMAGE SURFACE", 14f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Center);
            sub.characterSpacing = 8f;
            sub.rectTransform.anchorMin = new Vector2(0f, 0.44f);
            sub.rectTransform.anchorMax = new Vector2(1f, 0.44f);
            sub.rectTransform.offsetMin = new Vector2(0f, -28f);
            sub.rectTransform.offsetMax = new Vector2(0f, 4f);
        }

        private void BuildBottomStatus(RectTransform parent)
        {
            RectTransform statusBar = CreateRect("BottomStatusBar", parent);
            statusBar.anchorMin = new Vector2(0f, 0f);
            statusBar.anchorMax = new Vector2(1f, 0f);
            statusBar.pivot = new Vector2(0.5f, 0f);
            statusBar.sizeDelta = new Vector2(0f, 46f);
            statusBar.anchoredPosition = Vector2.zero;

            Image background = statusBar.gameObject.AddComponent<Image>();
            background.sprite = solidSprite;
            background.color = new Color(0.015f, 0.02f, 0.028f, 0.9f);
            background.raycastTarget = false;

            AddLine(statusBar, "TopLine", new Vector2(0f, 1f), new Vector2(1f, 1f), 2f, Cyan);

            statusText = CreateText(statusBar, "StatusText", "SYSTEM READY  |  STARTUP MENU", 15f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left);
            statusText.characterSpacing = 6f;
            statusText.rectTransform.anchorMin = Vector2.zero;
            statusText.rectTransform.anchorMax = Vector2.one;
            statusText.rectTransform.offsetMin = new Vector2(66f, 0f);
            statusText.rectTransform.offsetMax = new Vector2(-66f, 0f);
        }

        private Toggle CreateDemoToggle(RectTransform parent)
        {
            RectTransform row = CreateButtonBase(parent, "DemoModeToggle", "DEMO MODE", Gold, out TMP_Text label);
            label.text = "DEMO MODE";

            Toggle toggle = row.gameObject.AddComponent<Toggle>();
            toggle.transition = Selectable.Transition.ColorTint;
            toggle.targetGraphic = row.GetComponent<Image>();
            toggle.colors = CreateToggleColors();

            RectTransform box = CreateRect("ToggleBox", row);
            box.anchorMin = new Vector2(1f, 0.5f);
            box.anchorMax = new Vector2(1f, 0.5f);
            box.pivot = new Vector2(1f, 0.5f);
            box.sizeDelta = new Vector2(32f, 32f);
            box.anchoredPosition = new Vector2(-20f, 0f);

            Image boxImage = box.gameObject.AddComponent<Image>();
            boxImage.sprite = solidSprite;
            boxImage.color = new Color(0f, 0f, 0f, 0.46f);
            boxImage.raycastTarget = false;
            AddFrame(box, Cyan, Gold, 1f);

            RectTransform check = CreateRect("Checkmark", box);
            check.anchorMin = new Vector2(0.5f, 0.5f);
            check.anchorMax = new Vector2(0.5f, 0.5f);
            check.pivot = new Vector2(0.5f, 0.5f);
            check.sizeDelta = new Vector2(18f, 18f);
            check.anchoredPosition = Vector2.zero;

            Image checkImage = check.gameObject.AddComponent<Image>();
            checkImage.sprite = solidSprite;
            checkImage.color = Gold;
            checkImage.raycastTarget = false;
            toggle.graphic = checkImage;
            toggle.SetIsOnWithoutNotify(false);

            return toggle;
        }

        private Button CreateMenuButton(RectTransform parent, string name, string label, Color accent)
        {
            RectTransform buttonRoot = CreateButtonBase(parent, name, label, accent, out TMP_Text _);
            Button button = buttonRoot.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = buttonRoot.GetComponent<Image>();
            button.colors = CreateButtonColors();
            return button;
        }

        private RectTransform CreateButtonBase(RectTransform parent, string name, string label, Color accent, out TMP_Text labelText)
        {
            RectTransform buttonRoot = CreateRect(name, parent);
            AddLayout(buttonRoot.gameObject, -1f, 56f);

            Image image = buttonRoot.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = ButtonNormal;
            image.raycastTarget = true;

            AddFrame(buttonRoot, accent, new Color(accent.r, accent.g, accent.b, 0.42f), 1.5f);

            RectTransform accentLine = CreateRect("AccentLine", buttonRoot);
            accentLine.anchorMin = new Vector2(0f, 0f);
            accentLine.anchorMax = new Vector2(0f, 1f);
            accentLine.pivot = new Vector2(0f, 0.5f);
            accentLine.sizeDelta = new Vector2(4f, 0f);
            accentLine.anchoredPosition = Vector2.zero;

            Image accentImage = accentLine.gameObject.AddComponent<Image>();
            accentImage.sprite = solidSprite;
            accentImage.color = accent;
            accentImage.raycastTarget = false;

            labelText = CreateText(buttonRoot, "Label", label, 18f, FontStyles.Normal, TextPrimary, TextAlignmentOptions.MidlineLeft);
            labelText.characterSpacing = 6f;
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = new Vector2(24f, 0f);
            labelText.rectTransform.offsetMax = new Vector2(-68f, 0f);

            return buttonRoot;
        }

        private RectTransform CreateGlassPanel(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform panel = CreateRect(name, parent);
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;

            Image image = panel.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = Panel;
            image.raycastTarget = true;

            AddFrame(panel, Cyan, Gold, 2f);

            Shadow shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.52f);
            shadow.effectDistance = new Vector2(12f, -12f);

            return panel;
        }

        private TMP_Text CreateText(RectTransform parent, string name, string value, float size, FontStyles style, Color color, TextAlignmentOptions alignment)
        {
            RectTransform rect = CreateRect(name, parent);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset fontAsset = GetRuntimeFontAsset();
            if (fontAsset != null)
            {
                text.font = fontAsset;
            }

            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            Stretch(rect);
            return text;
        }

        private RawImage CreateRawImage(string name, RectTransform parent, Texture texture)
        {
            RectTransform rect = CreateRect(name, parent);
            RawImage image = rect.gameObject.AddComponent<RawImage>();
            image.texture = texture;
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        private void EnterExperience()
        {
            SetVisible(false);
            SetStatus("EXPERIENCE ACTIVE");
            Debug.Log("[KAELIS Menu] Enter Experience selected. Startup menu hidden.");
        }

        private void LogPlaceholder(string label)
        {
            SetStatus(label.ToUpperInvariant() + " PLACEHOLDER");
            Debug.Log("[KAELIS Menu] " + label + " selected. Placeholder action for Menu Stage 01.");
        }

        private void ExitApplication()
        {
            Debug.Log("[KAELIS Menu] Exit selected.");
#if UNITY_EDITOR
            Debug.Log("[KAELIS Menu] Application.Quit skipped in the Unity editor.");
#else
            Application.Quit();
#endif
        }

        private void UpdateDemoState(bool enabled)
        {
            demoModeEnabled = enabled;
            if (demoStateText != null)
            {
                demoStateText.text = "DEMO MODE  " + (demoModeEnabled ? "ON" : "OFF");
                demoStateText.color = demoModeEnabled ? Gold : TextSecondary;
            }

            SetStatus(demoModeEnabled ? "DEMO MODE STORED  |  NO DEMO PIPELINE YET" : "SYSTEM READY  |  DEMO OFF");
            Debug.Log("[KAELIS Menu] Demo Mode stored: " + (demoModeEnabled ? "ON" : "OFF") + ".");
        }

        private void SetVisible(bool shouldShow)
        {
            visible = shouldShow;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
                canvasGroup.interactable = visible;
            }

            if (menuRoot != null && menuRoot.activeSelf != visible)
            {
                menuRoot.SetActive(visible);
            }

            if (visible)
            {
                SetStatus("SYSTEM READY  |  STARTUP MENU");
            }
        }

        private void SetStatus(string value)
        {
            if (statusText != null)
            {
                statusText.text = value;
            }
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("RuntimeEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(transform, false);
        }

        private void EnsureGeneratedAssets()
        {
            if (solidTexture != null)
            {
                return;
            }

            solidTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            solidTexture.name = "KAELIS_Menu_Solid";
            solidTexture.hideFlags = HideFlags.HideAndDontSave;
            solidTexture.SetPixel(0, 0, Color.white);
            solidTexture.Apply(false, true);

            solidSprite = Sprite.Create(solidTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            solidSprite.name = "KAELIS_Menu_SolidSprite";
            solidSprite.hideFlags = HideFlags.HideAndDontSave;
        }

        private TMP_FontAsset GetRuntimeFontAsset()
        {
            if (runtimeFontAsset != null)
            {
                return runtimeFontAsset;
            }

            runtimeFontAsset = GetTmpDefaultFontAsset();
            runtimeFontAssetGenerated = false;
            if (runtimeFontAsset != null)
            {
                return runtimeFontAsset;
            }

            runtimeFontAsset = CreateFontAssetFromOsFonts();
            if (runtimeFontAsset == null)
            {
                runtimeFontAsset = CreateFontAssetFromBuiltin("Arial.ttf");
            }

            if (runtimeFontAsset == null)
            {
                runtimeFontAsset = CreateFontAssetFromBuiltin("LegacyRuntime.ttf");
            }

            if (runtimeFontAsset != null)
            {
                runtimeFontAsset.name = "KAELIS_RuntimeTMPFont";
                runtimeFontAsset.hideFlags = HideFlags.HideAndDontSave;
                runtimeFontAssetGenerated = true;
                return runtimeFontAsset;
            }

            return runtimeFontAsset;
        }

        private static TMP_FontAsset CreateFontAssetFromOsFonts()
        {
            try
            {
                Font baseFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Segoe UI", "Arial", "Liberation Sans", "Noto Sans" },
                    90);

                if (baseFont == null)
                {
                    return null;
                }

                return TMP_FontAsset.CreateFontAsset(baseFont);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[KAELIS Menu] Could not create TMP font asset from OS fonts: " + exception.Message);
                return null;
            }
        }

        private static TMP_FontAsset CreateFontAssetFromBuiltin(string fontName)
        {
            Font baseFont = LoadBuiltinFont(fontName);
            if (baseFont == null)
            {
                return null;
            }

            try
            {
                return TMP_FontAsset.CreateFontAsset(baseFont);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[KAELIS Menu] Could not create TMP font asset from " + fontName + ": " + exception.Message);
                return null;
            }
        }

        private static Font LoadBuiltinFont(string fontName)
        {
            try
            {
                return Resources.GetBuiltinResource<Font>(fontName);
            }
            catch (System.ArgumentException)
            {
                return null;
            }
        }

        private static TMP_FontAsset GetTmpDefaultFontAsset()
        {
            try
            {
                return TMP_Settings.defaultFontAsset;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[KAELIS Menu] TMP default font asset is unavailable: " + exception.Message);
                return null;
            }
        }

        private static ColorBlock CreateButtonColors()
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = ButtonNormal;
            colors.highlightedColor = ButtonHover;
            colors.pressedColor = ButtonPressed;
            colors.selectedColor = ButtonSelected;
            colors.disabledColor = new Color(0.05f, 0.05f, 0.05f, 0.55f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            return colors;
        }

        private static ColorBlock CreateToggleColors()
        {
            ColorBlock colors = CreateButtonColors();
            colors.selectedColor = PanelStrong;
            return colors;
        }

        private void AddFrame(RectTransform target, Color primary, Color secondary, float thickness)
        {
            AddLine(target, "FrameTop", new Vector2(0f, 1f), new Vector2(1f, 1f), thickness, primary);
            AddLine(target, "FrameBottom", new Vector2(0f, 0f), new Vector2(1f, 0f), thickness, secondary);
            AddLine(target, "FrameLeft", new Vector2(0f, 0f), new Vector2(0f, 1f), thickness, secondary);
            AddLine(target, "FrameRight", new Vector2(1f, 0f), new Vector2(1f, 1f), thickness, primary);
        }

        private void AddLine(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float thickness, Color color)
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

            Image image = line.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = color;
            image.raycastTarget = false;

            LayoutElement layoutElement = line.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }

        private void AddSpacer(RectTransform parent, float height)
        {
            RectTransform spacer = CreateRect("Spacer", parent);
            AddLayout(spacer.gameObject, -1f, height);
        }

        private static void AddLayout(GameObject target, float preferredWidth, float preferredHeight)
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

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        private static void DestroyGeneratedAsset(Object asset)
        {
            if (asset == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(asset);
            }
            else
            {
                DestroyImmediate(asset);
            }
        }
    }
}
