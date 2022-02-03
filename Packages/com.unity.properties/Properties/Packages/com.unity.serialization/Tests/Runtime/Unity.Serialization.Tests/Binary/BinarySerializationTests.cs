using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Tests;

namespace Unity.Serialization.Binary.Tests
{
    [TestFixture]
    sealed unsafe partial class BinarySerializationTests : SerializationTestFixture
    {
        protected override bool SupportsPolymorphicUnityObjectReferences => true;

        protected override T SerializeAndDeserialize<T>(T value, CommonSerializationParameters parameters = default)
        {
            var stream = new UnsafeAppendBuffer(16, 8, Allocator.Temp);

            var binarySerializationParameters = new BinarySerializationParameters
            {
                DisableSerializedReferences = parameters.DisableSerializedReferences
            };
            
            try
            {
                BinarySerialization.ToBinary(&stream, value, binarySerializationParameters);
                var reader = stream.AsReader();
                return BinarySerialization.FromBinary<T>(&reader, binarySerializationParameters);
            }
            finally
            {
                stream.Dispose();
            }
        }
    }
}