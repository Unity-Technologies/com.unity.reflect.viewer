using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using UnityEngine.InputSystem;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Viewer.Pipeline;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.Reflect.Viewer
{
    [RequireComponent(typeof(XRController))]
    public class VRSelector : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] InputActionAsset m_InputActionAsset;
        [SerializeField] Transform m_ControllerTransform;
        [SerializeField] XRBaseInteractable m_SelectionTarget;
        [SerializeField] float m_TriggerThreshold = 0.5f;
        #pragma warning restore 0649

        InputAction m_SelectAction;
        ISpatialPicker<Tuple<GameObject, RaycastHit>> m_ObjectPicker;
        bool m_CanSelect;
        bool m_Pressed;
        Ray m_Ray;
        string m_CurrentUserId;
        bool m_IsMeasureToolEnable;
        MeasureToolStateData? m_CachedMeasureToolStateData;

        readonly List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();

        ObjectSelectionInfo m_ObjectSelectionInfo;

        void Start()
        {
            m_SelectionTarget.gameObject.SetActive(false);

            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;
            UIStateManager.sessionStateChanged += OnSessionStateChanged;
            UIStateManager.externalToolChanged += OnToolStateDataChanged;
            UIStateManager.roomConnectionStateChanged += OnRoomConnectionStateChanged;

            m_SelectAction = m_InputActionAsset["VR/Select"];
        }

        void OnDestroy()
        {
            UIStateManager.stateChanged -= OnStateDataChanged;
            UIStateManager.projectStateChanged -= OnProjectStateDataChanged;
            UIStateManager.sessionStateChanged -= OnSessionStateChanged;
            UIStateManager.externalToolChanged -= OnToolStateDataChanged;
            UIStateManager.roomConnectionStateChanged -= OnRoomConnectionStateChanged;
        }

        void OnRoomConnectionStateChanged(RoomConnectionStateData data)
        {
            // Check if current user had change Id
            var matchmakerId = UIStateManager.current.roomConnectionStateData.localUser.matchmakerId;
            if (!string.IsNullOrEmpty(matchmakerId) && m_CurrentUserId != matchmakerId)
            {
                m_CurrentUserId = matchmakerId;
            }
        }

        void OnToolStateDataChanged(ExternalToolStateData toolData)
        {
            if (!UIStateManager.current.stateData.VREnable)
                return;

            var data = toolData.measureToolStateData;

            if (m_CachedMeasureToolStateData == null || m_CachedMeasureToolStateData.Value.toolState != data.toolState)
            {
                if (data.toolState)
                {
                    m_CanSelect = true;
                    m_IsMeasureToolEnable = true;
                    m_SelectionTarget.gameObject.SetActive(true);
                }
                else
                {
                    m_CanSelect = false;
                    m_IsMeasureToolEnable = false;
                    m_SelectionTarget.gameObject.SetActive(false);
                    CleanCache();
                }

                m_CachedMeasureToolStateData = data;
            }
        }

        void Update()
        {
            if (!m_CanSelect)
                return;

            var isButtonPressed = m_SelectAction.ReadValue<float>() > m_TriggerThreshold;

            UpdateTarget();

            if (!isButtonPressed)
            {
                m_Pressed = false;
                return;
            }

            if (!m_Pressed)
            {
                m_ObjectSelectionInfo.selectedObjects = m_Results.Select(x => x.Item1).ToList();
                //Only keep the first element
                if (m_ObjectSelectionInfo.selectedObjects.Count > 1)
                {
                    m_ObjectSelectionInfo.selectedObjects = m_ObjectSelectionInfo.selectedObjects.GetRange(0, 1);
                }
                m_ObjectSelectionInfo.currentIndex = 0; // TODO: deep selection in VR?
                m_ObjectSelectionInfo.userId = m_CurrentUserId;
                m_ObjectSelectionInfo.colorId = 0;

                if(!m_IsMeasureToolEnable)
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, m_ObjectSelectionInfo));

                m_Pressed = true;
            }
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_CanSelect = (data.toolState.activeTool == ToolType.SelectTool) || m_IsMeasureToolEnable;

            if (m_SelectionTarget == null || m_SelectionTarget.gameObject == null)
                return;

            if(!m_CanSelect)
                CleanCache();

            m_SelectionTarget.gameObject.SetActive(m_CanSelect);
        }

        void CleanCache()
        {
            if (m_ObjectPicker != null)
            {
                ((SpatialSelector)m_ObjectPicker).CleanCache();
            }
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            m_ObjectPicker = data.objectPicker;
        }

        void OnSessionStateChanged(UISessionStateData data)
        {
            m_CurrentUserId = data.sessionState.user.UserId;
        }

        void UpdateTarget()
        {
            m_Ray.origin = m_ControllerTransform.position;
            m_Ray.direction = m_ControllerTransform.forward;

            // disable the target first so it doesn't interfere with the raycasts
            m_SelectionTarget.gameObject.SetActive(false);

            // pick
            m_Results.Clear();
            m_ObjectPicker.VRPick(m_Ray, m_Results);

            // enable the target if there is a valid hit
            if (m_Results.Count == 0)
                return;

            m_SelectionTarget.transform.position = m_Results[0].Item2.point;
            m_SelectionTarget.gameObject.SetActive(true);
        }
    }
}
