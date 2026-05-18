Shader "Kaleidoscope2/RealCrystalOptics"
{
    Properties
    {
        _KaleidoscopeTex ("Kaleidoscope Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (0.9, 0.98, 1, 1)
        _Intensity ("Intensity", Range(0,20)) = 8
        _Alpha ("Alpha", Range(0,1)) = 0.58
        _Metallic ("Metallic", Range(0,1)) = 0.02
        _Smoothness ("Smoothness", Range(0,1)) = 0.96
        _Transparency ("Transparency", Range(0,1)) = 0
        _RefractionStrength ("Refraction Strength", Range(0,0.12)) = 0.085
        _FresnelPower ("Fresnel Power", Range(0.5,8)) = 3.2
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.6
        _InternalBrightness ("Internal Brightness", Range(0,3)) = 1
        _MinimumTransmission ("Minimum Transmission", Range(0,1)) = 0.1
        _SpecularStrength ("Specular Strength", Range(0,1)) = 0.75
        _DispersionStrength ("Chromatic Dispersion", Range(0,0.14)) = 0.075
        _RimStrength ("Rim Response", Range(0,1.5)) = 0.75
        _BrightnessFloor ("Brightness Floor", Range(0,0.35)) = 0.08
        _GlintStrength ("Glint Strength", Range(0,1.5)) = 0.65
        _FacetContrast ("Facet Contrast", Range(0,1.5)) = 0.65
        _InternalScatter ("Internal Scatter", Range(0,1.5)) = 0.45
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
        half _Intensity;
        half _Alpha;
        half _Metallic;
        half _Smoothness;
        half _Transparency;
        half _RefractionStrength;
        half _FresnelPower;
        half _ReflectionStrength;
        half _InternalBrightness;
        half _MinimumTransmission;
        half _SpecularStrength;
        half _DispersionStrength;
        half _RimStrength;
        half _BrightnessFloor;
        half _GlintStrength;
        half _FacetContrast;
        half _InternalScatter;

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
            float fresnel = pow(1.0 - ndv, max(0.5, _FresnelPower));
            float intensity01 = saturate(_Intensity / 20.0);

            float2 surfaceUv = saturate(IN.uv_KaleidoscopeTex);
            float2 screenUv = saturate(IN.screenPos.xy / max(0.0001, IN.screenPos.w));
            float2 baseUv = lerp(surfaceUv, screenUv, 0.82);
            float refraction01 = saturate(_RefractionStrength / 0.12);
            float facetRidge = saturate(abs(normal.x) * 0.42 + abs(normal.z) * 0.44 + abs(normal.y) * 0.14);
            facetRidge = pow(facetRidge, 1.35) * _FacetContrast;
            float2 normalOffset = normal.xy * _RefractionStrength * (1.18 + fresnel * 2.2 + facetRidge * 0.85);
            float2 viewOffset = viewDir.xy * _RefractionStrength * (0.48 + facetRidge * 0.26);
            float2 refractedUv = saturate(baseUv + normalOffset - viewOffset);
            float2 wideRefractedUv = saturate(baseUv + normalOffset * 1.72 - viewOffset * 0.72);
            float2 internalUv = saturate(lerp(surfaceUv, baseUv - normalOffset * 0.72 + viewOffset * 0.28, 0.74));
            float2 dispersionAxis = normal.xy + viewDir.xy * 0.35 + float2(0.001, -0.001);
            dispersionAxis = normalize(dispersionAxis);
            float2 dispersionOffset = dispersionAxis * _DispersionStrength * (0.52 + fresnel * 1.85 + facetRidge * 0.5);

            fixed3 raw = tex2D(_KaleidoscopeTex, baseUv).rgb;
            fixed3 refracted = tex2D(_KaleidoscopeTex, refractedUv).rgb;
            fixed3 wideRefracted = tex2D(_KaleidoscopeTex, wideRefractedUv).rgb;
            fixed3 internalLayer = tex2D(_KaleidoscopeTex, internalUv).rgb;
            fixed3 dispersed;
            dispersed.r = tex2D(_KaleidoscopeTex, saturate(refractedUv + dispersionOffset)).r;
            dispersed.g = tex2D(_KaleidoscopeTex, refractedUv).g;
            dispersed.b = tex2D(_KaleidoscopeTex, saturate(refractedUv - dispersionOffset)).b;
            fixed3 wideSpectrum;
            wideSpectrum.r = tex2D(_KaleidoscopeTex, saturate(wideRefractedUv + dispersionOffset * 1.55)).r;
            wideSpectrum.g = tex2D(_KaleidoscopeTex, saturate(wideRefractedUv - dispersionOffset * 0.18)).g;
            wideSpectrum.b = tex2D(_KaleidoscopeTex, saturate(wideRefractedUv - dispersionOffset * 1.45)).b;
            fixed3 internalColor = lerp(raw, refracted, saturate(0.48 + refraction01 * 0.42));
            internalColor = lerp(internalColor, wideRefracted, saturate(0.12 + refraction01 * 0.28 + facetRidge * 0.12));
            internalColor = lerp(internalColor, internalLayer, saturate(0.22 + refraction01 * 0.38 + fresnel * 0.16 + _InternalScatter * 0.08));
            internalColor = lerp(internalColor, dispersed, saturate(_DispersionStrength * 7.0 + fresnel * 0.18 + facetRidge * 0.1));
            internalColor = lerp(internalColor, wideSpectrum, saturate(_DispersionStrength * 3.4 + facetRidge * 0.16));

            float transmission = saturate(max(_MinimumTransmission, 1.0 - _Transparency * 0.65) * (0.72 + ndv * 0.28));
            float internalGain = saturate(_InternalBrightness * (0.32 + refraction01 * 0.42 + fresnel * 0.22));
            float reflection = saturate(_ReflectionStrength);
            fixed3 tintedInternal = lerp(internalColor, internalColor * _Tint.rgb, 0.34);
            fixed3 glassBase = lerp(_Tint.rgb * 0.055, tintedInternal, transmission);
            fixed3 fresnelReflection = _Tint.rgb * fresnel * (0.18 + reflection * 0.58);
            fixed3 specularColor = lerp(fixed3(0.04, 0.045, 0.05), _Tint.rgb * (0.35 + reflection * 0.45), saturate(_SpecularStrength));
            float3 keyDir = normalize(float3(0.36, 0.78, -0.5));
            float3 rimDir = normalize(float3(-0.58, 0.26, -0.76));
            float keyGlint = pow(saturate(dot(reflect(-keyDir, normal), viewDir)), 92.0);
            float rimGlint = pow(saturate(dot(reflect(-rimDir, normal), viewDir)), 68.0);
            float edgeRim = pow(saturate(fresnel), 2.25) * _RimStrength;
            float needleGlint = pow(saturate(dot(reflect(normalize(float3(-0.72, 0.34, -0.6)), normal), viewDir)), 154.0);
            fixed3 spectralGlint = fixed3(keyGlint * 1.0 + rimGlint * 0.25 + needleGlint * 0.75, keyGlint * 0.72 + rimGlint * 0.78 + needleGlint * 0.45, keyGlint * 0.42 + rimGlint * 1.0 + needleGlint * 1.15) * _GlintStrength;
            fixed3 spectralEdge = fixed3(1.0, 0.36 + facetRidge * 0.28, 0.18) * edgeRim * _DispersionStrength * 2.2;
            spectralEdge += fixed3(0.18, 0.72, 1.0) * pow(saturate(fresnel + facetRidge * 0.22), 3.1) * _DispersionStrength * 1.35;

            o.Albedo = max(glassBase * (0.64 + intensity01 * 0.36), _Tint.rgb * _BrightnessFloor);
            o.Specular = saturate(specularColor + fresnel * reflection * 0.24 + spectralGlint * 0.34 + spectralEdge * 0.16 + _Metallic * 0.08);
            o.Smoothness = saturate(_Smoothness);
            o.Emission = internalColor * (0.035 + internalGain * 0.16) * (0.75 + intensity01 * 0.45)
                + fresnelReflection
                + spectralGlint * (0.34 + fresnel * 0.5)
                + spectralEdge * (0.42 + facetRidge * 0.3)
                + internalColor * _InternalScatter * (0.08 + fresnel * 0.07)
                + _Tint.rgb * (_BrightnessFloor * (0.42 + edgeRim * 0.98));
            o.Alpha = saturate(max(_MinimumTransmission * 0.52, _Alpha * (1.0 - _Transparency * 0.32)) + fresnel * (0.24 + reflection * 0.26) + facetRidge * 0.04);
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
