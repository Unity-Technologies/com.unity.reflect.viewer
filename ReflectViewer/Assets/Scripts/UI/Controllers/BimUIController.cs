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
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Display BIM information
    /// </summary>
    [RequireComponent(typeof(DialogWindow)), RequireComponent(typeof(FoldoutRect))]
    public class BimUIController : MonoBehaviour
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
        ButtonControl m_FoldoutToggle;

        [SerializeField]
        TMP_InputField m_SearchInput;

        [SerializeField]
        Button m_CancelButton;

        [SerializeField]
        TextMeshProUGUI m_NoSelectionText;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        FoldoutRect m_FoldoutRect;

        ObjectSelectionInfo m_CurrentObjectSelectionInfo;
        string m_CurrentBimGroup;
        string m_CachedSearchString;
        string m_CurrentUserId;
        DialogType m_currentActiveSubDialog;

        Stack<BimListItem> m_BimListItemPool = new Stack<BimListItem>();
        List<BimListItem> m_ActiveBimListItem = new List<BimListItem>();

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;
            UIStateManager.sessionStateChanged += OnSessionStateChanged;

            m_DialogWindow = GetComponent<DialogWindow>();
            m_FoldoutRect = GetComponent<FoldoutRect>();

            if (m_FoldoutToggle.on)
                m_FoldoutRect.Fold(true);
            else
                m_FoldoutRect.Unfold(true);
            m_FoldoutToggle.onControlTap.AddListener(OnFoldoutToggle);

            m_FoldoutRect.rectFolded.AddListener(() => m_FoldoutToggle.on = true);
            m_FoldoutRect.rectUnfolded.AddListener(() => m_FoldoutToggle.on = false);

            m_BimGroupDropdown.onValueChanged.AddListener(OnBimGroupChanged);
            m_BimGroupDropdown.options.Add(k_AllInfoOption);

            m_SearchInput.onValueChanged.AddListener(OnSearchInputTextChanged);
            m_SearchInput.onDeselect.AddListener(OnSearchInputDeselected);
            m_SearchInput.onSelect.AddListener(OnSearchInputSelected);

            m_CancelButton.onClick.AddListener(OnCancelButtonClicked);

            m_NoSelectionText.text = k_DefaultNoSelectionText;
            m_NoSelectionText.gameObject.SetActive(true);
        }

        void OnFoldoutToggle(BaseEventData eventData)
        {
            if(m_FoldoutRect.isFolded)
            {
                m_FoldoutRect.Unfold();
            }
            else
            {
                m_FoldoutRect.Fold();
            }
        }

        void OnSessionStateChanged(UISessionStateData data)
        {
            m_CurrentUserId = data.sessionState.user?.UserId;
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

        void OnStateDataChanged(UIStateData data)
        {
            if (m_currentActiveSubDialog != data.activeSubDialog)
            {
                m_currentActiveSubDialog = data.activeSubDialog;
                if (m_currentActiveSubDialog == DialogType.BimInfo)
                {
                    m_SearchInput.text = null;
                }
            }

            if (data.bimGroup == m_CurrentBimGroup)
                return;

            if (string.IsNullOrEmpty(data.bimGroup))
                return;

            m_CurrentBimGroup = data.bimGroup;

            RefreshShownBimItems();
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (data.objectSelectionInfo != m_CurrentObjectSelectionInfo
            && (data.objectSelectionInfo.userId == m_CurrentUserId || data.objectSelectionInfo.userId == UIStateManager.current.roomConnectionStateData.localUser.matchmakerId))
            {
                var oldSelectedObject = m_CurrentObjectSelectionInfo.CurrentSelectedObject();
                var currentSelectedObject = data.objectSelectionInfo.CurrentSelectedObject();
                m_CurrentObjectSelectionInfo = data.objectSelectionInfo;

                if(currentSelectedObject != oldSelectedObject)
                {
                    ClearBimList();
                    m_BimGroupDropdown.options.Clear();
                    m_BimGroupDropdown.options.Add(k_AllInfoOption);
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

                    m_BimGroupDropdown.SetValueWithoutNotify(targetIndex); // Cant notify or this will trigger the valueChanged and do a dispatch inside a dispatch
                    m_CurrentBimGroup = m_BimGroupDropdown.options[targetIndex].text;

                    m_BimGroupDropdown.RefreshShownValue();
                    RefreshShownBimItems();
                }
            }

            if (data.bimSearchString != m_CachedSearchString)
            {
                SearchBimItem(data.bimSearchString);
                m_CachedSearchString = data.bimSearchString;
            }
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
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetBimGroup, groupKey));
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

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetBimSearch, search));
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
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.moveEnabled = !disableWASD;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
        }
    }
}
