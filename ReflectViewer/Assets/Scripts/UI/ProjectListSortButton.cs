using System;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class ProjectListSortButton : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Image m_ArrowImage;

        [Space(8)]
        [SerializeField]
        ProjectSortField m_SortField;
#pragma warning restore CS0649
        Button m_Button;
        IUISelector<ProjectListSortData> m_ProjectSortDataSelector;

        static Quaternion arrowUpRotation { get; } = Quaternion.Euler(new Vector3(0f, 0f, 180f));
        static Quaternion arrowDownRotation { get; } = Quaternion.Euler(new Vector3(0f, 0f, 0f));

        void Awake()
        {
            m_Button = GetComponent<Button>();

            m_ProjectSortDataSelector = UISelectorFactory.createSelector<ProjectListSortData>(ProjectContext.current, nameof(IProjectSortDataProvider.projectSortData), OnProjectSortDataChanged);
        }

        void OnDestroy()
        {
            m_ProjectSortDataSelector?.Dispose();
        }

        void OnProjectSortDataChanged(ProjectListSortData newData)
        {
            UpdateHeader();
        }

        void Start()
        {
            m_Button.onClick.AddListener(OnSortButtonClicked);
            UpdateHeader();
        }

        void UpdateHeader()
        {
            if (m_ProjectSortDataSelector != null)
            {
                m_ArrowImage.enabled = m_ProjectSortDataSelector.GetValue().sortField == m_SortField;
                if (m_ArrowImage.enabled)
                {
                    m_ArrowImage.transform.rotation = m_ProjectSortDataSelector.GetValue().method == ProjectSortMethod.Ascending ? arrowDownRotation : arrowUpRotation;
                }
            }
        }

        void OnSortButtonClicked()
        {
            var sortData = m_ProjectSortDataSelector.GetValue();
            sortData.method = (sortData.sortField != m_SortField || sortData.method == ProjectSortMethod.Descending) ? ProjectSortMethod.Ascending : ProjectSortMethod.Descending;
            sortData.sortField = m_SortField;
            Dispatcher.Dispatch(SetProjectSortMethodAction.From(sortData));
        }
    }
}
