using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Serialization.Json;
using UnityEngine;

namespace Unity.Serialization.Json.PerformanceTests
{
    [TestFixture]
    partial class JsonDeserializationPerformanceTests
    {
        [Test, Performance]
        [TestCase(100)]
        [TestCase(1000)]
        public void DeserializeFromFile_UsingJsonSerialization_MockEntityData(int count)
        {
            File.WriteAllText("test.json", JsonTestData.GetMockEntities(count));

            try
            {
                Measure.Method(() => { JsonSerialization.FromJson<MockEntityBatch>(new FileInfo("test.json")); })
                       .WarmupCount(1)
                       .MeasurementCount(100)
                       .Run();

                PerformanceTest.Active.CalculateStatisticalValues();

                var size = new FileInfo("test.json").Length / (double) 1024 / 1024;
                Debug.Log($"MB/s=[{size / (PerformanceTest.Active.SampleGroups.First().Median / 1000)}]");
            }
            finally
            {
                File.Delete("test.json");
            }
        }

        [Test, Performance]
        [TestCase(100)]
        [TestCase(1000)]
        public void DeserializeFromFile_UsingJsonUtility_MockEntityData(int count)
        {
            File.WriteAllText("test.json", JsonTestData.GetMockEntities(count));

            try
            {
                Measure.Method(() => { UnityEngine.JsonUtility.FromJson<MockEntityBatch>(File.ReadAllText("test.json")); })
                       .WarmupCount(1)
                       .MeasurementCount(100)
                       .Run();

                PerformanceTest.Active.CalculateStatisticalValues();

                var size = new FileInfo("test.json").Length / (double) 1024 / 1024;
                Debug.Log($"MB/s=[{size / (PerformanceTest.Active.SampleGroups.First().Median / 1000)}]");
            }
            finally
            {
                File.Delete("test.json");
            }
        }

        [Test, Performance]
        [TestCase(100, 10)]
        [TestCase(1000, 100)]
        [TestCase(10000, 500)]
        public unsafe void DeserializeFromFile_UsingSerializedObjectReaderWithNoActualization_MockEntityData(int count, int batchSize)
        {
            File.WriteAllText("test.json", JsonTestData.GetMockEntities(count));

            try
            {
                Measure.Method(() =>
                       {
                           var views = stackalloc SerializedValueView[batchSize];
                           var config = SerializedObjectReaderConfiguration.Default;

                           config.UseReadAsync = false;
                           config.ValidationType = JsonValidationType.None;
                           config.NodeBufferSize = batchSize;
                           config.BlockBufferSize = 512 << 10;
                           config.OutputBufferSize = 4096 << 10;

                           using (var stream = new FileStream("test.json", FileMode.Open, FileAccess.Read, FileShare.Read, config.BlockBufferSize))
                           using (var reader = new SerializedObjectReader(stream, config))
                           {
                               reader.Step(NodeType.BeginArray);

                               while (reader.ReadArrayElementBatch(views, batchSize) != 0)
                               {
                                   reader.DiscardCompleted();
                               }

                               reader.Step();
                           }
                       })
                       .WarmupCount(1)
                       .MeasurementCount(100)
                       .Run();

                PerformanceTest.Active.CalculateStatisticalValues();

                var size = new FileInfo("test.json").Length / (double) 1024 / 1024;
                Debug.Log($"MB/s=[{size / (PerformanceTest.Active.SampleGroups.First().Median / 1000)}]");
            }
            finally
            {
                File.Delete("test.json");
            }
        }
    }
}