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
        void Dispatch<TPayload>(TPayload payload);
        void WaitFor<TPayload>(IEnumerable<string> dispatchTokens);
        void RegisterMiddleware<TPayload>(IMiddleware<TPayload> middleware);
    }
}
