using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemeContext : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    List<Image> m_Backgrounds;
#pragma warning restore 0649

    public List<Image> Backgrounds => m_Backgrounds;

    //TODO Add other themed elements here (i.e. Texts, Highlight)
}
