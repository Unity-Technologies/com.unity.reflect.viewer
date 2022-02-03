using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.TestTools;

namespace ReflectViewerCoreRuntimeTests
{
    public class ListValueChangedTest : BaseRuntimeTests
    {
        [Serializable, GeneratePropertyBag]
        public struct TestData : IEquatable<TestData>
        {
            [CreateProperty] public List<int> testList { get; set; }

            public bool Equals(TestData other)
            {
                if (testList != null && other.testList != null)
                    return testList.SequenceEqual(other.testList);
                return testList == null && other.testList == null;
            }

            public override bool Equals(object obj)
            {
                return obj is TestData other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (testList != null ? testList.GetHashCode() : 0);
            }
        }

        [UnityTest]
        public IEnumerator UIContextsTests_ApplicationContext_ListForceNotify_LenghtChange()
        {
            int count = 0;

            //Given a defined data struct is registered
            var data = new TestData();
            data.testList = new List<int>{1};
            var contextTarget = ApplicationContext.BindTarget(data);
            yield return WaitAFrame();

            //When a UISelector is created
            UISelector.createSelector<List<int>>(ApplicationContext.current, "testList", (testList) =>
            {
                count++;
            });
            yield return WaitAFrame();

            //Then update the List and UpdateWith new value
            data.testList.Add(2);
            contextTarget.UpdateWith(ref data, UpdateNotification.ForceNotify);
            yield return WaitAFrame();

            //Verify the update lambda was invoked
            Assert.IsTrue(count == 2, "Verify the update lambda was invoked");
            yield return WaitAFrame();
        }

        [UnityTest]
        public IEnumerator UIContextsTests_ApplicationContext_ListForceNotify_ValueChange()
        {
            int count = 0;

            //Given a defined data struct is registered
            var data = new TestData();
            data.testList = new List<int>{1};
            var contextTarget = ApplicationContext.BindTarget(data);
            yield return WaitAFrame();

            //When a UISelector is created
            UISelector.createSelector<List<int>>(ApplicationContext.current, "testList", (testList) =>
            {
                count++;
            });
            yield return WaitAFrame();

            //Then update the List and UpdateWith
            data.testList[0] = 2;
            contextTarget.UpdateWith(ref data, UpdateNotification.ForceNotify);
            yield return WaitAFrame();

            //Verify the update lambda was invoked
            Assert.IsTrue(count == 2, "Verify the update lambda was invoked");
            yield return WaitAFrame();
        }

        [UnityTest]
        public IEnumerator UIContextsTests_ApplicationContext_ListForceNotify_NoChange()
        {
            int count = 0;

            //Given a defined data struct is registered
            var data = new TestData();
            data.testList = new List<int>{1};
            data.testList.Add(1);
            var contextTarget = ApplicationContext.BindTarget(data);
            yield return WaitAFrame();

            //When a UISelector is created
            UISelector.createSelector<List<int>>(ApplicationContext.current, "testList", (testList) =>
            {
                count++;
            });
            yield return WaitAFrame();

            //Then call UpdateWith without updating the List
            contextTarget.UpdateWith(ref data, UpdateNotification.ForceNotify);
            yield return WaitAFrame();

            //Verify the update lambda was not invoked
            Assert.IsTrue(count == 1, "Verify the update lambda was invoked");
            yield return WaitAFrame();
        }
    }
}
