using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    class NodeParserTests
    {
        class NodeComparer : IEqualityComparer<NodeType>
        {
            public bool Equals(NodeType x, NodeType y)
            {
                Assert.AreEqual(x, y);
                return x == y;
            }

            public int GetHashCode(NodeType obj)
            {
                return obj.GetHashCode();
            }
        }

        static IEnumerable<NodeType> StepNodes(string json)
        {
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            using (var parser = new NodeParser(tokenizer, Allocator.TempJob))
            {
                // Tokenize the entire input data.
                Write(tokenizer, json);

                // Read until we have no more input.
                while (parser.TokenNextIndex < tokenizer.TokenNextIndex)
                {
                    var node = parser.Step();

                    if (node == NodeType.None)
                    {
                        continue;
                    }

                    yield return node;
                }

                // Flush the parser.
                while (parser.NodeType != NodeType.None)
                {
                    yield return parser.Step();
                }
            }
        }

        static IEnumerable<NodeType> StepNodes(IEnumerable<string> parts)
        {
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            using (var parser = new NodeParser(tokenizer, Allocator.TempJob))
            {
                foreach (var json in parts)
                {
                    // Tokenize a part of the input data.
                    Write(tokenizer, json);

                    // Read until we consume all input data.
                    while (parser.TokenNextIndex < tokenizer.TokenNextIndex)
                    {
                        var node = parser.Step();

                        if (node == NodeType.None)
                        {
                            continue;
                        }

                        yield return node;
                    }
                }

                // Flush the parser.
                while (parser.NodeType != NodeType.None)
                {
                    yield return parser.Step();
                }
            }
        }

        static void Write(JsonTokenizer tokenizer, string json)
        {
            unsafe
            {
                fixed (char* ptr = json)
                {
                    tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                }
            }
        }

        static readonly Dictionary<string, NodeType[]> s_NodeParserStepExpected = new Dictionary<string, NodeType[]>
        {
            {"BeginObject,EndObject,None", new[] {NodeType.BeginObject, NodeType.EndObject, NodeType.None}},
            {"BeginArray,EndArray,None", new[] {NodeType.BeginArray, NodeType.EndArray, NodeType.None}},
            {"BeginArray,Primitive,Primitive,EndArray,None", new[] {NodeType.BeginArray, NodeType.Primitive, NodeType.Primitive, NodeType.EndArray, NodeType.None}}
        };

        /// <summary>
        /// Tests the parsers output against expected results.
        /// </summary>
        [Test]
        [TestCase(@"{}", @"BeginObject,EndObject,None")]
        [TestCase(@"[]", @"BeginArray,EndArray,None")]
        [TestCase(@"[1,2]", @"BeginArray,Primitive,Primitive,EndArray,None")]
        public void NodeParser_Step(string json, string expected)
        {
            Assert.IsTrue(s_NodeParserStepExpected[expected].SequenceEqual(StepNodes(json), new NodeComparer()));
        }

        /// <summary>
        /// Tests the parsers output against expected results when streaming.
        /// </summary>
        [Test]
        [TestCase(@"{|}", @"BeginObject,EndObject,None")]
        [TestCase(@"[|]", @"BeginArray,EndArray,None")]
        [TestCase(@"[|1|,2|]", @"BeginArray,Primitive,Primitive,EndArray,None")]
        public void NodeParser_Step_Streamed(string json,  string expected)
        {
            Assert.IsTrue(s_NodeParserStepExpected[expected].SequenceEqual(StepNodes(json.Split('|')), new NodeComparer()));
        }
    }
}