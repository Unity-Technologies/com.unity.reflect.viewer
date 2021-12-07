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
}
