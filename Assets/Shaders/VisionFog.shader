Shader "GrayBox/VisionFog"
{
    Properties
    {
        _MainTex ("Fog Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Feather ("Feather (world units)", Range(0.01, 3)) = 0.4
        _Tile ("Texture Tiling", Vector) = (1, 1, 0, 0)

        [Header(Window Edge)]
        _EdgeColor ("Edge Color", Color) = (0.33, 0.26, 0.21, 1)
        _EdgeWidth ("Edge Width (world units)", Range(0, 0.5)) = 0.08

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
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float  _Feather;
            float4 _Tile;
            fixed4 _EdgeColor;
            float  _EdgeWidth;

            // 由 VisionFog 每帧推送：0 = 关闭描边，1 = 完全开启
            float _EdgeStrength;

            // 通关时的溶解：与污渍使用同一套扩散逻辑
            float4 _Seeds[4];
            float _Radii[4];
            float _DissolveSoftness;

            float4 _RectA;
            float4 _RectB;
            float4 _RectC;
            float4 _RectD;

            // 漏光缝：额外挖开的椭圆（center.xy, halfSize.xy），未使用的填成极远处
            float4 _Holes[4];

            // 漏光缝的羽化单独放大，让边缘慢慢融进裱纸，而不是切出一个口子
            float _HoleFeather;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            // 0 inside the rectangle, grows outwards; used to fade the fog back in
            float BoxDistance(float2 p, float4 rect)
            {
                float2 d = abs(p - rect.xy) - rect.zw;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            // 椭圆版：返回值近似"到边缘的世界距离"，内部为负
            float EllipseDistance(float2 p, float4 rect)
            {
                float2 halfSize = max(rect.zw, float2(0.001, 0.001));
                float2 d = (p - rect.xy) / halfSize;
                return (length(d) - 1.0) * min(halfSize.x, halfSize.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 两块视窗共享一次平滑过渡，重叠区域取最清晰的那个。
                // C/D 是分光缝展开后的副窗口，未展开时被填到极远处。
                float dWindow = min(
                    min(BoxDistance(i.worldPos.xy, _RectA), BoxDistance(i.worldPos.xy, _RectB)),
                    min(BoxDistance(i.worldPos.xy, _RectC), BoxDistance(i.worldPos.xy, _RectD)));

                float fogAlpha = smoothstep(0.0, max(_Feather, 0.001), dWindow);

                // 漏光缝按椭圆挖开，边缘用更大的羽化融进裱纸；
                // 只有洞内才透光，不做成矩形窗口。
                [unroll]
                for (int h = 0; h < 4; h++)
                {
                    float4 hole = _Holes[h];
                    if (hole.z > 0.002)
                    {
                        float holeDistance = EllipseDistance(i.worldPos.xy, hole);
                        float cut = 1.0 - smoothstep(0.0, max(_HoleFeather, 0.001), holeDistance);
                        fogAlpha *= (1.0 - cut);
                    }
                }

                fixed4 tex = tex2D(_MainTex, i.uv * _Tile.xy);
                fixed4 tint = _Color * i.color;

                float3 color = tex.rgb * tint.rgb;
                float alpha = tex.a * tint.a * fogAlpha;

                // 贴着洞口外侧的一条软边，用来在画布与背景同色时标出视窗范围。
                // dWindow 在矩形内部是负数，必须用 step 掐掉，否则整块视窗都会被涂成描边色。
                float stroke = step(0.0, dWindow) *
                               (1.0 - smoothstep(0.0, max(_EdgeWidth, 0.001), dWindow)) *
                               saturate(_EdgeStrength);

                color = lerp(color, _EdgeColor.rgb, stroke);
                alpha = max(alpha, stroke);

                // 多个起点各自向外扩散，取并集把遮蔽层擦掉
                float erase = 0.0;
                [unroll]
                for (int s = 0; s < 4; s++)
                {
                    float radius = _Radii[s];
                    if (radius <= 0.0) continue;

                    float dist = length(i.worldPos.xy - _Seeds[s].xy);
                    float inside = 1.0 - smoothstep(max(radius - _DissolveSoftness, 0.0), radius, dist);
                    erase = max(erase, inside);
                }

                return fixed4(color, alpha * (1.0 - erase));
            }
            ENDCG
        }
    }

    Fallback Off
}
