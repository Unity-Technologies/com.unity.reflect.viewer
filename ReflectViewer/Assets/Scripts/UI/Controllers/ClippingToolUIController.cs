using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible for managing the selection of the clipping tool
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class ClippingToolUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Does dialog closes on selection.")]
        public bool m_CloseOnSelect;
        [SerializeField, Tooltip("Button to toggle the dialog.")]
        Button m_DialogButton;
        [SerializeField, Tooltip("List of button controls.")]
        List<ButtonControl> m_ButtonControls = new List<ButtonControl>();
#pragma warning restore CS0649

        ButtonControl m_ActiveButtonControl;
        ClippingTool? m_cachedClippingTool;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            foreach (var buttonControl in m_ButtonControls)
            {
                buttonControl.onControlTap.AddListener(OnButtonTap);
            }
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_cachedClippingTool == null || m_cachedClippingTool != stateData.toolState.clippingTool)
            {
                m_cachedClippingTool = stateData.toolState.clippingTool;

                var i = 0;
                foreach (var buttonControl in m_ButtonControls)
                {
                    var on = (i == (int) stateData.toolState.clippingTool);
                    buttonControl.@on = on;
                    i++;
                }
            }
        }

        void OnButtonTap(BaseEventData eventData)
        {
            var buttonControl = eventData.selectedObject.GetComponent<ButtonControl>();
            if (buttonControl == null)
                return;

            var clippingTool = (ClippingTool)m_ButtonControls.IndexOf(buttonControl);

            if (m_CloseOnSelect)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
            }

            var toolState = UIStateManager.current.stateData.toolState;
            toolState.clippingTool = clippingTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }
    }
}

