using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Serialization.Json.Adapters;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    partial class JsonAdapterTests
    {
        class DummyClass
        {
            
        }
        
        class ClassWithAdapters
        {
            public int A;
        }

        class DummyAdapter : IJsonAdapter<DummyClass>
        {
            public void Serialize(JsonSerializationContext<DummyClass> context, DummyClass value)
            {
                
            }

            public DummyClass Deserialize(JsonDeserializationContext<DummyClass> context)
            {
                return null;
            }
        }

        /// <summary>
        /// This class will intercept visitation for <see cref="ClassWithAdapters"/> types and write them out as a single integer value.
        /// </summary>
        class TestAdapter : IJsonAdapter<ClassWithAdapters>
        {
            void IJsonAdapter<ClassWithAdapters>.Serialize(JsonSerializationContext<ClassWithAdapters> context, ClassWithAdapters value)
            {
                context.Writer.WriteValue(value.A);
            }

            ClassWithAdapters IJsonAdapter<ClassWithAdapters>.Deserialize(JsonDeserializationContext<ClassWithAdapters> context)
            {
                return new ClassWithAdapters
                {
                    A = context.SerializedValue.AsInt32()
                };
            }
        }

        /// <summary>
        /// This class will intercept visitation for <see cref="ClassWithAdapters"/> types and write them out as a single integer value with an inverted sign.
        /// </summary>
        class TestInverter : IJsonAdapter<ClassWithAdapters>
        {
            void IJsonAdapter<ClassWithAdapters>.Serialize(JsonSerializationContext<ClassWithAdapters> context, ClassWithAdapters value)
            {
                context.Writer.WriteValue(-value.A);
            }

            ClassWithAdapters IJsonAdapter<ClassWithAdapters>.Deserialize(JsonDeserializationContext<ClassWithAdapters> context)
            {
                return new ClassWithAdapters
                {
                    A = -context.SerializedValue.AsInt32()
                };
            }
        }

        /// <summary>
        /// The <see cref="TestDecorator"/> shows an example of adding some additional structure to a type while letting the normal serialization flow happen.
        /// </summary>
        class TestDecorator : IJsonAdapter<ClassWithAdapters>
        {
            void IJsonAdapter<ClassWithAdapters>.Serialize(JsonSerializationContext<ClassWithAdapters> context, ClassWithAdapters value)
            {
                using (context.Writer.WriteObjectScope())
                {
                    context.Writer.WriteKey("Decorated");
                    context.ContinueVisitation();
                }
            }

            ClassWithAdapters IJsonAdapter<ClassWithAdapters>.Deserialize(JsonDeserializationContext<ClassWithAdapters> context)
            {
                return context.ContinueVisitation(context.SerializedValue["Decorated"]);
            }
        }

        class ClassWithAdaptedTypes
        {
            public ClassWithAdapters Value;
        }

        [Test]
        public void SerializeAndDeserialize_WithNoUserDefinedAdapter_ValueIsSerializedNormally()
        {
            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };

            var json = JsonSerialization.ToJson(src);

            Assert.That(UnFormat(json), Is.EqualTo(@"{""Value"":{""A"":42}}"));
            var dst = JsonSerialization.FromJson<ClassWithAdaptedTypes>(json);
            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }

        [Test]
        public void SerializeAndDeserialize_WithUserDefinedAdapter_AdapterIsInvoked()
        {
            var jsonSerializationParameters = new JsonSerializationParameters
            {
                UserDefinedAdapters = new List<IJsonAdapter>
                {
                    new DummyAdapter(),
                    new TestAdapter()
                }
            };

            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };

            var json = JsonSerialization.ToJson(src, jsonSerializationParameters);

            Assert.That(UnFormat(json), Is.EqualTo(@"{""Value"":42}"));

            var dst = JsonSerialization.FromJson<ClassWithAdaptedTypes>(json, jsonSerializationParameters);

            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }

        [Test]
        public void SerializeAndDeserialize_WithUserDefinedAdapter_AdapterCanContinue()
        {
            var jsonSerializationParameters = new JsonSerializationParameters
            {
                UserDefinedAdapters = new List<IJsonAdapter> {new TestDecorator()}
            };

            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };

            var json = JsonSerialization.ToJson(src, jsonSerializationParameters);

            Assert.That(UnFormat(json), Is.EqualTo(@"{""Value"":{""Decorated"":{""A"":42}}}"));

            var dst = JsonSerialization.FromJson<ClassWithAdaptedTypes>(json, jsonSerializationParameters);

            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }

        [Test]
        public void SerializeAndDeserialize_WithMultipleUserDefinedAdapters_AdaptersAreInvoked()
        {
            var jsonSerializationParameters = new JsonSerializationParameters
            {
                UserDefinedAdapters = new List<IJsonAdapter>
                {
                    // The order is important here.
                    new TestDecorator(),
                    new TestInverter()
                }
            };

            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };

            var json = JsonSerialization.ToJson(src, jsonSerializationParameters);

            Assert.That(UnFormat(json), Is.EqualTo(@"{""Value"":{""Decorated"":-42}}"));

            var dst = JsonSerialization.FromJson<ClassWithAdaptedTypes>(json, jsonSerializationParameters);

            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }

        [Test]
        public void SerializeAndDeserialize_WithMultipleUserDefinedAdapters_OnlyTheFirstAdapterIsInvoked()
        {
            var jsonSerializationParameters = new JsonSerializationParameters
            {
                UserDefinedAdapters = new List<IJsonAdapter>
                {
                    // The order is important here.
                    new TestInverter(),
                    new TestAdapter(),
                    new TestDecorator()
                }
            };

            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };

            var json = JsonSerialization.ToJson(src, jsonSerializationParameters);

            Assert.That(UnFormat(json), Is.EqualTo(@"{""Value"":-42}"));

            var dst = JsonSerialization.FromJson<ClassWithAdaptedTypes>(json, jsonSerializationParameters);

            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }

        static string UnFormat(string json)
        {
            return Regex.Replace(json, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");
        }
    }
}