using Unity.XRTools.Rendering;
using UnityEngine;
using UnityEngine.XR;

namespace Unity.SpatialFramework.VR
{
    /// <summary>
    /// Displays the XR tracking space boundary
    /// </summary>
    public class XRTrackingBoundary : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField, Tooltip("Reference to the line renderer that will draw the boundary")]
        XRLineRenderer m_BoundaryLineRenderer;

        [SerializeField, Tooltip("Reference to a GameObject to activate if there is no known tracking boundary.")]
        GameObject m_DefaultBoundary;
#pragma warning restore 649

        XRInputSubsystem m_XrInputSubsystem;

        // List<Vector3> m_BoundaryPoints = new List<Vector3>();

        void Start()
        {
            var hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (hmd.isValid)
            {
                OnDeviceConnected(hmd);
            }
            else
            {
                InputDevices.deviceConnected += OnDeviceConnected;
            }
        }

        void OnDeviceConnected(InputDevice device)
        {
            if (m_XrInputSubsystem == null)
            {
                m_XrInputSubsystem = device.subsystem;
                if (m_XrInputSubsystem != null)
                {
                    m_XrInputSubsystem.boundaryChanged += BoundaryChanged;
                    BoundaryChanged(m_XrInputSubsystem);
                }
            }
        }

        void BoundaryChanged(XRInputSubsystem inputSubsystem)
        {
            // m_XrInputSubsystem = inputSubsystem;
            // var allPoints = new List<Vector3>();

            // TODO Figure out why this boundary is not correct
            // if (m_XrInputSubsystem.TryGetBoundaryPoints(allPoints))
            // {
            //     m_DefaultBoundary.SetActive(false);
            //     m_BoundaryLineRenderer.enabled = true;
            //     var prevPoint = allPoints[0];
            //     var delta = Vector3.forward;
            //     var prevDelta = Vector3.zero;
            //     m_BoundaryPoints.Clear();
            //     for (var i = 1; i < allPoints.Count; i++)
            //     {
            //         var point = allPoints[i];
            //         delta = prevPoint - point;
            //         if (Vector3.Magnitude(delta) > 0.05f || Vector3.Dot(delta.normalized, prevDelta.normalized) < 0.5f)
            //         {
            //             prevPoint = point;
            //             prevDelta = delta;
            //             m_BoundaryPoints.Add(point);
            //         }
            //
            //     }
            //
            //     m_BoundaryLineRenderer.SetVertexCount(m_BoundaryPoints.Count);
            //     m_BoundaryLineRenderer.SetPositions(m_BoundaryPoints.ToArray());
            // }
            // else
            {
                m_DefaultBoundary.SetActive(true);
                m_BoundaryLineRenderer.enabled = false;
            }
        }
    }
}
