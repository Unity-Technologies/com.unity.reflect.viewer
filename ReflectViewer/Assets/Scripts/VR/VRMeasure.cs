using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer
{
    public class VRMeasure: VRPointer
    {
        IUISelector<bool> m_VREnableGetter;
        List<IDisposable> m_Disposable = new List<IDisposable>();

        void Awake()
        {
            m_SelectionTarget.gameObject.SetActive(false);

            m_Disposable.Add(UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState), OnToolStateDataChanged));
            m_Disposable.Add(m_VREnableGetter = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable)));
            m_Disposable.Add(UISelectorFactory.createSelector<IPicker>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectPicker), OnObjectSelectorChanged));
        }

        protected override void OnDestroy()
        {
            m_Disposable.ForEach(x => x.Dispose());
            base.OnDestroy();
        }

        void OnToolStateDataChanged(bool newData)
        {
            if (m_VREnableGetter != null && !m_VREnableGetter.GetValue())
                return;

            m_ShowPointer = newData;
            m_SelectionTarget.gameObject.SetActive(newData);
            StateChange();
        }
    }
}
