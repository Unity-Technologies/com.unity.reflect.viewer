using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VivoxUnity;

namespace VivoxTests
{
    public class CustomTestInterop : VxUnityInterop
    {
        public bool fired;

        // Setting up Unity Coroutine to run on the main thread 
        public override void StartVivoxUnity()
        {
            Debug.Log("Override of StartVivoxUnity is firing!");
            fired = true;
        }
    }

    public class VivoxUnityInteropTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void VivoxUnityInteropSimpleOverride()
        {
            var myInterop = new CustomTestInterop();
            // Use the Assert class to test conditions
            myInterop.StartVivoxUnity();
            Assert.IsTrue(myInterop.fired);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator VivoxUnityInteropOverrideVivoxInit()
        {

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            var singletonObject = new GameObject();
            singletonObject.AddComponent<CustomTestInterop>();

            Client _client = new Client();
            _client.Uninitialize();
            yield return null;

            _client.Initialize();
            yield return null;

            Assert.IsTrue(singletonObject.GetComponent<CustomTestInterop>().fired);
            _client.Uninitialize();

            GameObject.Destroy(singletonObject);
        }
    }
}
