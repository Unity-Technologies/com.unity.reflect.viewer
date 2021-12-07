using System;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class FilterListItem : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_ItemButton;

        [SerializeField]
        ButtonControl m_VisibleButton;

        [SerializeField]
        TextMeshProUGUI m_Text;

        [SerializeField]
        Image m_ItemBgImage;

        [SerializeField]
        Image m_ItemSelectImage;
#pragma warning restore CS0649

        string m_GroupKey;
        string m_FilterKey;

        public event Action<string, string> listItemClicked;
        public event Action<string, string, bool> visibleButtonClicked;

        public string groupKey => m_GroupKey;

        public string filterKey => m_FilterKey;

        static Color itemSelectedColor { get; } = new Color32(41, 41, 41, 127);
        static Color itemColor { get; } = new Color32(24, 24, 24, 255);
        static Color selectItemIndicatorColor { get; } = new Color32(32, 150, 243, 255);

        void Awake()
        {
            m_ItemButton.onClick.AddListener(OnItemButtonClicked);
            m_VisibleButton.onControlTap.AddListener(OnVisibleButtonTapped);
        }

        public void InitItem(string groupKey, string filterKey, bool visible, bool highlight)
        {
            m_GroupKey = groupKey;
            m_Text.text = m_FilterKey = filterKey;
            m_VisibleButton.on = visible;
            SetHighlight(highlight);
        }

        public void SetVisible(bool visible)
        {
            m_VisibleButton.on = visible;
        }

        public void SetHighlight(bool highlight)
        {
            m_Text.color = highlight ? UIConfig.propertyTextSelectedColor : UIConfig.propertyTextBaseColor;
            m_Text.fontStyle = highlight ? FontStyles.Bold : FontStyles.Normal;
            m_ItemBgImage.color = highlight ? itemSelectedColor : itemColor;
            m_ItemSelectImage.color = highlight ? selectItemIndicatorColor : itemColor;
        }

        void OnItemButtonClicked()
        {
            listItemClicked?.Invoke(m_GroupKey, m_FilterKey);
        }

        void OnVisibleButtonTapped(BaseEventData eventData)
        {
            var setVisible = !m_VisibleButton.on;
            visibleButtonClicked?.Invoke(m_GroupKey, m_FilterKey, setVisible);
        }
    }
}
