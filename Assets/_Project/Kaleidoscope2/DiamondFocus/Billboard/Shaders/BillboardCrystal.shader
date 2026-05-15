Shader "Kaleidoscope2/BillboardCrystal"
{
    Properties
    {
        _MainTex ("Source", 2D) = "black" {}
        _KaleidoscopeTex ("Kaleidoscope Texture", 2D) = "black" {}
        _OverlayAmount ("Overlay Amount", Range(0,1)) = 0
        _Intensity ("Intensity", Range(0,20)) = 8
        _Shape ("Shape", Float) = 0
        _MaterialMode ("Material Mode", Float) = 1
        _Rotation ("Rotation", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _KaleidoscopeTex;
            float _OverlayAmount;
            float _Intensity;
            float _Shape;
            float _MaterialMode;
            float4 _Rotation;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed3 source = tex2D(_MainTex, i.uv).rgb;
                float2 p = i.uv - 0.5;
                float angle = atan2(p.y, p.x) + _Rotation.z * 0.0174532925;
                float radius = length(p);
                float facets = _Shape < 2.5 ? 8.0 : 12.0;
                float edge = pow(saturate(1.0 - abs(frac(angle * facets * 0.15915494) - 0.5) * 2.0), 6.0);
                float mask = smoothstep(0.32, 0.16, radius) * saturate(0.7 + edge * 0.55);
                fixed3 crystal = tex2D(_KaleidoscopeTex, saturate(i.uv + normalize(p + 0.0001) * 0.012)).rgb;
                crystal += fixed3(0.72, 0.92, 1.0) * edge * saturate(_Intensity / 20.0);
                fixed3 composite = lerp(source, crystal, mask * saturate(_OverlayAmount));
                return fixed4(composite, 1.0);
            }
            ENDCG
        }
    }
}
