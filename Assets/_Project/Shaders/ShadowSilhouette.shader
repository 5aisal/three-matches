// Flat, unlit, alpha-blended colour for Shadow-Rafi (GAME_SPEC §19). Two tricks make it read as a cast shadow:
//  - the mesh is squashed toward a wall plane in world space (a scale on the wall's normal axis). It is done
//    here, not with a transform scale, because a non-uniform scale above a Humanoid Animator breaks the rig;
//  - a stencil lets each screen pixel be drawn once, so limbs that overlap in the flattened silhouette do not
//    double-darken.
Shader "ThreeMatches/ShadowSilhouette"
{
    Properties
    {
        [MainColor] _BaseColor ("Colour (alpha = opacity)", Color) = (0, 0, 0, 0.85)
        _FlattenPlaneZ ("Wall plane (world z)", Float) = 0
        _Flatten ("Scale on the wall normal", Range(0, 1)) = 0.02
        _StencilRef ("Stencil reference", Int) = 17
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Silhouette"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            Stencil
            {
                Ref [_StencilRef]
                Comp NotEqual
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _FlattenPlaneZ;
                float _Flatten;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                positionWS.z = _FlattenPlaneZ + (positionWS.z - _FlattenPlaneZ) * _Flatten;
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}
