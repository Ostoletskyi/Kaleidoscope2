Shader "Kaleidoscope2/Menu/AtmosphereOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _MenuTime ("Menu Time", Float) = 0
        _EffectMode ("Effect Mode", Float) = 0
        _NarrowBandColor ("Narrow Band Color", Color) = (0.58, 0.95, 1.0, 1)
        _WideBandColor ("Wide Band Color", Color) = (1.0, 0.72, 0.32, 1)
        _NarrowBandParams ("Narrow Band Params", Vector) = (0.018, 0.28, 0.075, 0.032)
        _WideBandParams ("Wide Band Params", Vector) = (0.010, 0.13, 0.18, 0.15)
        _BandDirection ("Band Directions", Vector) = (0.82, 0.57, 0.62, 0.78)
        _AtmosphereParams ("Atmosphere Params", Vector) = (0.12, 0.022, 0.45, 1.0)
        _CausticParams ("Caustic Params", Vector) = (0.018, 0.011, 0, 0)
        _ShimmerParams ("Shimmer Params", Vector) = (0.65, 0.7, 0.45, 0)
        _ShimmerTintA ("Shimmer Cool Tint", Color) = (0.55, 0.95, 1.0, 1)
        _ShimmerTintB ("Shimmer Warm Tint", Color) = (1.0, 0.72, 0.34, 1)

        [HideInInspector] _SrcBlend ("Source Blend", Float) = 5
        [HideInInspector] _DstBlend ("Destination Blend", Float) = 1
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend [_SrcBlend] [_DstBlend]
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _MenuTime;
            float _EffectMode;
            fixed4 _NarrowBandColor;
            fixed4 _WideBandColor;
            float4 _NarrowBandParams;
            float4 _WideBandParams;
            float4 _BandDirection;
            float4 _AtmosphereParams;
            float4 _CausticParams;
            float4 _ShimmerParams;
            fixed4 _ShimmerTintA;
            fixed4 _ShimmerTintB;
            float4 _ClipRect;

            #include "MenuLightBands.hlsl"
            #include "MenuCrystalShimmer.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 result;

                if (_EffectMode < 0.5)
                {
                    fixed4 tex = tex2D(_MainTex, i.texcoord);
                    result = KaelisMenuAtmosphereFragment(i.texcoord, _MenuTime);
                    result.rgb *= tex.rgb * i.color.rgb;
                    result.a *= tex.a * i.color.a;
                }
                else if (_EffectMode < 1.5)
                {
                    fixed4 tex = tex2D(_MainTex, i.texcoord);
                    result = KaelisMenuCrystalShimmerFragment(i.texcoord, _MenuTime);
                    result.rgb *= tex.rgb * i.color.rgb;
                    result.a *= tex.a * i.color.a;
                }
                else
                {
                    result = KaelisMenuDustFragment(i.texcoord, i.color);
                }

                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                return result;
            }
            ENDCG
        }
    }
}
