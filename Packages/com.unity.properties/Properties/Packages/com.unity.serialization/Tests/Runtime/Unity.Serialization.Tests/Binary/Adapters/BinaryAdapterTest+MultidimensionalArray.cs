using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Binary.Adapters;
using Unity.Serialization.Tests;

namespace Unity.Serialization.Binary.Tests
{
    partial class BinaryAdapterTests
    {
        unsafe class Array2Adapter<T> : IBinaryAdapter<T[,]>
        {
            public void Serialize(BinarySerializationContext<T[,]> context, T[,] value)
            {
                if (null == value)
                {
                    context.Writer->Add(-1);
                    return;
                }
                
                var xLength = value.GetLength(0);
                var yLength = value.GetLength(1);

                context.Writer->Add(xLength);
                context.Writer->Add(yLength);

                for (var x = 0; x < xLength; x++)
                {
                    for (var y = 0; y < yLength; y++)
                    {
                        context.SerializeValue(value[x, y]);
                    }
                }
            }

            public T[,] Deserialize(BinaryDeserializationContext<T[,]> context)
            {
                var xLength = context.Reader->ReadNext<int>();

                if (xLength == -1)
                    return null;
                
                var yLength = context.Reader->ReadNext<int>();

                var value = new T[xLength, yLength];

                for (var x = 0; x < xLength; x++)
                {
                    for (var y = 0; y < yLength; y++)
                    {
                        value[x, y] = context.DeserializeValue<T>();
                    }
                }

                return value;
            }
        }

        [Test]
        [Ignore("Multidimensional arrays are not supported by code generation")]
        public unsafe void SerializeAndDeserialize_MultidimensionalArray()
        {
            var src = new ClassWithMultidimensionalArray
            {
                MultidimensionalArrayInt32 = new[,]
                {
                    {1, 2},
                    {3, 4}
                }
            };

            var parameters = new BinarySerializationParameters
            {
                UserDefinedAdapters = new List<IBinaryAdapter>
                {
                    new Array2Adapter<int>()
                }
            };
            
            using (var stream = new UnsafeAppendBuffer(16, 4, Allocator.Temp))
            {
                BinarySerialization.ToBinary(&stream, src, parameters);
                var reader = stream.AsReader();
                var dst = BinarySerialization.FromBinary<ClassWithMultidimensionalArray>(&reader, parameters);
                
                Assert.That(dst.MultidimensionalArrayInt32, Is.EqualTo(new[,]
                {
                    {1, 2},
                    {3, 4}
                }));
            }
        }
    }
}