using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity.Properties.UI.Internal
{
    /// <summary>
    /// Lightweight implementation of the QuickSearch.QueryEngine. This is the fallback if the QuickSearch package is not installed.
    /// </summary>
    class PropertiesSearchBackend<TData> : SearchBackend<TData>
    {
        class SearchQuery : ISearchQuery<TData>
        {
            // ReSharper disable once StaticMemberInGenericType
            static readonly StringBuilder s_StringBuilder = new StringBuilder();
            static readonly string[] s_SupportedOperators = FilterOperator.GetSupportedOperators<TData>();

            readonly Func<TData, IEnumerable<string>> m_GetSearchDataFunc;
            readonly string[] m_Tokens;

            public string SearchString { get; }

            public ICollection<string> Tokens => m_Tokens;

            public SearchQuery(string searchString, Func<TData, IEnumerable<string>> getSearchDataFunc)
            {
                SearchString = searchString;
                m_Tokens = string.IsNullOrWhiteSpace(SearchString) ? Array.Empty<string>() : SplitTokens(SearchString).ToArray();
                m_GetSearchDataFunc = getSearchDataFunc;
            }

            static IEnumerable<string> SplitTokens(string input)
            {
                s_StringBuilder.Clear();
                
                var quoted = false;
                var prev = '\0';
                
                foreach (var c in input)
                {
                    // An unquoted space is treated as a filter separator
                    if (!quoted && c == ' ')
                    {
                        if (s_StringBuilder.Length > 0)
                        {
                            yield return s_StringBuilder.ToString();
                            s_StringBuilder.Clear();
                        }
                        
                        continue;
                    }

                    s_StringBuilder.Append(c);

                    // A quoted string should be treated as part of a filter, including spaces.
                    if (c == '"' && prev != '\\')
                    {
                        quoted = !quoted;

                        // When closing a quoted string this immediately terminates the filter and is treated as a separator.
                        if (!quoted)
                        {
                            yield return s_StringBuilder.ToString();
                            s_StringBuilder.Clear();
                        }
                    }
                    
                    prev = c;
                }

                if (s_StringBuilder.Length > 0)
                {
                    yield return s_StringBuilder.ToString();
                }
            }
            
            public IEnumerable<TData> Apply(IEnumerable<TData> data)
            {
                var tokens = FilteredSupportedTokens().ToArray();
                return tokens.Length == 0
                    ? data
                    : data.Where(d => m_GetSearchDataFunc(d).Any(s => tokens.Any(t => s.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)));
            }

            IEnumerable<string> FilteredSupportedTokens()
            {
                foreach (var token in m_Tokens)
                {
                    var isTokenSupported = true;
                    foreach (var supportedOperator in s_SupportedOperators)
                    {
                        if (token.IndexOf(supportedOperator, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;

                        isTokenSupported = false;
                        break;
                    }

                    if (isTokenSupported)
                        yield return token;
                }
            }
        }
        
        public override ISearchQuery<TData> Parse(string text)
        {
            return new SearchQuery(text, GetSearchData);
        }

        public override void AddSearchFilterProperty(string token, PropertyPath path, SearchFilterOptions options)
        {
            
        }

        public override void AddSearchFilterCallback<TFilter>(string token, Func<TData, TFilter> getFilterDataFunc, SearchFilterOptions options)
        {
        }
    }

    /// <summary>
    /// Helper class to get and apply filters without using the com.unity.quicksearch package.
    /// </summary>
    static class FilterOperator
    {
        /// <summary>
        /// Gets the available operator types for the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get filters for.</typeparam>
        /// <returns>An array of operator tokens.</returns>
        public static string[] GetSupportedOperators<T>()
        {
            var operators = new List<string>
            {
                ":"
            };

            var equatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
            var comparable = typeof(IComparable<T>).IsAssignableFrom(typeof(T));

            if (equatable)
            {
                operators.Add("!=");
                operators.Add("=");
            }

            if (comparable)
            {
                operators.Add(">");
                operators.Add(">=");
                operators.Add("<");
                operators.Add("<=");
            }

            return operators.ToArray();
        }

        public static bool ApplyOperator<T>(string token, T value, T input, StringComparison sc)
        {
            switch (token)
            {
                case ":":
                    return value?.ToString().IndexOf(input?.ToString() ?? string.Empty, sc) >= 0;
                case "=":
                    return (value as IEquatable<T>).Equals(input);
                case "!=":
                    return !(value as IEquatable<T>).Equals(input);
                case ">":
                    return (value as IComparable<T>).CompareTo(input) > 0;
                case ">=":
                    return (value as IComparable<T>).CompareTo(input) >= 0;
                case "<":
                    return (value as IComparable<T>).CompareTo(input) < 0;
                case "<=":
                    return (value as IComparable<T>).CompareTo(input) <= 0;
            }

            return false;
        }
    }
}