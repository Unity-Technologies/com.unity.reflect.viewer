using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Binary.Adapters;

namespace Unity.Serialization.Binary.Tests
{
    [TestFixture]
    unsafe partial class BinaryAdapterTests
    {
        class DummyClass
        {
            
        }
        
        class ClassWithAdapters
        {
            public int A;
        }

        class DummyAdapter : IBinaryAdapter<DummyClass>
        {
            public void Serialize(BinarySerializationContext<DummyClass> context, DummyClass value)
            {
                
            }

            public DummyClass Deserialize(BinaryDeserializationContext<DummyClass> context)
            {
                return null;
            }
        }

        class TestAdapter : IBinaryAdapter<ClassWithAdapters>
        {
            void IBinaryAdapter<ClassWithAdapters>.Serialize(BinarySerializationContext<ClassWithAdapters> context, ClassWithAdapters value)
            {
                context.Writer->Add(value.A);
            }

            ClassWithAdapters IBinaryAdapter<ClassWithAdapters>.Deserialize(BinaryDeserializationContext<ClassWithAdapters> context)
            {
                return new ClassWithAdapters
                {
                    A = context.Reader->ReadNext<int>()
                };
            }
        }

        class TestInverter : IBinaryAdapter<ClassWithAdapters>
        {
            void IBinaryAdapter<ClassWithAdapters>.Serialize(BinarySerializationContext<ClassWithAdapters> context, ClassWithAdapters value)
            {
                context.Writer->Add(-value.A);
            }

            ClassWithAdapters IBinaryAdapter<ClassWithAdapters>.Deserialize(BinaryDeserializationContext<ClassWithAdapters> context)
            {
                return new ClassWithAdapters
                {
                    A = -context.Reader->ReadNext<int>()
                };
            }
        }

        /// <summary>
        /// The <see cref="TestDecorator"/> shows an example of adding some additional structure to a type while letting the normal serialization flow happen.
        /// </summary>
        class TestDecorator : IBinaryAdapter<ClassWithAdapters>
        {
            void IBinaryAdapter<ClassWithAdapters>.Serialize(BinarySerializationContext<ClassWithAdapters> context, ClassWithAdapters value)
            {
                context.Writer->Add(1);
                context.ContinueVisitation();
            }

            ClassWithAdapters IBinaryAdapter<ClassWithAdapters>.Deserialize(BinaryDeserializationContext<ClassWithAdapters> context)
            {
                context.Reader->ReadNext<int>();
                return context.ContinueVisitation();
            }
        }

        class ClassWithAdaptedTypes
        {
            public ClassWithAdapters Value;
        }

        T SerializeAndDeserializeWithAdapters<T>(T value, out byte[] bytes, params IBinaryAdapter[] adapters)
        {
            var stream = new UnsafeAppendBuffer(16, 8, Allocator.Temp);

            var binarySerializationParameters = new BinarySerializationParameters
            {
                UserDefinedAdapters = new List<IBinaryAdapter>(adapters)
            };
            
            try
            {
                BinarySerialization.ToBinary(&stream, value, binarySerializationParameters);
                var reader = stream.AsReader();
                
                // Output the raw bytes for tests.
                bytes = new byte[(&stream)->Length];
                fixed (byte* output = bytes)
                    UnsafeUtility.MemCpy(output, reader.Ptr, (&stream)->Length);
                
                return BinarySerialization.FromBinary<T>(&reader, binarySerializationParameters);
            }
            finally
            {
                stream.Dispose();
            }
        }

        [Test]
        public void SerializeAndDeserialize_WithNoUserDefinedAdapter_ValueIsSerializedNormally()
        {
            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };

            var dst = SerializeAndDeserializeWithAdapters(src, out var bytes);

            Assert.That(bytes.Length, Is.EqualTo(6));
            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }

        [Test]
        public void SerializeAndDeserialize_WithUserDefinedAdapter_AdapterIsInvoked()
        {
            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };
            
            var dst = SerializeAndDeserializeWithAdapters(src, out var bytes, new DummyAdapter(), new TestAdapter());

            Assert.That(bytes.Length, Is.EqualTo(5));
            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }

        [Test]
        public void SerializeAndDeserialize_WithUserDefinedAdapter_AdapterCanContinue()
        {
            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };

            var dst = SerializeAndDeserializeWithAdapters(src, out var bytes, new TestDecorator());

            Assert.That(bytes.Length, Is.EqualTo(10));
            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }

        [Test]
        public void SerializeAndDeserialize_WithMultipleUserDefinedAdapters_AdaptersAreInvoked()
        {
            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };

            // The order is important here.
            var dst = SerializeAndDeserializeWithAdapters(src, out var bytes, new TestDecorator(), new TestInverter());
            
            Assert.That(bytes.Length, Is.EqualTo(9));
            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }

        [Test]
        public void SerializeAndDeserialize_WithMultipleUserDefinedAdapters_OnlyTheFirstAdapterIsInvoked()
        {
            var src = new ClassWithAdaptedTypes
            {
                Value = new ClassWithAdapters {A = 42}
            };

            // The order is important here.
            var dst = SerializeAndDeserializeWithAdapters(src, out var bytes, new TestInverter(), new TestAdapter(), new TestDecorator());

            Assert.That(bytes.Length, Is.EqualTo(5));
            Assert.That(dst.Value.A, Is.EqualTo(src.Value.A));
        }
    }
}