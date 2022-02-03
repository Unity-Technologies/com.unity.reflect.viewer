using System;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.Core
{
    [DefaultExecutionOrder(-100)]
    public abstract class UnityStoreContainer<TModel, TContext> : MonoBehaviour where TContext : ContextBase<TContext>, new() where TModel : new()
    {
        [SerializeField, Tooltip("State of the ViewModel")]
        [ContextButton("Value Changed",nameof(OnModelChanged))]
        private TModel m_ViewModel;

        Store<TModel, TContext> m_Store;

        protected virtual void Awake()
        {
            Setup(DispatcherFactory.GetDispatcher());
        }

        void Setup(IDispatcher dispatcher)
        {
            DisposeStore();

            m_Store = new Store<TModel, TContext>(dispatcher, m_ViewModel);
            m_Store.OnDataChanged += Store_OnDataChanged;
        }

        void DisposeStore()
        {
            if (m_Store != null)
            {
                m_Store.OnDataChanged -= Store_OnDataChanged;
                m_Store.Dispose();
                m_Store = null;
            }
        }

        protected virtual void OnDestroy()
        {
            DisposeStore();
        }

        void Store_OnDataChanged()
        {
            m_ViewModel = m_Store.Data;
        }

        protected virtual void OnModelChanged() => m_Store?.UpdateData(m_ViewModel);
    }
}
