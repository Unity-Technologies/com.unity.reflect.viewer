using System;
using System.Collections.Generic;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IUIContext
    {
        List<Type> implementsInterfaces { get; }
        IContextTarget target { get; }
        bool ContainsProperty(string propertyName);
        event Action stateChanged;
        Func<object> GetPropertyGetter(string name);
    }

    public class ContextButtonAttribute : PropertyAttribute
    {
        public string MethodToInvoke;
        public string ButtonLabel;
        public ContextButtonAttribute(string buttonLabel, string methodToInvoke)
        {
            ButtonLabel = buttonLabel;
            MethodToInvoke = methodToInvoke;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class UIContextPropertiesAttribute : Attribute
    {
        public string contextName { get; set; }
        public UIContextPropertiesAttribute(string contextName)
        {
            this.contextName = contextName;
        }
    }

    [AttributeUsage(AttributeTargets.Class,
                   AllowMultiple = true)]
    public class UIStoreContextPropertiesAttribute : Attribute
    {
        public Type stateDataType { get; set; }
        public Type contextType { get; set; }
        public UIStoreContextPropertiesAttribute(Type stateDataType, Type contextDataType)
        {
            this.contextType = contextDataType;
            this.stateDataType = stateDataType;
        }
    }

    public interface IContextPropertyProvider
    {
        public class ContextPropertyData
        {
            public Type context;
            public IProperty property;
        }

        IEnumerable<ContextPropertyData> GetProperties();
    }
}
