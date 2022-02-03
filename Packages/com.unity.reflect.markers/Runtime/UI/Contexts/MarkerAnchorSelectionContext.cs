using System;
using System.Collections.Generic;
using Unity.Reflect.Markers.Placement;
using UnityEngine;

namespace Unity.Reflect.Markers.UI
{
    [DisallowMultipleComponent]
    public class MarkerAnchorSelectionContext : MonoBehaviour
    {
        [Serializable]
        public struct SelectionContext
        {
            public SelectObjectDragToolAction.IAnchor selectedAnchor;
        }

        public List<SelectionContext> SelectionContextList;
        public SelectionContext LastContext => SelectionContextList[SelectionContextList.Count - 1];

        void Awake()
        {
            SelectionContextList = new List<SelectionContext>();
        }
    }
}
