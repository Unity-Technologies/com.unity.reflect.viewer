using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Properties;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [GeneratePropertyBag]
        internal class ClassWithReadOnlyValueType
        {
            public readonly int Value;
            public ClassWithReadOnlyValueType() { }
            public ClassWithReadOnlyValueType(int value) => Value = value;
        }

        [GeneratePropertyBag]
        internal class ClassWithReadOnlyObject
        {
            public readonly object Value;
            public ClassWithReadOnlyObject() { }
            public ClassWithReadOnlyObject(object value) => Value = value;
        }

        internal readonly struct ReadOnlyStruct
        {
            public readonly int Value;
            public ReadOnlyStruct(int value) => Value = value;
        }

        internal class ClassWithGenericField<T>
        {
            public T Value;
        }

        [GeneratePropertyBag]
        internal class ClassWithReadOnlyGenericType : ClassWithGenericField<ReadOnlyStruct>
        {
        }

        [GeneratePropertyBag]
        internal class ClassWithReadOnlyList
        {
            [CreateProperty] readonly List<int> m_List = new List<int>();
            public List<int> List => m_List;
        }

        internal class ClassWithReadOnlyPolymorphic
        {
            public readonly IContainerInterface Value = new ClassDerivedA();
            public ClassWithReadOnlyPolymorphic() { }
            public ClassWithReadOnlyPolymorphic(IContainerInterface value) => Value = value;
        }

        [Test]
        public void ClassWithReadOnlyValueType_Throws()
        {
            var src = new ClassWithReadOnlyValueType(42);

            Assert.Throws<SerializationException>(() => { SerializeAndDeserialize(src); },
                                                     PropertyChecks.GetReadOnlyValueTypeErrorMessage(typeof(ClassWithReadOnlyValueType), nameof(ClassWithReadOnlyValueType.Value)));
        }

        [Test]
        public void ClassWithReadOnlyObject_WhenValueIsInt32_Throws()
        {
            var src = new ClassWithReadOnlyObject(45);

            Assert.Throws<SerializationException>(() => { SerializeAndDeserialize(src); },
                                                     PropertyChecks.GetReadOnlyReferenceTypeErrorMessage(typeof(ClassWithReadOnlyObject), nameof(ClassWithReadOnlyObject.Value)));
        }

        [Test]
        public void ClassWithOnlyTypeGenericRead_Throws()
        {
            var src = new ClassWithReadOnlyGenericType
            {
                Value = new ReadOnlyStruct(42)
            };

            Assert.Throws<SerializationException>(() => { SerializeAndDeserialize(src); },
                                                     PropertyChecks.GetReadOnlyValueTypeErrorMessage(typeof(ReadOnlyStruct), nameof(ReadOnlyStruct.Value)));
        }

        [Test]
        public void ClassWithReadOnlyList_WhenListIsDefaultConstructed_DoesNotThrow()
        {
            var src = new ClassWithReadOnlyList();
            
            src.List.Add(1);
            src.List.Add(2);
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst.List, Is.Not.SameAs(src.List));
            Assert.That(dst.List.Count, Is.EqualTo(src.List.Count));
            Assert.That(dst.List[0], Is.EqualTo(src.List[0]));
            Assert.That(dst.List[1], Is.EqualTo(src.List[1]));
        }

        [Test]
        public void ClassWithReadOnlyPolymorphic_WhenDeserializedTypeDoesNotMatch_Throws()
        {
            var src = new ClassWithReadOnlyPolymorphic(new ClassDerivedB());
            
            Assert.Throws<SerializationException>(() => { SerializeAndDeserialize(src); },
                                                     PropertyChecks.GetReadOnlyReferenceTypeWithInvalidTypeErrorMessage(typeof(ClassWithReadOnlyPolymorphic), nameof(ClassWithReadOnlyPolymorphic.Value)));
        }
    }
}