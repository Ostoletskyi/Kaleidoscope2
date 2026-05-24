using System;
using System.Collections.Generic;
using Kaleidoscope2.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    internal sealed class KaelisMenuSectionController
    {
        private readonly KaelisMenuAssets assets;
        private readonly KaelisMenuTooltip tooltip;
        private readonly Dictionary<KaelisMenuSection, KaelisMenuSectionPanel> panels = new Dictionary<KaelisMenuSection, KaelisMenuSectionPanel>();
        private Action<KaelisMenuPanelCommand> commandHandler;
        private KaelisMenuInteractiveRow selectedModeRow;
        private KaelisMenuInteractiveRow selectedPresetRow;
        private Button presetApplyButton;
        private KaelisMenuInteractiveRow presetApplyRow;
        private KaelisMenuToggleControl secondDisplayToggle;
        private KaelisMenuToggleControl recordingToggle;
        private Button testSecondDisplayButton;
        private KaelisMenuInteractiveRow testSecondDisplayRow;
        private TMP_Text secondDisplayStatusText;
        private TMP_Text secondDisplayInfoText;
        private TMP_Text recordingOutputFolderText;
        private TMP_Text recordingStatusText;
        private TMP_Text recordingHotkeyText;
        private PremiumCrystalFactoryPreset selectedPremiumPreset = PremiumCrystalFactoryPreset.DiamondPalace;
        private bool hasSelectedPremiumPreset;
        private Action<PremiumCrystalOpticsParameter, float> premiumOpticsHandler;
        private Action<PremiumCrystalEffectToggle, bool> premiumEffectHandler;
        private Action<bool> premiumWheelScaleEnabledHandler;
        private Action<float> premiumWheelScaleStepHandler;

        public KaelisMenuSectionController(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip)
        {
            this.assets = assets;
            this.tooltip = tooltip;

            Root = KaelisMenuUiPrimitives.CreateRect("SectionPanelsRoot", parent);
            KaelisMenuUiPrimitives.Stretch(Root);
            Root.offsetMin = new Vector2(54f, 62f);
            Root.offsetMax = new Vector2(-54f, -58f);

            ProductionOptions = new KaelisProductionOptions();
            RefreshSecondDisplayAvailability();

            BuildModesPanel();
            BuildOpticsPanel();
            BuildPresetsPanel();
            BuildSettingsPanel();
            BuildExitPanel();
            Close();
        }

        public RectTransform Root { get; private set; }
        public KaelisMenuSection ActiveSection { get; private set; }
        public KaelisProductionOptions ProductionOptions { get; private set; }

        public void SetCommandHandler(Action<KaelisMenuPanelCommand> handler)
        {
            commandHandler = handler;
        }

        public void SetPremiumCrystalHandlers(
            Action<PremiumCrystalOpticsParameter, float> opticsHandler,
            Action<PremiumCrystalEffectToggle, bool> effectHandler,
            Action<bool> wheelScaleEnabledHandler,
            Action<float> wheelScaleStepHandler)
        {
            premiumOpticsHandler = opticsHandler;
            premiumEffectHandler = effectHandler;
            premiumWheelScaleEnabledHandler = wheelScaleEnabledHandler;
            premiumWheelScaleStepHandler = wheelScaleStepHandler;
        }

        public void SetActiveSection(KaelisMenuSection section)
        {
            ActiveSection = section;

            foreach (KeyValuePair<KaelisMenuSection, KaelisMenuSectionPanel> pair in panels)
            {
                pair.Value.SetVisible(pair.Key == section);
            }

            if (section == KaelisMenuSection.Modes)
            {
                RefreshSecondDisplayAvailability();
                RefreshProductionUi();
            }
        }

        public void Close()
        {
            SetActiveSection(KaelisMenuSection.None);
        }

        private void BuildModesPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Modes, "ModesSectionPanel", "MODES", "Runtime visual routes");
            AddModeRow(panel.Content, "Classic 2D", "Original kaleidoscope surface renderer.", "SAFE COMMAND", "SetVisualMode(Classic)", KaelisMenuPanelCommand.ApplyClassicMode, KaelisMenuStyle.Cyan, true);
            AddModeRow(panel.Content, "Premium 3D Crystal", "Crystal-based premium optical scene with real volumetric mesh and optics controls.", "SAFE COMMAND", "DiamondFocus + RealMesh3D", KaelisMenuPanelCommand.ApplyPremium3DMode, KaelisMenuStyle.GoldSoft, true);
            AddModeRow(panel.Content, "4D Tunnel / Funnel", "Depth/funnel mode with curved visual space.", "SAFE COMMAND", "SetVisualMode(Tunnel)", KaelisMenuPanelCommand.ApplyTunnelMode, KaelisMenuStyle.Cyan, true);
            AddModeRow(panel.Content, "5D Endless Flight", "Continuous movement toward the kaleidoscope center.", "SAFE COMMAND", "SetVisualMode(FiveD)", KaelisMenuPanelCommand.ApplyFiveDMode, KaelisMenuStyle.Cyan, true);
            BuildShowcaseRecordingBlock(panel.Content);
        }

        private void BuildShowcaseRecordingBlock(RectTransform parent)
        {
            RectTransform block = KaelisMenuUiPrimitives.CreateRect("ShowcaseRecordingBlock", parent);
            KaelisMenuUiPrimitives.AddLayout(block.gameObject, -1f, 462f);
            KaelisMenuUiPrimitives.AddImage(block, assets.SolidSprite, new Color(0.006f, 0.045f, 0.064f, 0.72f), true);
            KaelisMenuUiPrimitives.AddFrame(block, new Color(1f, 0.72f, 0.28f, 0.36f), new Color(0.14f, 0.78f, 0.92f, 0.24f), 0.95f, assets.SolidSprite);
            KaelisMenuUiPrimitives.AddInsetFrame(block, new Color(0.54f, 0.95f, 1f, 0.14f), 8f, 0.7f, assets.SolidSprite);
            KaelisMenuUiPrimitives.AddCornerCuts(block, new Color(1f, 0.78f, 0.32f, 0.34f), 26f, 1f, assets.SolidSprite);

            RectTransform content = KaelisMenuUiPrimitives.CreateRect("ShowcaseRecordingContent", block);
            KaelisMenuUiPrimitives.Stretch(content);
            content.offsetMin = new Vector2(18f, 14f);
            content.offsetMax = new Vector2(-18f, -14f);
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            RectTransform header = KaelisMenuUiPrimitives.CreateRect("ShowcaseRecordingHeader", content);
            KaelisMenuUiPrimitives.AddLayout(header.gameObject, -1f, 54f);
            TMP_Text title = KaelisMenuUiPrimitives.CreateText(header, "ShowcaseRecordingTitle", "SHOWCASE / RECORDING", 16f, KaelisMenuStyle.GoldSoft, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Button));
            title.characterSpacing = 4f;
            RectTransform titleRect = (RectTransform)title.transform;
            titleRect.offsetMin = new Vector2(2f, 24f);
            titleRect.offsetMax = new Vector2(-2f, -2f);

            TMP_Text subtitle = KaelisMenuUiPrimitives.CreateText(header, "ShowcaseRecordingDescription", "Production tools for external display output and video clip capture.", 12.5f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            subtitle.enableWordWrapping = true;
            RectTransform subtitleRect = (RectTransform)subtitle.transform;
            subtitleRect.offsetMin = new Vector2(2f, 0f);
            subtitleRect.offsetMax = new Vector2(-2f, -29f);

            secondDisplayToggle = KaelisMenuToggleControl.Create(content, assets, tooltip, "Output To Second Display", "First monitor remains the control panel. Kaleidoscope output is sent to the second monitor.", false, KaelisMenuBindingStatus.PartialBinding);
            secondDisplayToggle.gameObject.name = "OutputToSecondDisplayToggle";
            secondDisplayToggle.SetChangedHandler(OnSecondDisplayToggleChanged);
            secondDisplayStatusText = AddStatusLine(content, "SecondDisplayStatus", "Second display: Not detected", KaelisMenuStyle.TextMuted, 26f);
            secondDisplayInfoText = AddStatusLine(content, "SecondDisplayInfo", "First monitor remains the control panel. Kaleidoscope output is sent to the second monitor.", KaelisMenuStyle.TextSecondary, 34f);

            RectTransform displayActions = CreateActionRow(content, "SecondDisplayActions");
            testSecondDisplayButton = AddActionChip(displayActions, "TestSecondDisplayButton", "TEST DISPLAY", KaelisMenuPanelCommand.TestSecondDisplayOutput, KaelisMenuStyle.Cyan);
            testSecondDisplayRow = testSecondDisplayButton.GetComponent<KaelisMenuInteractiveRow>();

            recordingToggle = KaelisMenuToggleControl.Create(content, assets, tooltip, "Create Video Clip", "Records kaleidoscope output to a video file. Auto-starts when experience starts if enabled.", false, KaelisMenuBindingStatus.Reserved);
            recordingToggle.gameObject.name = "CreateVideoClipToggle";
            recordingToggle.SetChangedHandler(OnRecordingToggleChanged);
            recordingOutputFolderText = AddStatusLine(content, "RecordingOutputFolder", "No output folder selected", KaelisMenuStyle.TextMuted, 30f);

            RectTransform recordingActions = CreateActionRow(content, "RecordingOutputActions");
            AddActionChip(recordingActions, "SelectRecordingOutputFolderButton", "SELECT OUTPUT FOLDER", KaelisMenuPanelCommand.SelectRecordingOutputFolder, KaelisMenuStyle.GoldSoft);
            AddActionChip(recordingActions, "ClearRecordingOutputFolderButton", "CLEAR", KaelisMenuPanelCommand.ClearRecordingOutputFolder, KaelisMenuStyle.Cyan);

            recordingStatusText = AddStatusLine(content, "RecordingStatus", "Recording: Reserved", KaelisMenuStyle.GoldSoft, 26f);
            recordingHotkeyText = AddStatusLine(content, "RecordingHotkeyHint", "Ctrl + Shift + R = Start / Stop recording", KaelisMenuStyle.TextSecondary, 26f);
            RefreshProductionUi();
        }

        private void BuildOpticsPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Optics, "OpticsSectionPanel", "OPTICS", "Expressive crystal optics controls");
            AddOpticsSlider(panel.Content, "Brightness", "From dark jewel mood to bright luminous crystal.", 0.10f, 3.00f, 1.00f, PremiumCrystalOpticsParameter.Brightness);
            AddOpticsSlider(panel.Content, "Contrast", "From soft dreamy blending to hard dramatic separation.", 0.20f, 3.00f, 1.10f, PremiumCrystalOpticsParameter.Contrast);
            AddOpticsSlider(panel.Content, "Bloom / Glow", "Controls radiant gem glow and bloom intensity.", 0.00f, 5.00f, 0.80f, PremiumCrystalOpticsParameter.BloomGlow);
            AddOpticsSlider(panel.Content, "Facet Highlights", "Strengthens sparkle flashes and facet edge highlights.", 0.00f, 6.00f, 1.20f, PremiumCrystalOpticsParameter.FacetHighlights);
            AddOpticsSlider(panel.Content, "Refraction Strength", "Changes how strongly the background bends through the crystal.", 0.00f, 5.00f, 1.00f, PremiumCrystalOpticsParameter.RefractionStrength);
            AddOpticsSlider(panel.Content, "Reflection Strength", "Makes facets more mirror-like and polished.", 0.00f, 5.00f, 1.10f, PremiumCrystalOpticsParameter.ReflectionStrength);
            AddOpticsSlider(panel.Content, "Internal Reflections", "Adds deeper inner reflection echoes inside the gem.", 0.00f, 6.00f, 1.25f, PremiumCrystalOpticsParameter.InternalReflections);
            AddOpticsSlider(panel.Content, "Background Distortion", "Moves from subtle lensing to surreal image bending.", 0.00f, 5.00f, 0.90f, PremiumCrystalOpticsParameter.BackgroundDistortion);
            AddOpticsSlider(panel.Content, "Direct Transparency", "Controls how much direct background remains visible through crystal.", 0.00f, 1.00f, 0.25f, PremiumCrystalOpticsParameter.DirectTransparency);
            AddOpticsSlider(panel.Content, "Prism Dispersion", "Expands rainbow prism separation on edges and facets.", 0.00f, 5.00f, 1.00f, PremiumCrystalOpticsParameter.PrismDispersion);
            AddOpticsSlider(panel.Content, "Chromatic Aberration", "Adds RGB edge separation and spectral cinematic color.", 0.00f, 3.00f, 0.45f, PremiumCrystalOpticsParameter.ChromaticAberration);
            AddOpticsSlider(panel.Content, "Rainbow Edge", "Controls colorful glowing edges and spectral highlights.", 0.00f, 5.00f, 0.85f, PremiumCrystalOpticsParameter.RainbowEdge);
            AddOpticsSlider(panel.Content, "Spectral Split", "Deepens prismatic energy and color separation.", 0.00f, 4.00f, 0.65f, PremiumCrystalOpticsParameter.SpectralSplit);
            AddOpticsSlider(panel.Content, "Crystal Depth", "Moves from shallow glass to heavy optical mass.", 0.20f, 4.00f, 1.00f, PremiumCrystalOpticsParameter.CrystalDepth);
            KaelisMenuToggleControl causticsToggle = KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Caustics", "Adds focused light traces and projected sparkle patterns.", false, KaelisMenuBindingStatus.RealBinding);
            causticsToggle.SetChangedHandler(value => DispatchPremiumOptic(PremiumCrystalOpticsParameter.Caustics, value ? 1f : 0f));
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Spotlight Shadow", "Adds a controlled shadow relationship for a spotlight-like premium stage.", false, KaelisMenuBindingStatus.Reserved);
            KaelisMenuToggleControl mirrorBackdropToggle = KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Mirror Backdrop", "Controls the hidden reflection backdrop used by polished facets.", true, KaelisMenuBindingStatus.RealBinding);
            mirrorBackdropToggle.SetChangedHandler(value => DispatchPremiumEffect(PremiumCrystalEffectToggle.HiddenReflectionBackground, value));
        }

        private void BuildPresetsPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Presets, "PresetsSectionPanel", "PRESETS", "Factory looks and user slots");
            AddPresetCard(panel.Content, "Diamond Palace", "Clean diamond material, cold blue light, strong highlights, high clarity. Affects: material, bloom, contrast, reflection, dispersion.", PremiumCrystalFactoryPreset.DiamondPalace, KaelisMenuStyle.Cyan);
            AddPresetCard(panel.Content, "Blue Ice", "Cool crystal material, blue ambience, crisp contrast, gentle drift. Affects: material, background mood, contrast.", PremiumCrystalFactoryPreset.BlueIce, KaelisMenuStyle.Cyan);
            AddPresetCard(panel.Content, "Golden Prism", "Warm gold highlights, rich saturation, bright bloom, prism motion. Affects: bloom, dispersion, saturation, motion.", PremiumCrystalFactoryPreset.GoldenPrism, KaelisMenuStyle.GoldSoft);
            AddPresetCard(panel.Content, "Ruby Night", "Ruby accent material, dark luxury background, higher contrast, slow pulse. Affects: material, background, contrast.", PremiumCrystalFactoryPreset.RubyNight, KaelisMenuStyle.Red);
            AddPresetCard(panel.Content, "Emerald Depth", "Emerald/cyan optics profile, deep background, medium bloom, fluid motion. Affects: optics, background, motion.", PremiumCrystalFactoryPreset.EmeraldDepth, KaelisMenuStyle.Cyan);
            AddPresetCard(panel.Content, "Opal Dream", "Opal spectral split, soft background mood, lower contrast, dream motion. Affects: spectral split, contrast, motion.", PremiumCrystalFactoryPreset.OpalDream, KaelisMenuStyle.Cyan);
            AddPresetCard(panel.Content, "Cosmic Glass", "Cosmic backdrop, high saturation, wide spectral edges, orbital motion. Affects: background, saturation, rainbow edge.", PremiumCrystalFactoryPreset.CosmicGlass, KaelisMenuStyle.GoldSoft);
            AddPresetCard(panel.Content, "Dark Luxury", "Restrained bloom, deeper contrast, midnight glass material, calm motion. Affects: contrast, bloom, material.", PremiumCrystalFactoryPreset.DarkLuxury, KaelisMenuStyle.TextMuted);
            AddPresetCard(panel.Content, "Absolute Mirror", "Mirror-polished facets, low direct transparency, hidden reflection depth, sharp prism fire.", PremiumCrystalFactoryPreset.AbsoluteMirror, KaelisMenuStyle.GoldSoft);

            RectTransform actions = CreateActionRow(panel.Content, "PresetActions");
            presetApplyButton = AddActionChip(actions, "ApplyPresetButton", "APPLY SELECTED", KaelisMenuPanelCommand.ApplySelectedPreset, KaelisMenuStyle.GoldSoft);
            presetApplyRow = presetApplyButton.GetComponent<KaelisMenuInteractiveRow>();
            SetPresetApplyAvailable(false);
            AddActionChip(actions, "SaveCurrentPresetButton", "SAVE CURRENT", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted);
            AddActionChip(actions, "RenamePresetButton", "RENAME", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted);
            AddActionChip(actions, "DeletePresetButton", "DELETE", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.Red);
            AddActionChip(actions, "ResetFactoryPresetButton", "RESET FACTORY", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.Cyan);
        }

        private void BuildSettingsPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Settings, "SettingsSectionPanel", "SETTINGS", "Application and system controls");

            AddGroupLabel(panel.Content, "DISPLAY");
            AddCommandRow(panel.Content, "Resolution", "From Screen.resolutions. Reserved until a settings service is present.", "RESERVED", "Screen.resolutions", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.Cyan, 48f);
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Fullscreen", "Switches between fullscreen and windowed display when a safe settings service is added.", false, true);
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "VSync", "Controls vertical sync once a safe settings service is present.", false, true);
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Target FPS", "Selects the desired target frame rate.", 0f, 7f, 1f, string.Empty, true, new[] { "30", "60", "90", "120", "144", "165", "240", "Unlimited" });
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "UI Scale", "Adjusts menu and interface scale.", 70f, 160f, 100f, "%", true);

            AddGroupLabel(panel.Content, "AUDIO");
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Master Volume", "Controls total application output volume.", 0f, 100f, 100f, "%", true);
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Menu Volume", "Controls KAELIS menu ambience and interface sound volume.", 0f, 100f, 80f, "%", true);
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Demo Volume", "Reserved for the later Demo Mode task.", 0f, 100f, 75f, "%", true);
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Mute", "Silences application audio when a safe audio settings bridge exists.", false, true);

            AddGroupLabel(panel.Content, "CONTROLS");
            KaelisMenuToggleControl wheelScaleToggle = KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Mouse Wheel Crystal Scale", "Enables mouse wheel control of Premium3D crystal scale.", true, KaelisMenuBindingStatus.RealBinding);
            wheelScaleToggle.SetChangedHandler(value =>
            {
                if (premiumWheelScaleEnabledHandler != null)
                {
                    premiumWheelScaleEnabledHandler(value);
                }
            });
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Mouse Sensitivity", "Controls pointer and camera sensitivity for runtime interactions.", 0.10f, 5.00f, 1.00f, string.Empty, true);
            KaelisMenuSliderControl scaleStepSlider = KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Crystal Scale Step", "Adjusts per-wheel Premium3D crystal scale changes.", 1f, 50f, 10f, "%", KaelisMenuBindingStatus.RealBinding);
            scaleStepSlider.SetChangedHandler(value =>
            {
                if (premiumWheelScaleStepHandler != null)
                {
                    premiumWheelScaleStepHandler(value);
                }
            });
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Invert Zoom", "Reverses local zoom direction when safe input binding is available.", false, true);
            AddCommandRow(panel.Content, "Hotkeys", "Future dedicated hotkey panel. Reserved.", "RESERVED", "Hotkey panel", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted, 48f);
            AddCommandRow(panel.Content, "Reset Hotkeys", "Reserved until key binding persistence exists.", "RESERVED", "Reset hotkeys", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted, 48f);

            AddGroupLabel(panel.Content, "SYSTEM");
            AddLanguageRow(panel.Content);
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Start With Menu", "Controls whether KAELIS starts with this premium menu shell.", true, true);
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Auto Save Settings", "Automatically persists settings when a safe persistence service exists.", false, true);
            AddCommandRow(panel.Content, "Reset Settings", "Confirmation required. Reserved until settings persistence exists.", "RESERVED", "Reset settings", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.Red, 48f);
            AddCommandRow(panel.Content, "Open Logs Folder", "Reserved until a safe logs folder hook exists.", "RESERVED", "Open logs", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted, 48f);

            AddGroupLabel(panel.Content, "DIAGNOSTICS");
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Show FPS", "Shows a lightweight FPS overlay once diagnostics toggles are safely exposed.", false, true);
            AddCommandRow(panel.Content, "Show Diagnostics", "Uses existing public diagnostics visibility command.", "SAFE COMMAND", "SetDiagnosticsVisible(true)", KaelisMenuPanelCommand.ShowDiagnostics, KaelisMenuStyle.GoldSoft, 48f);
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Show Input Overlay", "Displays input visualization once a safe diagnostics bridge exists.", false, true);
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Show Render Stats", "Displays render statistics once a safe diagnostics bridge exists.", false, true);
        }

        private void BuildExitPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Exit, "ExitSectionPanel", "EXIT KAELIS?", "Confirm application exit");

            TMP_Text body = KaelisMenuUiPrimitives.CreateText(panel.Content, "ExitBody", "Close the KAELIS experience shell. Saving settings is reserved until a persistence service exists.", 18f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Status));
            body.enableWordWrapping = true;
            body.characterSpacing = 2f;
            KaelisMenuUiPrimitives.AddLayout(body.gameObject, -1f, 120f);

            RectTransform actions = CreateActionRow(panel.Content, "ExitActions");
            AddActionChip(actions, "ExitSaveAndQuitButton", "SAVE SETTINGS AND EXIT", KaelisMenuPanelCommand.SaveAndExit, KaelisMenuStyle.GoldSoft);
            AddActionChip(actions, "ExitWithoutSavingButton", "EXIT WITHOUT SAVING", KaelisMenuPanelCommand.ExitWithoutSaving, KaelisMenuStyle.Red);
            AddActionChip(actions, "ExitCancelButton", "CANCEL", KaelisMenuPanelCommand.CancelExit, KaelisMenuStyle.Cyan);
        }

        private KaelisMenuSectionPanel CreatePanel(KaelisMenuSection section, string name, string title, string subtitle)
        {
            RectTransform panel = KaelisMenuUiPrimitives.CreateRect(name, Root);
            KaelisMenuUiPrimitives.Stretch(panel);
            KaelisMenuUiPrimitives.AddImage(panel, assets.SolidSprite, new Color(0.006f, 0.030f, 0.044f, 0.82f), true);

            RectTransform haze = KaelisMenuUiPrimitives.CreateRect("PanelHaze", panel);
            KaelisMenuUiPrimitives.Stretch(haze);
            KaelisMenuUiPrimitives.AddImage(haze, assets.SolidSprite, new Color(0.12f, 0.58f, 0.72f, 0.075f), false);

            RectTransform topGlow = KaelisMenuUiPrimitives.CreateRect("PanelTopGlow", panel);
            topGlow.anchorMin = new Vector2(0f, 0.74f);
            topGlow.anchorMax = new Vector2(1f, 1f);
            topGlow.offsetMin = Vector2.zero;
            topGlow.offsetMax = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(topGlow, assets.SolidSprite, new Color(0.18f, 0.82f, 1f, 0.10f), false);

            KaelisMenuUiPrimitives.AddFrame(panel, new Color(0.76f, 0.98f, 1f, 0.42f), new Color(0.20f, 0.82f, 0.92f, 0.24f), 1.05f, assets.SolidSprite);
            KaelisMenuUiPrimitives.AddCornerCuts(panel, new Color(1f, 0.76f, 0.36f, 0.32f), 32f, 1.15f, assets.SolidSprite);

            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();

            RectTransform header = KaelisMenuUiPrimitives.CreateRect("SectionHeader", panel);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, 92f);
            header.anchoredPosition = Vector2.zero;

            TMP_Text titleText = KaelisMenuUiPrimitives.CreateText(header, "SectionTitle", title, 28f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Button));
            titleText.characterSpacing = 8f;
            RectTransform titleRect = (RectTransform)titleText.transform;
            titleRect.offsetMin = new Vector2(34f, 35f);
            titleRect.offsetMax = new Vector2(-170f, -8f);

            TMP_Text subtitleText = KaelisMenuUiPrimitives.CreateText(header, "SectionSubtitle", subtitle, 13f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            subtitleText.characterSpacing = 4f;
            RectTransform subtitleRect = (RectTransform)subtitleText.transform;
            subtitleRect.offsetMin = new Vector2(36f, 12f);
            subtitleRect.offsetMax = new Vector2(-170f, -56f);

            AddActionChip(header, "SectionCloseButton", "CLOSE", KaelisMenuPanelCommand.CloseSection, KaelisMenuStyle.Cyan, new Vector2(124f, 42f), new Vector2(1f, 1f), new Vector2(-92f, -38f));

            RectTransform viewport = KaelisMenuUiPrimitives.CreateRect("SectionViewport", panel);
            KaelisMenuUiPrimitives.Stretch(viewport);
            viewport.offsetMin = new Vector2(34f, 34f);
            viewport.offsetMax = new Vector2(-34f, -102f);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = KaelisMenuUiPrimitives.CreateRect("SectionContent", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = panel.gameObject.AddComponent<ScrollRect>();
            scrollRect.content = content;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 28f;

            KaelisMenuSectionPanel sectionPanel = new KaelisMenuSectionPanel(panel, content, group);
            panels.Add(section, sectionPanel);
            return sectionPanel;
        }

        private void AddModeRow(RectTransform parent, string title, string description, string badge, string commandId, KaelisMenuPanelCommand command, Color accent, bool selectable)
        {
            KaelisMenuInteractiveRow row = AddCommandRow(parent, title, description, badge, commandId, command, accent, 70f);
            if (!selectable)
            {
                row.SetSelectableVisual(false);
            }

            Button button = row.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (selectable)
                {
                    SelectModeRow(row);
                }

                row.Flash();
                Submit(command);
            });
        }

        private void AddPresetCard(RectTransform parent, string title, string description, PremiumCrystalFactoryPreset preset, Color accent)
        {
            KaelisMenuInteractiveRow row = AddCommandRow(parent, title, description, "FACTORY", "ApplyPremiumCrystalPreset(" + preset + ")", KaelisMenuPanelCommand.ReservedAction, accent, 62f);
            Button button = row.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SelectPresetRow(row, preset);
                row.Flash();
            });
        }

        private void AddLanguageRow(RectTransform parent)
        {
            KaelisMenuInteractiveRow row = AddCommandRow(parent, "Language", "Changes the KAELIS menu language immediately and stores it for the next session.", KaelisMenuLocalizationService.GetCurrentLanguageDisplayName(), "English / Русский / Deutsch / Українська", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.Cyan, 56f, KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Language));
            Button button = row.GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            TMP_Text badge = null;
            Transform badgeTransform = row.transform.Find("ItemBadge");
            if (badgeTransform != null)
            {
                badge = badgeTransform.GetComponent<TMP_Text>();
            }

            KaelisMenuLanguageSelector selector = row.gameObject.AddComponent<KaelisMenuLanguageSelector>();
            selector.Initialize(row, badge);
            row.SetSelected(true);
            row.SetTooltipCurrent(KaelisMenuLocalizationService.GetCurrentLanguageDisplayName());
            button.onClick.AddListener(selector.CycleLanguage);
        }

        private void AddOpticsSlider(RectTransform parent, string title, string description, float min, float max, float defaultValue, PremiumCrystalOpticsParameter parameter)
        {
            KaelisMenuSliderControl control = KaelisMenuSliderControl.Create(parent, assets, tooltip, title, description, min, max, defaultValue, string.Empty, KaelisMenuBindingStatus.RealBinding);
            control.SetChangedHandler(value => DispatchPremiumOptic(parameter, value));
        }

        private void DispatchPremiumOptic(PremiumCrystalOpticsParameter parameter, float value)
        {
            if (premiumOpticsHandler != null)
            {
                premiumOpticsHandler(parameter, value);
            }
        }

        private void DispatchPremiumEffect(PremiumCrystalEffectToggle effect, bool value)
        {
            if (premiumEffectHandler != null)
            {
                premiumEffectHandler(effect, value);
            }
        }

        public void RefreshSecondDisplayAvailability()
        {
            if (ProductionOptions == null)
            {
                return;
            }

            ProductionOptions.SecondDisplayAvailable = IsSecondDisplayAvailable();
            if (!ProductionOptions.SecondDisplayAvailable)
            {
                ProductionOptions.OutputToSecondDisplay = false;
            }

            RefreshProductionUi();
        }

        public void SetSecondDisplayOutputState(bool enabled)
        {
            if (ProductionOptions == null)
            {
                return;
            }

            ProductionOptions.OutputToSecondDisplay = enabled && ProductionOptions.SecondDisplayAvailable;
            RefreshProductionUi();
        }

        public void SetRecordingOutputFolder(string folderPath)
        {
            if (ProductionOptions == null)
            {
                return;
            }

            ProductionOptions.RecordingOutputFolder = folderPath;
            RefreshProductionUi();
        }

        public void ClearRecordingOutputFolder()
        {
            if (ProductionOptions == null)
            {
                return;
            }

            ProductionOptions.RecordingOutputFolder = null;
            RefreshProductionUi();
        }

        public string GetRecordingStatusForStatusBar()
        {
            if (ProductionOptions == null || !ProductionOptions.CreateVideoClip)
            {
                return "RECORDING OFF";
            }

            if (!ProductionOptions.HasRecordingOutputFolder)
            {
                return "OUTPUT FOLDER MISSING";
            }

            return ProductionOptions.RecordingBackendAvailable ? "RECORDING READY" : "RECORDING RESERVED";
        }

        private void OnSecondDisplayToggleChanged(bool enabled)
        {
            if (ProductionOptions == null)
            {
                return;
            }

            if (enabled && !ProductionOptions.SecondDisplayAvailable)
            {
                ProductionOptions.OutputToSecondDisplay = false;
                if (secondDisplayToggle != null)
                {
                    secondDisplayToggle.SetValueWithoutNotify(false);
                }

                RefreshProductionUi();
                Submit(KaelisMenuPanelCommand.SetSecondDisplayOutput);
                return;
            }

            ProductionOptions.OutputToSecondDisplay = enabled;
            RefreshProductionUi();
            Submit(KaelisMenuPanelCommand.SetSecondDisplayOutput);
        }

        private void OnRecordingToggleChanged(bool enabled)
        {
            if (ProductionOptions == null)
            {
                return;
            }

            ProductionOptions.CreateVideoClip = enabled;
            ProductionOptions.AutoStartRecordingWithExperience = enabled;
            ProductionOptions.IncludeAudioIfAvailable = true;
            RefreshProductionUi();
            Submit(KaelisMenuPanelCommand.SetRecordingEnabled);
        }

        private void RefreshProductionUi()
        {
            if (ProductionOptions == null)
            {
                return;
            }

            if (secondDisplayToggle != null)
            {
                secondDisplayToggle.SetValueWithoutNotify(ProductionOptions.OutputToSecondDisplay);
                secondDisplayToggle.SetInteractable(ProductionOptions.SecondDisplayAvailable);
                secondDisplayToggle.SetTooltipCurrent(ProductionOptions.SecondDisplayAvailable ? "SECOND DISPLAY READY" : "NO SECOND DISPLAY DETECTED");
            }

            if (secondDisplayStatusText != null)
            {
                string status = ProductionOptions.SecondDisplayAvailable
                    ? (ProductionOptions.OutputToSecondDisplay ? "Second display: Output enabled" : "Second display: Available")
                    : "Second display: Not detected";
                KaelisMenuLocalizationService.SetText(secondDisplayStatusText, status);
                secondDisplayStatusText.color = ProductionOptions.SecondDisplayAvailable ? KaelisMenuStyle.Cyan : KaelisMenuStyle.TextMuted;
            }

            if (testSecondDisplayButton != null)
            {
                testSecondDisplayButton.interactable = ProductionOptions.SecondDisplayAvailable;
            }

            if (testSecondDisplayRow != null)
            {
                testSecondDisplayRow.SetSelectableVisual(ProductionOptions.SecondDisplayAvailable);
            }

            if (recordingToggle != null)
            {
                recordingToggle.SetValueWithoutNotify(ProductionOptions.CreateVideoClip);
                recordingToggle.SetTooltipCurrent(GetRecordingStatusForStatusBar());
            }

            if (recordingOutputFolderText != null)
            {
                if (ProductionOptions.HasRecordingOutputFolder)
                {
                    KaelisMenuLocalizationService.SetRawText(recordingOutputFolderText, "Output folder: " + ProductionOptions.RecordingOutputFolder);
                    recordingOutputFolderText.color = KaelisMenuStyle.TextSecondary;
                }
                else
                {
                    KaelisMenuLocalizationService.SetText(recordingOutputFolderText, "No output folder selected");
                    recordingOutputFolderText.color = ProductionOptions.CreateVideoClip ? KaelisMenuStyle.Red : KaelisMenuStyle.TextMuted;
                }
            }

            if (recordingStatusText != null)
            {
                string recordingStatus = GetRecordingLine();
                KaelisMenuLocalizationService.SetText(recordingStatusText, recordingStatus);
                recordingStatusText.color = ProductionOptions.CreateVideoClip && !ProductionOptions.HasRecordingOutputFolder
                    ? KaelisMenuStyle.Red
                    : (ProductionOptions.CreateVideoClip ? KaelisMenuStyle.GoldSoft : KaelisMenuStyle.TextMuted);
            }

            if (secondDisplayInfoText != null)
            {
                KaelisMenuLocalizationService.SetText(secondDisplayInfoText, "First monitor remains the control panel. Kaleidoscope output is sent to the second monitor.");
            }

            if (recordingHotkeyText != null)
            {
                KaelisMenuLocalizationService.SetText(recordingHotkeyText, "Ctrl + Shift + R = Start / Stop recording");
            }
        }

        private TMP_Text AddStatusLine(RectTransform parent, string name, string text, Color color, float height)
        {
            RectTransform row = KaelisMenuUiPrimitives.CreateRect(name + "Row", parent);
            KaelisMenuUiPrimitives.AddLayout(row.gameObject, -1f, height);

            TMP_Text line = KaelisMenuUiPrimitives.CreateText(row, name, text, 12f, color, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            line.enableWordWrapping = true;
            line.characterSpacing = 1.8f;
            RectTransform lineRect = (RectTransform)line.transform;
            lineRect.offsetMin = new Vector2(4f, 0f);
            lineRect.offsetMax = new Vector2(-4f, 0f);
            return line;
        }

        private string GetRecordingLine()
        {
            if (ProductionOptions == null || !ProductionOptions.CreateVideoClip)
            {
                return "Recording: Disabled";
            }

            if (!ProductionOptions.HasRecordingOutputFolder)
            {
                return "Recording: Output folder missing";
            }

            return ProductionOptions.RecordingBackendAvailable ? "Recording: Ready" : "Recording: Reserved";
        }

        private static bool IsSecondDisplayAvailable()
        {
            return Display.displays != null && Display.displays.Length > 1;
        }

        private KaelisMenuInteractiveRow AddCommandRow(RectTransform parent, string title, string description, string badge, string commandId, KaelisMenuPanelCommand command, Color accent, float height, string hint = null)
        {
            RectTransform row = KaelisMenuUiPrimitives.CreateRect(ToObjectName(title) + "Command", parent);
            KaelisMenuUiPrimitives.AddLayout(row.gameObject, -1f, height);
            Image surface = KaelisMenuUiPrimitives.AddImage(row, assets.SolidSprite, new Color(0.010f, 0.060f, 0.080f, 0.58f), true);
            KaelisMenuUiPrimitives.AddFrame(row, new Color(accent.r, accent.g, accent.b, 0.24f), new Color(0.16f, 0.55f, 0.64f, 0.16f), 0.8f, assets.SolidSprite);
            KaelisMenuSliderControl.AddWideHighlight(row, assets, out CanvasGroup highlightGroup, out CanvasGroup flashGroup);

            KaelisMenuInteractiveRow interactive = row.gameObject.AddComponent<KaelisMenuInteractiveRow>();
            interactive.Configure(surface, highlightGroup, flashGroup, tooltip, title, description, "Status: " + badge, "Command: " + commandId, hint ?? KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Select));

            Button button = row.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = surface;
            button.onClick.AddListener(() =>
            {
                interactive.Flash();
                Submit(command);
            });

            RectTransform accentBar = KaelisMenuUiPrimitives.CreateRect("AccentBar", row);
            accentBar.anchorMin = new Vector2(0f, 0.18f);
            accentBar.anchorMax = new Vector2(0f, 0.82f);
            accentBar.sizeDelta = new Vector2(4f, 0f);
            accentBar.anchoredPosition = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(accentBar, assets.SolidSprite, new Color(accent.r, accent.g, accent.b, 0.78f), false);

            TMP_Text titleText = KaelisMenuUiPrimitives.CreateText(row, "ItemTitle", title.ToUpperInvariant(), 15f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Button));
            titleText.characterSpacing = 4f;
            RectTransform titleRect = (RectTransform)titleText.transform;
            titleRect.offsetMin = new Vector2(22f, height * 0.48f - 2f);
            titleRect.offsetMax = new Vector2(-185f, -5f);

            TMP_Text descriptionText = KaelisMenuUiPrimitives.CreateText(row, "ItemDescription", description, 12f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            descriptionText.enableWordWrapping = true;
            RectTransform descriptionRect = (RectTransform)descriptionText.transform;
            descriptionRect.offsetMin = new Vector2(22f, 7f);
            descriptionRect.offsetMax = new Vector2(-185f, -(height * 0.44f));

            TMP_Text badgeText = KaelisMenuUiPrimitives.CreateText(row, "ItemBadge", badge, 11f, new Color(accent.r, accent.g, accent.b, 0.92f), TextAlignmentOptions.Right, assets.GetFont(KaelisMenuFontRole.Status));
            badgeText.characterSpacing = 3f;
            RectTransform badgeRect = (RectTransform)badgeText.transform;
            badgeRect.offsetMin = new Vector2(0f, 0f);
            badgeRect.offsetMax = new Vector2(-20f, 0f);
            return interactive;
        }

        private void AddGroupLabel(RectTransform parent, string label)
        {
            RectTransform group = KaelisMenuUiPrimitives.CreateRect(label + "GroupLabel", parent);
            KaelisMenuUiPrimitives.AddLayout(group.gameObject, -1f, 28f);

            TMP_Text text = KaelisMenuUiPrimitives.CreateText(group, "GroupText", label, 13f, KaelisMenuStyle.GoldSoft, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            text.characterSpacing = 5f;
            RectTransform textRect = (RectTransform)text.transform;
            textRect.offsetMin = new Vector2(4f, 0f);
            textRect.offsetMax = new Vector2(-4f, 0f);
        }

        private RectTransform CreateActionRow(RectTransform parent, string name)
        {
            RectTransform actions = KaelisMenuUiPrimitives.CreateRect(name, parent);
            KaelisMenuUiPrimitives.AddLayout(actions.gameObject, -1f, 68f);
            HorizontalLayoutGroup layout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return actions;
        }

        private Button AddActionChip(RectTransform parent, string name, string label, KaelisMenuPanelCommand command, Color accent)
        {
            RectTransform chip = KaelisMenuUiPrimitives.CreateRect(name, parent);
            KaelisMenuUiPrimitives.AddLayout(chip.gameObject, -1f, 54f);
            return BuildActionChip(chip, label, command, accent);
        }

        private Button AddActionChip(RectTransform parent, string name, string label, KaelisMenuPanelCommand command, Color accent, Vector2 size, Vector2 anchor, Vector2 anchoredPosition)
        {
            RectTransform chip = KaelisMenuUiPrimitives.CreateRect(name, parent);
            chip.anchorMin = anchor;
            chip.anchorMax = anchor;
            chip.pivot = new Vector2(0.5f, 0.5f);
            chip.sizeDelta = size;
            chip.anchoredPosition = anchoredPosition;
            return BuildActionChip(chip, label, command, accent);
        }

        private Button BuildActionChip(RectTransform chip, string label, KaelisMenuPanelCommand command, Color accent)
        {
            Image surface = KaelisMenuUiPrimitives.AddImage(chip, assets.SolidSprite, new Color(0.010f, 0.060f, 0.080f, 0.70f), true);
            KaelisMenuUiPrimitives.AddFrame(chip, new Color(accent.r, accent.g, accent.b, 0.38f), new Color(accent.r, accent.g, accent.b, 0.18f), 0.95f, assets.SolidSprite);
            KaelisMenuSliderControl.AddWideHighlight(chip, assets, out CanvasGroup highlightGroup, out CanvasGroup flashGroup);

            KaelisMenuInteractiveRow interactive = chip.gameObject.AddComponent<KaelisMenuInteractiveRow>();
            interactive.Configure(surface, highlightGroup, flashGroup, tooltip, label, "Menu action.", "Status: " + (command == KaelisMenuPanelCommand.ReservedAction ? "RESERVED" : "SAFE COMMAND"), "Command: " + command, KaelisMenuInputHintProvider.Get(command == KaelisMenuPanelCommand.ReservedAction ? KaelisMenuInputHintKind.Reserved : KaelisMenuInputHintKind.Action));

            Button button = chip.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = surface;
            button.onClick.AddListener(() =>
            {
                interactive.Flash();
                Submit(command);
            });

            TMP_Text text = KaelisMenuUiPrimitives.CreateText(chip, "Label", label, 12.5f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Button));
            text.enableAutoSizing = true;
            text.fontSizeMin = 9f;
            text.fontSizeMax = 12.5f;
            text.characterSpacing = 3f;
            RectTransform textRect = (RectTransform)text.transform;
            textRect.offsetMin = new Vector2(10f, 0f);
            textRect.offsetMax = new Vector2(-10f, 0f);
            return button;
        }

        private void SelectModeRow(KaelisMenuInteractiveRow row)
        {
            if (selectedModeRow != null && selectedModeRow != row)
            {
                selectedModeRow.SetSelected(false);
            }

            selectedModeRow = row;
            if (selectedModeRow != null)
            {
                selectedModeRow.SetSelected(true);
            }
        }

        private void SelectPresetRow(KaelisMenuInteractiveRow row, PremiumCrystalFactoryPreset preset)
        {
            if (selectedPresetRow != null && selectedPresetRow != row)
            {
                selectedPresetRow.SetSelected(false);
            }

            selectedPremiumPreset = preset;
            hasSelectedPremiumPreset = true;
            selectedPresetRow = row;
            if (selectedPresetRow != null)
            {
                selectedPresetRow.SetSelected(true);
            }

            SetPresetApplyAvailable(true);
        }

        public bool TryGetSelectedPremiumPreset(out PremiumCrystalFactoryPreset preset)
        {
            preset = selectedPremiumPreset;
            return hasSelectedPremiumPreset;
        }

        private void SetPresetApplyAvailable(bool available)
        {
            if (presetApplyButton != null)
            {
                presetApplyButton.interactable = available;
            }

            if (presetApplyRow != null)
            {
                presetApplyRow.SetSelectableVisual(available);
            }
        }

        private void Submit(KaelisMenuPanelCommand command)
        {
            if (commandHandler != null)
            {
                commandHandler(command);
            }
        }

        private static string ToObjectName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "MenuItem";
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

            return writeIndex > 0 ? new string(characters, 0, writeIndex) : "MenuItem";
        }

        private sealed class KaelisMenuSectionPanel
        {
            private readonly CanvasGroup canvasGroup;

            public KaelisMenuSectionPanel(RectTransform root, RectTransform content, CanvasGroup canvasGroup)
            {
                Root = root;
                Content = content;
                this.canvasGroup = canvasGroup;
            }

            public RectTransform Root { get; private set; }
            public RectTransform Content { get; private set; }

            public void SetVisible(bool visible)
            {
                Root.gameObject.SetActive(visible);
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }
    }
}
