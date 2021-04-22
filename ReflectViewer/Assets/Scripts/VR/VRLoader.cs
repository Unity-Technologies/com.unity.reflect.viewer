using System.Collections;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Reflect.Viewer.UI
{
    public class VRLoader : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(LoadAsyncScene("ReflectVR"));
        }

        IEnumerator LoadAsyncScene(string scenePath)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableVR, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetCulling, false));
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(true);
            navigationState.navigationMode = NavigationMode.VR;
            navigationState.showScaleReference = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}
