using System;
using System.Collections.Generic;
using Kaleidoscope2.Core;
using Kaleidoscope2.Menu.About;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    internal sealed class KaelisMenuSectionController
    {
        private readonly KaelisMenuAssets assets;
        private readonly KaelisMenuTooltip tooltip;
        private readonly Dictionary<KaelisMenuSection, KaelisMenuSectionPanel> panels = new Dictionary<KaelisMenuSection, KaelisMenuSectionPanel>();
        private readonly KaelisMenuSectionGeometryDiagnostics geometryDiagnostics;
        private Action<KaelisMenuPanelCommand> commandHandler;
        private KaelisMenuInteractiveRow selectedModeRow;
        private KaelisMenuInteractiveRow selectedPresetRow;
        private Button presetApplyButton;
        private KaelisMenuInteractiveRow presetApplyRow;
        private KaelisMenuToggleControl secondDisplayToggle;
        private KaelisMenuToggleControl recordingToggle;
        private Button testSecondDisplayButton;
        private KaelisMenuInteractiveRow testSecondDisplayRow;
        private Button selectRecordingOutputFolderButton;
        private Button clearRecordingOutputFolderButton;
        private KaelisMenuInteractiveRow selectRecordingOutputFolderRow;
        private KaelisMenuInteractiveRow clearRecordingOutputFolderRow;
        private Button demoSaveBenchmarkResultButton;
        private Button benchmarkSaveResultButton;
        private KaelisMenuInteractiveRow demoSaveBenchmarkResultRow;
        private KaelisMenuInteractiveRow benchmarkSaveResultRow;
        private KaelisMenuToggleControl autoSaveSettingsToggle;
        private TMP_Text secondDisplayStatusText;
        private TMP_Text secondDisplayInfoText;
        private TMP_Text recordingOutputFolderText;
        private TMP_Text recordingStatusText;
        private TMP_Text recordingHotkeyText;
        private PremiumCrystalFactoryPreset selectedPremiumPreset = PremiumCrystalFactoryPreset.DiamondPalace;
        private bool hasSelectedPremiumPreset;
        private KaelisMenuInteractiveRow selectedPremiumShapeRow;
        private Action<PremiumCrystalOpticsParameter, float> premiumOpticsHandler;
        private Action<PremiumCrystalEffectToggle, bool> premiumEffectHandler;
        private Action<PremiumCrystalShapeType> premiumShapeHandler;
        private Action<PremiumCrystalOpticalMode> premiumOpticalModeHandler;
        private Action<bool> premiumWheelScaleEnabledHandler;
        private Action<float> premiumWheelScaleStepHandler;
        private Action<int> crystalDebugModeHandler;
        private Action<CrystalExperimentPresetType> experimentalCrystalPresetHandler;

        public KaelisMenuSectionController(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip)
        {
            this.assets = assets;
            this.tooltip = tooltip;

            Root = KaelisMenuUiPrimitives.CreateRect("SectionPanelsRoot", parent);
            KaelisMenuUiPrimitives.Stretch(Root);
            Root.offsetMin = new Vector2(54f, 62f);
            Root.offsetMax = new Vector2(-54f, -58f);
            geometryDiagnostics = Root.gameObject.AddComponent<KaelisMenuSectionGeometryDiagnostics>();

            ProductionOptions = new KaelisProductionOptions();
            SettingsOptions = new KaelisSettingsMenuOptions();
            RefreshSecondDisplayAvailability();

            BuildModesPanel();
            BuildOpticsPanel();
            BuildPresetsPanel();
            BuildSettingsPanel();
            BuildAboutPanel();
            BuildDemoPanel();
            BuildMeditationPanel();
            BuildReplayPanel();
            BuildBenchmarkPanel();
            BuildExitPanel();
            SetBenchmarkResultAvailable(false);
            Close();
        }

        public RectTransform Root { get; private set; }
        public KaelisMenuSection ActiveSection { get; private set; }
        public KaelisProductionOptions ProductionOptions { get; private set; }
        public KaelisSettingsMenuOptions SettingsOptions { get; private set; }

        public void SetCommandHandler(Action<KaelisMenuPanelCommand> handler)
        {
            commandHandler = handler;
        }

        public void SetPremiumCrystalHandlers(
            Action<PremiumCrystalOpticsParameter, float> opticsHandler,
            Action<PremiumCrystalEffectToggle, bool> effectHandler,
            Action<PremiumCrystalShapeType> shapeHandler,
            Action<PremiumCrystalOpticalMode> opticalModeHandler,
            Action<bool> wheelScaleEnabledHandler,
            Action<float> wheelScaleStepHandler,
            Action<int> debugModeHandler,
            Action<CrystalExperimentPresetType> experimentPresetHandler)
        {
            premiumOpticsHandler = opticsHandler;
            premiumEffectHandler = effectHandler;
            premiumShapeHandler = shapeHandler;
            premiumOpticalModeHandler = opticalModeHandler;
            premiumWheelScaleEnabledHandler = wheelScaleEnabledHandler;
            premiumWheelScaleStepHandler = wheelScaleStepHandler;
            crystalDebugModeHandler = debugModeHandler;
            experimentalCrystalPresetHandler = experimentPresetHandler;
        }

        public void SetActiveSection(KaelisMenuSection section)
        {
            ActiveSection = section;

            foreach (KeyValuePair<KaelisMenuSection, KaelisMenuSectionPanel> pair in panels)
            {
                pair.Value.ApplySharedLayout("KaelisMenuSectionController.SetActiveSection");
                pair.Value.SetVisible(pair.Key == section);
            }

            if (section != KaelisMenuSection.None && panels.TryGetValue(section, out KaelisMenuSectionPanel activePanel))
            {
                geometryDiagnostics.RequestValidation(section.ToString(), activePanel.Root);
            }

            if (section == KaelisMenuSection.Modes)
            {
                RefreshSecondDisplayAvailability();
                RefreshProductionUi();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (section == KaelisMenuSection.About)
            {
                LogAboutCloseButtonRaycastDiagnostic();
            }
#endif
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
            AddGroupLabel(panel.Content, "PREMIUM3D FORMS");
            AddStatusLine(panel.Content, "SmoothGeometryShortcutHint", "+ / - : Smooth Crystal Geometry    Num Del : Cycle Selected Control Class", KaelisMenuStyle.TextSecondary, 26f);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.Sphere, "Sphere", "Centered multifacet sphere with a balanced optical core.", KaelisMenuStyle.Cyan);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.Cube, "Cube", "Symmetric faceted cube with broad polished planes.", KaelisMenuStyle.GoldSoft);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.Octahedron, "Octahedron", "Clean double-sided point crystal with four-way symmetry.", KaelisMenuStyle.Cyan);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.Hexahedron, "Hexahedron", "Six-sided prism-like crystal with balanced front and back faces.", KaelisMenuStyle.GoldSoft);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.VolumetricRhombus, "Volumetric Rhombus", "Symmetric diamond body with deep mirrored facets.", KaelisMenuStyle.Cyan);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.Cone, "Cone", "Centered rotational crystal cone with a closed base.", KaelisMenuStyle.Cyan);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.PlateDisc, "Plate / Disc Crystal", "Broad symmetrical disc with readable edge thickness.", KaelisMenuStyle.GoldSoft);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.Icosahedron, "Icosahedron", "Dense balanced polyhedral gem with sharp facet changes.", KaelisMenuStyle.Cyan);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.Dodecahedron, "Dodecahedron", "Twelve-sided premium polygon crystal with full depth.", KaelisMenuStyle.GoldSoft);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.DoublePyramid, "Double Pyramid", "Symmetric bipyramid with a visible central girdle.", KaelisMenuStyle.Cyan);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.CrystalLens, "Crystal Lens", "Convex lens crystal with a solid rounded volume.", KaelisMenuStyle.Cyan);
            AddPremiumShapeRow(panel.Content, PremiumCrystalShapeType.StarPrism, "Star Prism", "Symmetric radial star crystal with closed prism volume.", KaelisMenuStyle.GoldSoft);
            AddGroupLabel(panel.Content, "PREMIUM OPTICAL MODE");
            AddPremiumOpticalModeRow(panel.Content, PremiumCrystalOpticalMode.HighPurityDiamond, "High-Purity Diamond", "Clear gemstone optics with controlled transmission.", KaelisMenuStyle.Cyan);
            AddPremiumOpticalModeRow(panel.Content, PremiumCrystalOpticalMode.PrismDispersion, "Prism Dispersion", "High spectral splitting with visible facet refraction.", KaelisMenuStyle.GoldSoft);
            AddPremiumOpticalModeRow(panel.Content, PremiumCrystalOpticalMode.MirrorFacets, "Mirror Facets", "Polished reflective facets with restrained transparency.", KaelisMenuStyle.Cyan);
            AddPremiumOpticalModeRow(panel.Content, PremiumCrystalOpticalMode.InternalReflection, "Internal Reflection", "Layered inner echoes and deeper optical mass.", KaelisMenuStyle.Cyan);
            AddPremiumOpticalModeRow(panel.Content, PremiumCrystalOpticalMode.AbsoluteMirror, "Absolute Mirror", "Solid reflective crystal. Transparency is disabled.", KaelisMenuStyle.GoldSoft);
            AddPremiumOpticalModeRow(panel.Content, PremiumCrystalOpticalMode.AlienArtifactExperimental, "Alien Artifact / Experimental", "Enters the preserved Alien Artifact Core experiment profile.", KaelisMenuStyle.Cyan);
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

            recordingToggle = KaelisMenuToggleControl.Create(content, assets, tooltip, "Create Video Clip", "Records kaleidoscope output to a video file. Auto-starts when experience starts if enabled.", false, KaelisMenuBindingStatus.Reserved, "Available after Showcase Recording service is implemented.");
            recordingToggle.gameObject.name = "CreateVideoClipToggle";
            recordingToggle.SetChangedHandler(OnRecordingToggleChanged);
            recordingOutputFolderText = AddStatusLine(content, "RecordingOutputFolder", "No output folder selected", KaelisMenuStyle.TextMuted, 30f);

            RectTransform recordingActions = CreateActionRow(content, "RecordingOutputActions");
            selectRecordingOutputFolderButton = AddActionChip(recordingActions, "SelectRecordingOutputFolderButton", "SELECT OUTPUT FOLDER", KaelisMenuPanelCommand.SelectRecordingOutputFolder, KaelisMenuStyle.GoldSoft);
            selectRecordingOutputFolderRow = selectRecordingOutputFolderButton.GetComponent<KaelisMenuInteractiveRow>();
            clearRecordingOutputFolderButton = AddActionChip(recordingActions, "ClearRecordingOutputFolderButton", "CLEAR", KaelisMenuPanelCommand.ClearRecordingOutputFolder, KaelisMenuStyle.Cyan);
            clearRecordingOutputFolderRow = clearRecordingOutputFolderButton.GetComponent<KaelisMenuInteractiveRow>();

            recordingStatusText = AddStatusLine(content, "RecordingStatus", "Recording: Showcase Recording service missing", KaelisMenuStyle.GoldSoft, 26f);
            recordingHotkeyText = AddStatusLine(content, "RecordingHotkeyHint", "Recording hotkey unavailable until Showcase Recording service is implemented.", KaelisMenuStyle.TextSecondary, 26f);
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
            AddOpticsSlider(panel.Content, "Direct Transparency", "Controls how much direct background remains visible through crystal.", 0.00f, 1.00f, 0.08f, PremiumCrystalOpticsParameter.DirectTransparency);
            AddOpticsSlider(panel.Content, "Prism Dispersion", "Expands rainbow prism separation on edges and facets.", 0.00f, 5.00f, 1.00f, PremiumCrystalOpticsParameter.PrismDispersion);
            AddOpticsSlider(panel.Content, "Chromatic Aberration", "Adds RGB edge separation and spectral cinematic color.", 0.00f, 3.00f, 0.45f, PremiumCrystalOpticsParameter.ChromaticAberration);
            AddOpticsSlider(panel.Content, "Rainbow Edge", "Controls colorful glowing edges and spectral highlights.", 0.00f, 5.00f, 0.85f, PremiumCrystalOpticsParameter.RainbowEdge);
            AddOpticsSlider(panel.Content, "Spectral Split", "Deepens prismatic energy and color separation.", 0.00f, 4.00f, 0.65f, PremiumCrystalOpticsParameter.SpectralSplit);
            AddOpticsSlider(panel.Content, "Crystal Depth", "Moves from shallow glass to heavy optical mass.", 0.20f, 4.00f, 1.00f, PremiumCrystalOpticsParameter.CrystalDepth);
            KaelisMenuToggleControl causticsToggle = KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Caustics", "Adds focused light traces and projected sparkle patterns.", false, KaelisMenuBindingStatus.RealBinding);
            causticsToggle.SetChangedHandler(value => DispatchPremiumOptic(PremiumCrystalOpticsParameter.Caustics, value ? 1f : 0f));
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Spotlight Shadow", "Adds a controlled shadow relationship for a spotlight-like premium stage.", false, KaelisMenuBindingStatus.Reserved, "Available after Premium Light Rig shadow service is implemented.");
            KaelisMenuToggleControl mirrorBackdropToggle = KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Mirror Backdrop", "Controls the hidden reflection backdrop used by polished facets.", true, KaelisMenuBindingStatus.RealBinding);
            mirrorBackdropToggle.SetChangedHandler(value => DispatchPremiumEffect(PremiumCrystalEffectToggle.HiddenReflectionBackground, value));
        }

        private void BuildPresetsPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Presets, "PresetsSectionPanel", "PRESETS", "Factory looks and user slots");
            AddPresetCard(panel.Content, "Diamond Palace", "Clean diamond material, cold blue light, strong highlights, high clarity. Affects: material, bloom, contrast, reflection, dispersion.", PremiumCrystalFactoryPreset.DiamondPalace, KaelisMenuStyle.Cyan);
            AddPresetCard(panel.Content, "Blue Ice", "Hexahedron ice material, blue ambience, crisp contrast, broad polished faces. Affects: shape, material, contrast, refraction.", PremiumCrystalFactoryPreset.BlueIce, KaelisMenuStyle.Cyan);
            AddPresetCard(panel.Content, "Golden Prism", "Star prism form, warm gold highlights, rich saturation, strong prism split. Affects: shape, bloom, dispersion, saturation.", PremiumCrystalFactoryPreset.GoldenPrism, KaelisMenuStyle.GoldSoft);
            AddPresetCard(panel.Content, "Ruby Night", "Ruby rhombic material, dark luxury background, higher contrast, deep reflection. Affects: shape, material, contrast.", PremiumCrystalFactoryPreset.RubyNight, KaelisMenuStyle.Red);
            AddPresetCard(panel.Content, "Emerald Depth", "Crystal lens emerald/cyan optics profile, deeper optical mass, internal reflection. Affects: shape, optics, background.", PremiumCrystalFactoryPreset.EmeraldDepth, KaelisMenuStyle.Cyan);
            AddPresetCard(panel.Content, "Opal Dream", "Dodecahedron crystal with opal spectral split and soft prism fire. Affects: shape, spectral split, contrast.", PremiumCrystalFactoryPreset.OpalDream, KaelisMenuStyle.Cyan);
            AddPresetCard(panel.Content, "Cosmic Glass", "Icosahedron prism, high saturation, wide spectral edges, deep background distortion. Affects: shape, background, rainbow edge.", PremiumCrystalFactoryPreset.CosmicGlass, KaelisMenuStyle.GoldSoft);
            AddPresetCard(panel.Content, "Dark Luxury", "Star prism luxury profile, restrained bloom, deeper contrast, polished material. Affects: shape, contrast, bloom, material.", PremiumCrystalFactoryPreset.DarkLuxury, KaelisMenuStyle.TextMuted);
            AddPresetCard(panel.Content, "Absolute Mirror", "Solid mirror-polished facets, zero transparency, hidden reflection depth, sharp highlights.", PremiumCrystalFactoryPreset.AbsoluteMirror, KaelisMenuStyle.GoldSoft);

            RectTransform actions = CreateActionRow(panel.Content, "PresetActions");
            presetApplyButton = AddActionChip(actions, "ApplyPresetButton", "APPLY PREMIUM LOOK", KaelisMenuPanelCommand.ApplySelectedPreset, KaelisMenuStyle.GoldSoft);
            presetApplyRow = presetApplyButton.GetComponent<KaelisMenuInteractiveRow>();
            SetPresetApplyAvailable(false);
            AddActionChip(actions, "SaveCurrentPresetButton", "SAVE CURRENT", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted, "Available after Preset Persistence service is implemented.");
            AddActionChip(actions, "RenamePresetButton", "RENAME", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted, "Available after Preset Persistence service is implemented.");
            AddActionChip(actions, "DeletePresetButton", "DELETE", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.Red, "Available after Preset Persistence service is implemented.");
            AddActionChip(actions, "ResetFactoryPresetButton", "RESET FACTORY", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.Cyan, "Available after Preset Persistence service is implemented.");

            AddGroupLabel(panel.Content, "EXPERIMENTAL CRYSTAL LAB");
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.Normal, "Normal / Restore Previous", "Restores the user crystal state cached before entering the experimental lab.", KaelisMenuStyle.Cyan);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.AlienArtifactCore, "Alien Artifact Core", "Dense unstable inner crystal with strong internal refraction and visible core formation.", KaelisMenuStyle.GoldSoft);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.PredatorCrystal, "Predator Crystal", "High contrast green-yellow spectral refraction with alien-tech edge behavior.", KaelisMenuStyle.Cyan);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.SingularityPrism, "Singularity Prism", "Pulls the background into a dark central prism with heavy bending.", KaelisMenuStyle.TextMuted);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.RecursiveEye, "Recursive Eye", "Mandala recursion and inner tunnel energy inside the crystal body.", KaelisMenuStyle.Cyan);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.GhostDiamond, "Ghost Diamond", "Almost transparent body with rim clarity and subtle internal echoes.", KaelisMenuStyle.Cyan);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.PlasmaLattice, "Plasma Lattice", "Bright internal grid-like energy with high spectral glow.", KaelisMenuStyle.GoldSoft);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.ObsidianCore, "Obsidian Core", "Dark solid gem with low transparency, strong rim light, and deep shadows.", KaelisMenuStyle.TextMuted);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.BrokenFacetStorm, "Broken Facet Storm", "Aggressive facet distortion with sharp broken highlights and chaotic internal returns.", KaelisMenuStyle.Red);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.LiquidGlass, "Liquid Glass", "Smooth flowing refraction, softer edges, and liquid optical mass.", KaelisMenuStyle.Cyan);
            AddExperimentalPresetCard(panel.Content, CrystalExperimentPresetType.CelestialReactor, "Celestial Reactor", "Bright core bloom with controlled caustic-like reflections and spectral heat.", KaelisMenuStyle.GoldSoft);
        }

        private void BuildSettingsPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Settings, "SettingsSectionPanel", "SETTINGS", "Application and system controls");

            AddGroupLabel(panel.Content, "DISPLAY");
            AddCommandRow(panel.Content, "Resolution", "Selects a supported screen resolution.", "SERVICE MISSING", "Screen.resolutions", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.Cyan, 48f, null, "Available after Display Settings service is implemented.");
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Fullscreen", "Switches between fullscreen and windowed display.", false, true, "Available after Display Settings service is implemented.");
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "VSync", "Controls vertical sync.", false, true, "Available after Performance Settings service is implemented.");
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Target FPS", "Selects the desired target frame rate.", 0f, 7f, 1f, string.Empty, true, new[] { "30", "60", "90", "120", "144", "165", "240", "Unlimited" }, "Available after Performance Settings service is implemented.");
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "UI Scale", "Adjusts menu and interface scale.", 70f, 160f, 100f, "%", true, "Available after Menu UI Scale service is implemented.");

            AddGroupLabel(panel.Content, "AUDIO");
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Master Volume", "Controls total application output volume.", 0f, 100f, 100f, "%", true, "Available after Audio Settings service is implemented.");
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Menu Volume", "Controls KAELIS menu ambience and interface sound volume.", 0f, 100f, 80f, "%", true, "Available after Menu Audio Settings service is implemented.");
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Demo Volume", "Controls curated demo and Meditation playlist volume.", 0f, 100f, 75f, "%", true, "Available after Session Audio service is implemented.");
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Mute", "Silences application audio.", false, true, "Available after Audio Settings service is implemented.");

            AddGroupLabel(panel.Content, "CONTROLS");
            KaelisMenuToggleControl wheelScaleToggle = KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Mouse Wheel Crystal Scale", "Enables mouse wheel scaling for the Classic2D crystal overlay and Premium3D crystal size.", true, KaelisMenuBindingStatus.RealBinding);
            wheelScaleToggle.SetChangedHandler(value =>
            {
                if (premiumWheelScaleEnabledHandler != null)
                {
                    premiumWheelScaleEnabledHandler(value);
                }
            });
            KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Mouse Sensitivity", "Controls pointer and camera sensitivity for runtime interactions.", 0.10f, 5.00f, 1.00f, string.Empty, true, "Available after Input Settings service is implemented.");
            KaelisMenuSliderControl scaleStepSlider = KaelisMenuSliderControl.Create(panel.Content, assets, tooltip, "Crystal Scale Step", "Adjusts per-wheel Classic2D crystal overlay and Premium3D crystal scale changes.", 1f, 50f, 10f, "%", KaelisMenuBindingStatus.RealBinding);
            scaleStepSlider.SetChangedHandler(value =>
            {
                if (premiumWheelScaleStepHandler != null)
                {
                    premiumWheelScaleStepHandler(value);
                }
            });
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Invert Zoom", "Reverses local zoom direction.", false, true, "Available after Input Settings service is implemented.");
            AddCommandRow(panel.Content, "Hotkeys", "Opens a future dedicated hotkey panel.", "SERVICE MISSING", "Hotkey panel", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted, 48f, null, "Available after Binding Settings service is implemented.");
            AddCommandRow(panel.Content, "Reset Hotkeys", "Restores future user key bindings to defaults.", "SERVICE MISSING", "Reset hotkeys", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted, 48f, null, "Available after Binding Settings service is implemented.");

            AddGroupLabel(panel.Content, "SYSTEM");
            AddLanguageRow(panel.Content);
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Start With Menu", "Controls whether KAELIS starts with this premium menu shell.", true, true, "Available after Startup Preferences service is implemented.");
            autoSaveSettingsToggle = KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Auto Save Settings", "Persists supported visual, crystal, control, startup, and menu-language preferences when they change.", false, KaelisMenuBindingStatus.RealBinding);
            autoSaveSettingsToggle.SetChangedHandler(OnAutoSaveSettingsChanged);
            AddCommandRow(panel.Content, "Reset Settings", "Confirmation required before restoring defaults. Settings persistence service is available; reset confirmation will be connected later.", "CONFIRMATION MISSING", "Reset settings", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.Red, 48f, null, "Settings persistence service is available. Reset remains disabled until a confirmation flow exists.");
            AddCommandRow(panel.Content, "Open Logs Folder", "Opens the runtime log folder through a safe platform shell hook.", "SERVICE MISSING", "Open logs", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted, 48f, null, "Available after Platform Shell service is implemented.");

            AddGroupLabel(panel.Content, "DIAGNOSTICS");
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Show FPS", "Shows a lightweight FPS overlay.", false, true, "Available after Diagnostics HUD service is implemented.");
            AddCommandRow(panel.Content, "Show Diagnostics", "Uses existing public diagnostics visibility command.", "SAFE COMMAND", "SetDiagnosticsVisible(true)", KaelisMenuPanelCommand.ShowDiagnostics, KaelisMenuStyle.GoldSoft, 48f);
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Show Input Overlay", "Displays input visualization.", false, true, "Available after Diagnostics HUD service is implemented.");
            KaelisMenuToggleControl.Create(panel.Content, assets, tooltip, "Show Render Stats", "Displays render statistics.", false, true, "Available after Diagnostics HUD service is implemented.");
        }

        private void BuildExitPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Exit, "ExitSectionPanel", "EXIT KAELIS?", "Confirm application exit");

            TMP_Text body = KaelisMenuUiPrimitives.CreateText(panel.Content, "ExitBody", "Close the KAELIS experience shell. Settings persistence is available; save-and-exit remains unavailable until its confirmation flow is connected.", 18f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Status));
            body.enableWordWrapping = true;
            body.characterSpacing = 2f;
            KaelisMenuUiPrimitives.AddLayout(body.gameObject, -1f, 120f);

            RectTransform actions = CreateActionRow(panel.Content, "ExitActions");
            AddActionChip(actions, "ExitSaveAndQuitButton", "SAVE UNAVAILABLE", KaelisMenuPanelCommand.ReservedAction, KaelisMenuStyle.TextMuted, "Settings persistence service available. Individual settings will be connected in later stages.");
            AddActionChip(actions, "ExitWithoutSavingButton", "EXIT WITHOUT SAVING", KaelisMenuPanelCommand.ExitWithoutSaving, KaelisMenuStyle.Red);
            AddActionChip(actions, "ExitCancelButton", "CANCEL", KaelisMenuPanelCommand.CancelExit, KaelisMenuStyle.Cyan);
        }

        private void BuildAboutPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.About, "AboutSectionPanel", "ABOUT", "Credits, author, music, and license");
            if (KaelisAboutContent.TryLoad(out KaelisAboutContent content, out string error))
            {
                new KaelisAboutScreenView(assets).Build(panel.Content, content);
                return;
            }

            AddStatusLine(panel.Content, "AboutPackageMissing", "About package unavailable: " + error, KaelisMenuStyle.Red, 92f);
            AddStatusLine(panel.Content, "AboutPackageInstruction", "Unpack the prepared About package into Assets/_Project/Kaleidoscope2/About. Placeholder author, license, or music text is intentionally not generated.", KaelisMenuStyle.TextSecondary, 86f);
        }

        private KaelisMenuSectionPanel CreatePanel(KaelisMenuSection section, string name, string title, string subtitle)
        {
            bool isAbout = section == KaelisMenuSection.About;
            RectTransform panel = KaelisMenuUiPrimitives.CreateRect(name, Root);
            KaelisMenuUiPrimitives.Stretch(panel);
            KaelisMenuUiPrimitives.AddImage(panel, assets.SolidSprite, KaelisMenuStyle.SectionPanelGlass, false);

            RectTransform haze = KaelisMenuUiPrimitives.CreateRect("PanelHaze", panel);
            KaelisMenuUiPrimitives.Stretch(haze);
            KaelisMenuUiPrimitives.AddImage(haze, assets.SolidSprite, KaelisMenuStyle.SectionPanelHaze, false);

            RectTransform topGlow = KaelisMenuUiPrimitives.CreateRect("PanelTopGlow", panel);
            topGlow.anchorMin = new Vector2(0f, 0.74f);
            topGlow.anchorMax = new Vector2(1f, 1f);
            topGlow.offsetMin = Vector2.zero;
            topGlow.offsetMax = Vector2.zero;
            KaelisMenuUiPrimitives.AddImage(topGlow, assets.SolidSprite, KaelisMenuStyle.SectionPanelTopGlow, false);

            KaelisMenuUiPrimitives.AddFrame(panel, KaelisMenuStyle.SectionFrameOuter, KaelisMenuStyle.SectionFrameInner, 1.05f, assets.SolidSprite);
            KaelisMenuUiPrimitives.AddCornerCuts(panel, KaelisMenuStyle.SectionCorner, 32f, 1.15f, assets.SolidSprite);

            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();

            RectTransform header = KaelisMenuUiPrimitives.CreateRect("SectionHeader", panel);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, KaelisMenuStyle.SectionHeaderHeight);
            header.anchoredPosition = Vector2.zero;

            TMP_FontAsset titleFont = assets.GetFont(KaelisMenuFontRole.Button);
            if (isAbout && assets.InterSemiBold != null)
            {
                titleFont = assets.InterSemiBold;
            }
            else if (RequiresUnicodeSafeFont(title))
            {
                titleFont = assets.InterSemiBold != null
                    ? assets.InterSemiBold
                    : (assets.InterMedium != null ? assets.InterMedium : (assets.InterRegular != null ? assets.InterRegular : titleFont));
            }

            TMP_Text titleText = KaelisMenuUiPrimitives.CreateText(header, "SectionTitle", title, 28f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Left, titleFont);

            titleText.characterSpacing = 8f;
            titleText.enableWordWrapping = false;
            KaelisMenuLocalizationService.SetText(titleText, title);
            RectTransform titleRect = (RectTransform)titleText.transform;
            titleRect.offsetMin = new Vector2(34f, 35f);
            titleRect.offsetMax = new Vector2(-190f, -8f);

            TMP_Text subtitleText = KaelisMenuUiPrimitives.CreateText(header, "SectionSubtitle", subtitle, 13f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            subtitleText.characterSpacing = 4f;
            subtitleText.enableWordWrapping = true;
            subtitleText.overflowMode = TextOverflowModes.Ellipsis;
            KaelisMenuLocalizationService.SetText(subtitleText, subtitle);
            RectTransform subtitleRect = (RectTransform)subtitleText.transform;
            subtitleRect.offsetMin = new Vector2(36f, 12f);
            subtitleRect.offsetMax = new Vector2(-190f, -56f);

            AddSectionHeaderDivider(header, isAbout ? "AboutHeaderSubtitleDivider" : "SectionHeaderSubtitleDivider");

            Button sectionCloseButton = AddActionChip(header, "SectionCloseButton", "CLOSE", KaelisMenuPanelCommand.CloseSection, KaelisMenuStyle.Cyan, new Vector2(124f, 42f), new Vector2(1f, 1f), new Vector2(-92f, -38f));
            if (isAbout)
            {
                IntegrateAboutCloseButton(sectionCloseButton);
            }

            RectTransform headerSafeZone = KaelisMenuUiPrimitives.CreateRect("HeaderSafeZone", panel);
            headerSafeZone.anchorMin = new Vector2(0f, 1f);
            headerSafeZone.anchorMax = new Vector2(1f, 1f);
            headerSafeZone.pivot = new Vector2(0.5f, 1f);
            headerSafeZone.sizeDelta = new Vector2(0f, KaelisMenuStyle.SectionHeaderSafeGap);
            headerSafeZone.anchoredPosition = new Vector2(0f, -KaelisMenuStyle.SectionHeaderHeight);

            RectTransform viewport = KaelisMenuUiPrimitives.CreateRect("SectionViewport", panel);
            KaelisMenuUiPrimitives.Stretch(viewport);
            viewport.offsetMin = KaelisMenuStyle.SectionViewportOffsetMin;
            viewport.offsetMax = KaelisMenuStyle.SectionViewportOffsetMax;
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform scrollHitArea = KaelisMenuUiPrimitives.CreateRect("SectionScrollHitArea", viewport);
            KaelisMenuUiPrimitives.Stretch(scrollHitArea);
            KaelisMenuUiPrimitives.AddImage(scrollHitArea, assets.SolidSprite, new Color(1f, 1f, 1f, 0f), true);

            RectTransform content = KaelisMenuUiPrimitives.CreateRect("SectionContent", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = KaelisMenuStyle.SectionContentPadding;
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

            AddSectionScrollbar(panel, scrollRect);

            header.SetAsLastSibling();
            Transform closeButton = header.Find("SectionCloseButton");
            if (closeButton != null)
            {
                closeButton.SetAsLastSibling();
            }

            KaelisMenuSectionPanel sectionPanel = new KaelisMenuSectionPanel(panel, content, group);
            sectionPanel.ApplySharedLayout("KaelisMenuSectionController.CreatePanel");
            geometryDiagnostics.Register(section.ToString(), panel);
            panels.Add(section, sectionPanel);
            return sectionPanel;
        }

        private void AddSectionHeaderDivider(RectTransform header, string name)
        {
            RectTransform divider = KaelisMenuUiPrimitives.CreateRect(name, header);
            divider.anchorMin = new Vector2(0f, 0f);
            divider.anchorMax = new Vector2(1f, 0f);
            divider.pivot = new Vector2(0.5f, 0f);
            divider.offsetMin = new Vector2(36f, 6f);
            divider.offsetMax = new Vector2(-170f, 7.2f);
            KaelisMenuUiPrimitives.AddImage(divider, assets.SolidSprite, KaelisMenuStyle.SectionHeaderDivider, false);
        }

        private void IntegrateAboutCloseButton(Button closeButton)
        {
            if (closeButton == null)
            {
                return;
            }

            RectTransform closeRect = closeButton.transform as RectTransform;
            if (closeRect != null)
            {
                closeRect.sizeDelta = new Vector2(118f, 38f);
                closeRect.anchoredPosition = new Vector2(-88f, -36f);
            }

            Image surface = closeButton.targetGraphic as Image;
            if (surface == null)
            {
                surface = closeButton.GetComponent<Image>();
            }

            if (surface != null)
            {
                surface.color = new Color(0.010f, 0.060f, 0.080f, 0.34f);
                surface.raycastTarget = true;
            }

            TMP_Text label = closeButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.font = assets.InterMedium != null ? assets.InterMedium : (assets.InterSemiBold != null ? assets.InterSemiBold : label.font);
                label.characterSpacing = 2.2f;
                label.fontSizeMax = 12f;
            }
        }

        private void AddSectionScrollbar(RectTransform panel, ScrollRect scrollRect)
        {
            RectTransform scrollbarRoot = KaelisMenuUiPrimitives.CreateRect("VerticalScrollbar", panel);
            scrollbarRoot.anchorMin = new Vector2(1f, 0f);
            scrollbarRoot.anchorMax = new Vector2(1f, 1f);
            scrollbarRoot.pivot = new Vector2(1f, 0.5f);
            scrollbarRoot.offsetMin = new Vector2(-22f, 44f);
            scrollbarRoot.offsetMax = new Vector2(-10f, KaelisMenuStyle.SectionViewportOffsetMax.y);
            KaelisMenuUiPrimitives.AddImage(scrollbarRoot, assets.SolidSprite, new Color(0.02f, 0.10f, 0.12f, 0.70f), true);

            Scrollbar scrollbar = scrollbarRoot.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            RectTransform slidingArea = KaelisMenuUiPrimitives.CreateRect("SlidingArea", scrollbarRoot);
            KaelisMenuUiPrimitives.Stretch(slidingArea);
            slidingArea.offsetMin = new Vector2(2f, 2f);
            slidingArea.offsetMax = new Vector2(-2f, -2f);

            RectTransform handle = KaelisMenuUiPrimitives.CreateRect("Handle", slidingArea);
            KaelisMenuUiPrimitives.Stretch(handle);
            Image handleImage = KaelisMenuUiPrimitives.AddImage(handle, assets.SolidSprite, new Color(0.55f, 0.95f, 1f, 0.62f), true);
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;

            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollRect.verticalScrollbarSpacing = 8f;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void LogAboutCloseButtonRaycastDiagnostic()
        {
            if (!panels.TryGetValue(KaelisMenuSection.About, out KaelisMenuSectionPanel panel) || panel == null)
            {
                return;
            }

            Canvas canvas = Root != null ? Root.GetComponentInParent<Canvas>() : null;
            GraphicRaycaster raycaster = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;
            if (raycaster == null || EventSystem.current == null)
            {
                Debug.LogWarning("[KAELIS Menu Raycast] About close button diagnostic skipped: GraphicRaycaster or EventSystem missing.");
                return;
            }

            Transform closeTransform = FindDescendantByName(panel.Root, "SectionCloseButton");
            RectTransform closeRect = closeTransform as RectTransform;
            if (closeRect == null)
            {
                Debug.LogWarning("[KAELIS Menu Raycast] About close button diagnostic skipped: SectionCloseButton missing.");
                return;
            }

            Canvas.ForceUpdateCanvases();
            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, closeRect.TransformPoint(closeRect.rect.center));
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPoint
            };

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);
            string report = BuildRaycastReport(results);
            bool closeButtonIsFirst = results.Count > 0
                && (results[0].gameObject.transform == closeTransform || results[0].gameObject.transform.IsChildOf(closeTransform));
            string message = "[KAELIS Menu Raycast] About close button center hits:\n" + report;
            if (closeButtonIsFirst)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
        }

        private static Transform FindDescendantByName(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < children.Length; index++)
            {
                if (children[index].name == name)
                {
                    return children[index];
                }
            }

            return null;
        }

        private static string BuildRaycastReport(List<RaycastResult> results)
        {
            if (results == null || results.Count == 0)
            {
                return "(no hits)";
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int index = 0; index < results.Count; index++)
            {
                GameObject hitObject = results[index].gameObject;
                Graphic graphic = hitObject != null ? hitObject.GetComponent<Graphic>() : null;
                CanvasGroup group = hitObject != null ? hitObject.GetComponentInParent<CanvasGroup>() : null;
                builder.Append(index.ToString("00"));
                builder.Append(": ");
                builder.Append(GetHierarchyPath(hitObject != null ? hitObject.transform : null));
                builder.Append(" | raycastTarget=");
                builder.Append(graphic != null ? graphic.raycastTarget.ToString() : "n/a");
                builder.Append(" | CanvasGroup=");
                builder.Append(group != null
                    ? ("alpha " + group.alpha.ToString("0.00") + ", interactable " + group.interactable + ", blocks " + group.blocksRaycasts)
                    : "none");
                builder.Append(" | active=");
                builder.Append(hitObject != null && hitObject.activeInHierarchy);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static string GetHierarchyPath(Transform target)
        {
            if (target == null)
            {
                return "(null)";
            }

            string path = target.name;
            Transform current = target.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
#endif

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
            MenuAudioFeedbackController.BindButton(button);
        }

        private void AddPremiumShapeRow(RectTransform parent, PremiumCrystalShapeType shape, string title, string description, Color accent)
        {
            string commandId = "SetPremiumCrystalShape(" + shape + ")";
            KaelisMenuInteractiveRow row = AddCommandRow(parent, title, description, "REAL FORM", commandId, KaelisMenuPanelCommand.SelectPremiumCrystalShape, accent, 56f, KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Geometry));
            Button button = row.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            button.onClick.AddListener(() =>
            {
                SelectPremiumShapeRow(row);
                row.Flash();
                DispatchPremiumShape(shape);
            });
            MenuAudioFeedbackController.BindButton(button);
        }

        private void BuildDemoPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Demo, "DemoSectionPanel", "DEMO", "Restorable comfort and transparent demonstrations");
            AddGroupLabel(panel.Content, "COMFORT");
            AddCommandRow(panel.Content, "Meditation Mode", "Open the comfort-session preview and START control. No visual or audio state changes until START is pressed.", "OPEN PANEL", "OpenMeditationPanel", KaelisMenuPanelCommand.OpenMeditationPanel, KaelisMenuStyle.Cyan, 70f);
            AddGroupLabel(panel.Content, "DEMONSTRATIONS");
            AddCommandRow(panel.Content, "Replay Demo", "Open the replay preview and START control. Playback begins only if semantic visual actions have been recorded.", "OPEN PANEL", "OpenReplayPanel", KaelisMenuPanelCommand.OpenReplayPanel, KaelisMenuStyle.GoldSoft, 70f);
            AddCommandRow(panel.Content, "Benchmark Demo", "Open the visual-performance setup screen before launching the curated silent 60-second showcase.", "OPEN PANEL", "OpenBenchmarkPanel", KaelisMenuPanelCommand.OpenBenchmarkPanel, KaelisMenuStyle.GoldSoft, 70f);
            AddCommandRow(panel.Content, "Stop Active Session", "Stops Meditation, Replay, or Benchmark and restores the full captured runtime state. Escape or middle mouse does the same.", "SAFE EXIT", "CancelTemporarySession", KaelisMenuPanelCommand.CancelTemporarySession, KaelisMenuStyle.Red, 70f);
            demoSaveBenchmarkResultRow = AddCommandRow(panel.Content, "Save Benchmark Result", "Available after a benchmark completes. Writes the most recent benchmark summary as timestamped JSON under persistent Benchmarks storage.", "NO RESULT YET", "SaveBenchmarkResult", KaelisMenuPanelCommand.SaveBenchmarkResult, KaelisMenuStyle.Cyan, 70f);
            demoSaveBenchmarkResultButton = demoSaveBenchmarkResultRow.GetComponent<Button>();
            AddStatusLine(panel.Content, "DemoSafetyNote", "Comfort note: avoid prolonged central fixation; all temporary modes expose an immediate restore exit.", KaelisMenuStyle.TextSecondary, 44f);
        }

        private void BuildMeditationPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.MeditationSetup, "MeditationSetupSectionPanel", "MEDITATION MODE", "Comfort Visual Session");
            AddGroupLabel(panel.Content, "PREVIEW");
            AddStatusLine(panel.Content, "MeditationIntroduction", "A curated image-and-playlist session with gentle semantic motion, minute direction reversal, and six child crystals that detach, orbit, and reform.", KaelisMenuStyle.TextSecondary, 82f);
            AddStatusLine(panel.Content, "MeditationSafetyDetails", "Motion is comfort-capped at 0.25-1.5 rotations/sec. Runtime frames hide during the visual session; H toggles clean view; Escape or middle mouse restores your prior state.", KaelisMenuStyle.TextSecondary, 70f);
            AddCommandRow(panel.Content, "Start Meditation Mode", "Capture the current state, then begin curated content and comfort-safe presentation.", "START", "SetMeditationModeEnabled(true)", KaelisMenuPanelCommand.StartMeditationMode, KaelisMenuStyle.Cyan, 84f);
        }

        private void BuildReplayPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.ReplaySetup, "ReplaySetupSectionPanel", "REPLAY DEMO", "Semantic Performance Playback");
            AddGroupLabel(panel.Content, "PREVIEW");
            AddStatusLine(panel.Content, "ReplayIntroduction", "Replay turns your recent visual control gestures into a looping curated presentation through the same semantic command route.", KaelisMenuStyle.TextSecondary, 82f);
            AddStatusLine(panel.Content, "ReplayAvailabilityDetails", "START requires at least one recorded visual control action. With none available, Replay remains stopped and reports that truthfully.", KaelisMenuStyle.TextSecondary, 70f);
            AddCommandRow(panel.Content, "Start Replay Demo", "Capture the current state, switch to curated images, and hide runtime frames during playback.", "START", "StartReplayDemo", KaelisMenuPanelCommand.StartReplayDemo, KaelisMenuStyle.GoldSoft, 84f);
        }

        private void BuildBenchmarkPanel()
        {
            KaelisMenuSectionPanel panel = CreatePanel(KaelisMenuSection.Benchmark, "BenchmarkSectionPanel", "BENCHMARK DEMO", "Visual Performance Mode");
            AddGroupLabel(panel.Content, "CINEMATIC PERFORMANCE SHOWCASE");
            AddStatusLine(panel.Content, "BenchmarkIntroduction", "A silent curated 60-second presentation of KAELIS modes and safe optical effects. Runtime controls hide while the visual performance is measured.", KaelisMenuStyle.TextSecondary, 74f);
            AddStatusLine(panel.Content, "BenchmarkHudDetails", "Live HUD: elapsed time, phase, current FPS, average FPS, peak FPS, and 1% low FPS. H toggles the live HUD; Escape or middle mouse restores the previous state.", KaelisMenuStyle.TextSecondary, 70f);
            AddStatusLine(panel.Content, "BenchmarkSafetyDetails", "No aggressive debug artifacts. Results appear only after complete snapshot restoration.", KaelisMenuStyle.TextSecondary, 48f);
            AddCommandRow(panel.Content, "Start Visual Performance Benchmark", "Begin the curated silent showcase now. Current settings are captured before any temporary visual changes.", "START", "StartBenchmarkDemo", KaelisMenuPanelCommand.StartBenchmarkDemo, KaelisMenuStyle.GoldSoft, 84f);
            benchmarkSaveResultRow = AddCommandRow(panel.Content, "Save Last Benchmark Result", "Available after a benchmark completes. Writes the most recent completed benchmark result to persistent Benchmarks storage.", "NO RESULT YET", "SaveBenchmarkResult", KaelisMenuPanelCommand.SaveBenchmarkResult, KaelisMenuStyle.Cyan, 62f);
            benchmarkSaveResultButton = benchmarkSaveResultRow.GetComponent<Button>();
        }

        private void AddPremiumOpticalModeRow(RectTransform parent, PremiumCrystalOpticalMode mode, string title, string description, Color accent)
        {
            string commandId = "SetPremiumCrystalOpticalMode(" + mode + ")";
            string badge = mode == PremiumCrystalOpticalMode.AlienArtifactExperimental ? "EXPLICIT EXPERIMENT" : "REAL OPTIC";
            KaelisMenuInteractiveRow row = AddCommandRow(parent, title, description, badge, commandId, KaelisMenuPanelCommand.SelectPremiumCrystalOpticalMode, accent, 56f, PremiumCrystalOpticalModeLibrary.GetTooltip(mode));
            Button button = row.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            button.onClick.AddListener(() =>
            {
                row.Flash();
                if (premiumOpticalModeHandler != null)
                {
                    premiumOpticalModeHandler(mode);
                }
            });
            MenuAudioFeedbackController.BindButton(button);
        }

        private void AddPresetCard(RectTransform parent, string title, string description, PremiumCrystalFactoryPreset preset, Color accent)
        {
            KaelisMenuInteractiveRow row = AddCommandRow(parent, title, description, "SELECT", "SelectPremiumCrystalPreset(" + preset + ")", KaelisMenuPanelCommand.SelectFactoryPresetCard, accent, 62f);
            Button button = row.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            button.onClick.AddListener(() =>
            {
                SelectPresetRow(row, preset);
                row.Flash();
            });
            MenuAudioFeedbackController.BindButton(button);
        }

        private void AddExperimentalPresetCard(RectTransform parent, CrystalExperimentPresetType preset, string title, string description, Color accent)
        {
            KaelisMenuInteractiveRow row = AddCommandRow(parent, title, description, preset == CrystalExperimentPresetType.Normal ? "RESTORE" : "EXPERIMENT", "ApplyExperimentalCrystalPreset(" + preset + ")", KaelisMenuPanelCommand.ApplyExperimentalPresetCard, accent, 62f);
            Button button = row.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            button.onClick.AddListener(() =>
            {
                row.Flash();
                if (experimentalCrystalPresetHandler != null)
                {
                    experimentalCrystalPresetHandler(preset);
                }
            });
            MenuAudioFeedbackController.BindButton(button);
        }

        private void AddLanguageRow(RectTransform parent)
        {
            KaelisMenuInteractiveRow row = AddCommandRow(parent, "Language", "Changes the KAELIS menu language immediately and stores it for the next session.", KaelisMenuLocalizationService.GetCurrentLanguageDisplayName(), "English / Русский / Deutsch / Українська", KaelisMenuPanelCommand.CycleLanguage, KaelisMenuStyle.Cyan, 56f, KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Language));
            Button button = row.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.interactable = true;

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
            MenuAudioFeedbackController.BindButton(button);
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

        private void DispatchPremiumShape(PremiumCrystalShapeType shape)
        {
            if (premiumShapeHandler != null)
            {
                premiumShapeHandler(shape);
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

        public void SetBenchmarkResultAvailable(bool available)
        {
            SetContextualButtonAvailability(
                demoSaveBenchmarkResultButton,
                demoSaveBenchmarkResultRow,
                available,
                available ? "Benchmark result ready to save." : "Complete a benchmark before saving results.");
            SetContextualButtonAvailability(
                benchmarkSaveResultButton,
                benchmarkSaveResultRow,
                available,
                available ? "Benchmark result ready to save." : "Complete a benchmark before saving results.");
        }

        public void SetSettingsAutoSaveState(bool enabled)
        {
            if (SettingsOptions != null)
            {
                SettingsOptions.AutoSaveSettings = enabled;
            }

            if (autoSaveSettingsToggle != null)
            {
                autoSaveSettingsToggle.SetValueWithoutNotify(enabled);
                autoSaveSettingsToggle.SetTooltipCurrent(enabled ? "AUTO SAVE ON" : "AUTO SAVE OFF");
            }
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

            return ProductionOptions.RecordingBackendAvailable ? "RECORDING READY" : "SHOWCASE RECORDING SERVICE MISSING";
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

        private void OnAutoSaveSettingsChanged(bool enabled)
        {
            if (SettingsOptions != null)
            {
                SettingsOptions.AutoSaveSettings = enabled;
            }

            Submit(KaelisMenuPanelCommand.SetAutoSaveSettings);
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

            bool recordingBackendAvailable = ProductionOptions.RecordingBackendAvailable;
            SetButtonAvailability(selectRecordingOutputFolderButton, selectRecordingOutputFolderRow, recordingBackendAvailable);
            SetButtonAvailability(clearRecordingOutputFolderButton, clearRecordingOutputFolderRow, recordingBackendAvailable);

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
                KaelisMenuLocalizationService.SetText(recordingHotkeyText, recordingBackendAvailable
                    ? "Ctrl + Shift + R = Start / Stop recording"
                    : "Recording hotkey unavailable until Showcase Recording service is implemented.");
            }
        }

        private static void SetButtonAvailability(Button button, KaelisMenuInteractiveRow row, bool available)
        {
            if (button != null)
            {
                button.interactable = available;
            }

            if (row != null)
            {
                row.SetSelectableVisual(available);
                if (!available)
                {
                    row.SetTooltipRange("Available after Showcase Recording service is implemented.");
                    row.SetTooltipCurrent("Available after Showcase Recording service is implemented.");
                }
                else
                {
                    row.SetTooltipRange("Status: SAFE COMMAND");
                    row.SetTooltipCurrent("Command available.");
                }
            }
        }

        private static void SetContextualButtonAvailability(Button button, KaelisMenuInteractiveRow row, bool available, string current)
        {
            if (button != null)
            {
                button.interactable = available;
            }

            if (row != null)
            {
                row.SetSelectableVisual(available);
                row.SetTooltipRange(available ? "Status: RESULT ACTION" : "Complete a benchmark before saving results.");
                row.SetTooltipCurrent(current);
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

            return ProductionOptions.RecordingBackendAvailable
                ? "Recording: Ready"
                : "Recording: Showcase Recording service missing";
        }

        private static bool IsSecondDisplayAvailable()
        {
            return Display.displays != null && Display.displays.Length > 1;
        }

        private static bool RequiresUnicodeSafeFont(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                if (value[index] > 0x007F)
                {
                    return true;
                }
            }

            return false;
        }

        private KaelisMenuInteractiveRow AddCommandRow(RectTransform parent, string title, string description, string badge, string commandId, KaelisMenuPanelCommand command, Color accent, float height, string hint = null, string unavailableReason = null)
        {
            RectTransform row = KaelisMenuUiPrimitives.CreateRect(ToObjectName(title) + "Command", parent);
            KaelisMenuUiPrimitives.AddLayout(row.gameObject, -1f, height);
            Image surface = KaelisMenuUiPrimitives.AddImage(row, assets.SolidSprite, new Color(0.010f, 0.060f, 0.080f, 0.58f), true);
            KaelisMenuUiPrimitives.AddFrame(row, new Color(accent.r, accent.g, accent.b, 0.24f), new Color(0.16f, 0.55f, 0.64f, 0.16f), 0.8f, assets.SolidSprite);
            KaelisMenuSliderControl.AddWideHighlight(row, assets, out CanvasGroup highlightGroup, out CanvasGroup flashGroup);

            KaelisMenuInteractiveRow interactive = row.gameObject.AddComponent<KaelisMenuInteractiveRow>();
            bool unavailable = command == KaelisMenuPanelCommand.ReservedAction && !string.IsNullOrWhiteSpace(unavailableReason);
            string statusLine = unavailable ? unavailableReason : "Status: " + badge;
            string inputHint = hint ?? KaelisMenuInputHintProvider.Get(command == KaelisMenuPanelCommand.ReservedAction ? KaelisMenuInputHintKind.Reserved : KaelisMenuInputHintKind.Select);
            interactive.Configure(surface, highlightGroup, flashGroup, tooltip, title, description, statusLine, "Command: " + commandId, inputHint);

            Button button = row.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = surface;
            button.interactable = command != KaelisMenuPanelCommand.ReservedAction;
            button.onClick.AddListener(() =>
            {
                interactive.Flash();
                Submit(command);
            });
            MenuAudioFeedbackController.BindButton(button);

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

        private Button AddActionChip(RectTransform parent, string name, string label, KaelisMenuPanelCommand command, Color accent, string unavailableReason = null)
        {
            RectTransform chip = KaelisMenuUiPrimitives.CreateRect(name, parent);
            KaelisMenuUiPrimitives.AddLayout(chip.gameObject, -1f, 54f);
            return BuildActionChip(chip, label, command, accent, unavailableReason);
        }

        private Button AddActionChip(RectTransform parent, string name, string label, KaelisMenuPanelCommand command, Color accent, Vector2 size, Vector2 anchor, Vector2 anchoredPosition, string unavailableReason = null)
        {
            RectTransform chip = KaelisMenuUiPrimitives.CreateRect(name, parent);
            chip.anchorMin = anchor;
            chip.anchorMax = anchor;
            chip.pivot = new Vector2(0.5f, 0.5f);
            chip.sizeDelta = size;
            chip.anchoredPosition = anchoredPosition;
            return BuildActionChip(chip, label, command, accent, unavailableReason);
        }

        private Button BuildActionChip(RectTransform chip, string label, KaelisMenuPanelCommand command, Color accent, string unavailableReason = null)
        {
            Image surface = KaelisMenuUiPrimitives.AddImage(chip, assets.SolidSprite, new Color(0.010f, 0.060f, 0.080f, 0.70f), true);
            KaelisMenuUiPrimitives.AddFrame(chip, new Color(accent.r, accent.g, accent.b, 0.38f), new Color(accent.r, accent.g, accent.b, 0.18f), 0.95f, assets.SolidSprite);
            KaelisMenuSliderControl.AddWideHighlight(chip, assets, out CanvasGroup highlightGroup, out CanvasGroup flashGroup);

            KaelisMenuInteractiveRow interactive = chip.gameObject.AddComponent<KaelisMenuInteractiveRow>();
            bool unavailable = command == KaelisMenuPanelCommand.ReservedAction && !string.IsNullOrWhiteSpace(unavailableReason);
            string status = unavailable ? unavailableReason : "Status: " + (command == KaelisMenuPanelCommand.ReservedAction ? "RESERVED" : "SAFE COMMAND");
            interactive.Configure(surface, highlightGroup, flashGroup, tooltip, label, "Menu action.", status, "Command: " + command, KaelisMenuInputHintProvider.Get(command == KaelisMenuPanelCommand.ReservedAction ? KaelisMenuInputHintKind.Reserved : KaelisMenuInputHintKind.Action));

            Button button = chip.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = surface;
            button.interactable = command != KaelisMenuPanelCommand.ReservedAction;
            button.onClick.AddListener(() =>
            {
                interactive.Flash();
                Submit(command);
            });
            MenuAudioFeedbackController.BindButton(button);

            TMP_Text text = KaelisMenuUiPrimitives.CreateText(chip, "Label", label, 12.5f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Button));
            text.enableAutoSizing = true;
            text.fontSizeMin = 9f;
            text.fontSizeMax = 12.5f;
            text.characterSpacing = 3f;
            KaelisMenuLocalizationService.SetText(text, label);
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

        private void SelectPremiumShapeRow(KaelisMenuInteractiveRow row)
        {
            if (selectedPremiumShapeRow != null && selectedPremiumShapeRow != row)
            {
                selectedPremiumShapeRow.SetSelected(false);
            }

            selectedPremiumShapeRow = row;
            if (selectedPremiumShapeRow != null)
            {
                selectedPremiumShapeRow.SetSelected(true);
            }
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

            public void ApplySharedLayout(string writer)
            {
                KaelisMenuSectionGeometryDiagnostics.ApplySharedSectionLayout(Root, writer);
            }

            public void SetVisible(bool visible)
            {
                Root.gameObject.SetActive(visible);
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }
    }

    public sealed class KaelisMenuSectionGeometryDiagnostics : MonoBehaviour
    {
        private const float RequiredVisibleGapPixels = 60f;
        private readonly Dictionary<string, RectTransform> registeredSections = new Dictionary<string, RectTransform>();
        private string pendingSectionName;
        private RectTransform pendingSectionPanel;
        private int pendingValidationFrames;

        internal void Register(string sectionName, RectTransform sectionPanel)
        {
            if (string.IsNullOrEmpty(sectionName) || sectionPanel == null)
            {
                return;
            }

            registeredSections[sectionName] = sectionPanel;
        }

        internal void RequestValidation(string sectionName, RectTransform sectionPanel)
        {
            if (sectionPanel == null)
            {
                return;
            }

            pendingSectionName = sectionName;
            pendingSectionPanel = sectionPanel;
            pendingValidationFrames = 2;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ValidateSectionGeometry(sectionPanel, sectionName, "SetActiveSection immediate", true, true);
#endif
        }

        internal static void ApplySharedSectionLayout(RectTransform sectionPanel, string writer)
        {
            if (sectionPanel == null)
            {
                return;
            }

            RectTransform headerSafeZone = FindDescendant<RectTransform>(sectionPanel, "HeaderSafeZone");
            if (headerSafeZone != null)
            {
                headerSafeZone.anchorMin = new Vector2(0f, 1f);
                headerSafeZone.anchorMax = new Vector2(1f, 1f);
                headerSafeZone.pivot = new Vector2(0.5f, 1f);
                headerSafeZone.sizeDelta = new Vector2(0f, KaelisMenuStyle.SectionHeaderSafeGap);
                headerSafeZone.anchoredPosition = new Vector2(0f, -KaelisMenuStyle.SectionHeaderHeight);
            }

            RectTransform viewport = FindDescendant<RectTransform>(sectionPanel, "SectionViewport");
            if (viewport != null)
            {
                KaelisMenuUiPrimitives.Stretch(viewport);
                viewport.offsetMin = KaelisMenuStyle.SectionViewportOffsetMin;
                viewport.offsetMax = KaelisMenuStyle.SectionViewportOffsetMax;
                if (viewport.GetComponent<RectMask2D>() == null)
                {
                    viewport.gameObject.AddComponent<RectMask2D>();
                }

                MarkLayoutOwner(viewport, writer);
            }

            RectTransform content = FindDescendant<RectTransform>(sectionPanel, "SectionContent");
            if (content != null)
            {
                VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
                if (layout != null)
                {
                    layout.padding = KaelisMenuStyle.SectionContentPadding;
                }
            }

            RectTransform scrollbar = FindDescendant<RectTransform>(sectionPanel, "VerticalScrollbar");
            if (scrollbar != null)
            {
                scrollbar.offsetMax = new Vector2(scrollbar.offsetMax.x, KaelisMenuStyle.SectionViewportOffsetMax.y);
            }
        }

        public static bool ValidateSectionGeometry(RectTransform sectionPanel, string sectionName, string phase, bool logDetails, bool assertOnFailure)
        {
            if (sectionPanel == null)
            {
                return false;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(sectionPanel);
            Canvas.ForceUpdateCanvases();

            RectTransform header = FindDescendant<RectTransform>(sectionPanel, "SectionHeader");
            RectTransform closeButton = FindDescendant<RectTransform>(sectionPanel, "SectionCloseButton");
            RectTransform viewport = FindDescendant<RectTransform>(sectionPanel, "SectionViewport");
            RectTransform content = FindDescendant<RectTransform>(sectionPanel, "SectionContent");
            RectTransform firstContentRow = FindFirstActiveChild(content);
            Rect viewportScreen = GetScreenRect(viewport);
            Rect headerScreen = GetScreenRect(header);
            Rect firstRowScreen = GetScreenRect(firstContentRow);
            RectTransform firstVisibleRow = FindFirstVisibleChild(content, viewportScreen);
            Rect firstVisibleRowScreen = GetScreenRect(firstVisibleRow);

            float viewportGap = headerScreen.yMin - viewportScreen.yMax;
            float firstVisibleGap = firstVisibleRow != null
                ? headerScreen.yMin - Mathf.Min(firstVisibleRowScreen.yMax, viewportScreen.yMax)
                : viewportGap;
            float canvasScale = GetCanvasScaleFactor(sectionPanel);
            float viewportGapReference = viewportGap / canvasScale;
            float firstVisibleGapReference = firstVisibleGap / canvasScale;
            bool viewportOverlapsHeader = Overlaps(viewportScreen, headerScreen);
            bool firstRowOverlapsHeader = firstContentRow != null && Overlaps(firstRowScreen, headerScreen);
            bool firstVisibleRowInsideViewport = firstVisibleRow == null
                || (firstVisibleRow.IsChildOf(viewport) && Overlaps(firstVisibleRowScreen, viewportScreen));
            bool hasViewportMask = viewport != null && viewport.GetComponent<RectMask2D>() != null;
            bool passes = header != null
                && closeButton != null
                && viewport != null
                && content != null
                && hasViewportMask
                && !viewportOverlapsHeader
                && viewportGapReference >= RequiredVisibleGapPixels
                && firstVisibleGapReference >= RequiredVisibleGapPixels
                && firstVisibleRowInsideViewport;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (logDetails || !passes)
            {
                Debug.Log(BuildReport(
                    sectionPanel,
                    sectionName,
                    phase,
                    header,
                    closeButton,
                    viewport,
                    content,
                    firstContentRow,
                    firstVisibleRow,
                    headerScreen,
                    viewportScreen,
                    firstRowScreen,
                    firstVisibleRowScreen,
                    viewportGap,
                    firstVisibleGap,
                    viewportGapReference,
                    firstVisibleGapReference,
                    canvasScale,
                    viewportOverlapsHeader,
                    firstRowOverlapsHeader,
                    hasViewportMask,
                    passes));
            }

            if (!passes)
            {
                string message = "[KAELIS Menu Geometry] " + sectionName + " failed live geometry assertion. First visible row gap: "
                    + firstVisibleGap.ToString("0.0") + " screen px / " + firstVisibleGapReference.ToString("0.0")
                    + " reference px, viewport gap: " + viewportGap.ToString("0.0") + " screen px / "
                    + viewportGapReference.ToString("0.0") + " reference px.";
                Debug.LogError(message);
                if (assertOnFailure)
                {
                    Debug.Assert(false, message);
                }
            }
#endif

            return passes;
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (pendingValidationFrames <= 0 || pendingSectionPanel == null || !pendingSectionPanel.gameObject.activeInHierarchy)
            {
                return;
            }

            ValidateSectionGeometry(pendingSectionPanel, pendingSectionName, "LateUpdate frame " + (3 - pendingValidationFrames).ToString(), pendingValidationFrames == 1, true);
            pendingValidationFrames--;
#endif
        }

        private static void MarkLayoutOwner(RectTransform viewport, string writer)
        {
            if (viewport == null)
            {
                return;
            }

            KaelisMenuViewportLayoutOwner owner = viewport.GetComponent<KaelisMenuViewportLayoutOwner>();
            if (owner == null)
            {
                owner = viewport.gameObject.AddComponent<KaelisMenuViewportLayoutOwner>();
            }

            owner.Mark(writer);
        }

        private static string BuildReport(
            RectTransform sectionPanel,
            string sectionName,
            string phase,
            RectTransform header,
            RectTransform closeButton,
            RectTransform viewport,
            RectTransform content,
            RectTransform firstContentRow,
            RectTransform firstVisibleRow,
            Rect headerScreen,
            Rect viewportScreen,
            Rect firstRowScreen,
            Rect firstVisibleRowScreen,
            float viewportGap,
            float firstVisibleGap,
            float viewportGapReference,
            float firstVisibleGapReference,
            float canvasScale,
            bool viewportOverlapsHeader,
            bool firstRowOverlapsHeader,
            bool hasViewportMask,
            bool passes)
        {
            KaelisMenuViewportLayoutOwner owner = viewport != null ? viewport.GetComponent<KaelisMenuViewportLayoutOwner>() : null;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendLine("[KAELIS Menu Geometry] " + sectionName + " / " + phase + " / " + (passes ? "PASS" : "FAIL"));
            builder.AppendLine("SectionPanel path: " + GetHierarchyPath(sectionPanel));
            builder.AppendLine("Header path: " + GetHierarchyPath(header));
            builder.AppendLine("Header anchoredPosition: " + FormatVector(header != null ? header.anchoredPosition : Vector2.zero));
            builder.AppendLine("Header sizeDelta: " + FormatVector(header != null ? header.sizeDelta : Vector2.zero));
            builder.AppendLine("Header world corners: " + FormatWorldCorners(header));
            builder.AppendLine("Close button path: " + GetHierarchyPath(closeButton));
            builder.AppendLine("Viewport path: " + GetHierarchyPath(viewport));
            builder.AppendLine("Viewport anchorMin/anchorMax: " + FormatVector(viewport != null ? viewport.anchorMin : Vector2.zero) + " / " + FormatVector(viewport != null ? viewport.anchorMax : Vector2.zero));
            builder.AppendLine("Viewport offsetMin/offsetMax: " + FormatVector(viewport != null ? viewport.offsetMin : Vector2.zero) + " / " + FormatVector(viewport != null ? viewport.offsetMax : Vector2.zero));
            builder.AppendLine("Viewport world corners: " + FormatWorldCorners(viewport));
            builder.AppendLine("Viewport screen rect: " + FormatRect(viewportScreen));
            builder.AppendLine("First content row path: " + GetHierarchyPath(firstContentRow));
            builder.AppendLine("First content row world corners: " + FormatWorldCorners(firstContentRow));
            builder.AppendLine("First visible content row path: " + GetHierarchyPath(firstVisibleRow));
            builder.AppendLine("First visible row screen rect: " + FormatRect(firstVisibleRowScreen));
            builder.AppendLine("First row overlaps header world/screen rect: " + firstRowOverlapsHeader);
            builder.AppendLine("Viewport overlaps header world/screen rect: " + viewportOverlapsHeader);
            builder.AppendLine("Canvas scale factor: " + canvasScale.ToString("0.000"));
            builder.AppendLine("Viewport gap below header: " + viewportGap.ToString("0.0") + " screen px / " + viewportGapReference.ToString("0.0") + " reference px");
            builder.AppendLine("First visible row gap below header: " + firstVisibleGap.ToString("0.0") + " screen px / " + firstVisibleGapReference.ToString("0.0") + " reference px");
            builder.AppendLine("Viewport has RectMask2D: " + hasViewportMask);
            builder.AppendLine("Which script last sets viewport offsets: " + (owner != null ? owner.LastWriterReport : "unknown / no KAELIS layout owner marker"));
            return builder.ToString();
        }

        private static RectTransform FindFirstActiveChild(RectTransform content)
        {
            if (content == null)
            {
                return null;
            }

            for (int index = 0; index < content.childCount; index++)
            {
                RectTransform child = content.GetChild(index) as RectTransform;
                if (child != null && child.gameObject.activeInHierarchy)
                {
                    return child;
                }
            }

            return null;
        }

        private static RectTransform FindFirstVisibleChild(RectTransform content, Rect viewportScreen)
        {
            if (content == null)
            {
                return null;
            }

            for (int index = 0; index < content.childCount; index++)
            {
                RectTransform child = content.GetChild(index) as RectTransform;
                if (child == null || !child.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (Overlaps(GetScreenRect(child), viewportScreen))
                {
                    return child;
                }
            }

            return null;
        }

        private static T FindDescendant<T>(Transform root, string name) where T : Component
        {
            if (root == null)
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < children.Length; index++)
            {
                if (children[index].name == name)
                {
                    return children[index].GetComponent<T>();
                }
            }

            return null;
        }

        private static Rect GetScreenRect(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return new Rect(0f, 0f, 0f, 0f);
            }

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector2 first = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            float minX = first.x;
            float maxX = first.x;
            float minY = first.y;
            float maxY = first.y;
            for (int index = 1; index < corners.Length; index++)
            {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, corners[index]);
                minX = Mathf.Min(minX, point.x);
                maxX = Mathf.Max(maxX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxY = Mathf.Max(maxY, point.y);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static float GetCanvasScaleFactor(RectTransform rectTransform)
        {
            Canvas canvas = rectTransform != null ? rectTransform.GetComponentInParent<Canvas>() : null;
            return canvas != null ? Mathf.Max(0.001f, canvas.scaleFactor) : 1f;
        }

        private static bool Overlaps(Rect a, Rect b)
        {
            return a.width > 0f && a.height > 0f && b.width > 0f && b.height > 0f && a.Overlaps(b);
        }

        private static string FormatWorldCorners(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return "(null)";
            }

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return FormatVector(corners[0]) + " | " + FormatVector(corners[1]) + " | " + FormatVector(corners[2]) + " | " + FormatVector(corners[3]);
        }

        private static string FormatRect(Rect rect)
        {
            return "xMin " + rect.xMin.ToString("0.0") + ", yMin " + rect.yMin.ToString("0.0")
                + ", xMax " + rect.xMax.ToString("0.0") + ", yMax " + rect.yMax.ToString("0.0");
        }

        private static string FormatVector(Vector2 value)
        {
            return "(" + value.x.ToString("0.0") + ", " + value.y.ToString("0.0") + ")";
        }

        private static string FormatVector(Vector3 value)
        {
            return "(" + value.x.ToString("0.0") + ", " + value.y.ToString("0.0") + ", " + value.z.ToString("0.0") + ")";
        }

        private static string GetHierarchyPath(Transform target)
        {
            if (target == null)
            {
                return "(null)";
            }

            string path = target.name;
            Transform current = target.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }

    internal sealed class KaelisMenuViewportLayoutOwner : MonoBehaviour
    {
        private string lastWriter = "not set";
        private int lastFrame = -1;
        private float lastTime;

        internal string LastWriterReport
        {
            get
            {
                return lastWriter + " (frame " + lastFrame.ToString() + ", time " + lastTime.ToString("0.000") + ")";
            }
        }

        internal void Mark(string writer)
        {
            lastWriter = string.IsNullOrEmpty(writer) ? "unknown" : writer;
            lastFrame = Time.frameCount;
            lastTime = Application.isPlaying ? Time.unscaledTime : 0f;
        }
    }
}
