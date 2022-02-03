using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine;
using Unity.TouchFramework;
using UnityEngine.Reflect.Utils;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

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

        IUISelector<bool> m_WalkModeEnableSelector;
        IUISelector<ICameraViewOption> m_CameraViewTypeSelector;
        IUISelector<SetDialogModeAction.DialogMode> m_DialogModeSelector;
        CameraViewOption m_CameraViewOption;

#pragma warning restore 0649

        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Start()
        {
            DrawCustomGizmos();
            m_FanOutWindow.Close();
        }

        void Awake()
        {
            m_NavigationGizmoButton.buttonClicked += OnNavigationButtonClicked;
            m_TopViewButton.buttonClicked += OnTopView;
            m_LeftViewButton.buttonClicked += OnLeftView;
            m_RightViewButton.buttonClicked += OnRightView;

            m_DisposeOnDestroy.Add(m_WalkModeEnableSelector = UISelectorFactory.createSelector<bool>(WalkModeContext.current, nameof(IWalkModeDataProvider.walkEnabled)));
            m_DisposeOnDestroy.Add(m_CameraViewTypeSelector = UISelectorFactory.createSelector<ICameraViewOption>(CameraOptionsContext.current, nameof(ICameraOptionsDataProvider.cameraViewOption)));
            m_DisposeOnDestroy.Add(m_DialogModeSelector = UISelectorFactory.createSelector<SetDialogModeAction.DialogMode>(UIStateContext.current, nameof(IDialogDataProvider.dialogMode)));

            var screenDpi = UIUtils.GetScreenDpi();
            var deviceType = UIUtils.GetDeviceType(Screen.width, Screen.height, screenDpi);
            if (deviceType == SetDisplayAction.DisplayType.Phone)
            {
                var position = m_FanOutWindow.transform.position;
                position.x += 60;
                m_FanOutWindow.transform.position = position;
            }
        }

        public void HideGizmo()
        {
            m_Target.gameObject.SetActive(false);
        }

        public void ShowGizmo()
        {
            m_Target.gameObject.SetActive(true);
        }

        void OnRightView()
        {
            DispatchAction(SetCameraViewTypeAction.CameraViewType.Right);
        }

        void OnLeftView()
        {
            DispatchAction(SetCameraViewTypeAction.CameraViewType.Left);
        }

        void OnTopView()
        {
            DispatchAction(SetCameraViewTypeAction.CameraViewType.Top);
        }

        void DispatchAction(SetCameraViewTypeAction.CameraViewType cameraViewType)
        {
            m_CameraViewOption.cameraViewType = cameraViewType;
            Dispatcher.Dispatch(SetCameraViewOptionAction.From(m_CameraViewOption));
        }

        [ContextMenu(nameof(OnNavigationButtonClicked))]
        void OnNavigationButtonClicked()
        {
            if (m_WalkModeEnableSelector.GetValue() && m_DialogModeSelector.GetValue() != SetDialogModeAction.DialogMode.Help)
                return;

            var dialogType = m_FanOutWindow.open ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.GizmoMode;
            Dispatcher.Dispatch(OpenSubDialogAction.From(dialogType));
        }

        void Update()
        {
            //Apply camera movement to the gizmo Cube
            if (Camera.main)
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
