Shader "Kaleidoscope2/DiamondCrystal3D"
{
    Properties
    {
        _SceneTex ("Scene Texture", 2D) = "white" {}
        _Transparency ("Transparency", Range(0,1)) = 0
        _RefractionStrength ("Refraction Strength", Range(0,0.12)) = 0.074
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.72
        _FresnelPower ("Fresnel Power", Range(0.5,8)) = 3.2
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
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float ior = max(1.01, _OpticalIOR);
                float3 physicalRefract = refract(-viewDir, normal, 1.0 / ior);

                float ndv = abs(dot(normal, viewDir));
                float fresnel = pow(saturate(1.0 - ndv), _FresnelPower);
                float edge = saturate(fresnel * 1.45);
                float focus = saturate(_FocusAmount);
                float angle = atan2(normal.z, normal.x) + _Rotation.z * 0.0174532925;
                float facetCount = _Shape < 0.5 ? 8.0 : (_Shape < 1.5 ? 8.0 : (_Shape < 2.5 ? 16.0 : (_Shape < 3.5 ? 3.0 : (_Shape < 4.5 ? 4.0 : 12.0))));
                float facet = pow(saturate(0.5 + 0.5 * cos(angle * facetCount)), 1.6);
                float3 lightA = normalize(float3(-0.35, 0.72, -0.58));
                float3 lightB = normalize(float3(0.5, 0.25, -0.82));
                float highlightA = pow(saturate(dot(reflect(-lightA, normal), viewDir)), 26.0);
                float highlightB = pow(saturate(dot(reflect(-lightB, normal), viewDir)), 58.0);

                float2 bend = normal.xy * _RefractionStrength * (0.95 + edge * 1.7 + focus * 0.55 + _DiamondLikeRefraction * 0.75);
                bend += physicalRefract.xy * _RefractionStrength * _PhysicallyBasedRefraction * (0.75 + (ior - 1.0) * 0.65);
                bend += (i.uv - 0.5) * _RefractionStrength * 0.25;
                float chroma = _ChromaticAberrationAmount * (28.0 + edge * 38.0 + focus * 18.0) * (1.0 + _SpectralDispersion * 0.85);
                fixed3 refracted = SampleRefracted(screenUv, bend, chroma);
                fixed3 wideSpectrum = fixed3(
                    tex2D(_SceneTex, saturate(screenUv + bend * (1.35 + _DiamondLikeRefraction) + normal.xy * chroma * 2.2)).r,
                    tex2D(_SceneTex, saturate(screenUv + bend * 0.42 + physicalRefract.xy * chroma * 0.9)).g,
                    tex2D(_SceneTex, saturate(screenUv - bend * (0.85 + _DiamondLikeRefraction * 0.4) - normal.xy * chroma * 2.0)).b);
                refracted = lerp(refracted, wideSpectrum, saturate(_SpectralDispersion * 0.28 + _DiamondLikeRefraction * 0.1));

                float3 reflectedVector = reflect(-viewDir, normal);
                float2 reflectedUv = frac(0.5 + Rotate2(reflectedVector.xy * (0.34 + edge * 0.22), _Rotation.y * 0.006));
                fixed3 reflected = tex2D(_SceneTex, reflectedUv).rgb;
                float2 reflectedUv2 = frac(0.5 + Rotate2(reflectedVector.yx * (0.48 + facet * 0.18) + normal.xy * 0.1, -_Rotation.x * 0.004));
                float2 reflectedUv3 = frac(0.5 + Rotate2((reflectedVector.xy + physicalRefract.xy) * (0.24 + edge * 0.32), _Rotation.z * 0.005));
                fixed3 multiBounce = (reflected + tex2D(_SceneTex, reflectedUv2).rgb + tex2D(_SceneTex, reflectedUv3).rgb) / 3.0;
                reflected = lerp(reflected, multiBounce, saturate(_MultiBounceInternalReflections * 0.45));

                fixed3 cool = fixed3(0.64, 0.92, 1.0);
                fixed3 warm = fixed3(1.0, 0.78, 0.42);
                fixed3 fire = fixed3(
                    tex2D(_SceneTex, saturate(screenUv + bend * 1.9 + float2(chroma, 0))).r,
                    tex2D(_SceneTex, saturate(screenUv - bend * 1.4 + float2(0, chroma))).g,
                    tex2D(_SceneTex, saturate(screenUv + bend * 0.6 - float2(chroma, chroma))).b);
                float spectralPhase = facet + angle * 0.159 + dot(i.worldPos, float3(0.37, 0.61, 0.23)) + _Rotation.y * 0.003;
                fixed3 spectralFire = SpectralPalette(spectralPhase);
                float causticPatternA = pow(saturate(SmoothPattern(i.worldPos + normal * 0.31, 2.4 + facetCount * 0.02) * facet + highlightA + highlightB), 5.5);
                float causticPatternB = pow(saturate(1.0 - abs(frac((angle + i.worldPos.y * 2.0) * facetCount * 0.14 + _Rotation.x * 0.002) - 0.5) * 2.0), 7.0);
                float causticEnergy = (causticPatternA + causticPatternB * edge) * _HighEnergyCaustics * (0.32 + _CinematicCrystalOptics * 0.35);
                float volumeCore = saturate(1.0 - length(i.uv - 0.5) * 1.55);
                float volumeDepth = pow(volumeCore, 1.8) * saturate(0.42 + normal.y * 0.32 + ndv * 0.38);
                float volumetricScatter = volumeDepth * _DeepVolumetricLightScattering * (0.35 + _CinematicCrystalOptics * 0.35 + focus * 0.16);

                float criticalAngle = saturate(1.0 / ior);
                float totalInternalReflection = smoothstep(criticalAngle * 0.38, 1.0, 1.0 - ndv) * _TotalInternalReflection;
                float returnLight = saturate(totalInternalReflection * _TotalInternalReturn + edge * 0.72 + (highlightA + highlightB) * 0.55);
                float directTransmission = saturate(_DirectTransmission * (0.12 + ndv * 0.32) * (1.0 - totalInternalReflection * 0.88));
                float facetRidge = pow(saturate(abs(facet - 0.5) * 2.0), 0.58);
                float facetDepth = saturate(1.0 - facetRidge * 0.36 * _FacetDepthContrast - (1.0 - ndv) * 0.18 * _FacetDepthContrast);

                fixed3 deepCore = fixed3(0.012, 0.055, 0.13);
                fixed3 icyBody = fixed3(0.46, 0.86, 1.0);
                fixed3 opticalSample = lerp(refracted, reflected, saturate(0.7 + totalInternalReflection * 0.25));
                fixed3 glass = lerp(deepCore, icyBody, saturate(edge * 0.55 + facet * 0.2 + volumetricScatter * 0.3));
                glass = lerp(glass, opticalSample, directTransmission);
                glass = lerp(glass, reflected, saturate(_ReflectionStrength * 0.24 + returnLight * 0.52));
                glass *= 0.58 + facetDepth * 0.5 + facet * _FacetContrast * 0.18;

                float whiteReturn = pow(saturate(returnLight + highlightA * 0.85 + highlightB * 1.15), 2.05);
                float spectralBand = pow(saturate(causticPatternA * 0.55 + causticPatternB * 0.65 + facet * edge * 0.42), 1.65);
                float plasmaNeedle = pow(saturate(causticPatternA * 0.65 + causticPatternB * 0.75 + highlightA * 1.2 + highlightB * 1.6 + edge * facet * 0.35), 2.15);
                float plasmaEnergy = plasmaNeedle * _BlueWhitePlasmaEnergy * (0.45 + _HighEnergyCaustics * 0.42 + focus * 0.25);
                fixed3 coldPlasma = lerp(fixed3(0.12, 0.46, 1.0), fixed3(1.0, 1.0, 0.94), saturate(plasmaEnergy * 0.55 + whiteReturn * 0.25));
                fixed3 prismaticFire = saturate(spectralFire * 1.35 + fire * 0.42) * spectralBand * (_SpectralFireIntensity * (0.38 + _SpectralDispersion * 0.22));

                glass += cool * edge * _EdgeHighlight * (0.45 + totalInternalReflection * 0.55);
                glass += fixed3(1.0, 0.97, 0.88) * whiteReturn * (0.62 + _BloomBoost * 0.22);
                glass += prismaticFire;
                glass += coldPlasma * plasmaEnergy;
                glass += causticEnergy * fixed3(0.78, 0.92, 1.0) * (0.72 + _SpectralFireIntensity * 0.18);
                glass += volumetricScatter * lerp(fixed3(0.14, 0.38, 0.78), spectralFire, saturate(_SpectralDispersion * 0.18));
                glass += cool * _InternalGlow * saturate(0.4 + normal.y * 0.6) * 0.28;
                glass = lerp(glass, glass * facetDepth + deepCore * (1.0 - facetDepth) * 0.95, saturate(_CrystalSolidity * 0.55));
                glass = lerp(glass, pow(saturate(glass), 0.78), saturate(_CinematicCrystalOptics * 0.45));
                glass = saturate((glass - 0.035) * (1.25 + _CrystalSolidity * 0.35));

                float solidAlpha = 1.0;
                float mode = floor(_CrystalMaterialMode + 0.5);

                if (mode < 0.5)
                {
                    fixed3 mirror = reflected * (0.96 + edge * 0.18);
                    mirror += fixed3(1.0, 0.96, 0.88) * (highlightA * 0.9 + highlightB * 1.25);
                    mirror += edge * fixed3(0.82, 0.95, 1.0) * (0.45 + _EdgeHighlight * 0.35);
                    mirror += spectralFire * causticEnergy * 0.16;
                    return fixed4(CrystalContrastGrade(mirror, whiteReturn + edge, 1.0 - facetDepth), solidAlpha);
                }

                if (mode > 1.5 && mode < 2.5)
                {
                    fixed3 glow = saturate(_GeneratedColor.rgb);
                    fixed3 glowCore = lerp(refracted, glow, 0.42);
                    glowCore += glow * (0.35 + edge * 1.4 + facet * 0.45 + _InternalGlow * 0.7 + focus * 0.25);
                    glowCore += fire * 0.12;
                    glowCore += spectralFire * causticEnergy * 0.22 + glow * volumetricScatter * 0.55;
                    return fixed4(CrystalContrastGrade(glowCore, whiteReturn + edge + 0.25, 1.0 - facetDepth), solidAlpha);
                }

                if (mode > 2.5 && mode < 3.5)
                {
                    float opticalChroma = _ChromaticAberrationAmount * (36.0 + (_OpticalDispersion + _SpectralDispersion) * 58.0 + edge * 32.0);
                    fixed3 spectral = SampleRefracted(screenUv, bend * (0.85 + (ior - 1.0) * 0.85 + _DiamondLikeRefraction * 0.3), opticalChroma);
                    float tir = smoothstep(1.0 / ior, 1.0, 1.0 - ndv) * _TotalInternalReflection;
                    float causticWave = SmoothPattern(i.worldPos + normal * 0.17, 2.2 + facetCount * 0.015);
                    float caustic = pow(saturate(causticWave * facet + highlightA + highlightB), 4.5) * (_OpticalCaustics + _HighEnergyCaustics * 0.65);
                    fixed3 optical = lerp(spectral, reflected, saturate(tir * (0.75 + _MultiBounceInternalReflections * 0.08) + edge * 0.18));
                    optical += fire * _OpticalDispersion * 0.28;
                    optical += spectralFire * (_SpectralDispersion * 0.18 + caustic * 0.16);
                    optical += caustic * fixed3(0.75, 0.95, 1.0);
                    optical += edge * fixed3(0.68, 0.9, 1.0) * _TotalInternalReflection;
                    optical += volumetricScatter * fixed3(0.55, 0.85, 1.0);
                    return fixed4(CrystalContrastGrade(optical, whiteReturn + caustic + edge, 1.0 - facetDepth), solidAlpha);
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
                        fixed3 woodCrystal = wood + reflected * 0.08 + (highlightA + highlightB) * 0.12;
                        return fixed4(CrystalContrastGrade(woodCrystal, highlightA + highlightB + edge * 0.25, 0.72), solidAlpha);
                    }

                    if (kind < 1.5)
                    {
                        fixed3 metal = lerp(reflected, tint, 0.28);
                        metal += fixed3(1.0, 0.92, 0.78) * (highlightA * 0.8 + highlightB * 1.4 + edge * 0.18);
                        return fixed4(CrystalContrastGrade(metal, highlightA + highlightB + edge, 0.45), solidAlpha);
                    }

                    if (kind < 2.5)
                    {
                        fixed3 plastic = tint * (0.62 + facet * 0.18);
                        plastic = lerp(plastic, reflected, 0.12);
                        plastic += fixed3(1.0, 1.0, 1.0) * (highlightA * 0.35 + highlightB * 0.75);
                        return fixed4(CrystalContrastGrade(plastic, highlightA + highlightB + edge * 0.35, 0.65), solidAlpha);
                    }

                    float veins = pow(abs(sin(i.worldPos.x * 17.0 + i.worldPos.y * 11.0 + i.worldPos.z * 9.0)), 8.0);
                    fixed3 stone = tint * (0.55 + facet * 0.18);
                    stone += veins * fixed3(0.18, 0.17, 0.15);
                    stone += reflected * 0.05 + (highlightA + highlightB) * 0.08;
                    return fixed4(CrystalContrastGrade(stone, highlightA + highlightB + edge * 0.28, 0.75), solidAlpha);
                }

                return fixed4(CrystalContrastGrade(glass, whiteReturn + plasmaEnergy * 0.45 + edge * 0.4, 1.0 - facetDepth), solidAlpha);
            }
            ENDCG
        }
    }
}
