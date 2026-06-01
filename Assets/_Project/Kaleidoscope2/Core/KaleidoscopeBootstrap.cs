using System;
using Kaleidoscope2.Demo;
using Kaleidoscope2.Settings;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum KaleidoscopeModuleArea
    {
        Unassigned = 0,
        Control = 1,
        Input = 2,
        Source = 3,
        PhysicsChamber = 4,
        Mirror = 5,
        Camera = 6,
        AudioReactive = 7,
        Tunnel = 8,
        Recording = 9,
        Presets = 10,
        Diagnostics = 11,
        DepthWarp = 12,
        OpticalLook = 13,
        VolumetricIllusion = 14,
        SevenD = 15,
        DiamondFocus = 16,
        DisplayOutput = 17,
        CrystalLightRig = 18,
        CrystalPresentation = 19,
        SettingsRestore = 20,
        ComfortSafety = 21,
        MeditationMode = 22,
        CrystalSplitComfort = 23,
        InputRecorder = 24,
        DemoReplay = 25,
        Benchmark = 26,
        BenchmarkResult = 27,
        DemoPanel = 28,
        VisualSessionUi = 29,
        CleanView = 30,
        SettingsPersistence = 31
    }

    [Serializable]
    public sealed class KaleidoscopeModuleRegistration
    {
        [SerializeField] private KaleidoscopeModuleArea moduleArea = KaleidoscopeModuleArea.Unassigned;
        [SerializeField] private MonoBehaviour moduleBehaviour;
        [SerializeField] private bool required = true;

        public KaleidoscopeModuleArea ModuleArea
        {
            get { return moduleArea; }
        }

        public MonoBehaviour ModuleBehaviour
        {
            get { return moduleBehaviour; }
        }

        public bool Required
        {
            get { return required; }
        }
    }

    [DisallowMultipleComponent]
    public sealed class KaleidoscopeBootstrap : MonoBehaviour
    {
        [SerializeField] private KaleidoscopeDirector director;
        [SerializeField] private KaleidoscopeModuleRegistration[] moduleRegistrations = new KaleidoscopeModuleRegistration[0];
        [SerializeField, HideInInspector] private MonoBehaviour[] moduleBehaviours = new MonoBehaviour[0];
        [SerializeField] private bool bootstrapOnAwake = true;
        [SerializeField] private bool activateModulesOnBootstrap = true;

        private bool bootstrapped;

        public KaleidoscopeDirector Director
        {
            get { return director; }
        }

        private void Awake()
        {
            if (bootstrapOnAwake)
            {
                Bootstrap();
            }
        }

        public void Bootstrap()
        {
            if (bootstrapped)
            {
                return;
            }

            if (director == null)
            {
                Debug.LogError("[KaleidoscopeBootstrap] Director reference is missing. Assign it explicitly.");
                return;
            }

            director.ClearRegisteredModules();

            if (moduleRegistrations != null && moduleRegistrations.Length > 0)
            {
                RegisterConfiguredModuleSlots();
            }
            else
            {
                RegisterLegacyModuleBehaviours();
            }

            RegisterInternalOrchestrationModules();

            if (activateModulesOnBootstrap)
            {
                director.ActivateAllModules();
            }

            director.Dispatch(KaleidoscopeCommand.ValidateSystem());
            bootstrapped = true;
        }

        private void RegisterConfiguredModuleSlots()
        {
            for (int index = 0; index < moduleRegistrations.Length; index++)
            {
                KaleidoscopeModuleRegistration registration = moduleRegistrations[index];

                if (registration == null)
                {
                    director.State.ReportMissingReference("Bootstrap.ModuleRegistrations[" + index + "]");
                    continue;
                }

                RegisterModuleBehaviour(
                    registration.ModuleBehaviour,
                    registration.ModuleArea,
                    registration.Required,
                    "Bootstrap.ModuleRegistrations[" + index + "]");
            }
        }

        private void RegisterLegacyModuleBehaviours()
        {
            if (moduleBehaviours == null || moduleBehaviours.Length == 0)
            {
                director.State.ReportWarning("[Bootstrap] No module registrations configured.");
                return;
            }

            director.State.ReportWarning("[Bootstrap] Using legacy flat module list. Convert this scene to module registrations.");

            for (int index = 0; index < moduleBehaviours.Length; index++)
            {
                RegisterModuleBehaviour(
                    moduleBehaviours[index],
                    KaleidoscopeModuleArea.Unassigned,
                    true,
                    "Bootstrap.ModuleBehaviours[" + index + "]");
            }
        }

        private void RegisterInternalOrchestrationModules()
        {
            SettingsRestoreService restore = EnsureInternalModule<SettingsRestoreService>();
            ComfortSafetyManager comfort = EnsureInternalModule<ComfortSafetyManager>();
            CrystalSplitComfortController split = EnsureInternalModule<CrystalSplitComfortController>();
            InputRecorder recorder = EnsureInternalModule<InputRecorder>();
            BenchmarkResultView results = EnsureInternalModule<BenchmarkResultView>();
            VisualSessionUiController sessionUi = EnsureInternalModule<VisualSessionUiController>();
            CleanViewController cleanView = EnsureInternalModule<CleanViewController>();
            SettingsPersistenceService settings = EnsureInternalModule<SettingsPersistenceService>();
            MeditationModeController meditation = EnsureInternalModule<MeditationModeController>();
            DemoReplayController replay = EnsureInternalModule<DemoReplayController>();
            BenchmarkController benchmark = EnsureInternalModule<BenchmarkController>();
            DemoPanel panel = EnsureInternalModule<DemoPanel>();

            restore.Configure(director);
            comfort.Configure(director);
            split.Configure(director, restore);
            recorder.Configure(director);
            sessionUi.Configure(director, results);
            cleanView.Configure(director);
            settings.Configure(director);
            meditation.Configure(director, restore, sessionUi);
            replay.Configure(director, restore, recorder, sessionUi);
            benchmark.Configure(director, restore, results, sessionUi);
            panel.Configure(restore, benchmark, results);

            director.RegisterModule(restore);
            director.RegisterModule(comfort);
            director.RegisterModule(split);
            director.RegisterModule(recorder);
            director.RegisterModule(results);
            director.RegisterModule(sessionUi);
            director.RegisterModule(cleanView);
            director.RegisterModule(settings);
            director.RegisterModule(meditation);
            director.RegisterModule(replay);
            director.RegisterModule(benchmark);
            director.RegisterModule(panel);
        }

        private T EnsureInternalModule<T>() where T : Component
        {
            T module = GetComponent<T>();
            return module != null ? module : gameObject.AddComponent<T>();
        }

        private void RegisterModuleBehaviour(
            MonoBehaviour behaviour,
            KaleidoscopeModuleArea moduleArea,
            bool required,
            string referencePath)
        {
            if (behaviour == null)
            {
                if (required)
                {
                    director.State.ReportMissingReference(referencePath + "." + moduleArea);
                }

                return;
            }

            IKaleidoscopeModule module = behaviour as IKaleidoscopeModule;

            if (module == null)
            {
                director.State.ReportWarning("[Bootstrap] " + behaviour.name + " does not implement IKaleidoscopeModule.");
                return;
            }

            if (moduleArea != KaleidoscopeModuleArea.Unassigned && module.ModuleId != moduleArea.ToString())
            {
                director.State.ReportWarning(
                    "[Bootstrap] Registered " + module.ModuleId + " in " + moduleArea + " slot. Check module ownership.");
            }

            director.RegisterModule(module);
        }
    }
}
