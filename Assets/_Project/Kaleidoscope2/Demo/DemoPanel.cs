using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [DisallowMultipleComponent]
    public sealed class DemoPanel : KaleidoscopeModuleBase
    {
        private SettingsRestoreService restoreService;
        private BenchmarkController benchmark;
        private BenchmarkResultView resultView;

        public override string ModuleId { get { return "DemoPanel"; } }

        public void Configure(SettingsRestoreService restore, BenchmarkController benchmarkController, BenchmarkResultView view)
        {
            restoreService = restore;
            benchmark = benchmarkController;
            resultView = view;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            if (restoreService != null && restoreService.HasActiveSession)
            {
                return CreateStatus("Demo panel: " + restoreService.ActiveSession + " active; Escape or middle mouse stops.");
            }

            if (resultView != null && resultView.HasResult)
            {
                return CreateStatus(resultView.GetStatus().Message);
            }

            return CreateStatus(benchmark != null ? "Demo panel ready: Replay Demo or 60-second Benchmark Demo." : "Demo panel unavailable.");
        }
    }
}
