Shader "GrayBox/StainDissolve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Softness ("Softness", Range(0.01, 1)) = 0.22
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
            float _Softness;

            // 由 StainDissolve 每帧推送：最多 4 个扩散起点(世界坐标)与各自的当前半径
            float4 _Seeds[4];
            float _Radii[4];

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 tint = _Color * i.color;

                float3 color = tex.rgb * tint.rgb;
                float alpha = tex.a * tint.a;

                // 四个起点各自的擦除圆取并集，边缘用 smoothstep 软化
                float erase = 0.0;
                [unroll]
                for (int s = 0; s < 4; s++)
                {
                    float radius = _Radii[s];
                    if (radius <= 0.0) continue;

                    float dist = length(i.worldPos.xy - _Seeds[s].xy);
                    float inside = 1.0 - smoothstep(max(radius - _Softness, 0.0), radius, dist);
                    erase = max(erase, inside);
                }

                return fixed4(color, alpha * (1.0 - erase));
            }
            ENDCG
        }
    }

    Fallback Off
}
