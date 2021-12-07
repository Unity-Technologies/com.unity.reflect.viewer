using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Unity.Properties.SourceGenerator
{
    /// <summary>
    /// Debug extensions for <see cref="GeneratorExecutionContext"/>.
    /// </summary>
    static class GeneratorExecutionContextExtensions
    {
        /// <summary>
        /// Gets the root project path. When running out of process the project path is passed in using the <see cref="GeneratorExecutionContext.AdditionalFiles"/>.
        /// </summary>
        /// <param name="context">The current generator context.</param>
        /// <returns>The project path.</returns>
        static string GetProjectPath(this GeneratorExecutionContext context)
            => context.AdditionalFiles.Any() ? context.AdditionalFiles[0].Path : Environment.CurrentDirectory;
        
        /// <summary>
        /// Gets the debug output path where properties related output should be placed.
        /// </summary>
        /// <param name="context">The current generator context.</param>
        /// <returns>The generated debug output path.</returns>
        static string GetGeneratedDebugPath(this GeneratorExecutionContext context)
        {
            var path = Path.Combine(context.GetProjectPath(), "Temp", "GeneratedCode", "Properties");
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Gets the debug output path for the specified class name.
        /// </summary>
        /// <param name="context">The current generator context.</param>
        /// <param name="name">The source file name without extension.</param>
        /// <returns>The generated debug output path for the given source file.</returns>
        public static string GetGeneratedDebugSourcePath(this GeneratorExecutionContext context, string name)
        {
            var path = Path.Combine(context.GetGeneratedDebugPath(), context.Compilation.Assembly.Name);
            Directory.CreateDirectory(path);
            return Path.Combine(path, $"{name}.g.cs");
        }
        
        public static void WaitForDebuggerAttach(this GeneratorExecutionContext context, string inAssembly = null)
        {
            if (inAssembly != null && !context.Compilation.Assembly.Name.Contains(inAssembly))
            {
                return;
            }

            // Debugger.Launch only works on Windows and not in Rider
            while (!Debugger.IsAttached)
            {
                Task.Delay(500).Wait();
            }

            context.LogInfo($"Debugger attached to assembly: {context.Compilation.Assembly.Name}");
        }
        
        public static void LogInfo(this GeneratorExecutionContext context, string message)
        {
            // Ignore IO exceptions in case there is already a lock, could use a named mutex but don't want to eat the performance cost
            try
            {
                var path = Path.Combine(context.GetGeneratedDebugPath(), "SourceGen.log");
                
                using (var w = File.AppendText(path))
                    w.WriteLine(message);
            }
            catch (IOException) { }
        }
        
        public static void LogError(this GeneratorExecutionContext context, string errorCode, string title, string errorMessage, Location location, string description = "")
        {
            context.LogInfo($"ERROR: {errorCode}, {title}, {errorMessage}");
            var descriptor = new DiagnosticDescriptor(errorCode, title, errorMessage, "Source Generator", DiagnosticSeverity.Error, true, description);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location));
        }
    }
}