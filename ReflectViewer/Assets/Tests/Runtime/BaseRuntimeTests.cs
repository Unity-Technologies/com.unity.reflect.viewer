using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.MARS.Providers;
using Unity.Reflect;
using Unity.Reflect.Multiplayer;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using Unity.TouchFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Utils;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReflectViewerRuntimeTests
{
    public class BaseRuntimeTests
    {
        #region Checks

        protected static bool IsGameObjectActive(string name)
        {
            GameObject gameObject = GameObject.Find(name);
            return gameObject.activeInHierarchy;
        }

        protected static bool IsDialogOpen(string name)
        {
            GameObject gameObject = GameObject.Find(name);
            DialogWindow dialog = gameObject.GetComponent<DialogWindow>();
            return dialog.open;
        }

        #endregion

        #region Actions

        protected static void WhenClickOnButton(string name)
        {
            GameObject gameObject = GameObject.Find(name);
            Button button = gameObject.GetComponent<Button>();
            button.onClick.Invoke();
        }

        protected static IEnumerator WhenUserLogout()
        {
            var loginManager = GameObject.FindObjectOfType<LoginManager>();
            loginManager.userLoggedOut.Invoke();
            yield return new WaitUntil(() => UIStateManager.current.sessionStateData.sessionState.loggedState == LoginState.LoggedOut);
            yield return WaitAFrame();
        }

        protected static IEnumerator WaitAFrame()
        {
            yield return Application.isBatchMode? null : new WaitForEndOfFrame();
        }

        protected static void AddUserToRoom(NetworkUserData user)
        {
            var identity = UIStateManager.current.GetUserIdentityFromSession(user.matchmakerId);

            var multiplayerController = Resources.FindObjectsOfTypeAll<MultiplayerController>().First();
            var color = (UIStateManager.current.stateData.colorPalette.Length > identity.colorIndex) ?
                            UIStateManager.current.stateData.colorPalette[identity.colorIndex] :
                            Color.blue; //temp fix for palettes
            user.visualRepresentation = multiplayerController.CreateVisualRepresentation(UIStateManager.current.m_RootNode.transform);
            user.visualRepresentation.avatarName = identity.fullName;
            user.visualRepresentation.color = color;
            user.visualRepresentation.avatarInitials = UIUtils.CreateInitialsFor(identity.fullName);

            UIStateManager.current.roomConnectionStateData.users.Add(user);
            UIStateManager.current.ForceSendConnectionChangedEvent();
        }

        #endregion

        #region Queries

        protected static GameObject GivenGameObjectNamed(string name)
        {
            return GameObject.Find(name);
        }

        protected static GameObject GivenChildNamed(GameObject parent, string childName)
        {
            Transform[] trs= parent.GetComponentsInChildren<Transform>(true);
            foreach(Transform t in trs){
                if(t.name == childName){
                    return t.gameObject;
                }
            }
            return null;
        }

        protected static T[] GivenObjectsInChildren<T>(GameObject parent) where T : Behaviour
        {
            return parent.transform.GetComponentsInChildren<T>();
        }

        protected static T GivenChildNamed<T>(GameObject parent, string childName) where T : Behaviour
        {
            return GivenChildNamed(parent, childName).GetComponent<T>();
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

        protected static void GivenViewerIsInVRMode()
        {
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.navigationMode = NavigationMode.VR;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
        }

        protected static void GivenViewerIsInOrbitMode()
        {
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.navigationMode = NavigationMode.Orbit;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
        }
        protected IEnumerator GivenUserIsLoggedInAndLandingScreenIsOpen()
        {
            yield return new WaitUntil(() => UIStateManager.current.sessionStateData.sessionState.loggedState == LoginState.LoggedIn);
            yield return WaitAFrame();
            yield return new WaitUntil(() => UIStateManager.current.stateData.activeDialog == DialogType.LandingScreen);
            yield return WaitAFrame();
        }

        protected IEnumerator AddProjects(string[] projectNames)
        {
            UnityProjectHost host = new UnityProjectHost("test server id", "test server name", new[] { "endpoint 1" }, "test accessToken", false);
            var projectList = new List<Project>();//UnityProjectHost host, string projectId, string projectName
            for (int i = 0; i < projectNames.Length; ++i)
            {
                var project = new Project(new UnityProject(host, i.ToString(), $"Project {projectNames[i]} {i}"));
                project.lastPublished.AddMinutes(i);
                projectList.Add(project);
            }
            ReflectPipelineFactory.projectsRefreshCompleted.Invoke(projectList);
            ReflectPipelineFactory.projectsRefreshCompleted = new ProjectListerSettings.ProjectsEvents();
            UIStateManager.current.ForceSendSessionStateChangedEvent();
            yield return WaitAFrame();
        }
    }
}
