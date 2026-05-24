using Kaleidoscope2.Menu.FX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    internal sealed class KaelisStartupMenuView
    {
        private readonly KaelisMenuAssets assets;
        private readonly MenuAtmosphereFXController atmosphereFX;
        private KaelisMenuAnimator animator;
        private CanvasGroup mainMenuCanvasGroup;

        public KaelisStartupMenuView(KaelisMenuAssets assets, MenuAtmosphereFXController atmosphereFX)
        {
            this.assets = assets;
            this.atmosphereFX = atmosphereFX;
        }

        public GameObject Root { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }
        public KaelisMenuButton EnterButton { get; private set; }
        public KaelisMenuButton DemoButton { get; private set; }
        public KaelisMenuButton ModesButton { get; private set; }
        public KaelisMenuButton OpticsButton { get; private set; }
        public KaelisMenuButton PresetsButton { get; private set; }
        public KaelisMenuButton SettingsButton { get; private set; }
        public KaelisMenuButton ExitButton { get; private set; }
        public Toggle DemoToggle { get { return DemoButton != null ? DemoButton.Toggle : null; } }
        public RawImage PreviewRawImage { get; private set; }
        public TMP_Text TitleText { get; private set; }
        public TMP_Text StatusText { get; private set; }
        public KaelisMenuSectionController SectionController { get; private set; }
        public KaelisContentSelectionPanel ContentSelectionPanel { get; private set; }
        public KaelisMenuTooltip Tooltip { get; private set; }

        public void Build(Transform parent)
        {
            Root = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            Root.transform.SetParent(parent, false);

            RectTransform canvasRect = (RectTransform)Root.transform;
            KaelisMenuUiPrimitives.Stretch(canvasRect);

            Canvas canvas = Root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue - 64;
            canvas.pixelPerfect = false;

            CanvasScaler scaler = Root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(KaelisMenuStyle.ReferenceWidth, KaelisMenuStyle.ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            CanvasGroup = Root.GetComponent<CanvasGroup>();
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;

            animator = Root.AddComponent<KaelisMenuAnimator>();

            BuildBackground(canvasRect);
            if (atmosphereFX != null)
            {
                atmosphereFX.Build(canvasRect, assets.SolidSprite);
            }

            Tooltip = KaelisMenuTooltip.Create(canvasRect, assets);

            RectTransform safeFrame = KaelisMenuUiPrimitives.CreateRect("SafeFrame", canvasRect);
            KaelisMenuUiPrimitives.Stretch(safeFrame);
            safeFrame.offsetMin = KaelisMenuStyle.SafeFrameMin;
            safeFrame.offsetMax = KaelisMenuStyle.SafeFrameMax;
            mainMenuCanvasGroup = safeFrame.gameObject.AddComponent<CanvasGroup>();

            BuildLeftPanel(safeFrame);
            BuildPreviewPanel(safeFrame);
            BuildStatusBar(safeFrame);
            ContentSelectionPanel = new KaelisContentSelectionPanel(canvasRect, assets, Tooltip);
        }

        public void Dispose()
        {
            animator = null;
            SectionController = null;
            ContentSelectionPanel = null;
            Tooltip = null;
            mainMenuCanvasGroup = null;
            Root = null;
            CanvasGroup = null;
        }

        public void SetMainMenuVisible(bool visible)
        {
            if (mainMenuCanvasGroup == null)
            {
                return;
            }

            mainMenuCanvasGroup.gameObject.SetActive(visible);
            mainMenuCanvasGroup.alpha = visible ? 1f : 0f;
            mainMenuCanvasGroup.interactable = visible;
            mainMenuCanvasGroup.blocksRaycasts = visible;
        }

        public void SetDemoState(bool enabled)
        {
            if (DemoToggle != null)
            {
                DemoToggle.SetIsOnWithoutNotify(enabled);
            }

            if (DemoButton != null)
            {
                DemoButton.SetSelectedVisual(enabled);
            }
        }

        public void SetStatus(string value)
        {
            if (StatusText != null)
            {
                KaelisMenuLocalizationService.SetText(StatusText, value);
            }
        }

        private void BuildBackground(RectTransform canvasRect)
        {
            RectTransform background = KaelisMenuUiPrimitives.CreateRect("ReferenceAtmosphere", canvasRect);
            KaelisMenuUiPrimitives.Stretch(background);
            if (assets.BackgroundTexture != null)
            {
                RawImage image = KaelisMenuUiPrimitives.AddRawImage(background, assets.BackgroundTexture, KaelisMenuStyle.BackgroundTint, false);
                image.uvRect = new Rect(0.02f, 0.02f, 0.96f, 0.96f);
            }
            else
            {
                KaelisMenuUiPrimitives.AddImage(background, assets.SolidSprite, KaelisMenuStyle.BackgroundFallback, false);
            }

            RectTransform scrim = KaelisMenuUiPrimitives.CreateRect("SoftDepthScrim", canvasRect);
            KaelisMenuUiPrimitives.Stretch(scrim);
            KaelisMenuUiPrimitives.AddImage(scrim, assets.SolidSprite, KaelisMenuStyle.BackgroundScrim, false);

            KaelisMenuUiPrimitives.AddAmbientBand(canvasRect, "CyanTopHaze", new Vector2(0f, 0.70f), new Vector2(1f, 1f), new Color(0.15f, 0.62f, 0.76f, 0.12f), assets.SolidSprite);
            KaelisMenuUiPrimitives.AddAmbientBand(canvasRect, "DeepBottomHaze", new Vector2(0f, 0f), new Vector2(1f, 0.34f), new Color(0f, 0.028f, 0.045f, 0.22f), assets.SolidSprite);

            PremiumMenuMotionController premiumMotion = PremiumMenuMotionController.Ensure(Root);
            if (premiumMotion != null)
            {
                premiumMotion.Build(canvasRect, assets.SolidSprite);
            }

            RectTransform shimmer = KaelisMenuUiPrimitives.CreateRect("PrismaticSheen", canvasRect);
            shimmer.anchorMin = new Vector2(-0.06f, 0.82f);
            shimmer.anchorMax = new Vector2(1.08f, 0.90f);
            shimmer.offsetMin = Vector2.zero;
            shimmer.offsetMax = Vector2.zero;
            shimmer.localEulerAngles = new Vector3(0f, 0f, -9f);
            KaelisMenuUiPrimitives.AddImage(shimmer, assets.SolidSprite, new Color(0.55f, 0.93f, 1f, 0.10f), false);
            if (animator != null)
            {
                animator.BindShimmerLayer(shimmer);
            }
        }

        private void BuildLeftPanel(RectTransform safeFrame)
        {
            RectTransform panel = KaelisMenuUiPrimitives.CreateRect("LeftControlPanel", safeFrame);
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.offsetMin = KaelisMenuStyle.LeftPanelOffsetMin;
            panel.offsetMax = KaelisMenuStyle.LeftPanelOffsetMax;
            BuildGlassPanel(panel, true);

            RectTransform logo = KaelisMenuUiPrimitives.CreateRect("LogoBlock", panel);
            logo.anchorMin = new Vector2(0f, 1f);
            logo.anchorMax = new Vector2(1f, 1f);
            logo.pivot = new Vector2(0.5f, 1f);
            logo.sizeDelta = new Vector2(0f, KaelisMenuStyle.LogoBlockHeight);
            logo.anchoredPosition = new Vector2(0f, -KaelisMenuStyle.LogoBlockTopInset);
            BuildBrandLogo(logo);

            RectTransform buttonStack = KaelisMenuUiPrimitives.CreateRect("ButtonStack", panel);
            KaelisMenuUiPrimitives.Stretch(buttonStack);
            buttonStack.offsetMin = new Vector2(55f, 56f);
            buttonStack.offsetMax = new Vector2(-55f, -KaelisMenuStyle.LogoButtonStackTopInset);

            VerticalLayoutGroup layout = buttonStack.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            EnterButton = KaelisMenuButton.CreateButton(buttonStack, "EnterExperienceButton", "ENTER EXPERIENCE", KaelisMenuButtonTone.Primary, KaelisMenuIconKind.Diamond, assets, KaelisMenuStyle.PrimaryButtonHeight);
            DemoButton = KaelisMenuButton.CreateToggle(buttonStack, "DemoModeToggle", "DEMO MODE", KaelisMenuButtonTone.Demo, KaelisMenuIconKind.Monitor, assets, KaelisMenuStyle.StandardButtonHeight);
            ModesButton = KaelisMenuButton.CreateButton(buttonStack, "ModesButton", "MODES", KaelisMenuButtonTone.Secondary, KaelisMenuIconKind.Layers, assets, KaelisMenuStyle.StandardButtonHeight);
            OpticsButton = KaelisMenuButton.CreateButton(buttonStack, "OpticsButton", "OPTICS", KaelisMenuButtonTone.Secondary, KaelisMenuIconKind.Optics, assets, KaelisMenuStyle.StandardButtonHeight);
            PresetsButton = KaelisMenuButton.CreateButton(buttonStack, "PresetsButton", "PRESETS", KaelisMenuButtonTone.Secondary, KaelisMenuIconKind.Star, assets, KaelisMenuStyle.StandardButtonHeight);
            SettingsButton = KaelisMenuButton.CreateButton(buttonStack, "SettingsButton", "SETTINGS", KaelisMenuButtonTone.Secondary, KaelisMenuIconKind.Settings, assets, KaelisMenuStyle.StandardButtonHeight);
            ExitButton = KaelisMenuButton.CreateButton(buttonStack, "ExitButton", "EXIT", KaelisMenuButtonTone.Exit, KaelisMenuIconKind.Exit, assets, KaelisMenuStyle.StandardButtonHeight);
            ConfigureMainButtonTooltips();
        }

        private void ConfigureMainButtonTooltips()
        {
            if (Tooltip == null)
            {
                return;
            }

            if (EnterButton != null)
            {
                EnterButton.ConfigureTooltip(Tooltip, "ENTER EXPERIENCE", "Open the content selection flow for image and audio sources.", "Images required; music optional", "Ready", KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Action));
            }

            if (DemoButton != null)
            {
                DemoButton.ConfigureTooltip(Tooltip, "DEMO MODE", "Reserved for a dedicated demo playback task. Current click stores only the UI state.", "OFF / ON", "Reserved", KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Toggle));
            }

            if (ModesButton != null)
            {
                ModesButton.ConfigureTooltip(Tooltip, "MODES", "Open visual route selection cards.", "Classic / Tunnel / Flight / Reserved", "Section", KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Action));
            }

            if (OpticsButton != null)
            {
                OpticsButton.ConfigureTooltip(Tooltip, "OPTICS", "Open expressive crystal optics controls.", "Extended creative ranges", "Section", KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Action));
            }

            if (PresetsButton != null)
            {
                PresetsButton.ConfigureTooltip(Tooltip, "PRESETS", "Open factory look cards and reserved user preset actions.", "Factory profiles", "Section", KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Action));
            }

            if (SettingsButton != null)
            {
                SettingsButton.ConfigureTooltip(Tooltip, "SETTINGS", "Open application, audio, controls, system, and diagnostics settings.", "System controls", "Section", KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Action));
            }

            if (ExitButton != null)
            {
                ExitButton.ConfigureTooltip(Tooltip, "EXIT", "Open the exit confirmation panel. First click never quits immediately.", "Confirm required", "Safe", KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Action));
            }
        }

        private void BuildPreviewPanel(RectTransform safeFrame)
        {
            RectTransform panel = KaelisMenuUiPrimitives.CreateRect("PreviewPanel", safeFrame);
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.offsetMin = KaelisMenuStyle.PreviewPanelOffsetMin;
            panel.offsetMax = KaelisMenuStyle.PreviewPanelOffsetMax;
            BuildGlassPanel(panel, false);

            RectTransform header = KaelisMenuUiPrimitives.CreateRect("PreviewHeader", panel);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, 72f);
            header.anchoredPosition = Vector2.zero;

            TMP_Text headerText = KaelisMenuUiPrimitives.CreateText(header, "LivePreviewLabel", "LIVE PREVIEW", 17f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Button));
            headerText.characterSpacing = 8f;
            RectTransform headerTextRect = (RectTransform)headerText.transform;
            headerTextRect.offsetMin = new Vector2(70f, 0f);
            headerTextRect.offsetMax = new Vector2(-70f, 0f);
            AddAccentDiamond(header, "PreviewHeaderAccent", new Vector2(0f, 1f), new Vector2(45f, -33f), 11f, new Color(1f, 0.74f, 0.32f, 0.20f), new Color(1f, 0.76f, 0.36f, 0.72f));

            RectTransform display = KaelisMenuUiPrimitives.CreateRect("PreviewDisplayFrame", panel);
            KaelisMenuUiPrimitives.Stretch(display);
            display.offsetMin = new Vector2(38f, 38f);
            display.offsetMax = new Vector2(-38f, -80f);

            if (assets.PreviewTexture != null)
            {
                PreviewRawImage = KaelisMenuUiPrimitives.AddRawImage(display, assets.PreviewTexture, Color.white, false);
                PreviewRawImage.name = "PreviewRawImage";
                PreviewRawImage.uvRect = KaelisMenuStyle.PreviewUv;
            }
            else
            {
                KaelisMenuUiPrimitives.AddImage(display, assets.SolidSprite, new Color(0.02f, 0.08f, 0.11f, 0.86f), false);
                PreviewRawImage = display.gameObject.AddComponent<RawImage>();
                PreviewRawImage.name = "PreviewRawImage";
                PreviewRawImage.color = new Color(0f, 0f, 0f, 0f);
                PreviewRawImage.raycastTarget = false;
            }

            RectTransform previewDepth = KaelisMenuUiPrimitives.CreateRect("PreviewDepthOverlay", display);
            KaelisMenuUiPrimitives.Stretch(previewDepth);
            KaelisMenuUiPrimitives.AddImage(previewDepth, assets.SolidSprite, new Color(0f, 0.018f, 0.030f, 0.10f), false);
            KaelisMenuUiPrimitives.AddAmbientBand(display, "PreviewTopCyanBloom", new Vector2(0f, 0.70f), new Vector2(1f, 1f), new Color(0.15f, 0.80f, 1f, 0.08f), assets.SolidSprite);
            if (atmosphereFX != null)
            {
                atmosphereFX.BindCrystalShimmerTarget(display);
            }

            KaelisMenuUiPrimitives.AddInsetFrame(display, new Color(0.22f, 0.92f, 1f, 0.34f), 0f, 1.15f, assets.SolidSprite);
            KaelisMenuUiPrimitives.AddCornerCuts(display, new Color(1f, 0.78f, 0.42f, 0.34f), 38f, 1.35f, assets.SolidSprite);

            RectTransform caption = KaelisMenuUiPrimitives.CreateRect("PreviewCaption", display);
            caption.anchorMin = new Vector2(0f, 0f);
            caption.anchorMax = new Vector2(1f, 0f);
            caption.pivot = new Vector2(0.5f, 0f);
            caption.sizeDelta = new Vector2(0f, 118f);
            caption.anchoredPosition = new Vector2(0f, 34f);

            TMP_Text captionTitle = KaelisMenuUiPrimitives.CreateText(caption, "PreviewPanelTitle", "PREVIEW PANEL", 27f, new Color(1f, 0.77f, 0.40f, 0.96f), TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.PreviewLabel));
            captionTitle.characterSpacing = 14f;
            RectTransform captionTitleRect = (RectTransform)captionTitle.transform;
            captionTitleRect.offsetMin = new Vector2(0f, 56f);
            captionTitleRect.offsetMax = new Vector2(0f, 112f);

            TMP_Text captionSub = KaelisMenuUiPrimitives.CreateText(caption, "PreviewPanelSubtitle", "RAW IMAGE SURFACE", 13f, new Color(0.78f, 0.84f, 0.86f, 0.78f), TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Status));
            captionSub.characterSpacing = 9f;
            RectTransform captionSubRect = (RectTransform)captionSub.transform;
            captionSubRect.offsetMin = new Vector2(0f, 20f);
            captionSubRect.offsetMax = new Vector2(0f, 60f);

            SectionController = new KaelisMenuSectionController(display, assets, Tooltip);
        }

        private void BuildStatusBar(RectTransform safeFrame)
        {
            RectTransform status = KaelisMenuUiPrimitives.CreateRect("BottomStatusBar", safeFrame);
            status.anchorMin = new Vector2(0f, 0f);
            status.anchorMax = new Vector2(1f, 0f);
            status.pivot = new Vector2(0.5f, 0f);
            status.sizeDelta = new Vector2(0f, 60f);
            status.anchoredPosition = Vector2.zero;
            BuildGlassPanel(status, false);

            AddAccentDiamond(status, "StatusDiamondLeft", new Vector2(0f, 0.5f), new Vector2(64f, 0f), 14f, new Color(1f, 0.70f, 0.28f, 0.18f), new Color(1f, 0.72f, 0.30f, 0.80f));

            StatusText = KaelisMenuUiPrimitives.CreateText(status, "StatusText", "SYSTEM READY     DEMO OFF", 15f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            StatusText.characterSpacing = 7f;
            RectTransform statusTextRect = (RectTransform)StatusText.transform;
            statusTextRect.offsetMin = new Vector2(116f, 0f);
            statusTextRect.offsetMax = new Vector2(-96f, 0f);

            AddAccentDiamond(status, "StatusDiamondRight", new Vector2(1f, 0.5f), new Vector2(-64f, 0f), 14f, new Color(0.20f, 0.84f, 1f, 0.14f), new Color(1f, 0.72f, 0.30f, 0.70f));
        }

        private void BuildBrandLogo(RectTransform logo)
        {
            if (assets.LogoTexture != null)
            {
                RectTransform frame = KaelisMenuUiPrimitives.CreateRect("BrandLogoFrame", logo);
                KaelisMenuUiPrimitives.Stretch(frame);
                frame.offsetMin = KaelisMenuStyle.LogoFrameOffsetMin;
                frame.offsetMax = KaelisMenuStyle.LogoFrameOffsetMax;
                KaelisMenuUiPrimitives.AddImage(frame, assets.SolidSprite, new Color(0.006f, 0.026f, 0.038f, 0.34f), false);
                KaelisMenuUiPrimitives.AddFrame(frame, new Color(0.72f, 0.96f, 1f, 0.18f), new Color(0.20f, 0.84f, 0.92f, 0.14f), 0.85f, assets.SolidSprite);
                KaelisMenuUiPrimitives.AddCornerCuts(frame, new Color(1f, 0.76f, 0.38f, 0.18f), 20f, 0.95f, assets.SolidSprite);

                RectTransform inner = KaelisMenuUiPrimitives.CreateRect("BrandLogoInnerFit", frame);
                KaelisMenuUiPrimitives.Stretch(inner);
                inner.offsetMin = KaelisMenuStyle.LogoInnerOffsetMin;
                inner.offsetMax = KaelisMenuStyle.LogoInnerOffsetMax;

                RectTransform cyanHalo = KaelisMenuUiPrimitives.CreateRect("BrandLogoCyanHalo", inner);
                cyanHalo.anchorMin = new Vector2(0.04f, 0.12f);
                cyanHalo.anchorMax = new Vector2(0.96f, 0.92f);
                cyanHalo.offsetMin = Vector2.zero;
                cyanHalo.offsetMax = Vector2.zero;
                KaelisMenuUiPrimitives.AddImage(cyanHalo, assets.SolidSprite, new Color(0.10f, 0.54f, 0.70f, 0.052f), false);

                RectTransform goldHalo = KaelisMenuUiPrimitives.CreateRect("BrandLogoGoldHalo", inner);
                goldHalo.anchorMin = new Vector2(0.16f, 0.08f);
                goldHalo.anchorMax = new Vector2(0.84f, 0.36f);
                goldHalo.offsetMin = Vector2.zero;
                goldHalo.offsetMax = Vector2.zero;
                KaelisMenuUiPrimitives.AddImage(goldHalo, assets.SolidSprite, new Color(1f, 0.66f, 0.22f, 0.040f), false);

                RectTransform imageRect = KaelisMenuUiPrimitives.CreateRect("BrandLogoImage", inner);
                KaelisMenuUiPrimitives.Stretch(imageRect);
                RawImage logoImage = KaelisMenuUiPrimitives.AddRawImage(imageRect, assets.LogoTexture, new Color(1f, 1f, 1f, 0.96f), false);
                logoImage.uvRect = KaelisMenuStyle.LogoUv;
                AspectRatioFitter aspect = imageRect.gameObject.AddComponent<AspectRatioFitter>();
                aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                aspect.aspectRatio = assets.LogoAspect;
                return;
            }

            BuildTextLogoFallback(logo);
        }

        private void BuildTextLogoFallback(RectTransform logo)
        {
            AddAccentDiamond(logo, "CrystalMark", new Vector2(0.5f, 1f), new Vector2(0f, -45f), 22f, new Color(0.18f, 0.88f, 1f, 0.16f), new Color(0.82f, 0.98f, 1f, 0.78f));

            TitleText = KaelisMenuUiPrimitives.CreateText(logo, "Title", "K A E L I S", 58f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Logo));
            TitleText.characterSpacing = 9f;
            RectTransform titleRect = (RectTransform)TitleText.transform;
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 0f);
            titleRect.offsetMin = new Vector2(0f, 78f);
            titleRect.offsetMax = new Vector2(0f, 152f);
            KaelisMenuUiPrimitives.AddTextGlow(TitleText.gameObject, new Color(1f, 0.76f, 0.42f, 0.30f), new Vector2(0f, 0f));

            TMP_Text subtitle = KaelisMenuUiPrimitives.CreateText(logo, "Subtitle", "- BEYOND THE REFLECTION -", 16f, new Color(0.91f, 0.82f, 0.66f, 0.92f), TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Subtitle));
            subtitle.characterSpacing = 12f;
            RectTransform subtitleRect = (RectTransform)subtitle.transform;
            subtitleRect.anchorMin = new Vector2(0f, 0f);
            subtitleRect.anchorMax = new Vector2(1f, 0f);
            subtitleRect.offsetMin = new Vector2(0f, 48f);
            subtitleRect.offsetMax = new Vector2(0f, 90f);
        }

        private void BuildGlassPanel(RectTransform panel, bool strong)
        {
            Color baseColor = strong ? KaelisMenuStyle.PanelBase : new Color(0.013f, 0.050f, 0.066f, 0.58f);
            KaelisMenuUiPrimitives.AddImage(panel, assets.SolidSprite, baseColor, false);

            RectTransform glow = KaelisMenuUiPrimitives.CreateRect("InnerCyanGlow", panel);
            KaelisMenuUiPrimitives.Stretch(glow);
            glow.offsetMin = new Vector2(8f, 8f);
            glow.offsetMax = new Vector2(-8f, -8f);
            KaelisMenuUiPrimitives.AddImage(glow, assets.SolidSprite, KaelisMenuStyle.PanelInnerGlow, false);

            RectTransform sheen = KaelisMenuUiPrimitives.CreateRect("TopGlassSheen", panel);
            sheen.anchorMin = new Vector2(0f, 0.68f);
            sheen.anchorMax = new Vector2(1f, 1f);
            sheen.offsetMin = Vector2.zero;
            sheen.offsetMax = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(sheen, assets.SolidSprite, KaelisMenuStyle.PanelSheen, false);

            RectTransform shade = KaelisMenuUiPrimitives.CreateRect("BottomDepthShade", panel);
            shade.anchorMin = new Vector2(0f, 0f);
            shade.anchorMax = new Vector2(1f, 0.35f);
            shade.offsetMin = Vector2.zero;
            shade.offsetMax = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(shade, assets.SolidSprite, new Color(0f, 0.010f, 0.018f, 0.22f), false);

            KaelisMenuUiPrimitives.AddFrame(panel, KaelisMenuStyle.IcyLine, KaelisMenuStyle.CyanLine, 1.15f, assets.SolidSprite);
            KaelisMenuUiPrimitives.AddCornerCuts(panel, new Color(1f, 0.76f, 0.42f, 0.36f), 38f, 1.35f, assets.SolidSprite);
        }

        private void AddAccentDiamond(RectTransform parent, string name, Vector2 anchor, Vector2 anchoredPosition, float size, Color glowColor, Color coreColor)
        {
            AddRotatedSquare(parent, name + "Glow", anchor, anchoredPosition, size * 1.9f, glowColor);
            AddRotatedSquare(parent, name + "Core", anchor, anchoredPosition, size, coreColor);
        }

        private void AddRotatedSquare(RectTransform parent, string name, Vector2 anchor, Vector2 anchoredPosition, float size, Color color)
        {
            RectTransform square = KaelisMenuUiPrimitives.CreateRect(name, parent);
            square.anchorMin = anchor;
            square.anchorMax = anchor;
            square.pivot = new Vector2(0.5f, 0.5f);
            square.sizeDelta = new Vector2(size, size);
            square.anchoredPosition = anchoredPosition;
            square.localEulerAngles = new Vector3(0f, 0f, 45f);
            KaelisMenuUiPrimitives.AddImage(square, assets.SolidSprite, color, false);
        }
    }
}
