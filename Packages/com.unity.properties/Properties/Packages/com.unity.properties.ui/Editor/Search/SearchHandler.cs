using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Available search handler modes.
    /// </summary>
    public enum SearchHandlerType
    {
        /// <summary>
        /// The search handler will perform the filtering synchronously.
        /// </summary>
        sync,
        
        /// <summary>
        /// The search handler will perform the filtering asynchronously.
        /// </summary>
        async
    }

    /// <summary>
    /// Interface used to reference an untyped search handler.
    /// </summary>
    public interface ISearchHandler
    {
        /// <summary>
        /// Gets the strongly typed search data type.
        /// </summary>
        Type SearchDataType { get; }

        /// <summary>
        /// The active search mode to use. <see cref="SearchHandlerType"/>.
        /// </summary>
        SearchHandlerType Mode { get; set; }
        
        /// <summary>
        /// The maximum number of elements to process in each enumerator batch. 
        /// </summary>
        int SearchDataBatchMaxSize { get; set; }
        
        /// <summary>
        /// The maximum amount of time in milliseconds to spend on filtering each frame.
        /// </summary>
        int MaxFrameProcessingTimeMs { get; set; }
    }
    
    /// <summary>
    /// The built in search handler. 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public sealed class SearchHandler<TData> : ISearchHandler, ISearchQueryHandler<TData> 
    {
        readonly SearchElement m_SearchElement;
        readonly Stopwatch m_Stopwatch = new Stopwatch();
        readonly List<TData> m_FrameBatch = new List<TData>();
        Func<IEnumerable<TData>> m_GetSearchDataFunc;
        ISearchQuery<TData> m_SearchQuery;
        int m_CurrentSearchDataIndex;
        int m_FilterBatchCount;
        IEnumerator<TData> m_Enumerator;
        int m_Count;

        /// <inheritdoc/>
        public Type SearchDataType => typeof(TData);
        
        /// <inheritdoc/>
        public SearchHandlerType Mode { get; set; }

        /// <inheritdoc/>
        public int SearchDataBatchMaxSize { get; set; } = 200;
        
        /// <inheritdoc/>
        public int MaxFrameProcessingTimeMs { get; set; } = 16;

        /// <summary>
        /// Callback invoked when a new search is started.
        /// </summary>
        public event Action<ISearchQuery<TData>> OnBeginSearch = delegate { };
        
        /// <summary>
        /// Callback invoked when receiving a batch of filtered data.
        /// </summary>
        public event Action<ISearchQuery<TData>, IEnumerable<TData>> OnFilter = delegate { };
        
        /// <summary>
        /// Callback invoked when a search is completed.
        /// </summary>
        public event Action<ISearchQuery<TData>> OnEndSearch = delegate { };
        
        /// <summary>
        /// Initializes a new <see cref="SearchHandler{TData}"/> for the specified search element. 
        /// </summary>
        /// <remarks>
        /// The search handler is automatically registered to the given element.
        /// </remarks>
        public SearchHandler(SearchElement element)
        {
            m_SearchElement = element;
            m_SearchElement.RegisterSearchQueryHandler(this);
            element.schedule.Execute(Update).Every(0);
        }
        
        /// <summary>
        /// Sets the callback used to gather the search data.
        /// </summary>
        /// <param name="getSearchDataFunc">The callback to be invoked when gathering search data.</param>
        public void SetSearchDataProvider(Func<IEnumerable<TData>> getSearchDataFunc)
        {
            Stop();
            m_GetSearchDataFunc = getSearchDataFunc;
        }
        
        /// <summary>
        /// Stops any currently running search.
        /// </summary>
        public void Stop()
        {
            if (null != m_SearchQuery)
                OnEndSearch(m_SearchQuery);
            
            m_SearchQuery = null;
            m_Enumerator = null;
            m_SearchElement.HideProgress();
        }

        void ISearchQueryHandler<TData>.HandleSearchQuery(ISearchQuery<TData> query)
        {
            if (null == m_GetSearchDataFunc)
                return;

            Stop();

            if (Mode == SearchHandlerType.sync || string.IsNullOrEmpty(query.SearchString))
            {
                OnBeginSearch(query);
                OnFilter(query, query.Apply(m_GetSearchDataFunc()));
                OnEndSearch(query);
            }
            else
            {
                m_SearchQuery = query;
                m_CurrentSearchDataIndex = 0;
                m_FilterBatchCount = 0;
                m_Count = m_GetSearchDataFunc().Count();
                OnBeginSearch(query);
            }
        }

        internal void Update()
        {
            if (null == m_SearchQuery || Mode == SearchHandlerType.sync)
                return;

            // The progress bar displays progress from 0 - 90% so we can visually see it reaching the end.
            m_SearchElement.ShowProgress(m_CurrentSearchDataIndex / (m_Count * 0.9f));
            m_Stopwatch.Restart();
            m_FrameBatch.Clear();

            var batchSize = Math.Max(SearchDataBatchMaxSize, 1);

            for (;;)
            {
                if (null == m_Enumerator)
                {
                    var batch = m_GetSearchDataFunc().Skip(m_CurrentSearchDataIndex).Take(batchSize);

                    if (!batch.Any())
                    {
                        // This is an empty batch. We must always call 'OnFilter' at least once regardless of results.
                        OnFilter(m_SearchQuery, m_FrameBatch);
                        Stop();
                        return;
                    }
                
                    m_Enumerator = m_SearchQuery.Apply(batch).GetEnumerator();
                    m_CurrentSearchDataIndex += batchSize;
                }
            
                while (m_Enumerator.MoveNext())
                {
                    m_FrameBatch.Add(m_Enumerator.Current);

                    if (m_Stopwatch.ElapsedMilliseconds >= MaxFrameProcessingTimeMs)
                    {
                        if (m_FrameBatch.Count > 0)
                        {
                            m_FilterBatchCount++;
                            OnFilter(m_SearchQuery, m_FrameBatch);
                        }
                        
                        return;
                    }
                }

                if (m_FilterBatchCount == 0)
                    OnFilter(m_SearchQuery, m_FrameBatch);
                
                Stop();
                return;
            }
        }
    }
}