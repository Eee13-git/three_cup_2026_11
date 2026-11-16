Shader "GrayBox/VisionFog"
{
    Properties
    {
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Feather ("Feather (world units)", Range(0.01, 3)) = 0.4
        _RectA ("Rect A (center.xy, halfSize.xy)", Vector) = (9999, 9999, 0.001, 0.001)
        _RectB ("Rect B (center.xy, halfSize.xy)", Vector) = (9999, 9999, 0.001, 0.001)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 color    : COLOR;
            };

            fixed4 _Color;
            float  _Feather;
            float4 _RectA;
            float4 _RectB;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color;
                return o;
            }

            // 0 inside the rectangle, grows outwards; used to fade the fog back in
            float BoxDistance(float2 p, float4 rect)
            {
                float2 d = abs(p - rect.xy) - rect.zw;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 两块视窗共享一次平滑过渡，重叠区域取最清晰的那个
                float d = min(BoxDistance(i.worldPos.xy, _RectA),
                              BoxDistance(i.worldPos.xy, _RectB));

                float alpha = smoothstep(0.0, max(_Feather, 0.001), d);
                fixed4 tint = _Color * i.color;
                return fixed4(tint.rgb, tint.a * alpha);
            }
            ENDCG
        }
    }

    Fallback Off
}
