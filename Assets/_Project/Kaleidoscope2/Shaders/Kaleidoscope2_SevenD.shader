Shader "Kaleidoscope2/SevenD"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strategy ("Strategy", Float) = 0
        _EffectStrength ("Effect Strength", Range(0,1)) = 0.92
        _PatternScale ("Pattern Scale", Range(0.2,3)) = 1.15
        _MotionSpeed ("Motion Speed", Range(0,3)) = 0.7
        _SourceBlend ("Source Blend", Range(0,1)) = 0.22
        _TimeValue ("Time", Float) = 0
        _InputTexelSize ("Input Texel Size", Vector) = (0.001, 0.001, 1024, 1024)
        _MotionOffset ("Motion Offset", Vector) = (0, 0, 0, 0)
        _FlightTime ("Flight Time", Float) = 0
        _MotionShake ("Motion Shake", Range(0,1)) = 0
        _ImageReanimationBlend ("Image Reanimation Blend", Range(0,1)) = 0
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
            float _Strategy;
            float _EffectStrength;
            float _PatternScale;
            float _MotionSpeed;
            float _SourceBlend;
            float _TimeValue;
            float4 _InputTexelSize;
            float4 _MotionOffset;
            float _FlightTime;
            float _MotionShake;
            float _ImageReanimationBlend;

            static const float PI = 3.14159265;
            static const float TWO_PI = 6.28318531;
            static const float GOLDEN = 2.39996323;

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

            float2 Rotate2(float2 p, float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            float Luma(float3 c)
            {
                return dot(c, float3(0.299, 0.587, 0.114));
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 SafeUv(float2 p)
            {
                return frac(0.5 + p);
            }

            fixed4 SampleKaleidoscope(float2 p)
            {
                return tex2D(_MainTex, SafeUv(p));
            }

            fixed4 SampleRomanesco(float2 p, float time)
            {
                float r = length(p);
                float a = atan2(p.y, p.x);
                float logR = log(max(r, 0.006));
                float breathing = sin(time * 0.42) * 0.04;

                float budLayer = frac(-logR * 1.38 + a / GOLDEN + time * 0.035);
                float branch = sin(a * 13.0 + logR * 8.0 - time * 0.55);
                float budRadius = lerp(0.18, 0.74, pow(budLayer, 0.72));
                float shell = pow(frac(r * 3.8 - a * 0.19 + time * 0.025), 0.82);

                float2 p1 = Rotate2(p, logR * 1.85 + branch * 0.11 + time * 0.05);
                p1 *= 0.62 + budRadius * 0.55 + breathing;
                float2 p2 = Rotate2(p * (1.35 + shell * 0.92), a * 0.33 + GOLDEN + time * 0.08);
                float2 p3 = Rotate2(normalize(p + 0.0001) * (0.18 + shell * 0.58), a * 2.0 + logR * 0.75);

                fixed4 c1 = SampleKaleidoscope(p1);
                fixed4 c2 = SampleKaleidoscope(p2);
                fixed4 c3 = SampleKaleidoscope(p3);
                float ridges = pow(saturate(0.5 + 0.5 * branch), 3.0);
                return lerp(lerp(c1, c2, 0.42), c3, ridges * 0.38);
            }

            fixed4 SampleSnowflake(float2 p, float time)
            {
                float r = length(p);
                float a = atan2(p.y, p.x);
                float sector = PI / 3.0;
                float folded = abs(frac(a / sector + 0.5) - 0.5) * sector;
                float branch = sin(r * 31.0 - time * 0.18) * 0.028;
                float crystalR = r + branch + sin(folded * 18.0 + r * 9.0) * 0.018;

                float2 axis = float2(cos(folded), sin(folded)) * crystalR;
                float2 branchUv = float2(axis.x, abs(axis.y) * 1.8 - crystalR * 0.18);
                float2 prismUv = Rotate2(branchUv, floor(a / sector) * 0.17 + time * 0.025);

                fixed4 c1 = SampleKaleidoscope(prismUv);
                fixed4 c2 = SampleKaleidoscope(float2(prismUv.x, -prismUv.y) * 1.18);
                fixed4 c3 = SampleKaleidoscope(Rotate2(prismUv, sector) * 0.86);
                float iceLine = pow(saturate(1.0 - abs(folded) / sector), 2.4);
                return lerp(lerp(c1, c2, 0.33), c3, iceLine * 0.38);
            }

            fixed4 SampleStructuralColor(float2 p, float time)
            {
                float r = length(p);
                float a = atan2(p.y, p.x);
                float ridge = sin(p.x * 42.0 + p.y * 17.0 + sin(a * 5.0 + time) * 2.0);
                float wave = sin(r * 54.0 - a * 11.0 + time * 1.35);
                float2 tangent = normalize(float2(-p.y, p.x) + 0.0001);
                float2 normal = normalize(p + 0.0001);
                float micro = (ridge * 0.55 + wave * 0.45);
                float offset = (0.004 + r * 0.011) * _EffectStrength;

                float2 baseP = p + tangent * micro * offset * 0.75;
                fixed red = SampleKaleidoscope(baseP + normal * offset * 1.25).r;
                fixed green = SampleKaleidoscope(baseP + tangent * offset * micro * 0.35).g;
                fixed blue = SampleKaleidoscope(baseP - normal * offset * 1.35).b;
                fixed4 body = SampleKaleidoscope(baseP * (1.0 + micro * 0.035));
                body.rgb = lerp(body.rgb, float3(red, green, blue), 0.72);
                body.rgb *= 0.88 + 0.24 * saturate(0.5 + 0.5 * wave);
                return body;
            }

            fixed4 SampleMurmuration(float2 p, float time)
            {
                float2 flow = 0.0;
                float density = 0.0;

                for (int i = 0; i < 24; i++)
                {
                    float fi = (float)i;
                    float h = Hash21(float2(fi, fi * 1.73));
                    float a = fi * GOLDEN + time * (0.12 + h * 0.22);
                    float rr = 0.12 + frac(fi * 0.137 + sin(time * 0.17 + fi) * 0.05) * 0.78;
                    float2 bird = float2(cos(a + sin(time * 0.31 + fi) * 0.28), sin(a * 0.77 - time * 0.18)) * rr;
                    bird += float2(sin(time * 0.23 + fi * 1.9), cos(time * 0.19 + fi * 1.4)) * 0.09;
                    float2 delta = p - bird;
                    float d = dot(delta, delta) + 0.006;
                    float influence = saturate(0.012 / d);
                    float2 heading = normalize(float2(-sin(a), cos(a)) + delta * 0.65);
                    flow += heading * influence;
                    density += influence;
                }

                flow /= max(1.0, density);
                float wake = saturate(density * 0.16);
                fixed4 c0 = SampleKaleidoscope(p - flow * 0.055 * _EffectStrength);
                fixed4 c1 = SampleKaleidoscope(p - flow * 0.11 * _EffectStrength + float2(sin(time), cos(time * 0.7)) * 0.006);
                fixed4 c2 = SampleKaleidoscope(p + flow * 0.035 * _EffectStrength);
                fixed4 result = lerp(c0, c1, 0.38);
                result = lerp(result, c2, wake * 0.42);
                result.rgb *= 0.88 + wake * 0.28;
                return result;
            }

            fixed4 SampleSunflower(float2 p, float time)
            {
                float r = length(p);
                float a = atan2(p.y, p.x);
                float ringIndex = floor(r * 14.0);
                float seedAngle = ringIndex * GOLDEN + time * 0.035;
                float petalFold = sin(a * 34.0 + r * 8.0 + time * 0.12);
                float seedPhase = frac(r * 8.0 - a / GOLDEN + time * 0.02);

                float2 phyllo = Rotate2(p, seedAngle + petalFold * 0.045);
                phyllo *= 0.72 + seedPhase * 0.72;
                float2 centerLens = normalize(p + 0.0001) * pow(saturate(r), 0.72) * 0.56;
                float2 petalUv = Rotate2(float2(abs(phyllo.x), phyllo.y), sin(r * 13.0 + time) * 0.12);

                fixed4 seedSample = SampleKaleidoscope(phyllo);
                fixed4 petalSample = SampleKaleidoscope(petalUv * 1.18);
                fixed4 centerSample = SampleKaleidoscope(centerLens);
                float center = smoothstep(0.62, 0.05, r);
                float petals = smoothstep(0.24, 0.86, r) * smoothstep(1.1, 0.7, r) * pow(saturate(0.5 + 0.5 * petalFold), 2.0);
                return lerp(lerp(seedSample, petalSample, petals * 0.46), centerSample, center * 0.42);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 baseP = (uv - 0.5) * _PatternScale;
                float baseR = length(baseP);
                float2 dir = baseR > 0.0001 ? baseP / baseR : float2(0.0, 1.0);
                float time = _TimeValue * max(0.05, _MotionSpeed);
                float shakePulse = saturate(_MotionShake);
                float2 shakeOffset = float2(sin(time * 31.0 + baseP.y * 17.0), cos(time * 29.0 - baseP.x * 19.0)) * shakePulse * 0.030;
                float2 motionOffset = _MotionOffset.xy + shakeOffset;
                float2 p = baseP + motionOffset * _PatternScale - dir * _FlightTime * (0.018 + saturate(1.0 - baseR) * 0.012);
                float r = length(p);
                fixed4 source = tex2D(_MainTex, frac(uv + motionOffset));

                float inward = sin(time * 0.44 + r * 5.0) * 0.012 * _EffectStrength;
                p -= normalize(p + 0.0001) * inward * saturate(1.0 - r);

                int strategy = (int)round(_Strategy);
                fixed4 remapped = SampleRomanesco(p, time);
                if (strategy == 1)
                {
                    remapped = SampleSnowflake(p, time);
                }
                else if (strategy == 2)
                {
                    remapped = SampleStructuralColor(p, time);
                }
                else if (strategy == 3)
                {
                    remapped = SampleMurmuration(p, time);
                }
                else if (strategy == 4)
                {
                    remapped = SampleSunflower(p, time);
                }

                float sourceAnchor = _SourceBlend * 0.18;
                float centerStability = pow(saturate(1.0 - r * 1.25), 2.2) * 0.10;
                fixed4 finalColor = lerp(source, remapped, saturate(_EffectStrength));
                finalColor.rgb = lerp(finalColor.rgb, source.rgb, sourceAnchor + centerStability);
                finalColor.a = source.a;
                fixed4 cleanCol = tex2D(_MainTex, uv);
                return lerp(finalColor, cleanCol, saturate(_ImageReanimationBlend));
            }
            ENDCG
        }
    }
}
