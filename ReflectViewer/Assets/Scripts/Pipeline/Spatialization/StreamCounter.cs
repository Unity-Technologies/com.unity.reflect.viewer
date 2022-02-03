using System;
using Unity.Reflect;
using UnityEngine.Events;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer.Pipeline
{

    [Serializable]
    public class StreamCounterSettings
    {
        [Serializable]
        public class StringEvent : UnityEvent<string> { }

        [Header("Events")]
        public StringEvent onAddedCountModified;
        public StringEvent onChangedCountModified;
        public StringEvent onRemovedCountModified;

        public void OnAddedCountModified(int count)
        {
            onAddedCountModified.Invoke(count.ToString());
        }

        public void OnChangedCountModified(int count)
        {
            onChangedCountModified.Invoke(count.ToString());
        }

        public void OnRemovedCountModified(int count)
        {
            onRemovedCountModified.Invoke(count.ToString());
        }
    }

    [Serializable]
    public abstract class CounterNode<T> : ReflectNode<StreamCounter>
    {
        protected abstract DataInput<T> GetInput();

        public StreamCounterSettings settings = new StreamCounterSettings();

        protected override StreamCounter Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var node = new StreamCounter(settings);

            var input = GetInput();
            input.streamBegin = node.OnBegin;
            input.streamEvent = (stream, eventType) => node.OnStreamEvent(eventType);

            return node;
        }
    }

    [Serializable]
    public class StreamAssetCounterNode : CounterNode<StreamAsset>
    {
        public StreamAssetInput input = new StreamAssetInput();

        protected override DataInput<StreamAsset> GetInput() { return input; }
    }

    [Serializable]
    public class StreamInstanceCounterNode : CounterNode<StreamInstance>
    {
        public StreamInstanceInput input = new StreamInstanceInput();

        protected override DataInput<StreamInstance> GetInput() { return input; }
    }

    [Serializable]
         public class GameObjectCounterNode : CounterNode<GameObject>
         {
             public GameObjectInput input = new GameObjectInput();

             protected override DataInput<GameObject> GetInput() { return input; }
         }

    public class StreamCounter : IReflectNodeProcessor
    {
        readonly StreamCounterSettings m_Settings;

        int m_AddedCount;
        int m_ChangedCount;
        int m_RemovedCount;

        public StreamCounter(StreamCounterSettings settings)
        {
            m_Settings = settings;
        }

        public void OnBegin()
        {
            ResetCounts();
        }

        public void OnStreamEvent(StreamEvent streamEvent)
        {
            switch (streamEvent)
            {
                case StreamEvent.Added :
                    ++m_AddedCount;
                    m_Settings.OnAddedCountModified(m_AddedCount);
                    break;

                case StreamEvent.Changed :
                    ++m_ChangedCount;
                    m_Settings.OnChangedCountModified(m_ChangedCount);
                    break;

                case StreamEvent.Removed :
                    ++m_RemovedCount;
                    m_Settings.OnRemovedCountModified(m_RemovedCount);
                    break;
            }
        }

        public void OnPipelineInitialized()
        {
            ResetCounts();
        }

        public void OnPipelineShutdown()
        {
        }

        void ResetCounts()
        {
            m_Settings.OnAddedCountModified(m_AddedCount = 0);
            m_Settings.OnChangedCountModified(m_ChangedCount = 0);
            m_Settings.OnRemovedCountModified(m_RemovedCount = 0);
        }
    }
}
