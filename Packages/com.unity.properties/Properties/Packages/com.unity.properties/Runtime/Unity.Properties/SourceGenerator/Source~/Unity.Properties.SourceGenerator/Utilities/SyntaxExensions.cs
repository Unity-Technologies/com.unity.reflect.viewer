using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unity.Properties.SourceGenerator
{
    public static class SyntaxExtensions
    {
        public static bool HasAttribute(this TypeDeclarationSyntax typeDeclarationSyntax, string attributeName)
        {
            return typeDeclarationSyntax.AttributeLists
                                        .SelectMany(list => list.Attributes.Select(a => a.Name.ToString()))
                                        .SingleOrDefault(a => a == attributeName) != null;
        }
        
        /// <summary>
        /// Returns all parent namespaces for the given <see cref="SyntaxNode"/>.
        /// </summary>
        /// <remarks>
        /// This method will scan upwards from the given node.
        /// </remarks>
        /// <param name="node">The node to start searching from.</param>
        /// <returns></returns>
        public static IEnumerable<NamespaceDeclarationSyntax> GetNamespaces(this SyntaxNode node)
            => node.GetNamespacesBottomUpImpl().Reverse();

        static IEnumerable<NamespaceDeclarationSyntax> GetNamespacesBottomUpImpl(this SyntaxNode node)
        {
            var current = node;
            
            while (current.Parent != null)
            {
                if (current.Parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                    yield return namespaceDeclarationSyntax;
                
                current = current.Parent;
            }
        }

        public static IEnumerable<TypeDeclarationSyntax> GetDeclaringTypes(this SyntaxNode node)
            => node.GetDeclaringTypesBottomUpImpl().Reverse();
        
        static IEnumerable<TypeDeclarationSyntax> GetDeclaringTypesBottomUpImpl(this SyntaxNode node)
        {
            var current = node;
            
            while (current.Parent != null)
            {
                if (current.Parent is TypeDeclarationSyntax typeDeclarationSyntax)
                    yield return typeDeclarationSyntax;
                
                current = current.Parent;
            }
        }
        
        public static string GetGeneratedSourceFilePath(this SyntaxTree syntaxTree)
        {
            var result = TryGetFileNameWithExtension(syntaxTree);

            var fileName =
                result.IsSuccess
                    ? Path.ChangeExtension(result.FileName, ".g.cs")
                    : Path.Combine(Path.GetRandomFileName(), ".g.cs");

            var saveToDirectory = Path.Combine(Environment.CurrentDirectory, "Temp", "GeneratedCode");

            Directory.CreateDirectory(saveToDirectory);

            return Path.Combine(saveToDirectory, fileName);
        }

        static (bool IsSuccess, string FileName) TryGetFileNameWithExtension(SyntaxTree syntaxTree)
        {
            var fileName = Path.GetFileNameWithoutExtension(syntaxTree.FilePath);
            return (IsSuccess: true, $"{fileName}{Path.GetExtension(syntaxTree.FilePath)}");
        }
        
        public static MemberDeclarationSyntax AddNamespaces(this ClassDeclarationSyntax classDeclarationSyntax, IEnumerable<NamespaceDeclarationSyntax> namespacesFromMostToLeastNested)
        {
            var namespaces = namespacesFromMostToLeastNested.ToArray();

            if (!namespaces.Any())
                return classDeclarationSyntax;

            return namespaces.Aggregate<NamespaceDeclarationSyntax, MemberDeclarationSyntax>(classDeclarationSyntax, (current, nds) => SyntaxFactory.NamespaceDeclaration(nds.Name).AddMembers(current));
        }
        
        public static MemberDeclarationSyntax AddNamespace(this ClassDeclarationSyntax classDeclarationSyntax, string name)
        {
            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(name)).AddMembers(classDeclarationSyntax);
        }
    }
}