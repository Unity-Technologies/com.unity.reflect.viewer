using Unity.SpatialFramework.Utils;
using Unity.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.SpatialFramework.UI
{
    public class BooleanToggleAction : MonoBehaviour, IPointerClickHandler
    {
#pragma warning disable 649
        [SerializeField]
        Image m_FillImage;

        [SerializeField]
        RectTransform m_Knob;

        [SerializeField]
        Color m_ColorOff;

        [SerializeField]
        Color m_ColorOn;
#pragma warning restore 649

        Toggle m_Toggle;

        Coroutine m_ColorFade;
        Coroutine m_LocalMove;

        void Awake()
        {
            m_Toggle = gameObject.GetComponent<Toggle>();
        }

        public void HoverEnter() { }

        public void HoverExit() { }

        public void Press()
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_Toggle.isOn)
            {
                this.RestartCoroutine(ref m_ColorFade, m_FillImage.TweenColor(m_ColorOn, .25f));
                this.RestartCoroutine(ref m_LocalMove, m_Knob.TweenMoveX(12f, .25f));
            }
            else
            {
                this.RestartCoroutine(ref m_ColorFade, m_FillImage.TweenColor(m_ColorOff, .25f));
                this.RestartCoroutine(ref m_LocalMove, m_Knob.TweenMoveX(-12f, .25f));
            }
        }
    }
}
