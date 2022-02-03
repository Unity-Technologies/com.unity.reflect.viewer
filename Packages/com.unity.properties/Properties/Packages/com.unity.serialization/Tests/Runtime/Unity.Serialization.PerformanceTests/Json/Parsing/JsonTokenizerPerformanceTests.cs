using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Unity.Serialization.Json.PerformanceTests
{
    [TestFixture]
    [Category("Performance")]
    class JsonTokenizerPerformanceTests
    {
#if UNITY_2019_2_OR_NEWER
        [Test, Performance]
#else
        [PerformanceTest]
#endif
        [TestCase(100, 1024)]
        [TestCase(1000, 1024)]
        [TestCase(10000, 1024)]
        public unsafe void PerformanceTest_JsonTokenizer_WriteWithNoValidation_MockEntities(int count, int initialTokenBuffer)
        {
            var json = JsonTestData.GetMockEntities(count);

            Measure.Method(() =>
                   {
                       fixed (char* ptr = json)
                       {
                           using (var tokenizer = new JsonTokenizer(initialTokenBuffer, JsonValidationType.None))
                           {
                               tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                           }
                       }
                   })
                   .WarmupCount(1)
                   .MeasurementCount(100)
                   .Run();

            PerformanceTest.Active.CalculateStatisticalValues();

            var size = json.Length / (double) 1024 / 1024;
            Debug.Log($"MB/s=[{size / (PerformanceTest.Active.SampleGroups.First().Median / 1000)}]");
        }

#if UNITY_2019_2_OR_NEWER
        [Test, Performance]
#else
        [PerformanceTest]
#endif
        [TestCase(100, 1024)]
        [TestCase(1000, 1024)]
        [TestCase(10000, 1024)]
        public unsafe void PerformanceTest_JsonTokenizer_WriteWithStandardValidation_MockEntities(int count, int initialTokenBuffer)
        {
            var json = JsonTestData.GetMockEntities(count);

            Measure.Method(() =>
                   {
                       fixed (char* ptr = json)
                       {
                           using (var tokenizer = new JsonTokenizer(initialTokenBuffer, JsonValidationType.Standard))
                           {
                               tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                           }
                       }
                   })
                   .WarmupCount(1)
                   .MeasurementCount(100)
                   .Run();

            PerformanceTest.Active.CalculateStatisticalValues();

            var size = json.Length / (double) 1024 / 1024;
            Debug.Log($"MB/s=[{size / (PerformanceTest.Active.SampleGroups.First().Median / 1000)}]");
        }
    }
}
