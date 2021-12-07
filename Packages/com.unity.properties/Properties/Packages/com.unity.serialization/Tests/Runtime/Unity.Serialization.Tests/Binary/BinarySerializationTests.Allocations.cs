using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;
using Unity.Serialization.Tests;

namespace Unity.Serialization.Binary.Tests
{
    partial class BinarySerializationTests
    {
        [TestFixture]
        internal unsafe class Allocations
        {
            [GeneratePropertyBag]
            internal struct StructWithInt32Property
            {
                [CreateProperty] public int Int32Property { get; set; }
            }

            [Test]
            public void ToBinary_StructWithInt32Property_DoesNotAllocate()
            {
                var container = new StructWithInt32Property();
                
                GCAllocTest.Method(() =>
                           {
                               using (var stream = new UnsafeAppendBuffer(16, 8, Allocator.Temp))
                               {
                                   BinarySerialization.ToBinary(&stream, container);
                               }
                           })
                           .ExpectedCount(0)
                           .Warmup()
                           .Run();
            }

            [Test]
            public void FromBinary_StructWithInt32Property_DoesNotAllocate()
            {
                var container = new StructWithInt32Property();

                using (var writer = new UnsafeAppendBuffer(16, 8, Allocator.Temp))
                {
                    var writerPtr = &writer;
                    
                    BinarySerialization.ToBinary(writerPtr, container);

                    GCAllocTest.Method(() =>
                               {
                                   var reader = writerPtr->AsReader();
                                   var readerPtr = &reader;

                                   BinarySerialization.FromBinary<StructWithInt32Property>(readerPtr);
                               })
                               .ExpectedCount(0)
                               .Warmup()
                               .Run();
                }
            }
        }
    }
}