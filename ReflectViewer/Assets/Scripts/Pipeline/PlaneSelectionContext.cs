using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [DisallowMultipleComponent]
    public class PlaneSelectionContext : MonoBehaviour
    {
        [Serializable]
        public struct SelectionContext
        {
            public Plane SelectedPlane;
            public Vector3 HitPoint;
        }

        public List<SelectionContext> SelectionContextList;
        public SelectionContext LastContext => SelectionContextList[SelectionContextList.Count - 1];

        void Awake()
        {
            SelectionContextList = new List<SelectionContext>();
        }
    }
}



