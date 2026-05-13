Shader "Kaleidoscope2/DiamondCrystal3D"
{
    Properties
    {
        _SceneTex ("Scene Texture", 2D) = "white" {}
        _Transparency ("Transparency", Range(0,1)) = 0.48
        _RefractionStrength ("Refraction Strength", Range(0,0.12)) = 0.042
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.42
        _FresnelPower ("Fresnel Power", Range(0.5,8)) = 3.2
        _EdgeHighlight ("Edge Highlight", Range(0,2)) = 0.9
        _ChromaticAberrationAmount ("Chromatic Aberration Amount", Float) = 0.004
        _FacetContrast ("Facet Contrast", Range(0,2)) = 1.05
        _InternalGlow ("Internal Glow", Range(0,1.5)) = 0.32
        _BloomBoost ("Bloom Boost", Range(0,3)) = 0.24
        _FocusAmount ("Focus Amount", Range(0,1)) = 0
        _Shape ("Shape", Float) = 0
        _CrystalMaterialMode ("Crystal Material Mode", Float) = 1
        _GeneratedColor ("Generated Color", Color) = (0.85, 0.96, 1, 1)
        _GeneratedMaterialKind ("Generated Material Kind", Float) = 0
        _OpticalIOR ("Optical IOR", Range(1,3)) = 2.42
        _OpticalCaustics ("Optical Caustics", Range(0,2)) = 0.8
        _OpticalDispersion ("Optical Dispersion", Range(0,2)) = 0.9
        _TotalInternalReflection ("Total Internal Reflection", Range(0,2)) = 0.9
        _Rotation ("Rotation", Vector) = (0, 0, 0, 0)
        _DiamondScale ("Diamond Scale", Range(0.1,1)) = 0.38
        _InputTexelSize ("Input Texel Size", Vector) = (0.001, 0.001, 1024, 1024)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _SceneTex;
            float _Transparency;
            float _RefractionStrength;
            float _ReflectionStrength;
            float _FresnelPower;
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
            float _OpticalIOR;
            float _OpticalCaustics;
            float _OpticalDispersion;
            float _TotalInternalReflection;
            float4 _Rotation;
            float _DiamondScale;
            float4 _InputTexelSize;

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

            fixed3 SampleRefracted(float2 uv, float2 offset, float chroma)
            {
                float2 redUv = saturate(uv + offset + offset * chroma);
                float2 greenUv = saturate(uv + offset * 0.72);
                float2 blueUv = saturate(uv + offset - offset * chroma);
                return fixed3(tex2D(_SceneTex, redUv).r, tex2D(_SceneTex, greenUv).g, tex2D(_SceneTex, blueUv).b);
            }

            float SmoothPattern(float3 value, float scale)
            {
                float wave = sin(dot(value, float3(12.9898, 78.233, 37.719)) * scale);
                return 0.5 + 0.5 * wave;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenUv = i.screenPos.xy / max(0.0001, i.screenPos.w);
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);

                float ndv = abs(dot(normal, viewDir));
                float fresnel = pow(saturate(1.0 - ndv), _FresnelPower);
                float edge = saturate(fresnel * 1.45);
                float focus = saturate(_FocusAmount);
                float angle = atan2(normal.z, normal.x) + _Rotation.z * 0.0174532925;
                float facetCount = _Shape < 0.5 ? 8.0 : (_Shape < 1.5 ? 12.0 : (_Shape < 2.5 ? 12.0 : (_Shape < 3.5 ? 4.0 : 96.0)));
                float facet = pow(saturate(0.5 + 0.5 * cos(angle * facetCount)), 1.6);
                float3 lightA = normalize(float3(-0.35, 0.72, -0.58));
                float3 lightB = normalize(float3(0.5, 0.25, -0.82));
                float highlightA = pow(saturate(dot(reflect(-lightA, normal), viewDir)), 26.0);
                float highlightB = pow(saturate(dot(reflect(-lightB, normal), viewDir)), 58.0);

                float2 bend = normal.xy * _RefractionStrength * (0.95 + edge * 1.7 + focus * 0.55);
                bend += (i.uv - 0.5) * _RefractionStrength * 0.25;
                float chroma = _ChromaticAberrationAmount * (28.0 + edge * 38.0 + focus * 18.0);
                fixed3 refracted = SampleRefracted(screenUv, bend, chroma);

                float3 reflectedVector = reflect(-viewDir, normal);
                float2 reflectedUv = frac(0.5 + Rotate2(reflectedVector.xy * (0.34 + edge * 0.22), _Rotation.y * 0.006));
                fixed3 reflected = tex2D(_SceneTex, reflectedUv).rgb;

                fixed3 cool = fixed3(0.64, 0.92, 1.0);
                fixed3 warm = fixed3(1.0, 0.78, 0.42);
                fixed3 fire = fixed3(
                    tex2D(_SceneTex, saturate(screenUv + bend * 1.9 + float2(chroma, 0))).r,
                    tex2D(_SceneTex, saturate(screenUv - bend * 1.4 + float2(0, chroma))).g,
                    tex2D(_SceneTex, saturate(screenUv + bend * 0.6 - float2(chroma, chroma))).b);

                fixed3 glass = lerp(refracted, reflected, saturate(_ReflectionStrength * (0.22 + edge * 0.78)));
                glass = lerp(glass, fire, saturate(facet * 0.22 + focus * 0.08));
                glass *= 0.82 + facet * _FacetContrast * 0.34;
                glass += cool * edge * _EdgeHighlight;
                glass += warm * (highlightA + highlightB) * (0.45 + _BloomBoost * 0.18);
                glass += cool * _InternalGlow * saturate(0.4 + normal.y * 0.6) * 0.32;
                glass += facet * focus * _BloomBoost * 0.08;

                float alpha = saturate((1.0 - _Transparency) * 0.42 + edge * 0.36 + facet * 0.12 + (highlightA + highlightB) * 0.24);
                float mode = floor(_CrystalMaterialMode + 0.5);

                if (mode < 0.5)
                {
                    fixed3 mirror = reflected * (0.96 + edge * 0.18);
                    mirror += fixed3(1.0, 0.96, 0.88) * (highlightA * 0.9 + highlightB * 1.25);
                    mirror += edge * fixed3(0.82, 0.95, 1.0) * (0.45 + _EdgeHighlight * 0.35);
                    return fixed4(mirror, saturate(0.88 + edge * 0.12));
                }

                if (mode > 1.5 && mode < 2.5)
                {
                    fixed3 glow = saturate(_GeneratedColor.rgb);
                    fixed3 glowCore = lerp(refracted, glow, 0.42);
                    glowCore += glow * (0.35 + edge * 1.4 + facet * 0.45 + _InternalGlow * 0.7 + focus * 0.25);
                    glowCore += fire * 0.12;
                    return fixed4(glowCore, saturate(alpha + edge * 0.25 + 0.08));
                }

                if (mode > 2.5 && mode < 3.5)
                {
                    float ior = max(1.01, _OpticalIOR);
                    float opticalChroma = _ChromaticAberrationAmount * (36.0 + _OpticalDispersion * 70.0 + edge * 32.0);
                    fixed3 spectral = SampleRefracted(screenUv, bend * (0.85 + (ior - 1.0) * 0.85), opticalChroma);
                    float tir = smoothstep(1.0 / ior, 1.0, 1.0 - ndv) * _TotalInternalReflection;
                    float causticWave = SmoothPattern(i.worldPos + normal * 0.17, 2.2 + facetCount * 0.015);
                    float caustic = pow(saturate(causticWave * facet + highlightA + highlightB), 4.5) * _OpticalCaustics;
                    fixed3 optical = lerp(spectral, reflected, saturate(tir * 0.75 + edge * 0.18));
                    optical += fire * _OpticalDispersion * 0.28;
                    optical += caustic * fixed3(0.75, 0.95, 1.0);
                    optical += edge * fixed3(0.68, 0.9, 1.0) * _TotalInternalReflection;
                    return fixed4(optical, saturate(alpha * 0.72 + tir * 0.28 + caustic * 0.22));
                }

                if (mode > 3.5)
                {
                    float kind = floor(_GeneratedMaterialKind + 0.5);
                    fixed3 tint = saturate(_GeneratedColor.rgb);

                    if (kind < 0.5)
                    {
                        float grain = 0.5 + 0.5 * sin((i.worldPos.x + i.worldPos.z) * 28.0 + sin(i.worldPos.y * 19.0) * 3.0);
                        fixed3 wood = tint * (0.58 + grain * 0.62);
                        wood += fixed3(0.08, 0.04, 0.015) * pow(grain, 5.0);
                        return fixed4(wood + reflected * 0.08 + (highlightA + highlightB) * 0.12, 0.92);
                    }

                    if (kind < 1.5)
                    {
                        fixed3 metal = lerp(reflected, tint, 0.28);
                        metal += fixed3(1.0, 0.92, 0.78) * (highlightA * 0.8 + highlightB * 1.4 + edge * 0.18);
                        return fixed4(metal, 0.94);
                    }

                    if (kind < 2.5)
                    {
                        fixed3 plastic = tint * (0.62 + facet * 0.18);
                        plastic = lerp(plastic, reflected, 0.12);
                        plastic += fixed3(1.0, 1.0, 1.0) * (highlightA * 0.35 + highlightB * 0.75);
                        return fixed4(plastic, 0.86);
                    }

                    float veins = pow(abs(sin(i.worldPos.x * 17.0 + i.worldPos.y * 11.0 + i.worldPos.z * 9.0)), 8.0);
                    fixed3 stone = tint * (0.55 + facet * 0.18);
                    stone += veins * fixed3(0.18, 0.17, 0.15);
                    stone += reflected * 0.05 + (highlightA + highlightB) * 0.08;
                    return fixed4(stone, 0.94);
                }

                return fixed4(glass, alpha);
            }
            ENDCG
        }
    }
}
