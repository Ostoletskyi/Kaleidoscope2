using System;
using Kaleidoscope2.AudioReactive;
using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [DisallowMultipleComponent]
    public sealed class MeditationModeController : KaleidoscopeModuleBase
    {
        public const float MinimumRotationsPerSecond = 0.25f;
        public const float MaximumRotationsPerSecond = 1.5f;
        public const float RareFlightPulseDurationSeconds = 0.1f;
        private const float DirectionIntervalSeconds = 60f;
        private const float ReversalTransitionSeconds = 2f;
        private const float BreathingCycleSeconds = 10f;
        private const float ResetImageIntervalSeconds = 40f;
        private const float MotionDurationMinimumSeconds = 0.1f;
        private const float MotionDurationMaximumSeconds = 1f;
        private const float MotionOffsetUnitsPerSecond = 0.12f;
        private const float FlightPulseUnitsPerSecond = 160f;
        private const float DiamondSpeedPulseUnitsPerSecond = 36f;
        private const int ActivitySeed = 73021;
        private static readonly Vector2[] SingleMotionDirections =
        {
            Vector2.left,
            Vector2.down,
            Vector2.right,
            Vector2.up
        };
        private static readonly Vector2[,] PairedMotionDirections =
        {
            { Vector2.left, Vector2.down },
            { Vector2.left, Vector2.up },
            { Vector2.down, Vector2.right },
            { Vector2.right, Vector2.up },
            { Vector2.down, Vector2.left },
            { Vector2.up, Vector2.left },
            { Vector2.right, Vector2.down },
            { Vector2.up, Vector2.right }
        };
        private static readonly int[] MirrorCounts = { 6, 12, 24, 48, 96, 192, 384, 768, 1536 };
        private static readonly int[] MirrorSelectionWeights = { 34, 22, 15, 10, 7, 5, 3, 2, 1 };

        private KaleidoscopeDirector director;
        private SettingsRestoreService restoreService;
        private VisualSessionUiController sessionUi;
        private float elapsedSeconds;
        private float nextResetSeconds;
        private float nextMotionActivitySeconds;
        private float nextMirrorVariationSeconds;
        private float primaryMotionEndSeconds;
        private float secondaryMotionEndSeconds;
        private Vector2 primaryMotionDirection;
        private Vector2 secondaryMotionDirection;
        private Vector2 lastDiamondDirection;
        private int motionActivityCount;
        private System.Random activityRandom;
        private string status = "Meditation Mode ready.";

        public override string ModuleId { get { return "MeditationMode"; } }
        public bool IsRunning { get { return restoreService != null && restoreService.IsActive(TemporarySessionKind.Meditation); } }

        public void Configure(KaleidoscopeDirector owner, SettingsRestoreService restore, VisualSessionUiController presentationUi)
        {
            director = owner;
            restoreService = restore;
            sessionUi = presentationUi;
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null
                && (command.Type == KaleidoscopeCommandType.SetMeditationModeEnabled
                    || command.Type == KaleidoscopeCommandType.CancelTemporarySession);
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command.Type == KaleidoscopeCommandType.SetMeditationModeEnabled)
            {
                if (command.BoolValue)
                {
                    StartMeditation();
                }
                else
                {
                    StopMeditation("Meditation Mode stopped.");
                }
                return;
            }

            if (IsRunning)
            {
                StopMeditation("Meditation Mode cancelled.");
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
                elapsedSeconds += Mathf.Max(0f, deltaTime);
                director.Dispatch(
                    KaleidoscopeCommand.SetComfortRotationTarget(EvaluateMirrorRotationSpeedUnits(elapsedSeconds)),
                    KaleidoscopeCommandOrigin.Meditation);
                TickGeneratedActivity(Mathf.Max(0f, deltaTime));
            }
            catch (Exception exception)
            {
                FailAndRestore("Meditation Mode failed during playback; prior settings restored.", exception);
            }
        }

        public static float EvaluateMirrorRotationSpeedUnits(float elapsed)
        {
            float time = Mathf.Max(0f, elapsed);
            float midpoint = (MinimumRotationsPerSecond + MaximumRotationsPerSecond) * 0.5f;
            float amplitude = (MaximumRotationsPerSecond - MinimumRotationsPerSecond) * 0.5f;
            float rotationsPerSecond = midpoint + amplitude * Mathf.Cos(2f * Mathf.PI * time / BreathingCycleSeconds);
            float direction = ResolveDirectionBlend(time);
            return rotationsPerSecond * 360f * direction;
        }

        public static int ResolveWeightedMirrorCount(float unitSample)
        {
            float normalized = Mathf.Clamp01(unitSample);
            int totalWeight = 0;
            for (int index = 0; index < MirrorSelectionWeights.Length; index++)
            {
                totalWeight += MirrorSelectionWeights[index];
            }

            float selection = normalized * totalWeight;
            int accumulated = 0;
            for (int index = 0; index < MirrorSelectionWeights.Length; index++)
            {
                accumulated += MirrorSelectionWeights[index];
                if (selection < accumulated || index == MirrorSelectionWeights.Length - 1)
                {
                    return MirrorCounts[index];
                }
            }

            return MirrorCounts[0];
        }

        private static float ResolveDirectionBlend(float time)
        {
            int nearestBoundary = Mathf.RoundToInt(time / DirectionIntervalSeconds);
            float boundary = nearestBoundary * DirectionIntervalSeconds;
            float halfTransition = ReversalTransitionSeconds * 0.5f;

            if (nearestBoundary >= 1 && Mathf.Abs(time - boundary) <= halfTransition)
            {
                float transition = Mathf.InverseLerp(boundary - halfTransition, boundary + halfTransition, time);
                transition = transition * transition * (3f - 2f * transition);
                float before = (nearestBoundary % 2) == 1 ? 1f : -1f;
                return before * (1f - 2f * transition);
            }

            return (Mathf.FloorToInt(time / DirectionIntervalSeconds) % 2) == 0 ? 1f : -1f;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            AudioReactiveModule audio = director != null ? DemoRuntimeLookup.FindModule<AudioReactiveModule>(director) : null;
            string activeStatus = status;
            if (IsRunning && audio != null && audio.CuratedDemoAudioUnavailable)
            {
                activeStatus += " Curated audio unavailable; visuals continue silently.";
            }

            return CreateStatus(IsRunning
                ? activeStatus + " Time " + elapsedSeconds.ToString("0.0") + "s, mirror speed " + EvaluateMirrorRotationSpeedUnits(elapsedSeconds).ToString("0") + " deg/sec."
                : status);
        }

        private void StartMeditation()
        {
            if (director == null || restoreService == null || !restoreService.TryBeginSession(TemporarySessionKind.Meditation))
            {
                status = "Meditation Mode unavailable while another temporary session is active.";
                return;
            }

            elapsedSeconds = 0f;
            ResetActivityTimeline();
            status = "Meditation Mode active. Escape or middle mouse stops and restores prior settings.";
            try
            {
                sessionUi?.BeginPresentation(TemporarySessionKind.Meditation, "MEDITATION MODE", KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetDemoImageContent(DemoContentCatalog.MeditationProfileId), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetSourceMode(KaleidoscopeSourceMode.ImageTexture), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetDemoAudioContent(DemoContentCatalog.MeditationPlaylistId), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetAudioPlaybackEnabled(true), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetComfortSafetyEnabled(true), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetCrystalSplitComfortEnabled(true), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetDiamondFocusEnabled(true), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetCrystalDebugMode(DiamondCrystalDebugMode.FinalCrystalComposite), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetCrystalRuntimeControlModule(CrystalRuntimeControlModule.CrystalDebugMode), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.CycleSelectedCrystalRuntimeControl(1), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetCrystalDebugEffect(CrystalDebugEffectType.None), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetDiamondMaterialMode(DiamondCrystalMaterialMode.Diamond), KaleidoscopeCommandOrigin.Meditation);
                director.Dispatch(KaleidoscopeCommand.SetComfortRotationTarget(EvaluateMirrorRotationSpeedUnits(0f)), KaleidoscopeCommandOrigin.Meditation);
            }
            catch (Exception exception)
            {
                FailAndRestore("Meditation Mode failed to start; prior settings restored.", exception);
            }
        }

        private void StopMeditation(string finalStatus)
        {
            if (!IsRunning || director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetCrystalSplitComfortEnabled(false), KaleidoscopeCommandOrigin.Meditation);
            director.Dispatch(KaleidoscopeCommand.SetComfortSafetyEnabled(false), KaleidoscopeCommandOrigin.Meditation);
            director.Dispatch(KaleidoscopeCommand.SetDiamondRotationDirection(Vector2.zero), KaleidoscopeCommandOrigin.Meditation);
            restoreService.RestoreAndEnd(TemporarySessionKind.Meditation);
            sessionUi?.EndPresentation(TemporarySessionKind.Meditation);
            elapsedSeconds = 0f;
            lastDiamondDirection = Vector2.zero;
            status = finalStatus;
        }

        private void FailAndRestore(string finalStatus, Exception exception)
        {
            ReportWarning(finalStatus + " " + exception.Message);
            if (director != null)
            {
                director.Dispatch(KaleidoscopeCommand.SetDiamondRotationDirection(Vector2.zero), KaleidoscopeCommandOrigin.Meditation);
            }

            if (restoreService != null)
            {
                restoreService.RestoreAndEnd(TemporarySessionKind.Meditation);
            }

            sessionUi?.EndPresentation(TemporarySessionKind.Meditation);
            elapsedSeconds = 0f;
            lastDiamondDirection = Vector2.zero;
            status = finalStatus;
        }

        private void ResetActivityTimeline()
        {
            activityRandom = new System.Random(ActivitySeed);
            nextResetSeconds = ResetImageIntervalSeconds;
            nextMotionActivitySeconds = 1.8f;
            nextMirrorVariationSeconds = 8f;
            primaryMotionEndSeconds = 0f;
            secondaryMotionEndSeconds = 0f;
            primaryMotionDirection = Vector2.zero;
            secondaryMotionDirection = Vector2.zero;
            lastDiamondDirection = Vector2.zero;
            motionActivityCount = 0;
        }

        private void TickGeneratedActivity(float deltaTime)
        {
            if (State == null || activityRandom == null)
            {
                return;
            }

            while (elapsedSeconds >= nextResetSeconds)
            {
                director.Dispatch(KaleidoscopeCommand.StartImageReanimation(), KaleidoscopeCommandOrigin.Meditation);
                nextResetSeconds += ResetImageIntervalSeconds;
            }

            if (elapsedSeconds >= nextMirrorVariationSeconds && !State.ImageReanimationActive)
            {
                director.Dispatch(
                    KaleidoscopeCommand.SetMirrorCount(ResolveWeightedMirrorCount((float)activityRandom.NextDouble())),
                    KaleidoscopeCommandOrigin.Meditation);
                nextMirrorVariationSeconds = elapsedSeconds + RandomRange(10f, 17f);
            }

            if (!State.ImageReanimationActive && elapsedSeconds >= nextMotionActivitySeconds)
            {
                BeginMotionActivity();
            }

            if (IsClassicFormationSelfRotationActive())
            {
                lastDiamondDirection = CrystalSplitComfortController.EvaluateClassicSelfRotationDirection(elapsedSeconds);
                return;
            }

            Vector2 motionDirection = State.ImageReanimationActive ? Vector2.zero : ResolveCurrentMotionDirection();
            DispatchDiamondDirectionIfChanged(motionDirection);
            if (motionDirection.sqrMagnitude <= 0.0001f || deltaTime <= 0f)
            {
                return;
            }

            VisualMotionSettings motion = State.GetVisualMotionSettings(State.ActiveVisualMode);
            if (motion != null)
            {
                Vector2 offset = motion.ImageOffset + motionDirection * MotionOffsetUnitsPerSecond * deltaTime;
                director.Dispatch(
                    KaleidoscopeCommand.SetVisualMotionImageOffset(State.ActiveVisualMode, offset),
                    KaleidoscopeCommandOrigin.Meditation);
            }
        }

        private bool IsClassicFormationSelfRotationActive()
        {
            return State != null
                && State.ActiveVisualMode == KaleidoscopeVisualMode.Classic
                && State.CrystalSplitPresentation.Enabled
                && CrystalSplitComfortController.IsSelfRotationWindow(elapsedSeconds);
        }

        private void BeginMotionActivity()
        {
            motionActivityCount++;
            bool usePair = activityRandom.NextDouble() < 0.34;
            if (usePair)
            {
                int pairIndex = activityRandom.Next(0, PairedMotionDirections.GetLength(0));
                primaryMotionDirection = PairedMotionDirections[pairIndex, 0];
                secondaryMotionDirection = PairedMotionDirections[pairIndex, 1];
                primaryMotionEndSeconds = elapsedSeconds + RandomRange(MotionDurationMinimumSeconds, MotionDurationMaximumSeconds);
                secondaryMotionEndSeconds = elapsedSeconds + RandomRange(MotionDurationMinimumSeconds, MotionDurationMaximumSeconds);
            }
            else
            {
                primaryMotionDirection = SingleMotionDirections[activityRandom.Next(0, SingleMotionDirections.Length)];
                secondaryMotionDirection = Vector2.zero;
                primaryMotionEndSeconds = elapsedSeconds + RandomRange(MotionDurationMinimumSeconds, MotionDurationMaximumSeconds);
                secondaryMotionEndSeconds = elapsedSeconds;
            }

            if ((motionActivityCount % 5) == 0)
            {
                DispatchRareFlightPulse(activityRandom.NextDouble() >= 0.5);
            }

            nextMotionActivitySeconds = elapsedSeconds + RandomRange(1.65f, 3.4f);
        }

        private Vector2 ResolveCurrentMotionDirection()
        {
            Vector2 direction = Vector2.zero;
            if (elapsedSeconds < primaryMotionEndSeconds)
            {
                direction += primaryMotionDirection;
            }

            if (elapsedSeconds < secondaryMotionEndSeconds)
            {
                direction += secondaryMotionDirection;
            }

            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private void DispatchRareFlightPulse(bool increase)
        {
            float flightDelta = FlightPulseUnitsPerSecond * RareFlightPulseDurationSeconds * (increase ? 1f : -1f);
            VisualMotionSettings motion = State.GetVisualMotionSettings(State.ActiveVisualMode);
            if (motion != null)
            {
                director.Dispatch(
                    KaleidoscopeCommand.SetVisualMotionFlightSpeedUnits(State.ActiveVisualMode, motion.FlightSpeedUnits + flightDelta),
                    KaleidoscopeCommandOrigin.Meditation);
            }

            float crystalDelta = DiamondSpeedPulseUnitsPerSecond * RareFlightPulseDurationSeconds;
            director.Dispatch(
                increase
                    ? KaleidoscopeCommand.IncreaseDiamondRotationSpeed(crystalDelta)
                    : KaleidoscopeCommand.DecreaseDiamondRotationSpeed(crystalDelta),
                KaleidoscopeCommandOrigin.Meditation);
        }

        private void DispatchDiamondDirectionIfChanged(Vector2 direction)
        {
            if (direction == lastDiamondDirection)
            {
                return;
            }

            lastDiamondDirection = direction;
            director.Dispatch(KaleidoscopeCommand.SetDiamondRotationDirection(direction), KaleidoscopeCommandOrigin.Meditation);
        }

        private float RandomRange(float minimum, float maximum)
        {
            return Mathf.Lerp(minimum, maximum, (float)activityRandom.NextDouble());
        }
    }
}
