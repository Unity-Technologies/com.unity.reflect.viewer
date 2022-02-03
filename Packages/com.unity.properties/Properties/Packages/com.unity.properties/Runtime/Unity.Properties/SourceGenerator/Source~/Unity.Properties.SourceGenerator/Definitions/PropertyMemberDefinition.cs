using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Unity.Properties.SourceGenerator
{
    /// <summary>
    /// The <see cref="PropertyMemberDefinition"/> class is used a declaration for the code generator. It is used to gather and pass around all the necessary information required to output a generated property class.
    /// </summary>
    class PropertyMemberDefinition
    {
        readonly ITypeSymbol m_TypeSymbol;
        

        /// <summary>
        /// Gets the underlying member symbol.
        /// </summary>
        public ISymbol MemberSymbol { get; }

        /// <summary>
        /// Gets the underlying member name. This corresponds to the field or C# property name.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Gets the member type symbol.
        /// </summary>
        public ITypeSymbol MemberType { get; }
        
        /// <summary>
        /// Gets the class definition name for the generated property.
        /// </summary>
        public string PropertyClassName { get; }
        
        /// <summary>
        /// Gets the name of the property. This corresponds to the field or C# property name or a custom user defined name.
        /// </summary>
        public string PropertyName  { get; }
        
        /// <summary>
        /// Gets the type name for the property. This corresponds to the sanitized field type or C# property type.
        /// </summary>
        public string PropertyTypeName  { get; }
        
        /// <summary>
        /// Gets the value indicating if the field is readonly. i.e. It has the <see langword="readonly"/> keyword or has no property setter.
        /// </summary>
        public bool IsReadOnly { get; }
        
        /// <summary>
        /// Gets the value indicating if the declared accessibility for the property.
        /// </summary>
        public Accessibility DeclaredAccessibility { get; }

        /// <summary>
        /// Gets the value indicating if the backing member is a field.
        /// </summary>
        public bool IsField { get; }

        /// <summary>
        /// Gets the custom attributes for this property.
        /// </summary>
        public ImmutableArray<AttributeData> GetAttributes => MemberSymbol.GetAttributes();

        /// <summary>
        /// Returns true if the given property has any custom non-properties attributes.
        /// </summary>
        public bool HasCustomAttributes => GetAttributes.Any(a => a.AttributeClass != null && a.AttributeClass.Name != "GenerateProperty");

        /// <summary>
        /// Gets the value indicating if the declared accessibility for the member type.
        /// </summary>
        public Accessibility MemberTypeDeclaredAccessibility { get; }

        /// <summary>
        /// Returns true if this is a valid property definition.
        /// </summary>
        public bool IsValidProperty => MemberTypeDeclaredAccessibility != Accessibility.Private;

        public PropertyMemberDefinition(ISymbol member)
        {
            MemberSymbol = member;
            
            switch (member)
            {
                case IFieldSymbol field:
                    m_TypeSymbol = field.Type;
                    IsReadOnly = field.IsReadOnly;
                    DeclaredAccessibility = field.DeclaredAccessibility;
                    MemberTypeDeclaredAccessibility = field.Type.DeclaredAccessibility;
                    IsField = true;
                    break;
                case IPropertySymbol property:
                    m_TypeSymbol = property.Type;
                    IsReadOnly = property.SetMethod == null;
                    DeclaredAccessibility = property.DeclaredAccessibility;
                    MemberTypeDeclaredAccessibility = property.Type.DeclaredAccessibility;
                    IsField = false;
                    break;
            }

            MemberName = member.Name;
            MemberType = m_TypeSymbol;
            PropertyName = member.Name;
            PropertyClassName = member.Name + "_Property";
            PropertyTypeName = m_TypeSymbol.ToString().Replace("*", "");
        }
    }
}