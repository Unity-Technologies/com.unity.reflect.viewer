using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unity.Properties.SourceGenerator
{
    /// <summary>
    /// The <see cref="PropertyBagGenerator"/> is the entry point for code generation. This generator does not work with syntax but rather exclusively with the symbols.
    ///
    /// The generator work as follows:
    ///     1) Using the compilation we gather all type symbols which should have property bags generated based on the following attributes:
    ///         * <see cref="Unity.Properties.GeneratePropertyBagAttribute"/>
    ///         * <see cref="Unity.Properties.GeneratePropertyBagsForTypesQualifiedWithAttribute"/>
    ///         * <see cref="Unity.Properties.GeneratePropertyBagsForTypeAttribute"/>
    /// 
    ///     2) The type symbols are then parsed and a packed in to a <see cref="PropertyBagDefinition"/> model.
    ///     3) The <see cref="PropertyBagDefinition"/> model is then passed to the <see cref="PropertyBagFactory"/> which handles writing out <see cref="ClassDeclarationSyntax"/> objects.
    ///     4) The generated syntax objects are then written out properly formatted code.
    ///
    /// The generator does not maintain any state; this means we can produce duplicate property bag code across multiple assemblies. This can happen if a container type references a type from another assembly.
    /// </summary>
    /// <remarks>
    /// See https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/work-with-semantics for a primer on working with roslyn.
    /// </remarks>
    [Generator]
    public class PropertyBagGenerator : ISourceGenerator
    {
        /// <summary>
        /// Called before generation occurs. A generator can use the context to register callbacks required to perform generation.
        /// </summary>
        /// <param name="context">The context which can be used to register a set of callbacks.</param>
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        /// <summary>
        /// Called to perform source generation. A generator can use the context to add source files via the AddSource(String, SourceText) method.
        /// </summary>
        /// <param name="context">The context which provides access to the current compilation and allows manipulation of the output.</param>
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                context.LogInfo($"Starting source generation for Assembly=[{context.Compilation.Assembly.Name}]");
                var stopwatch = Stopwatch.StartNew();

                // Scan through the assembly and gather all types which should have property bags generated.
                var containerTypeSymbols = ContainerTypeUtility.GetPropertyContainerTypes(context.Compilation.Assembly);
                var propertyBags = containerTypeSymbols.Select(containerTypeSymbol => new PropertyBagDefinition(containerTypeSymbol)).ToList();

                if (propertyBags.Count != 0)
                {
                    var namespaceDeclarationSyntax = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Unity.Properties.Generated"));
                    var usingSystemReflectionSyntax = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection"))
                        .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($"#if !{Defines.NET_DOTS}"))
                        .WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia("#endif"));

                    foreach (var propertyBag in propertyBags)
                    {
                        if (!propertyBag.IsValidPropertyBag)
                        {
                            var rule = new DiagnosticDescriptor(
                                "SGP002", 
                                "Unable to generate PropertyBag", 
                                $"Unable to generate PropertyBag for Type=[{propertyBag.ContainerType}]. The type is inaccessible due to its protection level. The type must be flagged as 'public' or 'internal.", 
                                "Source Generator", 
                                DiagnosticSeverity.Warning, 
                                true, 
                                string.Empty);
            
                            context.ReportDiagnostic(Diagnostic.Create(rule, propertyBag.ContainerType.GetSyntaxLocation()));
                            continue;
                        }

                        foreach (var property in propertyBag.GetPropertyMembers().Where(p => !p.IsValidProperty))
                        {
                            var rule = new DiagnosticDescriptor(
                                "SGP003", 
                                "Unable to generate Property", 
                                $"Unable to generate Property=[{property.PropertyName}] with Type=[{property.MemberType}] for Container=[{propertyBag.ContainerType}]. The member type is inaccessible due to its protection level. The member type must be flagged as 'public' or 'internal.", 
                                "Source Generator", 
                                DiagnosticSeverity.Warning, 
                                true, 
                                string.Empty);
            
                            context.ReportDiagnostic(Diagnostic.Create(rule, propertyBag.ContainerType.GetSyntaxLocation()));
                        }

                        var propertyBagDeclarationSyntax = PropertyBagFactory.CreatePropertyBagClassDeclarationSyntax(propertyBag);
                        var propertyBagCompilationUnitSyntax = SyntaxFactory.CompilationUnit();

                        if (propertyBag.UsesReflection)
                            propertyBagCompilationUnitSyntax = propertyBagCompilationUnitSyntax.AddUsings(usingSystemReflectionSyntax);

                        propertyBagCompilationUnitSyntax = propertyBagCompilationUnitSyntax.AddMembers(namespaceDeclarationSyntax.AddMembers(propertyBagDeclarationSyntax));
                        propertyBagCompilationUnitSyntax = propertyBagCompilationUnitSyntax.NormalizeWhitespace();

                        var propertyBagHint = propertyBag.PropertyBagClassName;
                        var propertyBagPath = context.GetGeneratedDebugSourcePath(propertyBagHint);
                        var propertyBagSourceText = propertyBagCompilationUnitSyntax.GetTextUtf8();

                        File.WriteAllText(propertyBagPath, propertyBagSourceText);
                        context.AddSource(propertyBagHint, propertyBagSourceText);
                    }

                    var registryDeclarationSyntax = PropertyBagRegistryFactory.CreatePropertyBagRegistryClassDeclarationSyntax(propertyBags);
                    var registryCompilationUnitSyntax = SyntaxFactory.CompilationUnit();

                    registryCompilationUnitSyntax = registryCompilationUnitSyntax.AddMembers(namespaceDeclarationSyntax.AddMembers(registryDeclarationSyntax));
                    registryCompilationUnitSyntax = registryCompilationUnitSyntax.NormalizeWhitespace();

                    var propertyBagRegistryHint = "PropertyBagRegistry";
                    var propertyBagRegistryPath = context.GetGeneratedDebugSourcePath(propertyBagRegistryHint);
                    var propertyBagRegistrySourceText = registryCompilationUnitSyntax.GetTextUtf8();
                    
                    File.WriteAllText(propertyBagRegistryPath, propertyBagRegistrySourceText);
                    context.AddSource(propertyBagRegistryHint, propertyBagRegistrySourceText);
                }
                
                context.LogInfo($"Finished source generation for Assembly=[{context.Compilation.Assembly.Name}] with {propertyBags.Count} property bags in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception exception)
            {
                var rule = new DiagnosticDescriptor(
                    "SGP001", 
                    "Unknown Exception", 
                    exception.ToString(), 
                    "Source Generator", 
                    DiagnosticSeverity.Error, 
                    true, 
                    string.Empty);
                
                context.ReportDiagnostic(Diagnostic.Create(rule, context.Compilation.SyntaxTrees.First().GetRoot().GetLocation()));
            }
        }
    }
}