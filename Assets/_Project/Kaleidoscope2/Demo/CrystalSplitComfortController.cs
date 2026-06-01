using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [DisallowMultipleComponent]
    public sealed class CrystalSplitComfortController : KaleidoscopeModuleBase
    {
        public const float SelfRotationDurationSeconds = 2.5f;
        public const float DetachDurationSeconds = 2.5f;
        public const float OrbitDurationSeconds = 5f;
        public const float MergeDurationSeconds = 2.5f;
        public const float DetachStartSeconds = SelfRotationDurationSeconds;
        public const float OrbitStartSeconds = DetachStartSeconds + DetachDurationSeconds;
        public const float MergeStartSeconds = OrbitStartSeconds + OrbitDurationSeconds;
        public const float CycleDurationSeconds = SelfRotationDurationSeconds + DetachDurationSeconds + OrbitDurationSeconds + MergeDurationSeconds;
        public const int CopyCount = 6;

        private KaleidoscopeDirector director;
        private SettingsRestoreService restoreService;
        private float elapsedSeconds;
        private Vector2 lastClassicRotationDirection;

        public override string ModuleId { get { return "CrystalSplitComfort"; } }

        public void Configure(KaleidoscopeDirector owner, SettingsRestoreService restore)
        {
            director = owner;
            restoreService = restore;
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null && command.Type == KaleidoscopeCommandType.SetCrystalSplitComfortEnabled;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (State == null)
            {
                return;
            }

            if (command.BoolValue && (restoreService == null || !restoreService.IsActive(TemporarySessionKind.Meditation)))
            {
                State.ReportWarning("[CrystalSplitComfort] Split comfort can be enabled only during Meditation Mode.");
                return;
            }

            elapsedSeconds = 0f;
            DispatchClassicFormationRotation(Vector2.zero);
            lastClassicRotationDirection = Vector2.zero;
            State.CrystalSplitPresentation.SetEnabled(command.BoolValue, CrystalFormationMode.SixCopyOrbitFormation);
            State.CrystalSplitPresentation.SetCycle(0f, 0f, 0f);
        }

        public override void Tick(float deltaTime)
        {
            if (State == null || !State.CrystalSplitPresentation.Enabled)
            {
                return;
            }

            elapsedSeconds += Mathf.Max(0f, deltaTime);
            DispatchClassicFormationRotation(IsClassicFormationActive()
                ? EvaluateClassicSelfRotationDirection(elapsedSeconds)
                : Vector2.zero);
            State.CrystalSplitPresentation.SetCycle(
                elapsedSeconds,
                EvaluateExpansion(elapsedSeconds),
                EvaluateOrbitAngleRadians(elapsedSeconds));
        }

        public static float EvaluateExpansion(float elapsed)
        {
            float phase = Mathf.Repeat(Mathf.Max(0f, elapsed), CycleDurationSeconds);
            if (phase < DetachStartSeconds)
            {
                return 0f;
            }

            if (phase < OrbitStartSeconds)
            {
                return SmoothStep((phase - DetachStartSeconds) / DetachDurationSeconds);
            }

            if (phase < MergeStartSeconds)
            {
                return 1f;
            }

            return SmoothStep((CycleDurationSeconds - phase) / MergeDurationSeconds);
        }

        public static float EvaluateOrbitAngleRadians(float elapsed)
        {
            float phase = Mathf.Repeat(Mathf.Max(0f, elapsed), CycleDurationSeconds);
            if (phase <= OrbitStartSeconds)
            {
                return 0f;
            }

            if (phase >= MergeStartSeconds)
            {
                return Mathf.PI * 2f;
            }

            float orbit = (phase - OrbitStartSeconds) / OrbitDurationSeconds;
            return Mathf.PI * 2f * SmoothStep(orbit);
        }

        public static bool IsSelfRotationWindow(float elapsed)
        {
            float phase = Mathf.Repeat(Mathf.Max(0f, elapsed), CycleDurationSeconds);
            return phase < SelfRotationDurationSeconds;
        }

        public static Vector2 EvaluateClassicSelfRotationDirection(float elapsed)
        {
            return IsSelfRotationWindow(elapsed) ? Vector2.left : Vector2.zero;
        }

        private void DispatchClassicFormationRotation(Vector2 direction)
        {
            if (director == null || direction == lastClassicRotationDirection)
            {
                return;
            }

            lastClassicRotationDirection = direction;
            director.Dispatch(KaleidoscopeCommand.SetDiamondRotationDirection(direction), KaleidoscopeCommandOrigin.Meditation);
        }

        private bool IsClassicFormationActive()
        {
            DiamondFocusSettings settings = State != null ? State.DiamondFocusSettings : null;
            return State != null
                && State.ActiveVisualMode == KaleidoscopeVisualMode.Classic
                && settings != null
                && settings.Enabled
                && settings.CrystalSimulationMode != CrystalRenderMode.RealMesh3D;
        }

        private static float SmoothStep(float value)
        {
            float clamped = Mathf.Clamp01(value);
            return clamped * clamped * (3f - 2f * clamped);
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            bool enabled = State != null && State.CrystalSplitPresentation.Enabled;
            return CreateStatus(enabled
                ? "Formation active: SixCopyOrbitFormation, Classic uses semantic self-rotation then six full copies; Premium uses six full mesh copies; expansion " + State.CrystalSplitPresentation.Expansion.ToString("0.00") + "."
                : "Split comfort inactive.");
        }
    }
}
