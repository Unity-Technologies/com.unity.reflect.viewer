using System;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Reflect.Viewer.Data;
using SharpFlux.Dispatching;
using UnityEngine.Reflect.Viewer.Core;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

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
        static HelpDialogController s_Instance;
        IUISelector<SetDialogModeAction.DialogMode> m_DialogModeSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            Assert.IsNull(s_Instance);
            s_Instance = this;

            m_DisposeOnDestroy.Add(m_DialogModeSelector = UISelectorFactory.createSelector<SetDialogModeAction.DialogMode>(UIStateContext.current, nameof(IDialogDataProvider.dialogMode),
                mode =>
                {
                    m_HelpScreenBackground.SetActive(mode == SetDialogModeAction.DialogMode.Help);
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetHelpModeIDAction.HelpModeEntryID>(UIStateContext.current, nameof(IHelpModeDataProvider.helpModeEntryId), OnHelpModeEntryChanged));
        }

        void OnHelpModeEntryChanged(SetHelpModeIDAction.HelpModeEntryID data)
        {
            if (m_DialogModeSelector.GetValue() == SetDialogModeAction.DialogMode.Help)
            {
                Display(data);
            }
        }

        void OnDestroy()
        {
            Assert.IsTrue(s_Instance == this);
            s_Instance = null;
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        public void Display(SetHelpModeIDAction.HelpModeEntryID helpModeId)
        {
            Assert.IsNotNull(m_Data.entries);
            if (helpModeId == SetHelpModeIDAction.HelpModeEntryID.None)
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
        public void Display(OpenDialogAction.DialogType dialogTypeId)
        {
            Assert.IsNotNull(m_Data.entries);
            if (dialogTypeId == OpenDialogAction.DialogType.None)
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
                Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
                Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));
            }
            else
            {
                Dispatcher.Dispatch(SetHelpModeIDAction.From(SetHelpModeIDAction.HelpModeEntryID.None));
            }
        }

        public static bool SetHelpID(SetHelpModeIDAction.HelpModeEntryID entryId)
        {
            if (s_Instance.m_DialogModeSelector.GetValue() == SetDialogModeAction.DialogMode.Help)
            {
                Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"HelpMode_{entryId.ToString()}"));
                Dispatcher.Dispatch(SetHelpModeIDAction.From(entryId));
                return true;
            }

            return false;
        }
    }
}
