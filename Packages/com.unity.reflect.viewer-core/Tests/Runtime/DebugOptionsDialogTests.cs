using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.TestTools;

namespace ReflectViewerCoreRuntimeTests
{
    public class DebugOptionsDialogTests : BaseExampleSceneTests
    {
        [UnityTest]
        public IEnumerator DebugOptions_InitialValuesAreUpdatedOnStart()
        {
            //Given a runtime that has started

            //When Start() as executed
            yield return WaitAFrame();

            //Then the debug options UGUI widgets should have the starting value
            Assert.True(IsDialogOpen("DebugOptionsDialog"));
            var listItem = GivenGameObjectNamed("Text List Item");
            var valueText = GivenObjectsInChildren<TMPro.TMP_Text>(listItem).First(c => c.name == "Value Text");
            var getter = UISelector.createSelector<string>(ApplicationContext.current, nameof(IQualitySettingsDataProvider.qualityLevel));
            var initialValue = getter();
            Assert.AreEqual(initialValue, valueText.text);
        }

        [UnityTest]
        public IEnumerator DebugOptions_QualityLevelIsUpdatedTwoWay()
        {
            //Given a runtime that has started

            //When Start() as executed and we change the value of the state
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new { qualityLevel = "Test" }));
            yield return WaitAFrame();

            //Then the Quality String option TMPRO should have the changed value
            Assert.True(IsDialogOpen("DebugOptionsDialog"));
            var listItem = GivenGameObjectNamed("Text List Item");
            var valueText = GivenObjectsInChildren<TMPro.TMP_Text>(listItem).First(c => c.name == "Value Text");
            Assert.AreEqual(valueText.text, "Test");
            //Then the Quality String option in global state should have the changed value
            var getter = UISelector.createSelector<string>(ApplicationContext.current, nameof(IQualitySettingsDataProvider.qualityLevel));
            var newValue = getter();
            Assert.AreEqual(newValue, "Test");
        }
    }
}
