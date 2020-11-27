using System;
using Unity.Reflect;
using UnityEngine.Events;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    public struct StreamCountData : IEquatable<StreamCountData>
    {
        public int addedCount;
        public int changedCount;
        public int removedCount;

        public static bool operator ==(StreamCountData a, StreamCountData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(StreamCountData a, StreamCountData b)
        {
            return !(a == b);
        }

        public bool Equals(StreamCountData other)
        {
            return addedCount == other.addedCount &&
                changedCount == other.changedCount &&
                removedCount == other.removedCount;
        }

        public override bool Equals(object obj)
        {
            return obj is StreamCountData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = addedCount;
                hashCode = (hashCode * 397) ^ changedCount;
                hashCode = (hashCode * 397) ^ removedCount;
                return hashCode;
            }
        }
    }

    [Serializable]
    public class StreamIndicatorSettings
    {
        [Serializable]
        public class IntEvent : UnityEvent<int> { }

        [Serializable]
        public class ProgressEvent : UnityEvent<int, int> { }

        [Serializable]
        public class StreamCounterEvent : UnityEvent<StreamCountData> { }

        [Header("Events")]
        public UnityEvent assetStreamBegin;
        public UnityEvent assetStreamEnd;
        public UnityEvent instanceStreamBegin;
        public UnityEvent instanceStreamEnd;
        public UnityEvent instanceDataStreamBegin;
        public UnityEvent instanceDataStreamEnd;
        public UnityEvent gameObjectStreamBegin;
        public ProgressEvent gameObjectStreamEvent;
        public UnityEvent gameObjectStreamEnd;

        public StreamCounterEvent assetCountModified;
        public StreamCounterEvent instanceCountModified;
        public StreamCounterEvent gameObjectCountModified;
    }

    public class StreamIndicatorNode : ReflectNode<StreamIndicator>
    {
        public StreamAssetInput streamAssetInput = new StreamAssetInput();
        public StreamInstanceInput streamInstanceInput = new StreamInstanceInput();
        public StreamInstanceDataInput streamInstanceDataInput = new StreamInstanceDataInput();
        public GameObjectInput gameObjectInput = new GameObjectInput();

        [HideInInspector]
        public StreamIndicatorSettings settings;

        protected override StreamIndicator Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var node = new StreamIndicator(settings);

            streamAssetInput.streamBegin = node.OnAssetStreamBegin;
            streamAssetInput.streamEvent = node.OnAssetStreamEvent;
            streamAssetInput.streamEnd = node.OnAssetStreamEnd;

            streamInstanceInput.streamBegin = node.OnInstanceStreamBegin;
            streamInstanceInput.streamEvent = node.OnInstanceStreamEvent;
            streamInstanceInput.streamEnd = node.OnInstanceStreamEnd;

            streamInstanceDataInput.streamBegin = node.OnInstanceDataStreamBegin;
            streamInstanceDataInput.streamEnd = node.OnInstanceDataStreamEnd;

            gameObjectInput.streamBegin = node.OnGameObjectStreamBegin;
            gameObjectInput.streamEvent = node.OnGameObjectStreamEvent;
            gameObjectInput.streamEnd = node.OnGameObjectStreamEnd;

            return node;
        }
    }

    public class StreamIndicator : IReflectNodeProcessor
    {
        DateTime m_Time;

        StreamIndicatorSettings m_Settings;

        StreamCountData m_AssetCountData;
        StreamCountData m_InstanceCountData;
        StreamCountData m_GameObjectCountData;

        public StreamIndicator(StreamIndicatorSettings settings)
        {
            m_Settings = settings;
        }

        public void OnAssetStreamBegin()
        {
            m_Settings.assetStreamBegin?.Invoke();
        }

        public void OnAssetStreamEvent(SyncedData<StreamAsset> streamAsset, StreamEvent eventType)
        {
            switch (eventType)
            {
                case StreamEvent.Added:
                    m_AssetCountData.addedCount++;
                    break;
                case StreamEvent.Changed:
                    m_AssetCountData.changedCount++;
                    break;
                case StreamEvent.Removed:
                    m_AssetCountData.removedCount++;
                    break;
            }
            m_Settings.assetCountModified?.Invoke(m_AssetCountData);
        }

        public void OnAssetStreamEnd()
        {
            m_Settings.assetStreamEnd?.Invoke();
        }

        public void OnInstanceStreamBegin()
        {
            m_Settings.instanceStreamBegin?.Invoke();
        }

        public void OnInstanceStreamEvent(SyncedData<StreamInstance> streamInstance, StreamEvent eventType)
        {
            switch (eventType)
            {
                case StreamEvent.Added:
                    m_InstanceCountData.addedCount++;
                    break;
                case StreamEvent.Changed:
                    m_InstanceCountData.changedCount++;
                    break;
                case StreamEvent.Removed:
                    m_InstanceCountData.removedCount++;
                    break;
            }
            m_Settings.instanceCountModified?.Invoke(m_InstanceCountData);
        }

        public void OnInstanceStreamEnd()
        {
            m_Settings.instanceStreamEnd?.Invoke();
        }


        public void OnInstanceDataStreamBegin()
        {
            m_Settings.instanceDataStreamBegin?.Invoke();
        }

        public void OnInstanceDataStreamEnd()
        {
            m_Settings.instanceDataStreamEnd?.Invoke();
        }

        public void OnGameObjectStreamBegin()
        {
            m_Time = DateTime.Now;
            Debug.Log("Stream begins...");

            m_Settings.gameObjectStreamBegin?.Invoke();
        }

        public void OnGameObjectStreamEvent(SyncedData<GameObject> gameObject, StreamEvent eventType)
        {
            switch (eventType)
            {
                case StreamEvent.Added:
                    m_GameObjectCountData.addedCount++;
                    break;
                case StreamEvent.Changed:
                    m_GameObjectCountData.changedCount++;
                    break;
                case StreamEvent.Removed:
                    m_GameObjectCountData.removedCount++;
                    break;
            }

            m_Settings.gameObjectStreamEvent?.Invoke(
                m_GameObjectCountData.addedCount - m_GameObjectCountData.removedCount,
                m_AssetCountData.addedCount - m_AssetCountData.removedCount);
            m_Settings.gameObjectCountModified?.Invoke(m_GameObjectCountData);
        }

        public void OnGameObjectStreamEnd()
        {
            Debug.Log("All Done " + (DateTime.Now - m_Time).TotalMilliseconds + " MS");

            m_Settings.gameObjectStreamEnd?.Invoke();
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
            m_AssetCountData.addedCount = 0;
            m_AssetCountData.changedCount = 0;
            m_AssetCountData.removedCount = 0;
            m_Settings.assetCountModified?.Invoke(m_AssetCountData);

            m_InstanceCountData.addedCount = 0;
            m_InstanceCountData.changedCount = 0;
            m_InstanceCountData.removedCount = 0;
            m_Settings.instanceCountModified?.Invoke(m_InstanceCountData);

            m_GameObjectCountData.addedCount = 0;
            m_GameObjectCountData.changedCount = 0;
            m_GameObjectCountData.removedCount = 0;
            m_Settings.gameObjectCountModified?.Invoke(m_GameObjectCountData);

        }
    }
}
