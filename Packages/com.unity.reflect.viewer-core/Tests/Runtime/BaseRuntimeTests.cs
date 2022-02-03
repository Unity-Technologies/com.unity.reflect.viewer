using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReflectViewerCoreRuntimeTests
{
    public class BaseRuntimeTests
    {
        #region Checks

        protected static bool IsGameObjectActive(string name)
        {
            var gameObject = GameObject.Find(name);
            return gameObject.activeInHierarchy;
        }

        protected static bool IsDialogOpen(string name)
        {
            var gameObject = GameObject.Find(name);
            var dialog = gameObject.GetComponent<DialogWindow>();
            return dialog.open;
        }

        #endregion

        #region Actions

        protected static void WhenClickOnButton(string name)
        {
            var gameObject = GameObject.Find(name);
            var button = gameObject.GetComponent<Button>();
            button.onClick.Invoke();
        }

        protected static IEnumerator WaitAFrame()
        {
            yield return Application.isBatchMode? null : new WaitForEndOfFrame();
        }

        #endregion

        #region Queries

        protected static GameObject GivenGameObjectNamed(string name)
        {
            return GameObject.Find(name);
        }

        protected static GameObject GivenChildNamed(GameObject parent, string childName)
        {
            return parent.transform.Find(childName).gameObject;
        }

        protected static T[] GivenObjectsInChildren<T>(GameObject parent) where T : Behaviour
        {
            return parent.transform.GetComponentsInChildren<T>();
        }

        protected static T GivenChildNamed<T>(GameObject parent, string childName) where T : Behaviour
        {
            return parent.transform.Find(childName).GetComponentInChildren<T>();
        }

        protected static T GivenObject<T>() where T : Behaviour
        {
            return Resources.FindObjectsOfTypeAll<T>().First();
        }

        protected static List<T> GivenObjects<T>() where T : Behaviour
        {
            return new List<T>(Resources.FindObjectsOfTypeAll<T>());
        }

        protected static T GivenObjectNamed<T>(string name) where T : Behaviour
        {
            T[] objects = Resources.FindObjectsOfTypeAll<T>();
            return objects.First(e => e.name == name);
        }

        #endregion


        protected static IEnumerator GivenTheSceneIsLoadedAndActive(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                yield return WaitAFrame();
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            yield return WaitAFrame();
        }

        protected static IEnumerator GivenTheSceneIsUnloaded(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            }
            yield return WaitAFrame();
        }
    }
}
