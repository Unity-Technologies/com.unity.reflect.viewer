using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace ReflectViewerRuntimeTests
{
    public class FollowCameraTests : BaseReflectSceneTests
    {
        NetworkUserData user = new NetworkUserData()
        {
            matchmakerId = "11",
            lastUpdateTimeStamp = DateTime.Now
        };

        [Ignore("needs package update of multiplayer package")]
        [UnityTest]
        public IEnumerator Multiplayer_WhenFollowCameraButtonClicked_CameraFollowsSelectedUser()
        {
            //Given a user in a certain position and rotation
            var position = new Vector3(5, 5, 5);
            var rotation = Quaternion.FromToRotation(Vector3.zero, Vector3.up);
            Camera mainCamera = GivenObjectNamed<Camera>("Main Camera");
            var freeflyCamera = mainCamera.GetComponent<FreeFlyCamera>();
            yield return WaitAFrame();
            freeflyCamera.enabled = true;
            yield return WaitAFrame();
            AddUserToRoom(user);
            var userObject = GivenObjects<UserUIButton>().Find((userCtrl) => userCtrl.MatchmakerId == user.matchmakerId);
            var userData = UIStateManager.current.roomConnectionStateData.users.Find(u => u.matchmakerId == userObject.MatchmakerId);
            var objectToFollow = userData.visualRepresentation.gameObject;
            objectToFollow.transform.position = position;
            objectToFollow.transform.rotation = rotation;

            //When clicking on the avatar
            userObject.m_Button.onClick.Invoke();
            yield return WaitAFrame();

            //Then the user info popup should show
            Assert.True(IsDialogOpen("CollaborationUserInfoDialog"));
            var userDialog = GivenObjectNamed<UserDetailsUIController>("CollaborationUserInfoDialog");

            //When click on "follow user" button
            userDialog.m_FollowCameraButton.onClick.Invoke();
            yield return WaitAFrame();

            //Then the camera should jump to its position and rotation. Then userinfo dialog should be closed
            Assert.That(mainCamera.transform.position.Equals(position));
            Assert.That(mainCamera.transform.rotation.Equals(rotation));
            Assert.False(IsDialogOpen("CollaborationUserInfoDialog"));

            //When the user moves its position and rotation
            position = new Vector3(10, 10, 10);
            rotation = Quaternion.FromToRotation(Vector3.zero, Vector3.down);
            objectToFollow.transform.position = position;
            objectToFollow.transform.rotation = rotation;
            yield return WaitAFrame();

            //Then camera should follow it
            Assert.That(mainCamera.transform.position.Equals(position));
            Assert.That(mainCamera.transform.rotation.Equals(rotation));
        }

        [Ignore("needs package update of multiplayer package")]
        [UnityTest]
        public IEnumerator FollowCamera_IfObjectToFollowIsDestroyed_StayInPreviousPosition()
        {
            //Given a user in a certain position and rotation
            var position = new Vector3(5, 5, 5);
            var rotation = Quaternion.FromToRotation(Vector3.zero, Vector3.up);
            Camera mainCamera = GivenObjectNamed<Camera>("Main Camera");
            var freeFlyCamera = mainCamera.GetComponent<FreeFlyCamera>();
            freeFlyCamera.enabled = true;
            yield return WaitAFrame();
            AddUserToRoom(user);
            yield return WaitAFrame();
            var userObject = GivenObjects<UserUIButton>().Find((userCtrl) => userCtrl.MatchmakerId == user.matchmakerId);
            var userData = UIStateManager.current.roomConnectionStateData.users.Find(u => u.matchmakerId == userObject.MatchmakerId);
            var objectToFollow = userData.visualRepresentation;
            objectToFollow.transform.position = position;
            objectToFollow.transform.rotation = rotation;
            Assert.NotNull(objectToFollow);
            var userDialog = GivenObjectNamed<UserDetailsUIController>("CollaborationUserInfoDialog");

            //When opening the user info dialog and clicking on "follow user" button
            userObject.m_Button.onClick.Invoke();
            yield return WaitAFrame();
            userDialog.m_FollowCameraButton.onClick.Invoke();
            yield return WaitAFrame();

            //Then the camera should jump to its position and rotation
            Assert.That(mainCamera.transform.position.Equals(position));
            Assert.That(mainCamera.transform.rotation.Equals(rotation));

            //When the user is destroyed (even if position changed)
            objectToFollow.transform.position = new Vector3(10, 10, 10);;
            objectToFollow.transform.rotation = Quaternion.FromToRotation(Vector3.zero, Vector3.down);
            GameObject.DestroyImmediate(objectToFollow);
            yield return WaitAFrame();
            yield return WaitAFrame();

            //Then camera should stay on previous position
            Assert.That(mainCamera.transform.position.Equals(position));
            Assert.That(mainCamera.transform.rotation.Equals(rotation));
        }

        [Ignore("needs fix for teleporting in vr")]
        [UnityTest]
        public IEnumerator  Multiplayer_WhenFollowCameraButtonClicked_And_InVRMode_JumpInsteadOfFollow()
        {
            //Given a user in a certain position and rotation
            yield return WaitAFrame();
            var position = new Vector3(5, 5, 5);
            var rotation = Quaternion.FromToRotation(Vector3.zero, Vector3.up);
            Camera mainCamera = GivenObjectNamed<Camera>("Main Camera");
            var freeFlyCamera = mainCamera.GetComponent<FreeFlyCamera>();
            mainCamera.transform.rotation = Quaternion.identity;
            mainCamera.transform.position = Vector3.zero;
            yield return WaitAFrame();
            freeFlyCamera.enabled = true;
            yield return WaitAFrame();
            AddUserToRoom(user);
            var userObject = GivenObjects<UserUIButton>().Find((userCtrl) => userCtrl.MatchmakerId == user.matchmakerId);
            var userData = UIStateManager.current.roomConnectionStateData.users.Find(u => u.matchmakerId == userObject.MatchmakerId);
            var objectToFollow = userData.visualRepresentation;
            objectToFollow.transform.position = position;
            objectToFollow.transform.rotation = rotation;

            //Given the app is in VR mode
            GivenViewerIsInVRMode();

            //When clicking on the avatar
            userObject.m_Button.onClick.Invoke();
            yield return WaitAFrame();

            //Then the user info popup should show
            Assert.True(IsDialogOpen("CollaborationUserInfoDialog"));
            var userDialog = GivenObjectNamed<UserDetailsUIController>("CollaborationUserInfoDialog");

            //When click on "follow user" button
            userDialog.m_FollowCameraButton.onClick.Invoke();
            yield return WaitAFrame();
            yield return new WaitWhile(() => freeFlyCamera.enabled == false);

            //Then camera should jump to its user
            Assert.That(mainCamera.transform.position.Equals(position));

            //When the user moves
            objectToFollow.transform.position = new Vector3(10, 10, 10);;
            objectToFollow.transform.rotation = Quaternion.FromToRotation(Vector3.zero, Vector3.down);
            yield return WaitAFrame();

            //Then the current user should stayed in the previous position
            Assert.That(mainCamera.transform.position.Equals(position));
        }
    }
}
