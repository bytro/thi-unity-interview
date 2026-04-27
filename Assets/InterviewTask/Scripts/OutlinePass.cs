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

    public OutlinePass(OutlineFeature.Settings settings)
    {
        _settings = settings;

        // Tell URP to prepare _CameraDepthTexture and _CameraNormalsTexture before this pass runs.
        ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);

        // TODO: load the Outline shader from Resources or via Shader.Find and create the material.
        //       Prefer loading from a known path so this works in builds, not just the Editor.
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_material == null)
            return;

        var cmd = CommandBufferPool.Get("Outline Effect");

        // Push current settings to the shader each frame so Inspector tweaks are live.
        _material.SetColor(OutlineColorId,    _settings.outlineColor);
        _material.SetFloat(ThicknessId,       _settings.thickness);
        _material.SetFloat(DepthThresholdId,  _settings.depthThreshold);
        _material.SetFloat(NormalThresholdId, _settings.normalThreshold);

        // TODO: obtain the camera color target handle from renderingData.cameraData.renderer
        // TODO: blit the color target through _material back to the same target
        //       Hint: Blitter.BlitCameraTexture(cmd, source, destination, _material, 0)
        //       Note: you may need a temporary RTHandle as an intermediate buffer to avoid
        //             reading and writing the same texture simultaneously.

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // TODO: release any temporary RTHandles allocated during Execute
    }
}
