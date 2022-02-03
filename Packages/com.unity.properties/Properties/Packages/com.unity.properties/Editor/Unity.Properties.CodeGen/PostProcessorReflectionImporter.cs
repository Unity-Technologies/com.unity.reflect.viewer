using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Unity.Properties.CodeGen
{
    class PostProcessorReflectionImporterProvider : IReflectionImporterProvider
    {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition module)
            => new PostProcessorReflectionImporter(module);
    }
    
    /// <summary>
    /// We could be running postprocessing from .NET core. In this case we need to force references to "mscorlib" instead of "System.Private.CoreLib".
    /// </summary>
    class PostProcessorReflectionImporter : DefaultReflectionImporter
    {
        const string SystemPrivateCoreLib = "System.Private.CoreLib";
        readonly AssemblyNameReference m_CoreLib;

        public PostProcessorReflectionImporter(ModuleDefinition module) : base(module)
        {
            m_CoreLib = module.AssemblyReferences.FirstOrDefault(a => a.Name == "mscorlib" || a.Name == "netstandard" || a.Name == SystemPrivateCoreLib);
        }

        public override AssemblyNameReference ImportReference(AssemblyName reference)
        {
            if (m_CoreLib != null && reference.Name == SystemPrivateCoreLib)
                return m_CoreLib;

            return base.ImportReference(reference);
        }
    }
}