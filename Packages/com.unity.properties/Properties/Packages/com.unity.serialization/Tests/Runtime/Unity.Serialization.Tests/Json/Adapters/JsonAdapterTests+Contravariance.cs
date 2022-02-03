using System.Collections.Generic;
using NUnit.Framework;
using Unity.Serialization.Json.Adapters;

namespace Unity.Serialization.Json.Tests
{
    partial class JsonAdapterTests
    {
        interface IShape
        {
            
        }

        class Square : IShape
        {
            
        }
        
        class Circle : IShape
        {
            
        }

        interface IAnimal
        {
            
        }

        class Dog : IAnimal
        {
            
        }

        class Cat : IAnimal
        {
            
        }
        
        class ClassWithShapes
        {
            public IShape Shape;
            public Square Square;
            public Circle Circle;
            public IAnimal Animal;
            public Dog Dog;
            public Cat Cat;
        }

        class ShapeAdapter : Adapters.Contravariant.IJsonAdapter<IShape>
        {
            public void Serialize(IJsonSerializationContext context, IShape value)
            {
                context.Writer.WriteValue("a shape");
            }

            public object Deserialize(IJsonDeserializationContext context)
            {
                return null;
            }
        }

        class AnimalAdapter : 
            Adapters.Contravariant.IJsonAdapter<IAnimal>,
            Adapters.Contravariant.IJsonAdapter<Dog>,
            IJsonAdapter<Cat>
        {
            public void Serialize(IJsonSerializationContext context, IAnimal value)
            {
                context.Writer.WriteValue("an animal");
            }

            object Adapters.Contravariant.IJsonAdapter<IAnimal>.Deserialize(IJsonDeserializationContext context)
            {
                return null;
            }
            
            public void Serialize(IJsonSerializationContext context, Dog value)
            {
                context.Writer.WriteValue("a dog");
            }

            object Adapters.Contravariant.IJsonAdapter<Dog>.Deserialize(IJsonDeserializationContext context)
            {
                return null;
            }

            public void Serialize(JsonSerializationContext<Cat> context, Cat value)
            {
                context.Writer.WriteValue("a cat");
            }

            public Cat Deserialize(JsonDeserializationContext<Cat> context)
            {
                return null;
            }
        }

        [Test]
        public void SerializeAndDeserialize_WithContravariantUserDefinedAdapter_AdapterIsInvokedCorrectly()
        {
            var jsonSerializationParameters = new JsonSerializationParameters
            {
                UserDefinedAdapters = new List<IJsonAdapter>
                {
                    new DummyAdapter(),
                    new ShapeAdapter(),
                    new AnimalAdapter()
                }
            };

            var src = new ClassWithShapes
            {
                Shape = new Square(),
                Square = new Square(),
                Circle = new Circle(),
                
                Animal = new Cat(),
                Dog = new Dog(),
                Cat = null
            };

            var json = JsonSerialization.ToJson(src, jsonSerializationParameters);

            Assert.That(UnFormat(json), Is.EqualTo(@"{""Shape"":""a shape"",""Square"":""a shape"",""Circle"":""a shape"",""Animal"":""an animal"",""Dog"":""a dog"",""Cat"":""a cat""}"));
        }
    }
}