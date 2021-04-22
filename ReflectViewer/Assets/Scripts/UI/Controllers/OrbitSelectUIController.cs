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
    public class OrbitSelectUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Does dialog closes on selection.")]
        bool m_CloseOnSelect;

        [SerializeField]
        Button m_OrbitButton;

        [SerializeField]
        Button m_PanButton;

        [SerializeField]
        Button m_ZoomButton;

#pragma warning restore CS0649


        void Awake()
        {
            m_OrbitButton.onClick.AddListener(OnOrbitButtonClicked);
            m_PanButton.onClick.AddListener(OnPanButtonClicked);
            m_ZoomButton.onClick.AddListener(OnZoomButtonClicked);
        }

        void OnZoomButtonClicked()
        {
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.ZoomTool;
            SetToolState(toolState);
        }

        void OnPanButtonClicked()
        {
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.PanTool;
            SetToolState(toolState);
        }

        void OnOrbitButtonClicked()
        {
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.OrbitTool;
            toolState.orbitType = OrbitType.OrbitAtPoint;
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
