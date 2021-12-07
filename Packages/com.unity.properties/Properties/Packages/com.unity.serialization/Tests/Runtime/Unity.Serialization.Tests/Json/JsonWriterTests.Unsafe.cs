using NUnit.Framework;
using Unity.Burst;
using Unity.Jobs;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    sealed partial class JsonWriterTests
    {
        [BurstCompile(CompileSynchronously = true)]
        struct WriteJob : IJob
        {
            public JsonWriter.Unsafe Writer;
            
            public unsafe void Execute()
            {
                Writer.WriteBeginObject();

                var key = stackalloc char[4] {'t', 'e', 's', 't'};
                Writer.WriteKey(key, 4);
                Writer.WriteBeginArray();
                Writer.WriteValue((int) -1);
                Writer.WriteValue((long) long.MaxValue);
                Writer.WriteEndArray();
                
                Writer.WriteEndObject();
            }
        }
        
        [Test]
        public void JsonWriter_IsBurstCompatible()
        {
            new WriteJob
            {
                Writer = m_Writer.AsUnsafe()
            }.Schedule().Complete();
            
            AssertThatJsonIs(@"{
    ""test"": [
        -1,
        9223372036854775807
    ]
}");
        }
    }
}