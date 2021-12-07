using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ProjectListColumnController : UIBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        RectTransform m_NameButton;
        [SerializeField]
        RectTransform m_ServerButton;
        [SerializeField]
        RectTransform m_OrganizationButton;
        [SerializeField]
        RectTransform m_LastUpdatedButton;
        [SerializeField]
        RectTransform m_CollaboratorsButton;

        public RectTransform tableContainer;

#pragma warning restore CS0649
        SetNavigationModeAction.NavigationMode m_NavigationMode;

        float m_ColumnSize;
        float m_CollaboratorsColumnSize;
        float m_RemainingWidth;
        float m_TableWidth;

        List<IDisposable> m_Disposables = new List<IDisposable>();
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogGetter;
        IUISelector<SetDisplayAction.ScreenSizeQualifier> m_ScreenSizeQualifierGetter;
        IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeGetter;

        const int k_CollaboratorSmallColumnSize = 212;
        const int k_CollaboratorRegularColumnSize = 260;
        const int k_RegularColumnSize = 180;
        const int k_SmallColumnSize = 108;

        protected override void Awake()
        {
            if (tableContainer == null)
            {
                tableContainer = transform.parent.GetComponent<RectTransform>();
            }
            m_ScreenSizeQualifierGetter = UISelectorFactory.createSelector<SetDisplayAction.ScreenSizeQualifier>(UIStateContext.current, nameof(IUIStateDisplayProvider<DisplayData>.display) + "." + nameof(IDisplayDataProvider.screenSizeQualifier));
        }

        protected override void OnEnable()
        {
            m_Disposables.Add(m_ActiveDialogGetter = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog),
                data =>
                {
                    UpdateLayout();
                }));
            m_Disposables.Add(m_NavigationModeGetter = UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode),
                data =>
                {
                    UpdateLayout();
                }));
            StartCoroutine(DelayedUpdate());
        }

        protected override void OnDisable()
        {
            foreach (var disposable in m_Disposables)
            {
                disposable.Dispose();
            }
            m_Disposables.Clear();
        }

        protected override void OnDestroy()
        {
            m_ScreenSizeQualifierGetter?.Dispose();
            base.OnDestroy();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(DelayedUpdate());
            }
        }

        void UpdateLayout()
        {
            if (gameObject != null && gameObject.activeInHierarchy && m_ActiveDialogGetter != null && m_NavigationModeGetter != null
                && m_ActiveDialogGetter.GetValue() == OpenDialogAction.DialogType.LandingScreen)
            {
                m_NavigationMode = m_NavigationModeGetter.GetValue();

                m_CollaboratorsColumnSize = k_CollaboratorRegularColumnSize;
                m_ColumnSize = k_RegularColumnSize;
                m_TableWidth = tableContainer.rect.size.x;
                m_RemainingWidth = m_TableWidth;

                var screenSizeQualifier = m_ScreenSizeQualifierGetter.GetValue();
                if (screenSizeQualifier > SetDisplayAction.ScreenSizeQualifier.Medium && m_NavigationMode != SetNavigationModeAction.NavigationMode.VR)
                {
                    m_ServerButton.gameObject.SetActive(true);
                    m_OrganizationButton.gameObject.SetActive(true);
                    m_RemainingWidth -= m_CollaboratorsColumnSize + 3 * m_ColumnSize;
                }
                else
                {
                    m_ColumnSize = k_SmallColumnSize;
                    m_OrganizationButton.gameObject.SetActive(false);
                    if (screenSizeQualifier > SetDisplayAction.ScreenSizeQualifier.Small ||
                        m_NavigationMode == SetNavigationModeAction.NavigationMode.VR)
                    {
                        m_RemainingWidth -= m_CollaboratorsColumnSize + 2 * m_ColumnSize;
                        m_ServerButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        m_CollaboratorsColumnSize = k_CollaboratorSmallColumnSize;
                        m_RemainingWidth -= m_CollaboratorsColumnSize + m_ColumnSize;
                        m_ServerButton.gameObject.SetActive(false);
                    }
                }

                m_LastUpdatedButton.sizeDelta = new Vector2(m_ColumnSize, m_LastUpdatedButton.sizeDelta.y);
                m_OrganizationButton.sizeDelta = new Vector2(m_ColumnSize, m_OrganizationButton.sizeDelta.y);
                m_ServerButton.sizeDelta = new Vector2(m_ColumnSize, m_ServerButton.sizeDelta.y);
                m_CollaboratorsButton.sizeDelta = new Vector2(m_CollaboratorsColumnSize, m_CollaboratorsButton.sizeDelta.y);
                m_NameButton.sizeDelta = new Vector2(m_RemainingWidth, m_NameButton.sizeDelta.y);
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            }
        }

        IEnumerator DelayedUpdate()
        {
            yield return new WaitForEndOfFrame();
            UpdateLayout();
        }
    }
}
