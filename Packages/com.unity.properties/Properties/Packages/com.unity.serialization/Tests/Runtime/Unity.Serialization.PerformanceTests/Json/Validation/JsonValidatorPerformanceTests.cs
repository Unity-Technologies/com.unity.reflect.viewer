using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Unity.Serialization.Json.PerformanceTests
{
    [TestFixture]
    [Category("Performance")]
    class JsonValidatorPerformanceTests
    {
        [Test, Performance]
        [TestCase(100)]
        [TestCase(1000)]
        public unsafe void PerformanceTest_JsonValidatorStandard_Validate_MockEntities(int count)
        {
            var json = JsonTestData.GetMockEntities(count);

            Measure.Method(() =>
                   {
                       fixed (char* ptr = json)
                       {
                           using (var validator = new JsonValidator(JsonValidationType.Standard, Allocator.TempJob))
                           {
                               validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
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
        
        [Test, Performance]
        [TestCase(100)]
        [TestCase(1000)]
        public unsafe void PerformanceTest_JsonValidatorSimple_Validate_MockEntities(int count)
        {
            var json = JsonTestData.GetMockEntities(count);

            Measure.Method(() =>
                {
                    fixed (char* ptr = json)
                    {
                        using (var validator = new JsonValidator(JsonValidationType.Simple, Allocator.TempJob))
                        {
                            validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
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
