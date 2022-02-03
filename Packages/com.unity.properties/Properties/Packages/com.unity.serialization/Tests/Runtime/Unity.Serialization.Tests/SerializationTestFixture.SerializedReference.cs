using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Properties;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [GeneratePropertyBag]
        internal class Node : IEnumerable<Node>
        {
            [CreateProperty] string m_Name;
            [CreateProperty] Node m_Parent;
            [CreateProperty] List<Node> m_Children = new List<Node>();
            
            public Node() { }
            public Node(string name) => m_Name = name;

            public Node Parent => m_Parent;
            public List<Node> Children => m_Children;

            public void Add(Node child)
            {
                m_Children.Add(child);
                child.m_Parent = this;
            }

            public IEnumerator<Node> GetEnumerator()
                => m_Children.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => m_Children.GetEnumerator();
        }
        
        [Test]
        public void ClassWithSerializedReferences_ReferencesAreMaintained()
        {
            var node = new Node("node");
            
            var src = new List<Node>
            {
                node,
                node
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst[0], Is.SameAs(dst[1]));
        }
        
        [Test]
        public void ClassWithSerializedReferences_WithDisableSerializedReferencesSet_ReferencesAreMaintained()
        {
            var node = new Node("node");
            
            var src = new List<Node>
            {
                node,
                node
            };

            var parameters = new CommonSerializationParameters {DisableSerializedReferences = true};

            var dst = SerializeAndDeserialize(src, parameters);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst[0], Is.Not.SameAs(dst[1]));
        }
        
        [Test]
        public void ClassWithRecursiveReferences_CanBeSerializedAndDeserialized()
        {
            var src = new Node("root")
            {
                new Node("a"),
                new Node("b"),
                new Node("c")
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));

            AssertThatParentReferencesAreSet(dst);
            
            void AssertThatParentReferencesAreSet(Node node)
            {
                foreach (var child in node)
                {
                    Assert.That(child.Parent, Is.EqualTo(node));
                    AssertThatParentReferencesAreSet(child);
                }
            }
        }
        
        [Test]
        public void ClassWithReferenceToSelf_CanBeSerializedAndDeserialized()
        {
            var src = new Node("root");
            
            src.Add(src);

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            
            Assert.That(dst, Is.SameAs(dst.Parent));
            Assert.That(dst, Is.SameAs(dst.Children[0]));
        }
        
        [Test]
        public void ClassWithCircularReferences_CanBeSerializedAndDeserialized()
        {
            var a = new Node("a");
            
            var src = new Node("root")
            {
                a
            };

            a.Add(src);

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Parent, Is.SameAs(dst.Children[0]));
            Assert.That(dst.Children[0].Parent, Is.SameAs(dst));
        }
    }
}