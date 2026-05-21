using System;
using System.Reflection;
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
            Require(FindChild<RawImage>(canvasTransform, "PreviewRawImage") != null, "Preview RawImage missing.");
            Require(FindChild<TMP_Text>(canvasTransform, "Title") != null, "KAELIS title text missing.");

            Toggle demoToggle = FindChild<Toggle>(canvasTransform, "DemoModeToggle");
            demoToggle.isOn = true;
            Require(controller.DemoModeEnabled, "Demo toggle must store enabled state.");

            Button enterButton = FindChild<Button>(canvasTransform, "EnterExperienceButton");
            enterButton.onClick.Invoke();
            Require(!canvasTransform.gameObject.activeSelf, "Enter Experience must hide the menu.");

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
