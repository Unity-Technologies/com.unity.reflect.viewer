using System;
using System.Collections.Generic;
using Unity.Reflect;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    [Serializable]
    public class LightFilterSettings
    {
        public bool enableLights;
    }

    public class LightFilterNode : ReflectNode<LightFilter>
    {
        public GameObjectInput gameObjectInput = new GameObjectInput();

        public LightFilterSettings settings;

        protected override LightFilter Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var p = new LightFilter(settings);

            gameObjectInput.streamEvent = p.OnStreamEvent;

            return p;
        }
    }

    public class LightFilter : IReflectNodeProcessor
    {
        readonly LightFilterSettings m_Settings;

        readonly List<Light> m_Lights;

        public LightFilter(LightFilterSettings settings)
        {
            m_Settings = settings;
            m_Lights = new List<Light>();
        }

        public void OnStreamEvent(SyncedData<GameObject> gameObject, StreamEvent streamEvent)
        {
            var lights = gameObject.data.GetComponentsInChildren<Light>(true);
            if (lights == null || lights.Length == 0)
                return;

            switch (streamEvent)
            {
                case StreamEvent.Added:
                    foreach (var light in lights)
                    {
                        light.enabled = m_Settings.enableLights;
                        m_Lights.Add(light);
                    }
                    break;
                case StreamEvent.Removed:
                    foreach (var light in lights)
                        m_Lights.Remove(light);
                    break;
            }
        }

        public void RefreshLights()
        {
            for (var i = m_Lights.Count - 1; i >= 0; --i)
            {
                if (m_Lights[i] == null)
                {
                    m_Lights.RemoveAt(i);
                    continue;
                }

                m_Lights[i].enabled = m_Settings.enableLights;
            }
        }

        public void OnPipelineInitialized()
        {
            // nothing to do here
        }

        public void OnPipelineShutdown()
        {
            m_Lights.Clear();
        }
    }
}
