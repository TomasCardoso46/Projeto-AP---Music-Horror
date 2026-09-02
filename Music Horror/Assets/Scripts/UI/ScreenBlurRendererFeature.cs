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

    private Material blurMaterial;
    private BlurPass blurPass;

    public override void Create()
    {
        Shader shader = Shader.Find("Custom/ScreenBlur");

        if (shader == null)
        {
            Debug.LogError(
                "ScreenBlurRendererFeature: Could not find " +
                "shader 'Custom/ScreenBlur'."
            );

            return;
        }

        blurMaterial = CoreUtils.CreateEngineMaterial(shader);

        blurPass = new BlurPass(blurMaterial);

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

        if (blurMaterial != null)
        {
            CoreUtils.Destroy(blurMaterial);
            blurMaterial = null;
        }
    }

    private class BlurPass : ScriptableRenderPass
    {
        private readonly Material material;

        private static readonly int BlurStrengthID =
            Shader.PropertyToID("_BlurStrength");

        private static readonly int BlurDirectionID =
            Shader.PropertyToID("_BlurDirection");

        private const string HorizontalPassName =
            "Screen Blur - Horizontal";

        private const string VerticalPassName =
            "Screen Blur - Vertical";

        public BlurPass(Material material)
        {
            this.material = material;

            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            if (material == null)
                return;

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

            material.SetFloat(
                BlurStrengthID,
                strength
            );

            material.SetVector(
                BlurDirectionID,
                new Vector2(1f, 0f)
            );

            var horizontalParameters =
                new RenderGraphUtils.BlitMaterialParameters(
                    source,
                    temporary,
                    material,
                    0
                );

            renderGraph.AddBlitPass(
                horizontalParameters,
                HorizontalPassName
            );

            material.SetVector(
                BlurDirectionID,
                new Vector2(0f, 1f)
            );

            var verticalParameters =
                new RenderGraphUtils.BlitMaterialParameters(
                    temporary,
                    source,
                    material,
                    0
                );

            renderGraph.AddBlitPass(
                verticalParameters,
                VerticalPassName
            );

            for (int i = 1; i < iterations; i++)
            {
                material.SetVector(
                    BlurDirectionID,
                    new Vector2(1f, 0f)
                );

                var extraHorizontal =
                    new RenderGraphUtils.BlitMaterialParameters(
                        source,
                        temporary,
                        material,
                        0
                    );

                renderGraph.AddBlitPass(
                    extraHorizontal,
                    $"Screen Blur - Horizontal {i + 1}"
                );

                material.SetVector(
                    BlurDirectionID,
                    new Vector2(0f, 1f)
                );

                var extraVertical =
                    new RenderGraphUtils.BlitMaterialParameters(
                        temporary,
                        source,
                        material,
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