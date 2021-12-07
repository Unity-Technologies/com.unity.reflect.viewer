using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;

namespace SharpFlux.Stores
{
    public abstract class StoreBase<TPayload, TData>: IStore<TData>, IDisposable where TData : new()
    {
        readonly object m_SyncRoot = new object();
        readonly IDispatcher m_Dispatcher;

        bool m_Disposed;
        protected TData m_ViewModel;
        public TData Data => m_ViewModel;

        //Returns the dispatch token that the dispatcher recognizes this store by
        //Can be used to WaitFor() this store
        public string DispatchToken { get; private set; }

        //Returns whether the store has changed during the most recent dispatch
        bool m_HasChanged;
        public bool HasChanged
        {
            get => m_HasChanged;
            protected set
            {
                if (!m_Dispatcher.IsDispatching)
                    throw new InvalidOperationException("Must be invoked while dispatching.");
                m_HasChanged = value;
            }
        }

        public StoreBase(IDispatcher dispatcher) : this(dispatcher, new TData())
        {
        }

        public StoreBase(IDispatcher dispatcher, TData initData)
        {
            m_Dispatcher = dispatcher;
            m_ViewModel = initData;
            DispatchToken = dispatcher.Register<TPayload>(InvokeOnDispatch);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;
            if (disposing)
            {
                m_Dispatcher.Unregister(DispatchToken);
            }
            m_Disposed = true;
        }

        protected void WaitFor(IEnumerable<string> dispatchTokens)
        {
            m_Dispatcher.WaitFor<TPayload>(dispatchTokens);
        }

        void InvokeOnDispatch(TPayload payload)
        {
            HasChanged = false;

            lock (m_SyncRoot)
            {
                TryApplyPayload(payload);
            }
            if (HasChanged)
                OnDataChanged?.Invoke();
        }

        /// <summary>
        /// Attempts to apply the dispatched payload and mutate m_ViewModel if applicable 
        /// </summary>
        /// <param name="payload">The latest dispatched payload</param>
        protected abstract void TryApplyPayload(TPayload payload);

        public event Action OnDataChanged;
    }
}
