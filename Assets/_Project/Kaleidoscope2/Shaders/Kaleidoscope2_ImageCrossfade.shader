Shader "Kaleidoscope2/ImageCrossfade"
{
    Properties
    {
        _MainTex ("Current", 2D) = "white" {}
        _NextTex ("Next", 2D) = "white" {}
        _Blend ("Blend", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NextTex;
            float _Blend;

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
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 currentColor = tex2D(_MainTex, i.uv);
                fixed4 nextColor = tex2D(_NextTex, i.uv);
                return lerp(currentColor, nextColor, saturate(_Blend));
            }
            ENDCG
        }
    }
}
