using System;
using NUnit.Framework;
using Unity.Collections;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    sealed partial class JsonWriterTests
    {
        JsonWriter m_Writer;

        [SetUp]
        public void SetUp()
            => m_Writer = new JsonWriter(Allocator.TempJob);

        [TearDown]
        public void TearDown()
            => m_Writer.Dispose();

        
        [Test]
        public void WriteObject_AsRootWithNoMembers()
        {
            using (m_Writer.WriteObjectScope()) {}
            AssertThatJsonIs("{}");
        }
        
        [Test]
        public void WriteArray_AsRootWithNoMembers()
        {
            using (m_Writer.WriteArrayScope()) {}
            AssertThatJsonIs("[]");
        }

        [Test]
        public void WriteValue_AsObjectMember()
        {
            using (m_Writer.WriteObjectScope())
            {
                m_Writer.WriteKey("int");
                m_Writer.WriteValue((int) -10);
                
                m_Writer.WriteKey("long");
                m_Writer.WriteValue((long) 500);
                
                m_Writer.WriteKey("float");
                m_Writer.WriteValue((float) 1.23f);

                m_Writer.WriteKey("string");
                m_Writer.WriteValue("test");
            }
            
            AssertThatJsonIs(expected: @"{
    ""int"": -10,
    ""long"": 500,
    ""float"": 1.23,
    ""string"": ""test""
}");
        }
        
        [Test]
        public void WriteValue_AsRoot()
        {
            m_Writer.WriteValue("test");
            
            AssertThatJsonIs(expected: @"""test""");
        }
        
        [Test]
        public void Write_WithValueAsRoot_Throws()
        {
            m_Writer.WriteValue(1);

            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Writer.WriteBeginArray();
            });
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Writer.WriteBeginObject();
            });
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Writer.WriteKey("test");
            });
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Writer.WriteValue("test");
            });
            
            AssertThatJsonIs(expected: @"1");
        }
        
        [Test]
        public void WriteValueNull_AsRoot()
        {
            m_Writer.WriteNull();
            
            AssertThatJsonIs(expected: @"null");
        }

        [Test]
        public void WriteObject_AsRootWithKey_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Writer.WriteBeginObject("obj");
            });
        }

        [Test]
        public void WriteObject_AsMemberWithNoKey_Throws()
        {
            m_Writer.WriteBeginObject();
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Writer.WriteBeginObject();
            });
        }

        [Test]
        public void WriteObject_AsMemberWithKey()
        {
            using (m_Writer.WriteObjectScope())
            {
                using (m_Writer.WriteObjectScope("obj"))
                {
                }
            }
            
            AssertThatJsonIs(expected: @"{
    ""obj"": {}
}");
        }

        [Test]
        public void WriteArray_AsRootWithObjectElements()
        {
            using (m_Writer.WriteArrayScope())
            {
                using (m_Writer.WriteObjectScope())
                {
                }
                
                using (m_Writer.WriteObjectScope())
                {
                }
                
                using (m_Writer.WriteObjectScope())
                {
                }
            }

            AssertThatJsonIs(expected: @"[
    {},
    {},
    {}
]");
        }

        [Test]
        public void WriteArray_AsRootWithValueElements()
        {
            using (m_Writer.WriteArrayScope())
            {
                m_Writer.WriteValue((int) -10);
                m_Writer.WriteValue((long) 500);
                m_Writer.WriteValue((float) 1.23f);
                m_Writer.WriteValue("test");
            }

            AssertThatJsonIs(expected: @"[
    -10,
    500,
    1.23,
    ""test""
]");
        }

        [Test]
        public void WriteKey_AsArrayElement_Throws()
        {
            m_Writer.WriteBeginArray();

            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Writer.WriteKey("test");
            });
        }

        [Test]
        public void WriteObject_WithArrayElement()
        {
            using (m_Writer.WriteObjectScope())
            {
                using (m_Writer.WriteArrayScope("arr"))
                {
                }
            }
            
            AssertThatJsonIs(expected: @"{
    ""arr"": []
}");
        }
        
        [Test]
        public void WriteValue_AsObjectMember_UsingKeyValue()
        {
            using (m_Writer.WriteObjectScope())
            {
                m_Writer.WriteKeyValue("int", (int) -10);
                m_Writer.WriteKeyValue("long", (long) 500);
                m_Writer.WriteKeyValue("float", (float) 1.23f);
                m_Writer.WriteKeyValue("string", "test");
            }
            
            AssertThatJsonIs(expected: @"{
    ""int"": -10,
    ""long"": 500,
    ""float"": 1.23,
    ""string"": ""test""
}");
        }

        [Test]
        public void WriteObject_WithNestedObjectAndArray()
        {
            using (m_Writer.WriteObjectScope())
            {
                m_Writer.WriteKeyValue("a", "test");
                
                using (m_Writer.WriteObjectScope("nested"))
                {
                    m_Writer.WriteKeyValue("b", 10.2f);
                    
                    using (m_Writer.WriteArrayScope("arr"))
                    {
                        using (m_Writer.WriteObjectScope())
                        {
                            m_Writer.WriteKeyValue("c", -100);
                        }
                        
                        m_Writer.WriteNull();
                    }
                }
            }
            
            AssertThatJsonIs(expected: @"{
    ""a"": ""test"",
    ""nested"": {
        ""b"": 10.2,
        ""arr"": [
            {
                ""c"": -100
            },
            null
        ]
    }
}");
        }

        [Test]
        public void WriteObject_Simplified()
        {
            using (var writer = new JsonWriter(Allocator.TempJob, new JsonWriterOptions {Simplified = true}))
            {
                using (writer.WriteObjectScope())
                {
                    using (writer.WriteArrayScope("arr"))
                    {
                        writer.WriteNull();
                    }
                }
            
                AssertThatJsonIs(writer, expected: @"{
    arr = [
        null
    ]
}");
            }
        }

        [Test]
        public void WriteObject_Minified()
        {
            using (var writer = new JsonWriter(Allocator.TempJob, new JsonWriterOptions {Minified = true}))
            {
                using (writer.WriteObjectScope())
                {
                    writer.WriteKeyValue("a", "test");
                    writer.WriteKeyValue("b", 1);
                    writer.WriteKeyValue("c", 2.3f);
                }
            
                AssertThatJsonIs(writer, expected: @"{""a"":""test"",""b"":1,""c"":2.3}");
            }
        }
        
        void AssertThatJsonIs(string expected)
        {
            AssertThatJsonIs(m_Writer, expected);
        }
        
        static void AssertThatJsonIs(JsonWriter writer, string expected)
        {
            Assert.That(writer.ToString(), Is.EqualTo(expected.Replace("\r", "")));
        }

    }
}