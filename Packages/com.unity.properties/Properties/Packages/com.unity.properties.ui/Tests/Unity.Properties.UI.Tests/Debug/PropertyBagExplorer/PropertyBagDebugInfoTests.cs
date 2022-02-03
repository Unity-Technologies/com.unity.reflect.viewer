using System;
using System.Linq;
using NUnit.Framework;
using Unity.Properties.Editor;
using Unity.Properties.UI;
using Unity.Properties.UI.Tests;
using Unity.Serialization.Json;
using Unity.Serialization.Json.Adapters;
#pragma warning disable 649

namespace Unity.Properties.Debug.Tests
{
    [GeneratePropertyBag]
    class CodeGenType
    {
        public float A;
        public int B;
        public string C;
        public double D;
    }

    class ReflectionType
    {
        public static void EnsurePropertyBag()
        {
            Internal.PropertyBagStore.GetPropertyBag<ReflectionType>();
        }
        
        public float A;
        public int B;
        public string C;
        public double D;
    }
    
    class ManualType
    {
        public static void EnsurePropertyBagExists()
        {
            PropertyBag.Register(new Bag());
        }
        
        public float A;
        public int B;
        public string C;
        public double D;

        class Bag : ContainerPropertyBag<ManualType>
        {
            public Bag()
            {
                AddProperty(new DelegateProperty<ManualType, float>(
                    name: nameof(A), 
                    getter: (ref ManualType c) => c.A, 
                    setter: (ref ManualType c, float v) => c.A = v));
                AddProperty(new DelegateProperty<ManualType, int>(
                    name: nameof(B),
                    getter: (ref ManualType c) => c.B, 
                    setter: (ref ManualType c, int v) => c.B = v));
                AddProperty(new DelegateProperty<ManualType, string>(
                    name: nameof(C),
                    getter: (ref ManualType c) => c.C, 
                    setter: (ref ManualType c, string v) => c.C = v));
                AddProperty(new DelegateProperty<ManualType, double>(
                    name: nameof(D),
                    getter: (ref ManualType c) => c.D, 
                    setter: (ref ManualType c, double v) => c.D = v));
            }
        }
    }

    class TypeWithMigration
    {
        public static void EnsurePropertyBag()
        {
            Internal.PropertyBagStore.GetPropertyBag<TypeWithMigration>();
        }
        
        public class Migration : IJsonMigration<TypeWithMigration>
        {
            public int Version { get; } = 0;
            public TypeWithMigration Migrate(JsonMigrationContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
    
    class TypeWithSerialization
    {
        public static void EnsurePropertyBag()
        {
            Internal.PropertyBagStore.GetPropertyBag<TypeWithSerialization>();
        }
        
        public class Migration : IJsonMigration<TypeWithSerialization>
        {
            public int Version { get; } = 0;
            public TypeWithSerialization Migrate(JsonMigrationContext context)
            {
                throw new NotImplementedException();
            }
        }
        
        public class Adapter : IJsonAdapter<TypeWithSerialization>
        {
            public void Serialize(JsonSerializationContext<TypeWithSerialization> context, TypeWithSerialization value)
            {
                throw new NotImplementedException();
            }

            public TypeWithSerialization Deserialize(JsonDeserializationContext<TypeWithSerialization> context)
            {
                throw new NotImplementedException();
            }
        }
    }
    
    class TypeWithAdapters
    {
        public static void EnsurePropertyBag()
        {
            Internal.PropertyBagStore.GetPropertyBag<TypeWithAdapters>();
        }
        
        public class Adapter : IJsonAdapter<TypeWithAdapters>
        {
            public void Serialize(JsonSerializationContext<TypeWithAdapters> context, TypeWithAdapters value)
            {
                throw new NotImplementedException();
            }

            public TypeWithAdapters Deserialize(JsonDeserializationContext<TypeWithAdapters> context)
            {
                throw new NotImplementedException();
            }
        }
    }

    class TypeWithInspector
    {
        public static void EnsurePropertyBag()
        {
            Internal.PropertyBagStore.GetPropertyBag<TypeWithInspector>();
        }
        
        public class Inspector : Inspector<TypeWithInspector>
        {
        }
    }
    
    class TypeWithPropertyInspector
    {
        public static void EnsurePropertyBag()
        {
            Internal.PropertyBagStore.GetPropertyBag<TypeWithPropertyInspector>();
        }
        
        public class Inspector : PropertyInspector<TypeWithPropertyInspector>
        {
        }
    }
    
    class TypeWithAttributeInspector
    {
        public static void EnsurePropertyBag()
        {
            Internal.PropertyBagStore.GetPropertyBag<TypeWithAttributeInspector>();
        }
        
        public class Inspector : PropertyInspector<TypeWithAttributeInspector, MinMaxAttribute>
        {
        }
    }
    
    class TypeWithUI
    {
        public static void EnsurePropertyBag()
        {
            Internal.PropertyBagStore.GetPropertyBag<TypeWithUI>();
        }
        
        public class Inspector : Inspector<TypeWithUI>
        {
        }
        
        public class PropertyInspector : PropertyInspector<TypeWithUI>
        {
        }
        
        public class AttributeInspector : PropertyInspector<TypeWithUI, MinMaxAttribute>
        {
        }
    }
    
    [UI]
    class PropertyBagDebugInfoTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ManualType.EnsurePropertyBagExists();
            ReflectionType.EnsurePropertyBag();
            TypeWithMigration.EnsurePropertyBag();
            TypeWithAdapters.EnsurePropertyBag();
            TypeWithSerialization.EnsurePropertyBag();
            TypeWithInspector.EnsurePropertyBag();
            TypeWithPropertyInspector.EnsurePropertyBag();
            TypeWithAttributeInspector.EnsurePropertyBag();
            TypeWithUI.EnsurePropertyBag();
        }
        
        [Test]
        public void TypeInfo_FromDebugInfo_AreCorrectlyGenerated()
        {
            Assert.That(PropertyBagDebugInfoStore.TryGetPropertyBagDetail(typeof(CodeGenType), out var debugInfo), Is.True);
            Assert.That(debugInfo.Serialization.Count, Is.EqualTo(0));
            Assert.That(debugInfo.UI.Count, Is.EqualTo(0));

            Assert.That(debugInfo.Name, Is.EqualTo(nameof(CodeGenType)));
            Assert.That(debugInfo.Namespace, Is.EqualTo("Unity.Properties.Debug.Tests"));
            Assert.That(debugInfo.Assembly, Is.EqualTo("Unity.Properties.UI.Tests"));
            Assert.That(debugInfo.FullName, Is.EqualTo("Unity.Properties.Debug.Tests.CodeGenType"));
            Assert.That(debugInfo.PropertyNames[0], Is.EqualTo("A"));
            Assert.That(debugInfo.PropertyNames[1], Is.EqualTo("B"));
            Assert.That(debugInfo.PropertyNames[2], Is.EqualTo("C"));
            Assert.That(debugInfo.PropertyNames[3], Is.EqualTo("D"));
            Assert.That(debugInfo.PropertyTypes[0], Is.EqualTo(TypeUtility.GetTypeDisplayName(typeof(float))));
            Assert.That(debugInfo.PropertyTypes[1], Is.EqualTo(TypeUtility.GetTypeDisplayName(typeof(int))));
            Assert.That(debugInfo.PropertyTypes[2], Is.EqualTo(TypeUtility.GetTypeDisplayName(typeof(string))));
            Assert.That(debugInfo.PropertyTypes[3], Is.EqualTo(TypeUtility.GetTypeDisplayName(typeof(double))));
            Assert.That(debugInfo.CanBeConstructed, Is.EqualTo(true));
            Assert.That(debugInfo.PropertyCount, Is.EqualTo(4));
            Assert.That(debugInfo.TypeTraits, Is.EqualTo(TypeTraits.Class));
            
        }

        [TestCase(typeof(CodeGenType), PropertyBagType.CodeGen)]
        [TestCase(typeof(ManualType), PropertyBagType.Manual)]
        [TestCase(typeof(ReflectionType), PropertyBagType.Reflection)]
        public void PropertyBagType_FromDebugInfo_AreCorrectlyGenerated(Type type, PropertyBagType bagType)
        {
            Assert.That(PropertyBagDebugInfoStore.TryGetPropertyBagDetail(type, out var debugInfo), Is.True);
            Assert.That(debugInfo.PropertyBagType, Is.EqualTo(bagType));
        }
        
        [Test]
        public void Properties_FromDebugInfo_AreCorrectlyGenerated()
        {
            Assert.That(PropertyBagDebugInfoStore.TryGetPropertyBagDetail(typeof(ReflectionType), out var debugInfo), Is.True);
            var properties = debugInfo.Properties.OfType<PropertyTypeDescriptor>().ToList();
            Assert.That(properties.Count, Is.EqualTo(4));
            Assert.That(properties[0].Descriptor.Name, Is.EqualTo("A"));
            Assert.That(properties[0].Value, Is.EqualTo(typeof(float)));
            Assert.That(properties[1].Descriptor.Name, Is.EqualTo("B"));
            Assert.That(properties[1].Value, Is.EqualTo(typeof(int)));
            Assert.That(properties[2].Descriptor.Name, Is.EqualTo("C"));
            Assert.That(properties[2].Value, Is.EqualTo(typeof(string)));
            Assert.That(properties[3].Descriptor.Name, Is.EqualTo("D"));
            Assert.That(properties[3].Value, Is.EqualTo(typeof(double)));
        }
        
        [TestCase(typeof(TypeWithMigration), typeof(TypeWithMigration.Migration))]
        [TestCase(typeof(TypeWithAdapters), typeof(TypeWithAdapters.Adapter))]
        // Adapters are cached before the migration
        [TestCase(typeof(TypeWithSerialization), typeof(TypeWithSerialization.Adapter), typeof(TypeWithSerialization.Migration))]
        public void Serialization_FromDebugInfo_AreCorrectlyGenerated(Type type, params Type[] types)
        {
            Assert.That(PropertyBagDebugInfoStore.TryGetPropertyBagDetail(type, out var debugInfo), Is.True);
            var serialization = debugInfo.Serialization.OfType<AttributeDescriptor<Type, string>>().ToList();
            Assert.That(serialization.Count, Is.EqualTo(types.Length));
            for(var i = 0; i < serialization.Count; ++i)
                Assert.That(serialization[i].Descriptor, Is.EqualTo(types[i]));
            Assert.That(debugInfo.Extensions.HasFlag(ExtensionType.Serialization), Is.True);
            Assert.That(debugInfo.Extensions.HasFlag(ExtensionType.UI), Is.False);
        }
        
        [TestCase(typeof(TypeWithInspector), typeof(TypeWithInspector.Inspector))]
        [TestCase(typeof(TypeWithPropertyInspector), typeof(TypeWithPropertyInspector.Inspector))]
        [TestCase(typeof(TypeWithAttributeInspector), typeof(TypeWithAttributeInspector.Inspector))]
        // Inspector are sorted alphabetically
        [TestCase(typeof(TypeWithUI), typeof(TypeWithUI.AttributeInspector), typeof(TypeWithUI.Inspector), typeof(TypeWithUI.PropertyInspector))]
        public void UI_FromDebugInfo_AreCorrectlyGenerated(Type type, params Type[] types)
        {
            Assert.That(PropertyBagDebugInfoStore.TryGetPropertyBagDetail(type, out var debugInfo), Is.True);
            var ui = debugInfo.UI.OfType<AttributeDescriptor<Type, string>>().ToList();
            Assert.That(ui.Count, Is.EqualTo(types.Length));
            for(var i = 0; i < ui.Count; ++i)
                Assert.That(ui[i].Descriptor, Is.EqualTo(types[i]));
            Assert.That(debugInfo.Extensions.HasFlag(ExtensionType.UI), Is.True);
            Assert.That(debugInfo.Extensions.HasFlag(ExtensionType.Serialization), Is.False);
        }
    }
}