using System;
using System.Collections;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.EventSystems;
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
        DisplayData m_CurrentDisplayData;
        NavigationMode m_NavigationMode;

        float m_ColumnSize;
        float m_CollaboratorsColumnSize;
        float m_RemainingWidth;
        float m_TableWidth;

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
        }

        protected override void OnEnable()
        {
            UIStateManager.stateChanged += UIStateManagerOnStateChanged;
            if (UIStateManager.current != null)
            {
                StartCoroutine(DelayedUpdate(UIStateManager.current.stateData));
            }
        }

        protected override void OnDisable()
        {
            UIStateManager.stateChanged -= UIStateManagerOnStateChanged;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled && UIStateManager.current != null)
            {
                StartCoroutine(DelayedUpdate(UIStateManager.current.stateData));
            }
        }

        void UIStateManagerOnStateChanged(UIStateData uiStateData)
        {
            UpdateLayout(uiStateData);
        }

        void UpdateLayout(UIStateData uiStateData)
        {
            if (gameObject != null && gameObject.activeInHierarchy && uiStateData.activeDialog == DialogType.LandingScreen)
            {
                m_CurrentDisplayData = uiStateData.display;
                m_NavigationMode = uiStateData.navigationState.navigationMode;

                m_CollaboratorsColumnSize = k_CollaboratorRegularColumnSize;
                m_ColumnSize = k_RegularColumnSize;
                m_TableWidth = tableContainer.rect.size.x;
                m_RemainingWidth = m_TableWidth;

                var screenSizeQualifier = m_CurrentDisplayData.screenSizeQualifier;
                if (screenSizeQualifier > ScreenSizeQualifier.Medium && m_NavigationMode != NavigationMode.VR)
                {
                    m_ServerButton.gameObject.SetActive(true);
                    m_OrganizationButton.gameObject.SetActive(true);
                    m_RemainingWidth -= m_CollaboratorsColumnSize + 3 * m_ColumnSize;
                }
                else
                {
                    m_ColumnSize = k_SmallColumnSize;
                    m_OrganizationButton.gameObject.SetActive(false);
                    if (screenSizeQualifier > ScreenSizeQualifier.Small ||
                        m_NavigationMode == NavigationMode.VR)
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

        IEnumerator DelayedUpdate(UIStateData uiStateData)
        {
            yield return new WaitForEndOfFrame();
            UpdateLayout(uiStateData);
        }
    }
}
