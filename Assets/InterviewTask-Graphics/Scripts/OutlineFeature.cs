using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Color outlineColor = Color.black;

        [Range(0f, 5f)]
        public float thickness = 1f;

        [Range(0f, 1f)]
        public float depthThreshold = 0.01f;

        [Range(0f, 1f)]
        public float normalThreshold = 0.4f;

        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public Settings settings = new Settings();

    private OutlinePass _pass;

    public override void Create()
    {
        // instantiate OutlinePass, passing settings to it
        // assign _pass.renderPassEvent from settings.renderPassEvent
        _pass = new OutlinePass(settings)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Skip preview cameras (e.g. material previews in the Inspector)
        if (renderingData.cameraData.cameraType == CameraType.Preview)
            return;

        // TODO: configure any per-frame pass state that depends on renderingData

        // enqueue _pass on the renderer
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(_pass);
    }
}
