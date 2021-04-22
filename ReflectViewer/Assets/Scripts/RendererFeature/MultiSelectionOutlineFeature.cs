#if URP_AVAILABLE
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.Reflect.Viewer.UI
{
    public class MultiSelectionOutlineFeature : ScriptableRendererFeature
    {
        public struct SelectionOutlineData
        {
            public int colorId;
            public List<Renderer> renderers;
        }

        [System.Serializable]
        public class MultiSelectionOutlineSettings
        {
            public bool IsEnabled = true;
            public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRendering;
            public LayerMask Filters;
            public Material MaterialToBlit;
        }

        // MUST be named "settings" (lowercase) to be shown in the Render Features inspector
        public MultiSelectionOutlineSettings settings = new MultiSelectionOutlineSettings();

        public List<SelectionOutlineData> datas {
            get => m_MultiSelectionOutlineRenderPass.datas;
            set => m_MultiSelectionOutlineRenderPass.datas = value;
        }

        public int paletteId
        {
            get => m_MultiSelectionOutlineRenderPass.paletteId;
            set => m_MultiSelectionOutlineRenderPass.paletteId = value;
        }

        RenderTargetHandle m_RenderTextureHandle;
        MultiSelectionOutlineRenderPass m_MultiSelectionOutlineRenderPass;

        static readonly int s_ColorPalette = Shader.PropertyToID("_ColorPalette");

        public override void Create()
        {
            m_MultiSelectionOutlineRenderPass = new MultiSelectionOutlineRenderPass(
                "MulitiSelection Outline pass",
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

            m_MultiSelectionOutlineRenderPass.source = renderer.cameraColorTarget;
            renderer.EnqueuePass(m_MultiSelectionOutlineRenderPass);
        }

        public void ChangePalette(Texture2D palette)
        {
            settings.MaterialToBlit.SetTexture(s_ColorPalette, palette);
        }
    }

    class MultiSelectionOutlineRenderPass : ScriptableRenderPass
    {
        public enum PassId
        {
            SelectionPassMask = 0,
            SelectionPassOutline,
            SelectionPassBlur,
            SelectionPassFinal
        };

        static readonly int s_BlurDirection = Shader.PropertyToID("_BlurDirection");
        static readonly int s_Id = Shader.PropertyToID("_Id");
        static readonly int s_PaletteId = Shader.PropertyToID("_PaletteId");

        // used to label this pass in Unity's Frame Debug utility
        string profilerTag;

        Material materialToBlit;

        RenderTargetHandle m_maskTexture;
        RenderTargetHandle m_blurTexture;
        RenderTargetHandle m_finalTexture;

        bool isPaletteDirty;
        Texture2D colorPalette;

        public RenderTargetIdentifier source { get; set; }

        public List<MultiSelectionOutlineFeature.SelectionOutlineData> datas { get; set; }
        public int paletteId { get; set; }

        public MultiSelectionOutlineRenderPass(string profilerTag,
            RenderPassEvent renderPassEvent, int layerMask, Material materialToBlit)
        {
            this.profilerTag = profilerTag;
            this.renderPassEvent = renderPassEvent;
            this.materialToBlit = materialToBlit;

            m_maskTexture.Init("_MaskTexture");
            m_blurTexture.Init("_BlurTexture");
            m_finalTexture.Init("_FinalTexture");

            datas = new List<MultiSelectionOutlineFeature.SelectionOutlineData>();
            paletteId = 0;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // create a temporary render texture that matches the camera
            cmd.GetTemporaryRT(m_maskTexture.id, cameraTextureDescriptor);
            cmd.GetTemporaryRT(m_blurTexture.id, cameraTextureDescriptor);

            var tempRTD = cameraTextureDescriptor;
            tempRTD.colorFormat = RenderTextureFormat.ARGB32;
            cmd.GetTemporaryRT(m_finalTexture.id, tempRTD);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (datas.Count > 0)
            {
                // fetch a command buffer to use
                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
                cmd.Clear();

                CoreUtils.SetRenderTarget(cmd, m_finalTexture.Identifier(), ClearFlag.All, Color.clear);

                foreach (var data in datas)
                {
                    CoreUtils.SetRenderTarget(cmd, m_maskTexture.Identifier(), ClearFlag.All, Color.clear);

                    foreach (var renderer in data.renderers)
                    {
                        cmd.DrawRenderer(renderer, materialToBlit, 0, (int)PassId.SelectionPassMask);
                    }

                    cmd.SetGlobalColor(s_BlurDirection, new Color(1, 0, 0, 0));
                    cmd.Blit(m_maskTexture.id, m_blurTexture.id, materialToBlit, (int)PassId.SelectionPassBlur);
                    CheckStereoKeyword(ref renderingData, ref cmd);

                    cmd.SetGlobalColor(s_BlurDirection, new Color(0, 1, 0, 0));
                    cmd.Blit(m_blurTexture.id, m_maskTexture.id, materialToBlit, (int)PassId.SelectionPassBlur);
                    CheckStereoKeyword(ref renderingData, ref cmd);

                    cmd.SetGlobalFloat(s_Id, data.colorId);
                    cmd.SetGlobalFloat(s_PaletteId, paletteId);
                    cmd.Blit(m_maskTexture.id, m_finalTexture.id, materialToBlit, (int)PassId.SelectionPassOutline);
                    CheckStereoKeyword(ref renderingData, ref cmd);
                }

                cmd.Blit(m_finalTexture.id, source, materialToBlit, (int)PassId.SelectionPassFinal);
                CheckStereoKeyword(ref renderingData, ref cmd);
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                CommandBufferPool.Release(cmd);
            }
        }

        void CheckStereoKeyword(ref RenderingData renderingData, ref CommandBuffer cmd)
        {
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
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_maskTexture.id);
            cmd.ReleaseTemporaryRT(m_blurTexture.id);
            cmd.ReleaseTemporaryRT(m_finalTexture.id);
        }
    }
}
#endif
