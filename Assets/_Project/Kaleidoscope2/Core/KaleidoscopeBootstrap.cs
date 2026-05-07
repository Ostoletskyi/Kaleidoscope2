using UnityEngine;

namespace Kaleidoscope2.Core
{
    [DisallowMultipleComponent]
    public sealed class KaleidoscopeBootstrap : MonoBehaviour
    {
        [SerializeField] private KaleidoscopeDirector director;
        [SerializeField] private MonoBehaviour[] moduleBehaviours = new MonoBehaviour[0];
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

            for (int index = 0; index < moduleBehaviours.Length; index++)
            {
                MonoBehaviour behaviour = moduleBehaviours[index];

                if (behaviour == null)
                {
                    director.State.ReportMissingReference("Bootstrap.ModuleBehaviours[" + index + "]");
                    continue;
                }

                IKaleidoscopeModule module = behaviour as IKaleidoscopeModule;

                if (module == null)
                {
                    director.State.ReportWarning("[Bootstrap] " + behaviour.name + " does not implement IKaleidoscopeModule.");
                    continue;
                }

                director.RegisterModule(module);
            }

            if (activateModulesOnBootstrap)
            {
                director.ActivateAllModules();
            }

            director.Dispatch(KaleidoscopeCommand.ValidateSystem());
            bootstrapped = true;
        }
    }
}
