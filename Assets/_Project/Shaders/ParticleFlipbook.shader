// Unlit URP particle shader for flipbook sheets (texture-sheet animation supplies the frame UVs).
// Blend is set per material (_SrcBlend/_DstBlend): additive flames/embers, alpha-blended smoke.
// _DecodeFireMask handles the CGHEVEN ground-fire sheet: its RGB is flat (yellow = fire, white = smoke) and the
// real density lives in a very low alpha, so alpha is boosted and the colour is rebuilt from a fire ramp.
Shader "ThreeMatches/ParticleFlipbook"
{
    Properties
    {
        [MainTexture] _BaseMap ("Flipbook", 2D) = "white" {}
        [HDR] _Tint ("Tint (alpha = opacity)", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity (HDR multiplier)", Range(0, 8)) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst blend", Float) = 1
        [Toggle] _DecodeFireMask ("Decode CGHEVEN fire mask", Float) = 0
        _AlphaGain ("Decode: alpha gain", Range(0, 12)) = 3.5
        [HDR] _FireCool ("Decode: fire edge colour", Color) = (0.9, 0.12, 0.02, 1)
        [HDR] _FireHot ("Decode: fire core colour", Color) = (1, 0.72, 0.22, 1)
        _SmokeColor ("Decode: smoke colour", Color) = (0.1, 0.085, 0.08, 1)
        _SmokeGain ("Decode: smoke gain", Range(0, 4)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Particle"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
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
                half4 _Tint;
                half _Intensity;
                half _DecodeFireMask;
                half _AlphaGain;
                half4 _FireCool;
                half4 _FireHot;
                half4 _SmokeColor;
                half _SmokeGain;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                if (_DecodeFireMask > 0.5h)
                {
                    half density = saturate(tex.a * _AlphaGain);
                    half fireMask = 1.0h - smoothstep(0.3h, 0.7h, tex.b);
                    half3 fire = lerp(_FireCool.rgb, _FireHot.rgb, saturate(density * 1.4h - 0.1h)) * (density * _Intensity);
                    half smokeA = saturate(tex.a * _SmokeGain * 2.0h) * (1.0h - fireMask);
                    // Premultiplied: fire adds light (alpha 0), smoke occludes. Vertex colour fades both.
                    half3 rgb = fire * fireMask + _SmokeColor.rgb * smokeA;
                    half a = smokeA;
                    return half4(rgb * input.color.a * _Tint.rgb * input.color.rgb, a * input.color.a * _Tint.a);
                }

                half3 rgb = tex.rgb * _Tint.rgb * input.color.rgb * _Intensity;
                half a = tex.a * _Tint.a * input.color.a;
                return half4(rgb, a);
            }
            ENDHLSL
        }
    }
}
