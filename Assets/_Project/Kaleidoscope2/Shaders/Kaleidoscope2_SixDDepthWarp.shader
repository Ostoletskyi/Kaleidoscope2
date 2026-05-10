Shader "Kaleidoscope2/SixDDepthWarp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthStrength ("Depth Strength", Range(0,2)) = 0.82
        _ParallaxScale ("Parallax Scale", Float) = 0.035
        _CenterPull ("Center Pull", Range(0,1.5)) = 0.74
        _OpticalCompression ("Optical Compression", Range(0,1.5)) = 0.52
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
            float _DepthStrength;
            float _ParallaxScale;
            float _CenterPull;
            float _OpticalCompression;
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
                float2 dir = radius > 0.0001 ? p / radius : float2(0.0, 1.0);
                float2 tangent = float2(-dir.y, dir.x);
                float2 texel = _InputTexelSize.xy;
                float shakePulse = saturate(_MotionShake);
                float2 motionOffset = _MotionOffset.xy;
                motionOffset -= dir * _FlightTime * (0.018 + saturate(1.0 - radius) * 0.012);
                motionOffset += float2(sin(_TimeValue * 31.0 + radius * 17.0), cos(_TimeValue * 27.0 - radius * 13.0)) * shakePulse * 0.026;
                float2 baseUV = frac(uv + motionOffset);

                float centerLuma = Luma(tex2D(_MainTex, baseUV).rgb);
                float lumaX = Luma(tex2D(_MainTex, baseUV + float2(texel.x, 0.0)).rgb);
                float lumaY = Luma(tex2D(_MainTex, baseUV + float2(0.0, texel.y)).rgb);
                float edge = saturate(abs(centerLuma - lumaX) + abs(centerLuma - lumaY));
                float depth = saturate((pow(saturate(centerLuma), 1.15) * 0.72 + edge * 1.65) * _DepthStrength);

                float breathing = sin(_TimeValue * 0.75) * _MotionBreathing;
                float innerPressure = pow(saturate(1.0 - radius * 1.55), 2.0) * _CenterPull;
                float compression = 1.0 + _OpticalCompression * pow(saturate(radius), 2.0) * 0.16;
                float parallax = (depth - 0.46) * _ParallaxScale;
                float radialWarp = innerPressure * 0.018 + breathing * 0.008 * saturate(1.0 - radius);
                float tangentialDrift = sin(_TimeValue * 0.33 + radius * 8.0) * _ParallaxScale * 0.22 * depth;

                float2 warped = baseUV + p * (compression - 1.0);
                warped -= dir * (parallax + radialWarp);
                warped += tangent * tangentialDrift;
                warped = lerp(baseUV, warped, saturate(_DepthStrength));

                return tex2D(_MainTex, frac(warped));
            }
            ENDCG
        }
    }
}
