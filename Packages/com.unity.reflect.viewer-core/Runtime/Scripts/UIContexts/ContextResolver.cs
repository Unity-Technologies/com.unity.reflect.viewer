using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class ContextResolver
    {
        class StateToContextBindingInfo
        {
            public Type stateDataType;
            public string contextName;
        }

        static List<Type> s_ContextTypes;
        static List<StateToContextBindingInfo> s_ContextPropertiesAttributeTypes;

        public static List<Type> GetContextTypes()
        {
            if (s_ContextTypes == null)
            {
                s_ContextTypes = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(type =>
                        !type.IsAbstract &&
                        (type.GetInterfaces().Contains(typeof(IUIContext))))
                    .ToList();
            }

            return s_ContextTypes;
        }

        public static IUIContext GetCurrentInstanceFromContextType(Type contextType)
        {
            // invalid contextType
            if (contextType.BaseType.GenericTypeArguments[0] != contextType)
                return null;

            var currentInstanceGetter = contextType.BaseType.GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.PropertyType == contextType)
                .Select(x => x.GetMethod)
                .FirstOrDefault();

            var uiContext = currentInstanceGetter.Invoke(null, null) as IUIContext;
            return uiContext;
        }

        static List<StateToContextBindingInfo> GetContextPropertyAttributeFields()
        {
            if (s_ContextPropertiesAttributeTypes == null)
            {
                var types = AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(assembly => assembly.GetTypes());
                var infos = new List<StateToContextBindingInfo>();
                infos.AddRange(
                    types.SelectMany(type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(field => field.GetCustomAttributes(typeof(UIContextPropertiesAttribute), false).Length > 0))
                    .Select(field => new StateToContextBindingInfo() { stateDataType = field.FieldType, contextName = field.GetCustomAttribute<UIContextPropertiesAttribute>(false).contextName }).ToArray());

                infos.AddRange(
                    types.SelectMany(type => type.GetCustomAttributes<UIStoreContextPropertiesAttribute>(false))
                    .Select(attr => new StateToContextBindingInfo() { stateDataType = attr.stateDataType, contextName = attr.contextType.Name }).ToArray());

                s_ContextPropertiesAttributeTypes = infos;
            }
            return s_ContextPropertiesAttributeTypes;
        }

        public static List<string> GetContextPropertyNames(string contextName)
        {
            var resultList = new List<string>();

            if (!string.IsNullOrEmpty(contextName))
            {
                resultList = GetContextPropertyAttributeFields()
                    .Where(bi => bi.contextName.Equals(contextName))
                    .SelectMany(bi => bi.stateDataType.GetProperties().Select(prop => prop.Name)).ToList();
            }

            return resultList;
        }

        public static Type GetContextPropertyType(string contextName, string propertyName)
        {
            if (string.IsNullOrEmpty(contextName))
                return null;

            var stateTypes =  GetContextPropertyAttributeFields()
                .Where(bi => bi.contextName.Equals(contextName))
                .Select(bi => bi.stateDataType);

            var type = stateTypes
                .Select(type => type.GetProperty(propertyName))
                .Where(property => property != null)
                .Select(property => property.PropertyType)
                .FirstOrDefault();

            return type;
        }
    }
}
