using System;
using SharpFlux.Dispatching;
using Unity.Reflect.Collections;
using Unity.Reflect.Viewer.UI;
using UnityEngine.Reflect.Viewer.Core;
using Unity.Reflect.Actors;
using UnityEngine.Reflect.Viewer.Core.Actions;

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

        [SerializeField]
        Camera m_Camera;
        [SerializeField]
        GameObject m_IndicatorPrefab;
        [SerializeField]
        AnimationCurve m_DistanceOverTime;
        [SerializeField]
        AnimationCurve m_IndicatorSizeOverTime;

        Transform m_CameraTransform;
        bool m_IsTeleporting;
        Vector3 m_Source;
        Vector3 m_Destination;
        float m_Timer;
        GameObject m_IndicatorInstance;
        Vector3 m_IndicatorScale = new Vector3(1f, 0f, 1f);

        Vector3? m_PreviousTarget;
        FreeFlyCamera m_FreeFlyCamera;
        IUISelector<IPicker> m_TeleportPickerSelector;
        IUISelector<bool> m_WalkModeEnableSelector;
        IUISelector<IWalkInstructionUI> m_WalkInstructionSelector;
        IUISelector<bool> m_HOLDFilterSelector;
        IUISelector<Vector3> m_TeleportTargetSelector;

        void Start()
        {
            m_CameraTransform = m_Camera.transform;
            m_FreeFlyCamera = m_Camera.GetComponent<FreeFlyCamera>();

            m_TeleportTargetSelector = UISelectorFactory.createSelector<Vector3>(ProjectContext.current, nameof(ITeleportDataProvider.teleportTarget), OnTeleportTargetChanged);
            m_TeleportPickerSelector = UISelectorFactory.createSelector<IPicker>(ProjectContext.current, nameof(ITeleportDataProvider.teleportPicker));
            m_WalkModeEnableSelector = UISelectorFactory.createSelector<bool>(WalkModeContext.current, nameof(IWalkModeDataProvider.walkEnabled));
            m_WalkInstructionSelector = UISelectorFactory.createSelector<IWalkInstructionUI>(WalkModeContext.current, nameof(IWalkModeDataProvider.instruction));
            m_HOLDFilterSelector = UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.filterHlods));
        }

        void OnDestroy()
        {
            m_TeleportTargetSelector?.Dispose();
            m_TeleportPickerSelector?.Dispose();
            m_WalkModeEnableSelector?.Dispose();
            m_WalkInstructionSelector?.Dispose();
            m_HOLDFilterSelector?.Dispose();
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

            if (m_WalkModeEnableSelector.GetValue())
            {
                Dispatcher.Dispatch(SetFinishTeleportAction.From(true));
                Dispatcher.Dispatch(SetFinishTeleportAction.From(false));
                m_WalkInstructionSelector.GetValue().Next();
            }

            // reset and enable free fly camera
            if (m_FreeFlyCamera == null)
                return;

            m_FreeFlyCamera.ResetDesiredPosition();
            m_FreeFlyCamera.enabled = true;
        }

        void OnTeleportTargetChanged(Vector3 newData)
        {
            if (m_PreviousTarget == newData)
                return;

            if (!newData.Equals(Vector3.zero)) //Cannot use nullable Vector3 with properties
            {
                m_Source = m_CameraTransform.position;
                m_Destination = newData;
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

            m_PreviousTarget = newData;
        }

        // called in UI event
        public void TriggerTeleport(Vector2 position)
        {
            if (m_IsTeleporting)
                return;

            var flags = m_HOLDFilterSelector.GetValue() ? new [] { SpatialActor.k_IsHlodFlag, SpatialActor.k_IsDisabledFlag } : new [] { SpatialActor.k_IsDisabledFlag };
            var picker = (ISpatialPickerAsync<Tuple<GameObject, RaycastHit>>) m_TeleportPickerSelector.GetValue();

            picker.Pick(m_Camera.ScreenPointToRay(position), results =>
            {
                if (results.Count == 0)
                    return;

                var hitInfo = results[0].Item2;

                var point = hitInfo.point;
                var normal = hitInfo.normal;
                var target = point +
                    m_ArrivalOffsetFixed +
                    m_ArrivalOffsetNormal * normal +
                    m_ArrivalOffsetRelative * (m_Source - point).normalized;

                Dispatcher.Dispatch(TeleportAction.From(target));
            }, flags);
        }

        public void AsyncGetTeleportTarget(Vector2 position, Action<Vector3> callback)
        {
            var flags = m_HOLDFilterSelector.GetValue() ? new [] { SpatialActor.k_IsHlodFlag, SpatialActor.k_IsDisabledFlag } : new [] { SpatialActor.k_IsDisabledFlag };
            ((ISpatialPickerAsync<Tuple<GameObject, RaycastHit>>)m_TeleportPickerSelector.GetValue()).Pick(m_Camera.ScreenPointToRay(position), results =>
            {
                if (results.Count == 0)
                {
                    callback(Vector3.zero);
                    return;
                }

                var hitInfo = results[0].Item2;

                var point = hitInfo.point;
                var target = point;

                callback(target);
            }, flags);
        }
    }
}
