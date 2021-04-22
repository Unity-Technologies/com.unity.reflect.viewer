using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Reflect;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ReflectViewerRuntimeTests
{
    public class MultiplayerUITests : BaseReflectSceneTests
    {
        UnityProjectHost m_ProjectServerHost = new UnityProjectHost("test server id", "test server name", new[] { "endpoint 1" }, "test accessToken", false);

        UserIdentity[] m_TestUsers =
        {
            new UserIdentity("1", 1, "User Alpha", DateTime.UtcNow, null),
            new UserIdentity("2", 2,  "User Beta", DateTime.UtcNow.AddSeconds(5), null),
            new UserIdentity("3", 3, "User Gamma", DateTime.UtcNow.AddSeconds(10), null),
            new UserIdentity("4", 4, "User Delta", DateTime.UtcNow.AddSeconds(15), null),
            new UserIdentity("5", 5, "User Epsilon", DateTime.UtcNow.AddSeconds(20), null),
            new UserIdentity("6", 6, "User  Zeta", DateTime.UtcNow.AddSeconds(25), null),
            new UserIdentity("7", 7,  "User Eta", DateTime.UtcNow.AddSeconds(30), null),
            new UserIdentity("8", 8, "User Theta", DateTime.UtcNow.AddSeconds(35), null),
            new UserIdentity("9", 9,  "User Iota", DateTime.UtcNow.AddSeconds(40), null),
            new UserIdentity("10", 10,"User Lambda", DateTime.UtcNow.AddSeconds(45), null)
        };

        IEnumerator AddTestProject()
        {
            var projectList = new List<Project>();//UnityProjectHost host, string projectId, string projectName

            var project = new Project(new UnityProject(m_ProjectServerHost, "TestProjectId", "TestProject"));
            project.lastPublished.AddMinutes(1);
            projectList.Add(project);

            ReflectPipelineFactory.projectsRefreshCompleted.Invoke(projectList);
            ReflectPipelineFactory.projectsRefreshCompleted = new ProjectListerSettings.ProjectsEvents();
            UIStateManager.current.ForceSendSessionStateChangedEvent();
            yield return WaitAFrame();
        }

        IEnumerator ConnectAllUsers(string projectId, IEnumerable<UserIdentity> testUsers)
        {
            Assert.AreEqual(1, UIStateManager.current.sessionStateData.sessionState.rooms.Length);
            var roomIndex = Array.FindIndex(UIStateManager.current.sessionStateData.sessionState.rooms, (r) => r.project.serverProjectId == projectId);
            foreach (var user in testUsers)
            {
                UIStateManager.current.sessionStateData.sessionState.rooms[roomIndex].users.Add(user);
                UIStateManager.current.ForceSendSessionStateChangedEvent();
            }
            foreach (var user in testUsers)
            {
                AddUserToRoom(new NetworkUserData()
                {
                    matchmakerId = user.matchmakerId,
                    lastUpdateTimeStamp = DateTime.Now,
                });
                yield return WaitAFrame();
            }
        }

        [Ignore("Cannot run this test on yamato without a valid reflect user logged in")]
        [UnityTest]
        public IEnumerator MultiplayerUITests_CollaborationBarDisplayConnectedUsers()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            //Given
            var appBar = GivenGameObjectNamed("AppBar");
            var collaborationBar = GivenChildNamed(appBar, "CollaborationHorizontalList");
            var groupBubble = GivenChildNamed( collaborationBar, "CollaborationGroup");

            yield return AddTestProject();

            //When multiple users connect
            yield return ConnectAllUsers("test server id:TestProjectId", m_TestUsers.Take(5));
            var avatars = GivenObjectsInChildren<UserDetailsUIController>(collaborationBar);

            //Then 3 avatars are shown in the collaboration bar and the group bubble is inactive
            Assert.AreEqual(5, avatars.Length - 1); //Ignore profile button
            Assert.IsFalse(groupBubble.activeInHierarchy);
            //Then assure the last user to connect is the first in the list
            Assert.AreEqual(m_TestUsers[4].matchmakerId, avatars[0].MatchmakerId);
            Assert.AreEqual(m_TestUsers[3].matchmakerId, avatars[1].MatchmakerId);
            Assert.AreEqual(m_TestUsers[2].matchmakerId, avatars[2].MatchmakerId);
        }

        [Ignore("Cannot run this test on yamato without a valid reflect user logged in")]
        [UnityTest]
        public IEnumerator MultiplayerUITests_ClickUser_ShowsUserInfoDialog()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            yield return AddTestProject();
            //Given users are connected
            yield return WaitAFrame();
            yield return ConnectAllUsers("test server id:TestProjectId", m_TestUsers);
            yield return WaitAFrame();
            var userInfoDialog = GivenObjectNamed<UserDetailsUIController>("CollaborationUserInfoDialog");
            var appBar = GivenGameObjectNamed("AppBar");
            var collaborationBar = GivenChildNamed(appBar, "CollaborationHorizontalList");
            var avatars = GivenObjectsInChildren<UserUIButton>(collaborationBar);

            //When we click on an avatar
            avatars[0].m_Button.onClick.Invoke();
            yield return WaitAFrame();

            //Then the userinfo dialog is displayed with the user fullname
            Assert.IsTrue(IsDialogOpen("CollaborationUserInfoDialog"));
            Assert.AreEqual(m_TestUsers[m_TestUsers.Length - 1].fullName, userInfoDialog.m_FullName.text);
        }

        [Ignore("Cannot run this test on yamato without a valid reflect user logged in")]
        [UnityTest]
        public IEnumerator MultiplayerUITests_GroupBubble_ShowCorrectNbOfUsers()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            //Given
            var appBar = GivenGameObjectNamed("AppBar");
            var collaborationBarController = GivenChildNamed<CollaborationUIController>( appBar,"CollaborationBar");
            var collaborationBar = GivenChildNamed(appBar, "CollaborationHorizontalList");

            yield return AddTestProject();
            yield return WaitAFrame();
            //When multiple users connect
            yield return ConnectAllUsers("test server id:TestProjectId", m_TestUsers);
            yield return WaitAFrame();
            var groupBubble = GivenChildNamed( collaborationBar, "CollaborationGroup");
            var groupText = GivenChildNamed<TMP_Text>( collaborationBar, "NbOfUsersText");

            //Then the group bubble is displayed
            Assert.IsTrue(groupBubble.activeInHierarchy);
            Assert.AreEqual(collaborationBarController.maxHorizontalAvatars, groupBubble.transform.GetSiblingIndex());
            Assert.AreEqual("+6", groupText.text);
        }

        [Ignore("Cannot run this test on yamato without a valid reflect user logged in")]
        [UnityTest]
        public IEnumerator MultiplayerUITests_VerticalUserList_DisplayAllUsers()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();
            yield return AddTestProject();
            yield return WaitAFrame();
            yield return ConnectAllUsers("test server id:TestProjectId", m_TestUsers);

            //When Clicking the group bubble
            var appBar = GivenGameObjectNamed("AppBar");
            var collaborationBar = GivenChildNamed(appBar, "CollaborationHorizontalList");
            var groupButton = GivenChildNamed<Button>(collaborationBar, "CollaborationGroupButton");
            groupButton.onClick.Invoke();
            yield return WaitAFrame();

            //Then the vertical user list is displayed with all connected users
            var userListDialog = GivenObject<CollaborationUserListController>();
            var verticalUserList = userListDialog.m_List;
            Assert.IsTrue(IsDialogOpen("CollaborationUserListDialog"));
            Assert.AreEqual(m_TestUsers.Length, verticalUserList.transform.childCount);
            Assert.AreEqual("10 Total Users", userListDialog.m_DialogTitleText.text);
            var userVerticalItems = GivenObjectsInChildren<UserDetailsUIController>(verticalUserList.gameObject);

            Assert.AreEqual(m_TestUsers.Length, userVerticalItems.Length);

            for(int i = 0 ; i < userVerticalItems.Length; ++i)
            {
                Assert.AreEqual(m_TestUsers[m_TestUsers.Length - i - 1].matchmakerId, userVerticalItems[i].MatchmakerId);
            }
        }
    }
}
