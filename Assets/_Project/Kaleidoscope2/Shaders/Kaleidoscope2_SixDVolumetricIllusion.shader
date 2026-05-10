Shader "Kaleidoscope2/SixDVolumetricIllusion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VolumetricDensity ("Volumetric Density", Range(0,1)) = 0.42
        _HazeStrength ("Haze Strength", Range(0,1)) = 0.36
        _CenterPull ("Center Pull", Range(0,1.5)) = 0.74
        _MotionBreathing ("Motion Breathing", Range(0,1)) = 0.32
        _TimeValue ("Time", Float) = 0
        _InputTexelSize ("Input Texel Size", Vector) = (0.001, 0.001, 1024, 1024)
        _MotionOffset ("Motion Offset", Vector) = (0, 0, 0, 0)
        _FlightTime ("Flight Time", Float) = 0
        _MotionShake ("Motion Shake", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _VolumetricDensity;
            float _HazeStrength;
            float _CenterPull;
            float _MotionBreathing;
            float _TimeValue;
            float4 _InputTexelSize;
            float4 _MotionOffset;
            float _FlightTime;
            float _MotionShake;

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

            float Luma(float3 c)
            {
                return dot(c, float3(0.299, 0.587, 0.114));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 p = uv - 0.5;
                float radius = length(p);
                float safeR = max(radius, 0.004);
                float2 dir = radius > 0.0001 ? p / radius : float2(0.0, 1.0);
                float shakePulse = saturate(_MotionShake);
                float2 motionOffset = _MotionOffset.xy;
                motionOffset -= dir * _FlightTime * (0.012 + saturate(1.0 - radius) * 0.009);
                motionOffset += float2(sin(_TimeValue * 25.0 + p.x * 21.0), cos(_TimeValue * 28.0 + p.y * 17.0)) * shakePulse * 0.018;
                float2 baseUV = frac(uv + motionOffset);

                fixed4 col = tex2D(_MainTex, baseUV);
                float luma = Luma(col.rgb);
                float depth = saturate(luma * 0.72 + pow(saturate(1.0 - radius), 2.0) * 0.42);

                float breath = sin(_TimeValue * 0.48) * _MotionBreathing;
                float layerDepth = -log(safeR) * 0.42 + _TimeValue * 0.08 + breath * 0.08;
                float softRings = pow(saturate(0.5 + 0.5 * sin(layerDepth * 8.0 + luma * 5.0)), 3.0);
                float tunnelHaze = pow(saturate(1.0 - radius * 1.28), 1.55) * _VolumetricDensity;
                float peripheralMist = smoothstep(0.28, 1.05, radius) * _HazeStrength * 0.38;
                float centerGlow = pow(saturate(1.0 - radius * (2.1 - _CenterPull * 0.28)), 3.2) * (0.16 + _CenterPull * 0.10);

                float2 scatterUV = frac(baseUV - dir * (0.012 + depth * 0.018) * _VolumetricDensity);
                fixed3 scatter = tex2D(_MainTex, scatterUV).rgb;
                fixed3 hazeColor = lerp(scatter, fixed3(0.72, 0.82, 0.92), 0.35);

                col.rgb = lerp(col.rgb, hazeColor, saturate(tunnelHaze * _HazeStrength));
                col.rgb += softRings * tunnelHaze * 0.075;
                col.rgb += centerGlow;
                col.rgb *= 1.0 - peripheralMist;
                col.rgb *= lerp(0.92, 1.08, depth);

                return col;
            }
            ENDCG
        }
    }
}
