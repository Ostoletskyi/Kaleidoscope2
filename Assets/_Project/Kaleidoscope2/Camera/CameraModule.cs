using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.CameraSystem
{
    public enum KaleidoscopeCameraRole
    {
        Source = 0,
        Viewer = 1,
        Render = 2,
        Offline = 3
    }

    [DisallowMultipleComponent]
    public sealed class CameraModule : KaleidoscopeModuleBase
    {
        [SerializeField] private UnityEngine.Camera sourceCamera;
        [SerializeField] private UnityEngine.Camera viewerCamera;
        [SerializeField] private UnityEngine.Camera renderCamera;
        [SerializeField] private UnityEngine.Camera offlineCamera;
        [SerializeField] private AudioListener viewerAudioListener;
        [SerializeField] private bool ensureViewerAudioListener = true;

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

        protected override void OnInitialized()
        {
            EnsureViewerAudioListener();
        }

        public UnityEngine.Camera GetCamera(KaleidoscopeCameraRole role)
        {
            switch (role)
            {
                case KaleidoscopeCameraRole.Source:
                    return sourceCamera;

                case KaleidoscopeCameraRole.Viewer:
                    return viewerCamera;

                case KaleidoscopeCameraRole.Render:
                    return renderCamera;

                case KaleidoscopeCameraRole.Offline:
                    return offlineCamera;
            }

            return null;
        }

        public bool TryGetCamera(KaleidoscopeCameraRole role, out UnityEngine.Camera camera)
        {
            camera = GetCamera(role);
            return camera != null;
        }

        public override void Validate()
        {
            EnsureViewerAudioListener();

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

            if (ensureViewerAudioListener && viewerCamera != null && viewerAudioListener == null)
            {
                ReportMissingReference("ViewerAudioListener");
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            int assignedRoles = 0;

            if (sourceCamera != null)
            {
                assignedRoles++;
            }

            if (viewerCamera != null)
            {
                assignedRoles++;
            }

            if (renderCamera != null)
            {
                assignedRoles++;
            }

            if (offlineCamera != null)
            {
                assignedRoles++;
            }

            string listenerStatus = viewerAudioListener != null && viewerAudioListener.enabled
                ? ", viewer audio listener ready."
                : ", viewer audio listener missing.";

            return CreateStatus("Explicit camera roles assigned: " + assignedRoles + "/4" + listenerStatus);
        }

        private void EnsureViewerAudioListener()
        {
            if (!ensureViewerAudioListener || viewerCamera == null)
            {
                return;
            }

            if (viewerAudioListener == null)
            {
                viewerAudioListener = viewerCamera.GetComponent<AudioListener>();
            }

            if (viewerAudioListener == null)
            {
                viewerAudioListener = viewerCamera.gameObject.AddComponent<AudioListener>();
            }

            viewerAudioListener.enabled = true;
        }
    }
}
