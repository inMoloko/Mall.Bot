using Mall.Bot.Search.Models;
using System.Collections.Generic;

namespace Mall.Bot.Common.MallHelpers.Models
{
    public class SearchResult
    {
        /// <summary>
        /// запрос
        /// </summary>
        public string QueryText;
        /// <summary>
        /// найденные органищации по запросу
        /// </summary>
        public List<FuzzySearchResult> QueryResults = new List<FuzzySearchResult>();

        public SearchResult(string queryText, List<FuzzySearchResult> queryResults)
        {
            QueryText = queryText;
            QueryResults = queryResults;
        }
    }
}
