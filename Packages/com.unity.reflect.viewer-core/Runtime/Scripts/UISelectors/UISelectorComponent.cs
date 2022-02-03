using UnityEngine;
using System;
using System.Linq;

namespace UnityEngine.Reflect.Viewer.Core
{
    [DisallowMultipleComponent]
    public class UISelectorComponent : MonoBehaviour, ISelectorComponent
    {
#pragma warning disable 649
        [SerializeField]
        public string ContextTypeName = "ApplicationContext";

        [SerializeField]
        public string PropertyName;
#pragma warning restore 649
        IUIContext m_RequiredContext = ApplicationContext.current;

        public IUIContext context => m_RequiredContext;
        public string propertyName => PropertyName;
        public Type ProperType => typeof(string);

        void Awake()
        {
            BindContext();
        }

        public void BindContext()
        {
            var contextType = ContextResolver.GetContextTypes().FirstOrDefault(x => Enumerable.Last<string>(x.FullName.Split('.')).Equals(ContextTypeName));
            m_RequiredContext = ContextResolver.GetCurrentInstanceFromContextType(contextType);
        }
    }
}
