using System;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.Data
{
    [CreateAssetMenu(fileName = "Help Dialog Data", menuName = "Virtual Production/Virtual Camera/Help Dialog Data")]
    public class HelpDialogData : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public OpenDialogAction.DialogType dialogTypeId;
            public SetHelpModeIDAction.HelpModeEntryID helpModeEntryId; // For buttons without dialog/subdialog
            public string title;
            public string content;
        }

#pragma warning disable CS0649
        [SerializeField]
        Entry[] m_Entries;
#pragma warning restore CS0649

        public Entry[] entries => m_Entries;
    }
}
