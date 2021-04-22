using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible for managing the selection of the type of orbit tool
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class InfoSelectUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Does dialog closes on selection.")]
        bool m_CloseOnSelect;

        [SerializeField]
        Button m_StatsButton;

        [SerializeField]
        Button m_DebugButton;

#pragma warning restore CS0649


        void Awake()
        {
            m_StatsButton.onClick.AddListener(OnStatsButtonClicked);
            m_DebugButton.onClick.AddListener(OnDebugButtonClicked);
        }

        void OnStatsButtonClicked()
        {
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.infoType = InfoType.Info;
            SetToolState(toolState);
        }

        void OnDebugButtonClicked()
        {
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.infoType = InfoType.Debug;
            SetToolState(toolState);
        }

        void SetToolState(ToolState toolState)
        {
            if (m_CloseOnSelect)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
            }
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }
    }
}
