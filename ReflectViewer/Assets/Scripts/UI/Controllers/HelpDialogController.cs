using System;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Reflect.Viewer.Data;
using TMPro;
using SharpFlux;
using SharpFlux.Dispatching;

namespace Unity.Reflect.Viewer.UI
{
    public class HelpDialogController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        HelpDialogData m_Data;
        [SerializeField]
        GameObject m_HelpScreenBackground;
#pragma warning restore CS0649
        DialogMode m_currentDialogMode;
        HelpModeEntryID m_currentHelpModeId;
        static HelpDialogController s_Instance;
        void Awake()
        {
            Assert.IsNull(s_Instance);
            s_Instance = this;
            UIStateManager.stateChanged += OnStateDataChanged;
        }
        void OnDestroy()
        {
            Assert.IsTrue(s_Instance == this);
            s_Instance = null;
        }
        public void Display(HelpModeEntryID helpModeId)
        {
            Assert.IsNotNull(m_Data.entries);
            if (helpModeId == HelpModeEntryID.None)
            {
                return;
            }
            foreach (var entry in m_Data.entries)
            {
                if (entry.helpModeEntryId == helpModeId)
                {
                    DisplayEntry(entry, false);
                    return;
                }
            }
            Debug.LogError($"Could not find help dialog data corresponding to id [{helpModeId}]");
        }
        public void Display(DialogType dialogTypeId)
        {
            Assert.IsNotNull(m_Data.entries);
            if (dialogTypeId == DialogType.None)
            {
                return;
            }
            foreach (var entry in m_Data.entries)
            {
                if (entry.dialogTypeId == dialogTypeId)
                {
                    DisplayEntry(entry, true);
                    return;
                }
            }
            Debug.LogError($"Could not find help dialog data corresponding to id [{dialogTypeId}]");
        }
        void OnStateDataChanged(UIStateData data)
        {
            if (m_currentDialogMode != data.dialogMode)
            {
                m_currentDialogMode = data.dialogMode;
                m_HelpScreenBackground.SetActive(data.dialogMode == DialogMode.Help);
            }

            if (m_currentDialogMode == DialogMode.Help)
            {
                if (m_currentHelpModeId != data.helpModeEntryId)
                {
                    m_currentHelpModeId = data.helpModeEntryId;
                    Display(data.helpModeEntryId);
                }
            }
        }
        void DisplayEntry(HelpDialogData.Entry entry, bool isDialogtype)
        {
            var data = UIStateManager.current.popUpManager.GetModalPopUpData();
            data.title = entry.title;
            data.text = entry.content;
            data.positiveCallback = delegate { Close(isDialogtype); };
            UIStateManager.current.popUpManager.DisplayModalPopUp(data);
        }

        public void Close(bool isDialogType)
        {
            if (isDialogType)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));
            }
            else
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetHelpModeID, HelpModeEntryID.None));
            }
        }

        public static bool SetHelpID(HelpModeEntryID entryId)
        {
            if (UIStateManager.current.stateData.dialogMode == DialogMode.Help)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetHelpModeID, entryId));
                return true;
            }
            return false;
        }
    }
}
