using System.Collections.Generic;
using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Diagnostics
{
    [DisallowMultipleComponent]
    public sealed class DebugHUD : MonoBehaviour
    {
        [SerializeField] private KaleidoscopeDirector director;
        [SerializeField] private bool showHud = true;
        [SerializeField] private Vector2 screenOffset = new Vector2(16f, 16f);
        [SerializeField] private float width = 420f;

        private GUIStyle labelStyle;
        private GUIStyle titleStyle;

        private void OnEnable()
        {
            labelStyle = null;
            titleStyle = null;
        }

        private void EnsureStyles()
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI()
        {
            if (!showHud)
            {
                return;
            }

            if (director != null && !director.State.Diagnostics.HudVisible)
            {
                return;
            }

            if (labelStyle == null || titleStyle == null)
            {
                EnsureStyles();
            }

            GUILayout.BeginArea(new Rect(screenOffset.x, screenOffset.y, width, Screen.height - screenOffset.y * 2f), GUI.skin.box);
            GUILayout.Label("Kaleidoscope2 Diagnostics", titleStyle);

            if (director == null)
            {
                GUILayout.Label("Director reference missing. Assign it explicitly on DebugHUD.", labelStyle);
                GUILayout.EndArea();
                return;
            }

            KaleidoscopeState state = director.State;
            DiagnosticsState diagnostics = state.Diagnostics;

            GUILayout.Label("Mode: " + state.ActiveVisualMode, labelStyle);
            GUILayout.Label("Source: " + state.ActiveSourceMode, labelStyle);
            GUILayout.Label("Mirror Count: " + state.MirrorSettings.MirrorCount, labelStyle);
            GUILayout.Label("Zoom: " + state.MirrorSettings.Zoom.ToString("0.00"), labelStyle);
            GUILayout.Label("Rotation Speed: " + state.MirrorSettings.RotationSpeed.ToString("0.0"), labelStyle);
            GUILayout.Label("Tunnel: " + (state.TunnelEnabled ? "Enabled" : "Disabled"), labelStyle);
            GUILayout.Label("Recording: " + (state.RecordingStatus == KaleidoscopeRecordingStatus.Idle ? "Disabled" : "Enabled") + " (" + state.RecordingStatus + ")", labelStyle);
            GUILayout.Label("Quality: " + state.QualityLevel, labelStyle);
            GUILayout.Label("FPS: " + diagnostics.FramesPerSecond.ToString("0.0"), labelStyle);

            DrawModuleStatuses(diagnostics);
            DrawStringList("Warnings", diagnostics.Warnings);
            DrawStringList("Missing References", diagnostics.MissingReferences);
            DrawStringList("Errors", diagnostics.Errors);

            GUILayout.EndArea();
        }

        private void DrawModuleStatuses(DiagnosticsState diagnostics)
        {
            GUILayout.Space(8f);
            GUILayout.Label("Active Modules", titleStyle);

            IReadOnlyList<KaleidoscopeModuleStatus> statuses = diagnostics.ModuleStatuses;

            if (statuses.Count == 0)
            {
                GUILayout.Label("None registered.", labelStyle);
                return;
            }

            for (int index = 0; index < statuses.Count; index++)
            {
                KaleidoscopeModuleStatus status = statuses[index];
                string active = status.IsActive ? "Active" : "Inactive";
                GUILayout.Label(status.ModuleId + " - " + active + " - " + status.Message, labelStyle);
            }
        }

        private void DrawStringList(string title, IReadOnlyList<string> values)
        {
            GUILayout.Space(8f);
            GUILayout.Label(title, titleStyle);

            if (values.Count == 0)
            {
                GUILayout.Label("None.", labelStyle);
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                GUILayout.Label(values[index], labelStyle);
            }
        }
    }
}
