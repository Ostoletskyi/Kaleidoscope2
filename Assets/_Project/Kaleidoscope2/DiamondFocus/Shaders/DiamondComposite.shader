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
                fixed3 composite = lerp(background, crystal.rgb, saturate(crystal.a));
                return fixed4(composite, clean.a);
            }
            ENDCG
        }
    }
}
