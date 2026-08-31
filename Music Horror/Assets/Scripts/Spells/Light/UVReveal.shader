Shader "NailInTheCoffin/UV Reveal"
{
    Properties
    {
        [MainTexture]
        _BaseMap ("Hidden Texture", 2D) = "white" {}

        [MainColor]
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _UVRevealColor ("UV Reveal Color", Color) = (1,1,1,1)

        _UVRevealIntensity ("UV Reveal Intensity", Float) = 3

        _UVRevealStrength ("UV Reveal Strength", Range(0,1)) = 0

        _UVEdgeSoftness ("UV Edge Softness", Range(0.01,1)) = 0.2

        _UVRevealMultiplier ("UV Reveal Multiplier", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "UVReveal"

            Tags
            {
                "LightMode"="UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha

            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseMap_ST;

                float4 _BaseColor;

                float4 _UVRevealColor;

                float _UVRevealIntensity;

                float _UVRevealStrength;

                float _UVEdgeSoftness;

                float _UVRevealMultiplier;

            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionHCS = positionInputs.positionCS;

                output.uv =
                    TRANSFORM_TEX(
                        input.uv,
                        _BaseMap
                    );

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 textureColor =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        input.uv
                    );

                float reveal =
                    saturate(_UVRevealStrength);

                // Hidden when outside the UV beam.
                float alpha =
                    textureColor.a *
                    _BaseColor.a *
                    reveal;

                // UV glow.
                float3 emission =
                    textureColor.rgb *
                    _BaseColor.rgb *
                    _UVRevealColor.rgb *
                    _UVRevealIntensity *
                    reveal;

                return half4(
                    emission,
                    alpha
                );
            }

            ENDHLSL
        }
    }

    FallBack Off
}