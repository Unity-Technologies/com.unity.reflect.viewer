using System;
using System.Collections.Generic;
using Unity.Reflect.Viewer.UI;
using UnityEngine.InputSystem;
using UnityEngine.Reflect.Viewer.Pipeline;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.Reflect.Viewer
{
    [RequireComponent(typeof(XRController), typeof(XRRayInteractor), typeof(XRInteractorLineVisual))]
    public class VRTeleporter : MonoBehaviour
    {
        const int k_MaxLinePoints = 20;

        #pragma warning disable 0649
        [SerializeField] InputActionAsset m_InputActionAsset;
        [SerializeField] BaseTeleportationInteractable m_TeleportationTarget;
        #pragma warning restore 0649

        InputAction m_TeleportAction;
        XRRayInteractor m_XrRayInteractor;
        XRInteractorLineVisual m_XrInteractorLineVisual;

        ISpatialPicker<Tuple<GameObject, RaycastHit>> m_ObjectPicker;
        bool m_CanTeleport;
        bool m_IsTeleporting;
        Vector3[] m_LinePoints;

        readonly List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();

        void Start()
        {
            m_XrRayInteractor = GetComponent<XRRayInteractor>();
            m_XrInteractorLineVisual = GetComponent<XRInteractorLineVisual>();

            m_LinePoints = new Vector3[k_MaxLinePoints];

            m_TeleportationTarget.gameObject.SetActive(false);

            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            m_TeleportAction = m_InputActionAsset["VR/Teleport"];
        }

        void Update()
        {
            var isButtonPressed = m_TeleportAction.ReadValue<float>() > 0;

            if (m_IsTeleporting)
            {
                if (isButtonPressed)
                    UpdateTarget();
                else
                    StopTeleport();
            }
            else if (isButtonPressed && m_CanTeleport)
                StartTeleport();
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_CanTeleport = data.toolState.activeTool != ToolType.SelectTool;
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            m_ObjectPicker = data.teleportPicker;
        }

        void StartTeleport()
        {
            m_XrRayInteractor.lineType = XRRayInteractor.LineType.ProjectileCurve;
            m_XrRayInteractor.enableUIInteraction = false;
            m_XrInteractorLineVisual.overrideInteractorLineLength = false;
            m_IsTeleporting = true;
        }

        void StopTeleport()
        {
            m_TeleportationTarget.gameObject.SetActive(false);
            m_XrRayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
            m_XrRayInteractor.enableUIInteraction = true;
            m_XrInteractorLineVisual.overrideInteractorLineLength = true;
            m_IsTeleporting = false;
        }

        void UpdateTarget()
        {
            if (!m_XrRayInteractor.GetLinePoints(ref m_LinePoints, out var nbPoints))
            {
                Debug.LogError($"[{nameof(VRTeleporter)}] XRRayInteractor.GetLinePoints failed!");
                return;
            }

            // disable the target first so it doesn't interfere with the raycasts
            m_TeleportationTarget.gameObject.SetActive(false);

            // pick
            m_Results.Clear();
            m_ObjectPicker.Pick(m_LinePoints, nbPoints, m_Results);

            // enable the target if there is a valid hit
            if (m_Results.Count == 0)
                return;

            m_TeleportationTarget.transform.position = m_Results[0].Item2.point;
            m_TeleportationTarget.gameObject.SetActive(true);
        }
    }
}
