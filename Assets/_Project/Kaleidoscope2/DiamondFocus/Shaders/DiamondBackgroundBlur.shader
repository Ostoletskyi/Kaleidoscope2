Shader "Kaleidoscope2/DiamondBackgroundBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _InputTexelSize ("Input Texel Size", Vector) = (0.001, 0.001, 1024, 1024)
        _BlurDirection ("Blur Direction", Vector) = (1, 0, 0, 0)
        _BlurRadius ("Blur Radius", Float) = 0
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
            float4 _InputTexelSize;
            float4 _BlurDirection;
            float _BlurRadius;

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
                float2 direction = normalize(_BlurDirection.xy + 0.00001);
                float2 stepUv = direction * _InputTexelSize.xy * max(0.0, _BlurRadius);

                fixed4 color = tex2D(_MainTex, i.uv) * 0.22702703;
                color += tex2D(_MainTex, i.uv + stepUv * 1.38461538) * 0.31621622;
                color += tex2D(_MainTex, i.uv - stepUv * 1.38461538) * 0.31621622;
                color += tex2D(_MainTex, i.uv + stepUv * 3.23076923) * 0.07027027;
                color += tex2D(_MainTex, i.uv - stepUv * 3.23076923) * 0.07027027;
                return color;
            }
            ENDCG
        }
    }
}
