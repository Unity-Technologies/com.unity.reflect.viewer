using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Select new active project, download projects, manage projects
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class FiltersUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_DialogButton;

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

        [SerializeField]
        TMP_InputField m_SearchInput;

        [SerializeField]
        Button m_CancelButton;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        Dictionary<ButtonControl, int> m_ProjectDictionary;

        Stack<FilterListItem> m_FilterListItemPool = new Stack<FilterListItem>();
        List<FilterListItem> m_ActiveFilterListItem = new List<FilterListItem>();
        HighlightFilterInfo m_CachedHighlightFilter;
        Coroutine m_SearchCoroutine;
        IUISelector<List<SetVisibleFilterAction.IFilterItemInfo>> m_FilterInfosSelector;
        IUISelector<string> m_FilterSearchStringSelector;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;

        string m_SelectedGroupKey;
        List<string> m_CacheFilterGroupList = new List<string>();
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DialogButton.buttonClicked -= OnDialogButtonClicked;
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DialogWindow = GetComponent<DialogWindow>();

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<List<string>>(ProjectContext.current, nameof(IProjectSortDataProvider.filterGroupList), OnFilterGroupListChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(ProjectContext.current, nameof(IProjectSortDataProvider.filterGroup), OnFilterGroupChanged));
            m_DisposeOnDestroy.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged));
            m_DisposeOnDestroy.Add(m_FilterInfosSelector = UISelectorFactory.createSelector<List<SetVisibleFilterAction.IFilterItemInfo>>(ProjectContext.current, nameof(IProjectSortDataProvider.filterItemInfos), OnFilterItemInfosChanged));
            m_DisposeOnDestroy.Add(m_FilterSearchStringSelector = UISelectorFactory.createSelector<string>(ProjectContext.current, nameof(IProjectSortDataProvider.filterSearchString), filterSearch => { SearchFilterItem(filterSearch); }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetVisibleFilterAction.IFilterItemInfo>(ProjectContext.current, nameof(IProjectSortDataProvider.lastChangedFilterItem), OnLastChangedFilterItemChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<HighlightFilterInfo>(ProjectContext.current, nameof(IProjectSortDataProvider.highlightFilter), OnHighlightFilterInfoChanged));
            m_DialogButton.buttonClicked += OnDialogButtonClicked;
            m_CancelButton.onClick.AddListener(OnCancelButtonClicked);

            m_FilterGroupDropdown.onValueChanged.AddListener(OnFilterGroupChanged);
            m_SearchInput.onValueChanged.AddListener(OnSearchInputTextChanged);
            m_SearchInput.onSelect.AddListener(OnSearchInputSelected);
            m_SearchInput.onDeselect.AddListener(OnSearchInputDeselected);
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType newData)
        {
            m_DialogButton.selected = newData == OpenDialogAction.DialogType.Filters;
            if (newData == OpenDialogAction.DialogType.Filters)
            {
                m_SearchInput.text = null;

                if (m_LastReceivedFilterItemInfo != null)
                    RebuildList(m_LastReceivedFilterItemInfo);
            }
        }

        void CreateFilterListItem(SetVisibleFilterAction.IFilterItemInfo filterItemInfo)
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

            bool isHighlighted = m_CachedHighlightFilter.groupKey == filterItemInfo.groupKey && m_CachedHighlightFilter.filterKey == filterItemInfo.filterKey;
            filterListItem.InitItem(filterItemInfo.groupKey, filterItemInfo.filterKey, filterItemInfo.visible, isHighlighted);
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
            Dispatcher.Dispatch(SetVisibleFilterAction.From(filterItemInfo));
        }

        void OnListItemClicked(string groupKey, string filterKey)
        {
            var highlightFilterInfo = new HighlightFilterInfo
            {
                groupKey = groupKey,
                filterKey = filterKey
            };

            // Toggle like behaviour
            if (highlightFilterInfo == m_CachedHighlightFilter)
                highlightFilterInfo = default;

            Dispatcher.Dispatch(SetHighlightFilterAction.From(highlightFilterInfo));
            Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"BimFilter_{groupKey}_{filterKey}"));
        }

        void OnFilterGroupListChanged(List<string> newData)
        {
            if (m_CacheFilterGroupList.SequenceEqual(newData))
                return;

            // fill filter group Dropdown
            m_FilterGroupDropdown.options.Clear();

            if (newData.Count == 0)
            {
                // show no data
                ClearFilterList();
                m_FilterGroupDropdown.interactable = false;
                m_FilterGroupDropdown.options.Add(new TMP_Dropdown.OptionData("No Category"));
                m_NoDataText.gameObject.SetActive(true);
                m_DropdownMask.SetActive(true);
            }
            else
            {
                m_FilterGroupDropdown.interactable = true;
                m_NoDataText.gameObject.SetActive(false);
                m_DropdownMask.SetActive(false);

                var filterGroupList = newData;
                foreach (string group in filterGroupList)
                {
                    m_FilterGroupDropdown.options.Add(new TMP_Dropdown.OptionData(group));
                }

                // default select index = 0,
                StartCoroutine(SetDefaultGroup());
            }

            m_CacheFilterGroupList = newData;
        }

        void OnFilterGroupChanged(string newData)
        {
            m_SelectedGroupKey = newData;

            if (m_FilterInfosSelector == null)
                return;

            var list = m_FilterInfosSelector.GetValue();
            if (list != null)
            {
                RebuildList(list);
            }
        }

        void OnFilterItemInfosChanged(List<SetVisibleFilterAction.IFilterItemInfo> newData)
        {
            m_LastReceivedFilterItemInfo = newData;

            if (m_ActiveDialogSelector.GetValue() == OpenDialogAction.DialogType.Filters)
                RebuildList(m_LastReceivedFilterItemInfo);
        }

        List<SetVisibleFilterAction.IFilterItemInfo> m_LastReceivedFilterItemInfo;

        void RebuildList(List<SetVisibleFilterAction.IFilterItemInfo> newData)
        {
            m_LastReceivedFilterItemInfo = null;

            ClearFilterList();
            foreach (var filterItemInfo in newData)
            {
                if (filterItemInfo.groupKey == m_SelectedGroupKey)
                    CreateFilterListItem(filterItemInfo);
            }

            if (m_FilterSearchStringSelector != null)
                SearchFilterItem(m_FilterSearchStringSelector.GetValue());
        }

        void OnLastChangedFilterItemChanged(SetVisibleFilterAction.IFilterItemInfo newData)
        {
            if (newData != null)
            {
                var filterItemInfo = (FilterItemInfo)newData;
                var filterListItem = m_ActiveFilterListItem.SingleOrDefault(e =>                e.groupKey == filterItemInfo.groupKey && e.filterKey == filterItemInfo.filterKey);

                if (filterListItem != null)
                    filterListItem.SetVisible(filterItemInfo.visible);
            }
        }

        void OnHighlightFilterInfoChanged(HighlightFilterInfo newData)
        {
            var filterListItem = m_ActiveFilterListItem.SingleOrDefault(e =>
                e.groupKey == m_CachedHighlightFilter.groupKey &&
                e.filterKey == m_CachedHighlightFilter.filterKey);
            if (filterListItem != null)
            {
                filterListItem.SetHighlight(false);
            }

            filterListItem = m_ActiveFilterListItem.SingleOrDefault(e =>
                e.groupKey == newData.groupKey && e.filterKey == newData.filterKey);
            if (filterListItem != null)
            {
                filterListItem.SetHighlight(true);
            }

            m_CachedHighlightFilter = newData;
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
            Dispatcher.Dispatch(SetFilterGroupAction.From(groupKey));
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.Filters;
            Dispatcher.Dispatch(OpenDialogAction.From(dialogType));
        }

        void OnCancelButtonClicked()
        {
            m_SearchInput.text = "";
        }

        void OnSearchInputTextChanged(string search)
        {
            if (m_SearchCoroutine != null)
            {
                StopCoroutine(m_SearchCoroutine);
                m_SearchCoroutine = null;
            }

            m_SearchCoroutine = StartCoroutine(SearchStringChanged(search));
        }

        void OnSearchInputSelected(string input)
        {
            DisableMovementMapping(true);
        }

        void OnSearchInputDeselected(string input)
        {
            DisableMovementMapping(false);
        }

        public static void DisableMovementMapping(bool disableWASD)
        {
            Dispatcher.Dispatch(SetMoveEnabledAction.From(!disableWASD));
        }

        IEnumerator SearchStringChanged(string search)
        {
            yield return new WaitForSeconds(0.2f);

            Dispatcher.Dispatch(SetFilterSearchAction.From(search));
            Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"BimFilterSearch"));
        }

        void SearchFilterItem(string search)
        {
            foreach (var filterListItem in m_ActiveFilterListItem)
            {
                if (filterListItem.filterKey.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    filterListItem.gameObject.SetActive(true);
                }
                else
                {
                    filterListItem.gameObject.SetActive(false);
                }
            }
        }
    }
}
