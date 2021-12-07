using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpFlux.Dispatching;
using Unity.Reflect;
using Unity.Reflect.Viewer;
using UnityEngine.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ReflectViewerRuntimeTests
{
    public class LandingPageUITests : BaseReflectSceneTests
    {
        [Category("YamatoIncompatible")]
        [UnityTest]
        public IEnumerator LandingPageUITests_Topbar_ButtonsCheck()
        {
            //Given the scene is loaded
            yield return WaitAFrame();
            using (var loggedSelector = UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState)))
            {
                yield return WaitAFrame();
                yield return new WaitUntil(() => loggedSelector.GetValue() == LoginState.LoggedIn);
            }
            yield return WaitAFrame();
            var leftTopBar = GivenGameObjectNamed("Left Topbar").transform.GetChild(0).gameObject;
            var projectListButton = GivenChildNamed(leftTopBar, "ProjectListButton");
            var refreshButton = GivenChildNamed(leftTopBar, "RefreshProjectsButton");
            var helpButton = GivenChildNamed(leftTopBar, "HelpButton");

            //When user is loggedin

            //Then the projectlistbutton and the refreshbutton should be visible
            Assert.IsTrue(projectListButton.transform.parent.gameObject.activeInHierarchy);
            Assert.IsTrue(refreshButton.transform.parent.gameObject.activeInHierarchy);

            //And the help button should be hidden
            Assert.IsFalse(helpButton.transform.parent.gameObject.activeInHierarchy);
        }

        [Category("YamatoIncompatible")]
        [UnityTest]
        public IEnumerator LandingPageUITests_Logout_LoginScreenIsVisible_AppBarIsHidden()
        {
            //Given the scene is loaded
            using (var loggedSelector = UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState)))
            {
                yield return WaitAFrame();
                yield return new WaitUntil(() => loggedSelector.GetValue() == LoginState.LoggedIn);

                yield return WaitAFrame();
                var appBarCanvasGroup = GivenObjectNamed<CanvasGroup>("AppBar");

                //When the user logout
                WhenClickOnButton("ProfileBtn");
                yield return WaitAFrame();
                Assert.IsTrue(IsDialogOpen("AccountDialog"));
                WhenClickOnButton("Logout Button");

                //Then the LoginPanel is visible and the AppBar is hidden
                Assert.IsTrue(IsDialogOpen("Login Screen Dialog"));
                Assert.IsFalse(appBarCanvasGroup.interactable);
                Assert.IsFalse(appBarCanvasGroup.blocksRaycasts);
                Assert.AreEqual(0f, appBarCanvasGroup.alpha);

                //When the user Login
                WhenClickOnButton("Login Button");
                yield return WaitAFrame();
                yield return new WaitUntil(() => loggedSelector.GetValue() == LoginState.LoggedIn);

                yield return WaitAFrame();

                //Then the appbar should be visible and the LoginScreen should be hidden
                Assert.IsTrue(appBarCanvasGroup.interactable);
                Assert.IsTrue(appBarCanvasGroup.blocksRaycasts);
                Assert.AreEqual(1f, appBarCanvasGroup.alpha);
            }
            Assert.IsTrue(IsDialogOpen("Landing Screen Dialog"));
            Assert.IsFalse(IsDialogOpen("Login Screen Dialog"));
        }

        [Category("YamatoIncompatible")]
        [UnityTest]
        public IEnumerator LandingPageUITests_ProjectList_NoProjectsShowEmptyState()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            var landingPageDialog = Resources.FindObjectsOfTypeAll<LandingScreenUIController>().First();

            //When the project receives an empty list
            ReflectProjectsManager.projectsRefreshCompleted.Invoke(new List<Project>());
            yield return WaitAFrame();

            // UIStateManager.current.ForceSendSessionStateChangedEvent();
            yield return WaitAFrame();

            //Then UI should display a message to indicate the list is empty
            var notFoundDisplayObj = GivenChildNamed(landingPageDialog.gameObject, "No Project Panel");
            Assert.IsTrue(notFoundDisplayObj.activeInHierarchy);
            var projectListContainer = GivenGameObjectNamed("Project List Container");
            Assert.AreEqual(0, projectListContainer.GetComponentsInChildren<ProjectListItem>().Length);
        }

        [Category("YamatoIncompatible")]
        [UnityTest]
        public IEnumerator LandingPageUITests_ProjectList_AllProjectsAreVisible()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            var landingPageDialog = Resources.FindObjectsOfTypeAll<LandingScreenUIController>().First().gameObject;

            //When the project receives the list
            yield return AddProjects(new[] { "A", "B", "C", "D", "E", "F", "G", "H" });

            //Then UI should display all projects
            var notFoundDisplayObj = GivenChildNamed(landingPageDialog.gameObject, "No Project Panel");
            var projectListContainer = GivenGameObjectNamed("Project List Container");
            Assert.IsFalse(notFoundDisplayObj.activeInHierarchy);
            Assert.AreEqual(8, projectListContainer.GetComponentsInChildren<ProjectListItem>().Length);
            yield return WaitAFrame();
        }

        [Category("YamatoIncompatible")]
        [UnityTest]
        public IEnumerator LandingPageUITests_ProjectList_Search()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            var landingPageDialog = Resources.FindObjectsOfTypeAll<LandingScreenUIController>().First();

            //Given a list of projects
            yield return AddProjects(new[] { "Bay A", "Shop A", "Station A", "Bay B", "Shop B", "Station B", "Bay C", "Shop C", "Station C" });

            //When we search for one of the names
            using (var filterGetter = UISelectorFactory.createSelector<string>(LandingScreenContext.current, nameof(IProjectListFilterDataProvider.searchString)))
            {
                Dispatcher.Dispatch(SetLandingScreenFilterProjectServerAction.From("Bay"));
                yield return new WaitUntil(() => filterGetter.GetValue() == "Bay");
            }
            yield return WaitAFrame();

            //Then UI should display all projects with the searched string in the name
            var notFoundDisplayObj = GivenChildNamed(landingPageDialog.gameObject, "No Project Panel");
            var projectListContainer = GivenGameObjectNamed("Project List Container");
            var items = projectListContainer.GetComponentsInChildren<ProjectListItem>();
            Assert.IsFalse(notFoundDisplayObj.activeInHierarchy);
            Assert.AreEqual(3, items.Length);
            Assert.IsTrue(items[0].room.project.name.Contains("Bay"));
            yield return WaitAFrame();
        }

        [Category("YamatoIncompatible")]
        [UnityTest]
        public IEnumerator LandingPageUITests_ProjectList_Collaborators()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            var landingPageDialog = Resources.FindObjectsOfTypeAll<LandingScreenUIController>().First();

            //When the project receives the list
            yield return AddProjects(new[] { "A", "B", "C", "D", "E", "F", "G", "H" });

            using (var roomSelector= UISelectorFactory.createSelector<IProjectRoom[]>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.rooms)))
            {
                //When User connects to Room A
                List<IProjectRoom> data = roomSelector.GetValue().ToList();
                ((ProjectRoom)data[0]).users.Add(new UserIdentity("1", 1, "User Alpha", DateTime.UtcNow, null));
                ((ProjectRoom)data[0]).users.Add(new UserIdentity("2", 2, "User Beta", DateTime.UtcNow.AddSeconds(5), null));

                yield return WaitAFrame();

                //TODO switch to forceUpdate on Value change
                Dispatcher.Dispatch(SetProjectRoomAction.From(data.Cast<IProjectRoom>().ToArray()));
            }
            yield return WaitAFrame();

            //Then UI should display all projects with the searched string in the name
            var notFoundDisplayObj = GivenChildNamed(landingPageDialog.gameObject, "No Project Panel");
            var projectListContainer = GivenGameObjectNamed("Project List Container");
            var items = projectListContainer.GetComponentsInChildren<ProjectListItem>();
            Assert.IsFalse(notFoundDisplayObj.activeInHierarchy);
            var avatars = items[0].gameObject.GetComponentsInChildren<UserDetailsUIController>();
            Assert.AreEqual(2, avatars.Length);
            yield return WaitAFrame();
        }

        [Category("YamatoIncompatible")]
        [UnityTest]
        public IEnumerator LandingPageUITests_ProjectList_ProjectItemPopup()
        {
            //Given the user is connected and in the landing screen
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            yield return AddProjects(new[] { "A", "B", "C", "D", "E", "F", "G", "H" });
            var projectListContainer = GivenGameObjectNamed("Project List Container");
            var items = projectListContainer.GetComponentsInChildren<ProjectListItem>();
            var projectItemDialog = GivenObject<LandingScreenProjectOptionsUIController>();
            Assert.IsFalse(projectItemDialog.gameObject.activeInHierarchy);

            //When Clicking in the option button
            var optionButton = GivenChildNamed<Button>(items[0].gameObject, "Option Button");
            optionButton.onClick.Invoke();
            yield return WaitAFrame();

            //Then the project options dialog should be displayed
            Assert.IsTrue(projectItemDialog.gameObject.activeInHierarchy);

            //When clicking again
            optionButton.onClick.Invoke();
            yield return WaitAFrame();

            //Then the project options dialog should be hidden
            Assert.IsFalse(projectItemDialog.gameObject.activeInHierarchy);
            yield return WaitAFrame();
        }

        [Category("YamatoIncompatible")]
        [UnityTest]
        public IEnumerator LandingPageUITests_ProjectList_Sorting()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            var landingPageDialog = Resources.FindObjectsOfTypeAll<LandingScreenUIController>().First();

            //Given a list of projects
            yield return AddProjects(new[] { "Bay A", "Shop A", "Station A", "Bay B", "Shop B", "Station B", "Bay C", "Shop C", "Station C" });

            //When clicking on the name header
            var nameHeaderButton = GivenChildNamed<Button>(landingPageDialog.gameObject, "NameColumn");
            nameHeaderButton.onClick.Invoke();
            yield return WaitAFrame();

            //Then UI should display all projects sorted by name in ascending order
            var projectListContainer = GivenGameObjectNamed("Project List Container");
            var items = projectListContainer.GetComponentsInChildren<ProjectListItem>();
            Assert.AreEqual(9, items.Length);
            Assert.IsTrue(items[0].room.project.name.Contains("Bay A"));
            Assert.IsTrue(items[1].room.project.name.Contains("Bay B"));
            Assert.IsTrue(items[2].room.project.name.Contains("Bay C"));

            //When clicking on the name header again
            nameHeaderButton.onClick.Invoke();
            yield return WaitAFrame();

            //Then UI should display all projects sorted by name in descending order
            items = projectListContainer.GetComponentsInChildren<ProjectListItem>();
            Assert.AreEqual(9, items.Length);
            Assert.IsTrue(items[0].room.project.name.Contains("Station C"));
            Assert.IsTrue(items[1].room.project.name.Contains("Station B"));
            Assert.IsTrue(items[2].room.project.name.Contains("Station A"));
            yield return WaitAFrame();
        }
    }
}
