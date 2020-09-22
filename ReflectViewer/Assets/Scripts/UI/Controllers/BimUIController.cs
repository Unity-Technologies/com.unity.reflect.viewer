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
    /// Display BIM information
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class BimUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Reference to the button prefab.")]
        BimListItem m_BimListItemPrefab;

        [SerializeField]
        Transform m_ParentTransform;

        [SerializeField]
        TMP_Dropdown m_BimGroupDropdown;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;

        ObjectSelectionInfo m_CurrentObjectSelectionInfo;
        string m_CurrentBimGroup;

        Stack<BimListItem> m_BimListItemPool = new Stack<BimListItem>();
        List<BimListItem> m_ActiveBimListItem = new List<BimListItem>();

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            m_DialogWindow = GetComponent<DialogWindow>();
            m_BimGroupDropdown.onValueChanged.AddListener(OnBimGroupChanged);
        }

        void Start()
        {
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

            foreach (var parameter in m_ActiveBimListItem)
                parameter.gameObject.SetActive(parameter.Group.Equals(m_CurrentBimGroup));
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (data.objectSelectionInfo != m_CurrentObjectSelectionInfo)
            {
                m_CurrentObjectSelectionInfo = data.objectSelectionInfo;

                if (data.objectSelectionInfo.selectedObjects == null ||
                    data.objectSelectionInfo.selectedObjects.Count == 0)
                {
                    ClearBimList();
                    return;
                }

                // TODO: handle selecting multiple objects
                var selectedObject = data.objectSelectionInfo.CurrentSelectedObject();
                var metadata = selectedObject.GetComponent<Metadata>();
                if (metadata == null)
                {
                    while (selectedObject.transform.parent != null)
                    {
                        selectedObject = selectedObject.transform.parent.gameObject;
                        metadata = selectedObject.GetComponent<Metadata>();
                        if (metadata != null)
                            break;
                    }
                    if(metadata == null)
                        return;
                }

                ClearBimList();
                m_BimGroupDropdown.options.Clear();

                foreach (var group in metadata.SortedByGroup())
                {
                    m_BimGroupDropdown.options.Add(new TMP_Dropdown.OptionData(group.Key));

                    foreach (var parameter in group.Value)
                    {
                        if (parameter.Value.visible)
                            CreateBimListItem(group.Key, parameter.Key, parameter.Value.value);
                    }
                }

                // default select index = 0,
                StartCoroutine(SetDefaultGroup());
            }
        }

        IEnumerator SetDefaultGroup()
        {
            yield return null;
            m_BimGroupDropdown.value = -1;
            m_BimGroupDropdown.value = 0;
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
