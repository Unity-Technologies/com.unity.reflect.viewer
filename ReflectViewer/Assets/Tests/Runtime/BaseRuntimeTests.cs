using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Dispatching;
using Unity.Reflect;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Utils;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
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

            using (var loggedSelector = UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState)))
            {
                yield return WaitAFrame();
                yield return new WaitUntil(() => loggedSelector.GetValue() == LoginState.LoggedOut);
            }
            yield return WaitAFrame();
        }

        protected static IEnumerator WaitAFrame()
        {
            yield return Application.isBatchMode ? null : new WaitForEndOfFrame();
        }

        protected static void AddUserToRoom(NetworkUserData user)
        {
            using (var rootSelector = UISelectorFactory.createSelector<Transform>(PipelineContext.current, nameof(IPipelineDataProvider.rootNode)))
            using (var usersSelector = UISelectorFactory.createSelector<List<NetworkUserData>>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.users)))
            {
                var identity = UIStateManager.current.GetUserIdentityFromSession(user.matchmakerId);

                var multiplayerController = Resources.FindObjectsOfTypeAll<MultiplayerController>().First();
                using (var colorGetter = UISelectorFactory.createSelector<Color[]>(UIStateContext.current, nameof(IUIStateDataProvider.colorPalette)))
                {
                    var color = colorGetter.GetValue().Length > identity.colorIndex ?
                    colorGetter.GetValue()[identity.colorIndex] :
                    Color.blue; //temp fix for palettes
                    user.visualRepresentation = multiplayerController.CreateVisualRepresentation(rootSelector.GetValue());
                    user.visualRepresentation.avatarName = identity.fullName;
                    user.visualRepresentation.color = color;
                    user.visualRepresentation.avatarInitials = UIUtils.CreateInitialsFor(identity.fullName);
                }
                usersSelector.GetValue().Add(user);
                RoomConnectionContext.current.ForceOnStateChanged();
            }
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
            foreach (Transform t in trs)
            {
                if (t.name == childName)
                {
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
            Dispatcher.Dispatch(SetNavigationModeAction.From(SetNavigationModeAction.NavigationMode.VR));
        }

        protected static void GivenViewerIsInOrbitMode()
        {
            Dispatcher.Dispatch(SetNavigationModeAction.From(SetNavigationModeAction.NavigationMode.Orbit));
        }

        protected IEnumerator GivenUserIsLoggedIn()
        {
            using (var loggedGetter = UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState)))
            {
                yield return WaitAFrame();
                yield return new WaitUntil(() => loggedGetter.GetValue() == LoginState.LoggedIn);
            }
        }

        protected IEnumerator GivenUserIsLoggedInAndLandingScreenIsOpen()
        {
            yield return GivenUserIsLoggedIn();
            yield return WaitAFrame();
            using (var dialogGetter = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog)))
            {
                yield return new WaitUntil(() => dialogGetter.GetValue() == OpenDialogAction.DialogType.LandingScreen);
            }
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
            ReflectProjectsManager.projectsRefreshCompleted.Invoke(projectList);
            ReflectProjectsManager.projectsRefreshCompleted = new ProjectListerSettings.ProjectsEvents();
            // Dispatcher.Dispatch(SetProjectRoomAction.From(projectList.Cast<IProjectRoom>().ToArray()));
            yield return WaitAFrame();
        }
    }
}
