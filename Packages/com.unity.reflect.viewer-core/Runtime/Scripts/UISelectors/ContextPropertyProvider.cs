using System.Collections.Generic;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core
{
    public abstract class ContextPropertyProvider<TContextData, TStateData>: MonoBehaviour, IContextPropertyProvider where TContextData : IUIContext
    {
        public IEnumerable<IContextPropertyProvider.ContextPropertyData> GetProperties()
        {
            return ContextPropertyHelper.GetProperties<TContextData, TStateData>();
        }
    }

    public static class ContextPropertyHelper
    {
        public static IEnumerable<IContextPropertyProvider.ContextPropertyData> GetProperties<TContextData, TStateData>() where TContextData : IUIContext
        {
            var propertyBag = PropertyBag.GetPropertyBag(typeof(TStateData)) as PropertyBag<TStateData>;
            foreach (var property in propertyBag.GetProperties())
            {
                var data = new IContextPropertyProvider.ContextPropertyData() { context = typeof(TContextData), property = property };
                yield return data;
            }
        }
    }
}
