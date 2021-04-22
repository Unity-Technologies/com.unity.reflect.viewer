using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.UI;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class MicrophoneButtonController : MonoBehaviour
    {
        [SerializeField, Tooltip("Microphone On Image [Optional]")]
        public GameObject m_MicToggleOnImage = null;
        [SerializeField, Tooltip("Microphone Off Image [Optional]")]
        public GameObject m_MicToggleOffImage = null;
        [SerializeField, Tooltip("Microphone volume [Optional]")]
        public Image m_MicLevel = null;

        Button m_Button;

        bool m_CachedMicEnabled = false;

        void Awake()
        {
            m_Button = GetComponent<Button>();
            UIStateManager.roomConnectionStateChanged += OnRoomDataChanged;
        }

        void Start()
        {
            m_Button.onClick.AddListener(OnButtonClick);
            UpdateIcon();
            HasPermission();
        }

        bool HasPermission()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
                return false;
            }
#endif
            return true;
        }

        void OnRoomDataChanged(RoomConnectionStateData obj)
        {
            UpdateIcon();
        }

        void UpdateIcon()
        {
            var voiceData = UIStateManager.current.roomConnectionStateData.localUser.voiceStateData;
            var muted = voiceData.isLocallyMuted || voiceData.isServerMuted;
            m_CachedMicEnabled = !muted;
            m_MicToggleOnImage.SetActive(m_CachedMicEnabled);
            m_MicToggleOffImage.SetActive(!m_CachedMicEnabled);
            if (m_CachedMicEnabled && m_MicLevel != null)
            {
                m_MicLevel.fillAmount = voiceData.micVolume;
            }
        }

        void OnButtonClick()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.Microphone)) return;

            if (HasPermission())
            {
                var matchmakerId = UIStateManager.current.roomConnectionStateData.localUser.matchmakerId;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ToggleUserMicrophone, matchmakerId));
            }
        }
    }
}
