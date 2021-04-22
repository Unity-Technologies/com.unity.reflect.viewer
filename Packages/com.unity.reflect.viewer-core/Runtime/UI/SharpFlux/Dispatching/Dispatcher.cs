using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Middleware;

namespace SharpFlux.Dispatching
{
    public class Dispatcher : IDispatcher
    {
        private readonly IDictionary<string, object> callbacks = new Dictionary<string, object>();
        private readonly IDictionary<string, bool> isHandledCallbacks = new Dictionary<string, bool>();
        private readonly IDictionary<string, bool> isPendingCallbacks = new Dictionary<string, bool>();
        private readonly IList<object> middlewares = new List<object>();

        private string prefix = "id_";
        private int lastId;
        private object pendingPayload = new object();
        public bool IsDispatching { get; private set; }

        public Dispatcher()
        {
            lastId = 1;
        }

        public string Register<TPayload>(Action<TPayload> callback)
        {
            var dispatchToken = prefix + lastId++;
            callbacks[dispatchToken] = callback;

            return dispatchToken;
        }

        public void DispatchImplementation<TPayload>(TPayload payload)
        {
            if (IsDispatching)
                throw new InvalidOperationException("Cannot dispatch while dispatching");

            try
            {
                StartDispatching(payload);

                // prior to invoking, pass it to any registered middleware (returns false, if it stops invocation)
                if (ApplyMiddleware<TPayload>(ref payload))
                {
                    // Middleware might have modified the payload
                    pendingPayload = payload;
                    foreach (var id in callbacks.Keys)
                    {
                        if (isPendingCallbacks.ContainsKey(id) && isPendingCallbacks[id])
                            continue;

                        InvokeCallback<TPayload>(id);
                    }
                }
            }
            finally
            {
                StopDispatching();
            }
        }

        private void StartDispatching<TPayload>(TPayload payload)
        {
            foreach (var id in callbacks.Keys)
            {
                isPendingCallbacks[id] = false;
                isHandledCallbacks[id] = false;
            }

            pendingPayload = payload;
            IsDispatching = true;
        }

        internal void InvokeCallback<TPayload>(string id)
        {
            isPendingCallbacks[id] = true;

            if (callbacks[id] is Action<TPayload> callback)
                callback((TPayload)pendingPayload);

            isHandledCallbacks[id] = true;
        }

        private bool ApplyMiddleware<TPayload>(ref TPayload payload)
        {
            bool proceedToInvocation = true;
            foreach (IMiddleware<TPayload> mw in middlewares.Where(mv => mv is IMiddleware<TPayload>))
            {
                if (mw.Apply(ref payload) == false)
                {
                    proceedToInvocation = false;
                    break;
                }
            }

            return proceedToInvocation;
        }

        private void StopDispatching()
        {
            pendingPayload = null;
            IsDispatching = false;
        }

        public void WaitFor<TPayload>(IEnumerable<string> dispatchTokens)
        {
            if (!IsDispatching)
                throw new InvalidOperationException("Must be handled when dispatching");

            foreach (var token in dispatchTokens)
            {
                if (isPendingCallbacks[token])
                {
                    if (!isHandledCallbacks[token]) //Store with this token is also waiting for us... Not allowed.
                        throw new InvalidOperationException($"Dispatcher WaitFor: circular dependency detected while waiting for {token}");

                    continue;
                }

                InvokeCallback<TPayload>(token);
            }
        }

        internal static IDispatcher s_DefaultDispatcher;
        public static void RegisterDefaultDispatcher(IDispatcher defaultDispatcher)
        {
            s_DefaultDispatcher = defaultDispatcher;
        }

        public static void Dispatch<TPayload>(TPayload payload)
        {
            if (s_DefaultDispatcher == null)
            {
                throw new InvalidOperationException("No default dispatcher was registered.");
            }

            s_DefaultDispatcher.DispatchImplementation(payload);
        }

        public static void RegisterMiddleware<TPayload>(IMiddleware<TPayload> middleware)
        {
            s_DefaultDispatcher.RegisterMiddlewareImplementation(middleware);
        }

        public void RegisterMiddlewareImplementation<TPayload>(IMiddleware<TPayload> middleware)
        {
            middlewares.Add(middleware);
        }

        public void Unregister(string id)
        {
            if (!callbacks.ContainsKey(id))
                return;

            callbacks.Remove(id);
        }
    }
}
