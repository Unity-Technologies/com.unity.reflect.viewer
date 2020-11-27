using System;
using SharpFlux;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class StatsInfoUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        BuildState m_BuildState;
        [SerializeField]
        ToolButton m_StatsButton;

        [SerializeField]
        Sprite m_InfoImage;
        [SerializeField]
        Sprite m_DebugImage;

        [SerializeField]
        TextMeshProUGUI m_BuildNumberText;

        [SerializeField]
        TextMeshProUGUI m_FpsMaxText;
        [SerializeField]
        TextMeshProUGUI m_FpsAvgText;
        [SerializeField]
        TextMeshProUGUI m_FpsMinText;

        [SerializeField]
        TextMeshProUGUI m_AssetsAddedText;
        [SerializeField]
        TextMeshProUGUI m_AssetsChangedText;
        [SerializeField]
        TextMeshProUGUI m_AssetsRemovedText;

        [SerializeField]
        TextMeshProUGUI m_InstancesAddedText;
        [SerializeField]
        TextMeshProUGUI m_InstancesChangedText;
        [SerializeField]
        TextMeshProUGUI m_InstancesRemovedText;

        [SerializeField]
        TextMeshProUGUI m_GameObjectsAddedText;
        [SerializeField]
        TextMeshProUGUI m_GameObjectsChangedText;
        [SerializeField]
        TextMeshProUGUI m_GameObjectsRemovedText;

        [SerializeField]
        Gradient m_ColorGradient;

        [SerializeField]
        int m_TargetFrameRate = 60;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        StatsInfoData m_CurrentStatsInfoData;
        DialogType m_CachedActiveDialog;
        ToolState m_CurrentToolState;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.debugStateChanged += OnDebugStateDataChanged;
            m_DialogWindow = GetComponent<DialogWindow>();
        }

        void Start()
        {
            m_StatsButton.buttonClicked += OnStatsButtonClicked;
            m_StatsButton.buttonLongPressed += OnStatsButtonLongPressed;

            m_BuildNumberText.text = m_BuildState.bundleVersion + "." + m_BuildState.buildNumber;
        }

        void OnStatsButtonClicked()
        {
            if (m_CurrentToolState.infoType == InfoType.Info)
            {
                var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.StatsInfo;
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
            }
            if (m_CurrentToolState.infoType == InfoType.Debug)
            {
                var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.DebugOptions;
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
            }
        }

        void OnStatsButtonLongPressed()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.InfoSelect));
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_CachedActiveDialog != data.activeDialog)
            {
                m_StatsButton.selected = data.activeDialog == DialogType.StatsInfo;
                m_CachedActiveDialog = data.activeDialog;
            }

            if (m_CurrentToolState != data.toolState)
            {
                if (data.toolState.infoType == InfoType.Info)
                {
                    m_StatsButton.SetIcon(m_InfoImage);
                }
                else if (data.toolState.infoType == InfoType.Debug)
                {
                    m_StatsButton.SetIcon(m_DebugImage);
                }
                m_CurrentToolState = data.toolState;
            }
            m_StatsButton.button.interactable = data.toolbarsEnabled;
        }

        void OnDebugStateDataChanged(UIDebugStateData data)
        {
            if (!m_DialogWindow.open)
            {
                return;
            }

            if (m_CurrentStatsInfoData != data.statsInfoData)
            {
                if (m_CurrentStatsInfoData.fpsMax != data.statsInfoData.fpsMax)
                {
                    m_FpsMaxText.text = data.statsInfoData.fpsMax.ToString();
                    m_FpsMaxText.color = m_ColorGradient.Evaluate((float)data.statsInfoData.fpsMax / m_TargetFrameRate);
                }
                if (m_CurrentStatsInfoData.fpsAvg != data.statsInfoData.fpsAvg)
                {
                    m_FpsAvgText.text = data.statsInfoData.fpsAvg.ToString();
                    m_FpsAvgText.color = m_ColorGradient.Evaluate((float)data.statsInfoData.fpsAvg / m_TargetFrameRate);
                }
                if (m_CurrentStatsInfoData.fpsMin != data.statsInfoData.fpsMin)
                {
                    m_FpsMinText.text = data.statsInfoData.fpsMin.ToString();
                    m_FpsMinText.color = m_ColorGradient.Evaluate((float)data.statsInfoData.fpsMin / m_TargetFrameRate);
                }

                if (m_CurrentStatsInfoData.assetsCountData != data.statsInfoData.assetsCountData)
                {
                    m_AssetsAddedText.text = data.statsInfoData.assetsCountData.addedCount.ToString();
                    m_AssetsChangedText.text = data.statsInfoData.assetsCountData.changedCount.ToString();
                    m_AssetsRemovedText.text = data.statsInfoData.assetsCountData.removedCount.ToString();
                }

                if (m_CurrentStatsInfoData.instancesCountData != data.statsInfoData.instancesCountData)
                {
                    m_InstancesAddedText.text = data.statsInfoData.instancesCountData.addedCount.ToString();
                    m_InstancesChangedText.text = data.statsInfoData.instancesCountData.changedCount.ToString();
                    m_InstancesRemovedText.text = data.statsInfoData.instancesCountData.removedCount.ToString();
                }

                if (m_CurrentStatsInfoData.gameObjectsCountData != data.statsInfoData.gameObjectsCountData)
                {
                    m_GameObjectsAddedText.text = data.statsInfoData.gameObjectsCountData.addedCount.ToString();
                    m_GameObjectsChangedText.text = data.statsInfoData.gameObjectsCountData.changedCount.ToString();
                    m_GameObjectsRemovedText.text = data.statsInfoData.gameObjectsCountData.removedCount.ToString();
                }

                m_CurrentStatsInfoData = data.statsInfoData;
            }
        }
    }
}
