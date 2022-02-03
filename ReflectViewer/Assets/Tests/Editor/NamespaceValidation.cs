using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Reflect.Viewer.UI;
using NUnit.Compatibility;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ReflectViewerEditorTests
{
    public class NamespaceValidation
    {
        public const string k_AssemblyName = "ReflectViewer";
        public const string k_ObsoleteNamespacePrefix= "UnityEngine.Reflect";
        [Test]
        public void Verify_Assembly_Types_Have_Namespace()
        {
            var assembly = Assembly.Load(new AssemblyName(k_AssemblyName));
            var types = assembly.GetTypes().Where((t) => t.Namespace == null && t.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) == null);

            if (types.Any())
                Assert.Fail("Types: {0} does not have a namespace", types
                    .Select((t) => t.Name)
                    .Aggregate((n1, n2) => string.Format("{0}, {1}", n1, n2)));
        }

        [Test]
        [Ignore("This will require major namespace refactoring, should start from the package then up to the viewer. It was tried and some references were lost, specially on the pipeline.")]
        public void Verify_Assembly_Types_Namespace_Is_Reflect()
        {
            var assembly = Assembly.Load(new AssemblyName(k_AssemblyName));
            var types = assembly.GetTypes().Where((t) => t.Namespace.Contains(k_ObsoleteNamespacePrefix) && t.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) == null);

            if (types.Any())
                Assert.Fail("Types: {0} are in the UnityEngine.Reflect namespace, please change to Unity.Reflect", types
                    .Select((t) => t.Name)
                    .Aggregate((n1, n2) => string.Format("{0}, {1}", n1, n2)));
        }
    }
}

