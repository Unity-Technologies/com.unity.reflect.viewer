using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core.Actions;
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
            SetToolState(SetInfoTypeAction.InfoType.Info);
        }

        void OnDebugButtonClicked()
        {
            SetToolState(SetInfoTypeAction.InfoType.Debug);
        }

        void SetToolState(SetInfoTypeAction.InfoType infoType)
        {
            if (m_CloseOnSelect)
            {
                Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
            }
            Dispatcher.Dispatch(SetInfoTypeAction.From(infoType));
        }
    }
}
