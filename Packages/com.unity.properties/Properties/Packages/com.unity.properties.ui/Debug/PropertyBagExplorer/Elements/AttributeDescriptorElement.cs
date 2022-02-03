using System.Collections.Generic;
using Unity.Properties.UI.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    sealed class AttributeDescriptorElement<TDescriptor, TValue, TDescriptorField, TValueField>
        : BindableElement
        , INotifyValueChanged<AttributeDescriptor<TDescriptor, TValue>>
    where TDescriptorField : BindableElement, INotifyValueChanged<TDescriptor>, new()
    where TValueField : BindableElement, INotifyValueChanged<TValue>, new()
    {
        const string k_Container = "unity-properties__type-descriptor__container";
        const string k_Descriptor = "unity-properties__type-descriptor__descriptor";
        const string k_Value = "unity-properties__type-descriptor__value";
        const string k_Link = "unity-properties__type-descriptor__link";
        
        TDescriptorField m_TypeField;
        TValueField m_ValueField;
        
        AttributeDescriptor<TDescriptor, TValue> m_Value;

        public AttributeDescriptorElement()
        {
            AddToClassList(UssClasses.Common.Row);
            AddToClassList(UssClasses.Common.Expand);
            AddToClassList(k_Container);
            Resources.Templates.Explorer.AttributeDescriptor.AddStyles(this);
            
            m_TypeField = new TDescriptorField();
            hierarchy.Add(m_TypeField);
            m_TypeField.AddToClassList(k_Descriptor);
            
            m_ValueField = new TValueField();
            hierarchy.Add(m_ValueField);
            m_ValueField.AddToClassList(k_Value);
            
            m_ValueField.RegisterCallback<ClickEvent, AttributeDescriptorElement<TDescriptor, TValue, TDescriptorField, TValueField>>((evt, typeDescriptor) =>
            {
                if (evt.clickCount != 1)
                    return;
            
                using (var pooled = TypeSelectedEvent<AttributeDescriptor<TDescriptor, TValue>>.GetPooled(typeDescriptor.value))
                {
                    pooled.target = typeDescriptor;
                    SendEvent(pooled);
                }
            }, this);
        }

        public AttributeDescriptorElement(AttributeDescriptor<TDescriptor, TValue> v)
            :this()
        {
            value = v;
        }
        
        public void SetValueWithoutNotify(AttributeDescriptor<TDescriptor, TValue> newValue)
        {
            m_Value = newValue;

            m_TypeField.value = newValue.Descriptor;
            m_ValueField.value = newValue.Value;
        }

        public void ShowValueAsLink()
        {
            m_ValueField.AddToClassList(k_Link);
        }
        
        public AttributeDescriptor<TDescriptor, TValue> value
        {
            get => m_Value;
            set
            {
                if (EqualityComparer<AttributeDescriptor<TDescriptor, TValue>>.Default.Equals(m_Value, value))
                    return;
                
                if (null != panel)
                {
                    using (var pooled = ChangeEvent<AttributeDescriptor<TDescriptor, TValue>>.GetPooled(m_Value, value))
                    {
                        pooled.target = this;
                        SetValueWithoutNotify(value);
                        SendEvent(pooled);
                    }
                }
                else
                    SetValueWithoutNotify(value);
            }
        }
    }
}
