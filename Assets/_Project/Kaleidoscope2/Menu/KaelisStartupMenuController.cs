using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    internal enum KaelisMenuButtonVisualKind
    {
        Primary,
        Secondary,
        Demo,
        Exit
    }

    internal enum KaelisMenuFontRole
    {
        Logo,
        Subtitle,
        PrimaryButton,
        SecondaryLabel,
        PreviewLabel,
        StatusText
    }

    internal enum KaelisMenuIconKind
    {
        Diamond,
        Monitor,
        Layers,
        Optics,
        Star,
        Settings,
        Exit
    }

    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class KaelisStartupMenuController : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float VisibilityFadeSeconds = 0.22f;

        private static readonly Color BackgroundTint = new Color(0.04f, 0.05f, 0.07f, 1f);
        private static readonly Color Scrim = new Color(0f, 0f, 0f, 0.64f);
        private static readonly Color Panel = new Color(0.018f, 0.027f, 0.038f, 0.82f);
        private static readonly Color PanelSoft = new Color(0.08f, 0.12f, 0.15f, 0.24f);
        private static readonly Color ButtonNormal = new Color(0.02f, 0.032f, 0.042f, 0.86f);
        private static readonly Color ButtonHover = new Color(0.055f, 0.083f, 0.095f, 0.95f);
        private static readonly Color ButtonPressed = new Color(0.24f, 0.16f, 0.055f, 1f);
        private static readonly Color ButtonSelected = new Color(0.04f, 0.11f, 0.125f, 0.98f);
        private static readonly Color ButtonDisabled = new Color(0.035f, 0.04f, 0.046f, 0.48f);
        private static readonly Color TextPrimary = new Color(0.94f, 0.96f, 0.95f, 1f);
        private static readonly Color TextSecondary = new Color(0.68f, 0.75f, 0.78f, 1f);
        private static readonly Color TextMuted = new Color(0.44f, 0.52f, 0.56f, 1f);
        private static readonly Color Gold = new Color(0.91f, 0.63f, 0.24f, 1f);
        private static readonly Color GoldSoft = new Color(1f, 0.73f, 0.32f, 0.62f);
        private static readonly Color Cyan = new Color(0.18f, 0.74f, 0.86f, 1f);
        private static readonly Color RedGlow = new Color(1f, 0.18f, 0.12f, 1f);
        private static readonly Color GreenReady = new Color(0.56f, 1f, 0.36f, 1f);

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
        private TMP_Text demoToggleValueText;
        private Toggle demoToggle;
        private KaelisMenuButtonTransition demoToggleTransition;
        private KaelisMenuToggleVisual demoToggleVisual;
        private KaelisMenuVisualAssets visualAssets;
        private TMP_FontAsset runtimeFontAsset;
        private bool runtimeFontAssetGenerated;
        private Texture2D solidTexture;
        private Sprite solidSprite;
        private Coroutine visibilityRoutine;
        private bool demoModeEnabled;
        private bool visible;

        public bool DemoModeEnabled
        {
            get { return demoModeEnabled; }
        }

        private void Awake()
        {
            EnsureGeneratedAssets();
            visualAssets = KaelisMenuVisualAssets.Load();
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
            layoutRoot.offsetMin = new Vector2(70f, 64f);
            layoutRoot.offsetMax = new Vector2(-70f, -72f);

            RectTransform leftPanel = CreateGlassPanel("LeftControlPanel", layoutRoot, new Vector2(0f, 0f), new Vector2(0f, 1f));
            leftPanel.pivot = new Vector2(0f, 0.5f);
            leftPanel.sizeDelta = new Vector2(440f, 0f);
            leftPanel.offsetMin = new Vector2(0f, 58f);
            leftPanel.offsetMax = new Vector2(440f, 0f);

            RectTransform previewPanel = CreatePreviewPanel(layoutRoot);
            BuildLeftPanel(leftPanel);
            BuildBottomStatus(layoutRoot);
            BuildPreviewPanel(previewPanel);
        }

        private void CreateBackground(RectTransform parent)
        {
            Texture selectedTexture = backgroundTexture != null ? backgroundTexture : fallbackConceptTexture;

            RawImage background = CreateRawImage("CinematicBackground", parent, selectedTexture);
            Stretch(background.rectTransform);
            background.color = selectedTexture != null ? new Color(0.78f, 0.82f, 0.88f, 1f) : BackgroundTint;
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
            washImage.color = new Color(0.12f, 0.018f, 0.02f, 0.22f);
            washImage.raycastTarget = false;

            RectTransform leftVeil = CreateRect("LeftReadabilityVeil", parent);
            leftVeil.anchorMin = new Vector2(0f, 0f);
            leftVeil.anchorMax = new Vector2(0.46f, 1f);
            leftVeil.offsetMin = Vector2.zero;
            leftVeil.offsetMax = Vector2.zero;

            Image leftVeilImage = leftVeil.gameObject.AddComponent<Image>();
            leftVeilImage.sprite = solidSprite;
            leftVeilImage.color = new Color(0f, 0.01f, 0.015f, 0.32f);
            leftVeilImage.raycastTarget = false;

            AddAmbientLine(parent, "TopAtmosphereLine", 1f, new Color(1f, 0.75f, 0.34f, 0.14f));
            AddAmbientLine(parent, "BottomAtmosphereLine", 0f, new Color(0.2f, 0.86f, 1f, 0.12f));
        }

        private void BuildLeftPanel(RectTransform panel)
        {
            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 30, 26);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TMP_Text eyebrow = CreateText(panel, "Eyebrow", "OPTICAL EXPERIENCE ENGINE", 15f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left, KaelisMenuFontRole.SecondaryLabel);
            eyebrow.characterSpacing = 18f;
            AddLayout(eyebrow.gameObject, -1f, 26f);

            TMP_Text title = CreateText(panel, "Title", "KAELIS", 64f, FontStyles.Normal, TextPrimary, TextAlignmentOptions.Left, KaelisMenuFontRole.Logo);
            title.characterSpacing = 6f;
            AddLayout(title.gameObject, -1f, 76f);
            AddTextGlow(title.gameObject, new Color(0.9f, 0.96f, 1f, 0.18f), new Vector2(0f, -2f));

            TMP_Text subtitle = CreateText(panel, "Subtitle", "BEYOND THE REFLECTION", 16f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left, KaelisMenuFontRole.Subtitle);
            subtitle.characterSpacing = 8f;
            AddLayout(subtitle.gameObject, -1f, 30f);

            AddSpacer(panel, 14f);

            Button enterButton = CreateMenuButton(panel, "EnterExperienceButton", "ENTER EXPERIENCE", Gold, KaelisMenuIconKind.Diamond, true, KaelisMenuButtonVisualKind.Primary);
            BindMenuButton(enterButton, EnterExperience);

            demoToggle = CreateDemoToggle(panel);
            demoToggle.onValueChanged.AddListener(UpdateDemoState);

            Button modesButton = CreateMenuButton(panel, "ModesButton", "MODES", Cyan, KaelisMenuIconKind.Layers, false, KaelisMenuButtonVisualKind.Secondary);
            BindMenuButton(modesButton, () => LogPlaceholder("Modes"));

            Button opticsButton = CreateMenuButton(panel, "OpticsButton", "OPTICS", Cyan, KaelisMenuIconKind.Optics, false, KaelisMenuButtonVisualKind.Secondary);
            BindMenuButton(opticsButton, () => LogPlaceholder("Optics"));

            Button presetsButton = CreateMenuButton(panel, "PresetsButton", "PRESETS", Cyan, KaelisMenuIconKind.Star, false, KaelisMenuButtonVisualKind.Secondary);
            BindMenuButton(presetsButton, () => LogPlaceholder("Presets"));

            Button settingsButton = CreateMenuButton(panel, "SettingsButton", "SETTINGS", Cyan, KaelisMenuIconKind.Settings, false, KaelisMenuButtonVisualKind.Secondary);
            BindMenuButton(settingsButton, () => LogPlaceholder("Settings"));

            Button exitButton = CreateMenuButton(panel, "ExitButton", "EXIT", RedGlow, KaelisMenuIconKind.Exit, false, KaelisMenuButtonVisualKind.Exit);
            BindMenuButton(exitButton, ExitApplication);

            AddSpacer(panel, 8f);
            demoStateText = CreateText(panel, "DemoState", "DEMO MODE  OFF", 13f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left, KaelisMenuFontRole.StatusText);
            demoStateText.characterSpacing = 6f;
            AddLayout(demoStateText.gameObject, -1f, 22f);
        }

        private RectTransform CreatePreviewPanel(RectTransform parent)
        {
            RectTransform panel = CreateGlassPanel("PreviewPanel", parent, new Vector2(0f, 0f), new Vector2(1f, 1f));
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.offsetMin = new Vector2(486f, 58f);
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

            TMP_Text title = CreateText(header, "Label", "LIVE PREVIEW", 18f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left, KaelisMenuFontRole.PreviewLabel);
            title.characterSpacing = 10f;
            Stretch(title.rectTransform);

            CreateModePill(header);

            RectTransform previewFrame = CreateRect("RawImagePreviewFrame", panel);
            previewFrame.anchorMin = new Vector2(0f, 0f);
            previewFrame.anchorMax = new Vector2(1f, 1f);
            previewFrame.offsetMin = new Vector2(32f, 34f);
            previewFrame.offsetMax = new Vector2(-32f, -92f);

            Image frame = previewFrame.gameObject.AddComponent<Image>();
            frame.sprite = solidSprite;
            frame.color = new Color(0.006f, 0.01f, 0.015f, 0.68f);
            frame.raycastTarget = false;
            AddFrame(previewFrame, new Color(Gold.r, Gold.g, Gold.b, 0.7f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.42f), 1.5f);
            AddInsetFrame(previewFrame, new Color(1f, 0.82f, 0.46f, 0.28f), 10f, 1f);
            AddCornerCuts(previewFrame, GoldSoft, 32f, 1.5f);

            RectTransform viewport = CreateRect("PreviewViewport", previewFrame);
            Stretch(viewport);
            viewport.offsetMin = new Vector2(12f, 12f);
            viewport.offsetMax = new Vector2(-12f, -12f);
            viewport.gameObject.AddComponent<RectMask2D>();

            RawImage preview = CreateRawImage("PreviewRawImage", viewport, previewTexture != null ? previewTexture : backgroundTexture);
            Stretch(preview.rectTransform);
            preview.color = new Color(0.98f, 0.98f, 1f, 0.82f);
            preview.raycastTarget = false;

            RectTransform overlay = CreateRect("PreviewGlassOverlay", viewport);
            Stretch(overlay);
            Image overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.sprite = solidSprite;
            overlayImage.color = new Color(0.005f, 0.01f, 0.014f, 0.16f);
            overlayImage.raycastTarget = false;

            CreatePreviewScanlines(viewport);
            CreatePreviewHud(viewport);
        }

        private void BuildBottomStatus(RectTransform parent)
        {
            RectTransform statusBar = CreateRect("BottomStatusBar", parent);
            statusBar.anchorMin = new Vector2(0f, 0f);
            statusBar.anchorMax = new Vector2(1f, 0f);
            statusBar.pivot = new Vector2(0.5f, 0f);
            statusBar.sizeDelta = new Vector2(0f, 48f);
            statusBar.anchoredPosition = Vector2.zero;

            Image background = statusBar.gameObject.AddComponent<Image>();
            background.sprite = solidSprite;
            background.color = new Color(0.01f, 0.018f, 0.026f, 0.9f);
            background.raycastTarget = false;

            AddFrame(statusBar, new Color(Cyan.r, Cyan.g, Cyan.b, 0.32f), new Color(1f, 1f, 1f, 0.12f), 1f);
            AddInsetFrame(statusBar, new Color(1f, 1f, 1f, 0.06f), 4f, 1f);

            RectTransform readyDot = CreateRect("ReadyDot", statusBar);
            readyDot.anchorMin = new Vector2(0f, 0.5f);
            readyDot.anchorMax = new Vector2(0f, 0.5f);
            readyDot.pivot = new Vector2(0.5f, 0.5f);
            readyDot.sizeDelta = new Vector2(8f, 8f);
            readyDot.anchoredPosition = new Vector2(28f, 0f);
            Image readyDotImage = readyDot.gameObject.AddComponent<Image>();
            readyDotImage.sprite = solidSprite;
            readyDotImage.color = GreenReady;
            readyDotImage.raycastTarget = false;

            statusText = CreateText(statusBar, "StatusText", "SYSTEM READY    |    DEMO OFF    |    MODE: STARTUP", 13f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left, KaelisMenuFontRole.StatusText);
            statusText.characterSpacing = 6f;
            statusText.rectTransform.anchorMin = Vector2.zero;
            statusText.rectTransform.anchorMax = Vector2.one;
            statusText.rectTransform.offsetMin = new Vector2(46f, 0f);
            statusText.rectTransform.offsetMax = new Vector2(-260f, 0f);

            TMP_Text version = CreateText(statusBar, "VersionText", "v0.1.0", 12f, FontStyles.Normal, TextMuted, TextAlignmentOptions.Right, KaelisMenuFontRole.StatusText);
            version.characterSpacing = 4f;
            version.rectTransform.anchorMin = new Vector2(1f, 0f);
            version.rectTransform.anchorMax = new Vector2(1f, 1f);
            version.rectTransform.pivot = new Vector2(1f, 0.5f);
            version.rectTransform.sizeDelta = new Vector2(120f, 0f);
            version.rectTransform.anchoredPosition = new Vector2(-132f, 0f);

            CreateStatusIcon(statusBar, "StatusGearIcon", new Vector2(-88f, 0f), KaelisMenuIconKind.Settings, TextMuted);
            CreateStatusIcon(statusBar, "StatusOpticsIcon", new Vector2(-48f, 0f), KaelisMenuIconKind.Optics, TextMuted);
        }

        private void CreateModePill(RectTransform parent)
        {
            RectTransform pill = CreateRect("ModePill", parent);
            pill.anchorMin = new Vector2(1f, 0.5f);
            pill.anchorMax = new Vector2(1f, 0.5f);
            pill.pivot = new Vector2(1f, 0.5f);
            pill.sizeDelta = new Vector2(168f, 30f);
            pill.anchoredPosition = new Vector2(0f, 0f);

            Image image = pill.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = new Color(0.018f, 0.026f, 0.032f, 0.88f);
            image.raycastTarget = false;
            AddFrame(pill, GoldSoft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.18f), 1f);

            CreateStatusIcon(pill, "PillDiamond", new Vector2(-132f, 0f), KaelisMenuIconKind.Diamond, Gold);

            TMP_Text label = CreateText(pill, "Label", "PREMIUM 3D", 13f, FontStyles.Normal, Gold, TextAlignmentOptions.Left, KaelisMenuFontRole.SecondaryLabel);
            label.characterSpacing = 4f;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(42f, 0f);
            label.rectTransform.offsetMax = new Vector2(-28f, 0f);

            RectTransform dot = CreateRect("ReadyDot", pill);
            dot.anchorMin = new Vector2(1f, 0.5f);
            dot.anchorMax = new Vector2(1f, 0.5f);
            dot.pivot = new Vector2(0.5f, 0.5f);
            dot.sizeDelta = new Vector2(9f, 9f);
            dot.anchoredPosition = new Vector2(-16f, 0f);

            Image dotImage = dot.gameObject.AddComponent<Image>();
            dotImage.sprite = solidSprite;
            dotImage.color = GreenReady;
            dotImage.raycastTarget = false;
        }

        private void CreatePreviewScanlines(RectTransform viewport)
        {
            for (int index = 1; index < 8; index++)
            {
                float y = index / 8f;
                AddLine(viewport, "PreviewScanline" + index, new Vector2(0f, y), new Vector2(1f, y), 1f, new Color(1f, 1f, 1f, 0.035f));
            }
        }

        private void CreatePreviewHud(RectTransform viewport)
        {
            RectTransform bottomBand = CreateRect("PreviewBottomBand", viewport);
            bottomBand.anchorMin = new Vector2(0f, 0f);
            bottomBand.anchorMax = new Vector2(1f, 0f);
            bottomBand.pivot = new Vector2(0.5f, 0f);
            bottomBand.sizeDelta = new Vector2(0f, 42f);
            bottomBand.anchoredPosition = Vector2.zero;

            Image bottomBandImage = bottomBand.gameObject.AddComponent<Image>();
            bottomBandImage.sprite = solidSprite;
            bottomBandImage.color = new Color(0f, 0.005f, 0.01f, 0.44f);
            bottomBandImage.raycastTarget = false;

            TMP_Text state = CreateText(bottomBand, "StateLabel", "PREMIUM 3D READY", 12f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Left, KaelisMenuFontRole.StatusText);
            state.characterSpacing = 5f;
            state.rectTransform.anchorMin = Vector2.zero;
            state.rectTransform.anchorMax = Vector2.one;
            state.rectTransform.offsetMin = new Vector2(18f, 0f);
            state.rectTransform.offsetMax = new Vector2(-18f, 0f);

            AddLine(viewport, "PreviewTopAccent", new Vector2(0f, 1f), new Vector2(1f, 1f), 2f, new Color(Gold.r, Gold.g, Gold.b, 0.28f));
            AddLine(viewport, "PreviewBottomAccent", new Vector2(0f, 0f), new Vector2(1f, 0f), 2f, new Color(Cyan.r, Cyan.g, Cyan.b, 0.22f));
        }

        private void CreateStatusIcon(RectTransform parent, string name, Vector2 anchoredPosition, KaelisMenuIconKind kind, Color color)
        {
            RectTransform iconRoot = CreateRect(name, parent);
            iconRoot.anchorMin = new Vector2(1f, 0.5f);
            iconRoot.anchorMax = new Vector2(1f, 0.5f);
            iconRoot.pivot = new Vector2(0.5f, 0.5f);
            iconRoot.sizeDelta = new Vector2(22f, 22f);
            iconRoot.anchoredPosition = anchoredPosition;

            AddIconGeometry(iconRoot, kind, color, 0.72f, 1.5f);
        }

        private void CreateMenuIcon(RectTransform buttonRoot, KaelisMenuIconKind kind, Color accent, bool primary)
        {
            RectTransform iconRoot = CreateRect("Icon", buttonRoot);
            iconRoot.anchorMin = new Vector2(0f, 0.5f);
            iconRoot.anchorMax = new Vector2(0f, 0.5f);
            iconRoot.pivot = new Vector2(0.5f, 0.5f);
            iconRoot.sizeDelta = new Vector2(40f, 40f);
            iconRoot.anchoredPosition = new Vector2(38f, 0f);

            Image badge = iconRoot.gameObject.AddComponent<Image>();
            badge.sprite = solidSprite;
            badge.color = new Color(accent.r, accent.g, accent.b, primary ? 0.09f : 0.035f);
            badge.raycastTarget = false;
            AddFrame(iconRoot, new Color(accent.r, accent.g, accent.b, primary ? 0.32f : 0.18f), new Color(1f, 1f, 1f, 0.08f), 1f);

            AddIconGeometry(iconRoot, kind, accent, primary ? 1f : 0.78f, primary ? 2f : 1.6f);
        }

        private void AddIconGeometry(RectTransform root, KaelisMenuIconKind kind, Color color, float alpha, float thickness)
        {
            Color iconColor = new Color(color.r, color.g, color.b, alpha);

            switch (kind)
            {
                case KaelisMenuIconKind.Diamond:
                    AddIconLineBetween(root, "DiamondA", new Vector2(0f, 14f), new Vector2(15f, 2f), iconColor, thickness);
                    AddIconLineBetween(root, "DiamondB", new Vector2(15f, 2f), new Vector2(0f, -15f), iconColor, thickness);
                    AddIconLineBetween(root, "DiamondC", new Vector2(0f, -15f), new Vector2(-15f, 2f), iconColor, thickness);
                    AddIconLineBetween(root, "DiamondD", new Vector2(-15f, 2f), new Vector2(0f, 14f), iconColor, thickness);
                    AddIconLineBetween(root, "DiamondCore", new Vector2(0f, 14f), new Vector2(0f, -15f), new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * 0.7f), thickness * 0.7f);
                    break;
                case KaelisMenuIconKind.Monitor:
                    AddIconBox(root, "Screen", new Vector2(0f, 4f), new Vector2(24f, 15f), iconColor, thickness);
                    AddIconLineBetween(root, "Stand", new Vector2(0f, -4f), new Vector2(0f, -13f), iconColor, thickness);
                    AddIconLineBetween(root, "Base", new Vector2(-8f, -13f), new Vector2(8f, -13f), iconColor, thickness);
                    break;
                case KaelisMenuIconKind.Layers:
                    AddIconDiamond(root, "LayerA", new Vector2(0f, 8f), new Vector2(24f, 12f), iconColor, thickness);
                    AddIconDiamond(root, "LayerB", new Vector2(0f, 0f), new Vector2(24f, 12f), iconColor, thickness);
                    AddIconDiamond(root, "LayerC", new Vector2(0f, -8f), new Vector2(24f, 12f), iconColor, thickness);
                    break;
                case KaelisMenuIconKind.Optics:
                    AddIconBox(root, "OpticBox", Vector2.zero, new Vector2(23f, 23f), iconColor, thickness);
                    AddIconLineBetween(root, "OpticH", new Vector2(-15f, 0f), new Vector2(15f, 0f), iconColor, thickness);
                    AddIconLineBetween(root, "OpticV", new Vector2(0f, -15f), new Vector2(0f, 15f), iconColor, thickness);
                    AddIconBox(root, "OpticCore", Vector2.zero, new Vector2(9f, 9f), iconColor, thickness);
                    break;
                case KaelisMenuIconKind.Star:
                    AddIconLineBetween(root, "StarA", new Vector2(0f, 15f), new Vector2(0f, -15f), iconColor, thickness);
                    AddIconLineBetween(root, "StarB", new Vector2(-13f, 8f), new Vector2(13f, -8f), iconColor, thickness);
                    AddIconLineBetween(root, "StarC", new Vector2(13f, 8f), new Vector2(-13f, -8f), iconColor, thickness);
                    break;
                case KaelisMenuIconKind.Settings:
                    AddIconBox(root, "SettingsCore", Vector2.zero, new Vector2(13f, 13f), iconColor, thickness);
                    AddIconLineBetween(root, "SettingsH", new Vector2(-15f, 0f), new Vector2(15f, 0f), iconColor, thickness);
                    AddIconLineBetween(root, "SettingsV", new Vector2(0f, -15f), new Vector2(0f, 15f), iconColor, thickness);
                    AddIconLineBetween(root, "SettingsD1", new Vector2(-10f, -10f), new Vector2(10f, 10f), new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * 0.55f), thickness);
                    AddIconLineBetween(root, "SettingsD2", new Vector2(-10f, 10f), new Vector2(10f, -10f), new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * 0.55f), thickness);
                    break;
                case KaelisMenuIconKind.Exit:
                    AddIconLineBetween(root, "ExitStem", new Vector2(0f, 13f), new Vector2(0f, -2f), iconColor, thickness * 1.2f);
                    AddIconLineBetween(root, "ExitLeft", new Vector2(-11f, 5f), new Vector2(-11f, -12f), iconColor, thickness);
                    AddIconLineBetween(root, "ExitRight", new Vector2(11f, 5f), new Vector2(11f, -12f), iconColor, thickness);
                    AddIconLineBetween(root, "ExitBase", new Vector2(-11f, -12f), new Vector2(11f, -12f), iconColor, thickness);
                    break;
            }
        }

        private void AddIconBox(RectTransform root, string name, Vector2 center, Vector2 size, Color color, float thickness)
        {
            float halfWidth = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;
            AddIconLineBetween(root, name + "Top", center + new Vector2(-halfWidth, halfHeight), center + new Vector2(halfWidth, halfHeight), color, thickness);
            AddIconLineBetween(root, name + "Bottom", center + new Vector2(-halfWidth, -halfHeight), center + new Vector2(halfWidth, -halfHeight), color, thickness);
            AddIconLineBetween(root, name + "Left", center + new Vector2(-halfWidth, -halfHeight), center + new Vector2(-halfWidth, halfHeight), color, thickness);
            AddIconLineBetween(root, name + "Right", center + new Vector2(halfWidth, -halfHeight), center + new Vector2(halfWidth, halfHeight), color, thickness);
        }

        private void AddIconDiamond(RectTransform root, string name, Vector2 center, Vector2 size, Color color, float thickness)
        {
            Vector2 top = center + new Vector2(0f, size.y * 0.5f);
            Vector2 right = center + new Vector2(size.x * 0.5f, 0f);
            Vector2 bottom = center + new Vector2(0f, -size.y * 0.5f);
            Vector2 left = center + new Vector2(-size.x * 0.5f, 0f);
            AddIconLineBetween(root, name + "A", top, right, color, thickness);
            AddIconLineBetween(root, name + "B", right, bottom, color, thickness);
            AddIconLineBetween(root, name + "C", bottom, left, color, thickness);
            AddIconLineBetween(root, name + "D", left, top, color, thickness);
        }

        private void AddIconLineBetween(RectTransform root, string name, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 delta = end - start;
            RectTransform line = CreateRect(name, root);
            line.anchorMin = new Vector2(0.5f, 0.5f);
            line.anchorMax = new Vector2(0.5f, 0.5f);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.sizeDelta = new Vector2(delta.magnitude, thickness);
            line.anchoredPosition = (start + end) * 0.5f;
            line.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            Image image = line.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = color;
            image.raycastTarget = false;

            LayoutElement layoutElement = line.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }

        private Toggle CreateDemoToggle(RectTransform parent)
        {
            RectTransform row = CreateButtonBase(
                parent,
                "DemoModeToggle",
                "DEMO MODE",
                Cyan,
                KaelisMenuIconKind.Monitor,
                false,
                KaelisMenuButtonVisualKind.Demo,
                56f,
                out TMP_Text label,
                out Image background,
                out Image glow,
                out Image accentLine);
            label.text = "DEMO MODE";

            Toggle toggle = row.gameObject.AddComponent<Toggle>();
            toggle.transition = Selectable.Transition.None;
            toggle.targetGraphic = background;
            toggle.graphic = null;

            demoToggleValueText = CreateText(row, "ToggleValue", "OFF", 14f, FontStyles.Normal, TextSecondary, TextAlignmentOptions.Right, KaelisMenuFontRole.StatusText);
            demoToggleValueText.characterSpacing = 4f;
            demoToggleValueText.rectTransform.anchorMin = new Vector2(1f, 0f);
            demoToggleValueText.rectTransform.anchorMax = new Vector2(1f, 1f);
            demoToggleValueText.rectTransform.pivot = new Vector2(1f, 0.5f);
            demoToggleValueText.rectTransform.sizeDelta = new Vector2(52f, 0f);
            demoToggleValueText.rectTransform.anchoredPosition = new Vector2(-78f, 0f);

            RectTransform track = CreateRect("ToggleTrack", row);
            track.anchorMin = new Vector2(1f, 0.5f);
            track.anchorMax = new Vector2(1f, 0.5f);
            track.pivot = new Vector2(1f, 0.5f);
            track.sizeDelta = new Vector2(52f, 26f);
            track.anchoredPosition = new Vector2(-18f, 0f);

            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.sprite = solidSprite;
            trackImage.color = new Color(0f, 0f, 0f, 0.34f);
            trackImage.raycastTarget = false;
            AddFrame(track, new Color(Cyan.r, Cyan.g, Cyan.b, 0.34f), new Color(1f, 1f, 1f, 0.18f), 1f);

            RectTransform knob = CreateRect("ToggleKnob", track);
            knob.anchorMin = new Vector2(0f, 0.5f);
            knob.anchorMax = new Vector2(0f, 0.5f);
            knob.pivot = new Vector2(0.5f, 0.5f);
            knob.sizeDelta = new Vector2(18f, 18f);
            knob.anchoredPosition = new Vector2(13f, 0f);

            Image knobImage = knob.gameObject.AddComponent<Image>();
            knobImage.sprite = solidSprite;
            knobImage.color = new Color(0.76f, 0.82f, 0.84f, 1f);
            knobImage.raycastTarget = false;

            demoToggleTransition = row.gameObject.AddComponent<KaelisMenuButtonTransition>();
            demoToggleTransition.Configure(toggle, background, glow, accentLine, label, row.GetComponent<KaelisMenuGemButtonVisual>(), ButtonNormal, ButtonHover, ButtonPressed, ButtonSelected, ButtonDisabled, Cyan, false);

            demoToggleVisual = row.gameObject.AddComponent<KaelisMenuToggleVisual>();
            demoToggleVisual.Configure(trackImage, knob, knobImage, demoToggleValueText, Cyan, Gold);
            toggle.SetIsOnWithoutNotify(false);

            return toggle;
        }

        private Button CreateMenuButton(RectTransform parent, string name, string label, Color accent, KaelisMenuIconKind iconKind, bool primary, KaelisMenuButtonVisualKind visualKind)
        {
            RectTransform buttonRoot = CreateButtonBase(
                parent,
                name,
                label,
                accent,
                iconKind,
                primary,
                visualKind,
                primary ? 64f : 56f,
                out TMP_Text labelText,
                out Image background,
                out Image glow,
                out Image accentLine);
            Button button = buttonRoot.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = background;

            KaelisMenuButtonTransition transition = buttonRoot.gameObject.AddComponent<KaelisMenuButtonTransition>();
            Color normal = primary ? new Color(0.075f, 0.055f, 0.025f, 0.92f) : ButtonNormal;
            Color hover = primary ? new Color(0.18f, 0.12f, 0.035f, 0.98f) : ButtonHover;
            Color pressed = primary ? new Color(0.36f, 0.22f, 0.07f, 1f) : ButtonPressed;
            Color selected = primary ? new Color(0.14f, 0.095f, 0.034f, 0.98f) : ButtonSelected;
            transition.Configure(button, background, glow, accentLine, labelText, buttonRoot.GetComponent<KaelisMenuGemButtonVisual>(), normal, hover, pressed, selected, ButtonDisabled, accent, primary);
            return button;
        }

        private RectTransform CreateButtonBase(
            RectTransform parent,
            string name,
            string label,
            Color accent,
            KaelisMenuIconKind iconKind,
            bool primary,
            KaelisMenuButtonVisualKind visualKind,
            float height,
            out TMP_Text labelText,
            out Image backgroundImage,
            out Image glowImage,
            out Image accentLineImage)
        {
            RectTransform buttonRoot = CreateRect(name, parent);
            AddLayout(buttonRoot.gameObject, -1f, height);

            backgroundImage = buttonRoot.gameObject.AddComponent<Image>();
            backgroundImage.sprite = solidSprite;
            backgroundImage.color = primary ? new Color(0.075f, 0.055f, 0.025f, 0.92f) : ButtonNormal;
            backgroundImage.raycastTarget = true;

            KaelisMenuGemButtonSprites gemSprites = visualAssets != null ? visualAssets.GetButtonSprites(visualKind) : KaelisMenuGemButtonSprites.Empty;
            if (gemSprites.HasNormal)
            {
                backgroundImage.sprite = gemSprites.normal;
                backgroundImage.type = Image.Type.Sliced;
                backgroundImage.color = Color.white;
            }

            if (gemSprites.HasAny)
            {
                RectTransform hoverClip = CreateRect("GemHoverClip", buttonRoot);
                hoverClip.anchorMin = new Vector2(0f, 0f);
                hoverClip.anchorMax = new Vector2(0f, 1f);
                hoverClip.pivot = new Vector2(0f, 0.5f);
                hoverClip.sizeDelta = new Vector2(0f, 0f);
                hoverClip.anchoredPosition = Vector2.zero;
                hoverClip.gameObject.AddComponent<RectMask2D>();

                RectTransform hoverImageRect = CreateRect("GemHoverImage", hoverClip);
                hoverImageRect.anchorMin = new Vector2(0f, 0f);
                hoverImageRect.anchorMax = new Vector2(0f, 1f);
                hoverImageRect.pivot = new Vector2(0f, 0.5f);
                hoverImageRect.sizeDelta = new Vector2(0f, 0f);
                hoverImageRect.anchoredPosition = Vector2.zero;
                Image hoverImage = hoverImageRect.gameObject.AddComponent<Image>();
                hoverImage.sprite = gemSprites.hover;
                hoverImage.type = Image.Type.Sliced;
                hoverImage.color = Color.white;
                hoverImage.raycastTarget = false;

                RectTransform stateOverlay = CreateRect("GemStateOverlay", buttonRoot);
                Stretch(stateOverlay);
                Image stateImage = stateOverlay.gameObject.AddComponent<Image>();
                stateImage.sprite = gemSprites.selected != null ? gemSprites.selected : gemSprites.pressed;
                stateImage.type = Image.Type.Sliced;
                stateImage.color = new Color(1f, 1f, 1f, 0f);
                stateImage.raycastTarget = false;

                RectTransform flashOverlay = CreateRect("GemActivationFlash", buttonRoot);
                Stretch(flashOverlay);
                Image flashImage = flashOverlay.gameObject.AddComponent<Image>();
                flashImage.sprite = gemSprites.pressed != null ? gemSprites.pressed : gemSprites.hover;
                flashImage.type = Image.Type.Sliced;
                flashImage.color = new Color(1f, 0.86f, 0.08f, 0f);
                flashImage.raycastTarget = false;

                KaelisMenuGemButtonVisual gemVisual = buttonRoot.gameObject.AddComponent<KaelisMenuGemButtonVisual>();
                gemVisual.Configure(buttonRoot, hoverClip, hoverImageRect, hoverImage, stateImage, flashImage, gemSprites, visualKind);
            }

            RectTransform glow = CreateRect("SoftAccentGlow", buttonRoot);
            Stretch(glow);
            Image glowImageComponent = glow.gameObject.AddComponent<Image>();
            glowImageComponent.sprite = solidSprite;
            glowImageComponent.color = new Color(accent.r, accent.g, accent.b, primary ? 0.16f : 0.035f);
            glowImageComponent.raycastTarget = false;
            glowImage = glowImageComponent;

            AddFrame(buttonRoot, new Color(accent.r, accent.g, accent.b, primary ? 0.76f : 0.34f), new Color(1f, 1f, 1f, primary ? 0.22f : 0.12f), primary ? 1.5f : 1f);
            AddInsetFrame(buttonRoot, new Color(accent.r, accent.g, accent.b, primary ? 0.3f : 0.16f), 5f, 1f);
            AddCornerCuts(buttonRoot, new Color(accent.r, accent.g, accent.b, primary ? 0.9f : 0.42f), 18f, 1.5f);

            RectTransform accentLine = CreateRect("AccentLine", buttonRoot);
            accentLine.anchorMin = new Vector2(0f, 0f);
            accentLine.anchorMax = new Vector2(0f, 1f);
            accentLine.pivot = new Vector2(0f, 0.5f);
            accentLine.sizeDelta = new Vector2(primary ? 3f : 2f, 0f);
            accentLine.anchoredPosition = Vector2.zero;

            Image accentImage = accentLine.gameObject.AddComponent<Image>();
            accentImage.sprite = solidSprite;
            accentImage.color = new Color(accent.r, accent.g, accent.b, primary ? 0.7f : 0.28f);
            accentImage.raycastTarget = false;
            accentLineImage = accentImage;

            CreateMenuIcon(buttonRoot, iconKind, accent, primary);

            labelText = CreateText(buttonRoot, "Label", label, primary ? 16f : 15f, FontStyles.Normal, TextPrimary, TextAlignmentOptions.MidlineLeft, primary ? KaelisMenuFontRole.PrimaryButton : KaelisMenuFontRole.SecondaryLabel);
            labelText.characterSpacing = primary ? 5f : 4f;
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = new Vector2(76f, 0f);
            labelText.rectTransform.offsetMax = new Vector2(-88f, 0f);

            TMP_Text chevron = CreateText(buttonRoot, "Chevron", ">", primary ? 28f : 25f, FontStyles.Normal, primary ? Gold : TextSecondary, TextAlignmentOptions.Center, KaelisMenuFontRole.SecondaryLabel);
            chevron.rectTransform.anchorMin = new Vector2(1f, 0f);
            chevron.rectTransform.anchorMax = new Vector2(1f, 1f);
            chevron.rectTransform.pivot = new Vector2(1f, 0.5f);
            chevron.rectTransform.sizeDelta = new Vector2(34f, 0f);
            chevron.rectTransform.anchoredPosition = new Vector2(-14f, 0f);

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

            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
            outline.effectDistance = new Vector2(2f, -2f);

            AddFrame(panel, new Color(Cyan.r, Cyan.g, Cyan.b, 0.32f), new Color(1f, 1f, 1f, 0.12f), 1f);
            AddInsetFrame(panel, new Color(1f, 1f, 1f, 0.08f), 8f, 1f);
            AddPanelSheen(panel);
            AddCornerCuts(panel, new Color(Cyan.r, Cyan.g, Cyan.b, 0.28f), 28f, 1.5f);

            Shadow shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.52f);
            shadow.effectDistance = new Vector2(12f, -12f);

            return panel;
        }

        private TMP_Text CreateText(RectTransform parent, string name, string value, float size, FontStyles style, Color color, TextAlignmentOptions alignment, KaelisMenuFontRole fontRole = KaelisMenuFontRole.SecondaryLabel)
        {
            RectTransform rect = CreateRect(name, parent);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset fontAsset = GetFontAsset(fontRole);
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

        private void BindMenuButton(Button button, UnityAction action)
        {
            KaelisMenuButtonTransition transition = button.GetComponent<KaelisMenuButtonTransition>();
            button.onClick.AddListener(() =>
            {
                if (transition != null)
                {
                    transition.InvokeWithFlash(action);
                    return;
                }

                action.Invoke();
            });
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

            if (demoToggleValueText != null)
            {
                demoToggleValueText.text = demoModeEnabled ? "ON" : "OFF";
                demoToggleValueText.color = demoModeEnabled ? Gold : TextSecondary;
            }

            if (demoToggleTransition != null)
            {
                demoToggleTransition.SetSelectedVisual(demoModeEnabled);
            }

            if (demoToggleVisual != null)
            {
                demoToggleVisual.SetState(demoModeEnabled);
            }

            SetStatus(demoModeEnabled ? "SYSTEM READY    |    DEMO ON    |    MODE: STARTUP" : "SYSTEM READY    |    DEMO OFF    |    MODE: STARTUP");
            Debug.Log("[KAELIS Menu] Demo Mode stored: " + (demoModeEnabled ? "ON" : "OFF") + ".");
        }

        private void SetVisible(bool shouldShow)
        {
            visible = shouldShow;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = visible;
                canvasGroup.interactable = visible;
            }

            if (visibilityRoutine != null)
            {
                StopCoroutine(visibilityRoutine);
                visibilityRoutine = null;
            }

            if (Application.isPlaying && canvasGroup != null && menuRoot != null)
            {
                visibilityRoutine = StartCoroutine(FadeVisible(visible));
            }
            else
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = visible ? 1f : 0f;
                }

                if (menuRoot != null && menuRoot.activeSelf != visible)
                {
                    menuRoot.SetActive(visible);
                }
            }

            if (visible)
            {
                SetStatus(demoModeEnabled ? "SYSTEM READY    |    DEMO ON    |    MODE: STARTUP" : "SYSTEM READY    |    DEMO OFF    |    MODE: STARTUP");
            }
        }

        private IEnumerator FadeVisible(bool shouldShow)
        {
            if (menuRoot == null || canvasGroup == null)
            {
                yield break;
            }

            if (shouldShow && !menuRoot.activeSelf)
            {
                menuRoot.SetActive(true);
            }

            float startAlpha = canvasGroup.alpha;
            float targetAlpha = shouldShow ? 1f : 0f;
            float elapsed = 0f;

            while (elapsed < VisibilityFadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / VisibilityFadeSeconds);
                t = 1f - ((1f - t) * (1f - t));
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;

            if (!shouldShow)
            {
                menuRoot.SetActive(false);
            }

            visibilityRoutine = null;
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

        private TMP_FontAsset GetFontAsset(KaelisMenuFontRole role)
        {
            if (visualAssets != null)
            {
                TMP_FontAsset roleFont = visualAssets.GetFont(role);
                if (roleFont != null)
                {
                    return roleFont;
                }
            }

            return GetRuntimeFontAsset();
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

        private void AddPanelSheen(RectTransform target)
        {
            RectTransform sheen = CreateRect("GlassSheen", target);
            sheen.anchorMin = new Vector2(0f, 1f);
            sheen.anchorMax = new Vector2(1f, 1f);
            sheen.pivot = new Vector2(0.5f, 1f);
            sheen.sizeDelta = new Vector2(0f, 76f);
            sheen.anchoredPosition = Vector2.zero;

            Image image = sheen.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = PanelSoft;
            image.raycastTarget = false;

            LayoutElement layoutElement = sheen.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }

        private void AddTextGlow(GameObject target, Color color, Vector2 distance)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = false;
        }

        private void AddAmbientLine(RectTransform parent, string name, float yAnchor, Color color)
        {
            RectTransform line = CreateRect(name, parent);
            line.anchorMin = new Vector2(0f, yAnchor);
            line.anchorMax = new Vector2(1f, yAnchor);
            line.pivot = new Vector2(0.5f, yAnchor);
            line.sizeDelta = new Vector2(0f, 2f);
            line.anchoredPosition = Vector2.zero;

            Image image = line.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = color;
            image.raycastTarget = false;

            LayoutElement layoutElement = line.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }

        private void AddInsetFrame(RectTransform target, Color color, float inset, float thickness)
        {
            RectTransform insetFrame = CreateRect("InsetFrame", target);
            Stretch(insetFrame);
            insetFrame.offsetMin = new Vector2(inset, inset);
            insetFrame.offsetMax = new Vector2(-inset, -inset);

            AddFrame(insetFrame, color, new Color(color.r, color.g, color.b, color.a * 0.65f), thickness);

            LayoutElement layoutElement = insetFrame.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }

        private void AddFrame(RectTransform target, Color primary, Color secondary, float thickness)
        {
            AddLine(target, "FrameTop", new Vector2(0f, 1f), new Vector2(1f, 1f), thickness, primary);
            AddLine(target, "FrameBottom", new Vector2(0f, 0f), new Vector2(1f, 0f), thickness, secondary);
            AddLine(target, "FrameLeft", new Vector2(0f, 0f), new Vector2(0f, 1f), thickness, secondary);
            AddLine(target, "FrameRight", new Vector2(1f, 0f), new Vector2(1f, 1f), thickness, primary);
        }

        private void AddCornerCuts(RectTransform target, Color color, float length, float thickness)
        {
            AddCornerCut(target, "CornerTopLeft", new Vector2(0f, 1f), new Vector2(length * 0.35f, -length * 0.35f), -45f, color, length, thickness);
            AddCornerCut(target, "CornerTopRight", new Vector2(1f, 1f), new Vector2(-length * 0.35f, -length * 0.35f), 45f, color, length, thickness);
            AddCornerCut(target, "CornerBottomLeft", new Vector2(0f, 0f), new Vector2(length * 0.35f, length * 0.35f), 45f, color, length, thickness);
            AddCornerCut(target, "CornerBottomRight", new Vector2(1f, 0f), new Vector2(-length * 0.35f, length * 0.35f), -45f, color, length, thickness);
        }

        private void AddCornerCut(RectTransform target, string name, Vector2 anchor, Vector2 anchoredPosition, float rotation, Color color, float length, float thickness)
        {
            RectTransform line = CreateRect(name, target);
            line.anchorMin = anchor;
            line.anchorMax = anchor;
            line.pivot = new Vector2(0.5f, 0.5f);
            line.sizeDelta = new Vector2(length, thickness);
            line.anchoredPosition = anchoredPosition;
            line.localEulerAngles = new Vector3(0f, 0f, rotation);

            Image image = line.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = color;
            image.raycastTarget = false;

            LayoutElement layoutElement = line.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
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

    internal sealed class KaelisMenuVisualAssets
    {
        private const string GemButtonPath = "GemButtons/";
        private const string MenuFontPath = "MenuFonts/";

        private Sprite primaryNormal;
        private Sprite primaryHover;
        private Sprite primaryPressed;
        private Sprite secondaryNormal;
        private Sprite secondaryHover;
        private Sprite secondaryPressed;
        private Sprite secondarySelected;
        private Sprite demoNormal;
        private Sprite demoActive;
        private Sprite exitNormal;
        private Sprite exitHover;
        private Sprite exitPressed;
        private TMP_FontAsset cinzelRegular;
        private TMP_FontAsset cinzelMedium;
        private TMP_FontAsset cinzelSemiBold;
        private TMP_FontAsset interRegular;
        private TMP_FontAsset interMedium;
        private TMP_FontAsset interSemiBold;

        public static KaelisMenuVisualAssets Load()
        {
            return new KaelisMenuVisualAssets
            {
                primaryNormal = LoadSprite("button_primary_normal"),
                primaryHover = LoadSprite("button_primary_hover"),
                primaryPressed = LoadSprite("button_primary_pressed"),
                secondaryNormal = LoadSprite("button_secondary_normal"),
                secondaryHover = LoadSprite("button_secondary_hover"),
                secondaryPressed = LoadSprite("button_secondary_pressed"),
                secondarySelected = LoadSprite("button_secondary_selected"),
                demoNormal = LoadSprite("button_demo_normal"),
                demoActive = LoadSprite("button_demo_active"),
                exitNormal = LoadSprite("button_exit_normal"),
                exitHover = LoadSprite("button_exit_hover"),
                exitPressed = LoadSprite("button_exit_pressed"),
                cinzelRegular = LoadFont("Kaelis_Cinzel_Regular"),
                cinzelMedium = LoadFont("Kaelis_Cinzel_Medium"),
                cinzelSemiBold = LoadFont("Kaelis_Cinzel_SemiBold"),
                interRegular = LoadFont("Kaelis_Inter_18pt_Regular"),
                interMedium = LoadFont("Kaelis_Inter_18pt_Medium"),
                interSemiBold = LoadFont("Kaelis_Inter_18pt_SemiBold")
            };
        }

        public KaelisMenuGemButtonSprites GetButtonSprites(KaelisMenuButtonVisualKind kind)
        {
            switch (kind)
            {
                case KaelisMenuButtonVisualKind.Primary:
                    return new KaelisMenuGemButtonSprites(primaryNormal, primaryHover, primaryPressed, primaryPressed);
                case KaelisMenuButtonVisualKind.Demo:
                    return new KaelisMenuGemButtonSprites(demoNormal, demoActive, demoActive, demoActive);
                case KaelisMenuButtonVisualKind.Exit:
                    return new KaelisMenuGemButtonSprites(exitNormal, exitHover, exitPressed, exitHover);
                default:
                    return new KaelisMenuGemButtonSprites(secondaryNormal, secondaryHover, secondaryPressed, secondarySelected);
            }
        }

        public TMP_FontAsset GetFont(KaelisMenuFontRole role)
        {
            switch (role)
            {
                case KaelisMenuFontRole.Logo:
                    return cinzelRegular != null ? cinzelRegular : cinzelMedium;
                case KaelisMenuFontRole.Subtitle:
                    return cinzelRegular;
                case KaelisMenuFontRole.PrimaryButton:
                    return cinzelSemiBold;
                case KaelisMenuFontRole.PreviewLabel:
                    return interMedium;
                case KaelisMenuFontRole.StatusText:
                    return interRegular != null ? interRegular : interMedium;
                default:
                    return interSemiBold;
            }
        }

        private static Sprite LoadSprite(string assetName)
        {
            return Resources.Load<Sprite>(GemButtonPath + assetName);
        }

        private static TMP_FontAsset LoadFont(string assetName)
        {
            return Resources.Load<TMP_FontAsset>(MenuFontPath + assetName);
        }
    }

    internal readonly struct KaelisMenuGemButtonSprites
    {
        public static readonly KaelisMenuGemButtonSprites Empty = new KaelisMenuGemButtonSprites(null, null, null, null);

        public readonly Sprite normal;
        public readonly Sprite hover;
        public readonly Sprite pressed;
        public readonly Sprite selected;

        public KaelisMenuGemButtonSprites(Sprite normal, Sprite hover, Sprite pressed, Sprite selected)
        {
            this.normal = normal;
            this.hover = hover;
            this.pressed = pressed;
            this.selected = selected;
        }

        public bool HasNormal
        {
            get { return normal != null; }
        }

        public bool HasAny
        {
            get { return normal != null || hover != null || pressed != null || selected != null; }
        }
    }

    internal sealed class KaelisMenuGemButtonVisual : MonoBehaviour
    {
        private RectTransform root;
        private RectTransform hoverClip;
        private RectTransform hoverImageRect;
        private Image hoverImage;
        private Image stateImage;
        private Image flashImage;
        private KaelisMenuGemButtonSprites sprites;
        private KaelisMenuButtonVisualKind kind;
        private float hoverFill;
        private float stateAlpha;
        private float flashAlpha;
        private bool configured;

        public void Configure(
            RectTransform root,
            RectTransform hoverClip,
            RectTransform hoverImageRect,
            Image hoverImage,
            Image stateImage,
            Image flashImage,
            KaelisMenuGemButtonSprites sprites,
            KaelisMenuButtonVisualKind kind)
        {
            this.root = root;
            this.hoverClip = hoverClip;
            this.hoverImageRect = hoverImageRect;
            this.hoverImage = hoverImage;
            this.stateImage = stateImage;
            this.flashImage = flashImage;
            this.sprites = sprites;
            this.kind = kind;
            configured = true;
            ApplyVisual(false, false, false, true, 1f);
        }

        public void TriggerFlash()
        {
            flashAlpha = 1f;
        }

        public void ApplyVisual(bool hovered, bool pressed, bool selected, bool interactable, float t)
        {
            if (!configured || root == null)
            {
                return;
            }

            float targetFill = 0f;
            if (interactable)
            {
                if (pressed)
                {
                    targetFill = 1f;
                }
                else if (hovered)
                {
                    targetFill = GetHoverFill();
                }
                else if (selected)
                {
                    targetFill = 0.28f;
                }
            }

            hoverFill = Mathf.Lerp(hoverFill, targetFill, t);
            float rootWidth = Mathf.Max(0f, root.rect.width);
            if (hoverClip != null)
            {
                hoverClip.sizeDelta = new Vector2(rootWidth * hoverFill, 0f);
            }

            if (hoverImageRect != null)
            {
                hoverImageRect.sizeDelta = new Vector2(rootWidth, 0f);
            }

            if (hoverImage != null)
            {
                hoverImage.enabled = hoverFill > 0.01f;
                hoverImage.color = Color.Lerp(hoverImage.color, interactable ? Color.white : new Color(1f, 1f, 1f, 0.35f), t);
            }

            Sprite stateSprite = pressed ? sprites.pressed : sprites.selected;
            if (stateSprite == null)
            {
                stateSprite = sprites.pressed != null ? sprites.pressed : sprites.hover;
            }

            if (stateImage != null)
            {
                stateImage.sprite = stateSprite;
                float targetStateAlpha = 0f;
                if (interactable)
                {
                    if (pressed)
                    {
                        targetStateAlpha = 0.92f;
                    }
                    else if (selected)
                    {
                        targetStateAlpha = kind == KaelisMenuButtonVisualKind.Demo ? 0.7f : 0.52f;
                    }
                }

                stateAlpha = Mathf.Lerp(stateAlpha, targetStateAlpha, t);
                stateImage.color = new Color(1f, 1f, 1f, stateAlpha);
            }

            flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, Time.unscaledDeltaTime * 9f);
            if (flashImage != null)
            {
                Color flashColor = kind == KaelisMenuButtonVisualKind.Exit
                    ? new Color(1f, 0.28f, 0.1f, flashAlpha * 0.78f)
                    : new Color(1f, 0.88f, 0.12f, flashAlpha * 0.82f);
                flashImage.color = flashColor;
            }
        }

        private float GetHoverFill()
        {
            switch (kind)
            {
                case KaelisMenuButtonVisualKind.Primary:
                    return 0.52f;
                case KaelisMenuButtonVisualKind.Exit:
                    return 0.62f;
                case KaelisMenuButtonVisualKind.Demo:
                    return 0.45f;
                default:
                    return 0.5f;
            }
        }
    }

    internal sealed class KaelisMenuButtonTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private const float ActivationFlashDelay = 0.08f;

        private Selectable selectable;
        private Image background;
        private Image glow;
        private Image accentLine;
        private TMP_Text label;
        private KaelisMenuGemButtonVisual gemVisual;
        private RectTransform rectTransform;
        private Color normalColor;
        private Color hoverColor;
        private Color pressedColor;
        private Color selectedColor;
        private Color disabledColor;
        private Color accentColor;
        private Color labelBaseColor;
        private bool primary;
        private bool hovered;
        private bool pressed;
        private bool keyboardSelected;
        private bool forcedSelected;
        private bool configured;

        public void Configure(
            Selectable selectable,
            Image background,
            Image glow,
            Image accentLine,
            TMP_Text label,
            KaelisMenuGemButtonVisual gemVisual,
            Color normalColor,
            Color hoverColor,
            Color pressedColor,
            Color selectedColor,
            Color disabledColor,
            Color accentColor,
            bool primary)
        {
            this.selectable = selectable;
            this.background = background;
            this.glow = glow;
            this.accentLine = accentLine;
            this.label = label;
            this.gemVisual = gemVisual;
            this.normalColor = normalColor;
            this.hoverColor = hoverColor;
            this.pressedColor = pressedColor;
            this.selectedColor = selectedColor;
            this.disabledColor = disabledColor;
            this.accentColor = accentColor;
            this.primary = primary;
            labelBaseColor = label != null ? label.color : Color.white;
            rectTransform = (RectTransform)transform;
            configured = true;

            ApplyInstant();
        }

        public void InvokeWithFlash(UnityAction action)
        {
            if (gemVisual != null)
            {
                gemVisual.TriggerFlash();
            }

            if (action == null)
            {
                return;
            }

            if (Application.isPlaying && gameObject.activeInHierarchy)
            {
                StartCoroutine(InvokeAfterFlash(action));
                return;
            }

            action.Invoke();
        }

        private IEnumerator InvokeAfterFlash(UnityAction action)
        {
            yield return new WaitForSecondsRealtime(ActivationFlashDelay);
            action.Invoke();
        }

        public void SetSelectedVisual(bool selected)
        {
            forcedSelected = selected;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsInteractable())
            {
                hovered = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsInteractable())
            {
                pressed = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            keyboardSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            keyboardSelected = false;
            pressed = false;
        }

        private void OnDisable()
        {
            hovered = false;
            pressed = false;
            keyboardSelected = false;
        }

        private void Update()
        {
            if (!configured)
            {
                return;
            }

            float t = 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime);
            Apply(t);
        }

        private void ApplyInstant()
        {
            Apply(1f);
        }

        private void Apply(float t)
        {
            bool interactable = IsInteractable();
            bool selected = forcedSelected || keyboardSelected;
            Color surface = disabledColor;
            if (interactable)
            {
                if (pressed)
                {
                    surface = pressedColor;
                }
                else if (selected)
                {
                    surface = selectedColor;
                }
                else if (hovered)
                {
                    surface = hoverColor;
                }
                else
                {
                    surface = normalColor;
                }
            }

            if (background != null)
            {
                Color target = gemVisual != null
                    ? (interactable ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.62f))
                    : surface;
                background.color = Color.Lerp(background.color, target, t);
            }

            if (gemVisual != null)
            {
                gemVisual.ApplyVisual(hovered, pressed, selected, interactable, t);
            }

            if (glow != null)
            {
                float alpha = 0.035f;
                if (primary)
                {
                    alpha = 0.14f;
                }

                if (hovered || selected)
                {
                    alpha += primary ? 0.16f : 0.11f;
                }

                if (pressed)
                {
                    alpha += 0.08f;
                }

                if (!interactable)
                {
                    alpha = 0.015f;
                }

                Color glowTarget = new Color(accentColor.r, accentColor.g, accentColor.b, alpha);
                glow.color = Color.Lerp(glow.color, glowTarget, t);
            }

            if (accentLine != null)
            {
                float alpha = primary ? 0.68f : 0.22f;
                if (hovered || selected)
                {
                    alpha = primary ? 0.95f : 0.58f;
                }

                if (!interactable)
                {
                    alpha = 0.1f;
                }

                Color accentTarget = new Color(accentColor.r, accentColor.g, accentColor.b, alpha);
                accentLine.color = Color.Lerp(accentLine.color, accentTarget, t);
            }

            if (label != null)
            {
                Color labelTarget = labelBaseColor;
                if (primary || hovered || selected)
                {
                    labelTarget = Color.Lerp(labelBaseColor, Color.white, primary ? 0.3f : 0.18f);
                }

                if (!interactable)
                {
                    labelTarget = new Color(labelBaseColor.r, labelBaseColor.g, labelBaseColor.b, 0.42f);
                }

                label.color = Color.Lerp(label.color, labelTarget, t);
            }

            if (rectTransform != null)
            {
                float targetScale = pressed ? 0.992f : ((hovered || selected) ? 1.006f : 1f);
                Vector3 target = new Vector3(targetScale, targetScale, 1f);
                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, target, t);
            }
        }

        private bool IsInteractable()
        {
            return selectable == null || selectable.IsInteractable();
        }
    }

    internal sealed class KaelisMenuToggleVisual : MonoBehaviour
    {
        private Image track;
        private RectTransform knob;
        private Image knobImage;
        private TMP_Text valueLabel;
        private Color offColor;
        private Color cyan;
        private Color gold;
        private bool enabledState;
        private bool configured;

        public void Configure(Image track, RectTransform knob, Image knobImage, TMP_Text valueLabel, Color cyan, Color gold)
        {
            this.track = track;
            this.knob = knob;
            this.knobImage = knobImage;
            this.valueLabel = valueLabel;
            this.cyan = cyan;
            this.gold = gold;
            offColor = track != null ? track.color : new Color(0f, 0f, 0f, 0.34f);
            configured = true;
            Apply(1f);
        }

        public void SetState(bool enabled)
        {
            enabledState = enabled;
        }

        private void Update()
        {
            if (!configured)
            {
                return;
            }

            float t = 1f - Mathf.Exp(-15f * Time.unscaledDeltaTime);
            Apply(t);
        }

        private void Apply(float t)
        {
            if (track != null)
            {
                Color onColor = new Color(cyan.r, cyan.g, cyan.b, 0.34f);
                track.color = Color.Lerp(track.color, enabledState ? onColor : offColor, t);
            }

            if (knob != null)
            {
                Vector2 target = new Vector2(enabledState ? 39f : 13f, 0f);
                knob.anchoredPosition = Vector2.Lerp(knob.anchoredPosition, target, t);
            }

            if (knobImage != null)
            {
                Color offKnob = new Color(0.76f, 0.82f, 0.84f, 1f);
                knobImage.color = Color.Lerp(knobImage.color, enabledState ? gold : offKnob, t);
            }

            if (valueLabel != null)
            {
                Color offText = new Color(0.68f, 0.75f, 0.78f, 1f);
                valueLabel.color = Color.Lerp(valueLabel.color, enabledState ? gold : offText, t);
            }
        }
    }
}
