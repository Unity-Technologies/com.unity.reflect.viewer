using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Properties.Editor;
using Unity.Properties.UI.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    class TypeNameLabel : BindableElement, INotifyValueChanged<Type>
    {
        /// <summary>
        ///   <para>Instantiates a <see cref="TypeNameLabel"/> using the data read from a UXML file.</para>
        /// </summary>
        [UsedImplicitly]
        class TypeNameElementFactory : UxmlFactory<TypeNameLabel, TypeNameElementTraits>
        {
        }

        /// <summary>
        ///   <para>Defines UxmlTraits for the <see cref="TypeNameLabel"/>.</para>
        /// </summary>
        class TypeNameElementTraits : UxmlTraits
        {
        }
        
        Label m_ParentTypeName;
        Label m_TypeName;
        Type m_Value;

        public TypeNameLabel()
        {
            Resources.Templates.Explorer.TypeName.AddStyles(this);
            AddToClassList("unity-properties__type-name__container");
            AddToClassList("unity-properties__row");
            m_ParentTypeName = new Label();
            m_ParentTypeName.AddToClassList("unity-properties__type-name__parent-type-name");
            hierarchy.Add(m_ParentTypeName);
            m_TypeName = new Label();
            m_TypeName.AddToClassList("unity-properties__type-name__nested-type-name");
            hierarchy.Add(m_TypeName);
        }

        public TypeNameLabel(Type type)
            :this()
        {
            value = type;
        }

        public Type value
        {
            get => m_Value;
            set
            {
                if (EqualityComparer<Type>.Default.Equals(m_Value, value))
                    return;
                if (panel != null)
                {
                    using (var pooled = ChangeEvent<Type>.GetPooled(m_Value, value))
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
        
        public void SetValueWithoutNotify(Type type)
        {
            m_Value = type;
            if (null == type)
            {
                m_ParentTypeName.Hide();
                m_TypeName.Hide();
                return;
            }
            
            m_TypeName.Show();
            var typeName = TypeUtility.GetTypeDisplayName(type);
            var lastDot = IndexOfNestedType(typeName);
                    
            if (lastDot < 0)
            {
                m_ParentTypeName.Hide();
                m_TypeName.text = typeName;
            }
            else
            {
                m_ParentTypeName.Show();
                m_ParentTypeName.text = typeName.Substring(0, lastDot);
                m_TypeName.text = typeName.Substring(lastDot);
            }
        }

        static int IndexOfNestedType(string typeName)
        {
            var generic = 0;
            for (var i = typeName.Length - 1; i >= 0; --i)
            {
                switch (typeName[i])
                {
                    case '>':
                        ++generic;
                        break;
                    case '<':
                        --generic;
                        break;
                    case '.' when generic == 0:
                        return i + 1;
                }
            }

            return -1;
        }
    }
}
