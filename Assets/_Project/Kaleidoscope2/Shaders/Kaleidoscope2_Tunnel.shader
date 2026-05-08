Shader "Kaleidoscope2/Tunnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthScale ("Depth Scale", Float) = 2.2
        _CenterDarken ("Center Darken", Range(0,1)) = 0.55
        _Scroll ("Scroll", Float) = 0
        _Bend ("Bend", Vector) = (0, 0, 0, 0)
        _TunnelBendOffset ("Tunnel Bend Offset", Vector) = (0, 0, 0, 0)
        _TunnelFoldStrength ("Tunnel Fold Strength", Float) = 0.45
        _TunnelFoldShadowStrength ("Tunnel Fold Shadow Strength", Float) = 0.55
        _TunnelDarknessDepth ("Tunnel Darkness Depth", Float) = 0.85
        _TunnelEndLightVisibility ("Tunnel End Light Visibility", Float) = 0.25
        _TunnelDepthFade ("Tunnel Depth Fade", Float) = 1.35
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
            float4 _Bend;
            float4 _TunnelBendOffset;
            float _TunnelFoldStrength;
            float _TunnelFoldShadowStrength;
            float _TunnelDarknessDepth;
            float _TunnelEndLightVisibility;
            float _TunnelDepthFade;

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

                float2 bend = _TunnelBendOffset.xy;
                float bendMag = saturate(length(bend));
                float2 bendDir = bendMag > 0.0001 ? bend / bendMag : float2(0.0, 1.0);
                float2 bendPerp = float2(-bendDir.y, bendDir.x);

                float radial = length(p);
                float straightDepth = saturate(1.0 - radial * 1.55);
                float depthCurve = pow(max(straightDepth, 0.0001), max(0.25, _TunnelDepthFade));

                // Shift the tunnel axis with depth so the corridor feels bent rather than only re-centered.
                float2 corridor = p - bend * depthCurve * (0.65 + bendMag * 0.35);

                float side = dot(corridor, bendDir);
                float foldMask = saturate(0.5 - side * 1.6);
                float stretchMask = saturate(0.5 + side * 1.2);

                float compression = 1.0 - foldMask * _TunnelFoldShadowStrength * bendMag * 0.65;
                float extension = 1.0 + stretchMask * _TunnelFoldStrength * bendMag * 0.35;

                corridor.x *= lerp(compression, extension, abs(bendDir.x));
                corridor.y *= lerp(compression, extension, abs(bendDir.y));
                corridor += bendPerp * side * bendMag * _TunnelFoldStrength * 0.12;

                float localRadius = length(corridor);
                float safeR = max(localRadius, 0.025);
                float perspective = _DepthScale / safeR;

                float2 scroll = float2(_Scroll, _Scroll * 0.37);
                float2 sampleUV = 0.5 + corridor * perspective + scroll;
                sampleUV = frac(sampleUV);

                fixed4 col = tex2D(_MainTex, sampleUV);

                float depthFade = pow(saturate(1.0 - localRadius), max(0.25, _TunnelDepthFade));
                float corridorShade = lerp(1.0, 1.0 - _TunnelDarknessDepth, 1.0 - depthFade);
                float foldShadow = foldMask * _TunnelFoldShadowStrength * bendMag;
                float outerHighlight = stretchMask * _TunnelFoldStrength * bendMag * 0.35;
                float endLight = _TunnelEndLightVisibility * (1.0 - bendMag) * depthFade;

                col.rgb *= corridorShade;
                col.rgb *= 1.0 - foldShadow * 0.7;
                col.rgb += outerHighlight * 0.08;
                col.rgb += endLight * 0.16;

                // Preserve the old center darken as a subtle extra vignette only.
                float center = saturate(1.0 - (radial * 2.0));
                col.rgb *= lerp(1.0, 1.0 - _CenterDarken * 0.25, center);

                return col;
            }
            ENDCG
        }
    }
}
