using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    class JsonTokenizerTests
    {
        [Test]
        [TestCase("{}")]
        [TestCase("{ }")]
        [TestCase(" \n{ \t}")]
        public unsafe void JsonTokenizer_Write_EmptyObject(string json)
        {
            fixed (char* ptr = json)
            {
                using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
                {
                    tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);

                    Assert.AreEqual(1, tokenizer.TokenNextIndex);
                    Assert.AreEqual(TokenType.Object, tokenizer.Tokens[0].Type);
                    Assert.AreEqual(-1, tokenizer.Tokens[0].Parent);
                    Assert.AreNotEqual(-1, tokenizer.Tokens[0].End);
                }
            }
        }

        [Test]
        [TestCase("[]")]
        [TestCase("[ ]")]
        [TestCase(" \n[ \t]")]
        public unsafe void JsonTokenizer_Write_EmptyArray(string json)
        {
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                }

                Assert.AreEqual(1, tokenizer.TokenNextIndex);
                Assert.AreEqual(TokenType.Array, tokenizer.Tokens[0].Type);
                Assert.AreEqual(-1, tokenizer.Tokens[0].Parent);
                Assert.AreNotEqual(-1, tokenizer.Tokens[0].End);
            }
        }

        [Test]
        [TestCase(@"{""test"": 10}")]
        [TestCase(@"{""foo"": 0.0}")]
        public unsafe void JsonTokenizer_Write_ObjectWithMember(string json)
        {
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                }

                Assert.AreEqual(3, tokenizer.TokenNextIndex);
                Assert.AreEqual(TokenType.Object, tokenizer.Tokens[0].Type);
                Assert.AreNotEqual(-1, tokenizer.Tokens[0].End);
                Assert.AreEqual(TokenType.String, tokenizer.Tokens[1].Type);
                Assert.AreEqual(TokenType.Primitive, tokenizer.Tokens[2].Type);
            }
        }

        [Test]
        [TestCase(@"{""test"":""ab", @"c""}")]
        [TestCase(@"{""test"":""", @"abc""}")]
        [TestCase(@"{""test"":""abc", @"""}")]
        [TestCase(@"{""test"":""a", @"b", @"c""}")]
        public unsafe void JsonTokenizer_Write_PartialString(params object[] parts)
        {
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            {
                foreach (string json in parts)
                {
                    Assert.IsNotNull(json);

                    fixed (char* ptr = json)
                    {
                        tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    }
                }

                Assert.AreEqual(parts.Length + 2, tokenizer.TokenNextIndex);
                Assert.AreEqual(TokenType.Object, tokenizer.Tokens[0].Type);
                Assert.AreEqual(TokenType.String, tokenizer.Tokens[1].Type);

                for (var i = 0; i < parts.Length; i++)
                {
                    var token = tokenizer.Tokens[i + 2];

                    Assert.AreEqual(i + 1, token.Parent);
                    Assert.AreEqual(TokenType.String, token.Type);

                    if (i == 0)
                    {
                        Assert.AreNotEqual(-1, token.Start);
                    }
                    else
                    {
                        Assert.AreEqual(-1, token.Start);
                    }

                    if (i == parts.Length - 1)
                    {
                        Assert.AreNotEqual(-1, token.End);
                    }
                    else
                    {
                        Assert.AreEqual(-1, token.End);
                    }
                }
            }
        }

        [Test]
        [TestCase(@"{""test"": 1|23 }")]
        [TestCase(@"{""test"": 12|3 }")]
        [TestCase(@"{""test"": 1|2|3 }")]
        public unsafe void JsonTokenizer_Write_PartialNumber(string input)
        {
            var parts = input.Split('|');
            
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            {
                foreach (var json in parts)
                {
                    Assert.IsNotNull(json);

                    fixed (char* ptr = json)
                    {
                        tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    }
                }

                Assert.AreEqual(parts.Length + 2, tokenizer.TokenNextIndex);
                Assert.AreEqual(TokenType.Object, tokenizer.Tokens[0].Type);
                Assert.AreEqual(TokenType.String, tokenizer.Tokens[1].Type);

                for (var i = 0; i < parts.Length; i++)
                {
                    var token = tokenizer.Tokens[i + 2];

                    Assert.AreEqual(i + 1, token.Parent);
                    Assert.AreEqual(TokenType.Primitive, token.Type);

                    if (i == 0)
                    {
                        Assert.AreNotEqual(-1, token.Start);
                    }
                    else
                    {
                        Assert.AreEqual(-1, token.Start);
                    }

                    if (i == parts.Length - 1)
                    {
                        Assert.AreNotEqual(-1, token.End);
                    }
                    else
                    {
                        Assert.AreEqual(-1, token.End);
                    }
                }
            }
        }

        [Test]
        [TestCase(@"{""te|st""|: 42}")]
        [TestCase(@"{""|test""|: 42}")]
        [TestCase(@"{""test|""|: 42}")]
        [TestCase(@"{""t|e|s|t""|: 42}")]
        public unsafe void JsonTokenizer_Write_PartialKey(string input)
        {
            var parts = input.Split('|');

            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            {
                foreach (var json in parts)
                {
                    fixed (char* ptr = json)
                    {
                        tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    }
                }

                Assert.AreEqual(parts.Length + 1, tokenizer.TokenNextIndex);
                Assert.AreEqual(TokenType.Object, tokenizer.Tokens[0].Type);

                for (var i = 0; i < parts.Length - 1; i++)
                {
                    var token = tokenizer.Tokens[i + 1];

                    Assert.AreEqual(i, token.Parent);
                    Assert.AreEqual(TokenType.String, token.Type);

                    if (i == 0)
                    {
                        Assert.AreNotEqual(-1, token.Start);
                    }
                    else
                    {
                        Assert.AreEqual(-1, token.Start);
                    }

                    if (i == parts.Length - 2)
                    {
                        Assert.AreNotEqual(-1, token.End);
                    }
                    else
                    {
                        Assert.AreEqual(-1, token.End);
                    }
                }
            }
        }

        [Test]
        [TestCase(@"{""foo"": 123, ""bar"": 456}", 5, 3)]
        public unsafe void JsonTokenizer_DiscardCompleted(string json, int expectedCountBeforeDiscard, int expectedCountAfterDiscard)
        {
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                }

                Assert.AreEqual(expectedCountBeforeDiscard, tokenizer.TokenNextIndex);

                tokenizer.DiscardCompleted();

                Assert.AreEqual(expectedCountAfterDiscard, tokenizer.TokenNextIndex);
            }
        }

        [Test]
        [TestCase(@"{""tes|t"": 123, ""bar"": 456}", 6, 3)]
        [TestCase(@"{""test"": 123, ""b|ar"": 456}", 6, 4)]
        [TestCase(@"{""test"": 123, ""b|ar"": 456}", 6, 4)]
        [TestCase(@"{""test"": a|b|c"" ", 5, 5)]
        public unsafe void JsonTokenizer_DiscardCompleted_Parts(string input, int expectedCountBeforeDiscard, int expectedCountAfterDiscard)
        {
            var parts = input.Split('|');
            
            using (var tokenizer = new JsonTokenizer(Allocator.TempJob))
            {
                foreach (var json in parts)
                {
                    fixed (char* ptr = json)
                    {
                        tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    }
                }

                Assert.AreEqual(expectedCountBeforeDiscard, tokenizer.TokenNextIndex);
                tokenizer.DiscardCompleted();
                Assert.AreEqual(expectedCountAfterDiscard, tokenizer.TokenNextIndex);
            }
        }

        [Test]
        public unsafe void JsonTokenizer_Write_TokenBufferOverflow_DoesNotThrow()
        {
            const string json = @"{""foo"": 123, ""bar"": 456}";

            using (var tokenizer = new JsonTokenizer(4))
            {
                Assert.DoesNotThrow(() =>
                {
                    fixed (char* ptr = json)
                    {
                        tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    }
                });
            }
        }

        [Test]
        [TestCase(@"{}}")]
        public unsafe void JsonTokenizer_Write_InvalidJson(string json)
        {
            using (var tokenizer = new JsonTokenizer(4))
            {
                Assert.Throws<InvalidJsonException>(() =>
                {
                    fixed (char* ptr = json)
                    {
                        tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    }
                });
            }
        }

        [Test]
        [TestCase(@"{a = /**/ 5}")]
        public unsafe void JsonTokenizer_Write_Comments(string json)
        {
            using (var tokenizer = new JsonTokenizer(4))
            {
                fixed (char* ptr = json)
                {
                    tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    
                    Assert.That(tokenizer.TokenNextIndex, Is.EqualTo(4));
                    Assert.That(tokenizer.Tokens[0].Type, Is.EqualTo(TokenType.Object));
                    Assert.That(tokenizer.Tokens[1].Type, Is.EqualTo(TokenType.Primitive));
                    Assert.That(tokenizer.Tokens[2].Type, Is.EqualTo(TokenType.Comment));
                    Assert.That(tokenizer.Tokens[3].Type, Is.EqualTo(TokenType.Primitive));
                }
            }
        }
    }
}
