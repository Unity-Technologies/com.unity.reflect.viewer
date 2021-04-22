using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class SyncToggleButton : MonoBehaviour
    {
        [SerializeField]
        Sprite m_SyncEnabledSprite;
        [SerializeField]
        Sprite m_SyncDisabledSprite;

        Button m_Button;

        bool? m_CachedSyndEnabled;

        void Awake()
        {
            m_Button = GetComponent<Button>();
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void Start()
        {
            m_Button.onClick.AddListener(OnButtonClick);
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_Button.interactable = data.toolbarsEnabled;

            if (m_CachedSyndEnabled != data.syncEnabled)
            {
                m_Button.image.sprite = data.syncEnabled ? m_SyncEnabledSprite : m_SyncDisabledSprite;
                m_CachedSyndEnabled = data.syncEnabled;
            }

            m_Button.transform.parent.gameObject.SetActive(data.activeDialog != DialogType.LandingScreen);
        }

        void OnButtonClick()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.Sync)) return;

            var enabled = !UIStateManager.current.stateData.syncEnabled;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSync, enabled));
        }
    }
}
