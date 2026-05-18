Shader "Kaleidoscope2/CrystalStage3D/PremiumOpticalBackground"
{
    Properties
    {
        _MainTex ("Selected Kaleidoscope Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (0.65, 0.9, 1, 1)
        _RingColor ("Ring Color", Color) = (1, 0.74, 0.32, 1)
        _GlowStrength ("Glow Strength", Range(0,1)) = 0.34
        _RingStrength ("Ring Strength", Range(0,1)) = 0.28
        _SparkleStrength ("Sparkle Strength", Range(0,1)) = 0.16
        _VignetteStrength ("Vignette Strength", Range(0,1)) = 0.42
        _PrismStrength ("Prism Strength", Range(0,1)) = 0.18
        _ViewAspect ("View Aspect", Float) = 1.7777
        _TextureAspect ("Texture Aspect", Float) = 1.7777
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 100
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            float4 _RingColor;
            half _GlowStrength;
            half _RingStrength;
            half _SparkleStrength;
            half _VignetteStrength;
            half _PrismStrength;
            float _ViewAspect;
            float _TextureAspect;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 CoverUv(float2 uv)
            {
                float viewAspect = max(0.0001, _ViewAspect);
                float textureAspect = max(0.0001, _TextureAspect);
                if (textureAspect > viewAspect)
                {
                    float scaleX = viewAspect / textureAspect;
                    uv.x = (uv.x - 0.5) * scaleX + 0.5;
                }
                else
                {
                    float scaleY = textureAspect / viewAspect;
                    uv.y = (uv.y - 0.5) * scaleY + 0.5;
                }

                return saturate(uv);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = saturate(i.uv);
                float2 sourceUv = CoverUv(uv);
                float2 centered = uv - 0.5;
                centered.x *= max(0.0001, _ViewAspect);
                float radius = length(centered);
                float angle = atan2(centered.y, centered.x);
                float time = _Time.y;

                fixed3 source = tex2D(_MainTex, sourceUv).rgb;
                source = max(source, fixed3(0.035, 0.035, 0.035));

                float radialGlow = exp(-radius * radius * 2.35);
                float pulse = 0.86 + 0.14 * sin(time * 0.65);
                fixed3 glow = _GlowColor.rgb * radialGlow * _GlowStrength * pulse;

                float ringWave = sin(radius * 42.0 - time * 0.75 + sin(angle * 6.0) * 0.22);
                float ringLine = pow(saturate(ringWave * 0.5 + 0.5), 22.0);
                float outerRing = pow(saturate(1.0 - abs(radius - 0.43) * 14.0), 2.0);
                fixed3 rings = _RingColor.rgb * (ringLine * 0.65 + outerRing * 0.85) * _RingStrength;

                float2 sparkleCell = floor(uv * float2(22.0, 13.0));
                float sparkleSeed = Hash21(sparkleCell);
                float2 sparkleCenter = (sparkleCell + float2(Hash21(sparkleCell + 1.7), Hash21(sparkleCell + 7.1))) / float2(22.0, 13.0);
                float sparkleDistance = length((uv - sparkleCenter) * float2(max(1.0, _ViewAspect), 1.0));
                float sparkleBlink = smoothstep(0.76, 1.0, sin(time * (1.35 + sparkleSeed * 1.8) + sparkleSeed * 38.0) * 0.5 + 0.5);
                float sparkle = pow(saturate(1.0 - sparkleDistance * 58.0), 4.0) * step(0.78, sparkleSeed) * sparkleBlink;
                fixed3 sparkles = lerp(fixed3(0.65, 0.9, 1.0), fixed3(1.0, 0.72, 0.38), sparkleSeed) * sparkle * _SparkleStrength;

                float flareLine = 1.0 - smoothstep(0.0, 0.045, abs(uv.y - (0.52 + centered.x * 0.16 + sin(time * 0.24) * 0.025)));
                float flareMask = smoothstep(0.12, 0.55, uv.x) * (1.0 - smoothstep(0.56, 1.02, uv.x));
                fixed3 prism = fixed3(flareLine * 0.92, flareLine * 0.62, flareLine * 1.0) * flareMask * _PrismStrength;

                float vignette = 1.0 - smoothstep(0.24, 0.95, radius);
                float vignetteMix = lerp(1.0 - _VignetteStrength, 1.0, vignette);

                fixed3 color = source * (0.82 + radialGlow * 0.18);
                color += glow + rings + sparkles + prism;
                color *= vignetteMix;
                return fixed4(saturate(color), 1.0);
            }
            ENDCG
        }
    }
    FallBack "Unlit/Texture"
}
