using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.CameraSystem
{
    [DisallowMultipleComponent]
    public sealed class CameraModule : KaleidoscopeModuleBase
    {
        [SerializeField] private UnityEngine.Camera sourceCamera;
        [SerializeField] private UnityEngine.Camera viewerCamera;
        [SerializeField] private UnityEngine.Camera renderCamera;
        [SerializeField] private UnityEngine.Camera offlineCamera;

        public override string ModuleId
        {
            get { return "Camera"; }
        }

        public UnityEngine.Camera SourceCamera
        {
            get { return sourceCamera; }
        }

        public UnityEngine.Camera ViewerCamera
        {
            get { return viewerCamera; }
        }

        public UnityEngine.Camera RenderCamera
        {
            get { return renderCamera; }
        }

        public UnityEngine.Camera OfflineCamera
        {
            get { return offlineCamera; }
        }

        public override void Validate()
        {
            if (sourceCamera == null)
            {
                ReportMissingReference("SourceCamera");
            }

            if (viewerCamera == null)
            {
                ReportMissingReference("ViewerCamera");
            }

            if (renderCamera == null)
            {
                ReportMissingReference("RenderCamera");
            }

            if (offlineCamera == null)
            {
                ReportMissingReference("OfflineCamera");
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus("Placeholder. Explicit camera roles are reserved for Stage 05.");
        }
    }
}
