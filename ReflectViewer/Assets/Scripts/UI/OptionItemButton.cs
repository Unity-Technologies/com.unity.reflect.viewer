using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public enum RegionOption
    {
        None,
        Default,
        China,
    }

    public enum CloudOption
    {
        None,
        Default,
        Production,
        Local,
        Staging,
        Test,
        Other
    }

    public class OptionItemButton : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_Button;

        [SerializeField]
        Image m_Image;

        [SerializeField]
        TextMeshProUGUI m_Label;

        [SerializeField]
        RegionOption m_RegionOption;
        [SerializeField]
        CloudOption m_CloudOption;
#pragma warning restore CS0649

        public TextMeshProUGUI label => m_Label;

        public RegionOption regionOption => m_RegionOption;
        public CloudOption cloudOption => m_CloudOption;

        public bool selected => m_Selected;

        public event Action buttonClicked;

        public event Action<RegionOption> regionButtonClicked;
        public event Action<CloudOption> cloudButtonClicked;


        static Color itemDefaultColor { get; } = new Color32(46, 46, 46, 255);

        static Color itemSelectedColor { get; } = new Color32(32, 150, 243, 255);

        bool m_Selected;

        void Awake()
        {
            m_Button.onClick.AddListener(OnButtonClicked);
        }


        public void SelectButton(bool select)
        {
            m_Selected = select;
            if (select)
            {
                m_Image.color = itemSelectedColor;
            }
            else
            {
                m_Image.color = itemDefaultColor;
            }

        }

        void OnButtonClicked()
        {
            if (m_RegionOption != RegionOption.None)
            {
                regionButtonClicked?.Invoke(m_RegionOption);
            }
            else if (m_CloudOption != CloudOption.None)
            {
                cloudButtonClicked?.Invoke(m_CloudOption);
            }
        }
    }
}
