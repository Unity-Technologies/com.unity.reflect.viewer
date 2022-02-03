using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Properties.Internal;

namespace Unity.Properties.Debug
{
    partial class Explorer
    {
        [UsedImplicitly]
        public readonly List<PropertyBagDebugInfo> DisplayList;
        
        [UsedImplicitly]
        public PropertyBagDebugInfo Details;

        public Context InspectionContext { get; }
        
        public Explorer(Explorer.Provider provider)
        {
            InspectionContext = provider.Context;
            InspectionContext.OnPropertyBagSelected += bagDetail => { Details = bagDetail; }; 
            DisplayList = new List<PropertyBagDebugInfo>(PropertyBagDebugInfoStore.AllDebugInfos);

            if (PropertyBagDebugInfoStore.TryGetPropertyBagDetail(InspectionContext.SelectedType, out var detail))
                Details = detail;
        }
    }
}
