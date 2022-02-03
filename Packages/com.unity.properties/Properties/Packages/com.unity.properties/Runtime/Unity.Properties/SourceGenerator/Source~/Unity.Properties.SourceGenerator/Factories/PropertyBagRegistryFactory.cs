using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unity.Properties.SourceGenerator
{
    /// <summary>
    /// The <see cref="PropertyBagRegistryFactory"/> is used to construct a proper class declaration for a given property bag. It contains all the necessary logic for writing code syntax blocks. 
    /// </summary>
    class PropertyBagRegistryFactory
    {
        public static ClassDeclarationSyntax CreatePropertyBagRegistryClassDeclarationSyntax(List<PropertyBagDefinition> propertyBags)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"[System.Runtime.CompilerServices.CompilerGenerated]");
            builder.AppendLine($"static class PropertyBagRegistry");
            builder.AppendLine($"{{");
            builder.AppendLine($"#if {Defines.UNITY_EDITOR}");
            builder.AppendLine($"    [UnityEditor.InitializeOnLoadMethod]");
            builder.AppendLine($"#endif");
            builder.AppendLine($"#if !{Defines.UNITY_DOTSPLAYER}");
            builder.AppendLine($"    [UnityEngine.Scripting.PreserveAttribute]");
            builder.AppendLine($"    [UnityEngine.RuntimeInitializeOnLoadMethodAttribute]");
            builder.AppendLine($"#endif");
            builder.AppendLine($"    public static void Initialize()");
            builder.AppendLine($"    {{");

            foreach (var propertyBag in propertyBags.Where(p => p.IsValidPropertyBag))
                builder.AppendLine($"Unity.Properties.PropertyBag.Register(new {propertyBag.PropertyBagClassName}());");

            builder.AppendLine($"    }}");
            builder.AppendLine($"}}");
            
            var propertyBagRegistryClassDeclarationSyntax = SyntaxFactory.ParseMemberDeclaration(builder.ToString()) as ClassDeclarationSyntax;

            if (null == propertyBagRegistryClassDeclarationSyntax)
                throw new Exception("Failed to construct ClassDeclarationSyntax for PropertyBagRegistry");
            
            return propertyBagRegistryClassDeclarationSyntax;
        }
    }
}