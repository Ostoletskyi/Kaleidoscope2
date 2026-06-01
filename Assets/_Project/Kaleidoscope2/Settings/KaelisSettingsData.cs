using System;
using Kaleidoscope2.Core;
using Kaleidoscope2.Menu;
using UnityEngine;

namespace Kaleidoscope2.Settings
{
    [Serializable]
    public sealed class KaelisSettingsData
    {
        public const int CurrentSchemaVersion = 2;

        public int version = CurrentSchemaVersion;
        public VisualPreferences visual = VisualPreferences.CreateDefault();
        public ControlPreferences controls = ControlPreferences.CreateDefault();
        public DisplaySettings display = DisplaySettings.CreateDefault();
        public PerformanceSettings performance = PerformanceSettings.CreateDefault();
        public AudioSettings audio = AudioSettings.CreateDefault();
        public UiSettings ui = UiSettings.CreateDefault();
        public StartupPreferences startup = StartupPreferences.CreateDefault();
        public DiagnosticsPreferences diagnostics = DiagnosticsPreferences.CreateDefault();

        public static KaelisSettingsData CreateDefault()
        {
            return new KaelisSettingsData();
        }

        public static KaelisSettingsData Sanitize(KaelisSettingsData data)
        {
            if (data == null)
            {
                return CreateDefault();
            }

            data.version = CurrentSchemaVersion;
            data.visual = VisualPreferences.Sanitize(data.visual);
            data.controls = ControlPreferences.Sanitize(data.controls);
            data.display = DisplaySettings.Sanitize(data.display);
            data.performance = PerformanceSettings.Sanitize(data.performance);
            data.audio = AudioSettings.Sanitize(data.audio);
            data.ui = UiSettings.Sanitize(data.ui);
            data.startup = StartupPreferences.Sanitize(data.startup);
            data.diagnostics = DiagnosticsPreferences.Sanitize(data.diagnostics);
            return data;
        }
    }

    [Serializable]
    public sealed class VisualPreferences
    {
        public int activeVisualMode;
        public bool diamondFocusEnabled;
        public int crystalRenderMode;
        public float classicCrystalScalePercent;
        public float premiumCrystalScalePercent;
        public int classicShape;
        public int premiumShape;
        public int premiumOpticalMode;
        public int debugMode;
        public int crystalDebugEffect;
        public string activePreset;

        public static VisualPreferences CreateDefault()
        {
            return new VisualPreferences
            {
                activeVisualMode = (int)KaleidoscopeVisualMode.Classic,
                diamondFocusEnabled = true,
                crystalRenderMode = (int)CrystalRenderMode.Billboard2D,
                classicCrystalScalePercent = DiamondFocusSettings.ClassicCrystalScalePercentDefault,
                premiumCrystalScalePercent = DiamondFocusSettings.PremiumCrystalScalePercentDefault,
                classicShape = (int)DiamondFocusShape.ClassicDiamond,
                premiumShape = (int)PremiumCrystalShapeType.VolumetricRhombus,
                premiumOpticalMode = (int)PremiumCrystalOpticalMode.HighPurityDiamond,
                debugMode = (int)DiamondCrystalDebugMode.FinalCrystalComposite,
                crystalDebugEffect = (int)CrystalDebugEffectType.None,
                activePreset = "None"
            };
        }

        public static VisualPreferences Sanitize(VisualPreferences settings)
        {
            if (settings == null)
            {
                return CreateDefault();
            }

            settings.activeVisualMode = Mathf.Clamp(settings.activeVisualMode, 0, Enum.GetValues(typeof(KaleidoscopeVisualMode)).Length - 1);
            settings.crystalRenderMode = settings.crystalRenderMode == (int)CrystalRenderMode.RealMesh3D
                ? (int)CrystalRenderMode.RealMesh3D
                : (int)CrystalRenderMode.Billboard2D;
            settings.classicCrystalScalePercent = Mathf.Clamp(
                settings.classicCrystalScalePercent,
                DiamondFocusSettings.ClassicCrystalScalePercentMin,
                DiamondFocusSettings.ClassicCrystalScalePercentMax);
            settings.premiumCrystalScalePercent = Mathf.Clamp(
                settings.premiumCrystalScalePercent,
                DiamondFocusSettings.PremiumCrystalScalePercentMin,
                DiamondFocusSettings.PremiumCrystalScalePercentMax);
            settings.classicShape = Mathf.Clamp(settings.classicShape, 0, DiamondFocusSettings.ShapeCount - 1);
            settings.premiumShape = Mathf.Clamp(settings.premiumShape, 0, PremiumCrystalShapeLibrary.Count - 1);
            settings.premiumOpticalMode = Mathf.Clamp(settings.premiumOpticalMode, 0, PremiumCrystalOpticalModeLibrary.Count - 1);
            settings.debugMode = Mathf.Clamp(settings.debugMode, 0, Enum.GetValues(typeof(DiamondCrystalDebugMode)).Length - 1);
            settings.crystalDebugEffect = Mathf.Clamp(settings.crystalDebugEffect, 0, CrystalDebugEffectLibrary.Count - 1);
            settings.activePreset = string.IsNullOrWhiteSpace(settings.activePreset) ? "None" : settings.activePreset;
            return settings;
        }
    }

    [Serializable]
    public sealed class ControlPreferences
    {
        public bool mouseWheelVisualScaleEnabled;
        public float mouseWheelVisualScaleStepPercent;

        public static ControlPreferences CreateDefault()
        {
            return new ControlPreferences
            {
                mouseWheelVisualScaleEnabled = true,
                mouseWheelVisualScaleStepPercent = KaleidoscopeState.MouseWheelVisualScaleStepPercentDefault
            };
        }

        public static ControlPreferences Sanitize(ControlPreferences settings)
        {
            if (settings == null)
            {
                return CreateDefault();
            }

            settings.mouseWheelVisualScaleStepPercent = Mathf.Clamp(
                settings.mouseWheelVisualScaleStepPercent,
                KaleidoscopeState.MouseWheelVisualScaleStepPercentMin,
                KaleidoscopeState.MouseWheelVisualScaleStepPercentMax);
            return settings;
        }
    }

    [Serializable]
    public sealed class DisplaySettings
    {
        public int resolutionWidth;
        public int resolutionHeight;
        public bool fullscreen;
        public int windowMode;

        public static DisplaySettings CreateDefault()
        {
            return new DisplaySettings
            {
                resolutionWidth = 0,
                resolutionHeight = 0,
                fullscreen = false,
                windowMode = 0
            };
        }

        public static DisplaySettings Sanitize(DisplaySettings settings)
        {
            if (settings == null)
            {
                return CreateDefault();
            }

            settings.resolutionWidth = Mathf.Max(0, settings.resolutionWidth);
            settings.resolutionHeight = Mathf.Max(0, settings.resolutionHeight);
            settings.windowMode = Mathf.Clamp(settings.windowMode, 0, 3);
            return settings;
        }
    }

    [Serializable]
    public sealed class PerformanceSettings
    {
        public bool vSync;
        public int targetFps;
        public int targetFpsOptionIndex;

        public static PerformanceSettings CreateDefault()
        {
            return new PerformanceSettings
            {
                vSync = false,
                targetFps = 60,
                targetFpsOptionIndex = 1
            };
        }

        public static PerformanceSettings Sanitize(PerformanceSettings settings)
        {
            if (settings == null)
            {
                return CreateDefault();
            }

            settings.targetFpsOptionIndex = Mathf.Clamp(settings.targetFpsOptionIndex, 0, 7);
            if (settings.targetFps != -1)
            {
                settings.targetFps = Mathf.Clamp(settings.targetFps, 15, 480);
            }

            return settings;
        }
    }

    [Serializable]
    public sealed class AudioSettings
    {
        public float masterVolumePercent;
        public float menuVolumePercent;
        public float demoVolumePercent;
        public bool muted;

        public static AudioSettings CreateDefault()
        {
            return new AudioSettings
            {
                masterVolumePercent = 100f,
                menuVolumePercent = 80f,
                demoVolumePercent = 75f,
                muted = false
            };
        }

        public static AudioSettings Sanitize(AudioSettings settings)
        {
            if (settings == null)
            {
                return CreateDefault();
            }

            settings.masterVolumePercent = Mathf.Clamp(settings.masterVolumePercent, 0f, 100f);
            settings.menuVolumePercent = Mathf.Clamp(settings.menuVolumePercent, 0f, 100f);
            settings.demoVolumePercent = Mathf.Clamp(settings.demoVolumePercent, 0f, 100f);
            return settings;
        }
    }

    [Serializable]
    public sealed class UiSettings
    {
        public float scalePercent;
        public string menuLanguage;

        public static UiSettings CreateDefault()
        {
            return new UiSettings
            {
                scalePercent = 100f,
                menuLanguage = KaelisMenuLanguage.English.ToString()
            };
        }

        public static UiSettings Sanitize(UiSettings settings)
        {
            if (settings == null)
            {
                return CreateDefault();
            }

            settings.scalePercent = Mathf.Clamp(settings.scalePercent, 70f, 160f);
            KaelisMenuLanguage language;
            if (string.IsNullOrWhiteSpace(settings.menuLanguage) || !Enum.TryParse(settings.menuLanguage, out language))
            {
                settings.menuLanguage = KaelisMenuLanguage.English.ToString();
            }

            return settings;
        }
    }

    [Serializable]
    public sealed class StartupPreferences
    {
        public bool startWithMenu;
        public bool autoSaveSettings;

        public static StartupPreferences CreateDefault()
        {
            return new StartupPreferences
            {
                startWithMenu = true,
                autoSaveSettings = false
            };
        }

        public static StartupPreferences Sanitize(StartupPreferences settings)
        {
            return settings ?? CreateDefault();
        }
    }

    [Serializable]
    public sealed class DiagnosticsPreferences
    {
        public bool showFps;
        public bool showDiagnostics;
        public bool showInputOverlay;
        public bool showRenderStats;

        public static DiagnosticsPreferences CreateDefault()
        {
            return new DiagnosticsPreferences
            {
                showFps = false,
                showDiagnostics = false,
                showInputOverlay = false,
                showRenderStats = false
            };
        }

        public static DiagnosticsPreferences Sanitize(DiagnosticsPreferences settings)
        {
            return settings ?? CreateDefault();
        }
    }
}
