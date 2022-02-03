using System;
using Unity.Reflect.Markers.Storage;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Collider))]
    public class MarkerRenderer : MonoBehaviour
    {
        MarkerRendererSpawner m_RendererSpawner;
        Marker m_Marker;
        public void Setup(MarkerRendererSpawner rendererSpawner, IMarker marker, Transform alignedObject)
        {
            m_RendererSpawner = rendererSpawner;
            m_Marker = (Marker)marker;
            var pose = m_Marker.GetWorldPose(new TransformData(alignedObject));
            transform.position = pose.position;
            transform.rotation = pose.rotation;
            gameObject.name = $"[Marker] {marker.Name}";
        }

        void OnMouseUp()
        {
            m_RendererSpawner.SelectMarker(m_Marker);
        }
    }
}
