using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.UI;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace UnityEngine.Reflect.Viewer
{
    public class UITeleportController : MonoBehaviour
    {
        [Tooltip("Offset from the contact point back towards the start position")]
        public float m_ArrivalOffsetRelative = 1f;
        [Tooltip("Offset along the surface normal of the selected object")]
        public float m_ArrivalOffsetNormal = 1f;
        [Tooltip("Fixed offset in world space")]
        public Vector3 m_ArrivalOffsetFixed = Vector3.zero;
        [Tooltip("Indicator offset in world space")]
        public Vector3 m_IndicatorOffsetFixed = Vector3.down;
        [Tooltip("Fixed time for the teleport animation")]
        public float m_LerpTime = 1f;

        #pragma warning disable 0649
        [SerializeField] Camera m_Camera;
        [SerializeField] GameObject m_IndicatorPrefab;
        [SerializeField] AnimationCurve m_DistanceOverTime;
        [SerializeField] AnimationCurve m_IndicatorSizeOverTime;
        #pragma warning restore 0649

        Transform m_CameraTransform;
        bool m_IsTeleporting;
        Vector3 m_Source;
        Vector3 m_Destination;
        float m_Timer;
        GameObject m_IndicatorInstance;
        Vector3 m_IndicatorScale = new Vector3(1f, 0f, 1f);

        ISpatialPicker<Tuple<GameObject, RaycastHit>> m_TeleportPicker;
        readonly List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();

        Vector3? m_PreviousTarget;
        FreeFlyCamera m_FreeFlyCamera;

        void Start()
        {
            m_CameraTransform = m_Camera.transform;
            m_FreeFlyCamera = m_Camera.GetComponent<FreeFlyCamera>();

            UIStateManager.projectStateChanged += OnProjectStateDataChanged;
        }

        void Update()
        {
            // if currently in movement
            if (!m_IsTeleporting)
                return;

            m_Timer += Time.deltaTime;

            // lerp toward destination
            var ratio = m_Timer / m_LerpTime;
            m_CameraTransform.position = Vector3.Lerp(m_Source, m_Destination, m_DistanceOverTime.Evaluate(ratio));

            // animate the indicator
            m_IndicatorScale.y = m_IndicatorSizeOverTime.Evaluate(ratio);
            m_IndicatorInstance.transform.localScale = m_IndicatorScale;

            if (m_Timer < m_LerpTime)
                return;

            // reset when timer ends
            m_Timer = 0f;
            m_IsTeleporting = false;
            Destroy(m_IndicatorInstance);

            if (UIStateManager.current.walkStateData.walkEnabled)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.FinishTeleport, true));
                UIStateManager.current.walkStateData.instruction.Next();
            }

            // reset and enable free fly camera
            if (m_FreeFlyCamera == null)
                return;

            m_FreeFlyCamera.ResetDesiredPosition();
            m_FreeFlyCamera.enabled = true;
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            m_TeleportPicker = data.teleportPicker;

            if (m_PreviousTarget == data.teleportTarget)
                return;

            if (data.teleportTarget.HasValue)
            {
                m_Source = m_CameraTransform.position;
                m_Destination = data.teleportTarget.Value;
                m_IsTeleporting = true;

                m_IndicatorInstance = Instantiate(m_IndicatorPrefab);
                m_IndicatorInstance.transform.position = m_Destination + m_IndicatorOffsetFixed;

                // billboard effect
                m_IndicatorInstance.transform.LookAt(m_Source);

                // avoid the full size indicator popping up before the animation
                m_IndicatorScale.y = 0f;
                m_IndicatorInstance.transform.localScale = m_IndicatorScale;

                // disable free fly camera so it doesn't interfere
                if (m_FreeFlyCamera != null)
                    m_FreeFlyCamera.enabled = false;
            }

            m_PreviousTarget = data.teleportTarget;
        }

        // called in UI event
        public void TriggerTeleport(Vector2 position)
        {
            if (m_IsTeleporting)
                return;

            m_TeleportPicker.Pick(m_Camera.ScreenPointToRay(position), m_Results);
            if (m_Results.Count == 0)
                return;

            var hitInfo = m_Results[0].Item2;

            var point = hitInfo.point;
            var normal = hitInfo.normal;
            var target = point +
                m_ArrivalOffsetFixed +
                m_ArrivalOffsetNormal * normal +
                m_ArrivalOffsetRelative * (m_Source - point).normalized;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Teleport, target));
        }

        public Vector3 GetTeleportTarget(Vector2 position)
        {
            m_TeleportPicker?.Pick(m_Camera.ScreenPointToRay(position), m_Results);
            if (m_Results.Count == 0)
                return Vector3.zero;

            var hitInfo = m_Results[0].Item2;

            var point = hitInfo.point;
            var target = point;
            target.y += 0.001f;

            return target;
        }
    }
}
