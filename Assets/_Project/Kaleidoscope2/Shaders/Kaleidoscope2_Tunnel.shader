Shader "Kaleidoscope2/Tunnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthScale ("Depth Scale", Float) = 2.2
        _CenterDarken ("Center Darken", Range(0,1)) = 0.55
        _Scroll ("Scroll", Float) = 0
        _Bend ("Bend", Vector) = (0, 0, 0, 0)
        _TunnelHoseEnabled ("Tunnel Hose Enabled", Float) = 0
        _TunnelBendOffset ("Tunnel Bend Offset", Vector) = (0, 0, 0, 0)
        _TunnelFoldStrength ("Tunnel Fold Strength", Float) = 0.45
        _TunnelFoldShadowStrength ("Tunnel Fold Shadow Strength", Float) = 0.55
        _TunnelDarknessDepth ("Tunnel Darkness Depth", Float) = 0.85
        _TunnelEndLightVisibility ("Tunnel End Light Visibility", Float) = 0.25
        _TunnelDepthFade ("Tunnel Depth Fade", Float) = 1.35
        _TunnelHoseOpening ("Tunnel Hose Opening", Range(-1,1)) = 0
        _TunnelWallCurvature ("Tunnel Wall Curvature", Range(-1,1)) = 0
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
            float _TunnelHoseEnabled;
            float4 _TunnelBendOffset;
            float _TunnelFoldStrength;
            float _TunnelFoldShadowStrength;
            float _TunnelDarknessDepth;
            float _TunnelEndLightVisibility;
            float _TunnelDepthFade;
            float _TunnelHoseOpening;
            float _TunnelWallCurvature;

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

                if (_TunnelHoseEnabled < 0.5)
                {
                    float r = length(p);
                    float safeR = max(r, 0.03);
                    float perspective = _DepthScale / safeR;
                    float2 scroll = float2(_Scroll, _Scroll * 0.37);
                    float depth = saturate(1.0 - safeR);
                    float2 legacyBend = _Bend.xy * depth * depth * 1.35;
                    float2 legacyUV = 0.5 + (p + legacyBend) * perspective + scroll;
                    legacyUV = frac(legacyUV);

                    fixed4 legacyCol = tex2D(_MainTex, legacyUV);
                    float legacyCenter = saturate(1.0 - (r * 2.0));
                    legacyCol.rgb *= lerp(1.0, 1.0 - _CenterDarken, legacyCenter);
                    return legacyCol;
                }

                float2 bend = _TunnelBendOffset.xy;
                float bendMag = saturate(length(bend));
                float2 bendDir = bendMag > 0.0001 ? bend / bendMag : float2(0.0, 1.0);
                float2 bendPerp = float2(-bendDir.y, bendDir.x);

                float opening = clamp(_TunnelHoseOpening, -1.0, 1.0);
                float wallCurvature = clamp(_TunnelWallCurvature, -1.0, 1.0);
                float opening01 = opening * 0.5 + 0.5;
                float entryRadius = lerp(0.34, 0.86, opening01);
                float profilePower = pow(6.0, saturate(-wallCurvature)) * pow(0.2, saturate(wallCurvature));

                float radial = length(p);
                float2 radialDir = radial > 0.0001 ? p / radial : bendDir;
                float normalizedRadius = radial / max(entryRadius, 0.05);
                float clippedRadius = saturate(normalizedRadius);
                float profiledRadius = pow(max(clippedRadius, 0.0001), profilePower) * entryRadius;
                profiledRadius = radial > 0.0001 ? profiledRadius : 0.0;
                profiledRadius += max(0.0, normalizedRadius - 1.0) * entryRadius * lerp(0.35, 0.75, opening01);
                float2 profiledP = radialDir * profiledRadius;

                float straightDepth = saturate(1.0 - pow(max(clippedRadius, 0.0001), 1.0 / max(profilePower, 0.05)));
                float expressiveDepth = lerp(straightDepth, saturate(straightDepth * 1.25), saturate(-wallCurvature));
                float depthCurve = pow(max(expressiveDepth, 0.0001), max(0.25, _TunnelDepthFade));

                // Shift the tunnel axis with depth so the corridor feels bent rather than only re-centered.
                float stillCore = saturate(1.0 - radial / max(0.12, entryRadius * 0.62));
                float movingZone = 1.0 - stillCore * 0.75;
                float2 corridor = profiledP - bend * depthCurve * movingZone * (0.65 + bendMag * 0.35);

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
                float perspective = (_DepthScale * lerp(0.92, 1.18, opening01)) / safeR;

                float2 scroll = float2(_Scroll, _Scroll * 0.37);
                float2 sampleUV = 0.5 + corridor * perspective + scroll;
                sampleUV = frac(sampleUV);

                fixed4 col = tex2D(_MainTex, sampleUV);

                float mouthMask = 1.0 - smoothstep(entryRadius, entryRadius + 0.16, radial);
                float depthFade = pow(saturate(1.0 - localRadius), max(0.25, _TunnelDepthFade));
                float corridorShade = lerp(1.0, 1.0 - _TunnelDarknessDepth, 1.0 - depthFade);
                float foldShadow = foldMask * _TunnelFoldShadowStrength * bendMag;
                float outerHighlight = stretchMask * _TunnelFoldStrength * bendMag * 0.35;
                float endLight = _TunnelEndLightVisibility * (1.0 - bendMag) * depthFade;
                float squeezeShadow = saturate(-wallCurvature) * smoothstep(0.15, 0.95, expressiveDepth) * 0.28;
                float flareHighlight = saturate(wallCurvature) * smoothstep(0.08, 0.85, clippedRadius) * 0.18;

                col.rgb *= corridorShade;
                col.rgb *= 1.0 - foldShadow * 0.7;
                col.rgb *= 1.0 - squeezeShadow;
                col.rgb *= lerp(0.38, 1.0, mouthMask);
                col.rgb += outerHighlight * 0.08;
                col.rgb += flareHighlight;
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
