Shader "Kaleidoscope2/RealCrystalOptics"
{
    Properties
    {
        _KaleidoscopeTex ("Kaleidoscope Texture", 2D) = "white" {}
        _HiddenReflectionTex ("Hidden Reflection Texture", 2D) = "black" {}
        _HiddenReflectionTexValid ("Hidden Reflection Texture Valid", Float) = 0
        _HiddenReflectionStrength ("Hidden Reflection Strength", Range(0,1.5)) = 0.82
        _DirectTransmission ("Direct Transmission", Range(0,0.35)) = 0.055
        _Tint ("Tint", Color) = (0.9, 0.98, 1, 1)
        _GemCoreColor ("Gem Core Color", Color) = (0.74, 0.9, 1, 1)
        _GemFireColor ("Gem Fire Color", Color) = (1, 0.86, 0.34, 1)
        _Intensity ("Intensity", Range(0,20)) = 5.2
        _Alpha ("Alpha", Range(0,1)) = 0.58
        _Metallic ("Metallic", Range(0,1)) = 0.02
        _Smoothness ("Smoothness", Range(0,1)) = 0.96
        _Transparency ("Transparency", Range(0,1)) = 0
        _RefractionStrength ("Refraction Strength", Range(0,0.28)) = 0.085
        _ScreenRefractionStrength ("Screen Refraction Strength", Range(0,0.24)) = 0.028
        _FresnelPower ("Fresnel Power", Range(0.5,8)) = 3.2
        _ReflectionStrength ("Reflection Strength", Range(0,1.5)) = 0.6
        _InternalBrightness ("Internal Brightness", Range(0,3)) = 0.68
        _MinimumTransmission ("Minimum Transmission", Range(0,1)) = 0.045
        _SpecularStrength ("Specular Strength", Range(0,1)) = 0.75
        _RimStrength ("Rim Response", Range(0,1.5)) = 0.75
        _BrightnessFloor ("Brightness Floor", Range(0,0.35)) = 0.035
        _GemTintStrength ("Gem Tint Strength", Range(0,1)) = 0.16
        _OpticalDensity ("Optical Density", Range(0,3)) = 0.72
        _FacetRefraction ("Facet Refraction", Range(0,3)) = 1.12
        _ThicknessRefraction ("Thickness Refraction", Range(0,3)) = 1.05
        _InternalReflectionStrength ("Internal Reflection", Range(0,3)) = 1.18
        _DispersionStrength ("Spectral Dispersion", Range(0,3)) = 0.82
        _FacetFire ("Facet Fire", Range(0,3)) = 0.68
        _DepthAbsorption ("Depth Absorption", Range(0,1)) = 0.42
        _Clarity ("Clarity", Range(0,1)) = 0.9
        _FacetContrast ("Facet Contrast", Range(0,2.2)) = 1.5
        _RefractiveIndex ("Refractive Index", Range(1,2.9)) = 2.417
        _PhysicalDispersion ("Physical Dispersion", Range(0,0.18)) = 0.044
        _AbsorptionStrength ("Absorption Strength", Range(0,3)) = 0.38
        _FresnelStrength ("Fresnel Strength", Range(0,3)) = 1.05
        _BackgroundDistortionStrength ("Background Distortion", Range(0,3)) = 1.15
        _SaturationBoost ("Saturation Boost", Range(0,3)) = 1.08
        _ContrastBoost ("Contrast Boost", Range(0,3)) = 1.08
        _OpalIridescence ("Opal Iridescence", Range(0,1)) = 0
        _HighlightCompression ("Highlight Compression", Range(0,4)) = 1.25
        _MaxCoreTransmission ("Max Core Transmission", Range(0,0.35)) = 0.045
        _CenterTransmissionBlock ("Center Transmission Block", Range(0,1.5)) = 0.95
        _ChromaticAberrationScale ("Chromatic Aberration Scale", Range(0,3)) = 1
        _SpectralSplitScale ("Spectral Split Scale", Range(0,4)) = 1
        _CoreDarkening ("Core Darkening", Range(0,1)) = 0.18
        _AbsoluteMirrorStrength ("Absolute Mirror Strength", Range(0,1)) = 0
        _CrystalDebugMode ("Crystal Debug Mode", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 250
        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf StandardSpecular alpha:fade
        #pragma target 3.0

        sampler2D _KaleidoscopeTex;
        sampler2D _HiddenReflectionTex;
        half _HiddenReflectionTexValid;
        half _HiddenReflectionStrength;
        half _DirectTransmission;
        fixed4 _Tint;
        fixed4 _GemCoreColor;
        fixed4 _GemFireColor;
        half _Intensity;
        half _Alpha;
        half _Metallic;
        half _Smoothness;
        half _Transparency;
        half _RefractionStrength;
        half _ScreenRefractionStrength;
        half _FresnelPower;
        half _ReflectionStrength;
        half _InternalBrightness;
        half _MinimumTransmission;
        half _SpecularStrength;
        half _RimStrength;
        half _BrightnessFloor;
        half _GemTintStrength;
        half _OpticalDensity;
        half _FacetRefraction;
        half _ThicknessRefraction;
        half _InternalReflectionStrength;
        half _DispersionStrength;
        half _FacetFire;
        half _DepthAbsorption;
        half _Clarity;
        half _FacetContrast;
        half _RefractiveIndex;
        half _PhysicalDispersion;
        half _AbsorptionStrength;
        half _FresnelStrength;
        half _BackgroundDistortionStrength;
        half _SaturationBoost;
        half _ContrastBoost;
        half _OpalIridescence;
        half _HighlightCompression;
        half _MaxCoreTransmission;
        half _CenterTransmissionBlock;
        half _ChromaticAberrationScale;
        half _SpectralSplitScale;
        half _CoreDarkening;
        half _AbsoluteMirrorStrength;
        half _CrystalDebugMode;

        struct Input
        {
            float2 uv_KaleidoscopeTex;
            float3 viewDir;
            float3 worldNormal;
            float3 worldPos;
            float4 screenPos;
        };

        float3 SoftCompressHighlights(float3 c, float strength)
        {
            float peak = max(c.r, max(c.g, c.b));
            return c / (1.0 + peak * max(0.0, strength));
        }

        float2 MirrorWrapUv(float2 uv)
        {
            float2 wrapped = frac(uv * 0.5);
            return 1.0 - abs(wrapped * 2.0 - 1.0);
        }

        void surf(Input IN, inout SurfaceOutputStandardSpecular o)
        {
            float3 viewDir = normalize(IN.viewDir);
            float3 normal = normalize(IN.worldNormal);
            float ndv = saturate(dot(viewDir, normal));
            float safeIor = max(1.01, _RefractiveIndex);
            float f0 = pow((safeIor - 1.0) / (safeIor + 1.0), 2.0);
            float schlick = f0 + (1.0 - f0) * pow(1.0 - ndv, 5.0);
            float fresnelCurve = pow(1.0 - ndv, max(0.5, _FresnelPower));
            float fresnel = saturate((fresnelCurve * 0.58 + schlick * 1.86) * _FresnelStrength);
            float intensity01 = saturate(_Intensity / 20.0);
            float debugMode = floor(_CrystalDebugMode + 0.5);

            if (debugMode > 5.5 && debugMode < 6.5)
            {
                o.Albedo = 0;
                o.Specular = 0;
                o.Smoothness = 0;
                o.Emission = 0;
                o.Alpha = 0;
                return;
            }

            float2 surfaceUv = saturate(IN.uv_KaleidoscopeTex);
            float2 screenUv = saturate(IN.screenPos.xy / max(0.0001, IN.screenPos.w));
            float2 baseUv = lerp(surfaceUv, screenUv, 0.88);
            float centerDepth = pow(saturate(1.0 - length(screenUv - 0.5) * 2.08), 1.32);
            float facetPlaneA = abs(dot(normal, normalize(float3(0.64, 0.3, -0.7))));
            float facetPlaneB = abs(dot(normal, normalize(float3(-0.44, 0.78, -0.45))));
            float facetPlaneC = abs(dot(normal, normalize(float3(0.18, -0.92, -0.35))));
            float facetMask = pow(saturate(abs(normal.x) * 0.4 + abs(normal.z) * 0.38 + abs(normal.y) * 0.2 + facetPlaneA * 0.16), max(0.65, _FacetContrast));
            float facetBreak = saturate(facetMask * 0.62 + facetPlaneA * 0.18 + facetPlaneB * 0.13 + facetPlaneC * 0.1);
            float thickness = saturate((1.0 - ndv) * 0.5 + facetBreak * 0.36 + _OpticalDensity * 0.22 + centerDepth * 0.42);
            float deepCore = saturate(centerDepth * (0.56 + _OpticalDensity * 0.22) + thickness * 0.34 + facetBreak * 0.12);
            float3 refractedVector = refract(-viewDir, normal, 1.0 / safeIor);
            float3 reflectedVector = reflect(-viewDir, normal);
            float totalInternalFeel = saturate(schlick * 1.72 + facetBreak * 0.18 + thickness * 0.2 + deepCore * 0.32);
            float mirrorMode = saturate(_AbsoluteMirrorStrength);
            float2 facetAxis = normalize(normal.xy + float2(normal.z, -normal.x) * 0.42 + refractedVector.xy * 0.36 + float2(0.001, -0.001));
            float distortionScale = _BackgroundDistortionStrength * (0.78 + (safeIor - 1.0) * 0.42);
            float2 facetOffset = (normal.xy * 1.04 - viewDir.xy * 0.24 + facetAxis * (0.22 + facetBreak * 0.16))
                * _ScreenRefractionStrength
                * _FacetRefraction
                * distortionScale
                * (0.64 + thickness * 1.08 + fresnel * 0.62 + deepCore * 0.72);
            float2 depthOffset = (normal.xy * 0.34 + refractedVector.xy * 0.56 + facetAxis * 0.38)
                * _RefractionStrength
                * _ThicknessRefraction
                * distortionScale
                * (0.22 + thickness * 0.92 + totalInternalFeel * 0.22 + deepCore * 0.58);
            float splitScale = max(0.0, _SpectralSplitScale);
            float chromaScale = max(0.0, _ChromaticAberrationScale);
            float dispersionScale = 0.35 + chromaScale * 0.55 + splitScale * 0.45;
            float2 dispersionOffset = facetAxis
                * _ScreenRefractionStrength
                * _DispersionStrength
                * (0.62 + _PhysicalDispersion * 18.0)
                * (0.14 + fresnel * 0.58 + facetBreak * 0.34 + deepCore * 0.16)
                * dispersionScale;

            float2 refractedUv = MirrorWrapUv(screenUv + facetOffset + depthOffset);
            float2 frontLayerUv = MirrorWrapUv(screenUv + facetOffset * (0.42 + deepCore * 0.18) - depthOffset * 0.24 + facetAxis * deepCore * 0.018);
            float2 backLayerUv = MirrorWrapUv(screenUv - facetOffset * (0.72 + deepCore * 0.34) + depthOffset * (1.52 + deepCore * 0.42));
            float echoScale = 0.65 + splitScale * 0.35;
            float2 echoLayerUv = MirrorWrapUv(screenUv - facetOffset * (1.32 + deepCore * 0.48) - depthOffset * (0.52 + deepCore * 0.22) + facetAxis * (fresnel * 0.035 + totalInternalFeel * 0.02 + deepCore * 0.04) * echoScale);
            float2 reflectionUv = MirrorWrapUv(screenUv - facetOffset * (0.92 + deepCore * 0.32) - depthOffset * (0.4 + deepCore * 0.24) + facetAxis * (fresnel * 0.03 + deepCore * 0.05) * echoScale);

            fixed3 sourceColor = tex2D(_KaleidoscopeTex, baseUv).rgb;
            fixed3 frontLayerColor = tex2D(_KaleidoscopeTex, frontLayerUv).rgb;
            fixed3 refractedColor;
            refractedColor.r = tex2D(_KaleidoscopeTex, MirrorWrapUv(refractedUv + dispersionOffset)).r;
            refractedColor.g = tex2D(_KaleidoscopeTex, refractedUv).g;
            refractedColor.b = tex2D(_KaleidoscopeTex, MirrorWrapUv(refractedUv - dispersionOffset)).b;
            fixed3 backLayerColor = tex2D(_KaleidoscopeTex, backLayerUv).rgb;
            fixed3 echoLayerColor;
            echoLayerColor.r = tex2D(_KaleidoscopeTex, MirrorWrapUv(echoLayerUv + dispersionOffset * (1.1 + splitScale * 0.45))).r;
            echoLayerColor.g = tex2D(_KaleidoscopeTex, MirrorWrapUv(echoLayerUv - dispersionOffset * 0.22)).g;
            echoLayerColor.b = tex2D(_KaleidoscopeTex, MirrorWrapUv(echoLayerUv - dispersionOffset * (1.0 + splitScale * 0.45))).b;
            fixed3 internalReflectionColor = tex2D(_KaleidoscopeTex, reflectionUv).rgb;
            fixed3 centerEchoColor;
            centerEchoColor.r = tex2D(_KaleidoscopeTex, MirrorWrapUv(backLayerUv + dispersionOffset * (1.5 + splitScale * 0.6) - facetAxis * 0.035)).r;
            centerEchoColor.g = tex2D(_KaleidoscopeTex, MirrorWrapUv(echoLayerUv + facetAxis * 0.028)).g;
            centerEchoColor.b = tex2D(_KaleidoscopeTex, MirrorWrapUv(reflectionUv - dispersionOffset * (1.4 + splitScale * 0.6) + facetAxis * 0.02)).b;
            float hiddenReflectionValid = saturate(_HiddenReflectionTexValid);
            float hiddenReflectionAmount = hiddenReflectionValid * saturate(max(_HiddenReflectionStrength, mirrorMode));
            float2 hiddenReflectionUv = MirrorWrapUv(0.5 + reflectedVector.xy * (0.34 + fresnel * 0.14 + deepCore * 0.12) + facetAxis * (facetBreak * 0.055 + fresnel * 0.045 + deepCore * 0.05) - facetOffset * 0.46 - depthOffset * (0.22 + deepCore * 0.18));
            fixed3 hiddenReflectionColor;
            hiddenReflectionColor.r = tex2D(_HiddenReflectionTex, MirrorWrapUv(hiddenReflectionUv + dispersionOffset * 0.74)).r;
            hiddenReflectionColor.g = tex2D(_HiddenReflectionTex, hiddenReflectionUv).g;
            hiddenReflectionColor.b = tex2D(_HiddenReflectionTex, MirrorWrapUv(hiddenReflectionUv - dispersionOffset * 0.86)).b;
            hiddenReflectionColor = lerp(internalReflectionColor, hiddenReflectionColor, hiddenReflectionAmount);
            fixed3 mixedReflectionColor = lerp(internalReflectionColor, saturate(hiddenReflectionColor * (1.12 + facetBreak * 0.28)), hiddenReflectionAmount);

            fixed3 deepReplacement = saturate(centerEchoColor * 0.72 + mixedReflectionColor * 0.46 + backLayerColor * 0.22);
            sourceColor = lerp(sourceColor, deepReplacement, saturate(deepCore * 0.96));

            float gemAbsorption = saturate(_AbsorptionStrength * (0.16 + thickness * 0.56 + _DepthAbsorption * 0.36) + deepCore * (0.24 + _OpticalDensity * 0.18));
            fixed3 absorptionTint = lerp(_Tint.rgb, _GemCoreColor.rgb, 0.7);
            fixed3 absorptionFilter = lerp(fixed3(1.0, 1.0, 1.0), absorptionTint, gemAbsorption);
            fixed3 absorbedFront = lerp(frontLayerColor, frontLayerColor * absorptionFilter, saturate(_GemTintStrength + thickness * 0.16 + deepCore * 0.12));
            fixed3 absorbedBack = backLayerColor * lerp(absorptionFilter, _GemCoreColor.rgb, saturate(_DepthAbsorption * 0.48 + thickness * 0.2 + deepCore * 0.28));
            fixed3 absorbedRefraction = refractedColor * lerp(fixed3(1.0, 1.0, 1.0), absorptionFilter, saturate(0.42 + thickness * 0.42 + deepCore * 0.24));
            fixed3 internalColor = lerp(sourceColor, absorbedRefraction, saturate(0.78 + _RefractionStrength * 2.7 + thickness * 0.24 + hiddenReflectionAmount * 0.1 + deepCore * 0.12));
            internalColor = lerp(internalColor, absorbedFront, saturate(0.16 + facetBreak * 0.16));
            internalColor = lerp(internalColor, absorbedBack, saturate(0.24 + _OpticalDensity * 0.2 + thickness * 0.38 + deepCore * 0.32));
            internalColor = lerp(internalColor, echoLayerColor, saturate(_InternalReflectionStrength * (0.08 + totalInternalFeel * 0.2 + facetBreak * 0.11 + deepCore * 0.24)));
            internalColor = lerp(internalColor, centerEchoColor, saturate(_InternalReflectionStrength * deepCore * (0.28 + hiddenReflectionAmount * 0.08)));
            internalColor = lerp(internalColor, mixedReflectionColor, saturate(_InternalReflectionStrength * (0.18 + fresnel * 0.36 + facetBreak * 0.22 + hiddenReflectionAmount * 0.14 + deepCore * 0.22)));

            float luma = dot(internalColor, fixed3(0.299, 0.587, 0.114));
            internalColor = lerp(fixed3(luma, luma, luma), internalColor, max(0.0, _SaturationBoost));
            internalColor = saturate((internalColor - 0.5) * max(0.05, _ContrastBoost) + 0.5);
            float opalSeed = frac(dot(abs(IN.worldPos), float3(8.71, 13.17, 5.31)) + facetBreak * 2.37 + ndv * 0.41);
            fixed3 opalA = fixed3(0.52, 0.95, 1.0);
            fixed3 opalB = fixed3(1.0, 0.52, 0.88);
            fixed3 opalC = fixed3(1.0, 0.92, 0.36);
            fixed3 opalLayer = lerp(lerp(opalA, opalB, opalSeed), opalC, saturate(abs(opalSeed - 0.5) * 1.65));
            internalColor = lerp(internalColor, saturate(internalColor * 0.74 + opalLayer * 0.32 + sourceColor * 0.2), _OpalIridescence * saturate(0.34 + fresnel * 0.38 + facetBreak * 0.22));

            float directTransmission = saturate(_DirectTransmission);
            float volumeBlock = saturate(deepCore * 0.82 + thickness * 0.22 + totalInternalFeel * 0.18);
            float centerBlock = saturate(deepCore * _CenterTransmissionBlock);

            // Keep direct transmission at the edges, but prevent the deep center from acting like a clean window.
            float rawTransmission = max(_MinimumTransmission * 0.08, directTransmission)
                * lerp(0.50, 0.78, _Clarity)
                * (0.42 + ndv * 0.20 + thickness * 0.10)
                * (1.0 - volumeBlock * 0.62)
                * (1.0 - mirrorMode);

            float transmission = saturate(min(
                rawTransmission * (1.0 - centerBlock * 0.88),
                lerp(rawTransmission, _MaxCoreTransmission, centerBlock)
            ));
            transmission = lerp(transmission, 0.0, mirrorMode);
            float internalGain = saturate(_InternalBrightness * (0.24 + thickness * 0.2 + fresnel * 0.18 + totalInternalFeel * 0.1));
            float reflection = clamp(_ReflectionStrength, 0.0, 1.5);
            fixed3 tintedInternal = lerp(internalColor, internalColor * _Tint.rgb, saturate(_GemTintStrength + thickness * 0.18));
            fixed3 coreBody = _GemCoreColor.rgb * (0.045 + _BrightnessFloor * 0.2) * (1.0 - deepCore * _CoreDarkening);
            fixed3 glassBase = lerp(coreBody, tintedInternal, transmission);
            fixed3 fresnelReflection = lerp(_Tint.rgb, _GemFireColor.rgb, saturate(facetBreak * 0.35 + _DispersionStrength * 0.2)) * fresnel * (0.22 + reflection * 0.58);
            fresnelReflection += mixedReflectionColor * hiddenReflectionAmount * fresnel * (0.18 + reflection * 0.38 + facetBreak * 0.2);
            fixed3 mirrorBody = saturate(mixedReflectionColor * (0.72 + reflection * 0.28) + fresnelReflection * 0.9 + _Tint.rgb * (0.06 + facetBreak * 0.04));
            glassBase = lerp(glassBase, mirrorBody, mirrorMode);
            fixed3 specularColor = lerp(fixed3(0.04, 0.045, 0.05), _Tint.rgb * (0.35 + reflection * 0.45), saturate(_SpecularStrength));
            float edgeRim = pow(saturate(fresnel), 2.05) * _RimStrength;
            float3 keyDir = normalize(float3(0.38, 0.76, -0.52));
            float3 sideDir = normalize(float3(-0.72, 0.34, -0.6));
            float3 crownDir = normalize(float3(0.18, 0.92, -0.34));
            float keyHighlight = pow(saturate(dot(reflect(-keyDir, normal), viewDir)), 88.0) * (0.7 + facetBreak * 0.45);
            float edgeHighlight = pow(saturate(dot(reflect(-sideDir, normal), viewDir)), 116.0) * (0.62 + totalInternalFeel * 0.56);
            float crownHighlight = pow(saturate(dot(reflect(-crownDir, normal), viewDir)), 52.0) * (0.42 + facetPlaneB * 0.5);
            fixed3 facetFire = (fixed3(1.0, 0.96, 0.9) * keyHighlight + _GemFireColor.rgb * (edgeHighlight * 0.86 + crownHighlight * 0.54) + opalLayer * edgeRim * _OpalIridescence * 0.32) * _FacetFire;
            fixed3 spectralEdge = fixed3(0.4, 0.76, 1.0) * edgeRim * _DispersionStrength * (0.16 + _PhysicalDispersion * 3.4);
            spectralEdge += _GemFireColor.rgb * pow(saturate(fresnel + facetBreak * 0.18), 2.6) * _DispersionStrength * (0.1 + _PhysicalDispersion * 2.2);

            if (debugMode > 0.5 && debugMode < 1.5)
            {
                o.Albedo = sourceColor;
                o.Specular = 0;
                o.Smoothness = 0;
                o.Emission = sourceColor * 0.08;
                o.Alpha = 1;
                return;
            }

            if (debugMode > 1.5 && debugMode < 2.5)
            {
                o.Albedo = refractedColor;
                o.Specular = 0.04;
                o.Smoothness = 0.55;
                o.Emission = refractedColor * 0.16;
                o.Alpha = 0.96;
                return;
            }

            if (debugMode > 2.5 && debugMode < 3.5)
            {
                o.Albedo = mixedReflectionColor;
                o.Specular = saturate(mixedReflectionColor * 0.8 + _Tint.rgb * 0.12);
                o.Smoothness = 1.0;
                o.Emission = mixedReflectionColor * 0.14;
                o.Alpha = 0.98;
                return;
            }

            if (debugMode > 3.5 && debugMode < 4.5)
            {
                fixed3 dispersionDebug = saturate(abs(refractedColor - sourceColor) * (2.2 + _SpectralSplitScale) + spectralEdge * 1.25 + facetFire * 0.18);
                o.Albedo = dispersionDebug;
                o.Specular = dispersionDebug * 0.35;
                o.Smoothness = 0.82;
                o.Emission = dispersionDebug * 0.45;
                o.Alpha = 0.98;
                return;
            }

            if (debugMode > 4.5 && debugMode < 5.5)
            {
                fixed3 normalDebug = normal * 0.5 + 0.5;
                o.Albedo = normalDebug;
                o.Specular = 0;
                o.Smoothness = 0.2;
                o.Emission = normalDebug * 0.08;
                o.Alpha = 1;
                return;
            }

            if (debugMode > 6.5 && debugMode < 7.5)
            {
                float uvStress = saturate(length(facetOffset + depthOffset) * 8.0);
                fixed3 stressDebug = saturate(fixed3(facetBreak, deepCore, uvStress) + abs(refractedColor - mixedReflectionColor) * 0.85 + spectralEdge * 0.7);
                o.Albedo = stressDebug;
                o.Specular = stressDebug * 0.4;
                o.Smoothness = 0.88;
                o.Emission = stressDebug * 0.32;
                o.Alpha = 0.98;
                return;
            }

            o.Albedo = max(glassBase * (0.64 + intensity01 * 0.36), _Tint.rgb * _BrightnessFloor);
            fixed3 specularOut = saturate(specularColor + fresnel * reflection * 0.3 + facetFire * 0.24 + spectralEdge * 0.12 + _Metallic * 0.08);
            o.Specular = lerp(specularOut, saturate(mixedReflectionColor * 0.55 + _Tint.rgb * 0.34 + facetFire * 0.24 + spectralEdge * 0.16), mirrorMode);
            o.Smoothness = saturate(lerp(_Smoothness, 1.0, mirrorMode));
            fixed3 emissionRaw = internalColor * (0.025 + internalGain * 0.14) * (0.62 + intensity01 * 0.32)
                + fresnelReflection * 0.82
                + facetFire * (0.16 + fresnel * 0.24)
                + spectralEdge * (0.52 + facetBreak * 0.16)
                + _Tint.rgb * (_BrightnessFloor * (0.24 + edgeRim * 0.62));
            o.Emission = SoftCompressHighlights(emissionRaw, _HighlightCompression);
            o.Alpha = lerp(saturate(max(_MinimumTransmission * 0.2, _Alpha * (1.0 - _Transparency * 0.72)) + fresnel * (0.2 + reflection * 0.24) + facetBreak * 0.045 + thickness * 0.038), 0.98, mirrorMode);
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
