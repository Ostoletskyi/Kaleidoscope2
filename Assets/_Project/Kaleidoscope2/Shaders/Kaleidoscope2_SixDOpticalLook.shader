Shader "Kaleidoscope2/SixDOpticalLook"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FocusStrength ("Focus Strength", Range(0,1)) = 0.38
        _FocusFalloff ("Focus Falloff", Range(0.4,4)) = 1.75
        _ChromaticAmount ("Chromatic Amount", Float) = 0.0065
        _DistortionStrength ("Distortion Strength", Range(0,1)) = 0.34
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
            float _FocusStrength;
            float _FocusFalloff;
            float _ChromaticAmount;
            float _DistortionStrength;
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

            fixed4 SampleSoft(float2 uv, float radius)
            {
                float2 t = _InputTexelSize.xy * radius;
                fixed4 c = tex2D(_MainTex, uv) * 0.42;
                c += tex2D(_MainTex, uv + float2(t.x, 0.0)) * 0.145;
                c += tex2D(_MainTex, uv - float2(t.x, 0.0)) * 0.145;
                c += tex2D(_MainTex, uv + float2(0.0, t.y)) * 0.145;
                c += tex2D(_MainTex, uv - float2(0.0, t.y)) * 0.145;
                return c;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 p = uv - 0.5;
                float radius = length(p);
                float2 dir = radius > 0.0001 ? p / radius : float2(0.0, 1.0);
                float shakePulse = saturate(_MotionShake);
                float2 motionOffset = _MotionOffset.xy;
                motionOffset -= dir * _FlightTime * (0.014 + saturate(1.0 - radius) * 0.010);
                motionOffset += float2(sin(_TimeValue * 37.0 + p.y * 19.0), cos(_TimeValue * 33.0 - p.x * 23.0)) * shakePulse * 0.020;

                float breathe = sin(_TimeValue * 0.62) * _MotionBreathing;
                float barrel = radius * radius * _DistortionStrength * (0.08 + breathe * 0.018);
                float2 lensUV = 0.5 + p * (1.0 + barrel) + motionOffset;

                float focusMask = smoothstep(0.18, 0.95, pow(saturate(radius), max(0.4, _FocusFalloff)));
                float blurRadius = focusMask * _FocusStrength * 5.0;
                fixed4 soft = SampleSoft(frac(lensUV), blurRadius);
                fixed4 sharp = tex2D(_MainTex, frac(lensUV));
                fixed4 col = lerp(sharp, soft, saturate(focusMask * _FocusStrength));

                float chroma = _ChromaticAmount * (0.35 + radius * 1.65 + _DistortionStrength * 0.75);
                if (chroma > 0.00001)
                {
                    float2 redUV = frac(lensUV + dir * chroma);
                    float2 blueUV = frac(lensUV - dir * chroma);
                    col.r = tex2D(_MainTex, redUV).r;
                    col.b = tex2D(_MainTex, blueUV).b;
                }

                float centerSharp = pow(saturate(1.0 - radius * 1.45), 2.4);
                col.rgb += centerSharp * 0.035;
                col.rgb *= lerp(1.0, 0.86, smoothstep(0.72, 1.05, radius));

                return col;
            }
            ENDCG
        }
    }
}
