Shader "Custom/PaletteSwap"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _DayPalette ("Day Palette", 2D) = "white" {}
        _NightPalette ("Night Palette", 2D) = "white" {}
        _PaletteSize ("Palette Size (num colors)", Float) = 8
        _Threshold ("Match Threshold", Float) = 0.01
        _IsNight ("Is Night", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _DayPalette;
            sampler2D _NightPalette;
            float _PaletteSize;
            float _Threshold;
            float _IsNight;

            // !! Change this to match your largest palette !!
            #define MAX_PALETTE_SIZE 32

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a < 0.01) return col; // skip transparent pixels

                fixed4 result = col; // fallback: original color
                bool matched = false;

                [unroll(MAX_PALETTE_SIZE)]
                for (int idx = 0; idx < MAX_PALETTE_SIZE; idx++)
                {
                    // Skip slots beyond actual palette size
                    if (idx >= (int)_PaletteSize || matched) continue;

                    float u = (idx + 0.5) / _PaletteSize;
                    fixed4 dayColor = tex2D(_DayPalette, float2(u, 0.5));

                    if (distance(col.rgb, dayColor.rgb) < _Threshold)
                    {
                        fixed4 nightColor = tex2D(_NightPalette, float2(u, 0.5));
                        result = lerp(dayColor, nightColor, _IsNight);
                        result.a = col.a;
                        matched = true;
                    }
                }

                return result;
            }
            ENDCG
        }
    }
}