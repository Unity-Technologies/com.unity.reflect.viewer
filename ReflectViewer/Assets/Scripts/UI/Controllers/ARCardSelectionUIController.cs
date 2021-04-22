using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
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
            if(UIStateManager.current.walkStateData.walkEnabled)
                UIStateManager.current.walkStateData.instruction.Cancel();
            var navigationState = UIStateManager.current.stateData.navigationState;
            var currentNavigationMode = navigationState.navigationMode == NavigationMode.Walk? NavigationMode.Orbit: navigationState.navigationMode;

            if (currentNavigationMode != NavigationMode.AR)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.UnloadScene, m_SceneDictionary[currentNavigationMode]));

                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.LoadScene, m_SceneDictionary[NavigationMode.AR]));
            }

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, false));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            navigationState.navigationMode = NavigationMode.AR;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARMode, arMode));
        }

        void OnBgButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
        }
    }
}
