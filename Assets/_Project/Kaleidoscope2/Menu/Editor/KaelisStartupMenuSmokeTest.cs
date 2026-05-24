using System;
using System.Reflection;
using Kaleidoscope2.Core;
using Kaleidoscope2.Menu;
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
            KaleidoscopeCommandType dispatchedType = KaleidoscopeCommandType.None;
            director.CommandDispatched += command => dispatchedType = command.Type;

            FindChild<Button>(canvasTransform, "ModesButton").onClick.Invoke();
            Require(modesPanel.gameObject.activeSelf, "Modes button must open Modes section.");
            Transform showcaseBlock = FindChild<Transform>(modesPanel, "ShowcaseRecordingBlock");
            Require(showcaseBlock != null, "Modes panel must replace Experimental with Showcase / Recording.");
            Require(FindChild<Button>(modesPanel, "ExperimentalComingSoonCommand") == null, "Experimental / Coming Soon row must be removed from Modes.");
            KaelisMenuToggleControl secondDisplayToggle = FindChild<KaelisMenuToggleControl>(showcaseBlock, "OutputToSecondDisplayToggle");
            Require(secondDisplayToggle != null, "Showcase / Recording must expose Output To Second Display toggle.");
            Require(FindChild<TMP_Text>(showcaseBlock, "SecondDisplayStatus") != null, "Showcase / Recording must show second display status.");
            Require(FindChild<Button>(showcaseBlock, "TestSecondDisplayButton") != null, "Showcase / Recording must expose Test Display action.");
            KaelisMenuToggleControl recordingToggle = FindChild<KaelisMenuToggleControl>(showcaseBlock, "CreateVideoClipToggle");
            Require(recordingToggle != null, "Showcase / Recording must expose Create Video Clip toggle.");
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

            FindChild<Button>(canvasTransform, "OpticsButton").onClick.Invoke();
            Require(!modesPanel.gameObject.activeSelf && opticsPanel.gameObject.activeSelf, "Optics button must switch to Optics section only.");
            KaelisMenuSliderControl brightnessSlider = FindChild<KaelisMenuSliderControl>(opticsPanel, "BrightnessSlider");
            Require(brightnessSlider != null, "Optics panel must expose a premium Brightness slider.");
            Require(Mathf.Approximately(brightnessSlider.MinValue, 0.10f) && Mathf.Approximately(brightnessSlider.MaxValue, 3.00f), "Brightness slider must use the expanded creative range.");
            Require(brightnessSlider.GetComponent<KaelisMenuInteractiveRow>().HasTooltipData, "Optics sliders must carry tooltip anchor data.");
            Require(brightnessSlider.GetComponent<KaelisMenuInteractiveRow>().TooltipKeys.Contains("Home / End"), "Sliders must show slider-specific keyboard hints.");
            Require(FindDescendant<Transform>(brightnessSlider.transform, "RightGold") != null, "Interactive hover frames must include the right frame edge.");
            Require(FindChild<KaelisMenuToggleControl>(opticsPanel, "CausticsToggle") != null, "Optics panel must expose a premium Caustics toggle.");
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
            FindChild<Button>(canvasTransform, "SettingsButton").onClick.Invoke();
            Require(!presetsPanel.gameObject.activeSelf && settingsPanel.gameObject.activeSelf, "Settings button must switch to Settings section only.");
            Require(FindChild<KaelisMenuSliderControl>(settingsPanel, "UIScaleSlider") != null, "Settings panel must expose UI Scale slider.");
            Require(FindChild<KaelisMenuSliderControl>(settingsPanel, "TargetFPSSlider") != null, "Settings panel must expose Target FPS slider.");
            Require(FindChild<KaelisMenuToggleControl>(settingsPanel, "InvertZoomToggle") != null, "Settings panel must expose Invert Zoom toggle.");
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

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException("[KAELIS Menu SmokeTest] " + message);
            }
        }
    }
}
