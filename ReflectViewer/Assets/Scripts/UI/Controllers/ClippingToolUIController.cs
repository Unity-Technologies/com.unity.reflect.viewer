using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
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
        IUISelector m_ClippingToolSelector;

        void Awake()
        {
            foreach (var buttonControl in m_ButtonControls)
            {
                buttonControl.onControlTap.AddListener(OnButtonTap);
            }

            m_ClippingToolSelector = UISelectorFactory.createSelector<SetClippingToolAction.ClippingTool>(ToolStateContext.current, nameof(IToolStateDataProvider.clippingTool),
                data =>
                {
                    var i = 0;
                    foreach (var buttonControl in m_ButtonControls)
                    {
                        var on = i == (int)data;
                        buttonControl.@on = on;
                        i++;
                    }
                });
        }

        void OnDestroy()
        {
            m_ClippingToolSelector?.Dispose();
        }

        void OnButtonTap(BaseEventData eventData)
        {
            var buttonControl = eventData.selectedObject.GetComponent<ButtonControl>();
            if (buttonControl == null)
                return;

            var clippingTool = (SetClippingToolAction.ClippingTool)m_ButtonControls.IndexOf(buttonControl);

            if (m_CloseOnSelect)
            {
                Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
            }

            Dispatcher.Dispatch(SetClippingToolAction.From(clippingTool));
        }
    }
}

