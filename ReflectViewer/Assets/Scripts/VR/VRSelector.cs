using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using Unity.Reflect.Viewer.UI;
using UnityEngine.InputSystem;
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

        readonly List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();

        ObjectSelectionInfo m_ObjectSelectionInfo;

        void Start()
        {
            m_SelectionTarget.gameObject.SetActive(false);

            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            m_SelectAction = m_InputActionAsset["VR/Select"];
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
                m_ObjectSelectionInfo.currentIndex = 0; // TODO: deep selection in VR?

                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, m_ObjectSelectionInfo));

                m_Pressed = true;
            }
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_CanSelect = data.toolState.activeTool == ToolType.SelectTool;

            if (m_SelectionTarget == null || m_SelectionTarget.gameObject == null)
                return;

            m_SelectionTarget.gameObject.SetActive(m_CanSelect);
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            m_ObjectPicker = data.objectPicker;
        }

        void UpdateTarget()
        {
            m_Ray.origin = m_ControllerTransform.position;
            m_Ray.direction = m_ControllerTransform.forward;

            // disable the target first so it doesn't interfere with the raycasts
            m_SelectionTarget.gameObject.SetActive(false);

            // pick
            m_Results.Clear();
            m_ObjectPicker.Pick(m_Ray, m_Results);

            // enable the target if there is a valid hit
            if (m_Results.Count == 0)
                return;

            m_SelectionTarget.transform.position = m_Results[0].Item2.point;
            m_SelectionTarget.gameObject.SetActive(true);
        }
    }
}
