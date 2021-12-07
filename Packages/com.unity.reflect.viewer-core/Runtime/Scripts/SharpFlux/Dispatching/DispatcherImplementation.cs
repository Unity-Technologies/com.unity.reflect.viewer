using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Middleware;

namespace SharpFlux.Dispatching
{
    internal class DispatcherImplementation : IDispatcher
    {
        readonly IDictionary<string, object> callbacks = new Dictionary<string, object>();
        readonly IDictionary<string, bool> isHandledCallbacks = new Dictionary<string, bool>();
        readonly IDictionary<string, bool> isPendingCallbacks = new Dictionary<string, bool>();
        readonly IList<object> middlewares = new List<object>();

        string prefix = "id_";
        int lastId;
        object pendingPayload = new object();
        public bool IsDispatching { get; private set; }

        internal DispatcherImplementation()
        {
            lastId = 1;
        }

        void StartDispatching<TPayload>(TPayload payload)
        {
            foreach (var id in callbacks.Keys)
            {
                isPendingCallbacks[id] = false;
                isHandledCallbacks[id] = false;
            }

            pendingPayload = payload;
            IsDispatching = true;
        }

        void InvokeCallback<TPayload>(string id)
        {
            isPendingCallbacks[id] = true;

            if (callbacks[id] is Action<TPayload> callback)
                callback((TPayload)pendingPayload);

            isHandledCallbacks[id] = true;
        }

        bool ApplyMiddleware<TPayload>(ref TPayload payload)
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

        void StopDispatching()
        {
            pendingPayload = null;
            IsDispatching = false;
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
