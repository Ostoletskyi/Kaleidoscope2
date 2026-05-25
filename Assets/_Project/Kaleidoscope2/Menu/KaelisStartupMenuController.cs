using System.Collections;
using Kaleidoscope2.Core;
using Kaleidoscope2.Menu.FX;
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

        [Header("Audio Feedback")]
        [SerializeField] private MenuAudioFeedbackSettings menuAudioFeedbackSettings = new MenuAudioFeedbackSettings();

        private KaelisMenuAssets assets;
        private KaelisStartupMenuView view;
        private MenuAtmosphereFXController atmosphereFX;
        private KaelisMenuCommandBridge commandBridge;
        private KaelisMenuActionRouter actionRouter;
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
            KaelisMenuLocalizationService.LoadSavedLanguage();
            MenuAudioFeedbackController.Ensure(gameObject, menuAudioFeedbackSettings);
            assets = KaelisMenuAssets.Load(backgroundTexture, previewTexture, fallbackConceptTexture);
            EnsureEventSystem();

            atmosphereFX = MenuAtmosphereFXController.Ensure(gameObject);
            view = new KaelisStartupMenuView(assets, atmosphereFX);
            view.Build(transform);

            commandBridge = new KaelisMenuCommandBridge();
            actionRouter = new KaelisMenuActionRouter(view.SectionController, view.ContentSelectionPanel, commandBridge, SetStatus, view.SetMainMenuVisible, HideStartupMenu);
            SubscribeToNavigationCommands();
            if (view.SectionController != null)
            {
                view.SectionController.SetCommandHandler(actionRouter.HandlePanelCommand);
                view.SectionController.SetPremiumCrystalHandlers(
                    actionRouter.HandlePremiumCrystalOptic,
                    actionRouter.HandlePremiumCrystalEffect,
                    actionRouter.HandlePremiumCrystalShape,
                    actionRouter.HandlePremiumCrystalOpticalMode,
                    actionRouter.HandlePremiumWheelScaleEnabled,
                    actionRouter.HandlePremiumWheelScaleStep,
                    actionRouter.HandleCrystalDebugMode,
                    actionRouter.HandleExperimentalCrystalPreset);
            }

            BindMenuButton(view.EnterButton, EnterExperience);
            BindMenuButton(view.ModesButton, () => OpenSection(KaelisMenuSection.Modes));
            BindMenuButton(view.OpticsButton, () => OpenSection(KaelisMenuSection.Optics));
            BindMenuButton(view.PresetsButton, () => OpenSection(KaelisMenuSection.Presets));
            BindMenuButton(view.SettingsButton, () => OpenSection(KaelisMenuSection.Settings));
            BindMenuButton(view.ExitButton, () => OpenSection(KaelisMenuSection.Exit));

            if (view.DemoToggle != null)
            {
                view.DemoToggle.onValueChanged.AddListener(UpdateDemoState);
            }

            SetVisible(startVisible, true);
            UpdateDemoState(false);
        }

        private void OnDestroy()
        {
            if (director != null)
            {
                director.CommandDispatched -= HandleDirectorCommand;
                director = null;
            }

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
            if (actionRouter != null)
            {
                actionRouter.TriggerEnterExperience();
            }
        }

        private void OpenSection(KaelisMenuSection section)
        {
            if (actionRouter != null)
            {
                actionRouter.OpenSection(section);
            }
        }

        private void SubscribeToNavigationCommands()
        {
            director = FindObjectOfType<KaleidoscopeDirector>();
            if (director != null)
            {
                director.CommandDispatched -= HandleDirectorCommand;
                director.CommandDispatched += HandleDirectorCommand;
            }
        }

        private void HandleDirectorCommand(KaleidoscopeCommand command)
        {
            if (command == null || command.Type != KaleidoscopeCommandType.ReturnToInitialMenu)
            {
                return;
            }

            if (actionRouter != null)
            {
                actionRouter.ReturnToInitialMenu();
            }

            SetVisible(true, false);
        }

        private void UpdateDemoState(bool enabled)
        {
            demoModeEnabled = enabled;

            if (view != null)
            {
                view.SetDemoState(demoModeEnabled);
                view.SetStatus(demoModeEnabled ? "SYSTEM READY     DEMO RESERVED" : "SYSTEM READY     DEMO OFF");
            }

            Debug.Log("[KAELIS Menu] Demo Mode reserved state stored: " + (demoModeEnabled ? "ON" : "OFF") + ".");
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
                    view.SetMainMenuVisible(true);
                    if (view.ContentSelectionPanel != null)
                    {
                        view.ContentSelectionPanel.SetVisible(false);
                    }

                    SetStatus(demoModeEnabled ? "SYSTEM READY     DEMO RESERVED" : "SYSTEM READY     DEMO OFF");
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
                SetStatus(demoModeEnabled ? "SYSTEM READY     DEMO RESERVED" : "SYSTEM READY     DEMO OFF");
            }

            visibilityRoutine = null;
        }

        private void HideStartupMenu()
        {
            SetVisible(false, false);
        }

        private void SetStatus(string value)
        {
            if (view != null)
            {
                view.SetStatus(value);
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
    }
}
