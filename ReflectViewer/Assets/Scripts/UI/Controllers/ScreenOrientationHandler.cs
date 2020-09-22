using SharpFlux;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Sets the app autorotate settings and sets the screen orientation
    /// </summary>
    [DisallowMultipleComponent]
    public class ScreenOrientationHandler : MonoBehaviour
    {
        ScreenOrientation m_ScreenOrientation;

        void Awake()
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;

            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void Start()
        {
            GuessScreenOrientation();
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_ScreenOrientation = data.screenOrientation;
            Screen.orientation = m_ScreenOrientation;
        }

        void GuessScreenOrientation()
        {
            m_ScreenOrientation = Input.deviceOrientation == DeviceOrientation.LandscapeLeft ?
                ScreenOrientation.LandscapeLeft : ScreenOrientation.LandscapeRight;

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetScreenOrientation, m_ScreenOrientation));
        }
    }
}
