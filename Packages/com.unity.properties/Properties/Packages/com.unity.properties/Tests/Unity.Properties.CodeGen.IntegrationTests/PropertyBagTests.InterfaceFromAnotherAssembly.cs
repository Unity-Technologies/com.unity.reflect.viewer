using JetBrains.Annotations;
using NUnit.Framework;
using Unity.Properties.Internal;

namespace Unity.Properties.CodeGen.IntegrationTests
{
    [UsedImplicitly]
    class ClassQualifiedWithTypeFromAnotherAssemblyWithGeneratePropertyBag : IInterfaceFromAnotherAssemblyWithGeneratePropertyBag
    {
        
    }

    partial class PropertyBagTests
    {
        [Test]
        public void ClassQualifiedWithTypeFromAnotherAssembly_HasPropertyBagGenerated()
        {
            var propertyBag = PropertyBagStore.GetPropertyBag(typeof(ClassQualifiedWithTypeFromAnotherAssemblyWithGeneratePropertyBag));
            Assert.That(propertyBag, Is.InstanceOf(typeof(ContainerPropertyBag<ClassQualifiedWithTypeFromAnotherAssemblyWithGeneratePropertyBag>)));
        }
    }
}