using System;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public class LeftSideBarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_FilterButton;

        [SerializeField]
        ToolButton m_SceneSettingButton;

        [SerializeField]
        ToolButton m_SunstudyButton;
#pragma warning restore CS0649

        bool m_ToolbarsEnabled;
        SettingsToolStateData? m_dataToolButtonStateData;
        NavigationMode? m_CurrentNavigationMode;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_ToolbarsEnabled != data.toolbarsEnabled)
            {
                m_ToolbarsEnabled = data.toolbarsEnabled;
                UpdateButtons(data.settingsToolStateData);
            }

            if (m_dataToolButtonStateData != data.settingsToolStateData)
            {
                m_dataToolButtonStateData = data.settingsToolStateData;
                UpdateButtons(data.settingsToolStateData);
            }
        }

        void UpdateButtons(SettingsToolStateData dataSettingsToolStateData)
        {
            m_FilterButton.button.interactable = m_ToolbarsEnabled && dataSettingsToolStateData.bimFilterEnabled;
            m_SceneSettingButton.button.interactable = m_ToolbarsEnabled && dataSettingsToolStateData.sceneOptionEnabled;
            m_SunstudyButton.button.interactable = m_ToolbarsEnabled && dataSettingsToolStateData.sunStudyEnabled;
        }
    }
}
