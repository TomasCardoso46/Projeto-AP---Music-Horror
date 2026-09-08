using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class ScreenBlurRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Header("Blur")]
        [Range(0f, 10f)]
        public float blurStrength = 3f;

        [Range(1, 8)]
        public int iterations = 2;

        [Header("Injection")]
        public RenderPassEvent injectionPoint =
            RenderPassEvent.AfterRenderingTransparents;
    }

    [SerializeField]
    private Settings settings = new Settings();

    [Header("Shader")]
    [SerializeField]
    private Shader blurShader;

    private Material horizontalMaterial;
    private Material verticalMaterial;
    private BlurPass blurPass;

    public override void Create()
    {
        if (blurShader == null)
        {
            Debug.LogError(
                "ScreenBlurRendererFeature: Blur Shader has not been assigned."
            );

            return;
        }

        horizontalMaterial = CoreUtils.CreateEngineMaterial(blurShader);
        verticalMaterial = CoreUtils.CreateEngineMaterial(blurShader);

        if (horizontalMaterial == null || verticalMaterial == null)
        {
            Debug.LogError(
                "ScreenBlurRendererFeature: Failed to create blur materials."
            );

            return;
        }

        blurPass = new BlurPass(
            horizontalMaterial,
            verticalMaterial
        );

        blurPass.renderPassEvent = settings.injectionPoint;
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (blurPass == null)
            return;

        if (!ScreenBlurSettings.Enabled)
            return;

        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        renderer.EnqueuePass(blurPass);
    }

    protected override void Dispose(bool disposing)
    {
        blurPass?.Dispose();
        blurPass = null;

        if (horizontalMaterial != null)
        {
            CoreUtils.Destroy(horizontalMaterial);
            horizontalMaterial = null;
        }

        if (verticalMaterial != null)
        {
            CoreUtils.Destroy(verticalMaterial);
            verticalMaterial = null;
        }
    }

    private class BlurPass : ScriptableRenderPass
    {
        private readonly Material horizontalMaterial;
        private readonly Material verticalMaterial;

        private static readonly int BlurStrengthID =
            Shader.PropertyToID("_BlurStrength");

        private static readonly int BlurDirectionID =
            Shader.PropertyToID("_BlurDirection");

        private const string HorizontalPassName =
            "Screen Blur - Horizontal";

        private const string VerticalPassName =
            "Screen Blur - Vertical";

        public BlurPass(
            Material horizontalMaterial,
            Material verticalMaterial)
        {
            this.horizontalMaterial = horizontalMaterial;
            this.verticalMaterial = verticalMaterial;

            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            if (horizontalMaterial == null ||
                verticalMaterial == null)
            {
                return;
            }

            if (!ScreenBlurSettings.Enabled)
                return;

            UniversalResourceData resourceData =
                frameData.Get<UniversalResourceData>();

            TextureHandle source =
                resourceData.activeColorTexture;

            if (!source.IsValid())
                return;

            UniversalCameraData cameraData =
                frameData.Get<UniversalCameraData>();

            var descriptor =
                cameraData.cameraTargetDescriptor;

            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            TextureHandle temporary =
                UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    descriptor,
                    "_ScreenBlurTemporary",
                    false
                );

            float strength =
                ScreenBlurSettings.Strength;

            int iterations =
                Mathf.Max(
                    1,
                    ScreenBlurSettings.Iterations
                );

            horizontalMaterial.SetFloat(
                BlurStrengthID,
                strength
            );

            horizontalMaterial.SetVector(
                BlurDirectionID,
                new Vector2(1f, 0f)
            );

            verticalMaterial.SetFloat(
                BlurStrengthID,
                strength
            );

            verticalMaterial.SetVector(
                BlurDirectionID,
                new Vector2(0f, 1f)
            );

            var horizontalParameters =
                new RenderGraphUtils.BlitMaterialParameters(
                    source,
                    temporary,
                    horizontalMaterial,
                    0
                );

            renderGraph.AddBlitPass(
                horizontalParameters,
                HorizontalPassName
            );

            var verticalParameters =
                new RenderGraphUtils.BlitMaterialParameters(
                    temporary,
                    source,
                    verticalMaterial,
                    0
                );

            renderGraph.AddBlitPass(
                verticalParameters,
                VerticalPassName
            );

            for (int i = 1; i < iterations; i++)
            {
                var extraHorizontal =
                    new RenderGraphUtils.BlitMaterialParameters(
                        source,
                        temporary,
                        horizontalMaterial,
                        0
                    );

                renderGraph.AddBlitPass(
                    extraHorizontal,
                    $"Screen Blur - Horizontal {i + 1}"
                );

                var extraVertical =
                    new RenderGraphUtils.BlitMaterialParameters(
                        temporary,
                        source,
                        verticalMaterial,
                        0
                    );

                renderGraph.AddBlitPass(
                    extraVertical,
                    $"Screen Blur - Vertical {i + 1}"
                );
            }
        }

        public void Dispose()
        {
        }
    }
}