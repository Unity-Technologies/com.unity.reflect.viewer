using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Unity.Properties.SourceGenerator
{
    static class SymbolExtensions
    {
        /// <summary>
        /// Returns <see langword="true"/> if the symbol has any attribute with the given name.
        /// </summary>
        /// <param name="symbol">The symbol to check.</param>
        /// <param name="attributeName">The attribute name to look for.</param>
        /// <returns><see langword="true"/> if the symbol has any attribute with the given name; <see langword="false"/> otherwise.</returns>
        public static bool HasAttribute(this ISymbol symbol, string attributeName)
            => symbol.GetAttributes().Any(a => a.AttributeClass.Name == attributeName);
        
        /// <summary>
        /// Enumerates all <see cref="ITypeSymbol"/> for a given <see cref="INamespaceSymbol"/> recursively.
        /// </summary>
        /// <remarks>
        /// This method will skip types contained within the <paramref name="visited"/> set. Any returned types are added to the <paramref name="visited"/> set.
        /// </remarks>
        /// <param name="symbol">The namespace to gather types from.</param>
        /// <param name="visited">A set of types which have already been visited.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> over all types in the given namespace.</returns>
        public static IEnumerable<ITypeSymbol> GetTypeMembersRecurse(this INamespaceSymbol symbol, HashSet<ISymbol> visited)
        {
            if (!visited.Add(symbol)) yield break;
            
            foreach (var member in symbol.GetNamespaceMembers().SelectMany(s => GetTypeMembersRecurse(s, visited)))
                yield return member;

            foreach (var member in symbol.GetTypeMembers().SelectMany(s => GetTypeMembersRecurse(s, visited)))
                yield return member;
        }
        
        /// <summary>
        /// Enumerates all nested <see cref="ITypeSymbol"/> for a given <see cref="ITypeSymbol"/>. This method will return the given <paramref name="symbol"/>.
        /// </summary>
        /// <remarks>
        /// This method will skip types contained within the <paramref name="visited"/> set. Any returned types are added to the <paramref name="visited"/> set.
        /// </remarks>
        /// <param name="symbol">The symbol to gather types from. This symbol is also returned in the enumerator.</param>
        /// <param name="visited">A set of types which have already been visited.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> over all types in the given type. Including the given type.</returns>
        static IEnumerable<ITypeSymbol> GetTypeMembersRecurse(this ITypeSymbol symbol, HashSet<ISymbol> visited)
        {
            if (!visited.Add(symbol)) yield break;
            
            yield return symbol;
            
            foreach (var member in symbol.GetTypeMembers().SelectMany(s => GetTypeMembersRecurse(s, visited)))
                yield return member;
        }

        /// <summary>
        /// Gets the formatted symbol name. This can be used as csharp output.
        /// </summary>
        /// <param name="symbol">The symbol to get the name for.</param>
        /// <returns>The valid C# name for the given type.</returns>
        public static string ToCSharpName(this ITypeSymbol symbol)
            => symbol.ToString().Replace("*", "");

        /// <summary>
        /// Gets the location of the syntax for a specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to get the location for.</param>
        /// <returns>The syntax location.</returns>
        public static Location GetSyntaxLocation(this ISymbol symbol)
            => symbol.DeclaringSyntaxReferences.First().SyntaxTree.GetLocation(symbol.DeclaringSyntaxReferences.First().Span);

        public static List<ITypeSymbol> GetContainingTypes(this ISymbol symbol)
        {
            var result = new List<ITypeSymbol>();

            var current = symbol.ContainingType;

            while (null != current)
            {
                result.Add(current);
                current = current.ContainingType;
            }
            
            result.Reverse();
            return result;
        }
    }
}