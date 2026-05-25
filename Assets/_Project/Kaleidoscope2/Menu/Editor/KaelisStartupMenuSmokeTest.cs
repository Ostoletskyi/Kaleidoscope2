using System;
using System.Reflection;
using Kaleidoscope2.Core;
using Kaleidoscope2.DiamondFocus;
using Kaleidoscope2.DiamondFocus.CrystalStage3D;
using Kaleidoscope2.DiamondFocus.RealMesh;
using Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D;
using Kaleidoscope2.InputSystem;
using Kaleidoscope2.Menu;
using Kaleidoscope2.Menu.FX;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.Editor
{
    public static class KaelisStartupMenuSmokeTest
    {
        private const string MainScenePath = "Assets/_Project/Kaleidoscope2/Scenes/Kaleidoscope2_Main.unity";

        public static void ImportTmpEssentials()
        {
            AssetDatabase.ImportPackage(
                "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage",
                false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KAELIS Menu SmokeTest] TMP Essential Resources imported.");
        }

        public static void Run()
        {
            KaelisMenuLocalizationService.SetLanguage(KaelisMenuLanguage.English, true);
            EditorSceneManager.OpenScene(MainScenePath);

            KaelisStartupMenuController controller = UnityEngine.Object.FindObjectOfType<KaelisStartupMenuController>();
            Require(controller != null, "Main scene must contain KaelisStartupMenuController.");

            InvokePrivate(controller, "Awake");

            Transform canvasTransform = controller.transform.Find("MainMenuCanvas");
            Require(canvasTransform != null, "MainMenuCanvas must be created on Awake.");
            Require(canvasTransform.GetComponent<Canvas>() != null, "MainMenuCanvas must have a Canvas.");
            Require(canvasTransform.GetComponent<GraphicRaycaster>() != null, "MainMenuCanvas must receive UI raycasts.");
            Transform backgroundArtwork = FindChild<Transform>(canvasTransform, "ReferenceAtmosphere");
            Transform beamLayer = FindChild<Transform>(canvasTransform, "PremiumMenuMotionStripes");
            Transform atmosphereLayer = FindChild<Transform>(canvasTransform, "MenuAtmosphereFX");
            Transform safeFrame = FindChild<Transform>(canvasTransform, "SafeFrame");
            Require(backgroundArtwork != null && beamLayer != null && atmosphereLayer != null && safeFrame != null, "Premium background hierarchy layers missing.");
            Require(backgroundArtwork.GetSiblingIndex() < beamLayer.GetSiblingIndex()
                && beamLayer.GetSiblingIndex() < atmosphereLayer.GetSiblingIndex()
                && atmosphereLayer.GetSiblingIndex() < safeFrame.GetSiblingIndex(),
                "Premium beams and prism must render above background artwork and behind menu panels.");
            Require(FindChild<Image>(canvasTransform, "LightBandCausticOverlay") != null, "Menu atmosphere light band overlay missing.");
            PremiumMenuMotionController premiumMotion = FindChild<PremiumMenuMotionController>(canvasTransform, "MainMenuCanvas");
            Require(premiumMotion != null, "Premium menu motion controller missing.");
            Canvas.ForceUpdateCanvases();
            InvokePrivate(premiumMotion, "Update");
            Rect canvasBounds = ((RectTransform)canvasTransform).rect;
            float screenDiagonal = Mathf.Sqrt(canvasBounds.width * canvasBounds.width + canvasBounds.height * canvasBounds.height);
            for (int stripeIndex = 1; stripeIndex <= 6; stripeIndex++)
            {
                Image stripe = FindChild<Image>(canvasTransform, "PremiumLightStripe_" + stripeIndex.ToString());
                Require(stripe != null, "Premium menu light stripe " + stripeIndex.ToString() + " missing.");
                Require(!stripe.raycastTarget, "Premium menu light stripes must not intercept button interaction.");
                Require(stripe.sprite != null, "Premium menu light stripes must use a soft alpha-gradient sprite.");
                Require(stripe.rectTransform.sizeDelta.x >= screenDiagonal * 1.35f, "Premium menu light stripes must extend beyond the visible screen diagonal.");
            }
            Require(FindChild<Image>(canvasTransform, "PremiumLightStripe_7") == null, "Premium menu must expose exactly six coordinated light beams.");
            Require(FindChild<Transform>(canvasTransform, "PrismaticSheen") == null, "Legacy hard-edged sheen stripe must not overlap the premium beam layer.");

            Require(FindChild<MenuDispersionDustController>(canvasTransform, "DispersionDust") != null, "Menu dispersion dust controller missing.");
            PremiumMenuPrismReactionController prismReaction = FindChild<PremiumMenuPrismReactionController>(canvasTransform, "PrismReaction");
            Require(prismReaction != null, "Menu reactive prism controller missing.");
            Require(!prismReaction.raycastTarget, "Menu reactive prism effect must not intercept button interaction.");
            Require(FindChild<MenuCrystalShimmerController>(canvasTransform, "CrystalShimmerHighlights") != null, "Menu crystal shimmer controller missing.");
            PremiumMenuPrismReactionController previewPrism = FindChild<PremiumMenuPrismReactionController>(canvasTransform, "PreviewPrismReaction");
            Require(previewPrism != null && !previewPrism.raycastTarget, "Preview artwork must expose a non-raycasting prism overlay above its image.");

            MenuAudioFeedbackController audioFeedback = controller.GetComponent<MenuAudioFeedbackController>();
            Require(audioFeedback != null, "Menu audio feedback controller must be centralized on the startup menu controller.");
            Require(audioFeedback.PlaybackSource != null && Mathf.Approximately(audioFeedback.PlaybackSource.spatialBlend, 0f), "Menu audio feedback must play through one 2D AudioSource.");
            Require(audioFeedback.Settings != null && audioFeedback.Settings.ButtonPressClip != null, "Menu click sound clip missing.");
            Require(audioFeedback.Settings.CheckboxEnabledClip != null && audioFeedback.Settings.CheckboxDisabledClip != null, "Menu checkbox feedback clips missing.");
            Require(audioFeedback.Settings.ButtonForwardClip != null && audioFeedback.Settings.ButtonBackClip != null, "Menu slider directional feedback clips missing.");
            Require(Mathf.Approximately(audioFeedback.Settings.Volume, 0.55f), "Menu audio feedback must use the authored default volume.");
            Require(audioFeedback.Settings.SliderCooldown >= 0.08f && audioFeedback.Settings.SliderCooldown <= 0.15f, "Menu slider feedback must use a controlled anti-spam cooldown.");

            Require(FindChild<Button>(canvasTransform, "EnterExperienceButton") != null, "Enter Experience button missing.");
            Require(FindChild<Toggle>(canvasTransform, "DemoModeToggle") != null, "Demo Mode toggle missing.");
            Require(FindChild<Button>(canvasTransform, "ModesButton") != null, "Modes button missing.");
            Require(FindChild<Button>(canvasTransform, "OpticsButton") != null, "Optics button missing.");
            Require(FindChild<Button>(canvasTransform, "PresetsButton") != null, "Presets button missing.");
            Require(FindChild<Button>(canvasTransform, "SettingsButton") != null, "Settings button missing.");
            Require(FindChild<Button>(canvasTransform, "ExitButton") != null, "Exit button missing.");
            RequireButtonLabel(canvasTransform, "EnterExperienceButton", "ENTER EXPERIENCE");
            RequireButtonLabel(canvasTransform, "DemoModeToggle", "DEMO MODE");
            RequireButtonLabel(canvasTransform, "ModesButton", "MODES");
            RequireButtonLabel(canvasTransform, "OpticsButton", "OPTICS");
            RequireButtonLabel(canvasTransform, "PresetsButton", "PRESETS");
            RequireButtonLabel(canvasTransform, "SettingsButton", "SETTINGS");
            RequireButtonLabel(canvasTransform, "ExitButton", "EXIT");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "EnterExperienceButton") != null, "Enter Experience must use KaelisMenuButton.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "DemoModeToggle") != null, "Demo Mode must use KaelisMenuButton.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "ExitButton") != null, "Exit must use KaelisMenuButton.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "ModesButton").HasTooltipData, "Main menu buttons must expose shared tooltip data.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "ModesButton").TooltipKeys == "Enter / Click", "Main section buttons must use action hotkey hints.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "DemoModeToggle").TooltipKeys.Contains("Space"), "Toggle buttons must use toggle hotkey hints.");
            Require(FindDescendant<RectMask2D>(FindChild<Transform>(canvasTransform, "EnterExperienceButton"), "GemActivationClip") != null, "Enter activation clip missing.");
            Require(FindDescendant<RectMask2D>(FindChild<Transform>(canvasTransform, "DemoModeToggle"), "GemActivationClip") != null, "Demo activation clip missing.");
            Require(FindDescendant<RectMask2D>(FindChild<Transform>(canvasTransform, "ExitButton"), "GemActivationClip") != null, "Exit activation clip missing.");
            Require(FindChild<Transform>(canvasTransform, "GemReleaseFlashLine") == null, "Release flash must not draw a thin line through button labels.");
            Require(FindChild<Transform>(canvasTransform, "ActivationTop") == null, "Button activation must not draw a thin top/strike line.");
            Require(FindChild<RawImage>(canvasTransform, "PreviewRawImage") != null, "Preview RawImage missing.");
            Require(FindChild<RawImage>(canvasTransform, "PreviewRawImage").color.a <= 0.60f, "Preview artwork opacity must allow the optical reaction to remain visible through the crystal image.");
            RawImage brandLogo = FindChild<RawImage>(canvasTransform, "BrandLogoImage");
            Require(brandLogo != null, "KAELIS brand logo image missing.");
            Require(brandLogo.texture != null, "KAELIS brand logo texture missing.");
            Require(brandLogo.uvRect == new Rect(0f, 0f, 1f, 1f), "KAELIS brand logo must use the full texture UV rect.");
            AspectRatioFitter brandFit = brandLogo.GetComponent<AspectRatioFitter>();
            Require(brandFit != null, "KAELIS brand logo must preserve aspect ratio.");
            Require(brandFit.aspectMode == AspectRatioFitter.AspectMode.FitInParent, "KAELIS brand logo must fit inside its frame.");
            Require(brandFit.aspectRatio > 1.70f && brandFit.aspectRatio < 1.86f, "KAELIS brand logo must fit by the full source image aspect ratio.");
            Require(FindChild<Mask>(canvasTransform, "BrandLogoFrame") == null, "KAELIS brand logo frame must not clip the logo.");
            Require(FindChild<RectMask2D>(canvasTransform, "BrandLogoFrame") == null, "KAELIS brand logo frame must not rect-mask the logo.");
            Require(FindChild<Transform>(canvasTransform, "SectionPanelsRoot") != null, "Section panels root missing.");
            Transform modesPanel = RequireSectionPanel(canvasTransform, "ModesSectionPanel");
            Transform opticsPanel = RequireSectionPanel(canvasTransform, "OpticsSectionPanel");
            Transform presetsPanel = RequireSectionPanel(canvasTransform, "PresetsSectionPanel");
            Transform settingsPanel = RequireSectionPanel(canvasTransform, "SettingsSectionPanel");
            Transform exitPanel = RequireSectionPanel(canvasTransform, "ExitSectionPanel");
            Require(FindChild<Transform>(canvasTransform, "DemoSectionPanel") == null, "Demo Mode must remain reserved and must not create a section panel.");
            Transform tooltipTransform = FindChild<Transform>(canvasTransform, "MenuTooltip");
            Require(tooltipTransform != null, "Menu tooltip bubble missing.");
            Component tooltipComponent = FindComponentByFullName(tooltipTransform, "Kaleidoscope2.Menu.KaelisMenuTooltip");
            Require(tooltipComponent != null, "Menu tooltip component missing.");
            RequireTooltipProperty(tooltipComponent, "FadeInSeconds", 0.5f);
            RequireTooltipFlag(tooltipComponent, "UsesAnchorPlacement");

            KaleidoscopeDirector director = UnityEngine.Object.FindObjectOfType<KaleidoscopeDirector>();
            Require(director != null, "Main scene must contain KaleidoscopeDirector for menu command dispatch.");
            DiamondFocusModule diamondFocusModule = UnityEngine.Object.FindObjectOfType<DiamondFocusModule>();
            Require(diamondFocusModule != null, "Main scene must contain DiamondFocusModule for crystal debug command routing.");
            if (!IsModuleRegistered(director, diamondFocusModule.ModuleId))
            {
                director.RegisterModule(diamondFocusModule);
            }

            KaleidoscopeCommandType dispatchedType = KaleidoscopeCommandType.None;
            director.CommandDispatched += command => dispatchedType = command.Type;
            ValidatePremiumCrystalMeshes();
            ValidatePremiumMorphCompletionRetainsTargetMesh();
            ValidateSharedCrystalDebugEffectMaterials();
            ValidateRuntimeCrystalControlRouting(director);
            ValidateDedicatedGeometryMorphShortcuts(director);
            ValidateControlOwnershipContract(director);
            ValidateStrictInputControlMap();
            ValidatePremiumFactoryPresetPayloads(director);

            FindChild<Button>(canvasTransform, "ModesButton").onClick.Invoke();
            Require(modesPanel.gameObject.activeSelf, "Modes button must open Modes section.");
            TMP_Text geometryHint = FindChild<TMP_Text>(modesPanel, "SmoothGeometryShortcutHint");
            Require(geometryHint != null && geometryHint.text.Contains("+ / - : Smooth Crystal Geometry") && geometryHint.text.Contains("Num Del"), "Geometry controls must distinguish dedicated smooth +/- morphing from selected-class cycling.");
            Transform showcaseBlock = FindChild<Transform>(modesPanel, "ShowcaseRecordingBlock");
            Require(showcaseBlock != null, "Modes panel must replace Experimental with Showcase / Recording.");
            Require(FindChild<Button>(modesPanel, "ExperimentalComingSoonCommand") == null, "Experimental / Coming Soon row must be removed from Modes.");
            KaelisMenuToggleControl secondDisplayToggle = FindChild<KaelisMenuToggleControl>(showcaseBlock, "OutputToSecondDisplayToggle");
            Require(secondDisplayToggle != null, "Showcase / Recording must expose Output To Second Display toggle.");
            Require(FindChild<TMP_Text>(showcaseBlock, "SecondDisplayStatus") != null, "Showcase / Recording must show second display status.");
            Require(FindChild<Button>(showcaseBlock, "TestSecondDisplayButton") != null, "Showcase / Recording must expose Test Display action.");
            KaelisMenuToggleControl recordingToggle = FindChild<KaelisMenuToggleControl>(showcaseBlock, "CreateVideoClipToggle");
            Require(recordingToggle != null, "Showcase / Recording must expose Create Video Clip toggle.");
            Require(!recordingToggle.Button.interactable, "Reserved recording toggle must be visibly inert until a recording backend exists.");
            Require(FindChild<Button>(showcaseBlock, "SelectRecordingOutputFolderButton") != null, "Showcase / Recording must expose recording output folder selector.");
            Require(FindChild<Button>(showcaseBlock, "ClearRecordingOutputFolderButton") != null, "Showcase / Recording must expose recording output folder clear action.");
            TMP_Text hotkeyHint = FindChild<TMP_Text>(showcaseBlock, "RecordingHotkeyHint");
            Require(hotkeyHint != null && hotkeyHint.text.Contains("Ctrl + Shift + R"), "Recording hotkey hint must mention Ctrl + Shift + R.");
            Require(FindDescendant<Transform>(showcaseBlock, "FrameRight") != null, "Showcase / Recording block must include a complete frame.");
            if (Display.displays == null || Display.displays.Length <= 1)
            {
                Button secondDisplayButton = secondDisplayToggle.GetComponent<Button>();
                Require(secondDisplayButton != null && !secondDisplayButton.interactable, "Second display toggle must disable when no second display is detected.");
            }

            Button classicModeCommand = FindChild<Button>(modesPanel, "Classic2DCommand");
            Require(classicModeCommand != null, "Modes panel must expose Classic 2D command.");
            classicModeCommand.onClick.Invoke();
            KaelisMenuInteractiveRow classicModeRow = classicModeCommand.GetComponent<KaelisMenuInteractiveRow>();
            Require(classicModeRow != null, "Classic 2D command must use interactive row behavior.");
            Require(classicModeRow.HasTooltipData, "Modes rows must carry tooltip anchor data.");
            Require(classicModeRow.IsSelected, "Classic 2D command must remain selected after click.");
            Require(dispatchedType == KaleidoscopeCommandType.SetVisualMode, "Classic 2D section action must use the public SetVisualMode command.");
            Button premiumModeCommand = FindChild<Button>(modesPanel, "Premium3DCrystalCommand");
            Require(premiumModeCommand != null, "Modes panel must expose Premium 3D Crystal command.");
            premiumModeCommand.onClick.Invoke();
            Require(director.State.DiamondFocusSettings.Enabled, "Premium 3D command must enable DiamondFocus.");
            Require(director.State.DiamondFocusSettings.CrystalSimulationMode == CrystalRenderMode.RealMesh3D, "Premium 3D command must switch to RealMesh3D.");
            Require(FindChild<Button>(modesPanel, "SphereCommand") != null, "Modes panel must expose Sphere.");
            Require(FindChild<Button>(modesPanel, "CubeCommand") != null, "Modes panel must expose Cube.");
            Require(FindChild<Button>(modesPanel, "OctahedronCommand") != null, "Modes panel must expose Octahedron.");
            Require(FindChild<Button>(modesPanel, "HexahedronCommand") != null, "Modes panel must expose Hexahedron.");
            Require(FindChild<Button>(modesPanel, "VolumetricRhombusCommand") != null, "Modes panel must expose Volumetric Rhombus.");
            Require(FindChild<Button>(modesPanel, "ConeCommand") != null, "Modes panel must expose Cone.");
            Require(FindChild<Button>(modesPanel, "PlateDiscCrystalCommand") != null, "Modes panel must expose Plate / Disc Crystal.");
            Require(FindChild<Button>(modesPanel, "IcosahedronCommand") != null, "Modes panel must expose Icosahedron.");
            Require(FindChild<Button>(modesPanel, "DodecahedronCommand") != null, "Modes panel must expose Dodecahedron.");
            Require(FindChild<Button>(modesPanel, "DoublePyramidCommand") != null, "Modes panel must expose Double Pyramid.");
            Require(FindChild<Button>(modesPanel, "CrystalLensCommand") != null, "Modes panel must expose Crystal Lens.");
            Button starPrismCommand = FindChild<Button>(modesPanel, "StarPrismCommand");
            Require(starPrismCommand != null, "Modes panel must expose Star Prism.");
            director.State.DiamondFocusSettings.SetPremiumCrystalShape(PremiumCrystalShapeType.Cube);
            dispatchedType = KaleidoscopeCommandType.None;
            starPrismCommand.onClick.Invoke();
            Require(dispatchedType == KaleidoscopeCommandType.SetPremiumCrystalShape, "Premium3D form control must dispatch SetPremiumCrystalShape.");
            Require(director.State.DiamondFocusSettings.PremiumCrystalShape == PremiumCrystalShapeType.StarPrism, "Premium3D Star Prism control must update the runtime Premium shape.");
            Require(director.State.DiamondFocusSettings.PremiumShapeTransitionActive, "Premium3D form control must preserve smooth visible shape transitions.");
            Button absoluteMirrorMode = FindChild<Button>(modesPanel, "AbsoluteMirrorCommand");
            Require(absoluteMirrorMode != null, "Modes panel must expose Absolute Mirror optical mode.");
            dispatchedType = KaleidoscopeCommandType.None;
            absoluteMirrorMode.onClick.Invoke();
            Require(dispatchedType == KaleidoscopeCommandType.SetPremiumCrystalOpticalMode, "Optical mode control must dispatch SetPremiumCrystalOpticalMode.");
            Require(director.State.DiamondFocusSettings.ActivePremiumCrystalOpticalMode == PremiumCrystalOpticalMode.AbsoluteMirror, "Absolute Mirror control must select the Premium optical mode.");
            Require(Mathf.Approximately(director.State.DiamondFocusSettings.PremiumOpticsDirectTransparency, 0f), "Absolute Mirror must disable direct transparency.");
            director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode.InternalReflection));
            dispatchedType = KaleidoscopeCommandType.None;
            director.Dispatch(KaleidoscopeCommand.CyclePremiumCrystalOpticalMode(1));
            Require(dispatchedType == KaleidoscopeCommandType.CyclePremiumCrystalOpticalMode, "F11 optical cycle command must dispatch through the Premium optical mode API.");
            Require(director.State.DiamondFocusSettings.ActivePremiumCrystalOpticalMode == PremiumCrystalOpticalMode.AbsoluteMirror, "Premium optical cycling must include Absolute Mirror.");
            Require(Mathf.Approximately(director.State.DiamondFocusSettings.PremiumOpticsDirectTransparency, 0f), "Cycled Absolute Mirror must still prohibit direct transparency.");

            FindChild<Button>(canvasTransform, "OpticsButton").onClick.Invoke();
            Require(!modesPanel.gameObject.activeSelf && opticsPanel.gameObject.activeSelf, "Optics button must switch to Optics section only.");
            KaelisMenuSliderControl brightnessSlider = FindChild<KaelisMenuSliderControl>(opticsPanel, "BrightnessSlider");
            Require(brightnessSlider != null, "Optics panel must expose a premium Brightness slider.");
            Require(Mathf.Approximately(brightnessSlider.MinValue, 0.10f) && Mathf.Approximately(brightnessSlider.MaxValue, 3.00f), "Brightness slider must use the expanded creative range.");
            Require(brightnessSlider.GetComponent<KaelisMenuInteractiveRow>().HasTooltipData, "Optics sliders must carry tooltip anchor data.");
            Require(brightnessSlider.GetComponent<KaelisMenuInteractiveRow>().TooltipKeys.Contains("Drag"), "Sliders must expose pointer adjustment without stealing runtime hotkeys.");
            Require(FindDescendant<Transform>(brightnessSlider.transform, "RightGold") != null, "Interactive hover frames must include the right frame edge.");
            Require(FindChild<KaelisMenuToggleControl>(opticsPanel, "CausticsToggle") != null, "Optics panel must expose a premium Caustics toggle.");
            Slider brightnessUnitySlider = brightnessSlider.GetComponentInChildren<Slider>(true);
            Require(brightnessUnitySlider != null, "Brightness slider must include a Unity Slider.");
            dispatchedType = KaleidoscopeCommandType.None;
            brightnessUnitySlider.value = 1.7f;
            Require(dispatchedType == KaleidoscopeCommandType.SetPremiumCrystalOptic, "Brightness slider must dispatch SetPremiumCrystalOptic.");
            FindChild<Button>(canvasTransform, "PresetsButton").onClick.Invoke();
            Require(!opticsPanel.gameObject.activeSelf && presetsPanel.gameObject.activeSelf, "Presets button must switch to Presets section only.");
            Button diamondPreset = FindChild<Button>(presetsPanel, "DiamondPalaceCommand");
            Require(diamondPreset != null, "Presets panel must expose Diamond Palace preset card.");
            diamondPreset.onClick.Invoke();
            KaelisMenuInteractiveRow diamondPresetRow = diamondPreset.GetComponent<KaelisMenuInteractiveRow>();
            Require(diamondPresetRow != null, "Diamond Palace preset card must use interactive row behavior.");
            Require(diamondPresetRow.IsSelected, "Diamond Palace preset card must remain selected after click.");
            Button applyPreset = FindChild<Button>(presetsPanel, "ApplyPresetButton");
            Require(applyPreset != null, "Presets panel must expose Apply Selected button.");
            Require(applyPreset.interactable, "Apply Selected button must become active after a preset is selected.");
            dispatchedType = KaleidoscopeCommandType.None;
            applyPreset.onClick.Invoke();
            Require(dispatchedType == KaleidoscopeCommandType.ApplyPremiumCrystalPreset, "Apply Selected must dispatch the Premium3D preset command.");
            Require(director.State.ActivePreset == "Diamond Palace", "Apply Selected must store the applied factory preset label.");
            Button alienArtifact = FindChild<Button>(presetsPanel, "AlienArtifactCoreCommand");
            Require(alienArtifact != null, "Presets panel must expose Alien Artifact Core experiment.");
            dispatchedType = KaleidoscopeCommandType.None;
            alienArtifact.onClick.Invoke();
            Require(dispatchedType == KaleidoscopeCommandType.ApplyExperimentalCrystalPreset, "Experiment card must dispatch ApplyExperimentalCrystalPreset.");
            Require(director.State.DiamondFocusSettings.ActiveExperimentalCrystalPreset == CrystalExperimentPresetType.AlienArtifactCore, "Experiment card must update runtime crystal experiment state.");
            Button normalExperiment = FindChild<Button>(presetsPanel, "NormalRestorePreviousCommand");
            Require(normalExperiment != null, "Presets panel must expose Normal / Restore Previous experiment control.");
            normalExperiment.onClick.Invoke();
            Require(director.State.DiamondFocusSettings.ActiveExperimentalCrystalPreset == CrystalExperimentPresetType.Normal, "Normal experiment must restore previous crystal state marker.");
            director.State.DiamondFocusSettings.SetDebugMode(DiamondCrystalDebugMode.FinalCrystalComposite);
            dispatchedType = KaleidoscopeCommandType.None;
            director.Dispatch(KaleidoscopeCommand.CycleCrystalDebugMode(1));
            Require(dispatchedType == KaleidoscopeCommandType.CycleCrystalDebugMode, "Diamond Focus debug hotkey command must dispatch CycleCrystalDebugMode.");
            Require(director.State.DiamondFocusSettings.DebugMode == DiamondCrystalDebugMode.RawKaleidoscopeTex, "CycleCrystalDebugMode must advance the runtime Diamond Focus debug mode.");
            director.State.DiamondFocusSettings.SetDebugMode(DiamondCrystalDebugMode.SurfaceNormalOnly);
            director.Dispatch(KaleidoscopeCommand.CycleCrystalDebugMode(1));
            Require(director.State.DiamondFocusSettings.DebugMode == DiamondCrystalDebugMode.ArtifactStressTest, "Debug cycling must skip the invisible Crystal Off stop.");
            director.State.DiamondFocusSettings.SetDebugMode(DiamondCrystalDebugMode.CrystalOff);
            Require(director.State.DiamondFocusSettings.DebugMode == DiamondCrystalDebugMode.CrystalOff, "Crystal Off must remain explicitly available for diagnostics.");
            director.State.DiamondFocusSettings.SetDebugMode(DiamondCrystalDebugMode.ReflectionOnly);
            director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode.PrismDispersion));
            Require(director.State.DiamondFocusSettings.DebugMode == DiamondCrystalDebugMode.ReflectionOnly, "Premium optical mode switching must preserve the active debug view.");
            director.State.DiamondFocusSettings.SetDebugMode(DiamondCrystalDebugMode.FinalCrystalComposite);
            dispatchedType = KaleidoscopeCommandType.None;
            director.Dispatch(KaleidoscopeCommand.SetCrystalDebugEffect(CrystalDebugEffectType.None));
            Require(dispatchedType == KaleidoscopeCommandType.SetCrystalDebugEffect, "Crystal debug effects must dispatch through SetCrystalDebugEffect.");
            for (int effectIndex = 1; effectIndex < CrystalDebugEffectLibrary.Count; effectIndex++)
            {
                director.Dispatch(KaleidoscopeCommand.CycleCrystalDebugEffect(1));
                CrystalDebugEffectType expectedEffect = (CrystalDebugEffectType)effectIndex;
                Require(director.State.DiamondFocusSettings.CrystalDebugEffects.SelectedEffect == expectedEffect, "Shared crystal debug cycling must expose " + CrystalDebugEffectLibrary.GetDisplayName(expectedEffect) + ".");
            }

            director.Dispatch(KaleidoscopeCommand.SetCrystalDebugEffect(CrystalDebugEffectType.SeaFrostedBrokenBottleGlass));
            director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode.AbsoluteMirror));
            Require(director.State.DiamondFocusSettings.CrystalDebugEffects.SelectedEffect == CrystalDebugEffectType.SeaFrostedBrokenBottleGlass, "Absolute Mirror must preserve the selected shared crystal debug effect.");
            Require(Mathf.Approximately(director.State.DiamondFocusSettings.PremiumOpticsDirectTransparency, 0f), "Shared effects must not re-enable direct transparency in Absolute Mirror.");
            director.Dispatch(KaleidoscopeCommand.SetCrystalDebugEffect(CrystalDebugEffectType.None));
            FindChild<Button>(canvasTransform, "SettingsButton").onClick.Invoke();
            Require(!presetsPanel.gameObject.activeSelf && settingsPanel.gameObject.activeSelf, "Settings button must switch to Settings section only.");
            Require(FindChild<KaelisMenuSliderControl>(settingsPanel, "UIScaleSlider") != null, "Settings panel must expose UI Scale slider.");
            Require(FindChild<KaelisMenuSliderControl>(settingsPanel, "TargetFPSSlider") != null, "Settings panel must expose Target FPS slider.");
            Require(FindChild<KaelisMenuToggleControl>(settingsPanel, "InvertZoomToggle") != null, "Settings panel must expose Invert Zoom toggle.");
            Require(!FindChild<KaelisMenuSliderControl>(settingsPanel, "UIScaleSlider").GetComponentInChildren<Slider>(true).interactable, "Reserved UI Scale slider must not impersonate a runtime binding.");
            Require(!FindChild<KaelisMenuToggleControl>(settingsPanel, "InvertZoomToggle").Button.interactable, "Reserved Invert Zoom toggle must not impersonate a runtime binding.");
            KaelisMenuToggleControl wheelScaleToggle = FindChild<KaelisMenuToggleControl>(settingsPanel, "MouseWheelCrystalScaleToggle");
            Require(wheelScaleToggle != null, "Settings panel must expose Mouse Wheel Crystal Scale toggle.");
            dispatchedType = KaleidoscopeCommandType.None;
            wheelScaleToggle.Button.onClick.Invoke();
            Require(dispatchedType == KaleidoscopeCommandType.SetPremiumCrystalWheelScaleEnabled, "Mouse Wheel Crystal Scale toggle must dispatch the runtime wheel setting.");
            Require(!director.State.MouseWheelVisualScaleEnabled, "Mouse Wheel Crystal Scale toggle must update the shared Classic2D/Premium3D wheel scale setting.");
            KaelisMenuSliderControl scaleStepSlider = FindChild<KaelisMenuSliderControl>(settingsPanel, "CrystalScaleStepSlider");
            Require(scaleStepSlider != null, "Settings panel must expose Crystal Scale Step slider.");
            Slider scaleStepUnitySlider = scaleStepSlider.GetComponentInChildren<Slider>(true);
            Require(scaleStepUnitySlider != null, "Crystal Scale Step must include a Unity Slider.");
            dispatchedType = KaleidoscopeCommandType.None;
            scaleStepUnitySlider.value = 15f;
            Require(dispatchedType == KaleidoscopeCommandType.SetPremiumCrystalWheelScaleStepPercent, "Crystal Scale Step must dispatch the runtime wheel step setting.");
            Require(Mathf.Approximately(director.State.MouseWheelVisualScaleStepPercent, 15f), "Crystal Scale Step must update the shared Classic2D/Premium3D wheel scale step.");
            Button languageCommand = FindChild<Button>(settingsPanel, "LanguageCommand");
            Require(languageCommand != null, "Settings panel must expose real Language selector.");
            Require(languageCommand.GetComponent<KaelisMenuInteractiveRow>().TooltipKeys.Contains("change language"), "Language selector must have truthful language-change hotkey hint.");
            FindChild<Button>(canvasTransform, "ExitButton").onClick.Invoke();
            Require(!settingsPanel.gameObject.activeSelf && exitPanel.gameObject.activeSelf, "Exit button must open confirmation section instead of quitting immediately.");
            Button exitCancel = FindChild<Button>(exitPanel, "ExitCancelButton");
            Require(exitCancel != null, "Exit section must include a Cancel button.");
            exitCancel.onClick.Invoke();
            Require(!exitPanel.gameObject.activeSelf, "Exit Cancel must close the Exit section.");

            Toggle demoToggle = FindChild<Toggle>(canvasTransform, "DemoModeToggle");
            demoToggle.isOn = true;
            Require(controller.DemoModeEnabled, "Demo toggle must store enabled state.");

            Button enterButton = FindChild<Button>(canvasTransform, "EnterExperienceButton");
            dispatchedType = KaleidoscopeCommandType.None;
            enterButton.onClick.Invoke();
            Transform contentFlow = FindChild<Transform>(canvasTransform, "ContentSelectionFlow");
            Require(contentFlow != null, "Enter Experience content selection flow missing.");
            Require(contentFlow.gameObject.activeSelf, "Enter Experience must open the content selection flow.");
            Require(dispatchedType == KaleidoscopeCommandType.None, "Enter Experience must not bypass content selection with an immediate runtime command.");
            Require(FindChild<Button>(contentFlow, "SelectImageFolderButton") != null, "Content flow image folder selector missing.");
            Require(FindChild<Button>(contentFlow, "SelectMusicFolderButton") != null, "Content flow music folder selector missing.");
            Require(FindChild<Button>(contentFlow, "SelectImageFolderButton").GetComponent<KaelisMenuInteractiveRow>() != null, "Content action buttons must use interactive rows and tooltips.");
            Require(FindDescendant<Transform>(FindChild<Transform>(contentFlow, "ImagesSourceBlock"), "FrameRight") != null, "Image source box must include right frame edge.");
            Require(FindDescendant<Transform>(FindChild<Transform>(contentFlow, "MusicSourceBlock"), "FrameLeft") != null, "Music source box must include left frame edge.");
            Button startExperience = FindChild<Button>(contentFlow, "StartExperienceButton");
            Require(startExperience != null, "Content flow Start Experience button missing.");
            startExperience.onClick.Invoke();
            TMP_Text validationText = FindChild<TMP_Text>(contentFlow, "ContentValidationMessage");
            Require(validationText != null, "Content flow validation text missing.");
            Require(validationText.text.Contains("Please select an image folder first."), "Start Experience must require an image folder.");
            Button backButton = FindChild<Button>(contentFlow, "BackContentButton");
            Require(backButton != null, "Content flow Back button missing.");
            backButton.onClick.Invoke();
            Require(!contentFlow.gameObject.activeSelf, "Back must close the content selection flow.");
            Require(FindChild<Transform>(canvasTransform, "SafeFrame").gameObject.activeSelf, "Back must return to the main startup menu.");

            FindChild<Button>(canvasTransform, "OpticsButton").onClick.Invoke();
            Require(opticsPanel.gameObject.activeSelf, "Optics section must be open before return-to-root navigation validation.");
            KaleidoscopeVisualMode preservedVisualMode = director.State.ActiveVisualMode;
            bool preservedCrystalEnabled = director.State.DiamondFocusSettings.Enabled;
            CrystalRenderMode preservedCrystalSimulation = director.State.DiamondFocusSettings.CrystalSimulationMode;
            director.Dispatch(KaleidoscopeCommand.SetControlMenuVisible(true));
            director.Dispatch(KaleidoscopeCommand.SetHotkeysHelpVisible(true));
            director.Dispatch(KaleidoscopeCommand.ReturnToInitialMenu());
            Require(!opticsPanel.gameObject.activeSelf && FindChild<Transform>(canvasTransform, "SafeFrame").gameObject.activeSelf, "Escape navigation command must return submenus to the initial KAELIS menu.");
            Require(!director.State.ControlMenuVisible && !director.State.HotkeysHelpVisible, "Escape navigation command must close runtime UI overlays.");
            Require(director.State.ActiveVisualMode == preservedVisualMode
                && director.State.DiamondFocusSettings.Enabled == preservedCrystalEnabled
                && director.State.DiamondFocusSettings.CrystalSimulationMode == preservedCrystalSimulation,
                "Escape navigation command must preserve visual and crystal state.");

            FindChild<Button>(canvasTransform, "SettingsButton").onClick.Invoke();
            languageCommand.onClick.Invoke();
            Require(KaelisMenuLocalizationService.CurrentLanguage == KaelisMenuLanguage.Russian, "Language selector must switch to Russian.");
            Require(PlayerPrefs.GetString(KaelisMenuLocalizationService.PlayerPrefsKey) == KaelisMenuLanguage.Russian.ToString(), "Language selection must persist.");
            RequireButtonLabel(canvasTransform, "ModesButton", "РЕЖИМЫ");
            languageCommand.onClick.Invoke();
            Require(KaelisMenuLocalizationService.CurrentLanguage == KaelisMenuLanguage.German, "Language selector must switch to German.");
            RequireButtonLabel(canvasTransform, "ModesButton", "MODI");
            languageCommand.onClick.Invoke();
            Require(KaelisMenuLocalizationService.CurrentLanguage == KaelisMenuLanguage.Ukrainian, "Language selector must switch to Ukrainian.");
            RequireButtonLabel(canvasTransform, "ModesButton", "РЕЖИМИ");
            languageCommand.onClick.Invoke();
            Require(KaelisMenuLocalizationService.CurrentLanguage == KaelisMenuLanguage.English, "Language selector must cycle back to English.");
            RequireButtonLabel(canvasTransform, "ModesButton", "MODES");

            InvokePrivate(controller, "OnDestroy");
            UnityEngine.Object.DestroyImmediate(canvasTransform.gameObject);

            Debug.Log("[KAELIS Menu SmokeTest] Startup menu hierarchy and core button behavior verified.");
        }

        private static void ValidatePremiumCrystalMeshes()
        {
            Array shapes = Enum.GetValues(typeof(CrystalShape));
            for (int index = 0; index < shapes.Length; index++)
            {
                CrystalShape shape = (CrystalShape)shapes.GetValue(index);
                Mesh mesh = RealCrystalShapeLibrary.CreateMesh(shape);
                Require(mesh != null, "Premium3D mesh must be generated for " + shape + ".");
                Require(RealCrystalVolumetricMeshFactory.HasVolume(mesh), "Premium3D mesh must have real volume for " + shape + ".");
                Require(RealCrystalShapeLibrary.HasSideFaces(mesh), "Premium3D mesh must have side faces for " + shape + ".");
                Require(RealCrystalShapeLibrary.HasSeparatedFrontBack(mesh), "Premium3D mesh must separate front/back depth for " + shape + ".");
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        private static void ValidateSharedCrystalDebugEffectMaterials()
        {
            Shader classicShader = Shader.Find("Kaleidoscope2/DiamondCrystal3D");
            Shader premiumShader = Shader.Find("Kaleidoscope2/RealCrystalOptics");
            Require(classicShader != null, "Classic crystal shader must support shared debug effects.");
            Require(premiumShader != null, "Premium crystal shader must support shared debug effects.");

            Material classicMaterial = new Material(classicShader);
            Material premiumMaterial = new Material(premiumShader);
            CrystalDebugEffectSettings effectSettings = new CrystalDebugEffectSettings();
            CrystalDebugEffectApplier effectApplier = new CrystalDebugEffectApplier();
            try
            {
                Array effects = Enum.GetValues(typeof(CrystalDebugEffectType));
                for (int index = 0; index < effects.Length; index++)
                {
                    CrystalDebugEffectType effect = (CrystalDebugEffectType)effects.GetValue(index);
                    effectSettings.SetEffect(effect);
                    effectApplier.Apply(classicMaterial, effectSettings, false);
                    effectApplier.Apply(premiumMaterial, effectSettings, false);
                    Require(Mathf.Approximately(classicMaterial.GetFloat("_CrystalDebugEffectType"), (float)effect), "Classic shader must accept " + effectSettings.DisplayName + ".");
                    Require(Mathf.Approximately(premiumMaterial.GetFloat("_CrystalDebugEffectType"), (float)effect), "Premium shader must accept " + effectSettings.DisplayName + ".");
                }

                effectSettings.SetEffect(CrystalDebugEffectType.SeaFrostedBrokenBottleGlass);
                effectApplier.Apply(classicMaterial, effectSettings, false);
                effectApplier.Apply(premiumMaterial, effectSettings, true);
                Require(classicMaterial.GetFloat("_CrystalDebugRoughness") >= 0.9f
                    && classicMaterial.GetFloat("_CrystalDebugEdgeGlow") >= 1f
                    && classicMaterial.GetFloat("_CrystalDebugHalo") > 0f,
                    "Classic Frosted Glass must receive matte and edge-readability effect values.");
                Require(premiumMaterial.GetFloat("_CrystalDebugRoughness") >= 0.9f
                    && premiumMaterial.GetFloat("_CrystalDebugEdgeGlow") >= 1f,
                    "Premium Frosted Glass must receive matte and edge-readability effect values.");
                Require(Mathf.Approximately(premiumMaterial.GetFloat("_CrystalDebugAbsoluteMirrorGuard"), 1f), "Absolute Mirror must enable the shared-effect opacity guard.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(classicMaterial);
                UnityEngine.Object.DestroyImmediate(premiumMaterial);
            }
        }

        private static void ValidatePremiumMorphCompletionRetainsTargetMesh()
        {
            GameObject owner = new GameObject("PremiumMorphCompletionValidationOwner");
            RenderTexture sourceTexture = new RenderTexture(96, 96, 24, RenderTextureFormat.ARGB32);
            SpatialCrystalStage3D stage = new SpatialCrystalStage3D();
            try
            {
                sourceTexture.Create();
                DiamondFocusSettings settings = new DiamondFocusSettings();
                settings.SetEnabled(true);
                settings.SetCrystalSimulationMode(CrystalRenderMode.RealMesh3D);
                settings.SetPremiumCrystalShape(PremiumCrystalShapeType.Hexahedron);
                settings.BeginPremiumCrystalShapeTransition(PremiumCrystalShapeType.Octahedron);

                CrystalSharedSettings sharedSettings = new CrystalSharedSettings();
                sharedSettings.SyncFromDiamond(settings);
                stage.Initialize(owner.transform, 31);
                Require(stage.Render(
                    sourceTexture,
                    sharedSettings,
                    sharedSettings.Shape,
                    CrystalMaterialMode.Diamond,
                    Vector3.zero,
                    8f,
                    true,
                    false,
                    CrystalStage3DDebugMode.FinalPremiumComposite), "Premium transition validation stage must render its morph frame.");
                int transitioningVertexCount = stage.MeshVertexCount;

                settings.TickShapeTransition(settings.ShapeTransitionDuration + 0.01f);
                sharedSettings.SyncFromDiamond(settings);
                Require(!settings.PremiumShapeTransitionActive, "Premium shape transition must finish for final mesh validation.");
                Require(settings.PremiumCrystalShape == PremiumCrystalShapeType.Octahedron, "Completed Premium transition must preserve its requested target shape.");
                Require(stage.Render(
                    sourceTexture,
                    sharedSettings,
                    sharedSettings.Shape,
                    CrystalMaterialMode.Diamond,
                    Vector3.zero,
                    8f,
                    true,
                    false,
                    CrystalStage3DDebugMode.FinalPremiumComposite), "Premium transition validation stage must render its settled frame.");
                Require(stage.MeshVertexCount == transitioningVertexCount, "Completed Premium transition must retain the final morph topology rather than snap to a reduced primitive mesh.");
                Require(stage.DiagnosticsLabel.Contains("final mesh source completed Premium morph topology retained"), "Completed Premium transition diagnostics must identify its retained Premium mesh source.");
                Require(stage.DiagnosticsLabel.Contains("premium shape PremiumOctahedron -> PremiumOctahedron"), "Completed Premium transition must render the requested Premium mesh profile.");
            }
            finally
            {
                stage.Shutdown();
                sourceTexture.Release();
                UnityEngine.Object.DestroyImmediate(sourceTexture);
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        private static void ValidateRuntimeCrystalControlRouting(KaleidoscopeDirector director)
        {
            DiamondFocusSettings settings = director.State.DiamondFocusSettings;
            settings.SetEnabled(false);
            settings.SetCrystalSimulationMode(CrystalRenderMode.Billboard2D);
            settings.SetPremiumCrystalShape(PremiumCrystalShapeType.Cube);
            settings.SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode.InternalReflection);
            settings.SetDebugMode(DiamondCrystalDebugMode.SurfaceNormalOnly);
            settings.SetCrystalDebugEffect(CrystalDebugEffectType.None);

            director.Dispatch(KaleidoscopeCommand.SetCrystalRuntimeControlModule(CrystalRuntimeControlModule.PremiumCrystalShape));
            director.Dispatch(KaleidoscopeCommand.CycleSelectedCrystalRuntimeControl(1));
            Require(settings.RuntimeControl.SelectedModule == CrystalRuntimeControlModule.PremiumCrystalShape, "Numpad shape module selection must be stored independently.");
            Require(settings.PremiumCrystalShape == PremiumCrystalShapeType.Octahedron, "Selected Shape control must cycle through safe Premium forms.");
            Require(!settings.Enabled && !settings.IsPremiumCrystalSimulation, "Shape class cycling must not activate or switch Premium mode.");

            director.Dispatch(KaleidoscopeCommand.SetCrystalRuntimeControlModule(CrystalRuntimeControlModule.PremiumOpticalMode));
            director.Dispatch(KaleidoscopeCommand.CycleSelectedCrystalRuntimeControl(1));
            Require(settings.ActivePremiumCrystalOpticalMode == PremiumCrystalOpticalMode.AbsoluteMirror, "Selected Optical control must cycle into Absolute Mirror.");
            Require(Mathf.Approximately(settings.PremiumOpticsDirectTransparency, 0f), "Runtime Optical cycling must preserve Absolute Mirror opacity.");
            Require(!settings.Enabled && !settings.IsPremiumCrystalSimulation, "Optical class cycling must not activate or switch Premium mode.");
            director.Dispatch(KaleidoscopeCommand.CycleSelectedCrystalRuntimeControl(1));
            Require(settings.ActivePremiumCrystalOpticalMode == PremiumCrystalOpticalMode.HighPurityDiamond, "Class 2 cycling must not enter the explicit experimental alias.");
            Require(settings.ActiveExperimentalCrystalPreset == CrystalExperimentPresetType.Normal, "Class 2 cycling must not activate Experimental Crystal presets.");

            director.Dispatch(KaleidoscopeCommand.SetCrystalRuntimeControlModule(CrystalRuntimeControlModule.CrystalDebugMode));
            director.Dispatch(KaleidoscopeCommand.CycleSelectedCrystalRuntimeControl(1));
            Require(settings.DebugMode == DiamondCrystalDebugMode.ArtifactStressTest, "Selected Debug Mode control must skip Crystal Off during normal cycling.");

            director.Dispatch(KaleidoscopeCommand.SetCrystalRuntimeControlModule(CrystalRuntimeControlModule.CrystalDebugEffect));
            director.Dispatch(KaleidoscopeCommand.CycleSelectedCrystalRuntimeControl(1));
            Require(settings.CrystalDebugEffects.SelectedEffect == CrystalDebugEffectType.PerfectMirrorBoost, "Selected Debug Effect control must cycle shared effects.");

            settings.SetRuntimeControlModule(CrystalRuntimeControlModule.CrystalDebugMode);
            settings.SetDebugMode(DiamondCrystalDebugMode.FinalCrystalComposite);
        }

        private static void ValidateControlOwnershipContract(KaleidoscopeDirector director)
        {
            DiamondFocusSettings settings = director.State.DiamondFocusSettings;
            settings.SetEnabled(false);
            settings.SetCrystalSimulationMode(CrystalRenderMode.Billboard2D);
            settings.SetDebugMode(DiamondCrystalDebugMode.SurfaceNormalOnly);

            director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalShape(PremiumCrystalShapeType.StarPrism));
            director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode.PrismDispersion));
            Require(!settings.Enabled && settings.CrystalSimulationMode == CrystalRenderMode.Billboard2D, "Shape and optics commands must remain local state selections until G or an explicit mode/preset action selects Premium.");

            director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.DebugOpticalDiagnostics, true));
            Require(settings.DebugMode == DiamondCrystalDebugMode.SurfaceNormalOnly, "Optical diagnostics must not overwrite the independently selected Debug Mode.");

            settings.SetEnabled(true);
            director.Dispatch(KaleidoscopeCommand.ToggleDiamondFocus());
            Require(!settings.Enabled
                && settings.CrystalSimulationMode == CrystalRenderMode.Billboard2D
                && settings.ActivePremiumCrystalOpticalMode == PremiumCrystalOpticalMode.PrismDispersion
                && settings.DebugMode == DiamondCrystalDebugMode.SurfaceNormalOnly,
                "Backspace visibility routing must not alter simulation, optical, or debug state.");

            director.Dispatch(KaleidoscopeCommand.ApplyExperimentalCrystalPreset(CrystalExperimentPresetType.AlienArtifactCore));
            Require(settings.Enabled && settings.CrystalSimulationMode == CrystalRenderMode.Billboard2D, "An explicit experimental preset may reveal its crystal but must not change the Classic/Premium simulation mode.");
            director.Dispatch(KaleidoscopeCommand.RestorePreviousCrystalPreset());
            Require(!settings.Enabled, "Restoring an experiment must restore the exact pre-lab visibility snapshot.");

            settings.SetEnabled(false);
            settings.SetDebugMode(DiamondCrystalDebugMode.FinalCrystalComposite);
            settings.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.DebugOpticalDiagnostics, false);
        }

        private static void ValidateDedicatedGeometryMorphShortcuts(KaleidoscopeDirector director)
        {
            DiamondFocusSettings settings = director.State.DiamondFocusSettings;
            settings.SetEnabled(true);
            settings.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.ShapeMorphing, true);
            settings.SetCrystalSimulationMode(CrystalRenderMode.Billboard2D);
            settings.SetShape(DiamondFocusShape.RhombicCrystal);
            settings.SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode.AbsoluteMirror);
            settings.SetDebugMode(DiamondCrystalDebugMode.ReflectionOnly);

            director.Dispatch(KaleidoscopeCommand.CycleCrystalGeometryForward());
            Require(settings.ShapeTransitionActive && settings.ShapeTransitionToShape == DiamondFocusShape.OvalRingGem, "Plus must start the existing smooth Classic geometry transition.");
            Require(!settings.IsPremiumCrystalSimulation, "Classic Plus geometry morph must not switch into Premium mode.");
            Require(settings.ActivePremiumCrystalOpticalMode == PremiumCrystalOpticalMode.AbsoluteMirror && settings.DebugMode == DiamondCrystalDebugMode.ReflectionOnly, "Classic geometry morph must not alter optics or debug state.");

            director.Dispatch(KaleidoscopeCommand.CycleCrystalGeometryBackward());
            Require(settings.ShapeTransitionActive && settings.ShapeTransitionToShape == DiamondFocusShape.RhombicCrystal, "Minus must reverse through the existing smooth Classic geometry transition.");
            settings.TickShapeTransition(settings.ShapeTransitionDuration + 0.01f);
            Require(!settings.ShapeTransitionActive && settings.Shape == DiamondFocusShape.RhombicCrystal, "Classic shape must remain stable after its morph completes.");

            settings.SetCrystalSimulationMode(CrystalRenderMode.RealMesh3D);
            settings.SetPremiumCrystalShape(PremiumCrystalShapeType.Cube);
            director.Dispatch(KaleidoscopeCommand.CycleCrystalGeometryForward());
            Require(settings.PremiumShapeTransitionActive && settings.PremiumShapeTransitionToShape == PremiumCrystalShapeType.Octahedron, "Plus must start the curated smooth Premium geometry transition.");
            Require(settings.IsPremiumCrystalSimulation, "Premium Plus geometry morph must not switch back to Classic mode.");
            Require(settings.ActivePremiumCrystalOpticalMode == PremiumCrystalOpticalMode.AbsoluteMirror && Mathf.Approximately(settings.PremiumOpticsDirectTransparency, 0f), "Premium geometry morph must preserve Absolute Mirror opacity.");
            Require(settings.DebugMode == DiamondCrystalDebugMode.ReflectionOnly, "Premium geometry morph must not alter the selected debug state.");
            settings.TickShapeTransition(settings.ShapeTransitionDuration + 0.01f);
            Require(settings.PremiumCrystalShape == PremiumCrystalShapeType.Octahedron && !settings.PremiumShapeTransitionActive, "Premium Plus morph must settle on its requested curated shape.");

            director.Dispatch(KaleidoscopeCommand.CycleCrystalGeometryBackward());
            Require(settings.PremiumShapeTransitionActive && settings.PremiumShapeTransitionToShape == PremiumCrystalShapeType.Cube, "Minus must start the reverse curated Premium geometry transition.");
            settings.TickShapeTransition(settings.ShapeTransitionDuration + 0.01f);
            Require(settings.PremiumCrystalShape == PremiumCrystalShapeType.Cube && !settings.PremiumShapeTransitionActive, "Premium Minus morph must retain its final curated shape after completion.");
        }

        private static void ValidateStrictInputControlMap()
        {
            InputModule input = UnityEngine.Object.FindObjectOfType<InputModule>();
            Require(input != null, "Main scene must contain InputModule for control-map validation.");
            Type inputType = typeof(InputModule);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            Require((KeyCode)inputType.GetField("crystalSimulationModeToggleKey", fields).GetValue(input) == KeyCode.G, "G must remain the sole keyboard Classic/Premium switch.");
            Require((KeyCode)inputType.GetField("selectPremiumShapeModuleKey", fields).GetValue(input) == KeyCode.Keypad1, "Numpad 1 must select Class 1: Premium Shapes.");
            Require((KeyCode)inputType.GetField("selectPremiumOpticalModeModuleKey", fields).GetValue(input) == KeyCode.Keypad3, "Numpad 3 must select Class 2: Premium Optical Modes.");
            Require((KeyCode)inputType.GetField("selectCrystalDebugModeModuleKey", fields).GetValue(input) == KeyCode.Keypad7, "Numpad 7 must select Class 3: Debug Modes.");
            Require((KeyCode)inputType.GetField("selectCrystalDebugEffectModuleKey", fields).GetValue(input) == KeyCode.Keypad9, "Numpad 9 must select Class 4: Debug Effects.");
            Require((KeyCode)inputType.GetField("crystalGeometryForwardKey", fields).GetValue(input) == KeyCode.KeypadPlus, "Numpad Plus must be the dedicated smooth crystal geometry-forward shortcut.");
            Require((KeyCode)inputType.GetField("crystalGeometryBackwardKey", fields).GetValue(input) == KeyCode.KeypadMinus, "Numpad Minus must be the dedicated smooth crystal geometry-backward shortcut.");
            Require((KeyCode)inputType.GetField("toggleHotkeysHelpKey", fields).GetValue(input) == KeyCode.F1, "F1 must open the current hotkey help.");
            Require((KeyCode)inputType.GetField("closeMenuKey", fields).GetValue(input) == KeyCode.Escape, "Escape must route back to the initial menu.");
            Require(inputType.GetField("toggleSecondDisplayOutputKey", fields) == null, "Function keys must not trigger second-display output.");
            Require(inputType.GetField("diamondRotateDownLeftKey", fields) == null
                && inputType.GetField("diamondRotateDownRightKey", fields) == null
                && inputType.GetField("diamondRotateUpLeftKey", fields) == null
                && inputType.GetField("diamondRotateUpRightKey", fields) == null, "Numpad class selectors must not remain bound to diagonal crystal rotation.");
            Require(inputType.GetField("diamondNextShapeKey", fields) == null
                && inputType.GetField("diamondPreviousShapeKey", fields) == null
                && inputType.GetField("diamondNextMaterialModeKey", fields) == null, "Legacy ambiguous field routes must stay removed; dedicated geometry shortcut and selected-class routes are explicit.");
        }

        private static void ValidatePremiumFactoryPresetPayloads(KaleidoscopeDirector director)
        {
            PremiumCrystalFactoryPreset[] presets =
            {
                PremiumCrystalFactoryPreset.DiamondPalace,
                PremiumCrystalFactoryPreset.BlueIce,
                PremiumCrystalFactoryPreset.GoldenPrism,
                PremiumCrystalFactoryPreset.RubyNight,
                PremiumCrystalFactoryPreset.EmeraldDepth,
                PremiumCrystalFactoryPreset.OpalDream,
                PremiumCrystalFactoryPreset.CosmicGlass,
                PremiumCrystalFactoryPreset.DarkLuxury,
                PremiumCrystalFactoryPreset.AbsoluteMirror
            };
            CrystalDebugEffectType[] effects =
            {
                CrystalDebugEffectType.GlimmerLensFlare,
                CrystalDebugEffectType.SeaFrostedBrokenBottleGlass,
                CrystalDebugEffectType.RainbowPrismFire,
                CrystalDebugEffectType.Halo,
                CrystalDebugEffectType.MirageAtmosphericHeatHaze,
                CrystalDebugEffectType.RainbowPrismFire,
                CrystalDebugEffectType.FacetChromaticAberration,
                CrystalDebugEffectType.PerfectMirrorBoost,
                CrystalDebugEffectType.PerfectMirrorBoost
            };

            DiamondFocusSettings settings = director.State.DiamondFocusSettings;
            settings.SetEnabled(false);
            settings.SetCrystalSimulationMode(CrystalRenderMode.Billboard2D);
            for (int index = 0; index < presets.Length; index++)
            {
                director.Dispatch(KaleidoscopeCommand.ApplyPremiumCrystalPreset(presets[index]));
                Require(settings.Enabled && settings.IsPremiumCrystalSimulation, "Applying a factory Premium preset must enter its authored Premium crystal state explicitly.");
                Require(settings.PremiumShapeTransitionActive, "Factory preset must retain smooth Premium shape transitions.");
                Require(settings.CrystalDebugEffects.SelectedEffect == effects[index], "Factory preset must apply its authored shared debug effect: " + presets[index] + ".");
                Require(settings.PremiumOpticsReflectionStrength > 0f && settings.PremiumOpticsBloomGlow >= 0f, "Factory preset must apply real optical values: " + presets[index] + ".");
            }

            CrystalSharedSettings sharedSettings = new CrystalSharedSettings();
            sharedSettings.SyncFromDiamond(settings);
            Require(settings.ActivePremiumCrystalOpticalMode == PremiumCrystalOpticalMode.AbsoluteMirror, "Absolute Mirror preset must select Absolute Mirror optical mode.");
            Require(Mathf.Approximately(settings.PremiumOpticsDirectTransparency, 0f), "Absolute Mirror preset must store zero direct transparency.");
            Require(Mathf.Approximately(sharedSettings.DirectTransmission, 0f), "Absolute Mirror shared render state must store zero direct transmission.");
            Require(Mathf.Approximately(sharedSettings.Transparency, 0f), "Absolute Mirror shared render state must remain opaque.");
            settings.SetEnabled(true);
            settings.SetCrystalSimulationMode(CrystalRenderMode.RealMesh3D);
        }

        private static T FindChild<T>(Transform root, string name) where T : Component
        {
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

        private static Component FindComponentByFullName(Transform target, string fullName)
        {
            Component[] components = target.GetComponents<Component>();
            for (int index = 0; index < components.Length; index++)
            {
                if (components[index] != null && components[index].GetType().FullName == fullName)
                {
                    return components[index];
                }
            }

            return null;
        }

        private static Transform RequireSectionPanel(Transform root, string name)
        {
            Transform panel = FindChild<Transform>(root, name);
            Require(panel != null, name + " missing.");
            Require(!panel.gameObject.activeSelf, name + " must start closed.");
            Require(panel.GetComponent<CanvasGroup>() != null, name + " must have a CanvasGroup.");
            return panel;
        }

        private static T FindDescendant<T>(Transform root, string name) where T : Component
        {
            Require(root != null, "Root missing while searching for " + name + ".");
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

        private static void RequireButtonLabel(Transform root, string buttonName, string expected)
        {
            Transform button = FindChild<Transform>(root, buttonName);
            Require(button != null, buttonName + " missing while checking label.");
            TMP_Text label = FindDescendant<TMP_Text>(button, "Label");
            Require(label != null, buttonName + " label missing.");
            Require(label.text == expected, buttonName + " label text mismatch.");
            Require(label.enableAutoSizing, buttonName + " label must use controlled auto-size.");
            Require(label.overflowMode != TextOverflowModes.Ellipsis, buttonName + " label must not ellipsize.");
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Require(method != null, methodName + " method missing.");
            method.Invoke(target, null);
        }

        private static void RequireTooltipProperty(Component tooltip, string propertyName, float expected)
        {
            PropertyInfo property = tooltip.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Require(property != null, "Tooltip " + propertyName + " property missing.");
            float value = Convert.ToSingle(property.GetValue(tooltip, null));
            Require(Mathf.Approximately(value, expected), "Tooltip " + propertyName + " must be " + expected + " seconds.");
        }

        private static void RequireTooltipFlag(Component tooltip, string propertyName)
        {
            PropertyInfo property = tooltip.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Require(property != null, "Tooltip " + propertyName + " property missing.");
            bool value = Convert.ToBoolean(property.GetValue(tooltip, null));
            Require(value, "Tooltip must use anchor-based placement.");
        }

        private static bool IsModuleRegistered(KaleidoscopeDirector director, string moduleId)
        {
            if (director == null || string.IsNullOrEmpty(moduleId))
            {
                return false;
            }

            var modules = director.RegisteredModules;
            for (int index = 0; index < modules.Count; index++)
            {
                IKaleidoscopeModule module = modules[index];
                if (module != null && module.ModuleId == moduleId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException("[KAELIS Menu SmokeTest] " + message);
            }
        }
    }
}
