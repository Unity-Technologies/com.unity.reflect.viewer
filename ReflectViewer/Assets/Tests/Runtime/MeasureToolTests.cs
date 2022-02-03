using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Reflect;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.TestTools;

namespace ReflectViewerRuntimeTests
{
    public class MeasureToolTests: BaseReflectSceneTests
    {
        class AllowMeasureToolAction: ActionBase
        {
            public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
            {
                object boxed = stateData;
                SetPropertyValue(ref stateData, ref boxed, nameof(IToolBarDataProvider.toolbarsEnabled), viewerActionData);
                onStateDataChanged?.Invoke();
            }

            public static Payload<IViewerAction> From(bool data)
           => Payload<IViewerAction>.From(new AllowMeasureToolAction(), data);

            public override bool RequiresContext(IUIContext context, object viewerActionData)
            {
                return context == UIStateContext.current;
            }
        }

        class ToggleMeasureToolAction: ActionBase
        {
            public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
            {
                object boxed = stateData;
                IMeasureToolDataProvider stateProvider = boxed as IMeasureToolDataProvider;
                SetPropertyValue(ref stateData, ref boxed, nameof(IMeasureToolDataProvider.toolState), viewerActionData);

                onStateDataChanged?.Invoke();
            }

            public static Payload<IViewerAction> From(bool data)
           => Payload<IViewerAction>.From(new ToggleMeasureToolAction(), data);

            public override bool RequiresContext(IUIContext context, object viewerActionData)
            {
                return context == MeasureToolContext.current;
            }
        }

        //[UnityTest] // FixMe For some reason this test passes on MAC but fails on Windows (on Yamato)
        public IEnumerator MeasureToolTests_LeftSidePanelToggleTest()
        {
            yield return WaitAFrame(); //to let all needed listeners to be added
            using (var canBeToggledGetter = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled)))
            using (var isToolActiveGetter = UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState)))
            {
                var uiRoot = GameObject.Find("UI Root");
                Assert.IsNotNull(uiRoot);
                var sideBar = uiRoot.GetComponentInChildren<LeftSideBarController>(true);
                Assert.IsNotNull(sideBar);

                Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.OrbitSidebar));
                Assert.IsTrue(sideBar.gameObject.activeSelf);
                yield return WaitAFrame(); //to let all needed listeners to be added

                var measureButton = typeof(LeftSideBarController).
                    GetField("m_MeasureToolButton", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).
                    GetValue(sideBar) as ToolButton;

                Assert.IsNotNull(measureButton);

                Assert.IsFalse(canBeToggledGetter.GetValue());
                Assert.IsFalse(isToolActiveGetter.GetValue());
                Assert.IsFalse(measureButton.selected);

                Dispatcher.Dispatch(AllowMeasureToolAction.From(true));
                Assert.IsTrue(canBeToggledGetter.GetValue());
                Assert.IsFalse(isToolActiveGetter.GetValue());
                Assert.IsTrue(measureButton.button.interactable);
                Assert.IsFalse(measureButton.selected);

                Dispatcher.Dispatch(ToggleMeasureToolAction.From(true));
                Assert.IsTrue(canBeToggledGetter.GetValue());
                Assert.IsTrue(isToolActiveGetter.GetValue());
                Assert.IsTrue(measureButton.button.interactable);
                Assert.IsTrue(measureButton.selected);

                Dispatcher.Dispatch(ToggleMeasureToolAction.From(false));
                Assert.IsTrue(canBeToggledGetter.GetValue());
                Assert.IsFalse(isToolActiveGetter.GetValue());
                Assert.IsTrue(measureButton.button.interactable);
                Assert.IsFalse(measureButton.selected);

                measureButton.button.onClick.Invoke();
                Assert.IsTrue(measureButton.button.interactable);
                Assert.IsTrue(measureButton.selected);
                Assert.IsTrue(canBeToggledGetter.GetValue());
                Assert.IsTrue(isToolActiveGetter.GetValue());

                measureButton.button.onClick.Invoke();
                Assert.IsTrue(canBeToggledGetter.GetValue());
                Assert.IsFalse(isToolActiveGetter.GetValue());
                Assert.IsTrue(measureButton.button.interactable);
                Assert.IsFalse(measureButton.selected);

                Dispatcher.Dispatch(AllowMeasureToolAction.From(false));
                Assert.IsFalse(canBeToggledGetter.GetValue());
                Assert.IsFalse(isToolActiveGetter.GetValue());
                Assert.IsFalse(measureButton.selected);
            }
        }

        [Category("YamatoIncompatible")]
        [UnityTest]
        public IEnumerator MeasureToolTests_OnOffFlowTest()
        {
            bool userLoggedIn = false;
            bool canBeToggledChanged;
            using (var canBeToggledGetter = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled),
                (data) => canBeToggledChanged = true))
            using (var isToolActiveGetter = UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState)))
            {
                //Reflect is just loaded and not logged in. Measure tool must be not active nor able to be activated
                Assert.IsFalse(canBeToggledGetter.GetValue());
                Assert.IsFalse(isToolActiveGetter.GetValue());
                try
                {
                    yield return GivenUserIsLoggedIn();
                    userLoggedIn = true;
                    //Reflect is logged in. Measure tool must be not active nor able to be activated
                    Assert.IsFalse(canBeToggledGetter.GetValue());
                    Assert.IsFalse(isToolActiveGetter.GetValue());

                    //Openning a project
                    ProjectListState? listState = null;
                    using (var projectListStateSelector = UISelectorFactory.createSelector<ProjectListState>(SessionStateContext<UnityUser, LinkPermission>.current,
                        nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.projectListState),
                        (data) => listState = data))
                    {
                        yield return new WaitUntil(() => listState == ProjectListState.Ready);
                    }
                    IProjectRoom room;
                    using (var roomSelector = UISelectorFactory.createSelector<IProjectRoom[]>(SessionStateContext<UnityUser, LinkPermission>.current,
                                                                nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.rooms)))
                    {
                        room = roomSelector.GetValue().FirstOrDefault(r => (r is ProjectRoom pr) && pr.project != null);
                    }
                    if (room == null)
                    {
                        throw new InvalidOperationException("There must be a project room with a project available to continue the test");
                    }

                    //When project is just opened measure tool must be available, but not selected
                    Dispatcher.Dispatch(OpenProjectActions<Project>.From(((ProjectRoom)room).project));
                    canBeToggledChanged = false;
                    yield return new WaitWhile(() => !canBeToggledChanged); //if state fails to update means test fails; timeout will stop the test
                    Assert.IsTrue(canBeToggledGetter.GetValue());
                    Assert.IsFalse(isToolActiveGetter.GetValue());

                    Dispatcher.Dispatch(ToggleMeasureToolAction.From(true));
                    Assert.IsTrue(isToolActiveGetter.GetValue());
                    Assert.IsTrue(canBeToggledGetter.GetValue());

                    Dispatcher.Dispatch(ToggleMeasureToolAction.From(false));
                    Assert.IsFalse(isToolActiveGetter.GetValue());
                    Assert.IsTrue(canBeToggledGetter.GetValue());

                    //If project is closed, measure tool must be deactivated and not available
                    Dispatcher.Dispatch(ToggleMeasureToolAction.From(true));
                    Dispatcher.Dispatch(CloseProjectActions<Project>.From(Project.Empty));
                    Assert.IsFalse(canBeToggledGetter.GetValue());
                    Assert.IsFalse(isToolActiveGetter.GetValue());

                    //If user logged out, measure tool must be deactivated and not available
                    yield return WhenUserLogout();
                    userLoggedIn = false;
                    Assert.IsFalse(canBeToggledGetter.GetValue());
                    Assert.IsFalse(isToolActiveGetter.GetValue());
                }
                finally
                {
                    if (userLoggedIn)
                    {
                        var loginManager = GameObject.FindObjectOfType<LoginManager>();
                        loginManager.userLoggedOut.Invoke();
                    }
                }
            }
        }
    }
}
