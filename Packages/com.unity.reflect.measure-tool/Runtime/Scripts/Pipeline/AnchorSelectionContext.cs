using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Reflect.MeasureTool
{
    [DisallowMultipleComponent]
    public class AnchorSelectionContext : MonoBehaviour
    {
        [Serializable]
        public struct SelectionContext
        {
            public IAnchor selectedAnchor;
        }

        public List<SelectionContext> SelectionContextList;
        public SelectionContext LastContext => SelectionContextList[SelectionContextList.Count - 1];

        void Awake()
        {
            SelectionContextList = new List<SelectionContext>();
        }
    }
}
