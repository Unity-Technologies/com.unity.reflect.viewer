using System;
using System.Collections.Generic;
using SharpFlux;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class ARCardSelectionUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_BgButton;

        [SerializeField]
        CardCarouselController[] m_ARCards;

#pragma warning restore CS0649

        Dictionary<NavigationMode, string> m_SceneDictionary = new Dictionary<NavigationMode, string>();

        void Awake()
        {
            foreach (var info in UIStateManager.current.stateData.navigationState.navigationModeInfos)
            {
                m_SceneDictionary[info.navigationMode] = info.modeScenePath;
            }
        }

        void Start()
        {
            m_BgButton.onClick.AddListener(OnBgButtonClicked);

            foreach (var arCard in m_ARCards)
            {
                arCard.buttonClicked += ARCardClicked;
            }
        }

        void ARCardClicked(ARMode arMode)
        {
            var navigationState = UIStateManager.current.stateData.navigationState;
            var currentNavigationMode = navigationState.navigationMode;

            if (currentNavigationMode != NavigationMode.AR)
            {
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.UnloadScene, m_SceneDictionary[currentNavigationMode]));

                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.LoadScene, m_SceneDictionary[NavigationMode.AR]));
            }

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            navigationState.navigationMode = NavigationMode.AR;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARMode, arMode));
        }

        void OnBgButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
        }
    }
}
