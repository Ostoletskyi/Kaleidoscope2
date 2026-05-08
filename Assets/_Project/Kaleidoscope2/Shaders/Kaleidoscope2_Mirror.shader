Shader "Kaleidoscope2/Mirror"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MirrorCount ("Mirror Count", Float) = 6
        _Rotation ("Rotation (rad)", Float) = 0
        _Zoom ("Zoom", Float) = 1
        _CenterOffset ("Center Offset", Vector) = (0, 0, 0, 0)
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
            float _MirrorCount;
            float _Rotation;
            float _Zoom;
            float4 _CenterOffset;

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

            static const float PI = 3.14159265;
            static const float TWO_PI = 6.28318530;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float2 p = (uv - 0.5) + _CenterOffset.xy;

                float zoom = max(_Zoom, 0.0001);
                p /= zoom;

                float r = length(p);
                float angle = atan2(p.y, p.x) + _Rotation;

                float mirrors = max(1.0, floor(_MirrorCount + 0.5));
                float segment = TWO_PI / mirrors;

                // Wrap angle into [0..segment), then mirror it around the segment center.
                angle = angle - segment * floor(angle / segment);
                angle = abs(angle - segment * 0.5);

                float2 dir = float2(cos(angle), sin(angle));
                float2 sampleUV = (dir * r) + 0.5;

                return tex2D(_MainTex, sampleUV);
            }
            ENDCG
        }
    }
}

