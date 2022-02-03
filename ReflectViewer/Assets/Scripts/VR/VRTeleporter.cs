using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.Reflect.Collections;
using Unity.Reflect.Viewer.UI;
using UnityEngine.InputSystem;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
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

        bool m_CanTeleport;
        bool m_IsTeleporting;
        Vector3[] m_LinePoints;
        Transform m_MainCamera;
        bool m_IsPicking;
        float m_LerpSpeed = 12;

        const float k_MaxRaycastDistance = 30;
        const float k_MaxVelocity = 50;

        List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();
        XRRig m_XrRig;
        IUISelector<Transform> m_RootSelector;
        float m_StartRaycastVelocity;

        IUISelector<IPicker> m_TeleportPickerSelector;
        IUISelector<CameraTransformInfo> m_CamInfoSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            ProjectContext.current.stateChanged += OnStateChange;
        }

        void OnStateChange()
        {
            SetInitialTeleportDistance();
        }

        void Start()
        {
            if (m_XrRig == null)
                m_XrRig = FindObjectOfType<XRRig>();

            m_XrRayInteractor = GetComponent<XRRayInteractor>();
            m_XrInteractorLineVisual = GetComponent<XRInteractorLineVisual>();
            m_LinePoints = new Vector3[k_MaxLinePoints];
            m_TeleportationTarget.gameObject.SetActive(false);
            m_MainCamera = Camera.main.transform;

            m_DisposeOnDestroy.Add(m_RootSelector = UISelectorFactory.createSelector<Transform>(PipelineContext.current, nameof(IPipelineDataProvider.rootNode)));
            m_DisposeOnDestroy.Add(m_CamInfoSelector = UISelectorFactory.createSelector<CameraTransformInfo>(ProjectContext.current, nameof(ITeleportDataProvider.cameraTransformInfo)));
            m_DisposeOnDestroy.Add(m_TeleportPickerSelector = UISelectorFactory.createSelector<IPicker>(ProjectContext.current, nameof(ITeleportDataProvider.teleportPicker),
                async =>
                {
                    SetInitialTeleportDistance();
                }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current, nameof(IToolStateDataProvider.activeTool),
                type =>
                {
                    m_CanTeleport = type != SetActiveToolAction.ToolType.SelectTool;
                }));
            m_TeleportAction = m_InputActionAsset["VR/Teleport"];
            m_InputActionAsset["VR/Select"].performed += OnTeleport;
        }

        void SetInitialTeleportDistance()
        {
            m_StartRaycastVelocity = 10f * Vector3.Distance(m_CamInfoSelector.GetValue().position, m_RootSelector.GetValue().position);
            SetTeleportCurve(m_StartRaycastVelocity);
        }

        void OnTeleport(InputAction.CallbackContext obj)
        {
            if (m_CanTeleport && m_IsTeleporting && m_TeleportationTarget.gameObject.activeSelf)
            {
                Vector3 offset = new Vector3(0, m_XrRig.cameraYOffset / 4f, 0);
                Dispatcher.Dispatch(TeleportAction.From(m_TeleportationTarget.transform.position + offset));
                SetTeleportCurve(k_MaxVelocity);
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
                    StopTeleportPicking();
            }
            else if (isButtonPressed && m_CanTeleport)
                StartTeleportPicking();
        }

        void StartTeleportPicking()
        {
            m_XrRayInteractor.lineType = XRRayInteractor.LineType.ProjectileCurve;
            m_XrRayInteractor.sampleFrequency = 40;
            m_XrRayInteractor.enableUIInteraction = false;
            m_XrInteractorLineVisual.overrideInteractorLineLength = false;
            m_IsTeleporting = true;
            SetTeleportCurve(m_StartRaycastVelocity);
            ((SpatialSelector)m_TeleportPickerSelector.GetValue()).SetPicking(true);
        }

        void StopTeleportPicking()
        {
            m_TeleportationTarget.gameObject.SetActive(false);
            m_XrRayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
            m_XrRayInteractor.maxRaycastDistance = k_MaxRaycastDistance;
            m_XrRayInteractor.enableUIInteraction = true;
            m_XrInteractorLineVisual.overrideInteractorLineLength = true;
            ((SpatialSelector)m_TeleportPickerSelector.GetValue()).SetPicking(false);
            m_IsTeleporting = false;
        }

        void UpdateTarget()
        {
            if (!m_XrRayInteractor.GetLinePoints(ref m_LinePoints, out var nbPoints))
            {
                Debug.LogError($"[{nameof(VRTeleporter)}] XRRayInteractor.GetLinePoints failed!");
                return;
            }

            if (!m_IsPicking)
            {
                m_IsPicking = true;

                // pick
                ((ISpatialPickerAsync<Tuple<GameObject, RaycastHit>>)m_TeleportPickerSelector.GetValue()).Pick(m_LinePoints, nbPoints, results =>
                {
                    m_IsPicking = false;
                    m_Results = results;

                    // enable the target if there is a valid hit
                    if (m_Results.Count == 0)
                    {
                        m_TeleportationTarget.gameObject.SetActive(false);
                        return;
                    }

                    // Ignore the first GameObject as he might detect the teleport cube
                    int position = 0;
                    for (int i = 0; i < m_Results.Count; ++i)
                    {
                        if (m_Results[i].Item1.CompareTag("IgnoreSpatialSelector"))
                        {
                            position += 1;
                        }
                    }

                    m_TeleportationTarget.transform.position = Vector3.Lerp(m_TeleportationTarget.transform.position, m_Results[position].Item2.point, m_LerpSpeed * Time.deltaTime);
                    m_TeleportationTarget.gameObject.SetActive(m_IsTeleporting);
                });
            }
            SetTeleportCurve(10 * Vector3.Distance(m_MainCamera.position, m_TeleportationTarget.transform.position));
        }

        void SetTeleportCurve(float velocity)
        {
            m_XrRayInteractor.velocity = velocity;
            m_XrRayInteractor.acceleration = velocity * 0.6f;
        }

        void OnDestroy()
        {
            ((SpatialSelector)m_TeleportPickerSelector.GetValue()).SetPicking(false);
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }
    }
}
