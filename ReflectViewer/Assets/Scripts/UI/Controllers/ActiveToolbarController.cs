using System;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible of managing the active toolbar.
    /// </summary>
    public class ActiveToolbarController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Reference to the Left Sidebar")]
        GameObject m_LeftSidebar;

        [SerializeField, Tooltip("Reference to the Top Left Sidebar")]
        GameObject m_TopSidebar;

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

        [SerializeField, Tooltip("Reference to the AR Scale Radial")]
        DialogWindow m_ARScaleRadial;

        [SerializeField, Tooltip("Reference to the Navigation button")]
        GameObject m_NavigationSidebar;
#pragma warning restore CS0649

        IDisposable m_SelectorToDispose;

        void Awake()
        {
            m_SelectorToDispose = UISelectorFactory.createSelector<SetActiveToolBarAction.ToolbarType>(UIStateContext.current, nameof(IToolBarDataProvider.activeToolbar), OnActiveToolBarChanged);
        }

        void OnDestroy()
        {
            m_SelectorToDispose?.Dispose();
        }

        void OnActiveToolBarChanged(SetActiveToolBarAction.ToolbarType newData)
        {
            m_FlySidebar.SetActive(false);
            m_WalkSidebar.SetActive(false);
            m_ARSidebar.SetActive(false);
            m_ARModelAlignViewSidebar.SetActive(false);
            m_ARInstructionSidebar.SetActive(false);
            m_ARScaleRadial.Close();

            switch (newData)
            {
                case SetActiveToolBarAction.ToolbarType.FlySidebar:
                    m_FlySidebar.SetActive(true);
                    break;
                case SetActiveToolBarAction.ToolbarType.WalkSidebar:
                    m_WalkSidebar.SetActive(true);
                    break;
                case SetActiveToolBarAction.ToolbarType.ARSidebar:
                    m_ARSidebar.SetActive(true);
                    break;
                case SetActiveToolBarAction.ToolbarType.ARModelAlignSidebar:
                    m_ARModelAlignViewSidebar.SetActive(true);
                    break;
                case SetActiveToolBarAction.ToolbarType.ARInstructionSidebar:
                    m_ARInstructionSidebar.SetActive(true);
                    break;
                case SetActiveToolBarAction.ToolbarType.ARScaleDial:
                    m_ARScaleRadial.Open();
                    break;
                case SetActiveToolBarAction.ToolbarType.TopSidebar:
                    m_TopSidebar.SetActive(true);
                    break;
                case SetActiveToolBarAction.ToolbarType.NavigationSidebar:
                    m_NavigationSidebar.SetActive(true);
                    break;
                case SetActiveToolBarAction.ToolbarType.NoSidebar:
                    m_LeftSidebar.SetActive(false);
                    m_NavigationSidebar.SetActive(false);
                    m_TopSidebar.SetActive(false);
                    break;
                case SetActiveToolBarAction.ToolbarType.LandingScreen:
                    m_TopSidebar.SetActive(true);
                    break;
                default:
                    m_LeftSidebar.SetActive(true);
                    m_NavigationSidebar.SetActive(true);
                    m_TopSidebar.SetActive(true);
                    break;
            }
        }
    }
}
