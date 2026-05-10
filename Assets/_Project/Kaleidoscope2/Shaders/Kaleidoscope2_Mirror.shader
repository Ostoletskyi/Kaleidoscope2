Shader "Kaleidoscope2/Mirror"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MirrorCount ("Mirror Count", Float) = 6
        _Rotation ("Rotation (rad)", Float) = 0
        _Zoom ("Zoom", Float) = 1
        _CenterOffset ("Center Offset", Vector) = (0, 0, 0, 0)
        _MotionOffset ("Motion Offset", Vector) = (0, 0, 0, 0)
        _Scroll ("Forward Scroll", Float) = 0
        _GuidesVisible ("Guides Visible", Float) = 0
        _GuideStrength ("Guide Strength", Range(0,1)) = 0.65
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
            float4 _MotionOffset;
            float _Scroll;
            float _GuidesVisible;
            float _GuideStrength;

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
                // "Forward" illusion: scroll along radius.
                float rr = r + _Scroll;
                float2 sampleUV = (dir * rr) + 0.5 + _MotionOffset.xy;

                float guideLine = 0.0;
                float guideShadow = 0.0;
                if (_GuidesVisible > 0.5)
                {
                    float rawAngle = atan2(p.y, p.x) + _Rotation;
                    float a = rawAngle - segment * floor(rawAngle / segment);
                    float d = min(a, segment - a);
                    float boundarySide = a < segment * 0.5 ? -1.0 : 1.0;

                    float bend = sin(r * 22.0 + _Time.y * 1.7) * 0.10 * segment;
                    d = abs(d + bend);

                    float guideWidth = max(segment * 0.055, 0.00065);
                    guideLine = 1.0 - smoothstep(0.0, guideWidth, d);
                    guideShadow = 1.0 - smoothstep(guideWidth, guideWidth * 4.5, d);

                    float2 tangent = float2(-dir.y, dir.x);
                    sampleUV += tangent * boundarySide * guideLine * _GuideStrength * 0.010;
                }

                fixed4 col = tex2D(_MainTex, frac(sampleUV));

                // Segment guides are toggled through MirrorSettings, routed by Director commands.
                if (_GuidesVisible > 0.5)
                {
                    float strength = saturate(_GuideStrength);
                    float3 edgeTint = lerp(col.rgb * 0.72, float3(0.86, 0.94, 1.0), 0.28);
                    col.rgb = lerp(col.rgb, edgeTint, guideShadow * strength * 0.55);
                    col.rgb += guideLine * strength * 0.10;
                }

                return col;
            }
            ENDCG
        }
    }
}
