using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ExtraButtonListItemController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        Image m_Icon;
        [SerializeField]
        TMP_Text m_Label;
        [SerializeField]
        Button m_Button;
#pragma warning restore CS0649

        public string Name
        {
            get { return m_Label.text; }
            set { m_Label.text = value; }
        }

        public Sprite Icon
        {
            get { return m_Icon.sprite; }
            set { m_Icon.sprite = value; }
        }

        public Action OnClick { get; set; }

        void Awake()
        {
            m_Button.onClick.AddListener(OnButtonClick);
        }

        void OnButtonClick()
        {
            OnClick?.Invoke();
        }
    }
}