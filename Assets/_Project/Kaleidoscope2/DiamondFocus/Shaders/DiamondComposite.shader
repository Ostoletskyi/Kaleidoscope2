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
        _CrystalDominance ("Crystal Dominance", Range(0,1)) = 0.82
        _BackgroundDim ("Background Dim", Range(0,1)) = 0.35
        _FocusVignette ("Focus Vignette", Range(0,1)) = 0.45
        _CompositeMode ("Composite Mode", Float) = 1
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
            float _CrystalDominance;
            float _BackgroundDim;
            float _FocusVignette;
            float _CompositeMode;

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
                fixed4 clean = tex2D(_MainTex, i.uv);
                fixed4 blurred = tex2D(_BlurredBackgroundTex, i.uv);
                fixed4 crystal = tex2D(_CrystalTex, i.uv);

                fixed3 background = lerp(clean.rgb, blurred.rgb, saturate(_BackgroundBlurAmount));
                fixed crystalAlpha = saturate(crystal.a);

                // Compute center-focused mask from crystal alpha so background stays behind the crystal.
                float2 centeredUv = i.uv * 2.0 - 1.0;
                float radial = saturate(length(centeredUv));
                float vignette = 1.0 - smoothstep(0.2, 1.0, radial);
                float focusMask = saturate(max(crystalAlpha, vignette * saturate(_FocusVignette)));

                // Dim and defocus background in premium mode to avoid fullscreen 2D-plane perception.
                fixed3 dimmedBackground = lerp(background, background * (1.0 - saturate(_BackgroundDim)), focusMask);

                float crystalBlend = saturate(lerp(crystalAlpha, max(crystalAlpha, focusMask) * crystalAlpha, saturate(_CrystalDominance)));
                fixed3 premiumComposite = lerp(dimmedBackground, crystal.rgb, crystalBlend);

                // mode 0 = legacy alpha-over, mode 1 = premium crystal-dominant
                fixed3 legacyComposite = lerp(background, crystal.rgb, crystalAlpha);
                fixed3 composite = lerp(legacyComposite, premiumComposite, step(0.5, _CompositeMode));
                return fixed4(composite, clean.a);
            }
            ENDCG
        }
    }
}
