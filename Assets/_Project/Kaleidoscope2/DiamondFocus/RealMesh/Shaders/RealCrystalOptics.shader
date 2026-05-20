Shader "Kaleidoscope2/RealCrystalOptics"
{
    Properties
    {
        _KaleidoscopeTex ("Kaleidoscope Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (0.9, 0.98, 1, 1)
        _GemCoreColor ("Gem Core Color", Color) = (0.74, 0.9, 1, 1)
        _GemFireColor ("Gem Fire Color", Color) = (1, 0.86, 0.34, 1)
        _Intensity ("Intensity", Range(0,20)) = 8
        _Alpha ("Alpha", Range(0,1)) = 0.58
        _Metallic ("Metallic", Range(0,1)) = 0.02
        _Smoothness ("Smoothness", Range(0,1)) = 0.96
        _Transparency ("Transparency", Range(0,1)) = 0
        _RefractionStrength ("Refraction Strength", Range(0,0.12)) = 0.085
        _ScreenRefractionStrength ("Screen Refraction Strength", Range(0,0.08)) = 0.028
        _FresnelPower ("Fresnel Power", Range(0.5,8)) = 3.2
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.6
        _InternalBrightness ("Internal Brightness", Range(0,3)) = 1
        _MinimumTransmission ("Minimum Transmission", Range(0,1)) = 0.1
        _SpecularStrength ("Specular Strength", Range(0,1)) = 0.75
        _RimStrength ("Rim Response", Range(0,1.5)) = 0.75
        _BrightnessFloor ("Brightness Floor", Range(0,0.35)) = 0.08
        _GemTintStrength ("Gem Tint Strength", Range(0,1)) = 0.16
        _OpticalDensity ("Optical Density", Range(0,2)) = 0.72
        _FacetRefraction ("Facet Refraction", Range(0,2)) = 1.12
        _ThicknessRefraction ("Thickness Refraction", Range(0,2)) = 1.05
        _InternalReflectionStrength ("Internal Reflection", Range(0,2)) = 1.18
        _DispersionStrength ("Spectral Dispersion", Range(0,2)) = 1.18
        _FacetFire ("Facet Fire", Range(0,2)) = 1.08
        _DepthAbsorption ("Depth Absorption", Range(0,1)) = 0.42
        _Clarity ("Clarity", Range(0,1)) = 0.9
        _FacetContrast ("Facet Contrast", Range(0,2.2)) = 1.5
        _RefractiveIndex ("Refractive Index", Range(1,2.9)) = 2.417
        _PhysicalDispersion ("Physical Dispersion", Range(0,0.08)) = 0.044
        _AbsorptionStrength ("Absorption Strength", Range(0,2)) = 0.38
        _FresnelStrength ("Fresnel Strength", Range(0,2)) = 1.35
        _BackgroundDistortionStrength ("Background Distortion", Range(0,2)) = 1.15
        _SaturationBoost ("Saturation Boost", Range(0,2)) = 1.08
        _ContrastBoost ("Contrast Boost", Range(0,2)) = 1.08
        _OpalIridescence ("Opal Iridescence", Range(0,1)) = 0
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

        struct Input
        {
            float2 uv_KaleidoscopeTex;
            float3 viewDir;
            float3 worldNormal;
            float3 worldPos;
            float4 screenPos;
        };

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

            float2 surfaceUv = saturate(IN.uv_KaleidoscopeTex);
            float2 screenUv = saturate(IN.screenPos.xy / max(0.0001, IN.screenPos.w));
            float2 baseUv = lerp(surfaceUv, screenUv, 0.88);
            float facetPlaneA = abs(dot(normal, normalize(float3(0.64, 0.3, -0.7))));
            float facetPlaneB = abs(dot(normal, normalize(float3(-0.44, 0.78, -0.45))));
            float facetPlaneC = abs(dot(normal, normalize(float3(0.18, -0.92, -0.35))));
            float facetMask = pow(saturate(abs(normal.x) * 0.4 + abs(normal.z) * 0.38 + abs(normal.y) * 0.2 + facetPlaneA * 0.16), max(0.65, _FacetContrast));
            float facetBreak = saturate(facetMask * 0.62 + facetPlaneA * 0.18 + facetPlaneB * 0.13 + facetPlaneC * 0.1);
            float thickness = saturate((1.0 - ndv) * 0.5 + facetBreak * 0.36 + _OpticalDensity * 0.22);
            float3 refractedVector = refract(-viewDir, normal, 1.0 / safeIor);
            float totalInternalFeel = saturate(schlick * 1.72 + facetBreak * 0.18 + thickness * 0.15);
            float2 facetAxis = normalize(normal.xy + float2(normal.z, -normal.x) * 0.42 + refractedVector.xy * 0.36 + float2(0.001, -0.001));
            float distortionScale = _BackgroundDistortionStrength * (0.78 + (safeIor - 1.0) * 0.42);
            float2 facetOffset = (normal.xy * 1.04 - viewDir.xy * 0.24 + facetAxis * (0.22 + facetBreak * 0.16))
                * _ScreenRefractionStrength
                * _FacetRefraction
                * distortionScale
                * (0.64 + thickness * 0.92 + fresnel * 0.62);
            float2 depthOffset = (normal.xy * 0.34 + refractedVector.xy * 0.56 + facetAxis * 0.38)
                * _RefractionStrength
                * _ThicknessRefraction
                * distortionScale
                * (0.22 + thickness * 0.78 + totalInternalFeel * 0.18);
            float2 dispersionOffset = facetAxis
                * _ScreenRefractionStrength
                * _DispersionStrength
                * (0.62 + _PhysicalDispersion * 18.0)
                * (0.14 + fresnel * 0.58 + facetBreak * 0.34);

            float2 refractedUv = saturate(screenUv + facetOffset + depthOffset);
            float2 frontLayerUv = saturate(screenUv + facetOffset * 0.42 - depthOffset * 0.24);
            float2 backLayerUv = saturate(screenUv - facetOffset * 0.72 + depthOffset * 1.52);
            float2 echoLayerUv = saturate(screenUv - facetOffset * 1.32 - depthOffset * 0.52 + facetAxis * (fresnel * 0.035 + totalInternalFeel * 0.02));
            float2 reflectionUv = saturate(screenUv - facetOffset * 0.92 - depthOffset * 0.4 + facetAxis * fresnel * 0.03);

            fixed3 sourceColor = tex2D(_KaleidoscopeTex, baseUv).rgb;
            fixed3 frontLayerColor = tex2D(_KaleidoscopeTex, frontLayerUv).rgb;
            fixed3 refractedColor;
            refractedColor.r = tex2D(_KaleidoscopeTex, saturate(refractedUv + dispersionOffset)).r;
            refractedColor.g = tex2D(_KaleidoscopeTex, refractedUv).g;
            refractedColor.b = tex2D(_KaleidoscopeTex, saturate(refractedUv - dispersionOffset)).b;
            fixed3 backLayerColor = tex2D(_KaleidoscopeTex, backLayerUv).rgb;
            fixed3 echoLayerColor;
            echoLayerColor.r = tex2D(_KaleidoscopeTex, saturate(echoLayerUv + dispersionOffset * 1.55)).r;
            echoLayerColor.g = tex2D(_KaleidoscopeTex, saturate(echoLayerUv - dispersionOffset * 0.22)).g;
            echoLayerColor.b = tex2D(_KaleidoscopeTex, saturate(echoLayerUv - dispersionOffset * 1.45)).b;
            fixed3 internalReflectionColor = tex2D(_KaleidoscopeTex, reflectionUv).rgb;

            float gemAbsorption = saturate(_AbsorptionStrength * (0.16 + thickness * 0.52 + _DepthAbsorption * 0.32));
            fixed3 absorptionTint = lerp(_Tint.rgb, _GemCoreColor.rgb, 0.7);
            fixed3 absorptionFilter = lerp(fixed3(1.0, 1.0, 1.0), absorptionTint, gemAbsorption);
            fixed3 absorbedFront = lerp(frontLayerColor, frontLayerColor * absorptionFilter, saturate(_GemTintStrength + thickness * 0.16));
            fixed3 absorbedBack = backLayerColor * lerp(absorptionFilter, _GemCoreColor.rgb, saturate(_DepthAbsorption * 0.42 + thickness * 0.18));
            fixed3 absorbedRefraction = refractedColor * lerp(fixed3(1.0, 1.0, 1.0), absorptionFilter, saturate(0.42 + thickness * 0.38));
            fixed3 internalColor = lerp(sourceColor, absorbedRefraction, saturate(0.62 + _RefractionStrength * 2.4 + thickness * 0.18));
            internalColor = lerp(internalColor, absorbedFront, saturate(0.16 + facetBreak * 0.16));
            internalColor = lerp(internalColor, absorbedBack, saturate(0.2 + _OpticalDensity * 0.18 + thickness * 0.34));
            internalColor = lerp(internalColor, echoLayerColor, saturate(_InternalReflectionStrength * (0.06 + totalInternalFeel * 0.18 + facetBreak * 0.1)));
            internalColor = lerp(internalColor, internalReflectionColor, saturate(_InternalReflectionStrength * (0.1 + fresnel * 0.26 + facetBreak * 0.14)));

            float luma = dot(internalColor, fixed3(0.299, 0.587, 0.114));
            internalColor = lerp(fixed3(luma, luma, luma), internalColor, max(0.0, _SaturationBoost));
            internalColor = saturate((internalColor - 0.5) * max(0.05, _ContrastBoost) + 0.5);
            float opalSeed = frac(dot(abs(IN.worldPos), float3(8.71, 13.17, 5.31)) + facetBreak * 2.37 + ndv * 0.41);
            fixed3 opalA = fixed3(0.52, 0.95, 1.0);
            fixed3 opalB = fixed3(1.0, 0.52, 0.88);
            fixed3 opalC = fixed3(1.0, 0.92, 0.36);
            fixed3 opalLayer = lerp(lerp(opalA, opalB, opalSeed), opalC, saturate(abs(opalSeed - 0.5) * 1.65));
            internalColor = lerp(internalColor, saturate(internalColor * 0.74 + opalLayer * 0.32 + sourceColor * 0.2), _OpalIridescence * saturate(0.34 + fresnel * 0.38 + facetBreak * 0.22));

            float transmission = saturate(max(_MinimumTransmission, 1.0 - _Transparency * 0.56) * lerp(0.68, 0.96, _Clarity) * (0.66 + ndv * 0.34));
            float internalGain = saturate(_InternalBrightness * (0.24 + thickness * 0.2 + fresnel * 0.18 + totalInternalFeel * 0.1));
            float reflection = saturate(_ReflectionStrength);
            fixed3 tintedInternal = lerp(internalColor, internalColor * _Tint.rgb, saturate(_GemTintStrength + thickness * 0.18));
            fixed3 glassBase = lerp(_GemCoreColor.rgb * (0.045 + _BrightnessFloor * 0.2), tintedInternal, transmission);
            fixed3 fresnelReflection = lerp(_Tint.rgb, _GemFireColor.rgb, saturate(facetBreak * 0.35 + _DispersionStrength * 0.2)) * fresnel * (0.22 + reflection * 0.58);
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

            o.Albedo = max(glassBase * (0.64 + intensity01 * 0.36), _Tint.rgb * _BrightnessFloor);
            o.Specular = saturate(specularColor + fresnel * reflection * 0.3 + facetFire * 0.24 + spectralEdge * 0.12 + _Metallic * 0.08);
            o.Smoothness = saturate(_Smoothness);
            o.Emission = internalColor * (0.04 + internalGain * 0.2) * (0.72 + intensity01 * 0.46)
                + fresnelReflection
                + facetFire * (0.26 + fresnel * 0.36)
                + spectralEdge * (0.84 + facetBreak * 0.22)
                + _Tint.rgb * (_BrightnessFloor * (0.42 + edgeRim * 0.98));
            o.Alpha = saturate(max(_MinimumTransmission * 0.52, _Alpha * (1.0 - _Transparency * 0.28)) + fresnel * (0.2 + reflection * 0.24) + facetBreak * 0.045 + thickness * 0.038);
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
