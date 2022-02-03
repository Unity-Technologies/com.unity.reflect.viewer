using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Display BIM information
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class BimUIController: MonoBehaviour
    {
        public const string k_AllBimOptionName = "All Information";
        public const string k_DefaultNoSelectionText = "Select an object to see its BIM information.";
        static readonly OptionData k_AllInfoOption = new OptionData(k_AllBimOptionName);

#pragma warning disable CS0649
        [SerializeField, Tooltip("Reference to the button prefab.")]
        BimListItem m_BimListItemPrefab;

        [SerializeField]
        Transform m_ParentTransform;

        [SerializeField]
        TMP_Dropdown m_BimGroupDropdown;

        [SerializeField]
        TMP_InputField m_SearchInput;

        [SerializeField]
        Button m_CancelButton;

        [SerializeField]
        TextMeshProUGUI m_NoSelectionText;

        [SerializeField]
        ToolButton m_SelectButton;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;

        string m_CurrentBimGroup;
        GameObject m_OldSelectedObject;
        SpatialSelector m_ObjectSelector;

        Stack<BimListItem> m_BimListItemPool = new Stack<BimListItem>();
        List<BimListItem> m_ActiveBimListItem = new List<BimListItem>();
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        IUISelector<SetActiveToolAction.ToolType> m_ActiveToolSelector;

        void Awake()
        {
            m_SelectButton.buttonClicked += OnSelectButtonClicked;

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<ObjectSelectionInfo>(ProjectContext.current,
                nameof(IObjectSelectorDataProvider.objectSelectionInfo), OnObjectSelectionChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(ProjectContext.current,
                nameof(IProjectSortDataProvider.bimSearchString), OnBimSearchStringChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current,
                nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged));
            m_DisposeOnDestroy.Add(m_ActiveToolSelector = UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current,
                nameof(IToolStateDataProvider.activeTool)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(UIStateContext.current,
                nameof(IUIStateDataProvider.bimGroup),
                bimGroup =>
                {
                    if (bimGroup == m_CurrentBimGroup)
                        return;

                    if (string.IsNullOrEmpty(bimGroup))
                        return;

                    m_CurrentBimGroup = bimGroup;
                    RefreshShownBimItems();
                }));

            m_DialogWindow = GetComponent<DialogWindow>();

            m_BimGroupDropdown.onValueChanged.AddListener(OnBimGroupChanged);
            m_BimGroupDropdown.options.Add(k_AllInfoOption);

            m_SearchInput.onValueChanged.AddListener(OnSearchInputTextChanged);
            m_SearchInput.onDeselect.AddListener(OnSearchInputDeselected);
            m_SearchInput.onSelect.AddListener(OnSearchInputSelected);

            m_CancelButton.onClick.AddListener(OnCancelButtonClicked);

            m_NoSelectionText.text = k_DefaultNoSelectionText;
            m_NoSelectionText.gameObject.SetActive(true);

            m_ObjectSelector = new SpatialSelector();
        }

        void OnDestroy()
        {
            m_SelectButton.buttonClicked -= OnSelectButtonClicked;

            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType newData)
        {
            if (newData == OpenDialogAction.DialogType.BimInfo)
            {
                m_SearchInput.text = null;
            }
            m_SelectButton.selected = newData == OpenDialogAction.DialogType.BimInfo;
        }

        void CreateBimListItem(string group, string category, string value)
        {
            var bimListItem = m_BimListItemPool.Count > 0 ? m_BimListItemPool.Pop() : Instantiate(m_BimListItemPrefab, m_ParentTransform);

            bimListItem.InitItem(group, category, value);
            bimListItem.gameObject.SetActive(true);
            bimListItem.transform.SetAsLastSibling();
            m_ActiveBimListItem.Add(bimListItem);
        }

        void ClearBimList()
        {
            foreach (var filterListItem in m_ActiveBimListItem)
            {
                filterListItem.gameObject.SetActive(false);
                m_BimListItemPool.Push(filterListItem);
            }

            m_ActiveBimListItem.Clear();
        }

        void OnObjectSelectionChanged(ObjectSelectionInfo newData)
        {
            var currentSelectedObject = newData.CurrentSelectedObject();

            if (currentSelectedObject != m_OldSelectedObject || m_OldSelectedObject == null)
            {
                ClearBimList();
                m_BimGroupDropdown.options.Clear();
                m_BimGroupDropdown.options.Add(k_AllInfoOption);
                m_OldSelectedObject = currentSelectedObject;
            }

            var isSelected = currentSelectedObject != null;
            m_NoSelectionText.gameObject.SetActive(!isSelected);
            if (isSelected)
            {
                var metadata = currentSelectedObject.GetComponentInParent<Metadata>();
                foreach (var group in metadata.SortedByGroup())
                {
                    m_BimGroupDropdown.options.Add(new OptionData(group.Key));

                    foreach (var parameter in group.Value)
                    {
                        if (parameter.Value.visible)
                            CreateBimListItem(group.Key, parameter.Key, parameter.Value.value);
                    }
                }

                int targetIndex = 0;

                m_BimGroupDropdown
                    .SetValueWithoutNotify(
                        targetIndex); // Cant notify or this will trigger the valueChanged and do a dispatch inside a dispatch
                m_CurrentBimGroup = m_BimGroupDropdown.options[targetIndex].text;

                m_BimGroupDropdown.RefreshShownValue();
                RefreshShownBimItems();
            }
        }

        void OnBimSearchStringChanged(string newData)
        {
            SearchBimItem(newData);
        }

        void RefreshShownBimItems()
        {
            foreach (var parameter in m_ActiveBimListItem)
                parameter.gameObject.SetActive(parameter.groupKey.Equals(m_CurrentBimGroup) || m_CurrentBimGroup == k_AllBimOptionName);

            SearchBimItem(m_SearchInput.text);
        }

        void OnBimGroupChanged(int index)
        {
            var groupKey = m_BimGroupDropdown.options[index].text;
            Dispatcher.Dispatch(SetBimGroupAction.From(groupKey));
            Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"BimSelectionFilter_{groupKey}"));
        }

        void OnCancelButtonClicked()
        {
            m_SearchInput.text = "";
        }

        Coroutine m_SearchCoroutine;
        void OnSearchInputTextChanged(string search)
        {
            if (m_SearchCoroutine != null)
            {
                StopCoroutine(m_SearchCoroutine);
                m_SearchCoroutine = null;
            }
            m_SearchCoroutine = StartCoroutine(SearchStringChanged(search));
        }

        IEnumerator SearchStringChanged(string search)
        {
            yield return new WaitForSeconds(0.2f);

            Dispatcher.Dispatch(SetBimSearchAction.From(search));
            Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"BimSelectionFilterSearch"));
        }

        void SearchBimItem(string search)
        {
            foreach (var bimListItem in m_ActiveBimListItem)
            {
                if (bimListItem.groupKey.Equals(m_CurrentBimGroup) || m_CurrentBimGroup == k_AllBimOptionName)
                {
                    if (bimListItem.category.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        bimListItem.value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        bimListItem.gameObject.SetActive(true);
                    }
                    else
                    {
                        bimListItem.gameObject.SetActive(false);
                    }
                }
            }
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

        void OnSelectButtonClicked()
        {
            Dispatcher.Dispatch(SetSpatialSelectorAction.From(m_ObjectSelector));
            var buttonSelected = m_SelectButton.selected;
            Dispatcher.Dispatch(SetActiveToolAction.From(buttonSelected ? SetActiveToolAction.ToolType.None : SetActiveToolAction.ToolType.SelectTool));
            Dispatcher.Dispatch(OpenDialogAction.From(buttonSelected ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.BimInfo));
        }
    }
}
