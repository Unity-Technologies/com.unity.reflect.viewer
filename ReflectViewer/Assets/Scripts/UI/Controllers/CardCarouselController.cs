using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class CardCarouselController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_Button;

        [SerializeField]
        ARMode m_ARMode;
#pragma warning restore CS0649

        public event Action<ARMode> buttonClicked;

        void Start()
        {
            m_Button.onClick.AddListener(OnButtonClicked);
        }

        void OnButtonClicked()
        {
            buttonClicked?.Invoke(m_ARMode);
        }
    }
}
