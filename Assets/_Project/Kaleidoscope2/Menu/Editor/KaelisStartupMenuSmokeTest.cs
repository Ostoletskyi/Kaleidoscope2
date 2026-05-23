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

            Toggle demoToggle = FindChild<Toggle>(canvasTransform, "DemoModeToggle");
            demoToggle.isOn = true;
            Require(controller.DemoModeEnabled, "Demo toggle must store enabled state.");

            KaleidoscopeDirector director = UnityEngine.Object.FindObjectOfType<KaleidoscopeDirector>();
            Require(director != null, "Main scene must contain KaleidoscopeDirector for Enter Experience dispatch.");
            KaleidoscopeCommandType dispatchedType = KaleidoscopeCommandType.None;
            director.CommandDispatched += command => dispatchedType = command.Type;

            Button enterButton = FindChild<Button>(canvasTransform, "EnterExperienceButton");
            enterButton.onClick.Invoke();
            Require(dispatchedType == KaleidoscopeCommandType.ToggleControlMenu, "Enter Experience must dispatch the same ToggleControlMenu command as middle mouse click.");

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

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException("[KAELIS Menu SmokeTest] " + message);
            }
        }
    }
}
