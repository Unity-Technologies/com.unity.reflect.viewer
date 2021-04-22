using UnityEngine;
using UnityEngine.Reflect.Utils;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class UserUIController : MonoBehaviour
    {
        [SerializeField, Tooltip("Add images that you want add the user color [Optional]")]
        public Image[] m_UserColoredImages = null;
        [SerializeField, Tooltip("User's Initials [Optional]")]
        public TMPro.TMP_Text m_Initials = null;

        public string MatchmakerId { get; protected set;}
        public static Color initialsTextColorAvatarSelected { get; }  = new Color32(59, 59, 59, 255);
        public static Color initialsTextColorRegular  { get; } = Color.white;
        public static Color bubbleColorSelected { get; } = Color.white;
        public static Color bubbleColorRegular { get; } = new Color32(61, 61, 61, 255);

        protected Color m_UserColor;

        public void Clear()
        {
            MatchmakerId = "";
            m_Initials.text = string.Empty;
            m_Initials.color = initialsTextColorRegular;
        }

        public void UpdateUser(string matchmakerId, bool forceUpdate = false)
        {
            if (MatchmakerId != matchmakerId || forceUpdate)
            {
                MatchmakerId = matchmakerId;
                var identity = UIStateManager.current.GetUserIdentityFromSession(MatchmakerId);
                if (identity != default && UIStateManager.current.stateData.colorPalette.Length > identity.colorIndex)
                {
                    m_UserColor = identity.colorIndex == -1 ? bubbleColorRegular : UIStateManager.current.stateData.colorPalette[identity.colorIndex];
                    UpdateUser(identity);
                }
            }

            UpdateUI();
        }

        protected virtual void UpdateUI()
        {
            ColorImages(m_UserColor);
            if (m_Initials != null)
            {
                m_Initials.color = initialsTextColorRegular;
            }
        }

        protected virtual void UpdateUser(UserIdentity identity)
        {
            if (!ReferenceEquals(m_Initials, null) && !string.IsNullOrEmpty(identity.fullName))
            {
                m_Initials.text = UIUtils.CreateInitialsFor(identity.fullName);
            }
        }

        protected void ColorImages(Color color)
        {
            foreach (var userColoredImage in m_UserColoredImages)
            {
                userColoredImage.color = color;
            }
        }

        protected bool IsFollowing()
        {
            return MatchmakerId != null && MatchmakerId == UIStateManager.current.stateData.toolState.followUserTool.userId;
        }

        protected bool IsMuted()
        {
            var user = UIStateManager.current.roomConnectionStateData.users.Find(data => data.matchmakerId == MatchmakerId);
            return  user.voiceStateData.isLocallyMuted || user.voiceStateData.isServerMuted;
        }

        protected bool IsLocallyMuted()
        {
            var user = UIStateManager.current.roomConnectionStateData.users.Find(data => data.matchmakerId == MatchmakerId);
            return user.voiceStateData.isLocallyMuted;
        }
    }
}
