Shader "Kaleidoscope2/CrystalStage3D/PremiumOpticalBackground"
{
    Properties
    {
        _MainTex ("Selected Kaleidoscope Texture", 2D) = "white" {}
        _ViewAspect ("View Aspect", Float) = 1.7777
        _TextureAspect ("Texture Aspect", Float) = 1.7777
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 100
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ViewAspect;
            float _TextureAspect;

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

            float2 CoverUv(float2 uv)
            {
                float viewAspect = max(0.0001, _ViewAspect);
                float textureAspect = max(0.0001, _TextureAspect);
                if (textureAspect > viewAspect)
                {
                    float scaleX = viewAspect / textureAspect;
                    uv.x = (uv.x - 0.5) * scaleX + 0.5;
                }
                else
                {
                    float scaleY = textureAspect / viewAspect;
                    uv.y = (uv.y - 0.5) * scaleY + 0.5;
                }

                return saturate(uv);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = saturate(i.uv);
                float2 sourceUv = CoverUv(uv);

                fixed3 source = tex2D(_MainTex, sourceUv).rgb;
                source = max(source, fixed3(0.035, 0.035, 0.035));

                return fixed4(saturate(source), 1.0);
            }
            ENDCG
        }
    }
    FallBack "Unlit/Texture"
}
