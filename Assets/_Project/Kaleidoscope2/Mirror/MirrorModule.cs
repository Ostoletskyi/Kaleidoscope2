using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Mirror
{
    public static class MirrorShaderIds
    {
        public static readonly int MirrorCount = Shader.PropertyToID("_MirrorCount");
        public static readonly int Rotation = Shader.PropertyToID("_Rotation");
        public static readonly int Zoom = Shader.PropertyToID("_Zoom");
        public static readonly int CenterOffset = Shader.PropertyToID("_CenterOffset");
    }

    [DisallowMultipleComponent]
    public sealed class MirrorModule : KaleidoscopeModuleBase
    {
        public override string ModuleId
        {
            get { return "Mirror"; }
        }

        public MirrorSettings Settings
        {
            get { return State != null ? State.MirrorSettings : null; }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Type == KaleidoscopeCommandType.SetMirrorCount
                || command.Type == KaleidoscopeCommandType.SetMirrorRotation
                || command.Type == KaleidoscopeCommandType.SetMirrorZoom
                || command.Type == KaleidoscopeCommandType.SetMirrorCenterOffset;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            MirrorSettings settings = Settings;
            string message = settings == null
                ? "Waiting for runtime state."
                : "Placeholder. Mirror count " + settings.MirrorCount + ". Rendering starts in Stage 04.";

            return CreateStatus(message);
        }
    }
}
