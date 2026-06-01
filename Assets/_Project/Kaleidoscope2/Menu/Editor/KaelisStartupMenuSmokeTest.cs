using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kaleidoscope2.AudioReactive;
using Kaleidoscope2.Core;
using Kaleidoscope2.Demo;
using Kaleidoscope2.DiamondFocus;
using Kaleidoscope2.DiamondFocus.CrystalStage3D;
using Kaleidoscope2.DiamondFocus.RealMesh;
using Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D;
using Kaleidoscope2.ImageSource;
using Kaleidoscope2.InputSystem;
using Kaleidoscope2.Menu;
using Kaleidoscope2.Menu.FX;
using Kaleidoscope2.Settings;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
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
            for (int stripeIndex = 1; stripeIndex <= 8; stripeIndex++)
            {
                Image stripe = FindChild<Image>(canvasTransform, "PremiumLightStripe_" + stripeIndex.ToString());
                Require(stripe != null, "Premium menu light stripe " + stripeIndex.ToString() + " missing.");
                Require(!stripe.raycastTarget, "Premium menu light stripes must not intercept button interaction.");
                Require(stripe.sprite != null, "Premium menu light stripes must use a soft alpha-gradient sprite.");
                Require(stripe.rectTransform.sizeDelta.x >= screenDiagonal * 1.35f, "Premium menu light stripes must extend beyond the visible screen diagonal.");
            }
            Require(FindChild<Image>(canvasTransform, "PremiumLightStripe_9") == null, "Premium menu must expose exactly eight coordinated light beams.");
            Require(FindChild<Transform>(canvasTransform, "PrismaticSheen") == null, "Legacy hard-edged sheen stripe must not overlap the premium beam layer.");

            Require(FindChild<MenuDispersionDustController>(canvasTransform, "DispersionDust") != null, "Menu dispersion dust controller missing.");
            Require(FindChild<MenuDefocusedLensParticleController>(canvasTransform, "DefocusedLensParticles") != null, "Menu defocused lens particle controller missing.");
            PremiumMenuPrismReactionController prismReaction = FindChild<PremiumMenuPrismReactionController>(canvasTransform, "PrismReaction");
            Require(prismReaction != null, "Menu reactive prism controller missing.");
            Require(!prismReaction.raycastTarget, "Menu reactive prism effect must not intercept button interaction.");
            Require(prismReaction.Settings != null, "Menu reactive prism controller must expose its normalized crystal interaction map.");
            Require(prismReaction.Settings.CrystalRectNormalized.width > 0.02f && prismReaction.Settings.CrystalRectNormalized.height > 0.02f, "Menu crystal interaction map must define a visible normalized crystal zone.");
            Require((prismReaction.Settings.TriggerLineEndNormalized - prismReaction.Settings.TriggerLineStartNormalized).sqrMagnitude > 0.01f, "Menu crystal interaction map must define a real trigger line.");
            Require(prismReaction.Settings.TriggerZoneWidthNormalized >= 0.01f, "Menu crystal interaction map must expose a non-zero trigger zone.");
            Require(prismReaction.Settings.CameraGlowIntensity > 0f, "Menu crystal interaction map must drive visible lens/camera glow when crossed.");
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
            Require(FindChild<Button>(canvasTransform, "MeditationModeButton") != null, "Meditation Mode button missing.");
            Require(FindChild<Button>(canvasTransform, "DemoModeButton") != null, "Demo button missing.");
            Require(FindChild<Button>(canvasTransform, "ModesButton") != null, "Modes button missing.");
            Require(FindChild<Button>(canvasTransform, "OpticsButton") != null, "Optics button missing.");
            Require(FindChild<Button>(canvasTransform, "PresetsButton") != null, "Presets button missing.");
            Require(FindChild<Button>(canvasTransform, "SettingsButton") != null, "Settings button missing.");
            Require(FindChild<Button>(canvasTransform, "AboutButton") != null, "About button missing.");
            Require(FindChild<Button>(canvasTransform, "ExitButton") != null, "Exit button missing.");
            RequireButtonLabel(canvasTransform, "EnterExperienceButton", "ENTER EXPERIENCE");
            RequireButtonLabel(canvasTransform, "MeditationModeButton", "MEDITATION MODE");
            RequireButtonLabel(canvasTransform, "DemoModeButton", "DEMO");
            RequireButtonLabel(canvasTransform, "ModesButton", "MODES");
            RequireButtonLabel(canvasTransform, "OpticsButton", "OPTICS");
            RequireButtonLabel(canvasTransform, "PresetsButton", "PRESETS");
            RequireButtonLabel(canvasTransform, "SettingsButton", "SETTINGS");
            RequireButtonLabel(canvasTransform, "AboutButton", "ABOUT");
            RequireButtonLabel(canvasTransform, "ExitButton", "EXIT");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "EnterExperienceButton") != null, "Enter Experience must use KaelisMenuButton.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "MeditationModeButton") != null, "Meditation Mode must use KaelisMenuButton.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "DemoModeButton") != null, "Demo must use KaelisMenuButton.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "ExitButton") != null, "Exit must use KaelisMenuButton.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "ModesButton").HasTooltipData, "Main menu buttons must expose shared tooltip data.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "ModesButton").TooltipKeys == "Enter / Click", "Main section buttons must use action hotkey hints.");
            Require(FindChild<KaelisMenuButton>(canvasTransform, "DemoModeButton").TooltipKeys == "Enter / Click", "Demo section button must use action hotkey hints.");
            Require(FindDescendant<RectMask2D>(FindChild<Transform>(canvasTransform, "EnterExperienceButton"), "GemActivationClip") != null, "Enter activation clip missing.");
            Require(FindDescendant<RectMask2D>(FindChild<Transform>(canvasTransform, "DemoModeButton"), "GemActivationClip") != null, "Demo activation clip missing.");
            Require(FindDescendant<RectMask2D>(FindChild<Transform>(canvasTransform, "ExitButton"), "GemActivationClip") != null, "Exit activation clip missing.");
            Require(FindChild<Transform>(canvasTransform, "GemReleaseFlashLine") == null, "Release flash must not draw a thin line through button labels.");
            Require(FindChild<Transform>(canvasTransform, "ActivationTop") == null, "Button activation must not draw a thin top/strike line.");
            RawImage previewRawImage = FindChild<RawImage>(canvasTransform, "PreviewRawImage");
            Require(previewRawImage != null, "Preview RawImage missing.");
            Require(previewRawImage.texture != null ? previewRawImage.color.a >= 0.78f : previewRawImage.color.a <= 0.01f, "Preview must either show assigned artwork clearly or stay transparent so the living background is not duplicated.");
            Require(FindChild<Transform>(canvasTransform, "PreviewDepthOverlay") == null, "Preview artwork must not be covered by the old full-area dark depth overlay.");
            Require(FindChild<Transform>(canvasTransform, "PreviewHeader") == null, "Preview area must not keep the old LIVE PREVIEW header.");
            Require(FindChild<TMP_Text>(canvasTransform, "LivePreviewLabel") == null, "Preview area must not show the old LIVE PREVIEW label.");
            Require(FindChild<Transform>(canvasTransform, "PreviewCaption") == null, "Preview area must not keep the old preview caption layer.");
            Require(FindChild<TMP_Text>(canvasTransform, "PreviewPanelTitle") == null, "Preview area must not show the old PREVIEW PANEL label.");
            Require(FindChild<TMP_Text>(canvasTransform, "PreviewPanelSubtitle") == null, "Preview area must not show the old RAW IMAGE SURFACE label.");
            Require(FindChild<Transform>(canvasTransform, "PreviewTopCyanBloom") == null, "Preview area must not add a preview-only top bloom overlay above the artwork.");
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
            Transform aboutPanel = RequireSectionPanel(canvasTransform, "AboutSectionPanel");
            Transform exitPanel = RequireSectionPanel(canvasTransform, "ExitSectionPanel");
            Transform demoPanel = RequireSectionPanel(canvasTransform, "DemoSectionPanel");
            Transform meditationSetupPanel = RequireSectionPanel(canvasTransform, "MeditationSetupSectionPanel");
            Transform replaySetupPanel = RequireSectionPanel(canvasTransform, "ReplaySetupSectionPanel");
            Transform benchmarkPanel = RequireSectionPanel(canvasTransform, "BenchmarkSectionPanel");
            ValidateMenuTransparencyStandard(canvasTransform, modesPanel, opticsPanel, presetsPanel, settingsPanel, aboutPanel, demoPanel, meditationSetupPanel, replaySetupPanel, benchmarkPanel, exitPanel);
            ValidateSectionScrollLayoutStandard(modesPanel, opticsPanel, presetsPanel, settingsPanel, aboutPanel, exitPanel, demoPanel, meditationSetupPanel, replaySetupPanel, benchmarkPanel);
            ValidateNoMenuTextArtifacts(canvasTransform);
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
            ValidateSectionCloseAndEscapeNavigation(canvasTransform, director, "ModesButton", modesPanel);
            ValidateSectionCloseAndEscapeNavigation(canvasTransform, director, "OpticsButton", opticsPanel);
            ValidateSectionCloseAndEscapeNavigation(canvasTransform, director, "PresetsButton", presetsPanel);
            ValidateSectionCloseAndEscapeNavigation(canvasTransform, director, "SettingsButton", settingsPanel);
            ValidateSectionCloseAndEscapeNavigation(canvasTransform, director, "AboutButton", aboutPanel);
            ValidateSectionCloseAndEscapeNavigation(canvasTransform, director, "ExitButton", exitPanel);
            ValidateSectionCloseAndEscapeNavigation(canvasTransform, director, "MeditationModeButton", meditationSetupPanel);
            ValidateSectionCloseAndEscapeNavigation(canvasTransform, director, "DemoModeButton", demoPanel);
            ValidateNestedDemoSectionCloseAndEscape(canvasTransform, director, demoPanel, "ReplayDemoCommand", replaySetupPanel);
            ValidateNestedDemoSectionCloseAndEscape(canvasTransform, director, demoPanel, "BenchmarkDemoCommand", benchmarkPanel);
            ValidatePremiumCrystalMeshes();
            ValidatePremiumMorphCompletionRetainsTargetMesh();
            ValidateSharedCrystalDebugEffectMaterials();
            ValidateRuntimeCrystalControlRouting(director);
            ValidateDedicatedGeometryMorphShortcuts(director);
            ValidateControlOwnershipContract(director);
            ValidateStrictInputControlMap();
            ValidatePremiumFactoryPresetPayloads(director);
            ValidatePremiumExposureResolution();
            ValidateComfortAndBenchmarkContracts(director);
            ValidateCuratedDemoTexturePlayback();
            ValidateSettingsPersistenceFoundation(settingsPanel);

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
            Button selectRecordingOutput = FindChild<Button>(showcaseBlock, "SelectRecordingOutputFolderButton");
            Button clearRecordingOutput = FindChild<Button>(showcaseBlock, "ClearRecordingOutputFolderButton");
            Require(selectRecordingOutput != null, "Showcase / Recording must expose recording output folder selector.");
            Require(clearRecordingOutput != null, "Showcase / Recording must expose recording output folder clear action.");
            Require(!selectRecordingOutput.interactable && !clearRecordingOutput.interactable, "Recording output folder actions must stay unavailable until Showcase Recording service exists.");
            TMP_Text hotkeyHint = FindChild<TMP_Text>(showcaseBlock, "RecordingHotkeyHint");
            Require(hotkeyHint != null && hotkeyHint.text.Contains("Showcase Recording service"), "Recording hotkey hint must not imply a working backend.");
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
            float preservedMirrorZoom = director.State.MirrorSettings.Zoom;
            float preservedPremiumScale = director.State.DiamondFocusSettings.PremiumCrystalScalePercent;
            director.Dispatch(KaleidoscopeCommand.SetClassicCrystalScalePercent(150f));
            Require(Mathf.Approximately(director.State.DiamondFocusSettings.ClassicCrystalScalePercent, 150f), "Classic mouse-wheel scale command must update the Classic crystal target.");
            Require(Mathf.Approximately(director.State.MirrorSettings.Zoom, preservedMirrorZoom), "Classic crystal scale command must not alter mirror/kaleidoscope zoom.");
            Require(Mathf.Approximately(director.State.DiamondFocusSettings.PremiumCrystalScalePercent, preservedPremiumScale), "Classic crystal scale command must not alter Premium crystal scale.");
            director.Dispatch(KaleidoscopeCommand.SetMirrorZoom(preservedMirrorZoom + 0.25f));
            Require(Mathf.Approximately(director.State.DiamondFocusSettings.ClassicCrystalScalePercent, 150f), "Arrow-key mirror zoom route must remain separate from Classic crystal scale.");
            director.Dispatch(KaleidoscopeCommand.SetMirrorZoom(preservedMirrorZoom));
            float expectedPremiumWheelScale = Mathf.Clamp(preservedPremiumScale + 12f, DiamondFocusSettings.PremiumCrystalScalePercentMin, DiamondFocusSettings.PremiumCrystalScalePercentMax);
            director.Dispatch(KaleidoscopeCommand.AdjustPremiumCrystalScalePercent(12f));
            Require(Mathf.Approximately(director.State.DiamondFocusSettings.PremiumCrystalScalePercent, expectedPremiumWheelScale), "Premium wheel scale command must still update the Premium crystal target.");
            Require(Mathf.Approximately(director.State.DiamondFocusSettings.ClassicCrystalScalePercent, 150f), "Premium crystal scale route must remain separate from Classic crystal scale.");
            director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalScalePercent(preservedPremiumScale));
            director.Dispatch(KaleidoscopeCommand.SetClassicCrystalScalePercent(DiamondFocusSettings.ClassicCrystalScalePercentDefault));
            ValidateSliderAudioRegression(audioFeedback, brightnessUnitySlider, scaleStepUnitySlider);
            Button languageCommand = FindChild<Button>(settingsPanel, "LanguageCommand");
            Require(languageCommand != null, "Settings panel must expose real Language selector.");
            Require(languageCommand.GetComponent<KaelisMenuInteractiveRow>().TooltipKeys.Contains("change language"), "Language selector must have truthful language-change hotkey hint.");

            FindChild<Button>(canvasTransform, "AboutButton").onClick.Invoke();
            Require(!settingsPanel.gameObject.activeSelf && aboutPanel.gameObject.activeSelf, "About button must open the About section only.");
            TMP_Text aboutTitle = FindChild<TMP_Text>(aboutPanel, "SectionTitle");
            Require(aboutTitle != null && aboutTitle.text == "ABOUT", "About section title must use one localized source label, not a mixed-language title.");
            Require(FindChild<Transform>(aboutPanel, "AboutHeaderSubtitleDivider") != null, "About header must expose a visual boundary under the subtitle.");
            Require(FindChild<TMP_Text>(aboutPanel, "AboutProjectTitle") != null, "About screen must show the KAELIS title from the About package.");
            Require(FindChild<Scrollbar>(aboutPanel, "VerticalScrollbar") != null, "About screen must expose a real vertical scrollbar.");
            Require(FindChild<Transform>(aboutPanel, "AboutContentStack") != null, "About screen must use the shared menu content stack.");
            Require(FindChild<Transform>(aboutPanel, "AboutHeroCard") != null, "About screen must use shared menu-style cards.");
            Require(FindChild<Transform>(aboutPanel, "AboutStoryCard") != null, "About screen must expose the creation story card.");
            Require(FindChild<Transform>(aboutPanel, "AboutCreditsCard") != null, "About screen must expose the credits/license card.");
            ValidateAboutVisualHarmony(canvasTransform, aboutPanel);
            ValidateAboutCloseRaycast(canvasTransform, aboutPanel);

            FindChild<Button>(canvasTransform, "ExitButton").onClick.Invoke();
            Require(!aboutPanel.gameObject.activeSelf && exitPanel.gameObject.activeSelf, "Exit button must open confirmation section instead of quitting immediately.");
            Button exitCancel = FindChild<Button>(exitPanel, "ExitCancelButton");
            Require(exitCancel != null, "Exit section must include a Cancel button.");
            exitCancel.onClick.Invoke();
            Require(!exitPanel.gameObject.activeSelf, "Exit Cancel must close the Exit section.");

            dispatchedType = KaleidoscopeCommandType.None;
            FindChild<Button>(canvasTransform, "MeditationModeButton").onClick.Invoke();
            Require(meditationSetupPanel.gameObject.activeSelf, "Selecting Meditation Mode must open its setup panel.");
            Require(dispatchedType == KaleidoscopeCommandType.None, "Opening Meditation setup must not start the session.");
            Require(FindChild<Button>(meditationSetupPanel, "StartMeditationModeCommand") != null, "Meditation setup panel must expose an explicit START action.");

            FindChild<Button>(canvasTransform, "DemoModeButton").onClick.Invoke();
            Require(demoPanel.gameObject.activeSelf, "Demo button must open the real Demo section.");
            Require(FindChild<Button>(demoPanel, "MeditationModeCommand") != null, "Demo section must expose Meditation Mode.");
            Require(FindChild<Button>(demoPanel, "ReplayDemoCommand") != null, "Demo section must expose Replay Demo.");
            Require(FindChild<Button>(demoPanel, "BenchmarkDemoCommand") != null, "Demo section must expose Benchmark Demo.");
            Require(!FindChild<Button>(demoPanel, "SaveBenchmarkResultCommand").interactable, "Save Benchmark Result must be context-disabled before a benchmark result exists.");
            dispatchedType = KaleidoscopeCommandType.None;
            FindChild<Button>(demoPanel, "MeditationModeCommand").onClick.Invoke();
            Require(meditationSetupPanel.gameObject.activeSelf, "Selecting Meditation from Demo must open its setup panel.");
            Require(dispatchedType == KaleidoscopeCommandType.None, "Opening Meditation setup from Demo must not start the session.");
            FindChild<Button>(canvasTransform, "DemoModeButton").onClick.Invoke();
            dispatchedType = KaleidoscopeCommandType.None;
            FindChild<Button>(demoPanel, "ReplayDemoCommand").onClick.Invoke();
            Require(replaySetupPanel.gameObject.activeSelf, "Selecting Replay Demo must open its setup panel.");
            Require(dispatchedType == KaleidoscopeCommandType.None, "Opening Replay setup must not start replay.");
            Require(FindChild<Button>(replaySetupPanel, "StartReplayDemoCommand") != null, "Replay setup panel must expose an explicit START action.");
            FindChild<Button>(canvasTransform, "DemoModeButton").onClick.Invoke();
            dispatchedType = KaleidoscopeCommandType.None;
            FindChild<Button>(demoPanel, "BenchmarkDemoCommand").onClick.Invoke();
            Require(benchmarkPanel.gameObject.activeSelf, "Selecting Benchmark Demo must open its setup panel.");
            Require(dispatchedType == KaleidoscopeCommandType.None, "Opening Benchmark setup must not begin a benchmark session.");
            Require(FindChild<Button>(benchmarkPanel, "StartVisualPerformanceBenchmarkCommand") != null, "Benchmark setup panel must expose an explicit START action.");
            Require(!FindChild<Button>(benchmarkPanel, "SaveLastBenchmarkResultCommand").interactable, "Save Last Benchmark Result must be context-disabled before a benchmark result exists.");

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
                    CrystalStage3DDebugMode.FinalPremiumComposite,
                    0f,
                    0f), "Premium transition validation stage must render its morph frame.");
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
                    CrystalStage3DDebugMode.FinalPremiumComposite,
                    0f,
                    0f), "Premium transition validation stage must render its settled frame.");
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

        private static void ValidateComfortAndBenchmarkContracts(KaleidoscopeDirector director)
        {
            Require(Mathf.Approximately(MeditationModeController.EvaluateMirrorRotationSpeedUnits(0f), 540f), "Meditation must start at its 1.5 rotations/sec comfort cap.");
            Require(Mathf.Approximately(MeditationModeController.EvaluateMirrorRotationSpeedUnits(5f), 90f), "Meditation breathing must smoothly reach 0.25 rotations/sec after five seconds.");
            Require(Mathf.Approximately(MeditationModeController.EvaluateMirrorRotationSpeedUnits(10f), 540f), "Meditation breathing must return to 1.5 rotations/sec after ten seconds.");
            Require(Mathf.Abs(MeditationModeController.EvaluateMirrorRotationSpeedUnits(60f)) < 0.01f, "Meditation direction reversal must cross zero at the scheduled one-minute boundary.");
            Require(MeditationModeController.EvaluateMirrorRotationSpeedUnits(61.01f) < 0f, "Meditation must continue in the opposite direction after reversal.");
            Require(MeditationModeController.ResolveWeightedMirrorCount(0f) == 6, "Meditation weighted mirror selection must favor the lowest number range first.");
            Require(MeditationModeController.ResolveWeightedMirrorCount(0.999f) == 1536, "Meditation weighted mirror selection must retain the rare highest-number outcome.");
            Require(Mathf.Approximately(MeditationModeController.RareFlightPulseDurationSeconds, 0.1f), "Meditation Q/E-equivalent pulses must remain exactly 0.1 seconds.");

            Require(Mathf.Approximately(CrystalSplitComfortController.EvaluateExpansion(0f), 0f), "Split comfort must start merged.");
            Require(CrystalSplitComfortController.CopyCount == 6, "Split comfort must remain authored as six full crystal copies.");
            Require(CrystalSplitComfortController.EvaluateClassicSelfRotationDirection(1f) == Vector2.left, "Classic formation self-rotation must use the semantic Numpad 4/6 rotation route before splitting.");
            Require(CrystalSplitComfortController.EvaluateClassicSelfRotationDirection(2.6f) == Vector2.zero, "Classic formation self-rotation must release before intact copies detach.");
            Require(Mathf.Approximately(CrystalSplitComfortController.EvaluateExpansion(2.4f), 0f), "Formation must remain whole through the Classic semantic self-rotation phase.");
            Require(Mathf.Approximately(CrystalSplitComfortController.EvaluateExpansion(5f), 1f), "Formation must fully detach after the calm reveal phase.");
            Require(Mathf.Approximately(CrystalSplitComfortController.EvaluateExpansion(7.5f), 1f), "Formation must remain separated through its orbit.");
            Require(Mathf.Approximately(CrystalSplitComfortController.EvaluateExpansion(12.5f), 0f), "Formation must re-form after its orbit.");
            Require(Mathf.Approximately(CrystalSplitComfortController.EvaluateOrbitAngleRadians(5f), 0f), "Orbital motion must begin after detachment.");
            Require(Mathf.Approximately(CrystalSplitComfortController.EvaluateOrbitAngleRadians(7.5f), Mathf.PI), "Orbital motion must traverse half a revolution midway.");
            Require(Mathf.Approximately(CrystalSplitComfortController.EvaluateOrbitAngleRadians(10f), Mathf.PI * 2f), "Orbital motion must complete one full revolution before merge.");
            Require(Mathf.Approximately(SpatialCrystalStage3D.PremiumComfortViewportSafeMargin, 0.15f), "Premium formation copies must reserve the declared 15% viewport margin.");
            float safeViewportRadius = SpatialCrystalStage3D.ResolveSafeComfortViewportRadius(new Vector2(0.5f, 0.5f), new Vector2(0.1f, 0.1f), SpatialCrystalStage3D.PremiumComfortViewportSafeMargin);
            Require(Mathf.Approximately(safeViewportRadius, 0.25f), "Premium formation orbit radius must account for copy half-extents inside the 15% viewport margin.");
            Vector2 smallOrbit = SpatialCrystalStage3D.ResolveAdaptiveComfortViewportLayout(new Vector2(0.15f, 0.15f), new Vector2(0.5f, 0.5f), SpatialCrystalStage3D.PremiumComfortViewportSafeMargin);
            Vector2 mediumOrbit = SpatialCrystalStage3D.ResolveAdaptiveComfortViewportLayout(new Vector2(0.60f, 0.60f), new Vector2(0.5f, 0.5f), SpatialCrystalStage3D.PremiumComfortViewportSafeMargin);
            Vector2 hugeOrbit = SpatialCrystalStage3D.ResolveAdaptiveComfortViewportLayout(new Vector2(1.20f, 1.20f), new Vector2(0.5f, 0.5f), SpatialCrystalStage3D.PremiumComfortViewportSafeMargin);
            Require(mediumOrbit.y > smallOrbit.y, "Premium formation orbit radius must increase when visible crystal copies get larger.");
            Require(mediumOrbit.y >= 0.60f * mediumOrbit.x * 1.1f, "Premium formation medium copies must keep enough adjacent spacing for a six-copy orbit.");
            Require(hugeOrbit.x < mediumOrbit.x, "Premium formation must reduce child copy scale when large copies would violate safe bounds.");
            Require(hugeOrbit.y <= SpatialCrystalStage3D.ResolveSafeComfortViewportRadius(new Vector2(0.5f, 0.5f), new Vector2(1.20f, 1.20f) * hugeOrbit.x * 0.5f, SpatialCrystalStage3D.PremiumComfortViewportSafeMargin) + 0.0001f, "Premium formation adaptive radius must remain inside the 15% safe viewport margin.");

            Require(DiamondFocusSettings.GetMaterialModeLabel(DiamondCrystalMaterialMode.FuturisticPlastic) == "Opal Prism Glass", "Former plastic material mode must resolve to a premium glass label.");
            CrystalModeProfile opalProfile = CrystalModeLibrary.Resolve(DiamondCrystalMaterialMode.FuturisticPlastic, 0, false, 1f);
            Require(opalProfile.Surface.Family == CrystalSurfaceFamily.Gemstone, "Former plastic material mode must resolve to an authored premium glass/gem surface profile.");
            CrystalModeProfile mercuryProfile = CrystalModeLibrary.Resolve(DiamondCrystalMaterialMode.Mercury, 0, false, 1f);
            Require(mercuryProfile.Surface.Family == CrystalSurfaceFamily.Metal && mercuryProfile.Surface.Metallic > 0.9f, "Mercury material mode must remain a strong premium mirror-metal profile.");

            BenchmarkMetrics metrics = new BenchmarkMetrics();
            metrics.Sample(0.02f);
            metrics.Sample(0.01f);
            Require(Mathf.Approximately(metrics.AverageFramesPerSecond(), 2f / 0.03f), "Benchmark average FPS must be derived from total frames and measured time.");
            Require(Mathf.Approximately(metrics.PeakFramesPerSecond(), 100f), "Benchmark peak FPS must retain the highest instantaneous sample.");
            Require(Mathf.Approximately(metrics.OnePercentLowFramesPerSecond(), 50f), "Benchmark 1% low FPS must retain the low percentile sample.");
            Require(BenchmarkController.ResolvePhaseName(0f).Contains("Classic"), "Benchmark must begin with its Classic visual-performance phase.");
            Require(BenchmarkController.ResolvePhaseName(7f).Contains("Premium"), "Benchmark must visibly identify its Premium crystal phase.");
            Require(BenchmarkController.ResolvePhaseName(56f).Contains("7D"), "Benchmark must visibly identify its final 7D phase.");

            bool initialCleanView = director.State.CleanViewEnabled;
            director.Dispatch(KaleidoscopeCommand.SetCleanViewEnabled(false));
            director.Dispatch(KaleidoscopeCommand.ToggleCleanView());
            Require(director.State.CleanViewEnabled, "H clean-view command route must hide non-essential overlay presentation.");
            director.Dispatch(KaleidoscopeCommand.ToggleCleanView());
            Require(!director.State.CleanViewEnabled, "H clean-view command route must restore non-essential overlay presentation.");
            director.Dispatch(KaleidoscopeCommand.SetCleanViewEnabled(initialCleanView), KaleidoscopeCommandOrigin.Restore);
        }

        private static void ValidateCuratedDemoTexturePlayback()
        {
            DemoContentCatalog catalog = DemoContentCatalog.LoadDefault();
            Require(catalog != null && catalog.HasImages(), "Demo content catalog must include curated benchmark/replay images.");
            Require(catalog.HasMeditationAudio() && catalog.MeditationAudioPlaylist.Length >= 15, "Demo content catalog must expose the authored Meditation audio playlist.");
            Require(AudioReactiveModule.ResolveWrappedTrackIndex(0, 3) == 0
                && AudioReactiveModule.ResolveWrappedTrackIndex(1, 3) == 1
                && AudioReactiveModule.ResolveWrappedTrackIndex(2, 3) == 2
                && AudioReactiveModule.ResolveWrappedTrackIndex(3, 3) == 0,
                "Meditation playlist sequence must advance 1, 2, 3, then loop to 1.");

            ImageSlideshowController slideshow = new ImageSlideshowController();
            string temporaryFolder = Path.Combine(Path.GetTempPath(), "Kaleidoscope2_UserSlideshowSmoke");
            string temporaryImagePathA = Path.Combine(temporaryFolder, "Slide_A.png");
            string temporaryImagePathB = Path.Combine(temporaryFolder, "Slide_B.png");
            try
            {
                slideshow.Configure(30f, 0.01f, false, true);
                slideshow.SetImageAssets(catalog.DemoImages);
                Require(slideshow.IsActive, "Curated DemoContent assets must start the slideshow.");
                Require(slideshow.SourceTexture == catalog.DemoImages[0], "Curated DemoContent assets must be sampled directly without readable-texture cloning.");
                for (int index = 1; index < catalog.DemoImages.Length; index++)
                {
                    slideshow.NextImage();
                    slideshow.Tick(0.02f);
                    Require(slideshow.SourceTexture == catalog.DemoImages[index], "Curated DemoContent slideshow must use every catalog image in authored order.");
                }

                slideshow.NextImage();
                slideshow.Tick(0.02f);
                Require(slideshow.SourceTexture == catalog.DemoImages[0], "Curated DemoContent slideshow must loop from its last image to its first.");

                Texture2D userImage = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    Directory.CreateDirectory(temporaryFolder);
                    userImage.SetPixel(0, 0, Color.cyan);
                    userImage.SetPixel(1, 0, Color.blue);
                    userImage.SetPixel(0, 1, Color.white);
                    userImage.SetPixel(1, 1, Color.black);
                    userImage.Apply(false, false);
                    File.WriteAllBytes(temporaryImagePathA, userImage.EncodeToPNG());
                    userImage.SetPixel(0, 0, Color.magenta);
                    userImage.Apply(false, false);
                    File.WriteAllBytes(temporaryImagePathB, userImage.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(userImage);
                }

                slideshow.SetImageFolder(temporaryFolder);
                Require(slideshow.IsActive && slideshow.ImageCount == 2 && slideshow.SourceTexture != null, "User-folder slideshow must still enumerate supported filesystem images.");
                slideshow.NextImage();
                slideshow.Tick(0.02f);
                Require(slideshow.CurrentImageIndex == 1, "User-folder slideshow must advance through filesystem images.");
                slideshow.NextImage();
                slideshow.Tick(0.02f);
                Require(slideshow.CurrentImageIndex == 0, "User-folder slideshow must retain looping behavior.");
            }
            finally
            {
                slideshow.Dispose();
                if (Directory.Exists(temporaryFolder))
                {
                    Directory.Delete(temporaryFolder, true);
                }
            }
        }

        private static void ValidateSettingsPersistenceFoundation(Transform settingsPanel)
        {
            KaelisMenuToggleControl autoSaveToggle = FindChild<KaelisMenuToggleControl>(settingsPanel, "AutoSaveSettingsToggle");
            Require(autoSaveToggle != null, "Settings panel must expose Auto Save Settings through the shared toggle control.");
            Require(autoSaveToggle.Button != null && autoSaveToggle.Button.interactable, "Auto Save Settings must be a real persistence binding, not a reserved placeholder.");
            Button resetSettingsButton = FindChild<Button>(settingsPanel, "ResetSettingsCommand");
            Require(resetSettingsButton != null && !resetSettingsButton.interactable, "Reset Settings must remain unavailable until a confirmation flow exists.");

            string temporaryRoot = Path.Combine(Path.GetTempPath(), "KaelisSettingsPersistenceSmoke_" + Guid.NewGuid().ToString("N"));
            string settingsPath = Path.Combine(temporaryRoot, "kaelis-settings.json");
            GameObject owner = new GameObject("KaelisSettingsPersistenceSmoke");

            try
            {
                KaleidoscopeDirector director = owner.AddComponent<KaleidoscopeDirector>();
                DiamondFocusModule diamondModule = owner.AddComponent<DiamondFocusModule>();
                SettingsPersistenceService service = owner.AddComponent<SettingsPersistenceService>();
                service.SetStoragePathForTests(settingsPath);
                director.RegisterModule(diamondModule);
                service.Configure(director);
                director.RegisterModule(service);

                Require(service.Data.version == KaelisSettingsData.CurrentSchemaVersion, "Missing settings file must create current-version safe defaults.");
                Require(!service.AutoSaveEnabled, "Auto Save Settings must default to OFF.");
                Require(File.Exists(settingsPath), "Missing settings file must be created with safe defaults on first startup.");
                Require(File.ReadAllText(settingsPath).Contains("\"version\": " + KaelisSettingsData.CurrentSchemaVersion.ToString()), "Created settings file must use the current schema version.");

                director.Dispatch(KaleidoscopeCommand.SaveSettings());
                Require(File.Exists(settingsPath), "SaveSettings command must create the settings file.");

                director.Dispatch(KaleidoscopeCommand.SetSettingsAutoSaveEnabled(true));
                Require(service.AutoSaveEnabled, "SetSettingsAutoSaveEnabled must update the owned settings data.");
                Require(File.ReadAllText(settingsPath).Contains("\"autoSaveSettings\": true"), "Auto Save Settings flag must persist when turned on.");

                director.Dispatch(KaleidoscopeCommand.SetVisualMode(KaleidoscopeVisualMode.SevenD));
                director.Dispatch(KaleidoscopeCommand.SetCrystalSimulationMode(CrystalRenderMode.RealMesh3D));
                director.Dispatch(KaleidoscopeCommand.SetClassicCrystalScalePercent(155f));
                director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalScalePercent(178f));
                director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalShape(PremiumCrystalShapeType.StarPrism));
                director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode.AbsoluteMirror));
                director.Dispatch(KaleidoscopeCommand.SetCrystalDebugMode(DiamondCrystalDebugMode.ReflectionOnly));
                director.Dispatch(KaleidoscopeCommand.SetCrystalDebugEffect(CrystalDebugEffectType.Halo));
                KaelisMenuLocalizationService.SetLanguage(KaelisMenuLanguage.German, true);
                string persistedJson = File.ReadAllText(settingsPath);
                KaelisSettingsData persistedData = KaelisSettingsData.Sanitize(JsonUtility.FromJson<KaelisSettingsData>(persistedJson));
                Require(persistedData.visual.activeVisualMode == (int)KaleidoscopeVisualMode.SevenD, "Auto Save must persist the final active visual mode.");
                Require(Mathf.Approximately(persistedData.visual.classicCrystalScalePercent, 155f), "Auto Save must persist Classic crystal scale percent.");
                Require(Mathf.Approximately(persistedData.visual.premiumCrystalScalePercent, 178f), "Auto Save must persist Premium crystal scale percent.");
                Require(persistedData.visual.premiumShape == (int)PremiumCrystalShapeType.StarPrism, "Auto Save must persist selected Premium shape.");
                Require(persistedData.visual.premiumOpticalMode == (int)PremiumCrystalOpticalMode.AbsoluteMirror, "Auto Save must persist selected Premium optical mode.");
                Require(persistedData.ui.menuLanguage == KaelisMenuLanguage.German.ToString(), "Auto Save must persist connected menu language.");

                GameObject reloadOwner = new GameObject("KaelisSettingsPersistenceReloadSmoke");
                try
                {
                    KaleidoscopeDirector reloadDirector = reloadOwner.AddComponent<KaleidoscopeDirector>();
                    DiamondFocusModule reloadDiamondModule = reloadOwner.AddComponent<DiamondFocusModule>();
                    SettingsPersistenceService reloadService = reloadOwner.AddComponent<SettingsPersistenceService>();
                    reloadService.SetStoragePathForTests(settingsPath);
                    reloadDirector.RegisterModule(reloadDiamondModule);
                    reloadService.Configure(reloadDirector);
                    reloadDirector.RegisterModule(reloadService);
                    Require(reloadDirector.State.ActiveVisualMode == KaleidoscopeVisualMode.SevenD, "Saved active visual mode must restore on startup.");
                    Require(reloadDirector.State.DiamondFocusSettings.CrystalSimulationMode == CrystalRenderMode.RealMesh3D, "Saved Classic/Premium crystal render mode must restore on startup.");
                    Require(Mathf.Approximately(reloadDirector.State.DiamondFocusSettings.ClassicCrystalScalePercent, 155f), "Saved Classic crystal scale must restore on startup.");
                    Require(Mathf.Approximately(reloadDirector.State.DiamondFocusSettings.PremiumCrystalScalePercent, 178f), "Saved Premium crystal scale must restore on startup.");
                    Require(reloadDirector.State.DiamondFocusSettings.PremiumCrystalShape == PremiumCrystalShapeType.StarPrism, "Saved Premium shape must restore on startup.");
                    Require(reloadDirector.State.DiamondFocusSettings.ActivePremiumCrystalOpticalMode == PremiumCrystalOpticalMode.AbsoluteMirror, "Saved Premium optical mode must restore on startup.");
                    Require(reloadDirector.State.DiamondFocusSettings.DebugMode == DiamondCrystalDebugMode.ReflectionOnly, "Saved debug mode must restore on startup.");
                    Require(reloadDirector.State.DiamondFocusSettings.CrystalDebugEffects.SelectedEffect == CrystalDebugEffectType.Halo, "Saved debug effect must restore on startup.");
                    Require(KaelisMenuLocalizationService.CurrentLanguage == KaelisMenuLanguage.German, "Saved menu language must restore on startup.");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(reloadOwner);
                }

                service.Data.startup.autoSaveSettings = false;
                service.LoadSettings();
                Require(service.AutoSaveEnabled, "Auto Save Settings flag must load from disk on startup.");

                File.WriteAllText(settingsPath, "{ this is not valid json");
                service.LoadSettings();
                Require(service.Data.version == KaelisSettingsData.CurrentSchemaVersion && !service.AutoSaveEnabled, "Corrupted settings JSON must not crash and must fall back to safe defaults.");
                Require(Directory.GetFiles(temporaryRoot, "kaelis-settings.json.corrupt.*.bak").Length > 0, "Corrupted settings JSON must be backed up before defaults are rewritten.");

                File.WriteAllText(settingsPath, "{\"version\":0,\"display\":{\"resolutionWidth\":-50,\"resolutionHeight\":-10,\"windowMode\":99},\"ui\":{\"scalePercent\":999},\"audio\":{\"masterVolumePercent\":-5,\"menuVolumePercent\":250,\"demoVolumePercent\":120},\"startup\":{\"autoSaveSettings\":true}}");
                service.LoadSettings();
                Require(service.Data.version == KaelisSettingsData.CurrentSchemaVersion, "Old settings data must be upgraded to the current schema version.");
                Require(service.Data.visual.activeVisualMode == (int)KaleidoscopeVisualMode.Classic, "Missing old visual settings must resolve to safe defaults.");
                Require(service.Data.display.resolutionWidth == 0 && service.Data.display.resolutionHeight == 0 && service.Data.display.windowMode == 3, "Loaded display placeholders must be sanitized.");
                Require(Mathf.Approximately(service.Data.ui.scalePercent, 160f), "Loaded UI scale placeholder must be clamped to its safe range.");
                Require(Mathf.Approximately(service.Data.audio.masterVolumePercent, 0f)
                    && Mathf.Approximately(service.Data.audio.menuVolumePercent, 100f)
                    && Mathf.Approximately(service.Data.audio.demoVolumePercent, 100f), "Loaded audio placeholders must be clamped to safe percent ranges.");
                Require(service.AutoSaveEnabled, "Sanitized old settings must preserve valid Auto Save state.");

                director.Dispatch(KaleidoscopeCommand.ResetSettings());
                Require(!service.AutoSaveEnabled, "ResetSettings command must restore safe defaults in memory.");
                Require(File.ReadAllText(settingsPath).Contains("\"autoSaveSettings\": false"), "ResetSettings command must save safe defaults through the persistence owner.");
                Require(!File.ReadAllText(settingsPath).Contains("RecordedAction"), "Settings persistence must not write Replay/InputRecorder raw action history.");
            }
            finally
            {
                KaelisMenuLocalizationService.SetLanguage(KaelisMenuLanguage.English, true);
                UnityEngine.Object.DestroyImmediate(owner);
                if (Directory.Exists(temporaryRoot))
                {
                    Directory.Delete(temporaryRoot, true);
                }
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
            Require((KeyCode)inputType.GetField("toggleCleanViewKey", fields).GetValue(input) == KeyCode.H, "H must toggle non-essential overlay visibility only.");
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

        private static void ValidatePremiumExposureResolution()
        {
            DiamondFocusSettings premiumSettings = new DiamondFocusSettings();
            premiumSettings.SetCrystalSimulationMode(CrystalRenderMode.RealMesh3D);
            premiumSettings.ResetPremiumCrystalOpticalControls();
            CrystalSharedSettings premiumResolved = new CrystalSharedSettings();
            premiumResolved.SyncFromDiamond(premiumSettings);

            Require(premiumResolved.Intensity < 5f, "Default Premium presentation must use a restrained resolved intensity.");
            Require(premiumResolved.InternalBrightness < 0.6f, "Default Premium presentation must keep internal brightness below washout levels.");
            Require(premiumResolved.FacetFire < 0.7f, "Default Premium presentation must retain readable facet sparkle without white clipping.");

            DiamondFocusSettings classicSettings = new DiamondFocusSettings();
            classicSettings.SetCrystalSimulationMode(CrystalRenderMode.Billboard2D);
            classicSettings.ResetPremiumCrystalOpticalControls();
            CrystalSharedSettings classicResolved = new CrystalSharedSettings();
            classicResolved.SyncFromDiamond(classicSettings);
            float classicBrightness01 = Mathf.InverseLerp(0.10f, 3f, classicSettings.PremiumOpticsBrightness);
            float expectedClassicIntensity = classicSettings.CrystalLightRigSettings.LightIntensity
                * Mathf.Lerp(0.55f, 2.35f, classicBrightness01)
                + Mathf.Clamp01(classicSettings.PremiumOpticsBloomGlow / 5f) * 3f;
            Require(Mathf.Approximately(classicResolved.Intensity, expectedClassicIntensity), "Classic resolved crystal intensity must retain its existing formula.");

            premiumSettings.SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode.AbsoluteMirror);
            premiumResolved.SyncFromDiamond(premiumSettings);
            Require(Mathf.Approximately(premiumResolved.DirectTransmission, 0f)
                && Mathf.Approximately(premiumResolved.AbsoluteMirrorStrength, 1f),
                "Premium exposure reduction must preserve the Absolute Mirror opacity guard.");
        }

        private static void ValidateSectionScrollLayoutStandard(params Transform[] sectionPanels)
        {
            for (int index = 0; index < sectionPanels.Length; index++)
            {
                Transform sectionPanel = sectionPanels[index];
                Require(sectionPanel != null, "Section panel missing during scroll layout validation.");

                ScrollRect scrollRect = sectionPanel.GetComponent<ScrollRect>();
                Require(scrollRect != null, sectionPanel.name + " must use the shared section ScrollRect.");
                Require(scrollRect.vertical && !scrollRect.horizontal, sectionPanel.name + " must use vertical-only section scrolling.");
                Require(scrollRect.movementType == ScrollRect.MovementType.Clamped, sectionPanel.name + " scroll movement must stay clamped.");
                Require(scrollRect.scrollSensitivity >= 20f, sectionPanel.name + " must keep responsive mouse-wheel scrolling.");

                RectTransform viewport = (RectTransform)FindChild<Transform>(sectionPanel, "SectionViewport");
                Require(viewport != null && scrollRect.viewport == viewport, sectionPanel.name + " must bind its ScrollRect viewport to SectionViewport.");
                Require(viewport.GetComponent<RectMask2D>() != null, sectionPanel.name + " viewport must mask scrollable content.");
                RectTransform header = (RectTransform)FindChild<Transform>(sectionPanel, "SectionHeader");
                Require(header != null, sectionPanel.name + " must expose a fixed header.");
                float headerSafeGap = -viewport.offsetMax.y - header.sizeDelta.y;
                Require(headerSafeGap >= 72f && headerSafeGap <= 110f, sectionPanel.name + " must keep a 72-110 px header safe zone before scroll content. Actual: " + headerSafeGap.ToString("0.0"));
                float expectedViewportTop = -(header.sizeDelta.y + headerSafeGap);
                Require(Mathf.Abs(viewport.offsetMax.y - expectedViewportTop) <= 0.5f, sectionPanel.name + " viewport top must match the fixed header plus safe zone. Expected: " + expectedViewportTop.ToString("0.0") + " Actual: " + viewport.offsetMax.y.ToString("0.0"));
                Transform headerSafeZone = FindChild<Transform>(sectionPanel, "HeaderSafeZone");
                Require(headerSafeZone != null, sectionPanel.name + " must expose the shared HeaderSafeZone object.");
                RectTransform headerSafeZoneRect = headerSafeZone as RectTransform;
                Require(headerSafeZoneRect != null && Mathf.Abs(headerSafeZoneRect.sizeDelta.y - headerSafeGap) <= 0.5f, sectionPanel.name + " HeaderSafeZone geometry must match the actual viewport gap.");
                Graphic headerSafeZoneGraphic = headerSafeZone.GetComponent<Graphic>();
                Require(headerSafeZoneGraphic == null || !headerSafeZoneGraphic.raycastTarget, sectionPanel.name + " HeaderSafeZone must not intercept close-button interaction.");

                Transform scrollHitArea = FindChild<Transform>(sectionPanel, "SectionScrollHitArea");
                Image scrollHitImage = scrollHitArea != null ? scrollHitArea.GetComponent<Image>() : null;
                Require(scrollHitImage != null && scrollHitImage.raycastTarget, sectionPanel.name + " must provide a transparent scroll hit area.");
                Require(scrollHitArea.parent == viewport, sectionPanel.name + " scroll hit area must stay inside the masked viewport.");

                RectTransform content = (RectTransform)FindChild<Transform>(sectionPanel, "SectionContent");
                Require(content != null && scrollRect.content == content, sectionPanel.name + " must bind ScrollRect content to SectionContent.");
                Require(content.parent == viewport, sectionPanel.name + " scroll content must stay inside the masked viewport.");
                VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
                Require(contentLayout != null, sectionPanel.name + " content must use the shared vertical layout group.");
                Require(contentLayout.padding.top >= 16, sectionPanel.name + " content must keep breathing room below the viewport mask.");
                Require(content.GetComponent<ContentSizeFitter>() != null, sectionPanel.name + " content must size to its children.");

                Transform closeButton = FindChild<Transform>(sectionPanel, "SectionCloseButton");
                Require(closeButton != null, sectionPanel.name + " must expose a fixed close button.");
                Require(header.GetSiblingIndex() > viewport.GetSiblingIndex(), sectionPanel.name + " header must render above the scroll viewport.");
                Require(closeButton.parent == header && closeButton.GetSiblingIndex() == header.childCount - 1, sectionPanel.name + " close button must stay above header text.");

                Scrollbar scrollbar = FindChild<Scrollbar>(sectionPanel, "VerticalScrollbar");
                Require(scrollbar != null && scrollRect.verticalScrollbar == scrollbar, sectionPanel.name + " must use the shared vertical scrollbar.");
                Require(scrollbar.direction == Scrollbar.Direction.BottomToTop, sectionPanel.name + " scrollbar direction must match mouse-wheel convention.");
                Require(scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHide, sectionPanel.name + " scrollbar must auto-hide without expanding over the fixed header.");
                RectTransform scrollbarRect = (RectTransform)scrollbar.transform;
                float scrollbarSafeGap = -scrollbarRect.offsetMax.y - header.sizeDelta.y;
                Require(scrollbarSafeGap >= 72f && scrollbarSafeGap <= 110f, sectionPanel.name + " scrollbar must share the header safe zone.");
                Require(Mathf.Abs(scrollbarSafeGap - headerSafeGap) <= 0.5f, sectionPanel.name + " scrollbar top must align with the viewport top.");
            }
        }

        private static void ValidateNoMenuTextArtifacts(Transform canvasTransform)
        {
            TMP_Text[] texts = canvasTransform.GetComponentsInChildren<TMP_Text>(true);
            for (int index = 0; index < texts.Length; index++)
            {
                TMP_Text text = texts[index];
                Require(!text.text.Contains("\u25A1") && !text.text.Contains("\uFFFD"), text.name + " must not contain missing-glyph placeholder characters.");
                string normalized = text.text.Replace("\r\n", "\n");
                string[] lines = normalized.Split('\n');
                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    string line = lines[lineIndex].Trim();
                    Require(!(line.StartsWith("|", StringComparison.Ordinal) && line.EndsWith("|", StringComparison.Ordinal)), text.name + " must not display raw markdown table rows.");
                }
            }
        }

        private static void ValidateSectionCloseAndEscapeNavigation(Transform canvasTransform, KaleidoscopeDirector director, string openerButtonName, Transform sectionPanel)
        {
            Button opener = FindChild<Button>(canvasTransform, openerButtonName);
            Require(opener != null, openerButtonName + " opener missing for close/Escape validation.");
            opener.onClick.Invoke();
            Require(sectionPanel.gameObject.activeSelf, sectionPanel.name + " must open before close validation.");
            Require(KaelisMenuSectionGeometryDiagnostics.ValidateSectionGeometry((RectTransform)sectionPanel, sectionPanel.name, "SmokeTest open via " + openerButtonName, true, false), sectionPanel.name + " live geometry must keep content below the fixed header.");
            ValidateOpenSectionCloseButton(sectionPanel);

            opener.onClick.Invoke();
            Require(sectionPanel.gameObject.activeSelf, sectionPanel.name + " must open before Escape validation.");
            Require(KaelisMenuSectionGeometryDiagnostics.ValidateSectionGeometry((RectTransform)sectionPanel, sectionPanel.name, "SmokeTest Escape via " + openerButtonName, false, false), sectionPanel.name + " live geometry must remain valid after reopening.");
            director.Dispatch(KaleidoscopeCommand.ReturnToInitialMenu());
            Require(!sectionPanel.gameObject.activeSelf, sectionPanel.name + " must close through Escape/ReturnToInitialMenu routing.");
            Require(FindChild<Transform>(canvasTransform, "SafeFrame").gameObject.activeSelf, "Escape/ReturnToInitialMenu must restore the root menu frame.");
        }

        private static void ValidateNestedDemoSectionCloseAndEscape(Transform canvasTransform, KaleidoscopeDirector director, Transform demoPanel, string nestedButtonName, Transform sectionPanel)
        {
            Button demoOpener = FindChild<Button>(canvasTransform, "DemoModeButton");
            Require(demoOpener != null, "Demo button missing for nested close/Escape validation.");
            demoOpener.onClick.Invoke();
            Require(demoPanel.gameObject.activeSelf, "Demo panel must open before nested section validation.");

            Button nestedOpener = FindChild<Button>(demoPanel, nestedButtonName);
            Require(nestedOpener != null, nestedButtonName + " missing for nested section validation.");
            nestedOpener.onClick.Invoke();
            Require(sectionPanel.gameObject.activeSelf, sectionPanel.name + " must open from Demo before close validation.");
            Require(KaelisMenuSectionGeometryDiagnostics.ValidateSectionGeometry((RectTransform)sectionPanel, sectionPanel.name, "SmokeTest nested open via " + nestedButtonName, true, false), sectionPanel.name + " nested live geometry must keep content below the fixed header.");
            ValidateOpenSectionCloseButton(sectionPanel);

            demoOpener.onClick.Invoke();
            nestedOpener = FindChild<Button>(demoPanel, nestedButtonName);
            nestedOpener.onClick.Invoke();
            Require(sectionPanel.gameObject.activeSelf, sectionPanel.name + " must open from Demo before Escape validation.");
            Require(KaelisMenuSectionGeometryDiagnostics.ValidateSectionGeometry((RectTransform)sectionPanel, sectionPanel.name, "SmokeTest nested Escape via " + nestedButtonName, false, false), sectionPanel.name + " nested live geometry must remain valid after reopening.");
            director.Dispatch(KaleidoscopeCommand.ReturnToInitialMenu());
            Require(!sectionPanel.gameObject.activeSelf, sectionPanel.name + " must close through Escape/ReturnToInitialMenu routing.");
            Require(FindChild<Transform>(canvasTransform, "SafeFrame").gameObject.activeSelf, "Escape/ReturnToInitialMenu must restore the root menu frame after nested section.");
        }

        private static void ValidateOpenSectionCloseButton(Transform sectionPanel)
        {
            Button closeButton = FindChild<Button>(sectionPanel, "SectionCloseButton");
            Require(closeButton != null && closeButton.interactable, sectionPanel.name + " close button must be clickable.");
            closeButton.onClick.Invoke();
            Require(!sectionPanel.gameObject.activeSelf, sectionPanel.name + " close button must close the active section.");
        }

        private static void ValidateSliderAudioRegression(MenuAudioFeedbackController audioFeedback, params Slider[] sliders)
        {
            Require(audioFeedback != null && audioFeedback.Settings != null, "Slider audio regression requires the central menu audio feedback controller.");
            Require(audioFeedback.Settings.ButtonForwardClip != null, "Slider forward audio clip must remain assigned.");
            Require(audioFeedback.Settings.ButtonBackClip != null, "Slider back audio clip must remain assigned.");
            Require(audioFeedback.Settings.ButtonForwardClip != audioFeedback.Settings.ButtonBackClip, "Slider forward/back audio clips must remain distinct.");

            Type feedbackType = typeof(MenuAudioFeedbackController).GetNestedType("FeedbackType", BindingFlags.NonPublic);
            MethodInfo resolveClip = typeof(MenuAudioFeedbackController).GetMethod("ResolveClip", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(feedbackType != null && resolveClip != null, "Slider audio clip resolution internals must remain inspectable for regression coverage.");
            object sliderForward = Enum.Parse(feedbackType, "SliderForward");
            object sliderBack = Enum.Parse(feedbackType, "SliderBack");
            Require(ReferenceEquals(resolveClip.Invoke(audioFeedback, new[] { sliderForward }), audioFeedback.Settings.ButtonForwardClip), "SliderForward feedback must resolve to the forward clip.");
            Require(ReferenceEquals(resolveClip.Invoke(audioFeedback, new[] { sliderBack }), audioFeedback.Settings.ButtonBackClip), "SliderBack feedback must resolve to the back clip.");

            FieldInfo boundSliderIdsField = typeof(MenuAudioFeedbackController).GetField("boundSliderIds", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(boundSliderIdsField != null, "Slider audio binding registry must remain available for duplicate-binding regression coverage.");
            HashSet<int> boundSliderIds = boundSliderIdsField.GetValue(audioFeedback) as HashSet<int>;
            Require(boundSliderIds != null, "Slider audio binding registry must be a bounded HashSet.");
            for (int index = 0; index < sliders.Length; index++)
            {
                Require(sliders[index] != null, "Slider audio regression received a missing slider.");
                Require(boundSliderIds.Contains(sliders[index].GetInstanceID()), sliders[index].name + " must be bound to directional slider audio.");
            }

            int countBefore = boundSliderIds.Count;
            MethodInfo bindSlider = typeof(MenuAudioFeedbackController).GetMethod("BindSlider", BindingFlags.Static | BindingFlags.NonPublic);
            Require(bindSlider != null, "Slider audio BindSlider route must remain centralized.");
            bindSlider.Invoke(null, new object[] { sliders[0] });
            Require(boundSliderIds.Count == countBefore, "Rebinding the same slider must not duplicate directional audio listeners.");
        }

        private static void ValidateAboutVisualHarmony(Transform canvasTransform, Transform aboutPanel)
        {
            TMP_Text[] texts = aboutPanel.GetComponentsInChildren<TMP_Text>(true);
            for (int index = 0; index < texts.Length; index++)
            {
                TMP_Text text = texts[index];
                Require(!text.text.Contains("\u25A1") && !text.text.Contains("\uFFFD"), text.name + " must not contain missing-glyph placeholder characters.");
                Require(text.overflowMode != TextOverflowModes.Truncate, text.name + " must not silently truncate About content.");
                if (ContainsCyrillic(text.text))
                {
                    Require(text.font != null && text.font.name.Contains("Inter"), text.name + " must use Inter for Cyrillic/localized UI text.");
                }

                string normalized = text.text.Replace("\r\n", "\n");
                string[] lines = normalized.Split('\n');
                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    string line = lines[lineIndex].Trim();
                    Require(!(line.StartsWith("|", StringComparison.Ordinal) && line.EndsWith("|", StringComparison.Ordinal)), text.name + " must not display raw markdown table rows.");
                }
            }

            Image panelBackground = aboutPanel.GetComponent<Image>();
            Require(panelBackground != null && panelBackground.color.a >= 0.42f && panelBackground.color.a <= 0.58f, "About full-screen panel must use the shared large-section overlay opacity range.");

            Require(FindChild<Transform>(aboutPanel, "AboutLuxuryContentPanel") == null, "About must not keep the old single web-page style content panel.");
            string[] cardNames = { "AboutHeroCard", "AboutStoryCard", "AboutCreditsCard" };
            for (int cardIndex = 0; cardIndex < cardNames.Length; cardIndex++)
            {
                Transform card = FindChild<Transform>(aboutPanel, cardNames[cardIndex]);
                Image cardBackground = card != null ? card.GetComponent<Image>() : null;
                Require(cardBackground != null, cardNames[cardIndex] + " must have a shared menu card background.");
                Require(cardBackground.color.a <= 0.42f, cardNames[cardIndex] + " must preserve background beam/prism visibility.");
                Require(!cardBackground.raycastTarget, cardNames[cardIndex] + " background must not block close or scroll routing.");
            }

            Transform scrollHitArea = FindChild<Transform>(aboutPanel, "SectionScrollHitArea");
            Image scrollHitImage = scrollHitArea != null ? scrollHitArea.GetComponent<Image>() : null;
            Require(scrollHitImage != null && scrollHitImage.raycastTarget, "Section viewport must provide a dedicated scroll hit area.");
            RectTransform viewportRect = (RectTransform)FindChild<Transform>(aboutPanel, "SectionViewport");
            RectTransform header = (RectTransform)FindChild<Transform>(aboutPanel, "SectionHeader");
            Require(header != null && viewportRect != null && viewportRect.offsetMax.y <= -92f, "Section scroll hit area must stay below the fixed header.");

            Transform beamLayer = FindChild<Transform>(canvasTransform, "PremiumMenuMotionStripes");
            Transform atmosphereLayer = FindChild<Transform>(canvasTransform, "MenuAtmosphereFX");
            PremiumMenuPrismReactionController prismReaction = FindChild<PremiumMenuPrismReactionController>(canvasTransform, "PrismReaction");
            Require(beamLayer != null && beamLayer.gameObject.activeInHierarchy, "About section must not disable premium menu beam movement.");
            Require(atmosphereLayer != null && atmosphereLayer.gameObject.activeInHierarchy, "About section must not disable optical atmosphere.");
            Require(prismReaction != null && prismReaction.gameObject.activeInHierarchy, "About section must keep prism/rainbow reaction active.");
        }

        private static void ValidateAboutCloseRaycast(Transform canvasTransform, Transform aboutPanel)
        {
            GraphicRaycaster raycaster = canvasTransform.GetComponent<GraphicRaycaster>();
            Require(raycaster != null, "Main menu canvas must have a GraphicRaycaster for close-button diagnostics.");
            GameObject temporaryEventSystem = null;
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                temporaryEventSystem = new GameObject("KaelisSmokeTestEventSystem");
                eventSystem = temporaryEventSystem.AddComponent<EventSystem>();
                temporaryEventSystem.AddComponent<StandaloneInputModule>();
            }

            try
            {
                Transform closeTransform = FindChild<Transform>(aboutPanel, "SectionCloseButton");
                Require(closeTransform != null, "About section close button missing.");
                Canvas.ForceUpdateCanvases();

                RectTransform closeRect = (RectTransform)closeTransform;
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, closeRect.TransformPoint(closeRect.rect.center));
                PointerEventData pointerData = new PointerEventData(eventSystem)
                {
                    position = screenPoint
                };

                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(pointerData, results);
                string report = BuildRaycastReport(results);
                Debug.Log("[KAELIS Menu Raycast] About close button center hits:\n" + report);
                if (results.Count == 0 && Application.isBatchMode)
                {
                    Debug.LogWarning("[KAELIS Menu Raycast] Batchmode returned no UI hits for the About close center. Falling back to structural close-button validation.");
                    Button batchCloseButton = closeTransform.GetComponent<Button>();
                    Require(batchCloseButton != null && batchCloseButton.interactable, "About close button must be an interactable Button.");
                    Require(batchCloseButton.targetGraphic != null && batchCloseButton.targetGraphic.raycastTarget, "About close button target graphic must remain raycastable.");
                    Require(closeTransform.GetSiblingIndex() == closeTransform.parent.childCount - 1, "About close button must remain above header decorative children.");
                    return;
                }

                Require(results.Count > 0, "About close-button raycast returned no UI hits.");
                Transform firstHit = results[0].gameObject.transform;
                Require(firstHit == closeTransform || firstHit.IsChildOf(closeTransform), "About close button is blocked. First raycast hit was " + GetHierarchyPath(firstHit) + ". Full hits:\n" + report);

                Button closeButton = closeTransform.GetComponent<Button>();
                Require(closeButton != null && closeButton.interactable, "About close button must be an interactable Button.");
            }
            finally
            {
                if (temporaryEventSystem != null)
                {
                    UnityEngine.Object.DestroyImmediate(temporaryEventSystem);
                }
            }
        }

        private static void ValidateMenuTransparencyStandard(Transform canvasTransform, params Transform[] standardPanels)
        {
            for (int index = 0; index < standardPanels.Length; index++)
            {
                Image panelBackground = standardPanels[index] != null ? standardPanels[index].GetComponent<Image>() : null;
                Require(panelBackground != null, standardPanels[index].name + " must have a glass background image.");
                Require(!panelBackground.raycastTarget, standardPanels[index].name + " decorative background must not block controls.");
                Require(panelBackground.color.a >= 0.42f && panelBackground.color.a <= 0.58f, standardPanels[index].name + " large section overlay opacity must stay in the shared 0.42-0.58 range.");

                Image haze = FindChild<Image>(standardPanels[index], "PanelHaze");
                Require(haze != null && haze.color.a >= 0.02f && haze.color.a <= 0.10f, standardPanels[index].name + " haze opacity must stay in the shared 0.02-0.10 range.");
            }

            Transform contentSelection = FindChild<Transform>(canvasTransform, "ContentSelectionFlow");
            Image contentSelectionBackground = contentSelection != null ? contentSelection.GetComponent<Image>() : null;
            Require(contentSelection != null, "Content selection flow must exist for shared menu standard validation.");
            Require(contentSelectionBackground != null && contentSelectionBackground.color.a >= 0.42f && contentSelectionBackground.color.a <= 0.58f, "Content selection panel must use the shared large-overlay opacity range.");
            RectTransform contentSelectionSafeZone = (RectTransform)FindChild<Transform>(contentSelection, "ContentSelectionHeaderSafeZone");
            RectTransform contentSelectionDivider = (RectTransform)FindChild<Transform>(contentSelection, "ContentSelectionHeaderDivider");
            RectTransform imageSourceBlock = (RectTransform)FindChild<Transform>(contentSelection, "ImagesSourceBlock");
            Require(contentSelectionSafeZone != null, "Content selection panel must expose a header safe zone before source blocks.");
            Require(contentSelectionDivider != null, "Content selection panel must expose a header/subtitle divider.");
            Require(imageSourceBlock != null, "Content selection panel must expose its first source block for safe-zone validation.");
            float contentSelectionSafeGap = contentSelectionSafeZone.offsetMax.y - contentSelectionSafeZone.offsetMin.y;
            Require(contentSelectionSafeGap >= 72f && contentSelectionSafeGap <= 110f, "Content selection header safe zone must follow the shared 72-110 px standard. Actual: " + contentSelectionSafeGap.ToString("0.0"));
            Require(imageSourceBlock.offsetMax.y <= contentSelectionSafeZone.offsetMin.y + 0.5f, "Content selection first source block must begin below the header safe zone.");
            Graphic contentSelectionSafeZoneGraphic = contentSelectionSafeZone.GetComponent<Graphic>();
            Graphic contentSelectionDividerGraphic = contentSelectionDivider.GetComponent<Graphic>();
            Require(contentSelectionSafeZoneGraphic == null || !contentSelectionSafeZoneGraphic.raycastTarget, "Content selection safe zone must not block interaction.");
            Require(contentSelectionDividerGraphic == null || !contentSelectionDividerGraphic.raycastTarget, "Content selection header divider must not block interaction.");
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

        private static bool ContainsCyrillic(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                if ((character >= '\u0400' && character <= '\u04FF') || (character >= '\u0500' && character <= '\u052F'))
                {
                    return true;
                }
            }

            return false;
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
