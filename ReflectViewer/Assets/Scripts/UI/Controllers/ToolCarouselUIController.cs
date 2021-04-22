using SharpFlux;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible for managing the selection of the type of orbit tool
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class ToolCarouselUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Button to toggle the dialog.")]
        CarouselPropertyControl m_CarouselPropertyControl;
        [SerializeField, Tooltip("List of button controls.")]
        List<GameObject> m_NavigationModeToolbar = new List<GameObject>();
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        ButtonControl m_ActiveButtonControl;
        NavigationMode? m_CachedNavigationMode;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_DialogWindow = GetComponent<DialogWindow>();
            m_CarouselPropertyControl.onValueChanged.AddListener(OnCarouselValueChanged);
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_CachedNavigationMode == null || m_CachedNavigationMode != stateData.navigationState.navigationMode)
            {
                m_CachedNavigationMode = stateData.navigationState.navigationMode;
            }
        }

        private void OnCarouselValueChanged(int carouselIndex)
        {
            // NOTE: this assumes carouselIndex is in the exact order as NavigationMode enum!
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.navigationMode = (NavigationMode)carouselIndex;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            // NOTE: this assumes carouselIndex is in the exact order as ToolbarType enum!
            var toolbarType = (ToolbarType)carouselIndex;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, toolbarType));

        }
    }
}
