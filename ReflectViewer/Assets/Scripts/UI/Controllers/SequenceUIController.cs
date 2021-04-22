using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
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

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
            m_DialogWindow = GetComponent<DialogWindow>();

            m_JoysticksToggle.onValueChanged.AddListener(OnJoysticksToggleChanged);
            m_ControlsToggle.onValueChanged.AddListener(OnControlsToggleChanged);
            m_HudToggle.onValueChanged.AddListener(OnHUDToggleChanged);
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_DialogButtonImage.enabled = data.activeDialog == DialogType.Sequence;
            m_DialogButton.interactable = data.toolbarsEnabled;
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
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.Sequence;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }
    }
}
