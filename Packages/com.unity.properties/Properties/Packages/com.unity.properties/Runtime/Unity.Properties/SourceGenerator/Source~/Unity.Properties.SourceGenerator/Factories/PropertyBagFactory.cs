using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unity.Properties.SourceGenerator
{ 
    /// <summary>
    /// The <see cref="PropertyBagFactory"/> is used to construct a proper class declaration for a given property bag. It contains all the necessary logic for writing code syntax blocks. 
    /// </summary>
    static class PropertyBagFactory
    {
        public static ClassDeclarationSyntax CreatePropertyBagClassDeclarationSyntax(PropertyBagDefinition propertyBag)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"[System.Runtime.CompilerServices.CompilerGenerated]");
            builder.AppendLine($"class {propertyBag.PropertyBagClassName} : Unity.Properties.ContainerPropertyBag<{propertyBag.ContainerTypeName}>");
            builder.AppendLine($"{{");

            builder.AppendLine($"public {propertyBag.PropertyBagClassName}()");
            builder.AppendLine($"{{");
            foreach (var property in propertyBag.GetValidPublicPropertyMembers())
            {
                builder.Append($"AddProperty(new ").Append(property.PropertyClassName).AppendLine("());");
            }

            if (propertyBag.GetValidNonPublicPropertyMembers().Any())
            {
                builder.AppendLine($"#if !{Defines.NET_DOTS}");
                foreach (var property in propertyBag.GetValidNonPublicPropertyMembers())
                {
                    builder.Append($"AddProperty(new ").Append(property.PropertyClassName).AppendLine("());");
                }
                builder.AppendLine($"#endif");
            }

            foreach (var registerCollectionTypeMethod in GetRegisterCollectionTypeMethod(propertyBag))
                builder.AppendLine(registerCollectionTypeMethod);

            builder.Append($"}}");
            
            foreach (var property in propertyBag.GetValidPublicPropertyMembers())
            {
                builder.AppendLine($"[System.Runtime.CompilerServices.CompilerGenerated]");
                builder.AppendLine($"class {property.PropertyClassName} : Unity.Properties.Property<{propertyBag.ContainerTypeName}, {property.PropertyTypeName}>");
                builder.AppendLine($"{{");
                builder.AppendLine($"public override string Name => \"{property.PropertyName}\";");
                builder.AppendLine($"public override bool IsReadOnly => {property.IsReadOnly.ToString().ToLower()};");

                if (property.HasCustomAttributes)
                {
                    builder.AppendLine($"public {property.PropertyClassName}()");
                    builder.AppendLine($"{{");
                    builder.AppendLine($"#if !{Defines.NET_DOTS}");
                    var getMemberMethod = property.IsField ? "GetField" : "GetProperty";
                    builder.AppendLine($"AddAttributes(typeof({propertyBag.ContainerTypeName}).{getMemberMethod}(\"{property.MemberName}\", BindingFlags.Instance | BindingFlags.Public).GetCustomAttributes());");
                    builder.AppendLine($"#endif");
                    builder.AppendLine($"}}");
                }
                
                builder.AppendLine($"public override {property.PropertyTypeName} GetValue(ref {propertyBag.ContainerTypeName} container) => container.{property.PropertyName};");

                builder.AppendLine(property.IsReadOnly
                    ? $"public override void SetValue(ref {propertyBag.ContainerTypeName} container, {property.PropertyTypeName} value) => throw new System.InvalidOperationException(\"Property is ReadOnly\");"
                    : $"public override void SetValue(ref {propertyBag.ContainerTypeName} container, {property.PropertyTypeName} value) => container.{property.PropertyName} = value;");

                builder.AppendLine($"}}");
            }

            if (propertyBag.GetValidNonPublicPropertyMembers().Any())
            {
                builder.AppendLine($"#if !{Defines.NET_DOTS}");
                foreach (var property in propertyBag.GetValidNonPublicPropertyMembers())
                {
                    var getMemberMethod = property.IsField ? "GetField" : "GetProperty";
                    builder.AppendLine($"[System.Runtime.CompilerServices.CompilerGenerated]");
                    builder.AppendLine($"class {property.PropertyClassName} : Unity.Properties.ReflectedMemberProperty<{propertyBag.ContainerTypeName}, {property.PropertyTypeName}>");
                    builder.AppendLine($"{{");
                    builder.AppendLine($"    public {property.PropertyClassName}()");
                    builder.AppendLine($"        : base(typeof({property.MemberSymbol.ContainingType}).{getMemberMethod}(\"{property.MemberName}\", BindingFlags.Instance | BindingFlags.NonPublic), \"{property.PropertyName}\")");
                    builder.AppendLine($"    {{");
                    builder.AppendLine($"    }}");
                    builder.AppendLine($"}}");
                }
                builder.AppendLine($"#endif");
            }
            
            builder.AppendLine($"}}");
            
            var propertyBagClassDeclarationSyntax = SyntaxFactory.ParseMemberDeclaration(builder.ToString()) as ClassDeclarationSyntax;

            if (null == propertyBagClassDeclarationSyntax)
                throw new Exception($"Failed to construct ClassDeclarationSyntax for ContainerType=[{propertyBag.ContainerTypeName}]");
            
            return propertyBagClassDeclarationSyntax;
        }

        static IEnumerable<string> GetRegisterCollectionTypeMethod(PropertyBagDefinition propertyBag)
        {
            var visited = new HashSet<ITypeSymbol>();
            
            foreach (var property in propertyBag.GetPropertyMembers())
                foreach (var line in GetRegisterCollectionTypesRecurse(propertyBag.ContainerType, property.MemberType, visited))
                    yield return line;
        }

        static IEnumerable<string> GetRegisterCollectionTypesRecurse(ITypeSymbol containerType, ITypeSymbol propertyType, HashSet<ITypeSymbol> visited)
        {
            if (!visited.Add(propertyType)) yield break;
            
            switch (propertyType)
            {
                case IArrayTypeSymbol arrayTypeSymbol:
                {
                    yield return $"Unity.Properties.PropertyBag.RegisterArray<{containerType.ToCSharpName()}, {arrayTypeSymbol.ToCSharpName()}>();";
                    
                    foreach (var inner in GetRegisterCollectionTypesRecurse(arrayTypeSymbol, arrayTypeSymbol.ElementType, visited))
                        yield return inner;
                    
                    break;
                }
                case INamedTypeSymbol namedTypeSymbol:
                {
                    if (propertyType.Interfaces.Any(i => i.MetadataName == "IList`1"))
                    {
                        var elementTypeSymbol = propertyType.Interfaces.First(i => i.MetadataName == "IList`1").TypeArguments[0];
                        
                        if (propertyType.MetadataName == "List`1")
                            yield return $"Unity.Properties.PropertyBag.RegisterList<{containerType.ToCSharpName()}, {elementTypeSymbol.ToCSharpName()}>();";
                        else 
                            yield return  $"Unity.Properties.PropertyBag.RegisterIList<{containerType.ToCSharpName()}, {namedTypeSymbol.ToCSharpName()}, {elementTypeSymbol.ToCSharpName()}>();";

                        foreach (var inner in GetRegisterCollectionTypesRecurse(namedTypeSymbol, elementTypeSymbol, visited))
                            yield return inner;
                    }
                    else if (propertyType.Interfaces.Any(i => i.MetadataName == "ISet`1"))
                    {
                        var elementTypeSymbol = propertyType.Interfaces.First(i => i.MetadataName == "ISet`1").TypeArguments[0];

                        if (propertyType.MetadataName == "HashSet`1")
                            yield return $"Unity.Properties.PropertyBag.RegisterHashSet<{containerType.ToCSharpName()}, {elementTypeSymbol.ToCSharpName()}>();";
                        else 
                            yield return  $"Unity.Properties.PropertyBag.RegisterISet<{containerType.ToCSharpName()}, {namedTypeSymbol.Name}, {elementTypeSymbol.ToCSharpName()}>();";
                        
                        foreach (var inner in GetRegisterCollectionTypesRecurse(namedTypeSymbol, elementTypeSymbol, visited))
                            yield return inner;
                    }
                    else if (propertyType.Interfaces.Any(i => i.MetadataName == "IDictionary`2"))
                    {
                        var interfaceSymbol = propertyType.Interfaces.First(i => i.MetadataName == "IDictionary`2");
                        
                        var keyTypeSymbol = interfaceSymbol.TypeArguments[0];
                        var valueTypeSymbol = interfaceSymbol.TypeArguments[1];
                        
                        if (propertyType.MetadataName == "Dictionary`2")
                            yield return $"Unity.Properties.PropertyBag.RegisterDictionary<{containerType.ToCSharpName()}, {keyTypeSymbol.ToCSharpName()}, {valueTypeSymbol.ToCSharpName()}>();";
                        else 
                            yield return  $"Unity.Properties.PropertyBag.RegisterIDictionary<{containerType.ToCSharpName()}, {namedTypeSymbol.ToCSharpName()}, {keyTypeSymbol.ToCSharpName()}, {valueTypeSymbol.ToCSharpName()}>();";

                        foreach (var inner in GetRegisterCollectionTypesRecurse(namedTypeSymbol, keyTypeSymbol, visited))
                            yield return inner;
                        
                        foreach (var inner in GetRegisterCollectionTypesRecurse(namedTypeSymbol, valueTypeSymbol, visited))
                            yield return inner;
                    }
                    
                    break;
                }
            }
        }
    }
}