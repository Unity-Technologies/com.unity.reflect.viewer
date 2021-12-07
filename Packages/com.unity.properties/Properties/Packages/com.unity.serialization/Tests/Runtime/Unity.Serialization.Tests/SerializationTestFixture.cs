using JetBrains.Annotations;
using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    struct CommonSerializationParameters
    {
        public bool DisableSerializedReferences { get; set; }
    }
    
    [TestFixture]
    abstract partial class SerializationTestFixture
    {
        protected abstract bool SupportsPolymorphicUnityObjectReferences { get; }
        
        protected abstract T SerializeAndDeserialize<T>([CanBeNull] T value, CommonSerializationParameters parameters = default);
    }
}