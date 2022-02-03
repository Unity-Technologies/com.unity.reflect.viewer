using NUnit.Framework;
using Unity.Collections;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    class PackedBinaryWriterTests
    {
        [Test]
        [TestCase(@"{}")]
        [TestCase(@"{""foo"": {}, ""bar"": ""hello world""}")]
        public unsafe void PackedBinaryWriter_Write(string json)
        {
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            using (var stream = new PackedBinaryStream(Allocator.TempJob))
            using (var writer = new PackedBinaryWriter(stream, tokenizer, Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    var buffer = new UnsafeBuffer<char>(ptr, json.Length);
                    tokenizer.Write(buffer, 0, json.Length);
                    writer.Write(buffer, tokenizer.TokenNextIndex);
                }
            }
        }

        [Test]
        [TestCase(@"{""t|e|st""|:""a|b|c""}")]
        public unsafe void PackedBinaryWriter_Write_PartialKey(string input)
        {
            var parts = input.Split('|');
            
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            using (var stream = new PackedBinaryStream(Allocator.TempJob))
            using (var writer = new PackedBinaryWriter(stream, tokenizer, Allocator.TempJob))
            {
                foreach (var json in parts)
                {
                    fixed (char* ptr = json)
                    {
                        var buffer = new UnsafeBuffer<char>(ptr, json.Length);
                        tokenizer.Write(buffer, 0, json.Length);
                        writer.Write(buffer, tokenizer.TokenNextIndex);
                    }
                }

                stream.DiscardCompleted();
            }

        }
    }
}
