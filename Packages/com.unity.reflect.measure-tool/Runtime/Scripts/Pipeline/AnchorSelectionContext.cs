using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.MeasureTool
{
    [DisallowMultipleComponent]
    public class AnchorSelectionContext : MonoBehaviour
    {
        [Serializable]
        public struct SelectionContext
        {
            public SelectObjectMeasureToolAction.IAnchor selectedAnchor;
        }

        public List<SelectionContext> SelectionContextList;
        public SelectionContext LastContext => SelectionContextList[SelectionContextList.Count - 1];

        void Awake()
        {
            SelectionContextList = new List<SelectionContext>();
        }
    }
}
