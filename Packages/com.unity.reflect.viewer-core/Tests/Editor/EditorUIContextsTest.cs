
using System;
using Moq;
using NUnit.Framework;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace ReflectViewerCoreEditorTests
{
    public class UIContextsTests
    {
        [TearDown]
        public void TearDown()
        {
            ApplicationContext.Clear();
        }

        [Serializable, GeneratePropertyBag]
        struct InternalData
        {
            [CreateProperty]
            public bool trueOrFalse { get; set; }
        }

        [Serializable, GeneratePropertyBag]
        struct AlternateData
        {
            [CreateProperty]
            public string aString { get; set; }
            [CreateProperty]
            public bool trueOrFalse { get; set; }
        }

        [Serializable, GeneratePropertyBag]
        struct DifferentData
        {
            [CreateProperty]
            public string aDifferentString { get; set; }
            [CreateProperty]
            public bool nothingTheSame { get; set; }
        }

        [Test]
        public void UIContextsTests_ApplicationContext_IsAbleToRegisterAStruct()
        {
            //Given a defined data struct is registered
            var data = new InternalData();

            //When the data is registered
            ApplicationContext.BindTarget(data);

            //Then I should be able to get the registered properties
            var contains = ApplicationContext.ContainsProperty("trueOrFalse");

            //Verify the property is contained in Context
            Assert.IsTrue(contains);
        }

        [Test]
        public void UIContextsTests_ApplicationContext_IsAbleCallUpdateDelegate()
        {
            //Given a defined data struct is registered
            var updateDelegateWasCalled = false;
            var data = new InternalData();
            var contextTarget = ApplicationContext.BindTarget(data);

            //When a UISelector is created
            var getter = UISelector.createSelector<bool>(ApplicationContext.current, "trueOrFalse", (trueOrFalse) =>
            {
                updateDelegateWasCalled = true;
            });

            //Then I should be able to invoke the update lambda
            data.trueOrFalse = true;
            contextTarget.UpdateWith(ref data);

            //Verify Getter returned True
            Assert.IsTrue(getter(), "Getter should return True");

            //Verify the update lambda was invoked
            Assert.IsTrue(updateDelegateWasCalled, "Verify the update lambda was invoked");
        }

        [Test]
        public void UIContextsTests_ApplicationContext_IsAbleSupportTwoPropertiesWithOneValueChanged()
        {
            //Given a defined data struct is registered
            var updateDelegateWasCalled = 0;
            var data = new AlternateData();
            var contextTarget = ApplicationContext.BindTarget(data);

            //When a UISelector is created
            var getter = UISelector.createSelector<bool>(ApplicationContext.current, "trueOrFalse", (trueOrFalse) =>
            {
                updateDelegateWasCalled++;
            });

            var getterString = UISelector.createSelector<string>(ApplicationContext.current, "aString", (aString) =>
            {
                updateDelegateWasCalled++;
            });

            //Then I should be able to invoke the update lambda
            data.trueOrFalse = true;
            data.aString = "changed";
            contextTarget.UpdateWith(ref data);

            //Verify Getter returned True
            Assert.IsTrue(getter(), "Getter should return True");

            //Verify Getter returned "changed"
            Assert.IsTrue(getterString().Equals("changed"), "Getter should return 'changed'");

            //Verify the update lambda was invoked
            Assert.IsTrue(updateDelegateWasCalled == 2, "Verify the update lambdas were invoked each");
        }

        [Test]
        public void UIContextsTests_ApplicationContext_IsAbleSupportTwoContextTwoPropertiesWithEachOneValueChanged()
        {
            //Given a defined data struct is registered
            var updateDelegateWasCalled1 = 0;
            var updateDelegateWasCalled2 = 0;
            var data1 = new InternalData();
            var contextTarget1 = ApplicationContext.BindTarget(data1);

            var data2 = new DifferentData();
            var contextTarget2 = DebugOptionContext.BindTarget(data2);

            //When a UISelector is created
            var getter = UISelector.createSelector<bool>(ApplicationContext.current, "trueOrFalse", (trueOrFalse) =>
            {
                updateDelegateWasCalled1++;
            });

            var getterString = UISelector.createSelector<string>(DebugOptionContext.current, "aDifferentString", (aString) =>
            {
                updateDelegateWasCalled2++;
            });

            //Then I should be able to invoke the update lambda
            data1.trueOrFalse = true;
            contextTarget1.UpdateWith(ref data1);


            data2.aDifferentString = "changed";
            contextTarget2.UpdateWith(ref data2);

            //Verify Getter returned True
            Assert.IsTrue(getter(), "Getter should return True");

            //Verify Getter returned "changed"
            Assert.IsTrue(getterString().Equals("changed"), "Getter should return 'changed'");

            //Verify the update lambda was invoked
            Assert.IsTrue(updateDelegateWasCalled1 == 1, "Verify the update lambdas were invoked ApplicationContext");

            Assert.IsTrue(updateDelegateWasCalled1 == 1, "Verify the update lambdas were invoked DebugContext");
        }
    }
}
