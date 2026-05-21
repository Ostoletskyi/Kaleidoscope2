param(
    [string]$FilePath = ""
)

$ErrorActionPreference = "Stop"

function Resolve-CrystalSharedSettingsPath {
    param([string]$ExplicitPath)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        if (-not (Test-Path -LiteralPath $ExplicitPath)) {
            throw "FilePath not found: $ExplicitPath"
        }
        return (Resolve-Path -LiteralPath $ExplicitPath).Path
    }

    $matches = Get-ChildItem -Path . -Recurse -Filter "CrystalSharedSettings.cs" -File | Select-Object -ExpandProperty FullName

    if ($matches.Count -eq 0) {
        throw "CrystalSharedSettings.cs was not found under current directory. Run this script from the Unity project root or pass -FilePath."
    }

    if ($matches.Count -gt 1) {
        Write-Host "Multiple CrystalSharedSettings.cs files found:" -ForegroundColor Yellow
        $matches | ForEach-Object { Write-Host "  $_" }
        throw "Pass -FilePath explicitly to avoid patching the wrong file."
    }

    return $matches[0]
}

function Replace-Exact {
    param(
        [string]$Source,
        [string]$Old,
        [string]$New,
        [string]$Label
    )

    if (-not $Source.Contains($Old)) {
        throw "Expected snippet not found: $Label"
    }

    return $Source.Replace($Old, $New)
}

$target = Resolve-CrystalSharedSettingsPath -ExplicitPath $FilePath
Write-Host "Patching: $target" -ForegroundColor Cyan

$original = Get-Content -LiteralPath $target -Raw -Encoding UTF8
$text = $original

# 1) Safer serialized defaults. These matter before SyncFromDiamond() runs and for any fallback paths.
$text = Replace-Exact $text `
    "[SerializeField, Range(0f, 2f)] private float spectralDispersion = 1.18f;" `
    "[SerializeField, Range(0f, 2f)] private float spectralDispersion = 0.82f;" `
    "default spectralDispersion"

$text = Replace-Exact $text `
    "[SerializeField, Range(0f, 2f)] private float facetFire = 1.08f;" `
    "[SerializeField, Range(0f, 2f)] private float facetFire = 0.68f;" `
    "default facetFire"

$text = Replace-Exact $text `
    "[SerializeField, Range(0f, 2f)] private float fresnelStrength = 1.35f;" `
    "[SerializeField, Range(0f, 2f)] private float fresnelStrength = 1.05f;" `
    "default fresnelStrength"

$text = Replace-Exact $text `
    "[SerializeField, Range(0f, 20f)] private float intensity = 8f;" `
    "[SerializeField, Range(0f, 20f)] private float intensity = 5.2f;" `
    "default intensity"

$text = Replace-Exact $text `
    "[SerializeField, Range(0f, 3f)] private float internalBrightness = 1f;" `
    "[SerializeField, Range(0f, 3f)] private float internalBrightness = 0.68f;" `
    "default internalBrightness"

$text = Replace-Exact $text `
    "[SerializeField, Range(0f, 1f)] private float minimumTransmission = 0.1f;" `
    "[SerializeField, Range(0f, 1f)] private float minimumTransmission = 0.045f;" `
    "default minimumTransmission"

# 2) Runtime SyncFromDiamond safety.
#    The shader defaults are not enough because SyncFromDiamond feeds runtime values into the material every frame/pass.
$text = Replace-Exact $text `
    "            intensity = Mathf.Clamp(lightRigSettings.LightIntensity, diamondSettings.ActiveCrystalBrightnessMin, diamondSettings.ActiveCrystalBrightnessMax);" `
    "            float rawCrystalIntensity = Mathf.Clamp(lightRigSettings.LightIntensity, diamondSettings.ActiveCrystalBrightnessMin, diamondSettings.ActiveCrystalBrightnessMax);
            intensity = Mathf.Clamp(rawCrystalIntensity * 0.65f, 0f, 20f);" `
    "SyncFromDiamond intensity"

$text = Replace-Exact $text `
    "            internalBrightness = diamondSettings.InternalBrightness;" `
    "            internalBrightness = Mathf.Clamp(diamondSettings.InternalBrightness * 0.68f, 0f, 3f);" `
    "SyncFromDiamond internalBrightness"

$text = Replace-Exact $text `
    "            minimumTransmission = Mathf.Clamp(0.08f + diamondSettings.DirectTransmission * 0.58f, 0.06f, 0.42f);" `
    "            minimumTransmission = Mathf.Clamp(0.025f + diamondSettings.DirectTransmission * 0.35f, 0.02f, 0.22f);" `
    "SyncFromDiamond minimumTransmission"

# 3) Calm Diamond profile. This keeps Diamond bright, but stops it from behaving like an overexposed white lamp.
$text = Replace-Exact $text `
    '                    SetPremiumMaterial("Diamond", new Color(0.94f, 0.99f, 1f, 1f), new Color(0.7f, 0.9f, 1f, 1f), new Color(1f, 0.88f, 0.34f, 1f), 0.12f, 0.86f, 1.28f, 1.22f, 1.42f, 1.36f, 1.34f, 0.22f, 0.98f, refractiveIndexValue: 2.417f, physicalDispersionValue: 0.044f, absorptionStrengthValue: 0.22f, fresnelStrengthValue: 1.55f, backgroundDistortionValue: 1.26f, saturationBoostValue: 1.06f, contrastBoostValue: 1.26f);' `
    '                    SetPremiumMaterial("Diamond", new Color(0.90f, 0.97f, 1f, 1f), new Color(0.58f, 0.76f, 0.92f, 1f), new Color(1f, 0.80f, 0.36f, 1f), 0.10f, 0.92f, 1.08f, 1.06f, 1.12f, 0.82f, 0.72f, 0.34f, 0.82f, refractiveIndexValue: 2.417f, physicalDispersionValue: 0.044f, absorptionStrengthValue: 0.34f, fresnelStrengthValue: 1.05f, backgroundDistortionValue: 1.16f, saturationBoostValue: 1.04f, contrastBoostValue: 1.08f);' `
    "Diamond material profile"

if ($text -eq $original) {
    throw "No changes were made. Aborting."
}

$backup = "$target.bak_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item -LiteralPath $target -Destination $backup -Force
Set-Content -LiteralPath $target -Value $text -Encoding UTF8

Write-Host "Done." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host "Next checks:" -ForegroundColor Cyan
Write-Host "  git diff -- $target"
Write-Host "  Unity compile"
Write-Host "  Test Diamond / Ruby / Opal visually"
