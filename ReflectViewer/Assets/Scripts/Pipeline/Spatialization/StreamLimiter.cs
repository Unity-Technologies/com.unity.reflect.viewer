using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Reflect;
using UnityEngine;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    [Serializable]
    public class StreamLimiterSettings
    {
        #pragma warning disable 0649
        [Tooltip("When enabled, this node is ignored (all inputs are sent directly to outputs)")]
        [SerializeField] bool m_Bypass;
        [SerializeField] InstancesPerFrameLimiter m_InstancesPerFrameLimiter;
        [Tooltip("This limit fallback is only used when there is no InstancesPerFrameLimiter")]
        [SerializeField, Range(1, 1000)] int m_Limit = 1000;
        #pragma warning restore 0649

        public bool bypass
        {
            get => m_Bypass;
            set => m_Bypass = value;
        }

        public int limit => m_InstancesPerFrameLimiter != null ? m_InstancesPerFrameLimiter.maxInstancesPerFrame : m_Limit;
    }

    [Serializable]
    public class StreamInstanceLimiterNode : ReflectNode<StreamLimiter<StreamInstance>>
    {
        public StreamInstanceInput instanceInput = new StreamInstanceInput();
        public StreamInstanceOutput instanceOutput = new StreamInstanceOutput();

        public StreamLimiterSettings settings;

        protected override StreamLimiter<StreamInstance> Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var node = new StreamLimiter<StreamInstance>(settings, instanceOutput);

            instanceInput.streamBegin = node.OnStreamInstanceBegin;
            instanceInput.streamEvent = node.OnStreamInstanceEvent;
            instanceInput.streamEnd = node.OnStreamInstanceEnd;

            return node;
        }
    }

    public class StreamLimiter<TStream> : ReflectTaskNodeProcessor
    {
        readonly StreamLimiterSettings m_Settings;
        readonly DataOutput<TStream> m_StreamOutput;
        readonly ConcurrentQueue<SyncedData<TStream>> m_StreamQueue;
        readonly Dictionary<SyncedData<TStream>, StreamEvent?> m_StreamEvents;

        int m_Counter;

        public StreamLimiter(StreamLimiterSettings settings, DataOutput<TStream> streamOutput)
        {
            m_Settings = settings;
            m_StreamOutput = streamOutput;

            m_StreamQueue = new ConcurrentQueue<SyncedData<TStream>>();
            m_StreamEvents = new Dictionary<SyncedData<TStream>, StreamEvent?>();
        }

        protected override Task RunInternal(CancellationToken token)
        {
            // nothing to do here, implementing ReflectTask to handle Update loop
            return null;
        }

        protected enum State
        {
            Idle,
            Processing,
            WaitingToFinish
        }

        protected State m_State = State.Idle;

        protected override void UpdateInternal(float unscaledDeltaTime)
        {
            if (m_Settings.bypass)
                return;

            lock (m_StreamEvents)
            {
                m_Counter = 0;
                while (m_Counter < m_Settings.limit && m_StreamQueue.TryDequeue(out var stream))
                {
                    if (!m_StreamEvents.TryGetValue(stream, out var eventType))
                        Debug.LogError($"StreamMessageType not found for {stream.ToString()}");

                    // do nothing if the original message type has been cancelled, do not count towards limit
                    if (eventType.HasValue)
                    {
                        m_StreamOutput.SendStreamEvent(stream, eventType.Value);
                        ++m_Counter;
                    }

                    m_StreamEvents.Remove(stream);
                }
            }

            if (m_State == State.WaitingToFinish && m_StreamQueue.IsEmpty)
            {
                m_StreamOutput.SendEnd();
                m_State = State.Idle;
            }
        }

        public void OnStreamInstanceEvent(SyncedData<TStream> stream, StreamEvent eventType)
        {
            if (m_Settings.bypass)
            {
                m_StreamOutput.SendStreamEvent(stream, eventType);
                return;
            }

            lock (m_StreamEvents)
            {
                if (!m_StreamEvents.TryGetValue(stream, out var prevType))
                {
                    m_StreamEvents.Add(stream, eventType);
                    m_StreamQueue.Enqueue(stream);
                    return;
                }

                m_StreamEvents[stream] = ConvertMessageType(prevType, eventType);
            }
        }

        static StreamEvent? ConvertMessageType(StreamEvent? initialType, StreamEvent? newType)
        {
            if (!initialType.HasValue)
                return newType;

            switch (initialType.Value)
            {
                case StreamEvent.Added:
                    if (newType.HasValue && newType.Value == StreamEvent.Removed)
                        return null;
                    break;
                case StreamEvent.Changed:
                    if (newType.HasValue && newType.Value == StreamEvent.Removed)
                        return StreamEvent.Removed;
                    break;
                case StreamEvent.Removed:
                    if (newType.HasValue && newType.Value == StreamEvent.Added)
                        return null;
                    break;
            }

            return newType;
        }

        public void OnStreamInstanceBegin()
        {
            m_State = State.Processing;
            m_StreamOutput.SendBegin();
        }

        public void OnStreamInstanceEnd()
        {
            m_State = State.WaitingToFinish;
        }

        public override void OnPipelineInitialized()
        {
            // nothing to do here
        }

        public override void OnPipelineShutdown()
        {
            m_StreamEvents.Clear();
            base.OnPipelineShutdown();
        }
    }
}
