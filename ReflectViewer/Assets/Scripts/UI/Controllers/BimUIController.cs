using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
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
        public const string allBimOptionName = "All Information";
        private static readonly OptionData k_AllInfoOption = new OptionData(allBimOptionName);

#pragma warning disable CS0649
        [SerializeField, Tooltip("Reference to the button prefab.")]
        BimListItem m_BimListItemPrefab;

        [SerializeField]
        Transform m_ParentTransform;

        [SerializeField]
        TMP_Dropdown m_BimGroupDropdown;

        [SerializeField]
        ButtonControl m_FoldoutToggle;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        FoldoutRect m_FoldoutRect;

        ObjectSelectionInfo m_CurrentObjectSelectionInfo;
        string m_CurrentBimGroup;

        Stack<BimListItem> m_BimListItemPool = new Stack<BimListItem>();
        List<BimListItem> m_ActiveBimListItem = new List<BimListItem>();

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

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
        }

        private void OnFoldoutToggle(BaseEventData eventData)
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
            if (data.bimGroup == m_CurrentBimGroup)
                return;

            if (string.IsNullOrEmpty(data.bimGroup))
                return;

            m_CurrentBimGroup = data.bimGroup;

            RefreshShownBimItems();
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (data.objectSelectionInfo != m_CurrentObjectSelectionInfo)
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

                if(currentSelectedObject != null)
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
        }

        void RefreshShownBimItems()
        {
            foreach (var parameter in m_ActiveBimListItem)
                parameter.gameObject.SetActive(parameter.Group.Equals(m_CurrentBimGroup) || m_CurrentBimGroup == allBimOptionName);
        }

        void OnBimGroupChanged(int index)
        {
            var groupKey = m_BimGroupDropdown.options[index].text;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetBimGroup, groupKey));
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.BimInfo;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }
    }
}
