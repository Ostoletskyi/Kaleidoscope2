using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [DisallowMultipleComponent]
    public sealed class VisualSessionUiController : KaleidoscopeModuleBase
    {
        private KaleidoscopeDirector director;
        private BenchmarkResultView benchmarkResultView;
        private TemporarySessionKind presentedSession;
        private string sessionLabel = string.Empty;
        private GUIStyle boxStyle;
        private GUIStyle labelStyle;

        public override string ModuleId { get { return "VisualSessionUi"; } }

        public void Configure(KaleidoscopeDirector owner, BenchmarkResultView resultView)
        {
            director = owner;
            benchmarkResultView = resultView;
        }

        public void BeginPresentation(TemporarySessionKind kind, string label, KaleidoscopeCommandOrigin origin)
        {
            benchmarkResultView?.HideOverlays();
            if (director != null)
            {
                director.Dispatch(KaleidoscopeCommand.SetControlMenuVisible(false), origin);
                director.Dispatch(KaleidoscopeCommand.SetHotkeysHelpVisible(false), origin);
                director.Dispatch(KaleidoscopeCommand.SetDiagnosticsVisible(false), origin);
            }

            presentedSession = kind;
            sessionLabel = label ?? string.Empty;
        }

        public void EndPresentation(TemporarySessionKind kind)
        {
            if (presentedSession != kind)
            {
                return;
            }

            presentedSession = TemporarySessionKind.None;
            sessionLabel = string.Empty;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus(presentedSession == TemporarySessionKind.None
                ? "Visual session presentation idle."
                : "Visual session presentation active for " + presentedSession + ".");
        }

        private void OnGUI()
        {
            if (presentedSession == TemporarySessionKind.None
                || string.IsNullOrWhiteSpace(sessionLabel)
                || (State != null && State.CleanViewEnabled))
            {
                return;
            }

            EnsureStyles();
            Rect safeArea = Screen.safeArea;
            float left = Mathf.Max(24f, safeArea.xMin + 16f);
            float top = Mathf.Max(24f, Screen.height - safeArea.yMax + 16f);
            Rect panel = new Rect(left, top, 405f, 44f);
            Color oldBackground = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.03f, 0.09f, 0.13f, 0.86f);
            GUI.Box(panel, GUIContent.none, boxStyle);
            GUI.backgroundColor = oldBackground;
            GUI.Label(new Rect(panel.x + 14f, panel.y + 12f, panel.width - 28f, 22f),
                sessionLabel + "  |  ESC / MIDDLE MOUSE TO EXIT",
                labelStyle);
        }

        private void EnsureStyles()
        {
            if (boxStyle != null)
            {
                return;
            }

            boxStyle = new GUIStyle(GUI.skin.box);
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = new Color(0.80f, 0.96f, 1f, 1f);
        }
    }
}
