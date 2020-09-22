using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
