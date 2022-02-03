using System;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Properties;
using Unity.Serialization.PerformanceTests;
using UnityEngine;

namespace Unity.Serialization.Json.PerformanceTests
{
    [TestFixture]
    [Category("Performance")]
    partial class JsonDeserializationPerformanceTests
    {
        [Test, Performance]
        [TestCase(@"{""Value"":""JarJar""}")]
        public void DeserializeFromString_UsingJsonUtility_BasicData(string json) 
        {
            Measure.Method(() => {
                       var data = UnityEngine.JsonUtility.FromJson<BasicData>(json);
                   })
                   .WarmupCount(1)
                   .MeasurementCount(100)
                   .IterationsPerMeasurement(100)
                   .Run();
            PerformanceTest.Active.CalculateStatisticalValues();
            Debug.Log(PerformanceTest.Active.ToString());
        }
        
        [Test, Performance]
        [TestCase(@"{""Value"":""JarJar""}")]
        public void DeserializeFromString_UsingJsonSerialization_BasicData(string json) 
        {
 
            Measure.Method(() => {
                       var data = JsonSerialization.FromJson<BasicData>(json);
                   })
                   .WarmupCount(1)
                   .MeasurementCount(100)
                   .IterationsPerMeasurement(100)
                   .Run();
            
            PerformanceTest.Active.CalculateStatisticalValues();
            Debug.Log(PerformanceTest.Active.ToString());
        }
        
        [TestCase(@"{""Value"":""Barry""}", 1)]
        [TestCase(@"{""Value"":""Barry""}", 10)]
        [TestCase(@"{""Value"":""Barry""}", 100)]
        [TestCase(@"{""Value"":""Barry""}", 1000)]
        [Test, Performance]
        public unsafe void DeserializeFromString_UsingSerializedObjectReaderWithNoActualization_BasicDataBatch(string json, int batchSize)
        {
            var jsonBatch = $"{{\"batch\":[{string.Join(",", Enumerable.Repeat(json, batchSize))}]}}";
            Measure.Method(() =>
                   {
                       var config = SerializedObjectReaderConfiguration.Default;
                       
                       config.UseReadAsync = false;
                       config.ValidationType = JsonValidationType.None;
                       config.NodeBufferSize = 128;
                       config.TokenBufferSize = Math.Max(jsonBatch.Length / 2, 16);
                       config.BlockBufferSize = jsonBatch.Length;
                       config.OutputBufferSize = jsonBatch.Length;
                       
                       fixed (char* ptr = jsonBatch)
                       {
                           using (var reader = new SerializedObjectReader(new UnsafeBuffer<char>(ptr, jsonBatch.Length), config))
                           {
                               var document = reader.ReadObject().AsUnsafe();
                           }
                       }

                   })
                   .WarmupCount(1)
                   .MeasurementCount(100)
                   .IterationsPerMeasurement(1)
                   .Run();
            PerformanceTest.Active.CalculateStatisticalValues();
        }
        
        [TestCase(@"{""Value"":""Barry""}", 1)]
        [TestCase(@"{""Value"":""Barry""}", 10)]
        [TestCase(@"{""Value"":""Barry""}", 100)]
        [TestCase(@"{""Value"":""Barry""}", 1000)]
        [Test, Performance]
        public unsafe void DeserializeFromString_UsingSerializedObjectReader_BasicDataBatch(string json, int batchSize)
        {
            var jsonBatch = $"{{\"batch\":[{string.Join(",", Enumerable.Repeat(json, batchSize))}]}}";
            Measure.Method(() =>
                   {
                       var config = SerializedObjectReaderConfiguration.Default;
                       
                       config.UseReadAsync = false;
                       config.ValidationType = JsonValidationType.None;
                       config.NodeBufferSize = 128;
                       config.TokenBufferSize = Math.Max(jsonBatch.Length / 2, 16);
                       config.BlockBufferSize = jsonBatch.Length;
                       config.OutputBufferSize = jsonBatch.Length;

                       var data = new BasicDataBatch();

                       fixed (char* ptr = jsonBatch)
                       {
                           using (var reader = new SerializedObjectReader(new UnsafeBuffer<char>(ptr, jsonBatch.Length), config))
                           {
                               var document = reader.ReadObject().AsUnsafe();

                               if (document.TryGetValue("batch", out var batch))
                               {
                                   var array = batch.AsArrayView();

                                   data.batch = new BasicData[array.Count()];

                                   var index = 0;

                                   foreach (var element in array)
                                   {
                                       var obj = element.AsObjectView();

                                       data.batch[index++] = new BasicData
                                       {
                                           Value = obj["Value"].ToString()
                                       };
                                   }
                               }
                           }
                       }

                   })
                   .WarmupCount(1)
                   .MeasurementCount(100)
                   .IterationsPerMeasurement(1)
                   .Run();
            PerformanceTest.Active.CalculateStatisticalValues();
        }
        
        [TestCase(@"{""Value"":""Barry""}", 1)]
        [TestCase(@"{""Value"":""Barry""}", 10)]
        [TestCase(@"{""Value"":""Barry""}", 100)]
        [TestCase(@"{""Value"":""Barry""}", 1000)]
        [Test, Performance]
        public void DeserializeFromString_UsingJsonSerialization_BasicDataBatch(string json, int count) {
            
            var jsonBatch = $"{{\"batch\":[{string.Join(",", Enumerable.Repeat(json, count))}]}}";
            Measure.Method(() => {
                       var data = JsonSerialization.FromJson<BasicDataBatch>(jsonBatch);
                   })
                   .WarmupCount(1)
                   .MeasurementCount(100)
                   .IterationsPerMeasurement(1)
                   .Run();
            PerformanceTest.Active.CalculateStatisticalValues();
        }
        
        [TestCase(@"{""Value"":""Barry""}", 1)]
        [TestCase(@"{""Value"":""Barry""}", 10)]
        [TestCase(@"{""Value"":""Barry""}", 100)]
        [TestCase(@"{""Value"":""Barry""}", 1000)]
        [Test, Performance]
        public void DeserializeFromString_UsingJsonUtility_BasicDataBatch(string json, int count) {
            
            var jsonBatch = $"{{\"batch\":[{string.Join(",", Enumerable.Repeat(json, count))}]}}";
            Measure.Method(() => {
                       var data = UnityEngine.JsonUtility.FromJson<BasicDataBatch>(jsonBatch);
                   })
                   .WarmupCount(1)
                   .MeasurementCount(100)
                   .IterationsPerMeasurement(1)
                   .Run();
            PerformanceTest.Active.CalculateStatisticalValues();
        }
    }
}