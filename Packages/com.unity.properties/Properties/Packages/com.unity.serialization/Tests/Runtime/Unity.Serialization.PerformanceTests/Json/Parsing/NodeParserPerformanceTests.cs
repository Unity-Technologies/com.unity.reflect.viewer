using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Unity.Serialization.Json.PerformanceTests
{
    [TestFixture]
    [Category("Performance")]
    class NodeParserPerformanceTests
    {
        JsonTokenizer m_Tokenizer;

        [SetUp]
        public void SetUp()
        {
            m_Tokenizer = new JsonTokenizer(Allocator.Persistent);
        }

        [TearDown]
        public void TearDown()
        {
            m_Tokenizer.Dispose();
        }

        #if UNITY_2019_2_OR_NEWER
        [Test, Performance]
        #else
        [PerformanceTest]
        #endif
        [TestCase(1000)]
        [TestCase(10000)]
        public unsafe void PerformanceTest_NodeParser_Step_MockEntities(int count)
        {
            var json = JsonTestData.GetMockEntities(count);

            fixed (char* ptr = json)
            {
                m_Tokenizer.Write(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
            }

            Measure.Method(() =>
                   {
                       using (var parser = new NodeParser(m_Tokenizer, Allocator.TempJob))
                       {
                           parser.Step(NodeType.None);
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
