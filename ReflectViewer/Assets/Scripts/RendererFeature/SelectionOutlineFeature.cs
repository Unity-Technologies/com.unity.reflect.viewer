#if URP_AVAILABLE
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.Reflect.Viewer.UI
{
    public class SelectionOutlineFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class SelectionOutlineSettings
        {
            public bool IsEnabled = true;
            public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRendering;
            public LayerMask Filters;
            public Material MaterialToBlit;
        }

        // MUST be named "settings" (lowercase) to be shown in the Render Features inspector
        public SelectionOutlineSettings settings = new SelectionOutlineSettings();

        RenderTargetHandle m_RenderTextureHandle;
        SelectionOutlineRenderPass m_SelectionOutlineRenderPass;

        public override void Create()
        {
            m_SelectionOutlineRenderPass = new SelectionOutlineRenderPass(
                "Selection Outline pass",
                settings.WhenToInsert,
                settings.Filters,
                settings.MaterialToBlit
            );
        }

        // called every frame once per camera
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.IsEnabled)
            {
                // we can do nothing this frame if we want
                return;
            }

            m_SelectionOutlineRenderPass.source = renderer.cameraColorTarget;
            renderer.EnqueuePass(m_SelectionOutlineRenderPass);
        }
    }

    class SelectionOutlineRenderPass : ScriptableRenderPass
    {
        public enum PassId
        {
            SelectionPassVisible = 0,
            SelectionPassOutline,
            SelectionPassBlur
        };

        static readonly int s_BlurDirection = Shader.PropertyToID("_BlurDirection");

        // used to label this pass in Unity's Frame Debug utility
        string profilerTag;

        Material materialToBlit;

        FilteringSettings m_FilteringSettingsOpaque;
        FilteringSettings m_FilteringSettingsTransparent;

        RenderStateBlock m_RenderStateBlock;

        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

        RenderTargetHandle m_maskTexture;
        RenderTargetHandle m_blurTexture;
        public RenderTargetIdentifier source { get; set; }

        public SelectionOutlineRenderPass(string profilerTag,
            RenderPassEvent renderPassEvent, int layerMask, Material materialToBlit)
        {
            this.profilerTag = profilerTag;
            this.renderPassEvent = renderPassEvent;
            this.materialToBlit = materialToBlit;

            m_FilteringSettingsOpaque = new FilteringSettings(RenderQueueRange.opaque, layerMask);
            m_FilteringSettingsTransparent = new FilteringSettings(RenderQueueRange.transparent, layerMask);
            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));

            m_maskTexture.Init("_MaskTexture");
            m_blurTexture.Init("_BlurTexture");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // create a temporary render texture that matches the camera
            cmd.GetTemporaryRT(m_maskTexture.id, cameraTextureDescriptor);
            cmd.GetTemporaryRT(m_blurTexture.id, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // fetch a command buffer to use
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            cmd.Clear();

            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = materialToBlit;
            drawingSettings.overrideMaterialPassIndex = (int)PassId.SelectionPassVisible;

            CoreUtils.SetRenderTarget(cmd, m_maskTexture.Identifier(), ClearFlag.All, Color.clear);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            context.DrawRenderers(renderingData.cullResults,
                ref drawingSettings,
                ref m_FilteringSettingsOpaque,
                ref m_RenderStateBlock);

            context.DrawRenderers(renderingData.cullResults,
                ref drawingSettings,
                ref m_FilteringSettingsTransparent,
                ref m_RenderStateBlock);

            cmd.SetGlobalColor(s_BlurDirection, new Color(1, 0, 0, 0));
            cmd.Blit(m_maskTexture.id, m_blurTexture.id, materialToBlit, (int)PassId.SelectionPassBlur);

            cmd.SetGlobalColor(s_BlurDirection, new Color(0, 1, 0, 0));
            cmd.Blit(m_blurTexture.id, m_maskTexture.id, materialToBlit, (int)PassId.SelectionPassBlur);

            cmd.Blit(m_maskTexture.id, source, materialToBlit, (int)PassId.SelectionPassOutline);

            // Add the following code snippet after calling cmd.Blit
            // in 2020.2+URP, cmd.Blit has a bug to turn off stereo shader keyword. Enable keyword again manually.
            if(renderingData.cameraData.isStereoEnabled)
            {
                if (SystemInfo.supportsMultiview)
                    cmd.EnableShaderKeyword("STEREO_MULTIVIEW_ON");
                else
                    cmd.EnableShaderKeyword("STEREO_INSTANCING_ON");
            }
            else
            {
                cmd.DisableShaderKeyword("STEREO_MULTIVIEW_ON");
                cmd.DisableShaderKeyword("STEREO_INSTANCING_ON");
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_maskTexture.id);
            cmd.ReleaseTemporaryRT(m_blurTexture.id);
        }
    }
}
#endif
