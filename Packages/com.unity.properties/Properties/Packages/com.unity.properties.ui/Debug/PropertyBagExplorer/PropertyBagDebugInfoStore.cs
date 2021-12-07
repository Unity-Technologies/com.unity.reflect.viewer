using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties.Debug
{
    static class PropertyBagDebugInfoStore
    {
        static readonly List<PropertyBagDebugInfo> s_AllDebugInfos;
        static readonly Dictionary<Type, PropertyBagDebugInfo> s_DebugInfoPerType;
        static readonly Dictionary<Type, int> s_Indices;

        public static IReadOnlyList<PropertyBagDebugInfo> AllDebugInfos => s_AllDebugInfos;
        
        public static bool TryGetPropertyBagDetail(Type type, out PropertyBagDebugInfo bag)
        {
            return s_DebugInfoPerType.TryGetValue(type, out bag);
        }

        public static int IndexOf(Type type)
        {
            if (null == type) 
                return -1;
            return s_Indices.TryGetValue(type, out var index) 
                ? index
                : -1;
        }
        
        static PropertyBagDebugInfoStore()
        {
            s_AllDebugInfos = new List<PropertyBagDebugInfo>();
            s_DebugInfoPerType = new Dictionary<Type, PropertyBagDebugInfo>();
            s_Indices = new Dictionary<Type, int>();

            foreach (var type in PropertyBagStore.AllTypes)
            {
                var info = new PropertyBagDebugInfo(type, PropertyBagStore.GetPropertyBag(type));
                s_AllDebugInfos.Add(info);
                s_DebugInfoPerType[type] = info;
            }

            s_AllDebugInfos.Sort((lhs, rhs) => string.Compare(lhs.Name, rhs.Name, StringComparison.Ordinal));
            for (var i = 0; i < s_AllDebugInfos.Count; ++i)
            {
                var info = AllDebugInfos[i];
                s_Indices[info.Type] = i;
            }
            PropertyBagStore.NewTypeRegistered += OnPropertyBagRegistered;
        }

        static void OnPropertyBagRegistered(Type type, IPropertyBag propertyBag)
        {
            // We don't unregister property bags once they are registered and we want to keep a sorted by type name list
            // for display. Whenever a new property bag in created, we will add it at the correct index.
            var index = 0;
            var info = new PropertyBagDebugInfo(type, propertyBag);
            for (var i = 0; i < s_AllDebugInfos.Count; ++i, ++index)
            {
                var bag = s_AllDebugInfos[i];
                if (string.Compare(bag.Name, info.Name, StringComparison.Ordinal) < 0)
                    continue;
                break;
            }

            s_AllDebugInfos.Insert(index, info);
            
            for (var i = index; i < s_AllDebugInfos.Count; ++i)
            {
                var bag = AllDebugInfos[i];
                s_Indices[bag.Type] = i;
            }
            s_DebugInfoPerType[type] = info;
        }
    }
}
