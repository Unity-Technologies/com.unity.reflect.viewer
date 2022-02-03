using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.PerformanceTesting;
using Unity.Serialization.Tests;
using UnityEngine;

namespace Unity.Serialization.Binary.PerformanceTests
{
    [TestFixture]
    sealed class BinarySerializationPerformanceTests
    {
        UnsafeAppendBuffer m_Stream;
        
        [SetUp]
        public void SetUp()
        {
            m_Stream = new UnsafeAppendBuffer(8, 8, Allocator.Persistent);
        }

        [TearDown]
        public void TearDown()
        {
            m_Stream.Dispose();
        }
        
        [Test, Performance]
        public unsafe void ToBinary_StructWithPrimitives() 
        {
            fixed (UnsafeAppendBuffer* stream = &m_Stream)
            {
                var ptr = stream;
                
                Measure.Method(() =>
                       {
                           ptr->Length = 0;
                           BinarySerialization.ToBinary(ptr, new StructWithPrimitives());
                       })
                       .WarmupCount(1)
                       .MeasurementCount(100)
                       .IterationsPerMeasurement(100)
                       .Run();
            
                PerformanceTest.Active.CalculateStatisticalValues();
                Debug.Log(PerformanceTest.Active.ToString());
            }
        }
    }
}