using System;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class StatsInfoUIController : MonoBehaviour
    {
        // use this to avoid GC allocation every frame
        static readonly string[] k_StringDisplayCache = new[]
        {
            "00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
            "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
            "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
            "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
            "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
            "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
            "90", "91", "92", "93", "94", "95", "96", "97", "98", "99",
        };

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
        [SerializeField]
        DialogWindow m_DebugDialogWindow;
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
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
            }
            if (m_CurrentToolState.infoType == InfoType.Debug)
            {
                var dialogType = m_DebugDialogWindow.open ? DialogType.None : DialogType.DebugOptions;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
            }
        }

        void OnStatsButtonLongPressed()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.InfoSelect));
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_CachedActiveDialog != data.activeDialog)
            {
                m_StatsButton.selected = (data.activeDialog == DialogType.StatsInfo || data.activeDialog == DialogType.DebugOptions);
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
                    m_FpsMaxText.text = k_StringDisplayCache[data.statsInfoData.fpsMax];
                    m_FpsMaxText.color = m_ColorGradient.Evaluate((float)data.statsInfoData.fpsMax / m_TargetFrameRate);
                }
                if (m_CurrentStatsInfoData.fpsAvg != data.statsInfoData.fpsAvg)
                {
                    m_FpsAvgText.text = k_StringDisplayCache[data.statsInfoData.fpsAvg];
                    m_FpsAvgText.color = m_ColorGradient.Evaluate((float)data.statsInfoData.fpsAvg / m_TargetFrameRate);
                }
                if (m_CurrentStatsInfoData.fpsMin != data.statsInfoData.fpsMin)
                {
                    m_FpsMinText.text = k_StringDisplayCache[data.statsInfoData.fpsMin];
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
