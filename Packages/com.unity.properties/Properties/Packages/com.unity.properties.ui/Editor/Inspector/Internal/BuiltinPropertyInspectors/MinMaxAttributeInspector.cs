using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    abstract class MinMaxAttributeInspectorBaseField<TValue> : BaseFieldAttributeInspector<MinMaxSlider, Vector2, TValue, MinMaxAttribute>
    {
        public override VisualElement Build()
        {
            var root = base.Build();
            m_Field.lowLimit = DrawerAttribute.Min;
            m_Field.highLimit = DrawerAttribute.Max;
            return root;
        }
    }
    
    [UsedImplicitly]
    class MinMaxFieldAttributeInspector : MinMaxAttributeInspectorBaseField<Vector2>
    {
    }
    
    [UsedImplicitly]
    class MinMaxFieldIntAttributeInspector : MinMaxAttributeInspectorBaseField<Vector2Int>
    {
    }
}