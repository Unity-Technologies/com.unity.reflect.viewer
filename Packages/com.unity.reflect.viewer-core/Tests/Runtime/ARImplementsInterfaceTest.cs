using System;
using System.Collections;
using NUnit.Framework;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.TestTools;

namespace ReflectViewerCoreRuntimeTests
{
    [Serializable, GeneratePropertyBag]
    struct TestData : IARModeDataProvider
    {
        [CreateProperty]
        public SetARModeAction.ARMode arMode { get; set; }

        public int instructionUIStep { get; set; }
    }

    struct TestData2
    {
        [CreateProperty]
        public SetARModeAction.ARMode arMode { get; set; }
    }

    public class ARImplementsInterfaceTest: BaseRuntimeTests
    {
        [UnityTest]
        public IEnumerator CreateARModeSelector_WithInterface()
        {
            var data = new TestData();
            var contextTarget = ARContext.BindTarget(data);
            yield return WaitAFrame();

            var arModeGetter = UISelector.createSelector<SetARModeAction.ARMode>(ARContext.current, nameof(IARModeDataProvider.arMode));
            yield return WaitAFrame();

            data.arMode = SetARModeAction.ARMode.WallBased;
            contextTarget.UpdateWith(ref data);
            yield return WaitAFrame();

            Assert.IsTrue(arModeGetter().Equals(SetARModeAction.ARMode.WallBased), "Getter should return WallBased AR Mode");
            yield return WaitAFrame();
        }

        [UnityTest]
        public IEnumerator CreateARModeSelector_MissingInterface()
        {
            var data = new TestData2();
            var contextTarget = ARContext.BindTarget(data);
            yield return WaitAFrame();

            var arModeGetter = UISelector.createSelector<SetARModeAction.ARMode>(ARContext.current, nameof(IARModeDataProvider.arMode));
            LogAssert.Expect(LogType.Warning, "The data type of " + data.GetType() + " does not implement the interface " + nameof(IARModeDataProvider));

            yield return WaitAFrame();
        }
    }
}
