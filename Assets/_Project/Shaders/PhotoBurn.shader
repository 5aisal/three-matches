// Photo burn dissolve (GAME_SPEC §24): a noise mask eats the photo from the bottom up. Just behind the front the
// paper glows white -> orange -> deep red (HDR so bloom picks it up), then chars to black before it is gone.
// _Burn 0..1 is driven per renderer from MemoryFrame.
Shader "ThreeMatches/PhotoBurn"
{
    Properties
    {
        [MainTexture] _BaseMap ("Photo", 2D) = "white" {}
        [MainColor] _BaseColor ("Photo colour", Color) = (1, 1, 1, 1)
        _Burn ("Burn amount", Range(0, 1)) = 0
        _EdgeWidth ("Glowing edge width", Range(0.02, 0.5)) = 0.14
        _NoiseScale ("Noise scale", Range(1, 30)) = 9
        _BottomBias ("Burn starts at the bottom", Range(0, 1)) = 0.35
        _EmissionStrength ("Edge emission (HDR)", Range(0, 12)) = 4
        _CharColor ("Char colour", Color) = (0.02, 0.015, 0.012, 1)
        [HDR] _EdgeWhite ("Edge hot colour", Color) = (1, 0.95, 0.75, 1)
        [HDR] _EdgeOrange ("Edge mid colour", Color) = (1, 0.42, 0.05, 1)
        [HDR] _EdgeRed ("Edge cool colour", Color) = (0.45, 0.05, 0.01, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "PhotoBurn"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Burn;
                half _EdgeWidth;
                half _NoiseScale;
                half _BottomBias;
                half _EmissionStrength;
                half4 _CharColor;
                half4 _EdgeWhite;
                half4 _EdgeOrange;
                half4 _EdgeRed;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash(i);
                float b = Hash(i + float2(1, 0));
                float c = Hash(i + float2(0, 1));
                float d = Hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float Fbm(float2 p)
            {
                float v = 0.0;
                float amp = 0.5;
                for (int o = 0; o < 4; o++)
                {
                    v += ValueNoise(p) * amp;
                    p *= 2.03;
                    amp *= 0.5;
                }
                return v; // 0..~0.94
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 photo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap)) * _BaseColor;

                float n = Fbm(input.uv * _NoiseScale);
                n = lerp(n, n * 0.6 + input.uv.y * 0.4, _BottomBias);
                // Burn 0 leaves everything intact (no char, no edge); burn 1 removes everything.
                float threshold = lerp(-_EdgeWidth * 1.7, 1.15, _Burn);
                float m = n - threshold;
                clip(m);

                float e = saturate(m / _EdgeWidth);
                float charAmount = 1.0 - smoothstep(_EdgeWidth, _EdgeWidth * 1.7, m);
                half3 baseColor = lerp(photo.rgb, _CharColor.rgb, charAmount);

                half3 ramp = lerp(_EdgeWhite.rgb, _EdgeOrange.rgb, smoothstep(0.0, 0.35, e));
                ramp = lerp(ramp, _EdgeRed.rgb, smoothstep(0.35, 1.0, e));
                half glow = pow(1.0 - e, 1.3) * step(0.0001, _Burn);
                half3 color = baseColor + ramp * (glow * _EmissionStrength);

                return half4(color, photo.a * smoothstep(0.0, 0.02, m));
            }
            ENDHLSL
        }
    }
}
