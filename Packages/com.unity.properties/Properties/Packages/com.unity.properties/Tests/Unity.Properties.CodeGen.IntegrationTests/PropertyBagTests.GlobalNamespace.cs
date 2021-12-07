using NUnit.Framework;
using Unity.Properties;
using Unity.Properties.Internal;

[GeneratePropertyBag]
class ClassInGlobalNamespace
{
}


namespace Unity.Properties.CodeGen.IntegrationTests
{
    partial class PropertyBagTests
    {
        [Test]
        public void ClassInGlobalNamespace_HasPropertyBagsGenerated()
        {
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(ClassInGlobalNamespace)), Is.InstanceOf<ContainerPropertyBag<ClassInGlobalNamespace>>());
        }
    }
}