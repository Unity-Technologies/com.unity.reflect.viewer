using System;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
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
        ProjectListSortData m_CurrentProjectSortData;

        static Quaternion arrowUpRotation { get; } = Quaternion.Euler(new Vector3(0f, 0f, 180f));
        static Quaternion arrowDownRotation { get; } = Quaternion.Euler(new Vector3(0f, 0f, 0f));

        void Awake()
        {
            m_Button = GetComponent<Button>();
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;
        }

        void Start()
        {
            m_CurrentProjectSortData = UIStateManager.current.projectStateData.projectSortData;
            m_Button.onClick.AddListener(OnSortButtonClicked);
            UpdateHeader();
        }

        void UpdateHeader()
        {
            m_ArrowImage.enabled = m_CurrentProjectSortData.sortField == m_SortField;
            if (m_ArrowImage.enabled)
            {
                m_ArrowImage.transform.rotation = m_CurrentProjectSortData.method == ProjectSortMethod.Ascending ? arrowDownRotation : arrowUpRotation;
            }
        }

        void OnSortButtonClicked()
        {
            ProjectListSortData sortData = UIStateManager.current.projectStateData.projectSortData;
            sortData.method = (sortData.sortField != m_SortField || sortData.method == ProjectSortMethod.Descending) ? ProjectSortMethod.Ascending : ProjectSortMethod.Descending;
            sortData.sortField = m_SortField;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetProjectSortMethod, sortData));
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (data.projectSortData != m_CurrentProjectSortData)
            {
                m_CurrentProjectSortData = data.projectSortData;
                UpdateHeader();
            }
        }
    }
}
