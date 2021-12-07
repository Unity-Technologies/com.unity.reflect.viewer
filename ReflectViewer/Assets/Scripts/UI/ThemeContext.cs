using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ThemeContext : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        List<Image> m_Backgrounds;
        [SerializeField]
        List<Image> m_SelectionBackgrounds;
        [SerializeField]
        List<Image> m_EnableBackgrounds;
        [SerializeField]
        List<LayoutElement> m_LayoutElements;
#pragma warning restore 0649

        public List<Image> Backgrounds => m_Backgrounds;
        public List<Image> SelectionBackgrounds => m_SelectionBackgrounds;
        public List<Image> EnableBackgrounds => m_EnableBackgrounds;
        public List<LayoutElement> LayoutElements => m_LayoutElements;

        //TODO Add other themed elements here (i.e. Texts, Highlight)
    }
}
