using Mall.Bot.Common.Helpers.Models;
using Mall.Bot.Search.Models;
using MoreLinq;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Linq;

namespace Mall.Bot.Search.Mall
{
    public static class Logging
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
    }
    public enum FuzzySearchResultDataType
    {
        Customer = 1,
        Organization,
        Category,
        Service,
        Section,
        Office
    }

    public class SearchHelper
    {
        public List<FuzzySearchResult> SearchCustomerByGeocode(DbGeography location, List<DBasesCustomersHolder> customers)
        {
            var res = new List<FuzzySearchResult>();

            foreach (var db in customers)
            {
                foreach (var item in db.Customers)
                {
                    var dist = location.Distance(item.Location);
                    if (dist < 500)
                    {
                        res.Add(new FuzzySearchResult(item.Name, item.CustomerID, db.DBaseID + item.CustomerID.ToString(), item.LocaleCity, (double)dist, 0.5F, FuzzySearchResultDataType.Customer, ""));
                    }
                }
            }
            return res;
        }
        public List<FuzzySearchResult> SearchCustomerByName(string query, List<DBasesCustomersHolder> customers, string metadata = null)
        {
            var fuzzySearchDataofCustomers = new List<FuzzySearchResult>();
            char p = 'A';
            foreach (var item in customers)
            {
                fuzzySearchDataofCustomers.AddRange(item.Customers.Select(x => 
                new FuzzySearchResult(x.Name, x.CustomerID, p + x.CustomerID.ToString(),x.LocaleCity, 1, null,FuzzySearchResultDataType.Customer,x.City)).ToList());
                p++;
            }
            if (!string.IsNullOrWhiteSpace(metadata))
            {
                var temp = new List<FuzzySearchResult>();
                metadata = metadata.Remove(metadata.Length - 1);
                var mallIDs = metadata.Split(';');

                foreach (var item in mallIDs)
                {
                    temp.Add(fuzzySearchDataofCustomers.FirstOrDefault(x => x.CustomersKey == item));
                }
                for (int i = 0; i < temp.Count; i++)
                {
                    temp[i].KeyWords += ", " + (i + 1).ToString();
                    if (temp[i].Name.Contains("МЕГА")) temp[i].KeyWords += ", мега";
                }
                fuzzySearchDataofCustomers = temp;
            }
            else
            {
                for (int i = 0; i < fuzzySearchDataofCustomers.Count; i++)
                {
                    fuzzySearchDataofCustomers[i].KeyWords += ", " + (i + 1).ToString();
                    if (fuzzySearchDataofCustomers[i].Name.Contains("МЕГА")) fuzzySearchDataofCustomers[i].KeyWords += ", мега";
                }
            }
            
            return Search(query, fuzzySearchDataofCustomers);
        }

        /// <summary>
        /// Удаляет лишние результаты
        /// </summary>
        /// <param name="queryResults"></param>
        /// <returns></returns>
        private List<FuzzySearchResult> DeleteBadResults(List<FuzzySearchResult> queryResults)
        {
            List<FuzzySearchResult> res = new List<FuzzySearchResult>();
            res.Add(queryResults[0]);
            for (int i = 0; i < queryResults.Count - 1; i++)
            {
                if (Math.Abs(queryResults[i].Distinction - queryResults[i + 1].Distinction) == 0) // надо настроить
                    res.Add(queryResults[i + 1]);
                else break;
            }
            return res;
        }
        private List<FuzzySearchResult> FindOrganizationsByCategory(IEnumerable<FuzzySearchResult> findedcategories, IEnumerable<IOrganization> orgs, IEnumerable<ICategory> cats)
        {
            var resOrgs = new List<IOrganization>();
            foreach (var item in findedcategories)
            {
                var category = cats.FirstOrDefault(x => x.CategoryID == item.ID);
                var childcategories = cats.Where(x => x.ParentID == category.CategoryID).ToList();

                resOrgs.AddRange(orgs.Where(x => x.CategoryOrganization.Select(z => z.CategoryID).Contains(category.CategoryID)).ToList());

                if (childcategories.Count > 0)
                {
                    foreach (var child  in childcategories)
                    {
                        resOrgs.AddRange(orgs.Where(x => x.CategoryOrganization.Select(z => z.CategoryID).Contains(child.CategoryID)).ToList());
                    }
                }
            }

            var result = resOrgs.Select(x => new FuzzySearchResult(x.Name, x.OrganizationID, 2, x.Rating, FuzzySearchResultDataType.Organization)).ToList();
            return result;
        }

        private List<FuzzySearchResult> Search(string query, List<FuzzySearchResult> fuzzySearchData, double accuracy = 0.5, bool IsCustomer = false)
        {
            var reader = new MindReader(fuzzySearchData, accuracy);
            // нечеткий поиск
            fuzzySearchData = reader.BotMainSearch(query);
            fuzzySearchData.Sort();
            if (IsCustomer) fuzzySearchData = fuzzySearchData.DistinctBy(x => x.CustomersKey).ToList();
            else fuzzySearchData = fuzzySearchData.DistinctBy(x => x.ID).ToList();

            if (fuzzySearchData.Count != 0) fuzzySearchData = DeleteBadResults(fuzzySearchData);
            else
            {
                // поиск по подстроке
                fuzzySearchData = reader.BotSecondSearch(query);
                fuzzySearchData.Sort();
                if (IsCustomer) fuzzySearchData = fuzzySearchData.DistinctBy(x => x.CustomersKey).ToList();
                else fuzzySearchData = fuzzySearchData.DistinctBy(x => x.ID).ToList();
                if (fuzzySearchData.Count != 0) fuzzySearchData = DeleteBadResults(fuzzySearchData);
            }
            return fuzzySearchData;
        }


        public List<FuzzySearchResult> SearchOrganization(string query, IEnumerable<IOrganization> orgs, IEnumerable<ICategory> cats, IEnumerable<IOrganizationSynonym> synonyms,  string metadata = null)
        {
            try
            {
                List<FuzzySearchResult> result = new List<FuzzySearchResult>(); // тут будут храниться все найденные организации либо категории. с сохранением типа данных
                List<FuzzySearchResult> fuzzySearchData; // тут будут данные для поиска
                
                if (string.IsNullOrWhiteSpace(metadata))
                {
                    // формируем входные данные
                    fuzzySearchData = cats.Select(x => new FuzzySearchResult(x.Name, x.CategoryID, 0, 0, FuzzySearchResultDataType.Category)).ToList();
                    // ищем сатегории
                    result = Search(query, fuzzySearchData);
                    // формируем организации
                    fuzzySearchData = orgs.Select(x => new FuzzySearchResult(x.Name, x.OrganizationID, 1, x.Rating, FuzzySearchResultDataType.Organization, x.KeyWords)).ToList();
                }
                else //организации уже были найдены. Т.е. в кэше есть о них информация
                {   // тогда поиск проводим исключительно по ним
                    metadata = metadata.Remove(metadata.Length - 1);
                    var orgIDs = metadata.Split(';');
                    // формируем входные данные (выборочно)
                    fuzzySearchData = orgs.Where(x => orgIDs.Contains(x.OrganizationID.ToString())).Select(x => new FuzzySearchResult(x.Name, x.OrganizationID, 1, x.Rating, FuzzySearchResultDataType.Organization, x.KeyWords)).ToList();

                    for (int i = 0; i < fuzzySearchData.Count; i++)
                    {
                        fuzzySearchData[i].KeyWords += ", " + (i + 1).ToString();
                    }
                }
                // добавляем к ключевым словам синонимы
                GetSynonyms(fuzzySearchData, synonyms);
                // ищем организации
                result.AddRange( Search(query, fuzzySearchData));
                                
                // логика такая: 
                // если категорий нет - выводим найденные организации
                // если в найденных результатах только категории, то выводим орг-ции из этих категорий
                // если там есть организации, то выводим эти организации (без использования найденных категорий)
                // если там есть организации, но они все Extra, то выводим все по найденным категориям, организации не трогаем.
                if (result.Where(x => x.DataType == FuzzySearchResultDataType.Organization).Count() != 0)
                {
                    if (IsExtras(result.Where(x => x.DataType == FuzzySearchResultDataType.Organization).ToList(), orgs) && result.Where(x => x.DataType == FuzzySearchResultDataType.Category).Count() != 0)
                        return FindOrganizationsByCategory(result.Where(x => x.DataType == FuzzySearchResultDataType.Category).ToList(), orgs, cats);

                    return result.Where(x => x.DataType == FuzzySearchResultDataType.Organization).ToList();
                }

                result = FindOrganizationsByCategory(result, orgs, cats);
                return result;
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
                return null;
            }
        }

        /// <summary>
        /// Интегрирование словаря синонимов с организациями
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="synonyms"></param>
        public void GetSynonyms(IEnumerable<FuzzySearchResult> collection, IEnumerable<IOrganizationSynonym> synonyms)
        {
            foreach (var item in collection)
            {
                if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    IOrganizationSynonym s = synonyms.FirstOrDefault(x => x.OrganizationName.ToLower().Replace(" ", "") == item.Name.ToLower().Replace(" ", ""));
                    if (s != null) item.KeyWords += ", " + s.Synonyms;
                }
            }
        }

        // true - если все Extra, иначе false
        private bool IsExtras(List<FuzzySearchResult> result, IEnumerable<IOrganization> organizations)
        {
            foreach (var item in result)
            {
                //if (organizations.FirstOrDefault(x => x.OrganizationID == item.ID).OrganizationType != OrganizationType.Extra)
                if (organizations.FirstOrDefault(x => x.OrganizationID == item.ID).CategoryOrganization.Any(i=>i.Category.StringID == "extra"))
                    return false;
            }
            return true;
        }
    }
}
