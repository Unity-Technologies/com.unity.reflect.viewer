using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Properties.Reflection.Internal;
#pragma warning disable 649

namespace Unity.Properties.Reflection.Tests
{
    partial class ReflectedPropertyBagTests
    {
        class ClassWithMultidimensionalArray
        {
            public int[,] IntArrayField;
        }

        class ClassWithMultidimensionalGeneric
        {
            public Dictionary<int, int[,]> DictionaryWithMultidimensionalArray;
            public Dictionary<int, List<int[,]>> DictionaryWithListOfMultidimensionalArray;
        }

        [Test]
        public void CreatePropertyBag_MultidimensionalArray_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => { new ReflectedPropertyBagProvider().CreatePropertyBag<int[,]>(); });
        }

        [Test]
        public void CreatePropertyBag_ClassWithMultidimensionalArray_GeneratesPropertyBag()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<ClassWithMultidimensionalArray>();
            var container = new ClassWithMultidimensionalArray();
            Assert.That(propertyBag.GetProperties(ref container).Count(), Is.EqualTo(1));
        }

        [Test]
        public void CreatePropertyBag_ClassWithMultidimensionalGeneric_GeneratesPropertyBag()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<ClassWithMultidimensionalGeneric>();
            var container = new ClassWithMultidimensionalGeneric();
            Assert.That(propertyBag.GetProperties(ref container).Count(), Is.EqualTo(2));
        }
    }
}
