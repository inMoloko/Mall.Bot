using FuzzyString;
using Mall.Bot.Common.MFCHelpers.Models;
using Mall.Bot.Search;
using Mall.Bot.Search.Mall;
using Mall.Bot.Search.Models;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;

namespace Mall.Bot.Common.MFCHelpers
{
    /// <summary>
    /// Сравнильщик. Нужен для пересечения множеств FSR
    /// </summary>
    class FuzzySearchResultComparer : IEqualityComparer<FuzzySearchResult>
    {
        public bool Equals(FuzzySearchResult x, FuzzySearchResult y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.ID == y.ID;
        }

        public int GetHashCode(FuzzySearchResult result)
        {
            if (Object.ReferenceEquals(result, null)) return 0;
            //получаем хэш код для имени
            int hashFSRName = result.Name == null ? 0 : result.Name.GetHashCode();
            //получаем хэш код для ID
            int hashFSRID = result.ID.GetHashCode();
            //хэш код - сложение по модулю 2 для верхних двух кодов
            return hashFSRName ^ hashFSRID;
        }
    }

    public class SearchHelper
    {
        private MFCBotModel mfcDataOfBot;

        public SearchHelper(MFCBotModel _mfcDataOfBot)
        {
            mfcDataOfBot = _mfcDataOfBot;
        }
        /// <summary>
        /// поиск филиалов по географическим координатам
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<FuzzySearchResult> SearchOfficeByGeocode(DbGeography location)
        {
            var result = new List<FuzzySearchResult>();
            foreach (var item in mfcDataOfBot.Offices.Where(x => x.Geo != null))
            {
                var dist = location.Distance(item.Geo);
                if (dist < 500)
                {
                    result.Add(new FuzzySearchResult(item.DisplayName, item.AisMFCID, (double)dist, 0.5F, FuzzySearchResultDataType.Customer));
                }
            }
            return result;
        }


        /// <summary>
        /// обертка для филиалов
        /// </summary>
        /// <param name="query"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public List<FuzzySearchResult> SearchOfficeByName(string query, string metadata = null)
        {
            var fuzzySearchData = mfcDataOfBot.Offices.Select(x => new FuzzySearchResult(x.DisplayName, x.AisMFCID, 1, 0, FuzzySearchResultDataType.Office, x.DisplayAddress.Replace(',', ' '))).ToList();
            return Search(fuzzySearchData, query, metadata);
        }
        /// <summary>
        /// обертка для услуг
        /// </summary>
        /// <param name="query"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        //public List<FuzzySearchResult> SearchServiceByName(string query, string metadata = null)
        //{
        //    var fuzzySearchData = mfcDataOfBot.Services.Select(x => new FuzzySearchResult(x.DisplayName, x.AisMFCID, 1, 0, FuzzySearchResultDataType.Service)).ToList();
        //    return Search(fuzzySearchData, query, metadata);
        //}
        /// <summary>
        /// обертка для секций (кнопок, услуг?, категорий услуг?? я хз как это назвать. пусть будут секции)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public List<FuzzySearchResult> SearchSectionByName(string query, string metadata = null)
        {
            var fuzzySearchData = mfcDataOfBot.Sections.Select(x => new FuzzySearchResult(x.Name, x.SectionID, 1, 0, FuzzySearchResultDataType.Section)).ToList();
            return Search(fuzzySearchData, query, metadata);
        }

        /// <summary>
        /// Поиск в МФЦ. 
        /// Ищем по алгоритму левенштейна, если нет, то по подстроке (надо доработать его), если нет, то модифицированный поиск по подстроке (для офисов поиск ведется и по адресу)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="query"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public List<FuzzySearchResult> Search(List<FuzzySearchResult> data, string query, string metadata = null)
        {
            // в метадате хранятся ID найденных объектов на предыдущих этапах. 
            if (metadata != null)
            {
                var IDS = metadata.Split(';');
                var temp = new List<FuzzySearchResult>();

                for (int i = 0; i < IDS.Length-1; i++)
                {
                    temp.Add(data.FirstOrDefault(x => x.ID == int.Parse(IDS[i])));
                    temp[i].KeyWords = $"{i + 1}";
                }
                // не пустая метадата говорит о том, что поиск будет проходить по ней
                data = temp;
            }

            var reader = new MindReader(data, 0.5);
            //основной поиск (левенштей)
            var result = reader.BotMainSearch(query);

            if (result.Count != 0)
            {
                result = result.DistinctBy(c => c.ID).ToList(); // убираем повторяющиеся 
                result.Sort();// сортируем по distinction
                result = DeleteBadResults(result); // исключаем плохо найденные результаты
            }
            else
            {
                //поиск по подстроке (нужно доработать)
                result = reader.BotSecondSearch(query);
                if (result.Count != 0)
                {
                    result = result.DistinctBy(c => c.ID).ToList(); // убираем повторяющиеся 
                    result.Sort();// сортируем по distinction
                    result = DeleteBadResults(result); // исключаем плохо найденные результаты
                }
            }

            if (result.Count == 0)
            {
                //если ничего не найдено, то проводим мод. поиск по подстроке
                if(data[0]?.DataType == FuzzySearchResultDataType.Office) result = GetOfficesContainsQuerry(query);
                else result = GetResultsContainsQuerry(data, query);
            }

            return result.ToList();
        }
        /// <summary>
        /// Модифицированный поиск по подстроке.
        /// 1. Разбиваем запрос на слова
        /// 2. По каждому слову находим множестно объектов в которых Longest Common Substring > чем длина слова, разделенная на 1.3 (таким образом задается погрешность при поиске)
        /// 3. Пересекаем множества найденных объектов
        /// </summary>
        /// <param name="data"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private List<FuzzySearchResult> GetResultsContainsQuerry(List<FuzzySearchResult> data, string query)
        {
            var result = new HashSet<FuzzySearchResult>();
            foreach (var item in query.Split(' '))
            {
                if (item.Length >= 5)
                {
                    var options = new List<FuzzyStringComparisonOptions>();
                    options.Add(FuzzyStringComparisonOptions.UseLongestCommonSubstring);

                    var findeddata = data.Where(x => x.Name.ToLower().LongestCommonSubstring(item).Length > item.Length / 1.3F).ToHashSet();
                    if (findeddata.Count != 0 && result.Count == 0) result = result.Union(findeddata, new FuzzySearchResultComparer()).ToHashSet();
                    else result = result.Intersect(findeddata, new FuzzySearchResultComparer()).ToHashSet();
                }
            }
            return result.ToList();
        }
        /// <summary>
        /// аналогично предыдущему методу, но без разбивания на слова + тут поиск ведется и по адресу филиала
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private List<FuzzySearchResult> GetOfficesContainsQuerry(string query)
        {
            var result = new List<FuzzySearchResult>();
            var options = new List<FuzzyStringComparisonOptions>();
            options.Add(FuzzyStringComparisonOptions.UseLongestCommonSubstring);

            foreach (var item in query.Split(' '))
            {
                if (item.Length >= 4)
                {
                    var temp = mfcDataOfBot.Offices.Where(x => x.DisplayName.ToLower().LongestCommonSubstring(item).Length > item.Length / 1.2F).ToList();
                    if (temp.Count == 0) temp = mfcDataOfBot.Offices.Where(x => x.DisplayAddress.ToLower().LongestCommonSubstring(item).Length > item.Length / 1.2F).ToList();
                    result.AddRange( temp.Select(x => new FuzzySearchResult(x.DisplayName, x.AisMFCID, 1, 0, FuzzySearchResultDataType.Customer)).ToList());
                }
            }
            return result.DistinctBy(x => x.ID).ToList();
        }
        /// <summary>
        /// Исключает плохо найденные результаты
        /// </summary>
        /// <param name="queryResults"></param>
        /// <returns></returns>
        public List<FuzzySearchResult> DeleteBadResults(List<FuzzySearchResult> queryResults)
        {
            List<FuzzySearchResult> res = new List<FuzzySearchResult>();
            res.Add(queryResults[0]);
            for (int i = 0; i < queryResults.Count - 1; i++)
            {
                if (Math.Abs(queryResults[i].Distinction - queryResults[i + 1].Distinction) < 0.05)
                    res.Add(queryResults[i + 1]);
                else break;
            }
            return res;
        }
    }
}
