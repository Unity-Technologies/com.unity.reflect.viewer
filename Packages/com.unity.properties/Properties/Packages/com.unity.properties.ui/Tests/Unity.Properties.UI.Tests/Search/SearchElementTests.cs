using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Properties.UI.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    /// <summary>
    /// The search engine backend type to use for tests.
    /// </summary>
    enum SearchBackendType
    {
        /// <summary>
        /// A simple implementation used as a fallback.
        /// </summary>
        Properties,
            
        /// <summary>
        /// The primary search engine. This is only available if the QuickSearch package is installed.
        /// </summary>
        QuickSearch
    }
    
    [TestFixture, UI]
    sealed partial class SearchElementTests
    {
        EditorWindow m_Window;

        SearchElement m_SearchElement;
        PropertyElement m_PropertyElement;

        [OneTimeSetUp]
        public void GlobalSetUp()
        {
            m_Window = EditorWindow.CreateInstance<EditorWindow>();
            m_Window.Show();
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            m_Window.Close();
        }

        [SetUp]
        public void SetUp()
        {
            m_SearchElement = new SearchElement {SearchDelay = 0};
            m_PropertyElement = new PropertyElement();
            m_Window.rootVisualElement.Add(m_SearchElement);
            m_Window.rootVisualElement.Add(m_PropertyElement);
        }

        [TearDown]
        public void Teardown()
        {
            m_Window.rootVisualElement.Clear();
        }

        class DataContainerInspector : PropertyInspector<TestDataContainerWithCustomInspector>
        {
            public override VisualElement Build()
            {
                var root = new VisualElement();
                new Internal.UITemplate("test-data-container").Clone(root);
                return root;
            }
        }

        class TestDataContainerWithCustomInspector
        {
            [CreateProperty] public TestData[] SourceData;
            [CreateProperty] public List<TestData> DestinationData;
        }

        class TestDataContainer
        {
            public static readonly PropertyPath ValidSourceDataPath = new PropertyPath(nameof(ValidSourceData));
            public static readonly PropertyPath ValidDestinationDataPath = new PropertyPath(nameof(ValidDestinationData));
            public static readonly PropertyPath ReadOnlyDestinationDataPath = new PropertyPath(nameof(ReadOnlyDestinationData));
            public static readonly PropertyPath NonCollectionSourceDataPath = new PropertyPath(nameof(NonCollectionSourceData));
            public static readonly PropertyPath NonCollectionDestinationDataPath = new PropertyPath(nameof(NonCollectionDestinationData));
            
#pragma warning disable 649
            [CreateProperty] public TestData[] ValidSourceData;
            [CreateProperty] public List<TestData> ValidDestinationData;
            [CreateProperty] public TestData[] ReadOnlyDestinationData;
            [CreateProperty] public TestData NonCollectionSourceData;
            [CreateProperty] public TestData NonCollectionDestinationData;
#pragma warning restore 649
        }
        
        [Test]
        public void Search_WithNoSearchDataProperties_DoesNotReturnAnyResults()
        {
            var originalData = Generate(100);
            var filteredData = default(TestData[]);

            m_SearchElement.RegisterSearchQueryHandler<TestData>(search => { filteredData = search.Apply(originalData).ToArray(); });
            m_SearchElement.value = "Mesh";
            
            Assert.That(filteredData.Length, Is.EqualTo(0));
        }

        [Test]
        [TestCase("Mesh", "Name", 25)]
        [TestCase("1", "Id",19)]
        [TestCase("nested0", "Nested.Value",1)]
        public void Search_WithSearchDataProperties_ReturnsFilteredResults(string searchString, string searchDataProperties, int expectedCount)
        {
            var originalData = Generate(100);
            var filteredData = default(TestData[]);

            m_SearchElement.RegisterSearchQueryHandler<TestData>(search => { filteredData = search.Apply(originalData).ToArray(); });
            
            foreach (var path in searchDataProperties.Split(' '))
                m_SearchElement.AddSearchDataProperty(new PropertyPath(path));

            m_SearchElement.value = searchString;
            
            Assert.That(filteredData.Length, Is.EqualTo(expectedCount));
        }

        [Test]
        [TestCase("id<10", "id:Id", 10)]
        [TestCase("x<50", "x:Position.x", 5)]
        [TestRequires_QUICKSEARCH_2_1_0_OR_NEWER("Filtering is only supported using the com.unity.quicksearch package.")]
        public void Search_WithSearchFilterProperties_ReturnsFilteredResults(string searchString, string searchFilterProperties, int expectedCount)
        {
            var originalData = Generate(100);
            var filteredData = default(TestData[]);

            foreach (var searchFilter in searchFilterProperties.Split(' '))
            {
                var tokenAndPath = searchFilter.Split(':');
                
                var token = tokenAndPath[0];
                var path = tokenAndPath[1];
                
                m_SearchElement.AddSearchFilterProperty(token, new PropertyPath(path));
            }

            m_SearchElement.RegisterSearchQueryHandler<TestData>(search => { filteredData = search.Apply(originalData).ToArray(); });
            m_SearchElement.value = searchString;
            
            Assert.That(filteredData.Length, Is.EqualTo(expectedCount));
        }
        
        [Test]
        public void RegisterHandler_WithInvalidSourceDataPath_Throw()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, new PropertyPath("SomeUnknownPath"), TestDataContainer.ValidDestinationDataPath);
            });
        }

        [Test]
        public void RegisterHandler_WithInvalidDestinationDataPath_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, new PropertyPath("SomeUnknownPath"));
            });
        }
        
        [Test]
        public void RegisterHandler_WithInvalidSourceDataType_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.NonCollectionSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            });
        }
        
        [Test]
        public void RegisterHandler_WithInvalidDestinationDataType_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            });

            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.NonCollectionDestinationDataPath);
            });
        }
        
        [Test]
        public void RegisterHandler_WithReadOnlyDestinationDataType_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer());

            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ReadOnlyDestinationDataPath);
            });
        }
        
        [Test]
        public void RegisterHandler_WithNullSourceData_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = null,
                ValidDestinationData = new List<TestData>()
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            });
        }
        
        [Test]
        public void RegisterHandler_WithNullDestinationData_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = null
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            });
        }

        [Test]
        public void Search_WithValidBindings_ResultsAreWrittenToDestinationData()
        {
            var container = new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            };
            
            m_PropertyElement.SetTarget(container);
            m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            m_SearchElement.AddSearchDataProperty(new PropertyPath("Name"));
            
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(0));

            m_SearchElement.Search("");
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(container.ValidSourceData.Length));

            m_SearchElement.Search("Mesh");
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(25));
        }
        
        [Test]
        public void Search_WithCollectionSearchData_ResultsAreWrittenToDestinationData()
        {
            var container = new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            };
            
            container.ValidSourceData[0].StringArray = new[]
            {
                "one", "two", "three", "four"
            };
            
            container.ValidSourceData[1].StringArray = new[]
            {
                "two", "three", "four"
            };
            
            container.ValidSourceData[2].StringArray = new[]
            {
                "three", "four"
            };
            
            container.ValidSourceData[3].StringArray = new[]
            {
                "four"
            };
            
            m_PropertyElement.SetTarget(container);
            m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            m_SearchElement.AddSearchDataProperty(new PropertyPath("StringArray"));
            
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(0));

            m_SearchElement.Search("two");
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(2));
            
            m_SearchElement.Search("three");
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(3));
        }
        
        [Test]
        public void Search_CustomInspectorWithDataBindings_ReturnsFilteredResults()
        {
            var container = new TestDataContainerWithCustomInspector
            {
                SourceData = Generate(100),
                DestinationData = new List<TestData>()
            };
            
            m_PropertyElement.SetTarget(container);

            var searchElement = m_PropertyElement.Q<SearchElement>("filter");
            var searchHandler = searchElement.GetUxmlSearchHandler();

            /* The UXML declaration of the search element is.
             * 
             * <pui:SearchElement
             *      name="filter"
             *      search-delay="100"
             *      handler-type="sync"
             *      search-data="Id Name"
             *      source-data="SourceData"
             *      filtered-data="DestinationData"
             *      search-filters="a:StringArray"
             *      global-string-comparison="Ordinal"/>
             */
            
            Assert.That(searchElement.SearchDelay, Is.EqualTo(100));
            Assert.That(searchHandler.Mode, Is.EqualTo(SearchHandlerType.sync));
            Assert.That(searchHandler.SearchDataType, Is.EqualTo(typeof(TestData)));
            Assert.That(searchElement.GlobalStringComparison, Is.EqualTo(StringComparison.Ordinal));

            searchElement.Search("Mesh");
            
            Assert.That(container.DestinationData.Count, Is.EqualTo(25));
        }
        
        [Test]
        [TestRequires_QUICKSEARCH_2_1_0_OR_NEWER("Filtering is only supported using the com.unity.quicksearch package.")]
        public void Search_CustomInspectorWithCollectionFilter_ReturnsFilteredResults()
        {
            var container = new TestDataContainerWithCustomInspector
            {
                SourceData = Generate(100),
                DestinationData = new List<TestData>()
            };
            
            container.SourceData[0].StringArray = new[]
            {
                "a", "b"
            };
            
            container.SourceData[1].StringArray = new[]
            {
                "b", "c"
            };
            
            container.SourceData[2].StringArray = new[]
            {
                "c", "d", "e"
            };
            
            m_PropertyElement.SetTarget(container);

            var searchElement = m_PropertyElement.Q<SearchElement>("filter");
            
            searchElement.Search("a:e");
            
            Assert.That(container.DestinationData.Count, Is.EqualTo(1));
            
            searchElement.Search("a:b");
            
            Assert.That(container.DestinationData.Count, Is.EqualTo(2));
        }
        
        [Test]
        [TestRequires_QUICKSEARCH_2_1_0_OR_NEWER("Filtering is only supported using the com.unity.quicksearch package.")]
        public void Search_CustomInspectorWithCollectionFilter_WhenCollectionIsIEnumerable_ReturnsFilteredResults()
        {
            var container = new TestDataContainerWithCustomInspector
            {
                SourceData = Generate(100),
                DestinationData = new List<TestData>()
            };
            
            container.SourceData[0].StringEnumerable = new[]
            {
                "a", "b"
            };
            
            container.SourceData[1].StringEnumerable = new[]
            {
                "b", "c"
            };
            
            container.SourceData[2].StringEnumerable = new[]
            {
                "c", "d", "e"
            };
            
            m_PropertyElement.SetTarget(container);

            var searchElement = m_PropertyElement.Q<SearchElement>("filter");
            
            searchElement.Search("e:e");
            
            Assert.That(container.DestinationData.Count, Is.EqualTo(1));
            
            searchElement.Search("e:b");
            
            Assert.That(container.DestinationData.Count, Is.EqualTo(2));
        }

        [Test]
        [TestRequires_QUICKSEARCH_2_1_0_OR_NEWER("Filtering is only supported using the com.unity.quicksearch package.")]
        [TestCase(SearchBackendType.Properties)]
        [TestCase(SearchBackendType.QuickSearch)]
        public void Search_TokensShouldBeEmptyOnEmptySearchString(SearchBackendType backendType)
        {
            var searchElement = new SearchElement();
            searchElement.RegisterSearchBackend(CreateSearchBackend<TestData>(backendType));

            searchElement.RegisterSearchQueryHandler<TestData>(q =>
            {
                Assert.DoesNotThrow(() =>
                {
                    var i = q.Tokens.Count;
                });
            });
            searchElement.AddSearchDataProperty(new PropertyPath(nameof(TestData.Name)));

            searchElement.Search(string.Empty);
            searchElement.Search(null);
            searchElement.Search("   ");
        }

        static IEnumerable<string> Search_PropertiesBackendSkipFilterTokens_SourceData()
        {
            var supportedTokens = FilterOperator.GetSupportedOperators<EquatableAndComparableTestData>();
            foreach (var supportedToken in supportedTokens)
            {
                yield return $"c{supportedToken}42";
                yield return $"c{supportedToken}42  ";
                yield return $"c{supportedToken}42 H";
                yield return $"c{supportedToken}42 h";
            }
            yield return "h";
            yield return "H";
            yield return "";
            yield return "  ";
        }

        [Test]
        [TestCaseSource(nameof(Search_PropertiesBackendSkipFilterTokens_SourceData))]
        public void Search_PropertiesBackendSkipFilterTokens(string input)
        {
            var searchElement = new SearchElement();

            searchElement.RegisterSearchBackend(CreateSearchBackend<EquatableAndComparableTestData>(SearchBackendType.Properties));
            
            var sourceData = new[]
            {
                new EquatableAndComparableTestData { Name = "hello" },
                new EquatableAndComparableTestData { Name = "hola" },
            };
            EquatableAndComparableTestData[] filtered = null;

            searchElement.RegisterSearchQueryHandler<EquatableAndComparableTestData>(q => filtered = q.Apply(sourceData).ToArray());
            searchElement.AddSearchDataProperty(new PropertyPath(nameof(EquatableAndComparableTestData.Name)));
            searchElement.AddSearchFilterPopupItem("c", "component type");

            searchElement.Search(input);
            Assert.That(filtered, Is.EquivalentTo(sourceData));
        }
        
        [Test]
        public void Search_BackendTokensParserHandlesDoubleQuotes([Values(SearchBackendType.Properties, SearchBackendType.QuickSearch)] SearchBackendType type)
        {
            SearchBackend<string> backend = null;
            
            switch (type)
            {
                case SearchBackendType.Properties:
                    backend = new PropertiesSearchBackend<string>();
                    break;
                case SearchBackendType.QuickSearch:
#if QUICKSEARCH_2_1_0_OR_NEWER
                    backend = new QuickSearchBackend<string>();
#else
                    Assert.Pass("QuickSearchBackend needs to be available for this test");
#endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            var query = backend.Parse("filter:\"Hello World\" \"Hello\" World filter:\"  Hello filter:\"World");
            Assert.That(query.Tokens, Is.EquivalentTo(new []
            {
                "filter:\"Hello World\"",
                "\"Hello\"",
                "World",
                "filter:\"  Hello filter:\"",
                "World"
            }));
        }
        
        static ISearchBackend<TData> CreateSearchBackend<TData>(SearchBackendType type)
        {
            switch (type)
            {
                case SearchBackendType.Properties:
                    return new PropertiesSearchBackend<TData>();
                case SearchBackendType.QuickSearch:
#if QUICKSEARCH_2_1_0_OR_NEWER
                    return new QuickSearchBackend<TData>();
#endif
                    throw new InvalidOperationException("QuickSearch needs to be installed to tests this backend");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

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

        class EquatableAndComparableTestData : IEquatable<EquatableAndComparableTestData>, IComparable<EquatableAndComparableTestData>
        {
            public string Name;

            public bool Equals(EquatableAndComparableTestData other)
                => throw new NotImplementedException();

            public int CompareTo(EquatableAndComparableTestData other)
                => throw new NotImplementedException();
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
    }
}