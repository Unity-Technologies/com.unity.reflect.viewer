using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class ReflectAction<TContext> : ActionBase where TContext : ContextBase<TContext>, new()
    {
        public sealed override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ContextBase<TContext>.current;
        }
    }

    public abstract class ReflectAction<TStateData, TContext> : ActionBase<TStateData> where TContext : ContextBase<TContext>, new()
    {
        public sealed override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ContextBase<TContext>.current;
        }
    }

    public abstract class ReflectAction<TActionData, TStateData, TContext> : ActionBase<TActionData, TStateData> where TContext : ContextBase<TContext>, new()
    {
        public sealed override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ContextBase<TContext>.current;
        }
    }

    /// <summary>
    /// Sets payload data into a specified property of state data from specified context
    /// </summary>
    public abstract class ReflectSetPropertyAction<TContext> : SetPropertyAction where TContext : ContextBase<TContext>, new()
    {
        public sealed override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ContextBase<TContext>.current;
        }
    }

    /// <summary>
    /// Sets payload data into a specified property of state data from specified context
    /// </summary>
    public sealed class ReflectSetPropertyAction : SetPropertyAction
    {
        IUIContext m_Context;
        string m_PropertyName;
        ReflectSetPropertyAction(string propertyName, IUIContext context) : base()
        {
            m_Context = context;
            m_PropertyName = propertyName;
        }

        public sealed override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == m_Context;
        }

        protected override string GetTargetPropertyName()
        {
            return m_PropertyName;
        }

        public static Payload<IViewerAction> From(object data, string property, IUIContext contex)
                => Payload<IViewerAction>.From(new ReflectSetPropertyAction(property, contex), data);

        public static Payload<IViewerAction> From<TContext>(object data, string property) where TContext : ContextBase<TContext>, new()
                => Payload<IViewerAction>.From(new ReflectSetPropertyAction(property, ContextBase<TContext>.current), data);
    }
}
