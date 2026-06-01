using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [Serializable]
    public sealed class BenchmarkMetrics
    {
        private readonly List<float> fpsSamples = new List<float>(7200);
        private float elapsedSeconds;

        public float ElapsedSeconds { get { return elapsedSeconds; } }
        public int SampleCount { get { return fpsSamples.Count; } }

        public void Reset()
        {
            elapsedSeconds = 0f;
            fpsSamples.Clear();
        }

        public void Sample(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime <= 0f)
            {
                return;
            }

            elapsedSeconds += unscaledDeltaTime;
            fpsSamples.Add(1f / unscaledDeltaTime);
        }

        public float AverageFramesPerSecond()
        {
            return elapsedSeconds > 0.0001f ? fpsSamples.Count / elapsedSeconds : 0f;
        }

        public float PeakFramesPerSecond()
        {
            float peak = 0f;
            for (int index = 0; index < fpsSamples.Count; index++)
            {
                peak = Mathf.Max(peak, fpsSamples[index]);
            }

            return peak;
        }

        public float OnePercentLowFramesPerSecond()
        {
            if (fpsSamples.Count == 0)
            {
                return 0f;
            }

            List<float> sorted = new List<float>(fpsSamples);
            sorted.Sort();
            int percentileIndex = Mathf.Clamp(Mathf.FloorToInt((sorted.Count - 1) * 0.01f), 0, sorted.Count - 1);
            return sorted[percentileIndex];
        }
    }

    [Serializable]
    public sealed class BenchmarkResult
    {
        public string TimestampUtc;
        public string UnityVersion;
        public string Resolution;
        public string RenderingPipeline;
        public string ActiveVisualMode;
        public string ScenarioVersion;
        public float DurationSeconds;
        public float AverageFps;
        public float PeakFps;
        public float OnePercentLowFps;
        public bool DemoContentFallbackUsed;
    }
}
