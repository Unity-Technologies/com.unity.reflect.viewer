using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    using SplitterBase =
#if UNITY_2020_2_OR_NEWER
        UnityEngine.UIElements.TwoPaneSplitView;
#else
        Unity.Properties.UI.Internal.TwoPaneSplitView;
#endif
    
    class Splitter : SplitterBase
    {
        public new class UxmlFactory : UxmlFactory<Splitter, SplitterBase.UxmlTraits> {}
    }
}
