using Unity.Properties;
using NUnit.Framework;
using Unity.Properties.CodeGen.IntegrationTests;
using Unity.Properties.Internal;
using UnityEngine;

[assembly: GeneratePropertyBagsForType(typeof(ClassWithGeneric<int>))]
[assembly: GeneratePropertyBagsForType(typeof(ClassWithGeneric<NestedClass<float>>))]
[assembly: GeneratePropertyBagsForType(typeof(ClassWithGenericParameterAndGenericBase<int>))]

namespace Unity.Properties.CodeGen.IntegrationTests
{
#pragma warning disable 649
    public class ClassWithGeneric<T>
    {
        public T Value;
    }

    public class NestedClass<V>
    {
        public V Value;
    }

    public class ClassWithGenericParameterAndGenericBase<T> : Foo<T, float>
    {
        
    }

    [GeneratePropertyBag]
    public class Baz : Bar<string>
    {
        public float Root;
    }

    public class Bar<T> : Foo<float, int>
    {
        public T Value;
    }

    public class Foo<T0, T1>
    {
        public T0 Value0;
        public T1 Value1;
        
        [CreateProperty] public T0 Value0Property { get; set; } 
    }
    
#pragma warning restore 649
    
    [TestFixture]
    sealed partial class PropertyBagTests
    {
        [Test]
        public void ClassWithGeneric_HasPropertyBagGenerated()
        {
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(ClassWithGeneric<int>)), Is.InstanceOf(typeof(ContainerPropertyBag<ClassWithGeneric<int>>)));
        }
        
        [Test]
        public void ClassWithGenericNestedGeneric_HasPropertyBagGenerated()
        {
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(ClassWithGeneric<NestedClass<float>>)), Is.InstanceOf(typeof(ContainerPropertyBag<ClassWithGeneric<NestedClass<float>>>)));
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(NestedClass<float>)), Is.InstanceOf(typeof(ContainerPropertyBag<NestedClass<float>>)));
        }
        
        [Test]
        public void ClassWithSomeResolvedGenerics_HasPropertyBagGenerated()
        {
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(ClassWithGenericParameterAndGenericBase<int>)), Is.InstanceOf(typeof(ContainerPropertyBag<ClassWithGenericParameterAndGenericBase<int>>)));
            
            var container = new ClassWithGenericParameterAndGenericBase<int>
            {
                Value0 = 1, 
                Value1 = 4.2f
            };

            PropertyContainer.Accept(new DebugVisitor(), ref container); 
        }
        
        [Test]
        public void ClassWithGenericBaseClass_HasPropertyBagGenerated()
        {
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(Baz)), Is.InstanceOf(typeof(ContainerPropertyBag<Baz>)));

            var container = new Baz 
            {
                Root = 1,
                Value = "Hello",
                Value0 = 1.23f,
                Value1 = 42,
                Value0Property = 1.4f
            };

            PropertyContainer.Accept(new DebugVisitor(), ref container); 
        }

        class DebugVisitor : PropertyVisitor
        {
            protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
            {
                Debug.Log(property.Name + " = " + value.ToString() + " (" + typeof(TValue) + ")");
            }
        }
    }
}