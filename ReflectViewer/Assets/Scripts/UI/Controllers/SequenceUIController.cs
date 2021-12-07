using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class SequenceUIController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        Button m_DialogButton;
        [SerializeField]
        SlideToggle m_JoysticksToggle;
        [SerializeField]
        SlideToggle m_ControlsToggle;
        [SerializeField]
        SlideToggle m_HudToggle;
        [SerializeField]
        List<Canvas> m_ControlsCanvases = new List<Canvas>();
#pragma warning restore 649

        DialogWindow m_DialogWindow;
        Image m_DialogButtonImage;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
            m_DialogWindow = GetComponent<DialogWindow>();

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog),
                type =>
                {
                    m_DialogButtonImage.enabled = type == OpenDialogAction.DialogType.Sequence;
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled),
                enabled =>
                {
                    m_DialogButton.interactable = enabled;
                }));

            m_JoysticksToggle.onValueChanged.AddListener(OnJoysticksToggleChanged);
            m_ControlsToggle.onValueChanged.AddListener(OnControlsToggleChanged);
            m_HudToggle.onValueChanged.AddListener(OnHUDToggleChanged);
        }

        void OnHUDToggleChanged(bool arg0)
        {
            throw new System.NotImplementedException();
        }

        void OnControlsToggleChanged(bool arg0)
        {
            throw new System.NotImplementedException();
        }

        void OnJoysticksToggleChanged(bool on)
        {
            throw new System.NotImplementedException();
        }

        void Start()
        {
            m_DialogButton.onClick.AddListener(OnDialogButtonClick);
        }

        void OnDialogButtonClick()
        {
            var dialogType = m_DialogWindow.open ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.Sequence;
            Dispatcher.Dispatch(OpenDialogAction.From(dialogType));
        }
    }
}
