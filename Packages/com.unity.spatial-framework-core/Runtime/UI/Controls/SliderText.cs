using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Unity.SpatialFramework.UI
{
    public class SliderText : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        TMP_Text m_Text;

        [SerializeField]
        bool m_RoundInt;
#pragma warning restore 649

        Slider m_Slider;

        void Awake()
        {
            m_Slider = transform.GetComponent<Slider>();
            m_Text.text = (Mathf.Round(m_Slider.value * 100f) / 100f).ToString();
        }

        public void SetSliderValue(float sliderValue)
        {
            if (m_RoundInt)
            {
                m_Text.text = Mathf.Round(sliderValue).ToString();
            }
            else
            {
                m_Text.text = (Mathf.Round(sliderValue * 100f) / 100f).ToString();
            }
        }
    }
}
