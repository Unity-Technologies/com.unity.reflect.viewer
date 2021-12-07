using System;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unity.Properties.SourceGenerator
{
    static class CompilationUnitSyntaxExtensions
    {
        public static string GetTextUtf8(this CompilationUnitSyntax unit)
            => unit.GetText(Encoding.UTF8).ToString();
    }
}