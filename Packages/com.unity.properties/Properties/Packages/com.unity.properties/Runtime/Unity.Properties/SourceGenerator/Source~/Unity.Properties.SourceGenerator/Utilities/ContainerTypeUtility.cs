using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Unity.Properties.SourceGenerator
{
    /// <summary>
    /// Helper class to gather all container types recursively for the given compilation.
    /// </summary>
    static class ContainerTypeUtility
    {
        [Flags]
        enum TypeOptions
        {
            /// <summary>
            /// If this option is selected, any inherited value types will have property bags generated.
            /// </summary>
            ValueType = 1 << 1,
        
            /// <summary>
            /// If this option is selected, any inherited reference types will have property bags generated.
            /// </summary>
            ReferenceType = 1 << 2,
        
            /// <summary>
            /// The default set of type options. This includes both <see cref="ValueType"/> and <see cref="ReferenceType"/>.
            /// </summary>
            Default = ValueType | ReferenceType
        }
        
        /// <summary>
        /// Enumerates all property container types for the given compilation.
        ///
        /// This includes the following:
        ///     * Types attributed with <see cref="Unity.Properties.GeneratePropertyBagAttribute"/>.
        ///     * Types specified by the assembly level attribute <see cref="Unity.Properties.GeneratePropertyBagsForTypesQualifiedWithAttribute"/>.
        ///     * Types specified by the assembly level attribute <see cref="Unity.Properties.GeneratePropertyBagsForTypeAttribute"/>.
        ///     * and ALL types contained within recursively.
        /// </summary>
        /// <param name="assembly">The assembly to gather types from.</param>
        /// <returns>An enumeration of ALL type symbols which should have property bags generated.</returns>
        public static IEnumerable<ITypeSymbol> GetPropertyContainerTypes(IAssemblySymbol assembly)
        {
            var visited = new HashSet<ITypeSymbol>();
            
            foreach (var type in GetTopLevelPropertyContainerTypes(assembly).SelectMany(x => GetPropertyContainerTypesRecursive(x, visited)))
                yield return type;
        }
        
        /// <summary>
        /// Enumerates all top level property container types.
        /// 
        /// This includes the following:
        ///     * Types attributed with <see cref="Unity.Properties.GeneratePropertyBagAttribute"/>.
        ///     * Types specified by the assembly level attribute <see cref="Unity.Properties.GeneratePropertyBagsForTypesQualifiedWithAttribute"/>.
        ///     * Types specified by the assembly level attribute <see cref="Unity.Properties.GeneratePropertyBagsForTypeAttribute"/>.
        /// </summary>
        /// <param name="assembly">The assembly to gather types from.</param>
        /// <returns>An enumeration of all top level types which should have property bags generated.</returns>
        static IEnumerable<ITypeSymbol> GetTopLevelPropertyContainerTypes(IAssemblySymbol assembly)
        {
            // Fetch assembly level attributes which drive property bag generation via interfaces.
            var generatePropertyBagsForTypesQualifiedWithAttributes = assembly.Modules.First().ReferencedAssemblySymbols
                .Append(assembly)
                .SelectMany(x => x.GetAttributes())
                .Where(x => x.AttributeClass.Name == "GeneratePropertyBagsForTypesQualifiedWithAttribute").ToImmutableHashSet();
            
            // Fetch any assembly declared property bag types. These are typically used for open generics or abstract types which can not be inferred at compile time.
            var generatePropertyBagsForTypes = assembly.GetAttributes()
                .Where(x => x.AttributeClass.Name == "GeneratePropertyBagsForTypeAttribute")
                .Select(x => x.ConstructorArguments[0].Value as ITypeSymbol)
                .Where(x => x != null);

            var visited = new HashSet<ISymbol>();

            bool IsRootContainerType(ITypeSymbol symbol)
            {
                if (symbol is INamedTypeSymbol named && named.IsGenericType)
                    return false;
                
                if (!IsContainerType(symbol))
                    return false;

                if (!symbol.HasAttribute("GeneratePropertyBagAttribute") && !MatchesAnyQualifiedWithAttribute(symbol, generatePropertyBagsForTypesQualifiedWithAttributes))
                    return false;

                return true;
            }

            foreach (var symbol in generatePropertyBagsForTypes)
            {
                if (!visited.Add(symbol))
                {
                    // This type was declared multiple times explicitly. We can safely ignore it.
                    continue;
                }

                if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsUnboundGenericType)
                {
                    // This is an explicitly declared open generic. We can safely ignore it.
                    continue;
                }

                // This type was explicitly declared and can should be included.
                yield return symbol;
            }

            foreach (var symbol in assembly.GlobalNamespace.GetTypeMembersRecurse(visited).Where(IsRootContainerType))
                yield return symbol;
        }

        static IEnumerable<ITypeSymbol> GetPropertyContainerTypesRecursive(ITypeSymbol symbol, ISet<ITypeSymbol> visited)
        {
            if (!IsContainerType(symbol) || !visited.Add(symbol)) yield break;

            switch (symbol)
            {
                case IArrayTypeSymbol array:
                {
                    foreach (var type in GetPropertyContainerTypesRecursive(array.ElementType, visited))
                    {
                        yield return type;
                    }
                
                    yield break;
                }
                case INamedTypeSymbol namedTypeSymbol when namedTypeSymbol.IsGenericType:
                {
                    if (symbol.Interfaces.Any(i => i.MetadataName == "IList`1"))
                    {
                        foreach (var type in GetPropertyContainerTypesRecursive(namedTypeSymbol.TypeArguments[0], visited))
                        {
                            yield return type;
                        }
                    
                        yield break;
                    }
                
                    if (symbol.Interfaces.Any(i => i.MetadataName == "ISet`1"))
                    {
                        foreach (var type in GetPropertyContainerTypesRecursive(namedTypeSymbol.TypeArguments[0], visited))
                        {
                            yield return type;
                        }
                    
                        yield break;
                    }
                
                    if (symbol.Interfaces.Any(i => i.MetadataName == "IDictionary`2"))
                    {
                        foreach (var type in GetPropertyContainerTypesRecursive(namedTypeSymbol.TypeArguments[0], visited))
                        {
                            yield return type;
                        }

                        foreach (var type in GetPropertyContainerTypesRecursive(namedTypeSymbol.TypeArguments[1], visited))
                        {
                            yield return type;
                        }
                    
                        yield break;
                    }

                    break;
                }
            }

            yield return symbol;
            
            foreach (var member in symbol.GetMembers())
            {
                switch (member)
                {
                    case IFieldSymbol field:
                    {
                        if (field.IsStatic || field.IsConst)
                            continue;
                        
                        var fieldType = field.Type;

                        if (!IsContainerType(fieldType)) continue;
                        if (field.HasAttribute("DontCreatePropertyAttribute"))  continue;
                        if (!(field.DeclaredAccessibility == Accessibility.Public || field.HasAttribute("CreatePropertyAttribute"))) continue;

                        foreach (var type in GetPropertyContainerTypesRecursive(fieldType, visited))
                            yield return type;
                        break;
                    }
                    case IPropertySymbol property:
                    {
                        if (property.IsStatic)
                            continue;
                        
                        var propertyType = property.Type;

                        if (!IsContainerType(propertyType)) continue;
                        if (property.HasAttribute("DontCreatePropertyAttribute")) continue;
                        if (!property.HasAttribute("CreatePropertyAttribute")) continue;

                        foreach (var type in GetPropertyContainerTypesRecursive(propertyType, visited))
                            yield return type;
                        break;
                    }
                }
            }
        }

        static bool MatchesAnyQualifiedWithAttribute(ITypeSymbol symbol, ImmutableHashSet<AttributeData> attributes)
        {
            foreach (var i in attributes)
            {
                var type = i.ConstructorArguments[0].Value as ITypeSymbol;
                
                // ReSharper disable once MergeConditionalExpression
                var options = null != i.ConstructorArguments[1].Value ? (TypeOptions) i.ConstructorArguments[1].Value : (TypeOptions) 0;

                if (MatchesTypeOptions(symbol, options) && symbol.AllInterfaces.Contains(type))
                    return true;
            }

            return false;
        }

        static bool MatchesTypeOptions(ITypeSymbol type, TypeOptions options)
        {
            if (type.IsValueType) return (options & TypeOptions.ValueType) != 0;
            return (options & TypeOptions.ReferenceType) != 0;
        }

        static bool IsUnsupportedUnityEngineType(ITypeSymbol type)
        {
            var current = type;
                
            while (current != null)
            {
                var name = current.ToString();
                
                switch (name)
                {
                    case "UnityEngine.MonoBehaviour":
                    case "UnityEngine.ScriptableObject":
                        return false;
                    case "UnityEngine.Object":
                        return true;
                    default:
                        current = current.BaseType;
                        break;
                }
            }

            return false;
        }
        
        static bool IsContainerType(ITypeSymbol symbol)
        {
            // Any special built in type does not have property bags generated.
            if (symbol.SpecialType != SpecialType.None) return false;
                
            // No support for pointer types.
            if (symbol.TypeKind == TypeKind.Pointer) return false;
            
            // Some enums are not picked up by SpecialType.
            if (symbol.BaseType?.Name == "Enum") return false;

            // Property bags are never generated for abstract types.
            if (symbol.IsAbstract) return false;
                
            // Nullable types do not have property bags.
            if (symbol.MetadataName == "Nullable`1") return false;

            // Unity Engine types do not have property bags.
            if (IsUnsupportedUnityEngineType(symbol)) return false;

            // Multi dimensional arrays are not supported yet.
            if (symbol is IArrayTypeSymbol arrayTypeSymbol && arrayTypeSymbol.Rank > 1) return false;

            // Anonymous types are not supported yet.
            if (symbol.IsAnonymousType) return false;

            return true;
        }
    }
}