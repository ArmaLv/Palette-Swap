Shader "Custom/PaletteSwap"
{
    Properties
    {
        _MainTex ("Day Sprite", 2D) = "white" {}
        _NightTex ("Night Sprite", 2D) = "white" {}
        _DayPalette ("Day Palette", 2D) = "white" {}
        _NightPalette ("Night Palette", 2D) = "white" {}
        _PaletteSize ("Palette Size (num colors)", Float) = 8
        _Threshold ("Match Threshold", Float) = 0.01
        _IsNight ("Is Night", Float) = 0
        [Toggle(_SPRITE_SWAP)] _SpriteSwap ("Sprite Swap", Float) = 0
        // Transition
        _TransitionMode ("Transition Mode", Int) = 0
        _RowSteps ("Row Steps", Float) = 8
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
            #pragma shader_feature _SPRITE_SWAP
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NightTex;
            sampler2D _DayPalette;
            sampler2D _NightPalette;
            float _PaletteSize;
            float _Threshold;
            float _IsNight;
            int   _TransitionMode;
            float _RowSteps;

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

            // Returns 0 (day) or 1 (night) per pixel — hard cut, no blending
            float TransitionWeight(float2 uv)
            {
                float threshold;

                if (_TransitionMode == 1) // Left to right
                    threshold = uv.x;
                else if (_TransitionMode == 2) // Right to left
                    threshold = 1.0 - uv.x;
                else if (_TransitionMode == 3) // Top to bottom
                    threshold = uv.y;
                else if (_TransitionMode == 4) // Bottom to top
                    threshold = 1.0 - uv.y;
                else if (_TransitionMode == 5) // Row by row (stepped)
                    threshold = floor(uv.y * _RowSteps) / (_RowSteps - 1.0);
                else if (_TransitionMode == 6) // Center horizontal
                    threshold = abs(uv.x - 0.5) * 2.0;
                else if (_TransitionMode == 7) // Center vertical
                    threshold = abs(uv.y - 0.5) * 2.0;
                else if (_TransitionMode == 8) // Center outward — normalized to corner dist
                    threshold = saturate(distance(uv, float2(0.5, 0.5)) / 0.7072);
                else // Blend (mode 0) — smooth simultaneous fade
                    return _IsNight;

                // Hard pixel-by-pixel step — this pixel has switched or it hasn't
                return step(threshold, _IsNight);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float weight = TransitionWeight(i.uv);

                fixed4 col;

                #ifdef _SPRITE_SWAP
                    // Sprite swap is also pixel-by-pixel in sync with the wipe
                    col = lerp(tex2D(_MainTex, i.uv),
                               tex2D(_NightTex, i.uv),
                               weight);
                #else
                    col = tex2D(_MainTex, i.uv);
                #endif

                if (col.a < 0.01) return col; // Skip transparent

                fixed4 result = col;
                bool matched = false;

                [unroll(MAX_PALETTE_SIZE)]
                for (int idx = 0; idx < MAX_PALETTE_SIZE; idx++)
                {
                    if (idx >= (int)_PaletteSize || matched) continue;

                    float u = (idx + 0.5) / _PaletteSize;
                    fixed4 dayColor = tex2D(_DayPalette, float2(u, 0.5));

                    if (distance(col.rgb, dayColor.rgb) < _Threshold)
                    {
                        fixed4 nightColor = tex2D(_NightPalette, float2(u, 0.5));
                        result = lerp(dayColor, nightColor, weight);
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