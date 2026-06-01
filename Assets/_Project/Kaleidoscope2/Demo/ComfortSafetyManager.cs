using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [DisallowMultipleComponent]
    public sealed class ComfortSafetyManager : KaleidoscopeModuleBase, IKaleidoscopeCommandConstraint
    {
        public const float MeditationMaximumRotationsPerSecond = 1.5f;
        public const float MeditationMaximumSpeedUnits = MeditationMaximumRotationsPerSecond * 360f;
        private const float SmoothAccelerationUnitsPerSecond = 1800f;

        private KaleidoscopeDirector director;
        private bool comfortEnabled;
        private float targetRotationSpeedUnits;

        public override string ModuleId { get { return "ComfortSafety"; } }
        public bool ComfortEnabled { get { return comfortEnabled; } }

        public void Configure(KaleidoscopeDirector owner)
        {
            director = owner;
        }

        public void ClearTemporaryPolicy()
        {
            comfortEnabled = false;
            targetRotationSpeedUnits = 0f;
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null
                && (command.Type == KaleidoscopeCommandType.SetComfortSafetyEnabled
                    || command.Type == KaleidoscopeCommandType.SetComfortRotationTarget);
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command.Type == KaleidoscopeCommandType.SetComfortSafetyEnabled)
            {
                comfortEnabled = command.BoolValue;
                targetRotationSpeedUnits = State != null && State.MirrorSettings != null
                    ? State.MirrorSettings.RotationSpeed
                    : 0f;
                return;
            }

            targetRotationSpeedUnits = Mathf.Clamp(
                command.FloatValue,
                -MeditationMaximumSpeedUnits,
                MeditationMaximumSpeedUnits);
        }

        public KaleidoscopeCommand ResolveCommand(KaleidoscopeCommand command, KaleidoscopeCommandOrigin origin)
        {
            if (!comfortEnabled || command == null)
            {
                return command;
            }

            if (command.Type != KaleidoscopeCommandType.SetMirrorRotationSpeed
                && command.Type != KaleidoscopeCommandType.SetMirrorRotationSpeedUnits)
            {
                return command;
            }

            float capped = Mathf.Clamp(command.FloatValue, -MeditationMaximumSpeedUnits, MeditationMaximumSpeedUnits);
            return origin == KaleidoscopeCommandOrigin.User
                ? KaleidoscopeCommand.SetComfortRotationTarget(capped)
                : KaleidoscopeCommand.SetMirrorRotationSpeedUnits(capped);
        }

        public override void Tick(float deltaTime)
        {
            if (!comfortEnabled || director == null || State == null || State.MirrorSettings == null)
            {
                return;
            }

            float current = State.MirrorSettings.RotationSpeed;
            float next = Mathf.MoveTowards(current, targetRotationSpeedUnits, SmoothAccelerationUnitsPerSecond * Mathf.Max(0f, deltaTime));
            if (!Mathf.Approximately(next, current))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorRotationSpeedUnits(next), KaleidoscopeCommandOrigin.Meditation);
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus(comfortEnabled
                ? "Comfort cap active: mirror rotation limited to 1.5 rotations/sec with smooth ramps."
                : "Comfort constraints inactive.");
        }
    }
}
