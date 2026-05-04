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

            // Z buffer to linear depth
            //   taken from UnityCG.cginc
            //   makes setting the threshold a bit more convenient
            //   also adds a little overhead, so a similar calculation
            // should probably be done in OutlineFeatures.cs
            float LinearEyeDepth( float z )
            {
                return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
            }

            // A Roberts cross on depth is a solid starting point:
            //   sample the four diagonal neighbours (±offset in x and y),
            //   compute two diagonal differences, combine their magnitudes.
            //
            // It might make sense to use a different kernel like Sobel or
            // Sharr to get better results. But this would also increase the
            // amount of texture lookups.
            // https://en.wikipedia.org/wiki/Sobel_operator
            //
            // Returns 1.0 where an edge is detected, 0.0 elsewhere.
            //
            // Parameters:
            //   uv        - centre UV of the current pixel
            //   texelSize - size of one pixel in UV space (1 / resolution)
            float DetectEdge(float2 uv, float2 texelSize)
            {
                // convolution with roberts kernel (omitted 0 multiplications)
                // gx = 1  0  gy=  0 1
                //      0 -1      -1 0
                // original version with non-linear values
                // float gx = SampleDepth(uv) - SampleDepth(uv + (float2(1,1) * texelSize));
                // float gy = SampleDepth(uv + (float2(1,0) * texelSize)) - SampleDepth(uv + (float2(0,1) * texelSize));
                // linear version
                float gx = LinearEyeDepth(SampleDepth(uv)) - LinearEyeDepth(SampleDepth(uv + (float2(1,1) * texelSize)));
                float gy = LinearEyeDepth(SampleDepth(uv + (float2(1,0) * texelSize))) - LinearEyeDepth(SampleDepth(uv + (float2(0,1) * texelSize)));

                 // get gradient via Pythagoras
                float g = sqrt(gx * gx + gy * gy);

                // use threshold to determine if we have an edge or not
                // as an alternative smoothstep(_DepthThreshold - .2, _DepthThreshold + .2, g) can be used
                return step(_DepthThreshold, g);
            }

            // A Roberts cross on normals
            // Returns 1.0 where an edge is detected, 0.0 elsewhere.
            //
            // Parameters:
            //   uv        - centre UV of the current pixel
            //   texelSize - size of one pixel in UV space (1 / resolution)
            float DetectEdgeNormals(float2 uv, float2 texelSize)
            {
                // the normal of the current pixel
                float3 cn = SampleNormal(uv);

                // convolution with roberts kernel (omitted 0 multiplications)
                // gx = 1  0  gy=  0 1
                //      0 -1      -1 0
                float gx =
                    1 // dot(cn, cn) = 1
                    - dot(cn, SampleNormal(uv + (float2(1,1) * texelSize)));
                float gy =
                    dot(cn, SampleNormal(uv + (float2(1,0) * texelSize)))
                    - dot(cn, SampleNormal(uv + (float2(0,1) * texelSize)));

                float g = sqrt(gx * gx + gy * gy);

                // as an alternative smoothstep(_NormalThreshold - .2, _NormalThreshold + .2, g) can be used
                return step(_NormalThreshold, g);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                // Sample the original scene colour.
                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                // compute texelSize from _ScreenParams (xy = width, height in pixels)
                float2 texelSize = _Thickness / float2(_ScreenParams.x, _ScreenParams.y);

                // get edge from depth
                float depthEdge = DetectEdge(uv, texelSize);
                // get edge from normals
                float normalEdge = DetectEdgeNormals(uv, texelSize);

                // combining both results by using depth outlines where normal outlines don't work
                // e.g. if two flat surfaces cover up each other
                float edge = normalEdge + (depthEdge * (1 - normalEdge));

                // lerping between sceneColor and _OutlineColor
                sceneColor = lerp(sceneColor, _OutlineColor, edge);

                return sceneColor;
            }
            ENDHLSL
        }
    }
}
