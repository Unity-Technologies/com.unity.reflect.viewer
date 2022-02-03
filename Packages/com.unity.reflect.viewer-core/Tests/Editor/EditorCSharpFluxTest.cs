
using System;
using Moq;
using NUnit.Framework;
using SharpFlux;
using SharpFlux.Dispatching;
using SharpFlux.Middleware;
using UnityEngine;

namespace ReflectViewerCoreEditorTests
{
    public class CSharpFluxTests
    {
        public enum TestActionTypes
        {
            Login,
            Logout
        }

        [Test]
        public void CSharpFluxTests_Dispatcher_IsAbleToDispatchEnumActions()
        {
            //Given a callback is registered
            var dispatcher = new Dispatcher();

            var isDelegateCalled = false;
            var delegateMock = new Action<Payload<TestActionTypes>>(c =>
            {
                // Verify the data from the Dispatch
                Assert.IsTrue(c.ActionType == TestActionTypes.Logout);
                Assert.IsTrue(c.Data.Equals("Logging out..."));
                isDelegateCalled = true;
            });

            //When a callback is registered for the Action
            dispatcher.Register(delegateMock);

            //Then I should be able to dispatch Enum actions
            dispatcher.DispatchImplementation(Payload<TestActionTypes>
                .From(TestActionTypes.Logout, "Logging out..."));

            //Verify the callback was invoked
            Assert.IsTrue(isDelegateCalled);
        }

        [Test]
        public void CSharpFluxTests_Dispatcher_RaisesInvalidOperationIfDispatchingIsAlreadyInProgress()
        {
            //Given a callback is registered
            var dispatcher = new Dispatcher();

            var isDelegateCalled = false;
            var delegateMock = new Action<Payload<TestActionTypes>>(c =>
            {
                // Verify the data from the Dispatch
                Assert.IsTrue(c.Data.Equals("Logging out..."));
                // Verify the exception is raised
                Assert.That(() => dispatcher.DispatchImplementation(Payload<TestActionTypes>
                        .From(TestActionTypes.Login, "Logging in...")),
                    Throws.TypeOf<InvalidOperationException>());
                isDelegateCalled = true;
            });

            //When a callback is registered for the Action
            dispatcher.Register(delegateMock);

            //Then I should be able to dispatch Enum actions
            dispatcher.DispatchImplementation(Payload<TestActionTypes>
                .From(TestActionTypes.Logout, "Logging out..."));

            //Verify the callback was invoked
            Assert.IsTrue(isDelegateCalled);
        }

        delegate void MockApplyCallback(ref Payload<TestActionTypes> payload);

        [Test]
        public void CSharpFluxTests_Dispatcher_MiddlewareIsInvoked()
        {
            //Given a callback is registered
            var dispatcher = new Dispatcher();

            Payload<TestActionTypes> action1 = Payload<TestActionTypes>
                .From(TestActionTypes.Login, "Logging in...");
            var middlewareMock = new Mock<IMiddleware<Payload<TestActionTypes>>>();
            middlewareMock.Setup(m => m.Apply(ref action1))
                .RefCallback(new MockApplyCallback((ref Payload<TestActionTypes> payloadToModify) =>
                {
                    payloadToModify = Payload<TestActionTypes>
                        .From(TestActionTypes.Login, "New Data");
                }))
                .Returns(true).Verifiable();

            //When a middleware is registered for the dispatcher
            dispatcher.RegisterMiddlewareImplementation(middlewareMock.Object);

            var isDelegateCalled = false;
            var delegateMock = new Action<Payload<TestActionTypes>>(c =>
            {
                // Verify the data was modified by the middleware
                Assert.IsTrue(c.Data.Equals("New Data"));
                isDelegateCalled = true;
            });

            //When a callback is registered for the Action
            dispatcher.Register(delegateMock);

            //Then I should be able to dispatch Enum actions
            dispatcher.DispatchImplementation(action1);

            //Verify the callback was invoked
            Assert.IsTrue(isDelegateCalled);
            middlewareMock.VerifyAll();
        }

        [Test]
        public void CSharpFluxTests_Dispatcher_IsAbleToDispatchUsingStaticDispatch()
        {
            //Given a callback is registered
            var dispatcher = new Dispatcher();
            Dispatcher.RegisterDefaultDispatcher(dispatcher);

            var isDelegateCalled = false;
            var delegateMock = new Action<Payload<TestActionTypes>>(c =>
            {
                // Verify the data from the Dispatch
                Assert.IsTrue(c.Data.Equals("Logging out..."));
                isDelegateCalled = true;
            });

            //When a callback is registered for the Action
            dispatcher.Register(delegateMock);

            //Then I should be able to dispatch Enum actions
            Dispatcher.Dispatch(Payload<TestActionTypes>
                .From(TestActionTypes.Logout, "Logging out..."));

            //Verify the callback was invoked
            Assert.IsTrue(isDelegateCalled);
        }

        [Test]
        public void CSharpFluxTests_Dispatcher_RaisesExceptionIfNoDefaultDispatchIsRegistered()
        {
            //Given no default Dispatcher registered
            Dispatcher.s_DefaultDispatcher = null;

            //Verify the exception is raised
            Assert.That(() => Dispatcher.Dispatch(Payload<TestActionTypes>
                    .From(TestActionTypes.Login, "Logging in...")),
                Throws.TypeOf<InvalidOperationException>());
        }
    }
}
