using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
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

    public class ThemeController: MonoBehaviour
    {
        public const string k_VROpaque = "VROpaque";
        public const string k_Default = "Default";

#pragma warning disable CS0649
        [SerializeField]
        List<ThemeSettings> m_ThemeSettings;

#pragma warning restore CS0649

        List<ThemeContext> m_ThemeContexts;
        IDisposable m_ThemeNameSelector;

        void OnDestroy()
        {
            m_ThemeNameSelector?.Dispose();
        }

        void Awake()
        {
            m_ThemeNameSelector = UISelectorFactory.createSelector<string>(UIStateContext.current, nameof(IUIStateDataProvider.themeName), OnThemeNameChanged);

            m_ThemeContexts = GetComponentsInChildren<ThemeContext>(true).ToList();
        }

        void OnThemeNameChanged(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                ThemeSettings settings = m_ThemeSettings.FirstOrDefault(s => s.Name == data);
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
