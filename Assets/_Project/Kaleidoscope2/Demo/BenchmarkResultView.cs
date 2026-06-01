using System;
using System.IO;
using Kaleidoscope2.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kaleidoscope2.Demo
{
    [DisallowMultipleComponent]
    public sealed class BenchmarkResultView : KaleidoscopeModuleBase
    {
        private BenchmarkResult result;
        private string savedPath;
        private bool liveHudVisible;
        private bool resultOverlayVisible;
        private float liveElapsedSeconds;
        private float liveDurationSeconds;
        private float liveCurrentFps;
        private float liveAverageFps;
        private float livePeakFps;
        private float liveOnePercentLowFps;
        private string livePhaseName = string.Empty;
        private GUIStyle hudBoxStyle;
        private GUIStyle hudTitleStyle;
        private GUIStyle hudValueStyle;
        private GUIStyle hudAccentStyle;

        public override string ModuleId { get { return "BenchmarkResult"; } }
        public BenchmarkResult Result { get { return result; } }
        public bool HasResult { get { return result != null; } }
        public string SavedPath { get { return savedPath; } }
        public bool IsLiveHudVisible { get { return liveHudVisible; } }

        public void BeginLiveHud(float durationSeconds)
        {
            liveHudVisible = true;
            resultOverlayVisible = false;
            liveElapsedSeconds = 0f;
            liveDurationSeconds = durationSeconds;
            liveCurrentFps = 0f;
            liveAverageFps = 0f;
            livePeakFps = 0f;
            liveOnePercentLowFps = 0f;
            livePhaseName = "Preparing curated showcase";
        }

        public void UpdateLiveHud(
            float elapsedSeconds,
            float durationSeconds,
            float currentFps,
            float averageFps,
            float peakFps,
            float onePercentLowFps,
            string phaseName)
        {
            liveElapsedSeconds = elapsedSeconds;
            liveDurationSeconds = durationSeconds;
            liveCurrentFps = currentFps;
            liveAverageFps = averageFps;
            livePeakFps = peakFps;
            liveOnePercentLowFps = onePercentLowFps;
            livePhaseName = phaseName ?? string.Empty;
        }

        public void HideLiveHud()
        {
            liveHudVisible = false;
        }

        public void HideOverlays()
        {
            liveHudVisible = false;
            resultOverlayVisible = false;
        }

        public void ShowResults(BenchmarkMetrics metrics, bool fallbackUsed, string priorVisualMode)
        {
            liveHudVisible = false;
            resultOverlayVisible = true;
            result = new BenchmarkResult
            {
                TimestampUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                UnityVersion = Application.unityVersion,
                Resolution = Screen.width + "x" + Screen.height,
                RenderingPipeline = GraphicsSettings.currentRenderPipeline != null
                    ? GraphicsSettings.currentRenderPipeline.name
                    : "Built-in Render Pipeline",
                ActiveVisualMode = priorVisualMode,
                ScenarioVersion = "safe-runtime-v1",
                DurationSeconds = metrics.ElapsedSeconds,
                AverageFps = metrics.AverageFramesPerSecond(),
                PeakFps = metrics.PeakFramesPerSecond(),
                OnePercentLowFps = metrics.OnePercentLowFramesPerSecond(),
                DemoContentFallbackUsed = fallbackUsed
            };
            savedPath = string.Empty;
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null && command.Type == KaleidoscopeCommandType.SaveBenchmarkResult;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            SaveResult();
        }

        public string SaveResult()
        {
            if (result == null)
            {
                return string.Empty;
            }

            string directory = Path.Combine(Application.persistentDataPath, "Benchmarks");
            Directory.CreateDirectory(directory);
            string filename = "Kaleidoscope2_Benchmark_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json";
            savedPath = Path.Combine(directory, filename);
            File.WriteAllText(savedPath, JsonUtility.ToJson(result, true));
            return savedPath;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            if (result == null)
            {
                return CreateStatus("No benchmark result available.");
            }

            string status = "Results: avg " + result.AverageFps.ToString("0.0")
                + " FPS, peak " + result.PeakFps.ToString("0.0")
                + " FPS, 1% low " + result.OnePercentLowFps.ToString("0.0") + " FPS.";
            if (!string.IsNullOrWhiteSpace(savedPath))
            {
                status += " Saved to " + savedPath + ".";
            }

            return CreateStatus(status);
        }

        private void OnGUI()
        {
            bool showLiveHud = liveHudVisible && (State == null || !State.CleanViewEnabled);
            if (!showLiveHud && !resultOverlayVisible)
            {
                return;
            }

            EnsureHudStyles();
            Rect safeArea = Screen.safeArea;
            float left = Mathf.Max(24f, safeArea.xMin + 16f);
            float top = Mathf.Max(24f, Screen.height - safeArea.yMax + 16f);

            if (showLiveHud)
            {
                Rect panel = new Rect(left, top, 334f, 212f);
                Color oldBackground = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.03f, 0.09f, 0.13f, 0.94f);
                GUI.Box(panel, GUIContent.none, hudBoxStyle);
                GUI.backgroundColor = oldBackground;
                GUILayout.BeginArea(new Rect(panel.x + 16f, panel.y + 12f, panel.width - 32f, panel.height - 24f));
                GUILayout.Label("KAELIS  |  VISUAL PERFORMANCE MODE", hudTitleStyle);
                GUILayout.Space(7f);
                GUILayout.Label(livePhaseName.ToUpperInvariant(), hudAccentStyle);
                GUILayout.Space(5f);
                GUILayout.Label("TIME        " + liveElapsedSeconds.ToString("00.0") + " / " + liveDurationSeconds.ToString("00.0") + " s", hudValueStyle);
                GUILayout.Label("CURRENT     " + liveCurrentFps.ToString("0.0") + " FPS", hudValueStyle);
                GUILayout.Label("AVERAGE     " + liveAverageFps.ToString("0.0") + " FPS", hudValueStyle);
                GUILayout.Label("PEAK        " + livePeakFps.ToString("0.0") + " FPS", hudValueStyle);
                GUILayout.Label("1% LOW      " + liveOnePercentLowFps.ToString("0.0") + " FPS", hudValueStyle);
                GUILayout.EndArea();
                return;
            }

            if (result == null)
            {
                return;
            }

            Rect resultPanel = new Rect(left, top, 372f, 202f);
            Color oldResultBackground = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.03f, 0.09f, 0.13f, 0.94f);
            GUI.Box(resultPanel, GUIContent.none, hudBoxStyle);
            GUI.backgroundColor = oldResultBackground;
            GUILayout.BeginArea(new Rect(resultPanel.x + 16f, resultPanel.y + 12f, resultPanel.width - 32f, resultPanel.height - 24f));
            GUILayout.Label("BENCHMARK COMPLETE  |  STATE RESTORED", hudTitleStyle);
            GUILayout.Space(8f);
            GUILayout.Label("AVERAGE     " + result.AverageFps.ToString("0.0") + " FPS", hudValueStyle);
            GUILayout.Label("PEAK        " + result.PeakFps.ToString("0.0") + " FPS", hudValueStyle);
            GUILayout.Label("1% LOW      " + result.OnePercentLowFps.ToString("0.0") + " FPS", hudValueStyle);
            GUILayout.Space(6f);
            GUILayout.Label("Open controls to save the JSON result.", hudAccentStyle);
            GUILayout.EndArea();
        }

        private void EnsureHudStyles()
        {
            if (hudBoxStyle != null)
            {
                return;
            }

            hudBoxStyle = new GUIStyle(GUI.skin.box);
            hudBoxStyle.normal.textColor = Color.white;
            hudBoxStyle.padding = new RectOffset(16, 16, 12, 12);

            hudTitleStyle = new GUIStyle(GUI.skin.label);
            hudTitleStyle.fontSize = 13;
            hudTitleStyle.fontStyle = FontStyle.Bold;
            hudTitleStyle.normal.textColor = new Color(0.60f, 0.94f, 1f, 1f);

            hudValueStyle = new GUIStyle(GUI.skin.label);
            hudValueStyle.fontSize = 15;
            hudValueStyle.fontStyle = FontStyle.Bold;
            hudValueStyle.normal.textColor = Color.white;

            hudAccentStyle = new GUIStyle(GUI.skin.label);
            hudAccentStyle.fontSize = 12;
            hudAccentStyle.fontStyle = FontStyle.Bold;
            hudAccentStyle.normal.textColor = new Color(1f, 0.80f, 0.38f, 1f);
        }
    }
}
