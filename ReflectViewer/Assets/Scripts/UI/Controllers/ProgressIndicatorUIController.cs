using System;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class ProgressIndicatorUIController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ProgressIndicatorControl m_ProgressIndicatorControl;
#pragma warning restore CS0649

        bool m_Initialized;
        IUISelector<int> m_ProgressTotalCountGetter;
        IUISelector<int> m_ProgressCurrentGetter;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetProgressStateAction.ProgressState>(ProgressContext.current, nameof(IProgressDataProvider.progressState), OnProgressStateChanged));
            m_DisposeOnDestroy.Add(m_ProgressTotalCountGetter = UISelectorFactory.createSelector<int>(ProgressContext.current, nameof(IProgressDataProvider.totalCount)));
            m_DisposeOnDestroy.Add(m_ProgressCurrentGetter = UISelectorFactory.createSelector<int>(ProgressContext.current, nameof(IProgressDataProvider.currentProgress)));

            m_Initialized = true;
        }

        void OnProgressStateChanged(SetProgressStateAction.ProgressState newData)
        {
            if (!m_Initialized)
                return;

            switch (newData)
            {
                case SetProgressStateAction.ProgressState.NoPendingRequest:
                    {
                        m_ProgressIndicatorControl.StopLooping();
                        break;
                    }
                case SetProgressStateAction.ProgressState.PendingIndeterminate:
                    {
                        m_ProgressIndicatorControl.StartLooping();
                        break;
                    }
                case SetProgressStateAction.ProgressState.PendingDeterminate:
                    {
                        float percent = 1;
                        if (m_ProgressTotalCountGetter.GetValue() != 0)
                        {
                            percent = (float)m_ProgressCurrentGetter.GetValue() / m_ProgressTotalCountGetter.GetValue();
                        }

                        m_ProgressIndicatorControl.SetProgress(percent);
                        break;
                    }
            }
        }
    }
}
