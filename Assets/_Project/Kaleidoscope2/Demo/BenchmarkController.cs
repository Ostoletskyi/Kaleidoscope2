using System;
using Kaleidoscope2.Core;
using Kaleidoscope2.Source;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [DisallowMultipleComponent]
    public sealed class BenchmarkController : KaleidoscopeModuleBase
    {
        public const float DurationSeconds = 60f;

        private static readonly PremiumCrystalShapeType[] Shapes =
        {
            PremiumCrystalShapeType.Sphere,
            PremiumCrystalShapeType.Cube,
            PremiumCrystalShapeType.Octahedron,
            PremiumCrystalShapeType.Hexahedron,
            PremiumCrystalShapeType.VolumetricRhombus,
            PremiumCrystalShapeType.Cone,
            PremiumCrystalShapeType.PlateDisc,
            PremiumCrystalShapeType.Icosahedron,
            PremiumCrystalShapeType.Dodecahedron,
            PremiumCrystalShapeType.DoublePyramid,
            PremiumCrystalShapeType.CrystalLens,
            PremiumCrystalShapeType.StarPrism
        };

        private readonly BenchmarkMetrics metrics = new BenchmarkMetrics();
        private KaleidoscopeDirector director;
        private SettingsRestoreService restoreService;
        private BenchmarkResultView resultView;
        private VisualSessionUiController sessionUi;
        private float elapsedSeconds;
        private int lastPhaseSlot = -1;
        private string status = "Benchmark Demo ready.";
        private string preRunVisualMode = "Unknown";
        private string currentPhaseName = "Ready";
        private float currentFps;

        public override string ModuleId { get { return "Benchmark"; } }
        public bool IsRunning { get { return restoreService != null && restoreService.IsActive(TemporarySessionKind.BenchmarkDemo); } }
        public float RemainingSeconds { get { return Mathf.Max(0f, DurationSeconds - elapsedSeconds); } }
        public float CurrentFps { get { return currentFps; } }
        public string CurrentPhaseName { get { return currentPhaseName; } }

        public void Configure(KaleidoscopeDirector owner, SettingsRestoreService restore, BenchmarkResultView view, VisualSessionUiController presentationUi)
        {
            director = owner;
            restoreService = restore;
            resultView = view;
            sessionUi = presentationUi;
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null
                && (command.Type == KaleidoscopeCommandType.StartBenchmarkDemo
                    || command.Type == KaleidoscopeCommandType.CancelTemporarySession);
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command.Type == KaleidoscopeCommandType.StartBenchmarkDemo)
            {
                StartBenchmark();
            }
            else if (IsRunning)
            {
                CancelBenchmark();
            }
        }

        public override void Tick(float deltaTime)
        {
            if (!IsRunning || director == null)
            {
                return;
            }

            try
            {
                float sampleDelta = Mathf.Max(0f, Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : deltaTime);
                metrics.Sample(sampleDelta);
                currentFps = sampleDelta > 0.0001f ? 1f / sampleDelta : 0f;
                elapsedSeconds += sampleDelta;
                ApplyScenario(elapsedSeconds);
                UpdateLiveHud();
                if (elapsedSeconds >= DurationSeconds)
                {
                    CompleteBenchmark();
                }
            }
            catch (Exception exception)
            {
                FailAndRestore("Benchmark failed during playback; prior settings restored.", exception);
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            if (IsRunning)
            {
                return CreateStatus("Benchmark running: " + RemainingSeconds.ToString("0.0") + "s remaining, FPS " + CurrentFps.ToString("0.0") + ". Escape or middle mouse cancels.");
            }

            return CreateStatus(status);
        }

        private void StartBenchmark()
        {
            if (director == null || restoreService == null || !restoreService.TryBeginSession(TemporarySessionKind.BenchmarkDemo))
            {
                status = "Benchmark unavailable while another temporary session is active.";
                return;
            }

            preRunVisualMode = State != null ? State.ActiveVisualMode.ToString() : "Unknown";
            elapsedSeconds = 0f;
            lastPhaseSlot = -1;
            currentFps = 0f;
            currentPhaseName = "Preparing curated showcase";
            metrics.Reset();
            status = "Benchmark running.";
            try
            {
                sessionUi?.BeginPresentation(TemporarySessionKind.BenchmarkDemo, string.Empty, KaleidoscopeCommandOrigin.Benchmark);
                if (resultView != null)
                {
                    resultView.BeginLiveHud(DurationSeconds);
                }

                director.Dispatch(KaleidoscopeCommand.SetDemoImageContent(DemoContentCatalog.BenchmarkProfileId), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetSourceMode(KaleidoscopeSourceMode.ImageTexture), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetAudioPlaybackEnabled(false), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetCrystalDebugMode(DiamondCrystalDebugMode.FinalCrystalComposite), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetCrystalDebugEffect(CrystalDebugEffectType.None), KaleidoscopeCommandOrigin.Benchmark);
                ApplyScenario(0f);
                UpdateLiveHud();
            }
            catch (Exception exception)
            {
                FailAndRestore("Benchmark failed to start; prior settings restored.", exception);
            }
        }

        private void ApplyScenario(float time)
        {
            currentPhaseName = ResolvePhaseName(time);
            int phaseSlot = ResolvePhaseSlot(time);
            if (phaseSlot == lastPhaseSlot)
            {
                return;
            }

            lastPhaseSlot = phaseSlot;
            if (time < 6f)
            {
                director.Dispatch(KaleidoscopeCommand.SetVisualMode(KaleidoscopeVisualMode.Classic), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(6 + phaseSlot * 24), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetMirrorZoom(Mathf.Lerp(0.7f, 1.4f, phaseSlot / 5f)), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetMirrorRotationSpeedUnits(Mathf.Lerp(120f, 720f, phaseSlot / 5f)), KaleidoscopeCommandOrigin.Benchmark);
                return;
            }

            if (time < 30f)
            {
                int shapeIndex = Mathf.Clamp(Mathf.FloorToInt((time - 6f) / 2f), 0, Shapes.Length - 1);
                director.Dispatch(KaleidoscopeCommand.SetDiamondFocusEnabled(true), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetCrystalSimulationMode(CrystalRenderMode.RealMesh3D), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalShape(Shapes[shapeIndex]), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalOpticalMode(ResolveSafeOpticalMode(shapeIndex)), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.Brightness, Mathf.Lerp(0.8f, 1.8f, shapeIndex / 11f)), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.ReflectionStrength, Mathf.Lerp(0.5f, 2.5f, shapeIndex / 11f)), KaleidoscopeCommandOrigin.Benchmark);
                return;
            }

            if (time < 36f)
            {
                director.Dispatch(KaleidoscopeCommand.SetVisualMode(KaleidoscopeVisualMode.Tunnel), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetMirrorRotationSpeedUnits(360f), KaleidoscopeCommandOrigin.Benchmark);
                return;
            }

            if (time < 42f)
            {
                director.Dispatch(KaleidoscopeCommand.SetVisualMode(KaleidoscopeVisualMode.Hose), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetTunnelHoseOpeningUnits(Mathf.Lerp(4f, 30f, (time - 36f) / 6f)), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetTunnelHoseChromaticAberration(true), KaleidoscopeCommandOrigin.Benchmark);
                return;
            }

            if (time < 48f)
            {
                director.Dispatch(KaleidoscopeCommand.SetVisualMode(KaleidoscopeVisualMode.FiveD), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetFiveDFlightSpeedUnits(Mathf.Lerp(2f, 24f, (time - 42f) / 6f)), KaleidoscopeCommandOrigin.Benchmark);
                return;
            }

            if (time < 54f)
            {
                director.Dispatch(KaleidoscopeCommand.SetVisualMode(KaleidoscopeVisualMode.SixD), KaleidoscopeCommandOrigin.Benchmark);
                director.Dispatch(KaleidoscopeCommand.SetVisualMotionFlightSpeedUnits(KaleidoscopeVisualMode.SixD, Mathf.Lerp(4f, 28f, (time - 48f) / 6f)), KaleidoscopeCommandOrigin.Benchmark);
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetVisualMode(KaleidoscopeVisualMode.SevenD), KaleidoscopeCommandOrigin.Benchmark);
            director.Dispatch(KaleidoscopeCommand.SetSevenDStrategy((SevenDVisualizationStrategy)(Mathf.FloorToInt(time - 54f) % 5)), KaleidoscopeCommandOrigin.Benchmark);
            director.Dispatch(KaleidoscopeCommand.SetVisualMotionFlightSpeedUnits(KaleidoscopeVisualMode.SevenD, Mathf.Lerp(4f, 28f, (time - 54f) / 6f)), KaleidoscopeCommandOrigin.Benchmark);
        }

        private void CompleteBenchmark()
        {
            bool fallback = ResolveContentFallback();
            restoreService.RestoreAndEnd(TemporarySessionKind.BenchmarkDemo);
            sessionUi?.EndPresentation(TemporarySessionKind.BenchmarkDemo);
            if (resultView != null)
            {
                resultView.ShowResults(metrics, fallback, preRunVisualMode);
            }

            status = "Benchmark complete; prior settings restored. Results ready to save.";
        }

        private void CancelBenchmark()
        {
            restoreService.RestoreAndEnd(TemporarySessionKind.BenchmarkDemo);
            sessionUi?.EndPresentation(TemporarySessionKind.BenchmarkDemo);
            if (resultView != null)
            {
                resultView.HideLiveHud();
            }

            status = "Benchmark cancelled; prior settings restored.";
        }

        private void FailAndRestore(string finalStatus, Exception exception)
        {
            ReportWarning(finalStatus + " " + exception.Message);
            if (restoreService != null)
            {
                restoreService.RestoreAndEnd(TemporarySessionKind.BenchmarkDemo);
            }

            sessionUi?.EndPresentation(TemporarySessionKind.BenchmarkDemo);
            if (resultView != null)
            {
                resultView.HideLiveHud();
            }

            status = finalStatus;
        }

        private void UpdateLiveHud()
        {
            if (resultView == null)
            {
                return;
            }

            resultView.UpdateLiveHud(
                Mathf.Min(elapsedSeconds, DurationSeconds),
                DurationSeconds,
                currentFps,
                metrics.AverageFramesPerSecond(),
                metrics.PeakFramesPerSecond(),
                metrics.OnePercentLowFramesPerSecond(),
                currentPhaseName);
        }

        private bool ResolveContentFallback()
        {
            SourceModule source = DemoRuntimeLookup.FindModule<SourceModule>(director);
            return source != null && source.UsingDemoContentFallback;
        }

        private static int ResolvePhaseSlot(float time)
        {
            return time < 30f ? Mathf.FloorToInt(time < 6f ? time : 6f + (time - 6f) / 2f) : Mathf.FloorToInt(time);
        }

        private static PremiumCrystalOpticalMode ResolveSafeOpticalMode(int shapeIndex)
        {
            switch (shapeIndex % 5)
            {
                case 1:
                    return PremiumCrystalOpticalMode.PrismDispersion;
                case 2:
                    return PremiumCrystalOpticalMode.MirrorFacets;
                case 3:
                    return PremiumCrystalOpticalMode.InternalReflection;
                case 4:
                    return PremiumCrystalOpticalMode.AbsoluteMirror;
                default:
                    return PremiumCrystalOpticalMode.HighPurityDiamond;
            }
        }

        public static string ResolvePhaseName(float time)
        {
            if (time < 6f)
            {
                return "Classic Mirror Sweep";
            }

            if (time < 30f)
            {
                int shapeIndex = Mathf.Clamp(Mathf.FloorToInt((time - 6f) / 2f), 0, Shapes.Length - 1);
                return "Premium Crystal: " + Shapes[shapeIndex];
            }

            if (time < 36f)
            {
                return "Tunnel Depth Sweep";
            }

            if (time < 42f)
            {
                return "Hose Optical Sweep";
            }

            if (time < 48f)
            {
                return "5D Endless Flight";
            }

            if (time < 54f)
            {
                return "6D Optical Pass";
            }

            return "7D Strategy Sweep";
        }
    }
}
