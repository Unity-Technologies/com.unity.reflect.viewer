using System;
using UnityEngine;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.UI
{
    public class GizmoController : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("Gizmo Axis properties")]
        [SerializeField]
        Transform m_Target;
        [SerializeField]
        Material m_LineMat;
        [SerializeField]
        Mesh m_CylinderMesh;

        [Header("Navigation")]
        [SerializeField, Tooltip("Button to toggle the dialog.")]
        ToolButton m_NavigationGizmoButton;
        [SerializeField]
        FanOutWindow m_FanOutWindow;
        [SerializeField, Tooltip("Button to switch to a top view.")]
        ToolButton m_TopViewButton;
        [SerializeField, Tooltip("Button to switch to a left view")]
        ToolButton m_LeftViewButton;
        [SerializeField, Tooltip("Button to switch to a right view")]
        ToolButton m_RightViewButton;

        CameraOptionData m_CameraOptionData;

#pragma warning restore 0649

        void Start()
        {
            DrawCustomGizmos();
            m_CameraOptionData = UIStateManager.current.stateData.cameraOptionData;
        }

        void Awake()
        {
            m_NavigationGizmoButton.buttonClicked += OnNavigationButtonClicked;
            m_TopViewButton.buttonClicked += OnTopView;
            m_LeftViewButton.buttonClicked += OnLeftView;
            m_RightViewButton.buttonClicked += OnRightView;
        }

        public void HideGizmo()
        {
            m_Target.gameObject.SetActive(false);
        }

        public void ShowGizmo()
        {
            m_Target.gameObject.SetActive(true);
        }

        void CheckClickAmount(CameraViewType currentCameraViewType)
        {
            if (m_CameraOptionData.cameraViewType != currentCameraViewType)
            {
                m_CameraOptionData.numberOfCLick = 0;
            }
            else
            {
                m_CameraOptionData.numberOfCLick += 1;
            }

            m_CameraOptionData.cameraViewType = currentCameraViewType;
        }

        void OnRightView()
        {
            CheckClickAmount(CameraViewType.Right);
            DispatchAction();
        }

        void OnLeftView()
        {
            CheckClickAmount(CameraViewType.Left);
            DispatchAction();
        }

        void OnTopView()
        {
            CheckClickAmount(CameraViewType.Top);
            m_CameraOptionData.numberOfCLick = 0;
            DispatchAction();
        }

        void DispatchAction()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetCameraOption, m_CameraOptionData));
        }

        [ContextMenu(nameof(OnNavigationButtonClicked))]
        void OnNavigationButtonClicked()
        {
            var dialogType = m_FanOutWindow.open ? DialogType.None : DialogType.GizmoMode;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }

        void Update()
        {
            //Apply camera movement to the gizmo Cube
            m_Target.rotation = Quaternion.Inverse(Camera.main.transform.rotation);
        }

        void DrawCustomGizmos()
        {
            Vector3 offset = new Vector3(-2f, -2f, -2f);
            DrawCustomLine(Vector3.up + offset, Color.green, new Vector3(0, 0, 0));
            DrawCustomLine(Vector3.right + offset, Color.red, new Vector3(0, 0, 90));
            DrawCustomLine(Vector3.forward + offset, new Color(0, 0.4f, 1f), new Vector3(90, 0, 0));
        }

        /// <summary>
        /// Draw a cylinder on each axis of the gizmo
        /// </summary>
        /// <param name="direction"> Direction offset for each axis</param>
        /// <param name="color">Specify the axis color</param>
        /// <param name="rotation"> specify the axis rotation</param>
        void DrawCustomLine(Vector3 direction, Color color, Vector3 rotation)
        {
            //Specify the axis cylinder length and size
            float axisLength = 0.25f;
            float axisSize = 0.06f;

            //Init and set the 3 axis gizmo position
            GameObject gizmoGameObject = new GameObject();
            gizmoGameObject.layer = LayerMask.NameToLayer("Gizmo");
            gizmoGameObject.transform.parent = m_Target;
            gizmoGameObject.transform.localPosition = direction * axisLength;
            gizmoGameObject.transform.localRotation = Quaternion.Euler(rotation);

            // Set the radius and length
            gizmoGameObject.transform.localScale = new Vector3(axisSize, axisLength, axisSize);
            MeshFilter ringMesh = gizmoGameObject.AddComponent<MeshFilter>();
            ringMesh.mesh = m_CylinderMesh;

            //Set material property
            MeshRenderer ringRenderer = gizmoGameObject.AddComponent<MeshRenderer>();
            ringRenderer.material = m_LineMat;
            ringRenderer.material.color = color;
        }
    }
}
