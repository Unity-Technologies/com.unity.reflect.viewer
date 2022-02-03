using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Middleware;

namespace SharpFlux.Dispatching
{
    public static class Dispatcher
    {
        static IDispatcher s_DefaultDispatcher;

        static void CheckDefaultDipatcher()
        {
            if (s_DefaultDispatcher == null)
            {
                throw new InvalidOperationException("No default dispatcher was registered.");
            }
        }

        internal static void RegisterDefaultDispatcher(IDispatcher defaultDispatcher)
        {
            s_DefaultDispatcher = defaultDispatcher;
        }

        public static void Dispatch<TPayload>(TPayload payload)
        {
            CheckDefaultDipatcher();
            s_DefaultDispatcher.DispatchImplementation(payload);
        }

        public static void RegisterMiddleware<TPayload>(IMiddleware<TPayload> middleware)
        {
            CheckDefaultDipatcher();
            s_DefaultDispatcher.RegisterMiddlewareImplementation(middleware);
        }
    }
}
