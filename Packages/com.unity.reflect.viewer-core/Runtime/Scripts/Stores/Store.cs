using System;
using SharpFlux;
using SharpFlux.Dispatching;
using SharpFlux.Stores;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class Store<TData, TContext>: StoreBase<Payload<IViewerAction>, TData>, IDisposable where TContext : ContextBase<TContext>, new() where TData : new()
    {
        IContextTarget m_ContextTarget;

        public Store(IDispatcher dispatcher) : base(dispatcher)
        {
            CreateContextTarget();
        }

        public Store(IDispatcher dispatcher, TData initData) : base(dispatcher, initData)
        {
            CreateContextTarget();
        }

        void CreateContextTarget()
        {
            m_ContextTarget = ContextBase<TContext>.BindTarget(Data);
        }

        public void UpdateData(TData newData)
        {
            m_ViewModel = newData;
            m_ContextTarget.UpdateWith(ref m_ViewModel);
        }

        private T HasContextTargetChanged<T>(Payload<IViewerAction> viewerAction, IUIContext context, T stateData, IContextTarget contextTarget)
        {
            if (viewerAction.ActionType.RequiresContext(context, viewerAction.Data))
            {
                viewerAction.ActionType.ApplyPayload(viewerAction.Data, ref stateData, () =>
                {
                    contextTarget.UpdateWith(ref stateData);
                    HasChanged = true;
                });
            }

            return stateData;
        }

        protected override void TryApplyPayload(Payload<IViewerAction> payload)
        {
            m_ViewModel = HasContextTargetChanged(payload, ContextBase<TContext>.current, m_ViewModel, m_ContextTarget);
        }
    }
}
