using System;
using System.Collections.Generic;
using SharpFlux.Middleware;

namespace SharpFlux.Dispatching
{
    public interface IDispatcher
    {
        bool IsDispatching { get; }

        string Register<TPayload>(Action<TPayload> callback);
        void Unregister(string dispatchToken);
        void DispatchImplementation<TPayload>(TPayload payload);
        void WaitFor<TPayload>(IEnumerable<string> dispatchTokens);
        void RegisterMiddlewareImplementation<TPayload>(IMiddleware<TPayload> middleware);
    }
}
