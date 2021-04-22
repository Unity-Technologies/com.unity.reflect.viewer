using System;
using Unity.Reflect.Viewer.UI;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible of managing the active toolbar.
    /// </summary>
    public class ActiveToolbarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Reference to the Orbit Sidebar")]
        GameObject m_OrbitSidebar;

        [SerializeField, Tooltip("Reference to the Left Sidebar")]
        GameObject m_LeftSidebar;

        [SerializeField, Tooltip("Reference to the Top Left Sidebar")]
        GameObject m_TopSidebar;

        [SerializeField, Tooltip("Reference to the Info/Debug button")]
        GameObject m_InfoSidebar;

        [SerializeField, Tooltip("Reference to the Fly Sidebar")]
        GameObject m_FlySidebar;

        [SerializeField, Tooltip("Reference to the Walk Sidebar")]
        GameObject m_WalkSidebar;

        [SerializeField, Tooltip("Reference to the AR Sidebar")]
        GameObject m_ARSidebar;

        [SerializeField, Tooltip("Reference to the AR Model Align View Sidebar")]
        GameObject m_ARModelAlignViewSidebar;

        [SerializeField, Tooltip("Reference to the AR Instruction Sidebar")]
        GameObject m_ARInstructionSidebar;

        [SerializeField, Tooltip("Reference to the Sun Study Time Of Day Year Radial")]
        DialogWindow m_TimeOfDayYearRadial;

        [SerializeField, Tooltip("Reference to the Sun Study Altitude Azimuth Radial")]
        DialogWindow m_AltitudeAzimuthRadial;

        [SerializeField, Tooltip("Reference to the AR Scale Radial")]
        DialogWindow m_ARScaleRadial;
#pragma warning restore CS0649

        ToolbarType m_currentActiveToolbar;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_currentActiveToolbar == stateData.activeToolbar)
            {
                return;
            }

            m_OrbitSidebar.SetActive(false);
            m_FlySidebar.SetActive(false);
            m_WalkSidebar.SetActive(false);
            m_ARSidebar.SetActive(false);
            m_ARModelAlignViewSidebar.SetActive(false);
            m_ARInstructionSidebar.SetActive(false);
            m_TimeOfDayYearRadial.Close();
            m_AltitudeAzimuthRadial.Close();
            m_ARScaleRadial.Close();

            switch (stateData.activeToolbar)
            {
                case ToolbarType.FlySidebar:
                    m_FlySidebar.SetActive(true);
                    break;
                case ToolbarType.WalkSidebar:
                    m_WalkSidebar.SetActive(true);
                    break;
                case ToolbarType.ARSidebar:
                    m_ARSidebar.SetActive(true);
                    break;
                case ToolbarType.ARModelAlignSidebar:
                    m_ARModelAlignViewSidebar.SetActive(true);
                    break;
                case ToolbarType.ARInstructionSidebar:
                    m_ARInstructionSidebar.SetActive(true);
                    break;
                case ToolbarType.TimeOfDayYearDial:
                    m_TimeOfDayYearRadial.Open();
                    break;
                case ToolbarType.AltitudeAzimuthDial:
                    m_AltitudeAzimuthRadial.Open();
                    break;
                case ToolbarType.ARScaleDial:
                    m_ARScaleRadial.Open();
                    break;
                case ToolbarType.NoSidebar:
                    m_OrbitSidebar.SetActive(false);
                    m_LeftSidebar.SetActive(false);
                    m_TopSidebar.SetActive(false);
                    m_InfoSidebar.SetActive(false);
                    break;
                default:
                    m_OrbitSidebar.SetActive(true);
                    m_LeftSidebar.SetActive(true);
                    m_TopSidebar.SetActive(true);
                    m_InfoSidebar.SetActive(true);
                    break;
            }

            m_currentActiveToolbar = stateData.activeToolbar;
        }
    }
}
