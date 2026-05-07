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
    public sealed class MirrorModule : KaleidoscopeModuleBase, IKaleidoscopeTextureProcessor
    {
        private Texture outputTexture;

        public override string ModuleId
        {
            get { return "Mirror"; }
        }

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        public MirrorSettings Settings
        {
            get { return State != null ? State.MirrorSettings : null; }
        }

        public Texture Process(Texture sourceTexture, KaleidoscopeState runtimeState)
        {
            outputTexture = sourceTexture;
            return outputTexture;
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Type == KaleidoscopeCommandType.SetMirrorCount
                || command.Type == KaleidoscopeCommandType.SetMirrorRotation
                || command.Type == KaleidoscopeCommandType.SetMirrorRotationSpeed
                || command.Type == KaleidoscopeCommandType.SetMirrorZoom
                || command.Type == KaleidoscopeCommandType.SetMirrorCenterOffset;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            MirrorSettings settings = Settings;
            string message = settings == null
                ? "Waiting for runtime state."
                : "Placeholder pass-through. Mirror count " + settings.MirrorCount + ", zoom " + settings.Zoom.ToString("0.00") + ".";

            return CreateStatus(message);
        }
    }
}
