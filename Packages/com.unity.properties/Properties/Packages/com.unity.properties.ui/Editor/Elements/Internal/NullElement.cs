using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.Properties.Editor;
using UnityEditor.UIElements;

namespace Unity.Properties.UI.Internal
{
    class NullElement<T> : BindableElement, IBinding
    {
        abstract class Null{}

        readonly BindingContextElement m_Root;
        readonly PropertyPath m_Path;
        readonly List<Type> m_PotentialTypes;

        public NullElement(BindingContextElement root, IProperty property, PropertyPath path)
        {
            m_PotentialTypes = new List<Type> {typeof(Null)};
            binding = this;
            m_Root = root;
            m_Path = path;
            name = m_Path.ToString();
            
            TypeUtility.GetAllConstructableTypes<T>(m_PotentialTypes);

            if (typeof(T).IsArray)
            {
                Resources.Templates.NullStringField.Clone(this);
                this.Q<Label>().text = GuiFactory.GetDisplayName(property);
                var button = this.Q<Button>();
                button.text = $"Null ({GetTypeName(typeof(T))})";
                button.clickable.clicked += ReloadWithArrayType;
                if (property.IsReadOnly)
                {
                    button.SetEnabledSmart(false);
                }
                return;
            }
            
            if (m_PotentialTypes.Count == 2)
            {
                Resources.Templates.NullStringField.Clone(this);
                this.Q<Label>().text = GuiFactory.GetDisplayName(property);
                var button = this.Q<Button>();
                button.text = $"Null ({GetTypeName(typeof(T))})";
                button.clickable.clicked += ReloadWithFirstType;
                if (property.IsReadOnly)
                {
                    button.SetEnabledSmart(false);
                }
                return;
            }
            
            var typeSelector = new PopupField<Type>(
                GuiFactory.GetDisplayName(property),
                m_PotentialTypes,
                typeof(Null),
                GetTypeName,
                GetTypeName);
            typeSelector.RegisterValueChangedCallback(OnCreateItem);
            if (property.IsReadOnly)
            {
                typeSelector.pickingMode = PickingMode.Ignore;
                typeSelector.Q(className: UssClasses.Unity.BasePopupFieldInput).SetEnabledSmart(false);
            }

            Add(typeSelector);
        }

        string GetTypeName(Type type)
        {
            if (type == typeof(Null))
            {
                return $"Null ({TypeUtility.GetTypeDisplayName(typeof(T))})";
            }
            return TypeUtility.GetTypeDisplayName(type);
        }

        void IBinding.PreUpdate()
        {
        }
        
        void IBinding.Update()
        {
            try
            {
                if (!m_Root.TryGetValue<T>(m_Path, out var value))
                {
                    return;
                }

                if (EqualityComparer<T>.Default.Equals(value, default))
                    return;

                ReloadWithInstance(value);
            }
            catch (Exception )
            {
                
            }
        }

        void IBinding.Release()
        {
        }

        void OnCreateItem(ChangeEvent<Type> evt)
        {
            var type = evt.newValue;
            if (type == typeof(Null))
            {
                return;
            }
            
            var instance = type == typeof(T)
                ? TypeConstruction.Construct<T>()
                : TypeConstruction.Construct<T>(type);

            ReloadWithInstance(instance);
        }
        
        void ReloadWithFirstType()
        {
            ReloadWithInstance(TypeConstruction.Construct<T>(m_PotentialTypes[1]));
        }
        
        void ReloadWithArrayType()
        {
            ReloadWithInstance(TypeConstruction.ConstructArray<T>());
        }

        void ReloadWithInstance(T value)
        {
            m_Root.SwapWithInstance(m_Path, this, value);
        }
    }
}
