using Unity.Reflect;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    [SerializeField]
    public class BoundingBoxFilterNode : ReflectNode<BoundingBoxFilter>
    {
        public StreamAssetInput input = new StreamAssetInput();
        public BoundingBoxFilterSettings settings = new BoundingBoxFilterSettings();

        protected override BoundingBoxFilter Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var node = new BoundingBoxFilter(settings);
            input.streamEvent = node.OnStreamEvent;
            input.streamEnd = node.OnEnd;

            return node;
        }
    }

    public sealed class BoundingBoxFilter : IReflectNodeProcessor, IOnDrawGizmosSelected
    {
        readonly BoundingBoxFilterSettings m_Settings;

        bool m_First = true;

        public BoundingBoxFilter(BoundingBoxFilterSettings settings)
        {
            m_Settings = settings;
        }

        public void OnStreamEvent(SyncedData<StreamAsset> streamAsset, StreamEvent streamEvent)
        {
            if (streamEvent != StreamEvent.Added)
                return;

            var syncBb = streamAsset.data.boundingBox;
            var bb = new Bounds(new Vector3(syncBb.Min.X, syncBb.Min.Y, syncBb.Min.Z), Vector3.zero);
            bb.Encapsulate(new Vector3(syncBb.Max.X, syncBb.Max.Y, syncBb.Max.Z));
            if (m_First && !Mathf.Approximately(bb.size.magnitude, 0.0f))
            {
                m_First = false;
                m_Settings.m_GlobalBoundingBox = bb;
            }

            if (!Mathf.Approximately(bb.size.magnitude, 0.0f))
            {
                m_Settings.m_GlobalBoundingBox.Encapsulate(bb);
            }
        }

        public void OnEnd()
        {
            Debug.Log($"BB: [{m_Settings.m_GlobalBoundingBox.min}, {m_Settings.m_GlobalBoundingBox.max}]");
            m_Settings.onBoundsCalculated?.Invoke(m_Settings.m_GlobalBoundingBox);
        }

        public void OnPipelineInitialized()
        {
            m_First = true;
        }

        public void OnPipelineShutdown()
        {
        }

        public void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color32( 100, 149, 237, 0); // Cornflower Blue
            UnityEditor.Handles.DrawWireCube(m_Settings.m_GlobalBoundingBox.center,
                m_Settings.m_GlobalBoundingBox.size);
#endif
        }
    }
}
