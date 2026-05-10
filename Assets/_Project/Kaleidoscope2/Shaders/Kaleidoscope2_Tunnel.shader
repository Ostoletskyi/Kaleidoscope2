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
        _TunnelShake ("Tunnel Shake", Range(0,1)) = 0
        _TunnelChromaticAberrationEnabled ("Tunnel Chromatic Aberration Enabled", Float) = 0
        _TunnelChromaticAberrationStrength ("Tunnel Chromatic Aberration Strength", Float) = 0.0075
        _ModeMotionOffset ("Mode Motion Offset", Vector) = (0, 0, 0, 0)
        _ModeMotionShake ("Mode Motion Shake", Range(0,1)) = 0
        _FiveDEnabled ("5D Enabled", Float) = 0
        _FiveDTime ("5D Time", Float) = 0
        _FiveDShake ("5D Shake", Range(0,1)) = 0
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
            float _TunnelShake;
            float _TunnelChromaticAberrationEnabled;
            float _TunnelChromaticAberrationStrength;
            float4 _ModeMotionOffset;
            float _ModeMotionShake;
            float _FiveDEnabled;
            float _FiveDTime;
            float _FiveDShake;

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
                float motionShake = saturate(_ModeMotionShake);
                float2 motionOffset = _ModeMotionOffset.xy;
                if (motionShake > 0.001)
                {
                    float motionPhase = _Scroll * 29.0 + dot(p, float2(13.0, -11.0));
                    motionOffset += float2(sin(motionPhase * 1.37), cos(motionPhase * 1.71)) * motionShake * motionShake * 0.035;
                }

                if (_FiveDEnabled > 0.5)
                {
                    const float PI = 3.14159265;
                    const float TWO_PI = 6.28318531;

                    float radius = length(p);
                    float2 safeDir = radius > 0.0001 ? p / radius : float2(0.0, 1.0);
                    float angle = atan2(p.y, p.x);
                    float shakeWave = sin(_FiveDTime * 37.0 + angle * 11.0) * _FiveDShake;
                    float2 shakenP = p + safeDir * shakeWave * 0.045 + float2(sin(_FiveDTime * 23.0), cos(_FiveDTime * 19.0)) * _FiveDShake * 0.018;

                    radius = length(shakenP);
                    angle = atan2(shakenP.y, shakenP.x);

                    float safeR = max(radius, 0.006);
                    float depth = -log(safeR) * 0.72 + _FiveDTime;
                    float loop = frac(depth);
                    float twist = depth * PI;

                    // One half-turn per loop gives the tunnel a Mobius-strip return.
                    float mobiusAngle = angle + twist;
                    float filmU = mobiusAngle / TWO_PI + loop * 0.5;
                    float filmV = loop + sin(mobiusAngle * 2.0 + depth) * 0.045;
                    float2 filmUV = frac(float2(filmU, filmV));

                    fixed4 col = tex2D(_MainTex, filmUV);

                    float tunnelWall = saturate(1.0 - radius * 1.35);
                    float centerPull = pow(saturate(1.0 - radius * 2.1), 2.5);
                    float rail = pow(saturate(abs(sin(mobiusAngle * 8.0))), 18.0);
                    float ribbonShadow = 1.0 - smoothstep(0.0, 0.42, abs(frac(filmU) - 0.5));
                    float depthShade = lerp(0.62, 1.18, tunnelWall);

                    col.rgb *= depthShade;
                    col.rgb *= lerp(0.78, 1.05, ribbonShadow);
                    col.rgb += rail * tunnelWall * 0.10;
                    col.rgb += centerPull * 0.18;
                    col.rgb += _FiveDShake * 0.08;

                    return col;
                }

                if (_TunnelHoseEnabled < 0.5)
                {
                    float r = length(p);
                    float safeR = max(r, 0.03);
                    float perspective = _DepthScale / safeR;
                    float2 scroll = float2(_Scroll, _Scroll * 0.37);
                    float depth = saturate(1.0 - safeR);
                    float2 legacyBend = _Bend.xy * depth * depth * 1.35;
                    float2 legacyUV = 0.5 + (p + legacyBend) * perspective + scroll;
                    legacyUV = frac(legacyUV + motionOffset);

                    fixed4 legacyCol = tex2D(_MainTex, legacyUV);
                    float legacyCenter = saturate(1.0 - (r * 2.0));
                    legacyCol.rgb *= lerp(1.0, 1.0 - _CenterDarken, legacyCenter);
                    return legacyCol;
                }

                float tunnelShake = saturate(_TunnelShake);
                if (tunnelShake > 0.001)
                {
                    float shakePulse = tunnelShake * tunnelShake;
                    float shakePhase = _Scroll * 37.0;
                    float2 shakeDirection = float2(sin(shakePhase * 1.31), cos(shakePhase * 1.73));
                    float shakeRipple = sin(shakePhase * 2.11 + dot(p, float2(19.0, -17.0))) * 0.018;
                    p += shakeDirection * shakePulse * 0.035 + p * shakeRipple * shakePulse;
                }

                float2 bend = _TunnelBendOffset.xy;
                float bendMag = saturate(length(bend));
                float2 bendDir = bendMag > 0.0001 ? bend / bendMag : float2(0.0, 1.0);
                float2 bendPerp = float2(-bendDir.y, bendDir.x);
                float bendEase = smoothstep(0.0, 1.0, bendMag);

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
                float normalizedSide = side / max(entryRadius * 0.72, 0.18);
                float foldMask = 1.0 - smoothstep(-0.85, 0.25, normalizedSide);
                float stretchMask = smoothstep(-0.25, 0.9, normalizedSide);
                float foldFeather = 1.0 - smoothstep(0.0, 1.25, abs(normalizedSide));
                foldMask = saturate(lerp(foldMask, foldMask * (0.45 + foldFeather * 0.55), 0.75));
                stretchMask = saturate(lerp(stretchMask, stretchMask * (0.55 + foldFeather * 0.45), 0.55));

                float compression = 1.0 - foldMask * _TunnelFoldShadowStrength * bendEase * 0.55;
                float extension = 1.0 + stretchMask * _TunnelFoldStrength * bendEase * 0.30;

                corridor.x *= lerp(compression, extension, abs(bendDir.x));
                corridor.y *= lerp(compression, extension, abs(bendDir.y));
                float softSide = side / (1.0 + abs(side) * 2.5);
                corridor += bendPerp * softSide * bendEase * _TunnelFoldStrength * 0.16;

                float localRadius = length(corridor);
                float safeR = max(localRadius, 0.025);
                float perspective = (_DepthScale * lerp(0.92, 1.18, opening01)) / safeR;

                float2 scroll = float2(_Scroll, _Scroll * 0.37);
                float2 sampleUV = 0.5 + corridor * perspective + scroll;
                sampleUV = frac(sampleUV + motionOffset);

                fixed4 col = tex2D(_MainTex, sampleUV);
                float chroma = _TunnelChromaticAberrationEnabled > 0.5 ? _TunnelChromaticAberrationStrength : 0.0;
                if (chroma > 0.00001)
                {
                    float2 chromaDir = localRadius > 0.0001 ? corridor / localRadius : bendDir;
                    float chromaAmount = chroma * (0.35 + localRadius * 1.8 + bendEase * 0.9 + abs(wallCurvature) * 0.45);
                    fixed red = tex2D(_MainTex, frac(sampleUV + chromaDir * chromaAmount)).r;
                    fixed green = tex2D(_MainTex, sampleUV).g;
                    fixed blue = tex2D(_MainTex, frac(sampleUV - chromaDir * chromaAmount)).b;
                    col.rgb = float3(red, green, blue);
                }

                float mouthMask = 1.0 - smoothstep(entryRadius, entryRadius + 0.16, radial);
                float depthFade = pow(saturate(1.0 - localRadius), max(0.25, _TunnelDepthFade));
                float corridorShade = lerp(1.0, 1.0 - _TunnelDarknessDepth, 1.0 - depthFade);
                float foldShadow = smoothstep(0.12, 0.95, foldMask) * _TunnelFoldShadowStrength * bendEase;
                float outerHighlight = smoothstep(0.08, 0.9, stretchMask) * _TunnelFoldStrength * bendEase * 0.30;
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
                col.rgb += tunnelShake * 0.08;

                // Preserve the old center darken as a subtle extra vignette only.
                float center = saturate(1.0 - (radial * 2.0));
                col.rgb *= lerp(1.0, 1.0 - _CenterDarken * 0.25, center);

                return col;
            }
            ENDCG
        }
    }
}
