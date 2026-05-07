using System;
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
        Diagnostics = 11
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
