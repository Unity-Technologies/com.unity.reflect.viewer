using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core.Actions;

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

        void Awake()
        {
            m_DialogWindow = GetComponent<DialogWindow>();
            m_CarouselPropertyControl.onValueChanged.AddListener(OnCarouselValueChanged);
        }

        private void OnCarouselValueChanged(int carouselIndex)
        {
            // NOTE: this assumes carouselIndex is in the exact order as NavigationMode enum!
            Dispatcher.Dispatch(SetNavigationModeAction.From((SetNavigationModeAction.NavigationMode)carouselIndex));

            // NOTE: this assumes carouselIndex is in the exact order as ToolbarType enum!
            Dispatcher.Dispatch(SetActiveToolBarAction.From((SetActiveToolBarAction.ToolbarType)carouselIndex));

        }
    }
}
