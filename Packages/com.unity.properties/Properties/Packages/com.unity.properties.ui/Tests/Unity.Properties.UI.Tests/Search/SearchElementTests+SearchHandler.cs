using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Properties.UI.Tests
{
    sealed partial class SearchElementTests
    {
        [Test]
        public void SearchHandler_WhenModeIsAsyncWithResults_FilterCallbacksAreInvoked()
        {
            var handler = new SearchHandler<TestData>(m_SearchElement)
            {
                Mode = SearchHandlerType.async,
                SearchDataBatchMaxSize = 1,
                
                // '0' will force the handler to return 'SearchDataBatchMaxSize' elements per frame.
                MaxFrameProcessingTimeMs = 0
            };

            var data = Generate(1000);
            var filtered = new List<TestData>();
            
            m_SearchElement.AddSearchDataProperty(new PropertyPath("Name"));
            
            var onBeginSearchCount = 0;
            var onFilterCount = 0;
            var onEndSearchCount = 0;

            handler.SetSearchDataProvider(() => data);
            handler.OnBeginSearch += query =>
            {
                onBeginSearchCount++;
            }; 
            handler.OnFilter += (query, results) =>
            {
                filtered.AddRange(results);
                onFilterCount++;
            }; 
            handler.OnEndSearch += query =>
            {
                onEndSearchCount++;
            };

            m_SearchElement.value = "Mat";

            // tick one frame
            handler.Update();
            
            Assert.That(onBeginSearchCount, Is.EqualTo(1));
            Assert.That(onFilterCount, Is.EqualTo(1));
            Assert.That(onEndSearchCount, Is.EqualTo(0));
        }

        [Test]
        public void SearchHandler_WhenModeIsAsyncWithNoResults_FilterCallbacksAreInvoked()
        {
            var handler = new SearchHandler<TestData>(m_SearchElement)
            {
                Mode = SearchHandlerType.async,
                SearchDataBatchMaxSize = 1,
                
                // '0' will force the handler to return 'SearchDataBatchMaxSize' elements per frame.
                MaxFrameProcessingTimeMs = 0
            };


            var data = Generate(1000);
            var filtered = new List<TestData>();
            
            m_SearchElement.AddSearchDataProperty(new PropertyPath("Name"));
            
            var onBeginSearchCount = 0;
            var onFilterCount = 0;
            var onEndSearchCount = 0;

            handler.SetSearchDataProvider(() => data);
            handler.OnBeginSearch += query =>
            {
                onBeginSearchCount++;
            }; 
            handler.OnFilter += (query, results) =>
            {
                filtered.AddRange(results);
                onFilterCount++;
            }; 
            handler.OnEndSearch += query =>
            {
                onEndSearchCount++;
            };

            m_SearchElement.value = "ZZ";

            // tick one frame
            handler.Update();
            
            Assert.That(onBeginSearchCount, Is.EqualTo(1));
            Assert.That(onFilterCount, Is.EqualTo(1));
            Assert.That(onEndSearchCount, Is.EqualTo(1));
        }
    }
}