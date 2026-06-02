using System;
using System.Collections.Generic;
using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [Serializable]
    public sealed class RecordedSemanticAction
    {
        public KaleidoscopeCommand Command;
        public float DeltaSeconds;
        public float PressDurationSeconds;
        public float DispatchTime;
    }

    [DisallowMultipleComponent]
    public sealed class InputRecorder : KaleidoscopeModuleBase
    {
        public const int Capacity = 500;

        private readonly List<RecordedSemanticAction> actions = new List<RecordedSemanticAction>(Capacity);
        private KaleidoscopeDirector director;
        private float previousUserActionTime = -1f;
        private bool subscribed;

        public override string ModuleId { get { return "InputRecorder"; } }
        public int Count { get { return actions.Count; } }

        public void Configure(KaleidoscopeDirector owner)
        {
            if (subscribed && director != null)
            {
                director.SemanticCommandDispatched -= HandleSemanticCommand;
                subscribed = false;
            }

            director = owner;
        }

        protected override void OnInitialized()
        {
            Subscribe();
        }

        protected override void OnActivated()
        {
            Subscribe();
        }

        protected override void OnDeactivated()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public List<RecordedSemanticAction> BuildPlaybackSequence()
        {
            List<RecordedSemanticAction> sequence = new List<RecordedSemanticAction>(Capacity);
            if (actions.Count == 0)
            {
                return sequence;
            }

            for (int index = 0; index < Capacity; index++)
            {
                RecordedSemanticAction source = actions[index % actions.Count];
                sequence.Add(new RecordedSemanticAction
                {
                    Command = source.Command,
                    DeltaSeconds = source.DeltaSeconds,
                    PressDurationSeconds = source.PressDurationSeconds,
                    DispatchTime = source.DispatchTime
                });
            }

            return sequence;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus("Semantic replay history: " + actions.Count + "/" + Capacity + " visual actions.");
        }

        private void Subscribe()
        {
            if (!subscribed && director != null)
            {
                director.SemanticCommandDispatched += HandleSemanticCommand;
                subscribed = true;
            }
        }

        private void Unsubscribe()
        {
            if (subscribed && director != null)
            {
                director.SemanticCommandDispatched -= HandleSemanticCommand;
                subscribed = false;
            }
        }

        private void HandleSemanticCommand(KaleidoscopeCommandDispatchEvent dispatch)
        {
            if (dispatch.Origin != KaleidoscopeCommandOrigin.User || dispatch.Command == null || !IsRecordable(dispatch.Command.Type))
            {
                return;
            }

            float delta = previousUserActionTime < 0f ? 0f : Mathf.Max(0f, dispatch.Timestamp - previousUserActionTime);
            previousUserActionTime = dispatch.Timestamp;
            if (actions.Count >= Capacity)
            {
                actions.RemoveAt(0);
            }

            actions.Add(new RecordedSemanticAction
            {
                Command = dispatch.Command,
                DeltaSeconds = delta,
                PressDurationSeconds = 0f,
                DispatchTime = dispatch.Timestamp
            });
        }

        private static bool IsRecordable(KaleidoscopeCommandType type)
        {
            switch (type)
            {
                case KaleidoscopeCommandType.SetVisualMode:
                case KaleidoscopeCommandType.SetMirrorCount:
                case KaleidoscopeCommandType.CycleTopRowMirrorCountPreset:
                case KaleidoscopeCommandType.SetMirrorRotationSpeed:
                case KaleidoscopeCommandType.SetMirrorRotationSpeedUnits:
                case KaleidoscopeCommandType.SetMirrorZoom:
                case KaleidoscopeCommandType.SetMirrorCenterOffset:
                case KaleidoscopeCommandType.SetTunnelEnabled:
                case KaleidoscopeCommandType.SetTunnelBend:
                case KaleidoscopeCommandType.SetTunnelHoseOpeningUnits:
                case KaleidoscopeCommandType.SetTunnelHoseWallCurvatureUnits:
                case KaleidoscopeCommandType.ResetTunnelHoseProfile:
                case KaleidoscopeCommandType.ToggleTunnelHoseChromaticAberration:
                case KaleidoscopeCommandType.SetTunnelHoseChromaticAberration:
                case KaleidoscopeCommandType.SetFiveDFlightSpeedUnits:
                case KaleidoscopeCommandType.SetSevenDStrategy:
                case KaleidoscopeCommandType.CycleSevenDStrategy:
                case KaleidoscopeCommandType.SetVisualMotionFlightSpeedUnits:
                case KaleidoscopeCommandType.SetVisualMotionImageOffset:
                case KaleidoscopeCommandType.ResetVisualMotion:
                case KaleidoscopeCommandType.SetVisualMotionImageVelocity:
                case KaleidoscopeCommandType.ToggleVisualMotionImageInertia:
                case KaleidoscopeCommandType.SetVisualMotionImageInertia:
                case KaleidoscopeCommandType.SetDiamondRotationDirection:
                case KaleidoscopeCommandType.SetDiamondRotationSpeed:
                case KaleidoscopeCommandType.SetDiamondFocusEnabled:
                case KaleidoscopeCommandType.SetCrystalSimulationMode:
                case KaleidoscopeCommandType.SetPremiumCrystalShape:
                case KaleidoscopeCommandType.SetPremiumCrystalOptic:
                case KaleidoscopeCommandType.SetPremiumCrystalEffectEnabled:
                case KaleidoscopeCommandType.ApplyPremiumCrystalPreset:
                case KaleidoscopeCommandType.SetPremiumCrystalOpticalMode:
                case KaleidoscopeCommandType.SetCrystalDebugEffect:
                case KaleidoscopeCommandType.SetDiamondMaterialMode:
                case KaleidoscopeCommandType.SetClassicCrystalScalePercent:
                case KaleidoscopeCommandType.SetPremiumCrystalScalePercent:
                    return true;
                default:
                    return false;
            }
        }
    }
}
