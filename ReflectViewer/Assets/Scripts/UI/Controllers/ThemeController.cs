using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public class ThemeSettings
    {
        public string Name;
        public Color BackgroundColor;
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
                        foreach (var background in context.Backgrounds)
                        {
                            background.color = settings.BackgroundColor;
                        }
                    }
                }
            }
        }
    }
}
