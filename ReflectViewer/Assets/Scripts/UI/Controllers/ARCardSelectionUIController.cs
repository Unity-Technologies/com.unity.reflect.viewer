using System;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
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
        IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeSelector;

        void Awake()
        {
            m_NavigationModeSelector = UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode));
        }

        void OnDestroy()
        {
            m_NavigationModeSelector?.Dispose();
        }

        void Start()
        {
            m_BgButton.onClick.AddListener(OnBgButtonClicked);

            foreach (var arCard in m_ARCards)
            {
                arCard.buttonClicked += ARCardClicked;
            }
        }

        void ARCardClicked(SetARModeAction.ARMode arMode)
        {
            Dispatcher.Dispatch(SetARModeAction.From(arMode));
            Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"ARMode_{arMode}"));
            SelectARMode(arMode);
        }

        void OnBgButtonClicked()
        {
            Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
        }

        public void SelectARMode(SetARModeAction.ARMode arMode)
        {
            if (arMode == SetARModeAction.ARMode.None)
                return;

            Dispatcher.Dispatch(SetWalkEnableAction.From(false));

            var currentNavigationMode = m_NavigationModeSelector.GetValue() == SetNavigationModeAction.NavigationMode.Walk
                ? SetNavigationModeAction.NavigationMode.Orbit
                : m_NavigationModeSelector.GetValue();

            if (currentNavigationMode != SetNavigationModeAction.NavigationMode.AR)
            {
                Dispatcher.Dispatch(UnloadSceneActions<Project>.From(UIStateManager.current.GetSceneDictionary()[currentNavigationMode]));
                Dispatcher.Dispatch(LoadSceneActions<Project>.From(UIStateManager.current.GetSceneDictionary()[SetNavigationModeAction.NavigationMode.AR]));
            }

            Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
            Dispatcher.Dispatch(SetInstructionMode.From(false));
            Dispatcher.Dispatch(CloseAllDialogsAction.From(null));
            Dispatcher.Dispatch(SetNavigationModeAction.From(SetNavigationModeAction.NavigationMode.AR));
            Dispatcher.Dispatch(SetARModeAction.From(arMode));
        }
    }
}
