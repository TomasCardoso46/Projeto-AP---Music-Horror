Shader "Custom/ScreenBlur"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Screen Blur"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BlitTexture);

            float4 _BlitTexture_TexelSize;

            float _BlurStrength;
            float2 _BlurDirection;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    GetFullScreenTriangleVertexPosition(input.vertexID);

                output.uv =
                    GetFullScreenTriangleTexCoord(input.vertexID);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 offset =
                    _BlitTexture_TexelSize.xy *
                    _BlurDirection *
                    _BlurStrength;

                half4 color = 0;

                color += SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.uv - offset * 4.0
                ) * 0.05;

                color += SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.uv - offset * 3.0
                ) * 0.09;

                color += SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.uv - offset * 2.0
                ) * 0.12;

                color += SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.uv - offset
                ) * 0.15;

                color += SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.uv
                ) * 0.18;

                color += SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.uv + offset
                ) * 0.15;

                color += SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.uv + offset * 2.0
                ) * 0.12;

                color += SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.uv + offset * 3.0
                ) * 0.09;

                color += SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.uv + offset * 4.0
                ) * 0.05;

                return color;
            }

            ENDHLSL
        }
    }
}