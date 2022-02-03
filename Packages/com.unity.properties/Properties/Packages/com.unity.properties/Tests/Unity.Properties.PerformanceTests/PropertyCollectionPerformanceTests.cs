using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Properties;

[assembly: GeneratePropertyBagsInEditor]

namespace Unity.Properties.PerformanceTests
{
    [TestFixture]
    sealed class PropertyCollectionPerformanceTests
    {
        [GeneratePropertyBag]
        internal class ClassWithPrimitives
        {
#pragma warning disable 649
            public bool BoolValue;
            public sbyte Int8Value;
            public short Int16Value;
            public int Int32Value;
            public long Int64Value;
            public byte UInt8Value;
            public ushort UInt16Value;
            public uint UInt32Value;
            public ulong UInt64Value;
            public float Float32Value;
            public double Float64Value;
            public char CharValue;
            public string StringValue;
#pragma warning restore 649
        }
        
        [Test, Performance]
        public void PropertyBag_GetPropertiesTyped()
        {
            var propertyBag = PropertyBag.GetPropertyBag<ClassWithPrimitives>();
            var container = new ClassWithPrimitives();
            
            Measure.Method(() => {

                    if (propertyBag is ContainerPropertyBag<ClassWithPrimitives> typed)
                    {
                        /*
                        foreach (var property in typed.m_PropertiesList)
                        {
                        }
                        */
                    }
                    else
                    {
                        foreach (var property in propertyBag.GetProperties(ref container))
                        {
                        }
                    }
                })
                .WarmupCount(1)
                .MeasurementCount(100)
                .IterationsPerMeasurement(100)
                .GC()
                .Run();
        }
        
        [Test, Performance]
        public void PropertyBag_GetProperties()
        {
            var propertyBag = PropertyBag.GetPropertyBag<ClassWithPrimitives>();
            var container = new ClassWithPrimitives();
            
            Measure.Method(() => {
                    foreach (var property in propertyBag.GetProperties(ref container))
                    {
                    }
                })
                .WarmupCount(1)
                .MeasurementCount(100)
                .IterationsPerMeasurement(100)
                .GC()
                .Run();
        }
        
        [Test, Performance]
        public void PropertyBag_GetProperties_pooled()
        {
            var propertyBag = PropertyBag.GetPropertyBag<ClassWithPrimitives>();
            var container = new ClassWithPrimitives();
            
            Measure.Method(() => {
                    foreach (var property in propertyBag.GetProperties())
                    {
                    }
                })
                .WarmupCount(1)
                .MeasurementCount(100)
                .IterationsPerMeasurement(100)
                .GC()
                .Run();
        }
    }
}