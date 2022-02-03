using System;
using System.Collections.Generic;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.Reflect.Viewer.Pipeline;

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
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DialogWindow = GetComponent<DialogWindow>();

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(StatsInfoContext.current, nameof(IStatsInfoFPSDataProvider.fpsMax), (fpsMax) =>
            {
                m_FpsMaxText.text = k_StringDisplayCache[fpsMax];
                m_FpsMaxText.color = m_ColorGradient.Evaluate((float) fpsMax / m_TargetFrameRate);
            }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(StatsInfoContext.current, nameof(IStatsInfoFPSDataProvider.fpsAvg), (fpsAvg) =>
            {
                m_FpsAvgText.text = k_StringDisplayCache[fpsAvg];
                m_FpsAvgText.color = m_ColorGradient.Evaluate((float) fpsAvg / m_TargetFrameRate);
            }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(StatsInfoContext.current, nameof(IStatsInfoFPSDataProvider.fpsMin), (fpsMin) =>
            {
                m_FpsMinText.text = k_StringDisplayCache[fpsMin];
                m_FpsMinText.color = m_ColorGradient.Evaluate((float) fpsMin / m_TargetFrameRate);
            }));


            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<StreamCountData>(StatsInfoContext.current, nameof(IStatsInfoStreamDataProvider<StreamCountData>.assetsCountData), (assetsCountData) =>
            {
                m_AssetsAddedText.text = assetsCountData.addedCount.ToString();
                m_AssetsChangedText.text = assetsCountData.changedCount.ToString();
                m_AssetsRemovedText.text = assetsCountData.removedCount.ToString();
            }));


            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<StreamCountData>(StatsInfoContext.current, nameof(IStatsInfoStreamDataProvider<StreamCountData>.instancesCountData), (instancesCountData) =>
            {
                m_InstancesAddedText.text = instancesCountData.addedCount.ToString();
                m_InstancesChangedText.text = instancesCountData.changedCount.ToString();
                m_InstancesRemovedText.text = instancesCountData.removedCount.ToString();
            }));


            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<StreamCountData>(StatsInfoContext.current, nameof(IStatsInfoStreamDataProvider<StreamCountData>.gameObjectsCountData), (gameObjectsCountData) =>
            {
                m_GameObjectsAddedText.text = gameObjectsCountData.addedCount.ToString();
                m_GameObjectsChangedText.text = gameObjectsCountData.changedCount.ToString();
                m_GameObjectsRemovedText.text = gameObjectsCountData.removedCount.ToString();
            }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableStatsInfo), OnEnableStatsInfoChanged));
        }

        void Start()
        {
            m_BuildNumberText.text = m_BuildState.bundleVersion + "." + m_BuildState.buildNumber;
        }

        void OnEnableStatsInfoChanged(bool on)
        {
            if (on)
                m_DialogWindow.Open();
            else
                m_DialogWindow.Close();
        }
    }
}
