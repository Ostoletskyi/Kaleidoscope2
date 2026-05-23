using System.Collections;
using Kaleidoscope2.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class KaelisStartupMenuController : MonoBehaviour
    {
        [Header("Startup")]
        [SerializeField] private bool startVisible = true;

        [Header("Concept References")]
        [SerializeField] private Texture backgroundTexture;
        [SerializeField] private Texture previewTexture;
        [SerializeField] private Texture fallbackConceptTexture;

        private KaelisMenuAssets assets;
        private KaelisStartupMenuView view;
        private KaleidoscopeDirector director;
        private Coroutine visibilityRoutine;
        private bool demoModeEnabled;
        private bool visible;

        public bool DemoModeEnabled
        {
            get { return demoModeEnabled; }
        }

        private void Awake()
        {
            assets = KaelisMenuAssets.Load(backgroundTexture, previewTexture, fallbackConceptTexture);
            EnsureEventSystem();
            ResolveRuntimeDirector();

            view = new KaelisStartupMenuView(assets);
            view.Build(transform);

            BindMenuButton(view.EnterButton, EnterExperience);
            BindMenuButton(view.ModesButton, () => LogPlaceholder("Modes"));
            BindMenuButton(view.OpticsButton, () => LogPlaceholder("Optics"));
            BindMenuButton(view.PresetsButton, () => LogPlaceholder("Presets"));
            BindMenuButton(view.SettingsButton, () => LogPlaceholder("Settings"));
            BindMenuButton(view.ExitButton, ExitApplication);

            if (view.DemoToggle != null)
            {
                view.DemoToggle.onValueChanged.AddListener(UpdateDemoState);
            }

            SetVisible(startVisible, true);
            UpdateDemoState(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetVisible(!visible, false);
            }
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.Dispose();
                view = null;
            }

            if (assets != null)
            {
                assets.Dispose();
                assets = null;
            }
        }

        private void BindMenuButton(KaelisMenuButton menuButton, UnityAction action)
        {
            if (menuButton == null || menuButton.Button == null || action == null)
            {
                return;
            }

            menuButton.Button.onClick.AddListener(() => menuButton.InvokeWithFlash(action));
        }

        private void EnterExperience()
        {
            ToggleRuntimeControlMenuFromUserAction();
        }

        private void ToggleRuntimeControlMenuFromUserAction()
        {
            if (director == null)
            {
                ResolveRuntimeDirector();
            }

            if (director == null)
            {
                SetStatus("RUNTIME MENU UNAVAILABLE");
                Debug.LogWarning("[KAELIS Menu] Enter Experience could not find KaleidoscopeDirector for ToggleControlMenu.");
                return;
            }

            director.Dispatch(KaleidoscopeCommand.ToggleControlMenu());
            SetStatus("RUNTIME MENU TOGGLED");
            Debug.Log("[KAELIS Menu] Enter Experience dispatched ToggleControlMenu, matching middle mouse click.");
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

            if (view != null)
            {
                view.SetDemoState(demoModeEnabled);
                view.SetStatus(demoModeEnabled ? "SYSTEM READY     DEMO ON" : "SYSTEM READY     DEMO OFF");
            }

            Debug.Log("[KAELIS Menu] Demo Mode stored: " + (demoModeEnabled ? "ON" : "OFF") + ".");
        }

        private void SetVisible(bool shouldShow, bool instant)
        {
            visible = shouldShow;

            if (view == null || view.Root == null || view.CanvasGroup == null)
            {
                return;
            }

            view.CanvasGroup.blocksRaycasts = visible;
            view.CanvasGroup.interactable = visible;

            if (visibilityRoutine != null)
            {
                StopCoroutine(visibilityRoutine);
                visibilityRoutine = null;
            }

            if (instant || !Application.isPlaying)
            {
                view.CanvasGroup.alpha = visible ? 1f : 0f;
                view.Root.SetActive(visible);
                if (visible)
                {
                    SetStatus(demoModeEnabled ? "SYSTEM READY     DEMO ON" : "SYSTEM READY     DEMO OFF");
                }

                return;
            }

            visibilityRoutine = StartCoroutine(FadeVisible(visible));
        }

        private IEnumerator FadeVisible(bool shouldShow)
        {
            if (view == null || view.Root == null || view.CanvasGroup == null)
            {
                yield break;
            }

            if (shouldShow && !view.Root.activeSelf)
            {
                view.Root.SetActive(true);
            }

            float startAlpha = view.CanvasGroup.alpha;
            float targetAlpha = shouldShow ? 1f : 0f;
            float elapsed = 0f;

            while (elapsed < KaelisMenuStyle.VisibilityFadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / KaelisMenuStyle.VisibilityFadeSeconds);
                t = 1f - ((1f - t) * (1f - t));
                view.CanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            view.CanvasGroup.alpha = targetAlpha;

            if (!shouldShow)
            {
                view.Root.SetActive(false);
            }
            else
            {
                SetStatus(demoModeEnabled ? "SYSTEM READY     DEMO ON" : "SYSTEM READY     DEMO OFF");
            }

            visibilityRoutine = null;
        }

        private void SetStatus(string value)
        {
            if (view != null)
            {
                view.SetStatus(value);
            }
        }

        private void ResolveRuntimeDirector()
        {
            director = UnityEngine.Object.FindObjectOfType<KaleidoscopeDirector>();
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
    }
}
