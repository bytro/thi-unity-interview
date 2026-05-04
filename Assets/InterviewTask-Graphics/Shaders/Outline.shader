Shader "Interview/OutlineEffect"
{
    Properties
    {
        // _BlitTexture is bound automatically by Blitter — do not rename it.
        _OutlineColor    ("Outline Color",       Color)  = (0, 0, 0, 1)
        _Thickness       ("Thickness (px)",      Float)  = 1.0
        _DepthThreshold  ("Depth Threshold",     Float)  = 0.01
        _NormalThreshold ("Normal Threshold",    Float)  = 0.4
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        // Full-screen pass: no depth write, always draw on top.
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "OutlineEffect"

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Blit.hlsl defines: Vert, Varyings (with texcoord), and _BlitTexture / sampler_LinearClamp.
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Depth and normals textures prepared by ConfigureInput in OutlinePass.cs
            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D_X(_CameraNormalsTexture);
            SAMPLER(sampler_CameraNormalsTexture);

            float4 _OutlineColor;
            float  _Thickness;
            float  _DepthThreshold;
            float  _NormalThreshold;

            float SampleDepth(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
            }

            float3 SampleNormal(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv).rgb;
            }

            // TODO: Implement edge detection.
            //
            // A Roberts cross on depth is a solid starting point:
            //   sample the four diagonal neighbours (±offset in x and y),
            //   compute two diagonal differences, combine their magnitudes.
            //
            // Return 1.0 where an edge is detected, 0.0 elsewhere.
            //
            // Parameters:
            //   uv        - centre UV of the current pixel
            //   texelSize - size of one pixel in UV space (1 / resolution)
            float DetectEdge(float2 uv, float2 texelSize)
            {
                // TODO
                return 0.0;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                // Sample the original scene colour.
                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                // compute texelSize from _ScreenParams (xy = width, height in pixels)
                float2 texelSize = 1 / float2(_ScreenParams.x, _ScreenParams.y);
                // TODO: call DetectEdge and use the result to lerp between sceneColor and _OutlineColor

                return half4(1,0,0,1);
                return sceneColor;
            }
            ENDHLSL
        }
    }
}
