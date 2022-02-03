using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core
{
    public static class UISelectorFactory
    {
#if UNITY_EDITOR && UI_SELECTOR_STATS
        private static Dictionary<IUISelector, Tuple<string, int>> s_CreatedSelectors = new Dictionary<IUISelector, Tuple<string, int>> ();
        public static IReadOnlyDictionary<IUISelector, Tuple<string, int>> CreatedSelectors => s_CreatedSelectors;
#endif

        public static IUISelector<T> createSelector<T>(IUIContext context, string name, Action<T> updateFunc = null
#if UNITY_EDITOR && UI_SELECTOR_STATS
            , [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
        {
            if (!context.ContainsProperty(name))
                throw new ArgumentException($"Property {name} not registered in context {context.GetType().FullName}");

            object target;
            if (context.target.TryGetTarget(out target) && context.implementsInterfaces != null)
            {
                foreach (var implementInterface in context.implementsInterfaces)
                {
                    if (implementInterface.GetProperty(name) != null && !implementInterface.IsGenericType && !implementInterface.IsAssignableFrom(target.GetType()))
                    {
                        Debug.LogWarning("The data type of " + target.GetType() + " does not implement the interface " + implementInterface.Name);
                    }
                }
            }

            Func<T> propertyGetter = () =>
            {
                object getterTarget;
                if (!context.target.TryGetTarget(out getterTarget))
                {
                    Debug.LogError("Cannot get the Target Reference");
                }

                if (PropertyContainer.TryGetValue<T>(ref getterTarget, name, out var value))
                    return value;
                else
                    return default;
            };

            var newSelector = new UISelector<T>(
                propertyGetter,
                context,
                updateFunc);

#if UNITY_EDITOR && UI_SELECTOR_STATS
            newSelector.OnDisposed += () =>
            {
                s_CreatedSelectors.Remove(newSelector);
            };
            s_CreatedSelectors.Add(newSelector, new Tuple<string, int>(sourceFilePath, sourceLineNumber));
#endif

            // this need to be all released on application unloading
            Application.quitting += () =>
            {
                if (newSelector.isDisposed == false)
                {
                    newSelector.Dispose();
                    newSelector = null;
                }
            };
            return newSelector;
        }
    }
}
