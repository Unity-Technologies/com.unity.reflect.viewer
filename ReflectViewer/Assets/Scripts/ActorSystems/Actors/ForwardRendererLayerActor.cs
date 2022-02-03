using System;
using System.Collections.Generic;
using Unity.Reflect.ActorFramework;
using UnityEngine;
#if URP_AVAILABLE
using UnityEngine.Rendering.Universal;
#endif

namespace Unity.Reflect.Viewer.Actors
{
    [Actor("f46b7557-8f60-4adc-a632-efab9cd84d21", true)]
    public class ForwardRendererLayerActor
    {
        Settings m_Settings;

#if URP_AVAILABLE
        readonly Dictionary<ForwardRendererData, LayerMask> m_OpaqueLayerMasks = new Dictionary<ForwardRendererData, LayerMask>();
        readonly Dictionary<ForwardRendererData, LayerMask> m_TransparentLayerMasks = new Dictionary<ForwardRendererData, LayerMask>();
#endif

        public void Inject()
        {
#if URP_AVAILABLE
            foreach (var data in m_Settings.forwardRendererDatas)
            {
                m_OpaqueLayerMasks[data] = data.opaqueLayerMask;
                m_TransparentLayerMasks[data] = data.transparentLayerMask;

                var layerMask = m_Settings.disabledLayers;
                data.opaqueLayerMask &= ~layerMask;
                data.transparentLayerMask &= ~layerMask;
            }
#endif
        }

        public void Shutdown()
        {
#if URP_AVAILABLE
            foreach (var data in m_Settings.forwardRendererDatas)
            {
                data.opaqueLayerMask = m_OpaqueLayerMasks[data];
                data.transparentLayerMask = m_TransparentLayerMasks[data];
            }
#endif
        }

        public class Settings : ActorSettings
        {
#if URP_AVAILABLE
            public List<ForwardRendererData> forwardRendererDatas;
#endif
            public LayerMask disabledLayers;

            public Settings()
                : base(Guid.NewGuid().ToString()) { }
        }
    }
}
