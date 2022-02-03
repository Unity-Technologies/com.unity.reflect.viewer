using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Serialization.Json.Adapters;
using Unity.Serialization.Tests;

namespace Unity.Serialization.Json.Tests
{
    partial class JsonAdapterTests
    {
        class Array2Adapter<T> : IJsonAdapter<T[,]>
        {
            public void Serialize(JsonSerializationContext<T[,]> context, T[,] value)
            {
                if (null == value)
                {
                    context.Writer.WriteNull();
                    return;
                }
                
                using (context.Writer.WriteArrayScope())
                {
                    for (var x = 0; x < value.GetLength(0); x++)
                    {
                        using (context.Writer.WriteArrayScope())
                        {
                            for (var y = 0; y < value.GetLength(1); y++)
                            {
                                context.SerializeValue(value[x, y]);
                            }
                        }
                    }
                }
            }

            public T[,] Deserialize(JsonDeserializationContext<T[,]> context)
            {
                if (context.SerializedValue.IsNull())
                {
                    return null;
                }
                
                var xArray = context.SerializedValue.AsArrayView();
                
                var xCount = xArray.Count();
                var yCount = xArray.First().AsArrayView().Count();

                var value = new T[xCount, yCount];
                
                var xIndex = 0;
                
                foreach (var yArray in xArray)
                {
                    var yIndex = 0;
                    
                    foreach (var element in yArray.AsArrayView())
                    {
                        value[xIndex, yIndex] = context.DeserializeValue<T>(element);
                        yIndex++;
                    }

                    xIndex++;
                }

                return value;
            }
        }

        [Test]
        [Ignore("Multidimensional arrays are not supported by code generation")]
        public void SerializeAndDeserialize_MultidimensionalArray()
        {
            var src = new ClassWithMultidimensionalArray
            {
                MultidimensionalArrayInt32 = new[,]
                {
                    {1, 2},
                    {3, 4}
                }
            };

            var parameters = new JsonSerializationParameters
            {
                UserDefinedAdapters = new List<IJsonAdapter>
                {
                    new Array2Adapter<int>()
                }
            };

            var json = JsonSerialization.ToJson(src, parameters);
            Assert.That(UnFormat(json), Is.EqualTo("{\"MultidimensionalArrayInt32\":[[1,2],[3,4]]}"));

            var dst = default(ClassWithMultidimensionalArray);
            JsonSerialization.FromJsonOverride(json, ref dst, parameters);
            Assert.That(dst.MultidimensionalArrayInt32, Is.EqualTo(new [,]
            {
                {1, 2},
                {3, 4}
            }));
        }
    }
}