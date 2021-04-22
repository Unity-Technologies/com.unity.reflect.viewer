using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public class ThemeSettings
    {
        public string Name;
        public Color BackgroundColor;
        public Sprite SelectionBackground;
        public bool IsBackgroundEnable;
        public float LayoutSize;
    }

    public class ThemeController : MonoBehaviour
    {
        public const string k_VROpaque = "VROpaque";
        public const string k_Default = "Default";

        string m_CurrentThemeName = k_Default;

#pragma warning disable CS0649
        [SerializeField]
        List<ThemeSettings> m_ThemeSettings;

#pragma warning restore CS0649

        List<ThemeContext> m_ThemeContexts;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_ThemeContexts = GetComponentsInChildren<ThemeContext>(true).ToList();
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (!string.IsNullOrEmpty(data.themeName) && data.themeName != m_CurrentThemeName)
            {
                m_CurrentThemeName = data.themeName;
                ThemeSettings settings = m_ThemeSettings.FirstOrDefault(s => s.Name == data.themeName);
                if (settings != null)
                {
                    foreach (var context in m_ThemeContexts)
                    {
                        foreach (var background in context.EnableBackgrounds)
                        {
                            background.enabled = settings.IsBackgroundEnable;
                        }

                        foreach (var background in context.Backgrounds)
                        {
                            background.color = settings.BackgroundColor;
                        }

                        foreach (var selectionBackground in context.SelectionBackgrounds)
                        {
                            selectionBackground.sprite = settings.SelectionBackground;
                        }

                        foreach (var layout in context.LayoutElements)
                        {
                            layout.minWidth = layout.minHeight = settings.LayoutSize;
                        }
                    }
                }
            }
        }
    }
}
