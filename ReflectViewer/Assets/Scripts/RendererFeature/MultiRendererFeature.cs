#if URP_AVAILABLE
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Reflect.Viewer
{
    public class MultiRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>();

        public override void Create()
        {
            foreach (var rendererFeature in m_RendererFeatures)
                rendererFeature.Create();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            foreach (var rendererFeature in m_RendererFeatures)
                rendererFeature.AddRenderPasses(renderer, ref renderingData);
        }
    }
}
#endif
