using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class CameraOptionsUIController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        Button m_DialogButton;
        [SerializeField]
        TMP_Dropdown m_CameraTypeDropdown;
        [SerializeField]
        TMP_Dropdown m_CameraViewDropdown;
        [SerializeField]
        Button m_CameraDefaultViewButton;

        [SerializeField]
        SlideToggle m_JoysticksToggle;
        [SerializeField]
        SegmentedPropertyControl m_JoystickPreferenceSwitch;

        [SerializeField]
        SlideToggle m_NavigationAutoToggle;
        [SerializeField]
        MinMaxPropertyControl m_NavigationSpeedControl;

#pragma warning restore 649

        DialogWindow m_DialogWindow;
        Image m_DialogButtonImage;

        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
            m_DialogWindow = GetComponent<DialogWindow>();

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog),
                type =>
                {
                    m_DialogButtonImage.enabled = type == OpenDialogAction.DialogType.CameraOptions;
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled), data => { m_DialogButton.interactable = data; }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetCameraProjectionTypeAction.CameraProjectionType>(CameraOptionsContext.current, nameof(ICameraOptionsDataProvider.cameraProjectionType), OnCameraProjectionTypeChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<ICameraViewOption>(CameraOptionsContext.current, nameof(ICameraOptionsDataProvider.cameraViewOption), OnCameraViewTypeChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(CameraOptionsContext.current, nameof(ICameraOptionsDataProvider.enableJoysticks),
                data =>
                {
                    m_JoysticksToggle.on = data;
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<JoystickPreference>(CameraOptionsContext.current, nameof(ICameraOptionsDataProvider.joystickPreference),
                data =>
                {
                    var joystickPreferenceIndex = data == JoystickPreference.RightHanded ? 0 : 1;
                    m_JoystickPreferenceSwitch.activePropertyIndex = joystickPreferenceIndex;
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(CameraOptionsContext.current, nameof(ICameraOptionsDataProvider.enableAutoNavigationSpeed),
                data =>
                {
                    m_NavigationAutoToggle.on = data;
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(CameraOptionsContext.current, nameof(ICameraOptionsDataProvider.navigationSpeed),
                data =>
                {
                    m_NavigationSpeedControl.SetValue(data);
                }));
        }

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void OnCameraProjectionTypeChanged(SetCameraProjectionTypeAction.CameraProjectionType type)
        {
            int cameraProjectionTypeIndex = 0;
            switch (type)
            {
                case SetCameraProjectionTypeAction.CameraProjectionType.Perspective:
                    cameraProjectionTypeIndex = 0;
                    break;
                case SetCameraProjectionTypeAction.CameraProjectionType.Orthographic:
                    cameraProjectionTypeIndex = 1;
                    break;
            }
            m_CameraTypeDropdown.SetValueWithoutNotify(cameraProjectionTypeIndex);
        }

        void OnCameraViewTypeChanged(ICameraViewOption type)
        {
            if (type == null)
                return;

            int cameraViewTypeIndex = 0;
            switch (type.cameraViewType)
            {
                case SetCameraViewTypeAction.CameraViewType.Default:
                    cameraViewTypeIndex = -1;
                    break;
                case SetCameraViewTypeAction.CameraViewType.Top:
                    cameraViewTypeIndex = 0;
                    break;
                case SetCameraViewTypeAction.CameraViewType.Left:
                    cameraViewTypeIndex = 1;
                    break;
                case SetCameraViewTypeAction.CameraViewType.Right:
                    cameraViewTypeIndex = 2;
                    break;
            }
            if( cameraViewTypeIndex != -1)
                m_CameraViewDropdown.SetValueWithoutNotify(cameraViewTypeIndex);
        }

        void Start()
        {
            m_DialogButton.onClick.AddListener(OnDialogButtonClicked);

            m_CameraTypeDropdown.onValueChanged.AddListener(OnCameraTypeChanged);
            m_CameraViewDropdown.onValueChanged.AddListener(OnCameraViewChanged);
            m_CameraDefaultViewButton.onClick.AddListener(OnCameraDefaultViewButtonClicked);
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.CameraOptions;
            Dispatcher.Dispatch(OpenDialogAction.From(dialogType));
        }


        void OnCameraTypeChanged(int index)
        {
            var cameraProjectionType =
                index == 0 ? SetCameraProjectionTypeAction.CameraProjectionType.Perspective : SetCameraProjectionTypeAction.CameraProjectionType.Orthographic;

            Dispatcher.Dispatch(SetCameraProjectionTypeAction.From(cameraProjectionType));
        }

        void OnCameraViewChanged(int index)
        {
            var cameraViewType = SetCameraViewTypeAction.CameraViewType.Default;
            if (index == 0)
                cameraViewType = SetCameraViewTypeAction.CameraViewType.Top;
            else if (index == 1)
                cameraViewType = SetCameraViewTypeAction.CameraViewType.Left;
            else if (index == 2)
                cameraViewType = SetCameraViewTypeAction.CameraViewType.Right;

            Dispatcher.Dispatch(SetCameraViewTypeAction.From(cameraViewType));
        }

        void OnCameraDefaultViewButtonClicked()
        {
            var cameraViewType = SetCameraViewTypeAction.CameraViewType.Default;
            Dispatcher.Dispatch(SetCameraViewTypeAction.From(cameraViewType));
        }
    }
}
