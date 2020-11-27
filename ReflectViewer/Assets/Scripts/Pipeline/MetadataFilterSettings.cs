using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    [Serializable]
    public class MetadataFilterSettings
    {
        [System.Serializable]
        public class MetadataGroupsChangedEvent : UnityEvent<IEnumerable<string>> { }
        [System.Serializable]
        public class MetadataCategoriesChangedEvent : UnityEvent<string, IEnumerable<string>> { }

        [SerializeField]
        public string[] m_Safelist =
        {
            "Category", "Family", "Document", "System Classification", "Type", "Manufacturer", "Phase Created",
            "Phase Demolished", "Layer"
        };

        [SerializeField]
        public List<ForwardRendererData> forwardRendererDatas;

        [HideInInspector]
        public MetadataGroupsChangedEvent groupsChanged;

        [HideInInspector]
        public MetadataCategoriesChangedEvent categoriesChanged;
    }
}
