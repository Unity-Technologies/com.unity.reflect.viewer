using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Properties.Internal;

namespace Unity.Properties.CodeGen.IntegrationTests
{
#pragma warning disable 649

    [GeneratePropertyBag]
    class ClassWithCollections
    {
        public List<int> Int32List;
        public Dictionary<string, int> DictionaryStringInt32;
        public CustomList CustomList;
    }
    
    [GeneratePropertyBag]
    class ClassWithNestedCollections
    {
        public class NestedClass
        {
        }
        
        public Dictionary<string, List<List<NestedClass>>> DictionaryWithNestedCollections;
    }

    [GeneratePropertyBag]
    class CustomList : IList<int>
    {
        public int Count { get; }
        public bool IsReadOnly { get; }

        public int this[int index]
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
        
        public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public void Add(int item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(int item) => throw new NotImplementedException();
        public void CopyTo(int[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(int item) => throw new NotImplementedException();
        public int IndexOf(int item) => throw new NotImplementedException();
        public void Insert(int index, int item) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();
    }
#pragma warning restore 649

    partial class PropertyBagTests
    {
        [Test]
        public void ClassWithCollections_HasCollectionPropertyBagsGenerated()
        {
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(List<int>)), Is.InstanceOf<ListPropertyBag<int>>());
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(Dictionary<string, int>)), Is.InstanceOf<DictionaryPropertyBag<string, int>>());
        }
        
        [Test]
        public void ClassWithNestedCollections_HasCollectionPropertyBagsGenerated()
        {
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(ClassWithNestedCollections)), Is.InstanceOf<ContainerPropertyBag<ClassWithNestedCollections>>());
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(List<List<ClassWithNestedCollections.NestedClass>>)), Is.InstanceOf<ListPropertyBag<List<ClassWithNestedCollections.NestedClass>>>());
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(List<ClassWithNestedCollections.NestedClass>)), Is.InstanceOf<ListPropertyBag<ClassWithNestedCollections.NestedClass>>());
        }
    }
}