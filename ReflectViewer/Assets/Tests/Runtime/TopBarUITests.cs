using NUnit.Framework;
using System.Collections;
using Unity.MARS.Providers;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect.Utils;
using UnityEngine.TestTools;

namespace ReflectViewerRuntimeTests
{
    public class TopBarUITests: BaseReflectSceneTests
    {
        [UnityTest]
        public IEnumerator TopBarUITests_CheckProfileBtnIsInLandingPage()
        {
            //Given the scene is loaded
            yield return WaitAFrame();

            //Then the settings button should be active and visible
            Assert.IsTrue(IsGameObjectActive("ProfileBtn"));
        }

        [Ignore("Cannot run this test on yamato without a valid reflect user logged in")]
        [UnityTest]
        public IEnumerator TopBarUITests_ClickProfileBtn_DisplaysAccountDialog()
        {
            yield return GivenUserIsLoggedInAndLandingScreenIsOpen();

            //Given the scene is loaded
            yield return WaitAFrame();
            //When clicking the collaboration settings button
            WhenClickOnButton("ProfileBtn");
            yield return WaitAFrame();

            //Then the dialog should be displayed
            Assert.IsTrue(IsDialogOpen("AccountDialog"));

            //When clicking the same button
            WhenClickOnButton("ProfileBtn");
            yield return WaitAFrame();

            //Then the dialog should be hidden
            Assert.IsFalse(IsDialogOpen("AccountDialog"));
        }

        [UnityTest]
        public IEnumerator TopBarUITests_SessionStarts_DisplaysInitials()
        {
            //Given the scene is loaded and session is ready
            yield return WaitAFrame();
            //Given the user name
            var userName = UIStateManager.current.sessionStateData.sessionState.user.DisplayName;

            //When The session starts
            var button = GivenGameObjectNamed("ProfileBtn");
            var initials = GivenChildNamed<TMPro.TMP_Text>(button, "InitialsText");
            yield return WaitAFrame();

            //Then the user initials should be displayed
            Assert.IsTrue(initials.gameObject.activeInHierarchy);
            Assert.AreEqual(initials.text,UIUtils.CreateInitialsFor(userName));
        }


        [Ignore("needs package bump in multiplayer package")]
        [UnityTest]
        public IEnumerator TopBarUITests_LoggedOut_State()
        {
            //Given the scene is loaded and session is ready
            var uiStateManager = GivenObject<UIStateManager>();
            yield return new WaitWhile(() => uiStateManager.SessionReady() == false);

            //When the user Logs out
            yield return WhenUserLogout();

            //Then the topBar should be inactive
            //TODO assert which objects should be visible / invisible
        }

        [UnityTest]
        public IEnumerator TopBarUITests_LoggedIn_State()
        {
            //Given the scene is loaded and session is ready
            var uiStateManager = GivenObject<UIStateManager>();
            yield return new WaitWhile(() => uiStateManager.SessionReady() == false);

            //Then
            //TODO assert which objects should be visible / invisible
        }

        [UnityTest]
        public IEnumerator TopBarUITests_HelpDialog_Tests()
        {
            yield return null;
        }
    }
}
