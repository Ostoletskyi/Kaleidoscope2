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
        _RefractionStrength ("Refraction Strength", Range(0,0.12)) = 0.04
        _FresnelPower ("Fresnel Power", Range(0.5,8)) = 3.2
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.6
        _InternalBrightness ("Internal Brightness", Range(0,3)) = 1
        _MinimumTransmission ("Minimum Transmission", Range(0,1)) = 0.1
        _SpecularStrength ("Specular Strength", Range(0,1)) = 0.75
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

        struct Input
        {
            float2 uv_KaleidoscopeTex;
            float3 viewDir;
            float3 worldNormal;
            float3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutputStandardSpecular o)
        {
            float3 viewDir = normalize(IN.viewDir);
            float3 normal = normalize(IN.worldNormal);
            float ndv = saturate(dot(viewDir, normal));
            float fresnel = pow(1.0 - ndv, max(0.5, _FresnelPower));
            float intensity01 = saturate(_Intensity / 20.0);

            float2 baseUv = saturate(IN.uv_KaleidoscopeTex);
            float refraction01 = saturate(_RefractionStrength / 0.12);
            float2 normalOffset = normal.xy * _RefractionStrength * (0.65 + fresnel * 1.35);
            float2 viewOffset = viewDir.xy * _RefractionStrength * 0.35;
            float2 refractedUv = saturate(baseUv + normalOffset - viewOffset);
            float2 internalUv = saturate(baseUv - normalOffset * 0.55 + viewOffset * 0.2);

            fixed3 raw = tex2D(_KaleidoscopeTex, baseUv).rgb;
            fixed3 refracted = tex2D(_KaleidoscopeTex, refractedUv).rgb;
            fixed3 internalLayer = tex2D(_KaleidoscopeTex, internalUv).rgb;
            fixed3 internalColor = lerp(raw, refracted, saturate(0.48 + refraction01 * 0.42));
            internalColor = lerp(internalColor, internalLayer, saturate(0.18 + refraction01 * 0.35 + fresnel * 0.12));

            float transmission = saturate(max(_MinimumTransmission, 1.0 - _Transparency * 0.65) * (0.72 + ndv * 0.28));
            float internalGain = saturate(_InternalBrightness * (0.32 + refraction01 * 0.42 + fresnel * 0.22));
            float reflection = saturate(_ReflectionStrength);
            fixed3 tintedInternal = lerp(internalColor, internalColor * _Tint.rgb, 0.34);
            fixed3 glassBase = lerp(_Tint.rgb * 0.055, tintedInternal, transmission);
            fixed3 fresnelReflection = _Tint.rgb * fresnel * (0.18 + reflection * 0.58);
            fixed3 specularColor = lerp(fixed3(0.04, 0.045, 0.05), _Tint.rgb * (0.35 + reflection * 0.45), saturate(_SpecularStrength));

            o.Albedo = glassBase * (0.64 + intensity01 * 0.36);
            o.Specular = saturate(specularColor + fresnel * reflection * 0.12 + _Metallic * 0.08);
            o.Smoothness = saturate(_Smoothness);
            o.Emission = internalColor * (0.035 + internalGain * 0.16) * (0.75 + intensity01 * 0.45) + fresnelReflection;
            o.Alpha = saturate(max(_MinimumTransmission * 0.45, _Alpha * (1.0 - _Transparency * 0.45)) + fresnel * (0.18 + reflection * 0.22));
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
