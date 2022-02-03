using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Unity.Properties.UI.PerformanceTests
{
    [TestFixture, UI]
    sealed class SearchElementPerformanceTests
    {
        class TestData
        {
            public struct NestedStruct
            {
                public string Value;
            }
            
            [CreateProperty] public int Id { get; set; }
            [CreateProperty] public string Name { get; set; }
            [CreateProperty] public Vector2 Position { get; set; }
            [CreateProperty] public bool Active { get; set; }
            [CreateProperty] public NestedStruct Nested { get; set; }
            [CreateProperty] public string[] StringArray { get; set; }
            [CreateProperty] public IEnumerable<string> StringEnumerable { get; set; }
        }
        
        static TestData[] Generate(int size)
        {
            var data = new TestData[size];
            
            for (var i = 0; i < size; ++i)
            {
                var posX = i * 10;
                var posY = i * -25;

                string name;

                switch (i % 4)
                {
                    case 0:
                        name = $"Material {i}";
                        break;
                    case 1:
                        name = $"Mesh {i}";
                        break;
                    case 2:
                        name = $"Camera {i}";
                        break;
                    default:
                        name = $"Object {i}";
                        break;
                }

                data[i] = new TestData {Id = i, Name = name, Position = new Vector2(posX, posY), Active = i % 2 == 0, Nested = new TestData.NestedStruct { Value = $"nested{i}"}};
            }

            return data;
        }

        [Test, Performance]
        public void SearchElementPerformance_WithNoSearchData()
        {
            var searchElement = new SearchElement {SearchDelay = 0, GlobalStringComparison = StringComparison.Ordinal};
            
            var originalData = Generate(100000);
            var filteredData = new List<TestData>();
            
            searchElement.RegisterSearchQueryHandler<TestData>(search =>
            {
                foreach (var element in search.Apply(originalData))
                {
                    filteredData.Add(element);
                }
            });

            Measure.Method(() =>
                {
                    searchElement.Search("Mat");
                })
                .WarmupCount(1)
                .MeasurementCount(1)
                .Run();
            
            Assert.That(filteredData.Count, Is.EqualTo(0));
        }
        
        [Test, Performance]
        public void SearchElementPerformance_WithSearchDataProperty()
        {
            var searchElement = new SearchElement {SearchDelay = 0, GlobalStringComparison = StringComparison.Ordinal};
            
            var originalData = Generate(100000);
            var filteredData = new List<TestData>();
            
            searchElement.RegisterSearchQueryHandler<TestData>(search =>
            {
                filteredData.Clear();
                
                foreach (var element in search.Apply(originalData))
                {
                    filteredData.Add(element);
                }
            });
            
            searchElement.AddSearchDataProperty(new PropertyPath(nameof(TestData.Name)));

            Measure.Method(() =>
                {
                    searchElement.Search("Mat");
                })
                .WarmupCount(1)
                .MeasurementCount(1)
                .Run();
            
            Assert.That(filteredData.Count, Is.EqualTo(25000));
        }
    }
}