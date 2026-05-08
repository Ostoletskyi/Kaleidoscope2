Shader "Kaleidoscope2/Tunnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthScale ("Depth Scale", Float) = 2.2
        _CenterDarken ("Center Darken", Range(0,1)) = 0.55
        _Scroll ("Scroll", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DepthScale;
            float _CenterDarken;
            float _Scroll;

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

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 p = uv - 0.5;

                float r = length(p);
                float safeR = max(r, 0.03);

                // Perspective warp: pixels near center sample "deeper" (higher magnification).
                float perspective = _DepthScale / safeR;

                // A subtle scroll gives the tunnel a sense of depth motion.
                float2 scroll = float2(_Scroll, _Scroll * 0.37);

                float2 sampleUV = 0.5 + p * perspective + scroll;
                sampleUV = frac(sampleUV);

                fixed4 col = tex2D(_MainTex, sampleUV);

                // Darken the center slightly.
                float center = saturate(1.0 - (r * 2.0));
                col.rgb *= lerp(1.0, 1.0 - _CenterDarken, center);

                return col;
            }
            ENDCG
        }
    }
}

