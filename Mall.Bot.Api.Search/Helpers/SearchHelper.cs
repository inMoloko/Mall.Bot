using Mall.Bot.Common;
using Mall.Bot.Common.DBHelpers.Models;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Bot.Api.Search.Helpers
{
    public class SearchHelper
    {
        public List<FuzzySearchResult> GetCustomer(List<string> customerNames, List<BotCustomer> customers)
        {
            var qa = new QueryAnaliser();
            var fuzzySearchDataofCustomers = customers.Select(x => new FuzzySearchModel
            {
                FuzzySearchModelID = x.BotCustomerID,
                Name = x.Name,
                KeyWords = x.KeyWords +", "+ x.City
            }).ToList();
            var results = new List<FuzzySearchResult>();

            foreach (var item in customerNames)
            {
                var reader = new MindReader(fuzzySearchDataofCustomers, 0.5);

                var result = reader.BotMainSearch(item);

                if (result.Count != 0)
                {
                    result = result.DistinctBy(c => c.ID).ToList();
                    result.Sort();
                    result = qa.DeleteBadREsults(result);
                }
                else
                {
                    result = reader.BotSecondSearch(item);
                    if (result.Count != 0)
                    {
                        result = result.DistinctBy(c => c.ID).ToList();
                        result.Sort();
                        result = qa.DeleteBadREsults(result);
                    }
                }
                results.AddRange(result);
            }
            results.Sort();
            return results;
        }
    }
}