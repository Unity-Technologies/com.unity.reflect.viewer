using System;
using Unity.Reflect.Viewer.UI;
using UnityEngine;

namespace Unity.Reflect.Viewer.Data
{
    [CreateAssetMenu(fileName = "Help Dialog Data", menuName = "Virtual Production/Virtual Camera/Help Dialog Data")]
    public class HelpDialogData : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public DialogType dialogTypeId;
            public HelpModeEntryID helpModeEntryId; // For buttons without dialog/subdialog
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
