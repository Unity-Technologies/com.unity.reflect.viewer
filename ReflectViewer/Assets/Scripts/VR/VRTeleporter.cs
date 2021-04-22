using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
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
        [SerializeField]
        InputActionAsset m_InputActionAsset;
        [SerializeField]
        BaseTeleportationInteractable m_TeleportationTarget;
#pragma warning restore 0649

        InputAction m_TeleportAction;
        XRRayInteractor m_XrRayInteractor;
        XRInteractorLineVisual m_XrInteractorLineVisual;

        ISpatialPicker<Tuple<GameObject, RaycastHit>> m_ObjectPicker;
        bool m_CanTeleport;
        bool m_IsTeleporting;
        Vector3[] m_LinePoints;
        Transform m_MainCamera;

        const float k_MaxRaycastDistance = 30;
        const float k_MaxVelocity = 50;

        readonly List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();
        XRRig m_XrRig;

        void Start()
        {
            if (m_XrRig == null)
                m_XrRig = FindObjectOfType<XRRig>();

            m_XrRayInteractor = GetComponent<XRRayInteractor>();
            m_XrInteractorLineVisual = GetComponent<XRInteractorLineVisual>();

            m_LinePoints = new Vector3[k_MaxLinePoints];

            m_TeleportationTarget.gameObject.SetActive(false);

            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            m_TeleportAction = m_InputActionAsset["VR/Teleport"];
            m_InputActionAsset["VR/Select"].performed += OnTeleport;
            m_MainCamera = Camera.main.transform;
        }

        void OnTeleport(InputAction.CallbackContext obj)
        {
            if (m_CanTeleport && m_IsTeleporting)
            {
                Vector3 offset = new Vector3(0, m_XrRig.cameraYOffset / 4f, 0);
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Teleport, m_TeleportationTarget.transform.position + offset));
            }
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
            SetTeleportCurve(k_MaxVelocity);
            m_XrRayInteractor.sampleFrequency = 40;
            m_XrRayInteractor.enableUIInteraction = false;
            m_XrInteractorLineVisual.overrideInteractorLineLength = false;
            m_IsTeleporting = true;
        }

        void StopTeleport()
        {
            m_TeleportationTarget.gameObject.SetActive(false);
            m_XrRayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
            m_XrRayInteractor.maxRaycastDistance = k_MaxRaycastDistance;
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
            m_ObjectPicker?.Pick(m_LinePoints, nbPoints, m_Results);

            // enable the target if there is a valid hit
            if (m_Results.Count == 0)
                return;

            m_TeleportationTarget.transform.position = m_Results[0].Item2.point;
            m_TeleportationTarget.gameObject.SetActive(true);

            // This help to keep a curve for the teleport line
            SetTeleportCurve(1.5f * Vector3.Distance(m_MainCamera.position, m_TeleportationTarget.transform.position));
        }

        void SetTeleportCurve(float velocity)
        {
            m_XrRayInteractor.velocity = velocity;
            m_XrRayInteractor.acceleration = velocity * 0.6f;
        }
    }
}
