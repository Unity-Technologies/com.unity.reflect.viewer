using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using Unity.Reflect.Viewer.UI;
using UnityEngine.Reflect.Viewer.Pipeline;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.Reflect.Viewer
{
    [RequireComponent(typeof(XRController))]
    public class VRSelector : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] XRBaseInteractable m_SelectionTarget;
        #pragma warning restore 0649

        XRController m_XrController;
        Transform m_XrControllerTransform;

        ISpatialPicker<Tuple<GameObject, RaycastHit>> m_ObjectPicker;
        bool m_CanSelect;
        Ray m_Ray;

        readonly List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();

        ObjectSelectionInfo m_ObjectSelectionInfo;

        void Start()
        {
            m_XrController = GetComponent<XRController>();
            m_XrControllerTransform = m_XrController.transform;

            m_SelectionTarget.gameObject.SetActive(false);

            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;
        }

        void Update()
        {
            if (!m_XrController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out var isButtonPressed))
                return;

            if (!m_CanSelect)
                return;

            UpdateTarget();

            if (!isButtonPressed)
                return;

            m_ObjectSelectionInfo.selectedObjects = m_Results.Select(x => x.Item1).ToList();
            m_ObjectSelectionInfo.currentIndex = 0; // TODO: deep selection in VR?

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, m_ObjectSelectionInfo));
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_CanSelect = data.toolState.activeTool == ToolType.SelectTool;
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            m_ObjectPicker = data.objectPicker;
        }

        void UpdateTarget()
        {
            m_Ray.origin = m_XrControllerTransform.position;
            m_Ray.direction = m_XrControllerTransform.forward;

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
