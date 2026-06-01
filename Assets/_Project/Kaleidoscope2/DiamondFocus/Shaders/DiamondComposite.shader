Shader "Kaleidoscope2/DiamondComposite"
{
    Properties
    {
        _MainTex ("Background", 2D) = "white" {}
        _BlurredBackgroundTex ("Blurred Background", 2D) = "white" {}
        _CrystalTex ("3D Crystal", 2D) = "black" {}
        _FocusAmount ("Focus Amount", Range(0,1)) = 0
        _BackgroundBlurAmount ("Background Blur Amount", Range(0,1)) = 0
        _InputTexelSize ("Input Texel Size", Vector) = (0.001, 0.001, 1024, 1024)
        _CrystalTexelSize ("Crystal Texel Size", Vector) = (0.001, 0.001, 1024, 1024)
        _ComfortSplitAmount ("Comfort Split Amount", Range(0,1)) = 0
        _ComfortOrbitAngle ("Comfort Orbit Angle", Float) = 0
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
            sampler2D _BlurredBackgroundTex;
            sampler2D _CrystalTex;
            float4 _MainTex_ST;
            float _BackgroundBlurAmount;
            float _ComfortSplitAmount;
            float _ComfortOrbitAngle;

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

            static const float KaelisSplitTau = 6.2831853;
            static const float KaelisSplitChildCount = 6.0;

            fixed4 SampleComfortCrystal(float2 uv, float2 offset, float scale, float brightness)
            {
                float2 centered = (uv - float2(0.5, 0.5) - offset) / scale;
                float2 localUv = centered + float2(0.5, 0.5);
                fixed4 value = tex2D(_CrystalTex, localUv);
                float2 distanceFromCenter = abs(localUv - float2(0.5, 0.5));
                float edgeMask = 1.0 - smoothstep(0.498, 0.5, max(distanceFromCenter.x, distanceFromCenter.y));
                value.a *= edgeMask;
                value.rgb *= brightness;
                return value;
            }

            float2 ResolveChildOffset(float segmentIndex, float opening)
            {
                float angle = segmentIndex * (KaelisSplitTau / KaelisSplitChildCount) + _ComfortOrbitAngle;
                float radius = 0.245 * opening;
                return float2(cos(angle), sin(angle)) * radius;
            }

            fixed4 BlendCrystal(fixed4 below, fixed4 above)
            {
                float alpha = above.a + below.a * (1.0 - above.a);
                fixed3 rgb = alpha > 0.0001
                    ? (above.rgb * above.a + below.rgb * below.a * (1.0 - above.a)) / alpha
                    : fixed3(0, 0, 0);
                return fixed4(rgb, alpha);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 clean = tex2D(_MainTex, i.uv);
                fixed4 blurred = tex2D(_BlurredBackgroundTex, i.uv);
                fixed4 crystal = SampleComfortCrystal(i.uv, float2(0.0, 0.0), 1.0, 1.0);
                float opening = saturate(_ComfortSplitAmount);
                if (opening > 0.0001)
                {
                    fixed4 split = fixed4(0, 0, 0, 0);
                    float childScale = lerp(0.76, 0.36, opening);
                    split = BlendCrystal(split, SampleComfortCrystal(i.uv, ResolveChildOffset(0.0, opening), childScale, 1.0));
                    split = BlendCrystal(split, SampleComfortCrystal(i.uv, ResolveChildOffset(1.0, opening), childScale, 1.0));
                    split = BlendCrystal(split, SampleComfortCrystal(i.uv, ResolveChildOffset(2.0, opening), childScale, 1.0));
                    split = BlendCrystal(split, SampleComfortCrystal(i.uv, ResolveChildOffset(3.0, opening), childScale, 1.0));
                    split = BlendCrystal(split, SampleComfortCrystal(i.uv, ResolveChildOffset(4.0, opening), childScale, 1.0));
                    split = BlendCrystal(split, SampleComfortCrystal(i.uv, ResolveChildOffset(5.0, opening), childScale, 1.0));
                    float transition = opening * opening * (3.0 - 2.0 * opening);
                    crystal = lerp(crystal, split, transition);
                }

                fixed3 background = lerp(clean.rgb, blurred.rgb, saturate(_BackgroundBlurAmount));
                fixed3 composite = lerp(background, crystal.rgb, saturate(crystal.a));
                return fixed4(composite, clean.a);
            }
            ENDCG
        }
    }
}
