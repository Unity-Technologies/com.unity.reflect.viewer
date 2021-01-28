using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Select new active project, download projects, manage projects
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class FiltersUIController : MonoBehaviour
    {
        public const string noCategoryOptionName = "No Category";
        private static readonly new TMP_Dropdown.OptionData k_NoCategoryOption = new TMP_Dropdown.OptionData(noCategoryOptionName);

#pragma warning disable CS0649
        [SerializeField]
        Button m_DialogButton;

        [SerializeField, Tooltip("Reference to the button prefab.")]
        FilterListItem m_FilterListItemPrefab;

        [SerializeField]
        Transform m_ParentTransform;

        [SerializeField]
        TMP_Dropdown m_FilterGroupDropdown;

        [SerializeField]
        TextMeshProUGUI m_NoDataText;

        [SerializeField]
        GameObject m_DropdownMask;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        Image m_DialogButtonImage;
        Dictionary<ButtonControl, int> m_ProjectDictionary;

        Stack<FilterListItem> m_FilterListItemPool = new Stack<FilterListItem>();
        List<FilterListItem> m_ActiveFilterListItem = new List<FilterListItem>();

        List<FilterItemInfo> m_CurrentFilterKeys;
        List<string> m_CurrentFilterGroupList;
        FilterItemInfo m_LastChangedFilterItem;
        HighlightFilterInfo m_CurrentHighlightFilter;
        string m_CurrentFilterGroup;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
            m_DialogWindow = GetComponent<DialogWindow>();
            m_FilterGroupDropdown.onValueChanged.AddListener(OnFilterGroupChanged);
        }

        void Start()
        {
            m_DialogButton.onClick.AddListener(OnDialogButtonClicked);
        }

        void CreateFilterListItem(FilterItemInfo filterItemInfo)
        {
            FilterListItem filterListItem;
            if (m_FilterListItemPool.Count > 0)
            {
                filterListItem = m_FilterListItemPool.Pop();
            }
            else
            {
                filterListItem = Instantiate(m_FilterListItemPrefab, m_ParentTransform);
                filterListItem.visibleButtonClicked += OnVisibleButtonClicked;
                filterListItem.listItemClicked += OnListItemClicked;
            }

            filterListItem.InitItem(filterItemInfo.groupKey, filterItemInfo.filterKey, filterItemInfo.visible,
                filterItemInfo.highlight);
            filterListItem.gameObject.SetActive(true);
            filterListItem.transform.SetAsLastSibling();
            m_ActiveFilterListItem.Add(filterListItem);
        }

        void ClearFilterList()
        {
            foreach (var filterListItem in m_ActiveFilterListItem)
            {
                filterListItem.gameObject.SetActive(false);
                m_FilterListItemPool.Push(filterListItem);
            }

            m_ActiveFilterListItem.Clear();
        }

        void OnVisibleButtonClicked(string groupKey, string filterKey, bool visible)
        {
            var filterItemInfo = new FilterItemInfo
            {
                groupKey = groupKey,
                filterKey = filterKey,
                visible = visible
            };
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetVisibleFilter,
                filterItemInfo));
        }

        void OnListItemClicked(string groupKey, string filterKey)
        {
            var highlightFilterInfo = new HighlightFilterInfo
            {
                groupKey = groupKey,
                filterKey = filterKey
            };
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetHighlightFilter,
                highlightFilterInfo));
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_DialogButtonImage.enabled = data.activeDialog == DialogType.Filters;
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (!EnumerableExtension.SafeSequenceEquals(data.filterGroupList, m_CurrentFilterGroupList))
            {
                // fill filter group Dropdown
                m_FilterGroupDropdown.options.Clear();

                if (data.filterGroupList.Count == 0)
                {
                    // show no data
                    ClearFilterList();
                    m_FilterGroupDropdown.interactable = false;
                    m_FilterGroupDropdown.options.Add(k_NoCategoryOption);
                    m_NoDataText.gameObject.SetActive(true);
                    m_DropdownMask.SetActive(true);
                }
                else
                {
                    m_FilterGroupDropdown.interactable = true;
                    m_NoDataText.gameObject.SetActive(false);
                    m_DropdownMask.SetActive(false);

                    var filterGroupList = data.filterGroupList;
                    foreach (string group in filterGroupList)
                    {
                        m_FilterGroupDropdown.options.Add(new TMP_Dropdown.OptionData(group));
                    }

                    // default select index = 0,
                    StartCoroutine(SetDefaultGroup());
                }
                m_CurrentFilterGroupList = data.filterGroupList;

            }

            if (!EnumerableExtension.SafeSequenceEquals(data.filterItemInfos, m_CurrentFilterKeys))
            {
                ClearFilterList();
                foreach (var filterItemInfo in data.filterItemInfos)
                {
                    CreateFilterListItem(filterItemInfo);
                }

                m_CurrentFilterKeys = data.filterItemInfos;
            }

            if (data.lastChangedFilterItem != m_LastChangedFilterItem)
            {
                var filterItemInfo = data.lastChangedFilterItem;
                var filterListItem = m_ActiveFilterListItem.SingleOrDefault(e =>
                    e.groupKey == filterItemInfo.groupKey && e.filterKey == filterItemInfo.filterKey);

                if (filterListItem != null)
                    filterListItem.SetVisible(filterItemInfo.visible);

                m_LastChangedFilterItem = data.lastChangedFilterItem;
            }

            if (data.highlightFilter != m_CurrentHighlightFilter)
            {
                var filterListItem = m_ActiveFilterListItem.SingleOrDefault(e =>
                    e.groupKey == m_CurrentHighlightFilter.groupKey &&
                    e.filterKey == m_CurrentHighlightFilter.filterKey);
                if (filterListItem != null)
                {
                    filterListItem.SetHighlight(false);
                }

                filterListItem = m_ActiveFilterListItem.SingleOrDefault(e =>
                    e.groupKey == data.highlightFilter.groupKey && e.filterKey == data.highlightFilter.filterKey);
                if (filterListItem != null)
                {
                    filterListItem.SetHighlight(true);
                }

                m_CurrentHighlightFilter = data.highlightFilter;
            }
        }

        IEnumerator SetDefaultGroup()
        {
            yield return null;
            m_FilterGroupDropdown.value = -1;
            m_FilterGroupDropdown.value = 0;
        }

        void OnFilterGroupChanged(int index)
        {
            var groupKey = m_FilterGroupDropdown.options[index].text;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetFilterGroup, groupKey));
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.Filters;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }
    }
}
