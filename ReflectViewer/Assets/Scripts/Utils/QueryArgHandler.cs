using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public class QueryArgHandler
    {
        public static List<QueryArgMethodInfo> m_QueryArgSetterList = new List<QueryArgMethodInfo>();
        public static List<QueryArgMethodInfo> m_QueryArgGetterList = new List<QueryArgMethodInfo>();

        public static void RegisterMethod(QueryArgMethodInfo queryArgMethodInfo)
        {
            m_QueryArgSetterList.Add(queryArgMethodInfo);
        }

        public static void Unregister(Component component)
        {
            m_QueryArgSetterList.RemoveAll(x => x.component == component);
            m_QueryArgGetterList.RemoveAll(x => x.component == component);
        }

       static void Register(Component component, string key, MethodInfo setter, MethodInfo getter = null, string value = "")
        {
            var queryArgSetter = new QueryArgMethodInfo()
            {
                component = component,
                key = key,
                value = value,
                method = setter,
            };
            m_QueryArgSetterList.Add(queryArgSetter);

            if (getter != null)
            {
                var queryArgGetter = new QueryArgMethodInfo()
                {
                    component = component,
                    key = key,
                    value = value,
                    method = getter,
                };
                m_QueryArgGetterList.Add(queryArgGetter);
            }
        }

        public static void Register(Component component, string key, Action setter, Func<string> getter = null, string value = "")
        {
            Register(component, key, setter.Method, getter!= null? getter.Method:null, value);
        }

        public static void Register(Component component, string key, Action<int> setter, Func<string> getter = null, string value = "")
        {
            Register(component, key, setter.Method, getter != null ? getter.Method : null, value);
        }

        public static void Register(Component component, string key, Action<double> setter, Func<string> getter = null, string value = "")
        {
            Register(component, key, setter.Method, getter != null ? getter.Method : null, value);
        }

        public static void Register(Component component, string key, Action<float> setter, Func<string> getter = null, string value = "")
        {
            Register(component, key, setter.Method, getter != null ? getter.Method : null, value);
        }

        public static void Register(Component component, string key, Action<bool> setter, Func<string> getter = null, string value = "")
        {
            Register(component, key, setter.Method, getter != null ? getter.Method : null, value);
        }

        public static void Register(Component component, string key, Action<string> setter, Func<string> getter = null, string value = "")
        {
            Register(component, key, setter.Method, getter != null ? getter.Method : null, value);
        }

        // TODO ux to filter out part of query params
        public static string GetQueryString()
        {
            var returnStr = "";
            var queryArgToStrings = m_QueryArgGetterList.Where(x => x.component.gameObject.activeSelf).ToList();
            foreach (var queryArgToString in queryArgToStrings)
            {
                var key = queryArgToString.key;
                var nextQueryArg = (string)queryArgToString.method.Invoke(queryArgToString.component, null);
               
                if (!string.IsNullOrEmpty(nextQueryArg))
                {
                    if (returnStr.Length > 0)
                    {
                        returnStr += "&";
                    }
                    returnStr += $"{key}={nextQueryArg}";
                }
            }
            return returnStr;
        }

        public static void InvokeQueryArgMethods(Dictionary<string, string> queryArgs)
        {
            if (queryArgs.Count == 0)
            {
                return;
            }

            Debug.Log($"Consuming {queryArgs.Count} QueryArg to modify initial state of project.");
            foreach (var kv in queryArgs)
            {
                var queryArgMethods = m_QueryArgSetterList.Where(x => x.component.gameObject.activeSelf && x.key.Equals(kv.Key)).ToList();
                queryArgMethods.ForEach(x => InvokeMethod(x, kv));
            }
        }

        static void InvokeMethod(QueryArgMethodInfo queryArgMethod, KeyValuePair<string, string> kv)
        {
            var parameter = queryArgMethod.method.GetParameters().FirstOrDefault();
            if (parameter != null)
            {
                if (parameter.ParameterType.Equals(typeof(int)))
                {
                    if (int.TryParse(kv.Value, out int intValue))
                    {
                        queryArgMethod.method.Invoke(queryArgMethod.component, new object[] { intValue });
                    }
                    return;
                }
                if (parameter.ParameterType.Equals(typeof(bool)))
                {
                    if (bool.TryParse(kv.Value, out bool boolValue))
                    {
                        queryArgMethod.method.Invoke(queryArgMethod.component, new object[] { boolValue });
                    }
                    return;
                }
                if (parameter.ParameterType.Equals(typeof(double)))
                {
                    if (double.TryParse(kv.Value, out double doubleValue))
                    {
                        queryArgMethod.method.Invoke(queryArgMethod.component, new object[] { doubleValue });
                    }
                    return;
                }
                if (parameter.ParameterType.Equals(typeof(float)))
                {
                    if (float.TryParse(kv.Value, out float floatValue))
                    {
                        queryArgMethod.method.Invoke(queryArgMethod.component, new object[] { floatValue });
                    }
                    return;
                }
                // Default to string type
                queryArgMethod.method.Invoke(queryArgMethod.component, new object[] { kv.Value });
            }
            else
            {
                // If same values, even empty, Invoke parameterless method
                if (queryArgMethod.value.Equals(kv.Value))
                {
                    queryArgMethod.method.Invoke(queryArgMethod.component, null);
                }
            }
        }
    }

    public struct QueryArgMethodInfo
    {
        public string key;
        public string value;
        public MethodInfo method;
        public Component component;
    }
}
