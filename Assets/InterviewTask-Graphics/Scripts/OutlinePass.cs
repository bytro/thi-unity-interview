using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// NOTE: This uses the legacy ScriptableRenderPass.Execute path which is still
// fully supported in URP 17 (Unity 6). The newer Render Graph API
// (RecordRenderGraph) is worth discussing in the interview but is not required.
public class OutlinePass : ScriptableRenderPass
{
    private static readonly int OutlineColorId    = Shader.PropertyToID("_OutlineColor");
    private static readonly int ThicknessId       = Shader.PropertyToID("_Thickness");
    private static readonly int DepthThresholdId  = Shader.PropertyToID("_DepthThreshold");
    private static readonly int NormalThresholdId = Shader.PropertyToID("_NormalThreshold");

    private Material _material;
    private OutlineFeature.Settings _settings;

    ProfilingSampler m_blitProfilingSampler = new ProfilingSampler("OutlineBlit");
    RTHandle tempRT = null;

    public OutlinePass(OutlineFeature.Settings settings)
    {
        _settings = settings;

        // Tell URP to prepare _CameraDepthTexture and _CameraNormalsTexture before this pass runs.
        ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);

        // loading the Outline shader via Shader.Find and creating the material.
        // shader is always include in builds
        var shader = Shader.Find("Interview/OutlineEffect");
        if (shader != null)
        {
            _material = CoreUtils.CreateEngineMaterial(shader);
        }
        else
        {
            Debug.LogError("Could not find Outline shader");
        }
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_material == null)
            return;

        var cmd = CommandBufferPool.Get("Outline Effect");

        // Push current settings to the shader each frame so Inspector tweaks are live.
        _material.SetColor(OutlineColorId, _settings.outlineColor);
        _material.SetFloat(ThicknessId, _settings.thickness);
        _material.SetFloat(DepthThresholdId, _settings.depthThreshold);
        _material.SetFloat(NormalThresholdId, _settings.normalThreshold);

        // get a handle to the cameraColorTarget
        var colorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // blitting the color target through _material back to the same target
        // using a temporary RTHandle
        using (new ProfilingScope(cmd, m_blitProfilingSampler))
        {
            Blitter.BlitCameraTexture(cmd, colorTarget, tempRT);
            Blitter.BlitCameraTexture(cmd, tempRT, colorTarget, _material, 0);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // allocating a temporary RTHandle if needed
        var colorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        RenderingUtils.ReAllocateIfNeeded(ref tempRT, colorTarget.rt.descriptor);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // releasing temporary RTHandle
        if (tempRT != null)
        {
            tempRT.Release();
        }
    }

    public void Cleanup()
    {
        // allowing to get rid of the created material
        // the whole material creation and cleanup could
        // probably move over to the OutlineFeature
        CoreUtils.Destroy(_material);
    }
}
