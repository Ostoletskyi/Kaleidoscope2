Shader "Kaleidoscope2/DiamondCrystal3D"
{
    Properties
    {
        _KaleidoscopeTex ("Kaleidoscope Texture", 2D) = "white" {}
        _KaleidoscopeTexValid ("Kaleidoscope Texture Valid", Float) = 1
        _FallbackWarningColor ("Missing Kaleidoscope Warning Color", Color) = (1, 0, 0.85, 1)
        _Transparency ("Transparency", Range(0,1)) = 0
        _RefractionStrength ("Refraction Strength", Range(0,0.12)) = 0.074
        _DispersionStrength ("Dispersion Strength", Range(0,2)) = 1
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.72
        _FresnelPower ("Fresnel Power", Range(0.5,8)) = 3.2
        _InternalBrightness ("Internal Brightness", Range(0,3)) = 1
        _NoiseDistortionStrength ("Noise Distortion Strength", Range(0,1)) = 0.08
        _EdgeHighlight ("Edge Highlight", Range(0,2)) = 1.35
        _ChromaticAberrationAmount ("Chromatic Aberration Amount", Float) = 0.004
        _FacetContrast ("Facet Contrast", Range(0,2)) = 1.45
        _InternalGlow ("Internal Glow", Range(0,1.5)) = 0.62
        _BloomBoost ("Bloom Boost", Range(0,3)) = 0.44
        _FocusAmount ("Focus Amount", Range(0,1)) = 0
        _Shape ("Shape", Float) = 0
        _CrystalMaterialMode ("Crystal Material Mode", Float) = 1
        _GeneratedColor ("Generated Color", Color) = (0.85, 0.96, 1, 1)
        _GeneratedMaterialKind ("Generated Material Kind", Float) = 0
        _SurfaceTint ("Surface Tint", Color) = (0.9, 0.98, 1, 1)
        _SurfaceTintStrength ("Surface Tint Strength", Range(0,1)) = 0.08
        _Metallic ("Metallic", Range(0,1)) = 0
        _Roughness ("Roughness", Range(0,1)) = 0.02
        _ScratchStrength ("Scratch Strength", Range(0,0.45)) = 0.035
        _Iridescence ("Iridescence", Range(0,1)) = 0.05
        _SurfacePatternStrength ("Surface Pattern Strength", Range(0,0.45)) = 0.025
        _Clarity ("Clarity", Range(0,1)) = 0.96
        _CrystalSurfaceFamily ("Crystal Surface Family", Float) = 1
        _SampleMipBias ("Sample Mip Bias", Range(-1,2)) = 0
        _DirectedLightIntensity ("Directed Crystal Light Intensity", Range(-10,10)) = 0
        _CrystalLightRigEnabled ("Crystal Light Rig Enabled", Float) = 1
        _CrystalRigLightIntensity ("Crystal Rig Light Intensity", Range(0,20)) = 8
        _CrystalRigGlintIntensity ("Crystal Rig Glint Intensity", Range(0,3)) = 1
        _CrystalRigRimIntensity ("Crystal Rig Rim Intensity", Range(0,3)) = 0.9
        _CrystalRigSpectralIntensity ("Crystal Rig Spectral Intensity", Range(0,3)) = 0.7
        _CrystalRigPulse ("Crystal Rig Pulse", Range(0,1)) = 0
        _CrystalRigOrbitPhase ("Crystal Rig Orbit Phase", Float) = 0
        _OpticalIOR ("Optical IOR", Range(0,10)) = 2.42
        _OpticalCaustics ("Optical Caustics", Range(0,2)) = 0.8
        _OpticalDispersion ("Optical Dispersion", Range(0,2)) = 1.45
        _TotalInternalReflection ("Total Internal Reflection", Range(0,2)) = 1.45
        _DiamondLikeRefraction ("Diamond-like Refraction", Range(0,2)) = 1.8
        _SpectralDispersion ("Spectral Dispersion", Range(0,3)) = 2.25
        _HighEnergyCaustics ("High Energy Caustics", Range(0,3)) = 1.85
        _MultiBounceInternalReflections ("Multi-bounce Internal Reflections", Range(0,3)) = 1.75
        _CinematicCrystalOptics ("Cinematic Crystal Optics", Range(0,2)) = 1.45
        _PhysicallyBasedRefraction ("Physically Based Refraction", Range(0,2)) = 1.55
        _DeepVolumetricLightScattering ("Deep Volumetric Light Scattering", Range(0,3)) = 1.35
        _CrystalSolidity ("Crystal Solidity", Range(0,1)) = 1
        _BlueWhitePlasmaEnergy ("Blue White Plasma Energy", Range(0,3)) = 2.05
        _DirectTransmission ("Direct Transmission", Range(0,1)) = 0.06
        _TotalInternalReturn ("Total Internal Return", Range(0,3)) = 1.65
        _SpectralFireIntensity ("Spectral Fire Intensity", Range(0,3)) = 1.85
        _FacetDepthContrast ("Facet Depth Contrast", Range(0,2)) = 1.22
        _Rotation ("Rotation", Vector) = (0, 0, 0, 0)
        _DiamondScale ("Diamond Scale", Range(0.1,1)) = 0.38
        _InputTexelSize ("Input Texel Size", Vector) = (0.001, 0.001, 1024, 1024)
        _CrystalDebugMode ("Crystal Debug Mode", Float) = 0
        _CrystalDebugEffectType ("Crystal Debug Effect Type", Float) = 0
        _CrystalDebugEffectBlend ("Crystal Debug Effect Blend", Range(0,1)) = 1
        _CrystalDebugMirrorBoost ("Crystal Debug Mirror Boost", Range(0,1)) = 0
        _CrystalDebugFrost ("Crystal Debug Frost", Range(0,1)) = 0
        _CrystalDebugNegative ("Crystal Debug Negative", Range(0,1)) = 0
        _CrystalDebugHalo ("Crystal Debug Halo", Range(0,2)) = 0
        _CrystalDebugStone ("Crystal Debug Stone", Range(0,1)) = 0
        _CrystalDebugChromatic ("Crystal Debug Chromatic", Range(0,2)) = 0
        _CrystalDebugGlimmer ("Crystal Debug Glimmer", Range(0,2)) = 0
        _CrystalDebugRainbow ("Crystal Debug Rainbow", Range(0,2)) = 0
        _CrystalDebugMirage ("Crystal Debug Mirage", Range(0,2)) = 0
        _CrystalDebugRoughness ("Crystal Debug Roughness", Range(0,1)) = 0
        _CrystalDebugContrast ("Crystal Debug Contrast", Range(0,1)) = 0
        _CrystalDebugBrightness ("Crystal Debug Brightness", Range(0,1)) = 0
        _CrystalDebugCracks ("Crystal Debug Cracks", Range(0,1)) = 0
        _CrystalDebugVeins ("Crystal Debug Veins", Range(0,1)) = 0
        _CrystalDebugEdgeGlow ("Crystal Debug Edge Glow", Range(0,2)) = 0
        _CrystalDebugFlare ("Crystal Debug Flare", Range(0,2)) = 0
        _CrystalDebugDistortion ("Crystal Debug Distortion", Range(0,2)) = 0
        _CrystalDebugSpeed ("Crystal Debug Speed", Range(0,2)) = 0
        _CrystalDebugSpectralSplit ("Crystal Debug Spectral Split", Range(0,2)) = 0
        _CrystalDebugTint ("Crystal Debug Tint", Color) = (1, 1, 1, 1)
        _CrystalDebugAbsoluteMirrorGuard ("Crystal Debug Absolute Mirror Guard", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _KaleidoscopeTex;
            float _KaleidoscopeTexValid;
            float4 _FallbackWarningColor;
            float _Transparency;
            float _RefractionStrength;
            float _DispersionStrength;
            float _ReflectionStrength;
            float _FresnelPower;
            float _InternalBrightness;
            float _NoiseDistortionStrength;
            float _EdgeHighlight;
            float _ChromaticAberrationAmount;
            float _FacetContrast;
            float _InternalGlow;
            float _BloomBoost;
            float _FocusAmount;
            float _Shape;
            float _CrystalMaterialMode;
            float4 _GeneratedColor;
            float _GeneratedMaterialKind;
            float4 _SurfaceTint;
            float _SurfaceTintStrength;
            float _Metallic;
            float _Roughness;
            float _ScratchStrength;
            float _Iridescence;
            float _SurfacePatternStrength;
            float _Clarity;
            float _CrystalSurfaceFamily;
            float _SampleMipBias;
            float _DirectedLightIntensity;
            float _CrystalLightRigEnabled;
            float _CrystalRigLightIntensity;
            float _CrystalRigGlintIntensity;
            float _CrystalRigRimIntensity;
            float _CrystalRigSpectralIntensity;
            float _CrystalRigPulse;
            float _CrystalRigOrbitPhase;
            float _OpticalIOR;
            float _OpticalCaustics;
            float _OpticalDispersion;
            float _TotalInternalReflection;
            float _DiamondLikeRefraction;
            float _SpectralDispersion;
            float _HighEnergyCaustics;
            float _MultiBounceInternalReflections;
            float _CinematicCrystalOptics;
            float _PhysicallyBasedRefraction;
            float _DeepVolumetricLightScattering;
            float _CrystalSolidity;
            float _BlueWhitePlasmaEnergy;
            float _DirectTransmission;
            float _TotalInternalReturn;
            float _SpectralFireIntensity;
            float _FacetDepthContrast;
            float4 _Rotation;
            float _DiamondScale;
            float4 _InputTexelSize;
            float _CrystalDebugMode;
            float _CrystalDebugEffectType;
            float _CrystalDebugEffectBlend;
            float _CrystalDebugMirrorBoost;
            float _CrystalDebugFrost;
            float _CrystalDebugNegative;
            float _CrystalDebugHalo;
            float _CrystalDebugStone;
            float _CrystalDebugChromatic;
            float _CrystalDebugGlimmer;
            float _CrystalDebugRainbow;
            float _CrystalDebugMirage;
            float _CrystalDebugRoughness;
            float _CrystalDebugContrast;
            float _CrystalDebugBrightness;
            float _CrystalDebugCracks;
            float _CrystalDebugVeins;
            float _CrystalDebugEdgeGlow;
            float _CrystalDebugFlare;
            float _CrystalDebugDistortion;
            float _CrystalDebugSpeed;
            float _CrystalDebugSpectralSplit;
            float4 _CrystalDebugTint;
            float _CrystalDebugAbsoluteMirrorGuard;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float2 Rotate2(float2 value, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float2(c * value.x - s * value.y, s * value.x + c * value.y);
            }

            float2 ClampOffset(float2 offset, float limit)
            {
                float safeLimit = max(0.0001, limit);
                return clamp(offset, float2(-safeLimit, -safeLimit), float2(safeLimit, safeLimit));
            }

            fixed3 SampleKaleidoscope(float2 uv)
            {
                float2 safeUv = saturate(uv);
                fixed3 baseSample = tex2D(_KaleidoscopeTex, safeUv).rgb;
                float blur = saturate(_Roughness * 0.7 + _SampleMipBias * 0.18);
                float2 texel = _InputTexelSize.xy * (1.0 + blur * 3.0);
                fixed3 softened = (
                    tex2D(_KaleidoscopeTex, saturate(safeUv + texel)).rgb +
                    tex2D(_KaleidoscopeTex, saturate(safeUv - texel)).rgb +
                    tex2D(_KaleidoscopeTex, saturate(safeUv + float2(texel.x, -texel.y))).rgb +
                    tex2D(_KaleidoscopeTex, saturate(safeUv + float2(-texel.x, texel.y))).rgb) * 0.25;
                return lerp(baseSample, softened, blur * 0.55);
            }

            fixed3 SampleRefracted(float2 uv, float2 offset, float chroma)
            {
                float2 redUv = saturate(uv + offset + offset * chroma);
                float2 greenUv = saturate(uv + offset * 0.72);
                float2 blueUv = saturate(uv + offset - offset * chroma);
                return fixed3(SampleKaleidoscope(redUv).r, SampleKaleidoscope(greenUv).g, SampleKaleidoscope(blueUv).b);
            }

            float2 NormalizeOrDefault2(float2 value, float2 fallback)
            {
                float lengthSq = dot(value, value);
                return lengthSq > 0.00001 ? value * rsqrt(lengthSq) : fallback;
            }

            fixed3 SampleThinDispersion(float2 refractedUv, float2 axis, float strength)
            {
                float2 safeAxis = NormalizeOrDefault2(axis, float2(0.7071, 0.7071));
                float shift = clamp(0.002 * (1.0 + strength * 0.85), 0.0008, 0.0095);
                float2 offset = safeAxis * shift;
                return fixed3(
                    SampleKaleidoscope(refractedUv + offset).r,
                    SampleKaleidoscope(refractedUv).g,
                    SampleKaleidoscope(refractedUv - offset).b);
            }

            float SmoothPattern(float3 value, float scale)
            {
                float wave = sin(dot(value, float3(12.9898, 78.233, 37.719)) * scale);
                return 0.5 + 0.5 * wave;
            }

            fixed3 SpectralPalette(float value)
            {
                float3 phase = float3(0.0, 0.36, 0.68);
                return saturate(0.5 + 0.5 * cos(6.2831853 * (value + phase)));
            }

            fixed3 CrystalContrastGrade(fixed3 color, float sparkle, float depth)
            {
                float sparkle01 = saturate(sparkle);
                float depth01 = saturate(depth);
                color *= lerp(0.32, 0.7, sparkle01);
                color = lerp(color * fixed3(0.22, 0.32, 0.52), color * fixed3(0.72, 0.82, 1.0), sparkle01 * 0.78);
                color += fixed3(0.035, 0.12, 0.34) * depth01 * 0.13;
                color += fixed3(0.42, 0.72, 1.0) * sparkle01 * 0.18;
                return saturate((color - 0.085) * (1.08 + depth01 * 0.22));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenUv = i.screenPos.xy / max(0.0001, i.screenPos.w);
                float debugMode = floor(_CrystalDebugMode + 0.5);
                float effectBlend = saturate(_CrystalDebugEffectBlend);
                float mirrorBoost = saturate(_CrystalDebugMirrorBoost * effectBlend);
                float frostAmount = saturate(_CrystalDebugFrost * effectBlend);
                float negativeAmount = saturate(_CrystalDebugNegative * effectBlend);
                float haloAmount = saturate(_CrystalDebugHalo * effectBlend);
                float stoneAmount = saturate(_CrystalDebugStone * effectBlend);
                float chromaticAmount = max(0.0, _CrystalDebugChromatic * effectBlend);
                float glimmerAmount = max(0.0, _CrystalDebugGlimmer * effectBlend);
                float rainbowAmount = max(0.0, _CrystalDebugRainbow * effectBlend);
                float mirageAmount = max(0.0, _CrystalDebugMirage * effectBlend);
                if (debugMode > 5.5 && debugMode < 6.5)
                {
                    return fixed4(0.0, 0.0, 0.0, 0.0);
                }

                if (_KaleidoscopeTexValid < 0.5)
                {
                    float checker = fmod(floor(screenUv.x * 10.0) + floor(screenUv.y * 10.0), 2.0);
                    fixed3 warning = lerp(fixed3(0.02, 0.0, 0.025), _FallbackWarningColor.rgb, checker);
                    return fixed4(warning, 1.0);
                }

                if (debugMode > 0.5 && debugMode < 1.5)
                {
                    return fixed4(SampleKaleidoscope(screenUv), 1.0);
                }

                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float ior = max(1.01, _OpticalIOR);
                float3 physicalRefract = refract(-viewDir, normal, 1.0 / ior);

                float ndv = abs(dot(normal, viewDir));
                float directedLight = clamp(_DirectedLightIntensity, -10.0, 10.0);
                float positiveLight = max(0.0, directedLight);
                float negativeLight = max(0.0, -directedLight);
                float lightGain = max(0.18, 1.0 + positiveLight * 0.18 - negativeLight * 0.075);
                float glintGain = max(0.12, 1.0 + positiveLight * 0.32 - negativeLight * 0.08);
                float internalLightGain = max(0.22, 1.0 + positiveLight * 0.16 - negativeLight * 0.065);
                float rigEnabled = step(0.5, _CrystalLightRigEnabled);
                float rigBase = rigEnabled * saturate(_CrystalRigLightIntensity / 20.0);
                float rigGlint = rigEnabled * max(0.0, _CrystalRigGlintIntensity);
                float rigRim = rigEnabled * max(0.0, _CrystalRigRimIntensity);
                float rigSpectral = rigEnabled * max(0.0, _CrystalRigSpectralIntensity);
                float rigPulse = rigEnabled * max(0.0, _CrystalRigPulse);
                lightGain += rigBase * 0.34 + rigRim * 0.18;
                glintGain += rigGlint * 0.82 + rigSpectral * 0.24;
                internalLightGain += rigBase * 0.2 + rigSpectral * 0.28;
                float fresnel = pow(saturate(1.0 - ndv), _FresnelPower);
                float fresnelReflection = saturate(fresnel * (0.85 + _ReflectionStrength * 0.75) * (0.75 + lightGain * 0.25));
                float edge = saturate(fresnel * (1.35 + lightGain * 0.18));
                float focus = saturate(_FocusAmount);
                float angle = atan2(normal.z, normal.x) + _Rotation.z * 0.0174532925;
                float facetCount = _Shape < 0.5 ? 8.0 : (_Shape < 1.5 ? 8.0 : (_Shape < 2.5 ? 16.0 : (_Shape < 3.5 ? 3.0 : (_Shape < 4.5 ? 4.0 : 12.0))));
                float facet = pow(saturate(0.5 + 0.5 * cos(angle * facetCount)), 1.6);
                float3 lightA = normalize(float3(-0.35, 0.72, -0.58));
                float3 lightB = normalize(float3(0.5, 0.25, -0.82));
                float3 directedLightDir = normalize(float3(-0.22, 0.78, -0.58));
                float rigPhase = _CrystalRigOrbitPhase + _Rotation.y * 0.002;
                float3 rigKeyDir = normalize(float3(cos(rigPhase) * 0.46, 0.78, sin(rigPhase) * 0.46));
                float3 rigRimDirA = normalize(float3(cos(rigPhase + 2.094), 0.16, sin(rigPhase + 2.094)));
                float3 rigRimDirB = normalize(float3(cos(rigPhase + 4.188), -0.08, sin(rigPhase + 4.188)));
                float3 rigGlintDirA = normalize(float3(cos(rigPhase * 1.37 + 0.62), 0.42, sin(rigPhase * 1.37 + 0.62)));
                float3 rigGlintDirB = normalize(float3(cos(rigPhase * -1.11 + 1.84), -0.36, sin(rigPhase * -1.11 + 1.84)));
                float highlightA = pow(saturate(dot(reflect(-lightA, normal), viewDir)), 26.0) * lightGain;
                float highlightB = pow(saturate(dot(reflect(-lightB, normal), viewDir)), 58.0) * lightGain;
                float rigKeyHighlight = pow(saturate(dot(reflect(-rigKeyDir, normal), viewDir)), 42.0) * rigBase * (0.75 + rigPulse * 0.25);
                float rigRimHighlight = (pow(saturate(dot(reflect(-rigRimDirA, normal), viewDir)), 54.0) + pow(saturate(dot(reflect(-rigRimDirB, normal), viewDir)), 48.0)) * rigRim;
                float rigOrbitNeedle = (pow(saturate(dot(reflect(-rigGlintDirA, normal), viewDir)), 142.0) + pow(saturate(dot(reflect(-rigGlintDirB, normal), viewDir)), 168.0)) * rigGlint * (0.78 + rigPulse * 0.62);
                highlightA += rigKeyHighlight * 0.74;
                highlightB += rigRimHighlight * 0.68;
                float facetKnife = pow(saturate(abs(facet - 0.5) * 2.0), 2.6);
                float facetLock = pow(saturate(dot(normal, normalize(directedLightDir + viewDir))), 120.0);
                float grazingLock = pow(saturate(dot(reflect(-directedLightDir, normal), viewDir)), 92.0);
                float glintGate = (facetLock * (0.32 + facetKnife * 1.25) + grazingLock * (0.22 + edge * 0.92)) * (0.55 + _FacetContrast * 0.25);
                glintGate += rigOrbitNeedle * (0.9 + facetKnife * 0.7 + edge * 0.45);
                float noiseA = SmoothPattern(i.worldPos + normal * 0.21, 2.1 + facetCount * 0.03) - 0.5;
                float noiseB = SmoothPattern(float3(i.uv, facet) + normal * 0.11, 3.7) - 0.5;

                float2 bend = normal.xy * _RefractionStrength * (0.95 + edge * 1.7 + focus * 0.55 + _DiamondLikeRefraction * 0.75);
                bend += physicalRefract.xy * _RefractionStrength * _PhysicallyBasedRefraction * (0.75 + (ior - 1.0) * 0.65);
                bend += (i.uv - 0.5) * _RefractionStrength * 0.25;
                bend += float2(noiseA, noiseB) * _RefractionStrength * _NoiseDistortionStrength * 0.45;
                float hazeTime = _Time.y * (0.7 + _CrystalDebugSpeed * 1.4);
                float2 hazeWave = float2(
                    sin(i.worldPos.y * 16.0 + hazeTime + i.worldPos.x * 5.0),
                    cos(i.worldPos.x * 13.0 - hazeTime * 1.13 + i.worldPos.z * 4.0));
                bend += hazeWave * _RefractionStrength * _CrystalDebugDistortion * mirageAmount * 0.42;
                bend += float2(noiseB, -noiseA) * _RefractionStrength * frostAmount * _CrystalDebugRoughness * 0.32;
                float maxBend = lerp(0.012, 0.075, saturate(_RefractionStrength / 0.085));
                bend = ClampOffset(bend, maxBend);
                float chroma = _ChromaticAberrationAmount * (28.0 + edge * 38.0 + focus * 18.0) * (1.0 + _SpectralDispersion * 0.85) * _DispersionStrength;
                chroma += (chromaticAmount * (0.01 + edge * 0.025) + rainbowAmount * edge * 0.012) * (1.0 + _CrystalDebugSpectralSplit);
                chroma = min(chroma, 0.42);
                fixed3 refracted = SampleRefracted(screenUv, bend, chroma);
                float2 spectralAxis = ClampOffset(normal.xy * chroma * 0.16 + physicalRefract.xy * chroma * 0.06, 0.035);
                fixed3 wideSpectrum = fixed3(
                    SampleKaleidoscope(screenUv + ClampOffset(bend * (1.35 + _DiamondLikeRefraction), maxBend * 1.55) + spectralAxis).r,
                    SampleKaleidoscope(screenUv + ClampOffset(bend * 0.42, maxBend * 0.65)).g,
                    SampleKaleidoscope(screenUv - ClampOffset(bend * (0.85 + _DiamondLikeRefraction * 0.4), maxBend * 1.25) - spectralAxis).b);
                float2 thinDispersionAxis = NormalizeOrDefault2(normal.xy + physicalRefract.xy * 0.42 + (i.uv - 0.5) * 0.2, float2(0.7071, 0.7071));
                fixed3 thinDispersion = SampleThinDispersion(saturate(screenUv + bend * 0.78), thinDispersionAxis, _DispersionStrength * (0.5 + _SpectralDispersion * 0.28));
                refracted = lerp(refracted, wideSpectrum, saturate(_SpectralDispersion * 0.28 + _DiamondLikeRefraction * 0.1));
                refracted = lerp(refracted, thinDispersion, saturate(0.16 + _SpectralDispersion * 0.055 + edge * 0.12));

                float3 reflectedVector = reflect(-viewDir, normal);
                float2 localUv = saturate(i.uv);
                float2 reflectionBase = saturate(lerp(screenUv, localUv, saturate(0.28 + _Metallic * 0.22 + _CrystalSolidity * 0.12)));
                float2 reflectedUv = saturate(reflectionBase + ClampOffset(Rotate2(reflectedVector.xy * (0.07 + edge * 0.035), _Rotation.y * 0.006) + normal.xy * 0.018, 0.11));
                fixed3 reflected = SampleKaleidoscope(reflectedUv);
                float2 reflectedUv2 = saturate(reflectionBase + ClampOffset(Rotate2(reflectedVector.yx * (0.085 + facet * 0.03) + normal.xy * 0.032, -_Rotation.x * 0.004), 0.12));
                float2 reflectedUv3 = saturate(reflectionBase + ClampOffset(Rotate2((reflectedVector.xy + physicalRefract.xy) * (0.055 + edge * 0.055), _Rotation.z * 0.005), 0.12));
                fixed3 multiBounce = (reflected + SampleKaleidoscope(reflectedUv2) + SampleKaleidoscope(reflectedUv3)) / 3.0;
                float2 internalUvA = saturate(reflectionBase + ClampOffset(Rotate2(reflectedVector.xy + normal.xy * 0.42, _Rotation.x * 0.004), 0.105));
                float2 internalUvB = saturate(reflectionBase + ClampOffset(Rotate2(-reflectedVector.yx + physicalRefract.xy * 0.55, _Rotation.z * -0.004), 0.105));
                fixed3 internalBounceColor = (multiBounce + SampleKaleidoscope(internalUvA) + SampleKaleidoscope(internalUvB)) / 3.0;
                reflected = lerp(reflected, multiBounce, saturate(_MultiBounceInternalReflections * 0.35));
                reflected = lerp(reflected, internalBounceColor, saturate(_MultiBounceInternalReflections * (0.12 + fresnelReflection * 0.42 + facetKnife * 0.16)));

                if (debugMode > 1.5 && debugMode < 2.5)
                {
                    return fixed4(refracted, 1.0);
                }

                if (debugMode > 2.5 && debugMode < 3.5)
                {
                    return fixed4(reflected, 1.0);
                }

                fixed3 cool = fixed3(0.64, 0.92, 1.0);
                fixed3 warm = fixed3(1.0, 0.78, 0.42);
                fixed3 fire = fixed3(
                    SampleKaleidoscope(screenUv + ClampOffset(bend * 1.9, maxBend * 1.65) + ClampOffset(float2(chroma, 0.0), 0.035)).r,
                    SampleKaleidoscope(screenUv - ClampOffset(bend * 1.4, maxBend * 1.35) + ClampOffset(float2(0.0, chroma), 0.035)).g,
                    SampleKaleidoscope(screenUv + ClampOffset(bend * 0.6, maxBend * 0.8) - ClampOffset(float2(chroma, chroma), 0.035)).b);
                float spectralPhase = facet + angle * 0.159 + dot(i.worldPos, float3(0.37, 0.61, 0.23)) + _Rotation.y * 0.003;
                fixed3 spectralFire = SpectralPalette(spectralPhase);
                fixed3 glintDispersion = SampleThinDispersion(saturate(screenUv + bend * 0.28), thinDispersionAxis, _DispersionStrength * (0.65 + _SpectralDispersion * 0.32));
                fixed3 glintColor = lerp(fixed3(1.0, 0.965, 0.88), saturate(glintDispersion + fire * spectralFire * (0.45 + rigSpectral * 0.45)), saturate(0.42 + rigSpectral * 0.18));
                float glintEnergy = min(4.6, glintGate * glintGain * (0.28 + fresnelReflection * 0.72 + _ReflectionStrength * 0.45 + rigGlint * 0.18) * (1.0 - saturate(_Roughness) * 0.62));

                if (debugMode > 3.5 && debugMode < 4.5)
                {
                    fixed3 raw = SampleKaleidoscope(screenUv);
                    return fixed4(saturate(abs(thinDispersion - raw) * 4.0 + glintDispersion * glintEnergy * 0.35), 1.0);
                }

                if (debugMode > 4.5 && debugMode < 5.5)
                {
                    return fixed4(normal * 0.5 + 0.5, 1.0);
                }

                if (debugMode > 6.5 && debugMode < 7.5)
                {
                    float bendLoad = saturate(length(bend) / max(0.0001, maxBend));
                    fixed3 uvStress = fixed3(saturate(abs(reflectedUv.x - screenUv.x) * 6.0), saturate(abs(reflectedUv.y - screenUv.y) * 6.0), bendLoad);
                    return fixed4(saturate(abs(reflected - refracted) * 2.0 + uvStress * 0.45), 1.0);
                }

                float causticPatternA = pow(saturate(SmoothPattern(i.worldPos + normal * 0.31, 2.4 + facetCount * 0.02) * facet + highlightA + highlightB), 5.5);
                float causticPatternB = pow(saturate(1.0 - abs(frac((angle + i.worldPos.y * 2.0) * facetCount * 0.14 + _Rotation.x * 0.002) - 0.5) * 2.0), 7.0);
                float causticEnergy = (causticPatternA + causticPatternB * edge) * _HighEnergyCaustics * (0.32 + _CinematicCrystalOptics * 0.35);
                float volumeCore = saturate(1.0 - length(i.uv - 0.5) * 1.55);
                float volumeDepth = pow(volumeCore, 1.8) * saturate(0.42 + normal.y * 0.32 + ndv * 0.38);
                float volumetricScatter = volumeDepth * _DeepVolumetricLightScattering * (0.35 + _CinematicCrystalOptics * 0.35 + focus * 0.16);

                float criticalAngle = saturate(1.0 / ior);
                float totalInternalReflection = smoothstep(criticalAngle * 0.38, 1.0, 1.0 - ndv) * _TotalInternalReflection;
                float returnLight = saturate((totalInternalReflection * _TotalInternalReturn + edge * 0.72 + (highlightA + highlightB) * 0.55 + glintEnergy * 0.22) * internalLightGain);
                float directTransmission = saturate(_DirectTransmission * (0.12 + ndv * 0.32) * (1.0 - totalInternalReflection * 0.88));
                float facetRidge = pow(saturate(abs(facet - 0.5) * 2.0), 0.58);
                float facetDepth = saturate(1.0 - facetRidge * 0.36 * _FacetDepthContrast - (1.0 - ndv) * 0.18 * _FacetDepthContrast);
                float internalBrightness = max(0.0, _InternalBrightness);

                fixed3 deepCore = fixed3(0.012, 0.055, 0.13);
                fixed3 icyBody = fixed3(0.46, 0.86, 1.0);
                fixed3 opticalSample = lerp(refracted, reflected, saturate(0.45 + fresnelReflection * 0.45 + totalInternalReflection * 0.25));
                fixed3 glass = lerp(deepCore, icyBody, saturate(edge * 0.55 + facet * 0.2 + volumetricScatter * 0.3));
                glass = lerp(glass, opticalSample, directTransmission);
                glass = lerp(glass, reflected, saturate(_ReflectionStrength * 0.22 + fresnelReflection * 0.25 + returnLight * 0.52));
                glass *= 0.58 + facetDepth * 0.5 + facet * _FacetContrast * 0.18;

                float whiteReturn = pow(saturate(returnLight + highlightA * 0.85 + highlightB * 1.15 + glintEnergy * 0.58), 2.05);
                float spectralBand = pow(saturate(causticPatternA * 0.55 + causticPatternB * 0.65 + facet * edge * 0.42), 1.65);
                float plasmaNeedle = pow(saturate(causticPatternA * 0.65 + causticPatternB * 0.75 + highlightA * 1.2 + highlightB * 1.6 + edge * facet * 0.35), 2.15);
                float plasmaEnergy = plasmaNeedle * _BlueWhitePlasmaEnergy * (0.45 + _HighEnergyCaustics * 0.42 + focus * 0.25);
                fixed3 coldPlasma = lerp(fixed3(0.12, 0.46, 1.0), fixed3(1.0, 1.0, 0.94), saturate(plasmaEnergy * 0.55 + whiteReturn * 0.25));
                fixed3 prismaticFire = saturate(fire * (0.72 + spectralFire * 0.42)) * spectralBand * (_SpectralFireIntensity * (0.38 + _SpectralDispersion * 0.22));
                prismaticFire *= 1.0 + rigSpectral * 0.34;

                glass += cool * edge * _EdgeHighlight * (0.45 + totalInternalReflection * 0.55);
                glass += fixed3(1.0, 0.97, 0.88) * whiteReturn * (0.62 + _BloomBoost * 0.22) * internalBrightness;
                glass += glintColor * glintEnergy * (0.74 + _BloomBoost * 0.18) * internalBrightness;
                glass += SpectralPalette(rigPhase * 0.37 + facet) * rigOrbitNeedle * rigSpectral * 0.42 * internalBrightness;
                glass += prismaticFire * internalBrightness;
                glass += coldPlasma * plasmaEnergy * internalBrightness;
                glass += causticEnergy * fixed3(0.78, 0.92, 1.0) * (0.72 + _SpectralFireIntensity * 0.18) * internalBrightness;
                glass += volumetricScatter * lerp(fixed3(0.14, 0.38, 0.78), spectralFire, saturate(_SpectralDispersion * 0.18)) * internalBrightness;
                glass += cool * _InternalGlow * saturate(0.4 + normal.y * 0.6) * 0.28 * internalBrightness;
                glass = lerp(glass, glass * facetDepth + deepCore * (1.0 - facetDepth) * 0.95, saturate(_CrystalSolidity * 0.55));
                glass = lerp(glass, pow(saturate(glass), 0.78), saturate(_CinematicCrystalOptics * 0.45));
                glass = saturate((glass - 0.035) * (1.25 + _CrystalSolidity * 0.35));

                float solidAlpha = saturate(1.0 - _Transparency * 0.85);
                float family = floor(_CrystalSurfaceFamily + 0.5);
                fixed3 tint = saturate(_SurfaceTint.rgb);
                float tintStrength = saturate(_SurfaceTintStrength);
                float clarity = saturate(_Clarity);
                float roughness = saturate(_Roughness);
                float microPattern = (SmoothPattern(i.worldPos * 0.73 + normal * 0.19, 3.1 + facetCount * 0.04) - 0.5) * _SurfacePatternStrength;
                float scratchMask = pow(saturate(abs(sin(dot(i.worldPos, float3(21.7, 9.3, 14.1)) * 11.0 + _Rotation.y * 0.01))), 18.0) * _ScratchStrength;
                fixed3 tintFilter = lerp(fixed3(1.0, 1.0, 1.0), tint, tintStrength);
                fixed3 filteredRefracted = refracted * tintFilter;
                fixed3 filteredReflected = reflected * lerp(fixed3(1.0, 1.0, 1.0), tint, tintStrength * 0.55);
                fixed3 filteredOptical = opticalSample * tintFilter;
                fixed3 surfaceBody = lerp(filteredOptical, glass * tintFilter, saturate(0.45 + clarity * 0.35));
                surfaceBody *= 1.0 + microPattern * 0.18;
                surfaceBody = lerp(surfaceBody, filteredReflected, saturate(_ReflectionStrength * 0.24 + roughness * 0.08));

                fixed3 finalCrystal = surfaceBody;
                float sparkle = whiteReturn + edge + highlightA + highlightB;

                if (family < 0.5)
                {
                    finalCrystal = filteredReflected * (1.0 + edge * 0.18);
                    finalCrystal = lerp(finalCrystal, filteredRefracted, saturate(0.08 + _DispersionStrength * 0.08));
                    finalCrystal += prismaticFire * 0.22 + glintColor * glintEnergy * 0.92 + fixed3(1.0, 0.98, 0.92) * (highlightA * 0.7 + highlightB * 1.0);
                    sparkle += causticEnergy * 0.35;
                }
                else if (family < 1.5)
                {
                    finalCrystal = lerp(filteredRefracted, glass * tintFilter, 0.74);
                    finalCrystal = lerp(finalCrystal, filteredReflected, saturate(returnLight * 0.5 + _ReflectionStrength * 0.18));
                    finalCrystal += prismaticFire * 0.55 + glintColor * glintEnergy * 1.18 + coldPlasma * plasmaEnergy * 0.42;
                    sparkle += plasmaEnergy * 0.45;
                }
                else if (family < 2.5)
                {
                    finalCrystal = lerp(filteredRefracted, filteredOptical, 0.5 + clarity * 0.22);
                    finalCrystal = lerp(finalCrystal, glass * tintFilter, 0.24);
                    finalCrystal = lerp(finalCrystal, filteredReflected, saturate(_ReflectionStrength * 0.2 + edge * 0.18));
                    finalCrystal += prismaticFire * (0.18 + _Iridescence * 0.18) + glintColor * glintEnergy * 0.72 + volumetricScatter * tint * 0.18;
                    sparkle += _Iridescence * 0.22;
                }
                else if (family < 3.5)
                {
                    fixed3 pearl = lerp(tint, spectralFire, _Iridescence * edge);
                    finalCrystal = lerp(filteredOptical, filteredReflected, 0.16 + roughness * 0.12);
                    finalCrystal = lerp(finalCrystal, finalCrystal * pearl, tintStrength * 0.38);
                    finalCrystal += fixed3(1.0, 1.0, 1.0) * (highlightA * 0.28 + highlightB * 0.62);
                    finalCrystal += spectralFire * edge * _Iridescence * 0.18 + glintColor * glintEnergy * 0.45;
                }
                else
                {
                    fixed3 metalReflection = filteredReflected * (1.05 - roughness * 0.22);
                    metalReflection += lerp(fixed3(0.88, 0.92, 0.96), tint, tintStrength) * (highlightA * 0.5 + highlightB * 1.15 + edge * 0.16 + glintEnergy * 0.55);
                    finalCrystal = lerp(filteredOptical, metalReflection, saturate(0.68 + _Metallic * 0.3));
                    finalCrystal *= 1.0 - roughness * 0.12;
                    sparkle += _Metallic * 0.35;
                }

                finalCrystal = lerp(finalCrystal, finalCrystal * (0.94 + facetDepth * 0.12), saturate(_FacetDepthContrast * 0.32));
                finalCrystal *= 1.0 - scratchMask * (0.22 + roughness * 0.18);
                finalCrystal += fixed3(1.0, 0.98, 0.92) * scratchMask * (0.06 + edge * 0.08);
                finalCrystal += glintColor * glintEnergy * 0.28;
                sparkle += glintEnergy * 1.35 + fresnelReflection * 0.42;
                fixed3 effectColor = CrystalContrastGrade(finalCrystal, sparkle, 1.0 - facetDepth);

                fixed3 polishedReflection = saturate(filteredReflected * (1.16 + edge * 0.18)
                    + fixed3(1.0, 0.98, 0.94) * (highlightA + highlightB + glintEnergy) * 0.34);
                effectColor = lerp(effectColor, polishedReflection, mirrorBoost);

                float abrasion = SmoothPattern(i.worldPos + normal * 0.37, 8.4);
                fixed3 frostColor = lerp(effectColor * (0.66 + abrasion * 0.18), _CrystalDebugTint.rgb * (0.35 + edge * 0.24), 0.36);
                frostColor += fixed3(0.68, 0.9, 0.88) * edge * 0.16;
                effectColor = lerp(effectColor, frostColor, frostAmount);

                float crackWave = abs(sin(dot(i.worldPos, float3(15.3, 9.7, 12.1)) * 4.1 + abrasion * 3.2));
                float crackMask = pow(saturate(1.0 - crackWave * 7.0), 1.4) * _CrystalDebugCracks;
                float veinMask = pow(saturate(0.55 + 0.45 * sin(dot(i.worldPos, float3(5.1, 8.6, 3.4)) * 3.3 + abrasion * 4.0)), 2.8) * _CrystalDebugVeins;
                fixed3 stoneColor = _CrystalDebugTint.rgb * (0.46 + abrasion * 0.32);
                stoneColor = lerp(stoneColor, fixed3(0.67, 0.58, 0.42), veinMask * 0.46);
                stoneColor *= 1.0 - crackMask * 0.72;
                stoneColor += fixed3(0.16, 0.12, 0.08) * facetDepth * 0.18;
                effectColor = lerp(effectColor, stoneColor, stoneAmount);

                fixed3 prismAura = SpectralPalette(spectralPhase + _Time.y * 0.05) * (edge * 0.44 + facetKnife * 0.18);
                effectColor += prismAura * rainbowAmount * 0.78;
                effectColor += fixed3(0.38, 0.74, 1.0) * edge * haloAmount * _CrystalDebugEdgeGlow * 0.34;
                float animatedGlint = pow(saturate(glintGate + edge * 0.24 + sin(_Time.y * 2.2 + spectralPhase * 11.0) * 0.08), 4.2);
                effectColor += lerp(fixed3(1.0, 0.96, 0.82), _CrystalDebugTint.rgb, 0.34)
                    * animatedGlint * glimmerAmount * (0.46 + _CrystalDebugFlare * 0.38);
                effectColor += _CrystalDebugTint.rgb * saturate(hazeWave.x * hazeWave.y + 0.2) * mirageAmount * edge * 0.08;

                effectColor = lerp(effectColor, 1.0 - saturate(effectColor), negativeAmount);
                float debugGradeAmount = step(0.0001, _CrystalDebugContrast + _CrystalDebugBrightness) * effectBlend;
                fixed3 gradedEffectColor = saturate((effectColor - 0.5) * (1.0 + _CrystalDebugContrast) + 0.5
                    + _CrystalDebugBrightness);
                effectColor = lerp(effectColor, gradedEffectColor, debugGradeAmount);
                solidAlpha = lerp(solidAlpha, 1.0, saturate(stoneAmount + mirrorBoost));
                solidAlpha = max(solidAlpha, saturate(_CrystalDebugAbsoluteMirrorGuard) * 0.98);
                return fixed4(effectColor, solidAlpha);
            }
            ENDCG
        }
    }
}
