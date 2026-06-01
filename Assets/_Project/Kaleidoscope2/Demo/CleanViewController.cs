using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [DisallowMultipleComponent]
    public sealed class CleanViewController : KaleidoscopeModuleBase
    {
        private KaleidoscopeDirector director;
        private bool hiddenVisibilityCaptured;
        private bool priorControlMenuVisible;
        private bool priorHotkeysHelpVisible;
        private bool priorDiagnosticsVisible;

        public override string ModuleId { get { return "CleanView"; } }

        public void Configure(KaleidoscopeDirector owner)
        {
            director = owner;
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null
                && (command.Type == KaleidoscopeCommandType.SetCleanViewEnabled
                    || command.Type == KaleidoscopeCommandType.ToggleCleanView);
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (director == null || State == null)
            {
                return;
            }

            if (State.CleanViewEnabled)
            {
                priorControlMenuVisible = State.ControlMenuVisible;
                priorHotkeysHelpVisible = State.HotkeysHelpVisible;
                priorDiagnosticsVisible = State.Diagnostics != null && State.Diagnostics.HudVisible;
                hiddenVisibilityCaptured = true;
                director.Dispatch(KaleidoscopeCommand.SetControlMenuVisible(false));
                director.Dispatch(KaleidoscopeCommand.SetHotkeysHelpVisible(false));
                director.Dispatch(KaleidoscopeCommand.SetDiagnosticsVisible(false));
                return;
            }

            if (!hiddenVisibilityCaptured)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetControlMenuVisible(priorControlMenuVisible));
            director.Dispatch(KaleidoscopeCommand.SetHotkeysHelpVisible(priorHotkeysHelpVisible));
            director.Dispatch(KaleidoscopeCommand.SetDiagnosticsVisible(priorDiagnosticsVisible));
            hiddenVisibilityCaptured = false;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus(State != null && State.CleanViewEnabled
                ? "Clean view active: non-essential overlays hidden; press H to restore."
                : "Clean view inactive; press H to hide non-essential overlays.");
        }
    }
}
