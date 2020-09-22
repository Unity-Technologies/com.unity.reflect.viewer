using System;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class ProgressIndicatorUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ProgressIndicatorControl m_ProgressIndicatorControl;
#pragma warning restore CS0649

        ProgressData m_CurrentProgressData;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_CurrentProgressData != data.progressData)
            {
                switch (data.progressData.progressState)
                {
                    case ProgressData.ProgressState.NoPendingRequest:
                    {
                        m_ProgressIndicatorControl.StopLooping();
                        break;
                    }
                    case ProgressData.ProgressState.PendingIndeterminate:
                    {
                        m_ProgressIndicatorControl.StartLooping();
                        break;
                    }
                    case ProgressData.ProgressState.PendingDeterminate:
                    {
                        float percent = 1;
                        if (data.progressData.totalCount != 0)
                        {
                            percent = (float) data.progressData.currentProgress / data.progressData.totalCount;
                        }

                        m_ProgressIndicatorControl.SetProgress(percent);
                        break;
                    }
                }
                m_CurrentProgressData = data.progressData;
            }
        }
    }
}
