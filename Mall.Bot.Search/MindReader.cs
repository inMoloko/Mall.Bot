using System;
using System.Collections.Generic;
using System.Linq;
using FuzzyString;
using NickBuhro.Translit;
using Mall.Bot.Search.Models;
using Mall.Bot.Search.Mall;

namespace Mall.Bot.Search
{
    public enum MyFuzzyStringComparisonOptions
    {
        UseDamerauLevenshtein = 13,
    }
    /// <summary>
    /// Нечеткий поиск
    /// </summary>
    public class MindReader
    {
        Dictionary<string, int> _collection;
        List<FuzzySearchResult> _collectionOfFuzzySearchResult;

        double _threshold;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="collection">Колекция объектов для поиска</param>
        /// <param name="threshold">Порог прохождения</param>
        public MindReader(Dictionary<string, int> collection, double threshold = 0.5)
        {
            if (threshold > 1 || threshold < 0)
                throw new ArgumentOutOfRangeException("threshold", "От 0 до 1");
            _threshold = threshold;
            _collection = collection;
        }

        public MindReader(List<FuzzySearchResult> collection, double threshold)
        {
            if (threshold > 1 || threshold < 0)
                throw new ArgumentOutOfRangeException("threshold", "От 0 до 1");
            _threshold = threshold;
            _collectionOfFuzzySearchResult = collection;
        }

        /// <summary>
        /// Добавочный поиск по подстроке + мод. левинштейна
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<FuzzySearchResult> BotSecondSearch(string text)
        {
            List<FuzzyStringComparisonOptions> options = new List<FuzzyStringComparisonOptions>();
            options.Add(FuzzyStringComparisonOptions.UseLongestCommonSubstring);

            List<FuzzySearchResult> resluts = new List<FuzzySearchResult>();
            foreach (var item in _collectionOfFuzzySearchResult)
            {
                if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    var distinction = AverageDistinction(text, item.Name.ToLower(), options);
                    if (distinction < _threshold)
                    {
                        item.Distinction = distinction;
                        resluts.Add(item);
                    }
                    // была проблема с тем, что один и тот же объект затирал себя, поэтому терялся Distinction
                    FuzzySearchResult temp; 
                    if (item.DataType == FuzzySearchResultDataType.Customer) temp = new FuzzySearchResult(item.Name, item.ID, item.CustomersKey, item.LocaleCity, item.Distinction, item.Raiting, item.DataType, item.KeyWords);
                    else temp = new FuzzySearchResult(item.Name, item.ID, item.Distinction, item.Raiting, item.DataType, item.KeyWords);

                    distinction = AverageDistinction(text, ToTranslit(item.Name.ToLower()), options);
                    if (distinction < _threshold)
                    {
                        temp.Distinction = distinction;
                        resluts.Add(temp);
                    }
                }
            }
            return resluts;
        }
        /// <summary>
        /// Поиск гудоиск
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<FuzzySearchResult> BotMainSearch(string text)
        {
            List<FuzzyStringComparisonOptions> options = new List<FuzzyStringComparisonOptions>();
            options.Add((FuzzyStringComparisonOptions)MyFuzzyStringComparisonOptions.UseDamerauLevenshtein);

            List<FuzzySearchResult> resluts = new List<FuzzySearchResult>();
            foreach (var item in _collectionOfFuzzySearchResult)
            {
                if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    var sourses = item.Name.Split(' ');
                    FuzzySearchResult temp;
                    double distinction;

                    foreach (var sourse in sourses)
                    {

                        if (item.DataType == FuzzySearchResultDataType.Customer) temp = new FuzzySearchResult(item.Name, item.ID, item.CustomersKey, item.LocaleCity, item.Distinction, item.Raiting, item.DataType, item.KeyWords);
                        else temp = new FuzzySearchResult(item.Name, item.ID, item.Distinction, item.Raiting, item.DataType,item.OtherData, item.KeyWords);

                        distinction = AverageDistinction(text, sourse, options);
                        if (distinction < _threshold)
                        {
                            temp.Distinction = distinction;
                            resluts.Add(temp);
                        }

                        if (item.DataType == FuzzySearchResultDataType.Customer) temp = new FuzzySearchResult(item.Name, item.ID, item.CustomersKey, item.LocaleCity, item.Distinction, item.Raiting, item.DataType, item.KeyWords);
                        else temp = new FuzzySearchResult(item.Name, item.ID, item.Distinction, item.Raiting, item.DataType, item.OtherData, item.KeyWords);

                        distinction = AverageDistinction(text, ToTranslit(sourse), options);
                        if (distinction < _threshold)
                        {
                            temp.Distinction = distinction;
                            resluts.Add(temp);
                        }
                    }
                    #region KeyWordsSearch
                    if (item.KeyWords != null)
                    {
                        char[] splitters = { '~', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '-', '+', '=', '!', '"', '№', ';', '%', ':', '?', '.', ',', '/', '<', '|', '>', '`', '\\', '\'', ' ' };
                        List<string> KeyWords = item.KeyWords.Split(',').Select(e => e.Trim(splitters)).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        foreach (string keyWord in KeyWords)
                        {
                            if (!string.IsNullOrWhiteSpace(keyWord))
                            {
                                if (item.DataType == FuzzySearchResultDataType.Customer) temp = new FuzzySearchResult(item.Name, item.ID, item.CustomersKey, item.LocaleCity, item.Distinction, item.Raiting, item.DataType, item.KeyWords);
                                else temp = new FuzzySearchResult(item.Name, item.ID, item.Distinction, item.Raiting, item.DataType, item.OtherData, item.KeyWords);

                                distinction = AverageDistinction(text, keyWord, options) * 1.5;
                                if (distinction < _threshold)
                                {
                                    if (distinction == 0) distinction = 0.00001;
                                    temp.Distinction = distinction;
                                    resluts.Add(temp);
                                }
                                if (distinction == 0) break;

                                if (item.DataType == FuzzySearchResultDataType.Customer) temp = new FuzzySearchResult(item.Name, item.ID, item.CustomersKey, item.LocaleCity, item.Distinction, item.Raiting, item.DataType, item.KeyWords);
                                else temp = new FuzzySearchResult(item.Name, item.ID, item.Distinction, item.Raiting, item.DataType, item.OtherData, item.KeyWords);

                                distinction = AverageDistinction(text, ToTranslit(keyWord), options) * 1.5;
                                if (distinction < _threshold)
                                {
                                    if (distinction == 0) distinction = 0.00001;
                                    temp.Distinction = distinction;
                                    resluts.Add(temp);
                                }

                                if (distinction == 0.00001) break;
                            }
                        }
                    }
                    #endregion
                }
            }
            return resluts;
        }
        private static bool IsBasicLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
        public string ToTranslit(string text)
        {
            foreach (char c in text)
            {
                int res;
                if (!int.TryParse(c.ToString(), out res))
                {
                    if (IsBasicLetter(c))
                    {
                        text = Transliteration.LatinToCyrillyc(text, Language.Russian);
                        break;
                    }
                    else
                    {
                        text = Transliteration.CyrillicToLatin(text);
                        break;
                    }
                }
            }
            return text;
        }
        /// <summary>
        /// Вроде считает среднее отклонение, 0 нет отличий 1 нет совпадений
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private double AverageDistinction(string target, string source, List<FuzzyStringComparisonOptions> options)
        {
            List<double> results = new List<double>();
            if (!options.Contains(FuzzyStringComparisonOptions.CaseSensitive))
            {
                source = source.Capitalize();
                target = target.Capitalize();
            }
            if (options.Contains(FuzzyStringComparisonOptions.UseHammingDistance) && source.Length == target.Length)
                results.Add((double)(source.HammingDistance(target) / (double)target.Length));
            if (options.Contains(FuzzyStringComparisonOptions.UseJaccardDistance))
                results.Add(source.JaccardDistance(target));
            if (options.Contains(FuzzyStringComparisonOptions.UseJaroDistance))
                results.Add(source.JaroDistance(target));
            if (options.Contains(FuzzyStringComparisonOptions.UseJaroWinklerDistance))
                results.Add(source.JaroWinklerDistance(target));
            if (options.Contains(FuzzyStringComparisonOptions.UseNormalizedLevenshteinDistance))
                results.Add(Convert.ToDouble(source.NormalizedLevenshteinDistance(target)) / Convert.ToDouble(Math.Max(source.Length, target.Length) - source.LevenshteinDistanceLowerBounds(target)));
            else if (options.Contains(FuzzyStringComparisonOptions.UseLevenshteinDistance))
                results.Add(Convert.ToDouble(source.LevenshteinDistance(target)) / Convert.ToDouble(source.LevenshteinDistanceUpperBounds(target)));
            if (options.Contains(FuzzyStringComparisonOptions.UseLongestCommonSubsequence))
                results.Add(1.0 - Convert.ToDouble((double)source.LongestCommonSubsequence(target).Length / Convert.ToDouble(Math.Min(source.Length, target.Length))));
            if (options.Contains(FuzzyStringComparisonOptions.UseLongestCommonSubstring))
            {// тут были перепутаны target и source
                var length = (double)target.LongestCommonSubstring(source).Length;
                var max = (double)Math.Max(source.Length, target.Length);
                results.Add(1.0 - length / max);
            }
            if (options.Contains(FuzzyStringComparisonOptions.UseSorensenDiceDistance))
                results.Add(source.SorensenDiceDistance(target));
            if (options.Contains(FuzzyStringComparisonOptions.UseOverlapCoefficient))
                results.Add(1.0 - source.OverlapCoefficient(target));
            if (options.Contains(FuzzyStringComparisonOptions.UseRatcliffObershelpSimilarity))
                results.Add(1.0 - source.RatcliffObershelpSimilarity(target));
            if (options.Contains((FuzzyStringComparisonOptions)MyFuzzyStringComparisonOptions.UseDamerauLevenshtein))
                results.Add((double)dynamicEditDistance(target, source) / (double)Math.Max(target.Length, source.Length));
            return results.Average();
        }
        /// <summary>
        /// Damerau-Levenshtein алгоритм
        /// </summary>
        /// <param name="original"></param>
        /// <param name="modified"></param>
        /// <returns></returns>
        public static int DamerauLevenshtein(string original, string modified)
        {
            int len_orig = original.Length;
            int len_diff = modified.Length;

            var matrix = new int[len_orig + 1, len_diff + 1];

            for (int i = 0; i <= len_orig; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= len_diff; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= len_orig; i++)
            {
                for (int j = 1; j <= len_diff; j++)
                {
                    int cost = modified[j - 1] == original[i - 1] ? 0 : 1;
                    var vals = new int[] {
                matrix[i - 1, j] + 1,
                matrix[i, j - 1] + 1,
                matrix[i - 1, j - 1] + cost
            };
                    matrix[i, j] = vals.Min();
                    if (i > 1 && j > 1 && original[i - 1] == modified[j - 2] && original[i - 2] == modified[j - 1])
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost);
                }
            }
            return matrix[len_orig, len_diff];
        }
        public int dynamicEditDistance(string str1, string str2)
        {
            int lenstr1 = str1.Length;
            int lenstr2 = str2.Length;

            int[,] temp = new int[lenstr1 + 1, lenstr2 + 1];

            for (int i = 0; i <= lenstr1; i++)
                temp[i, 0] = i;

            for (int i = 0; i <= lenstr2; i++)
                temp[0, i] = i;

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    if (str1[i - 1] == str2[j - 1])
                    {
                        temp[i, j] = temp[i - 1, j - 1];
                    }
                    else
                    {
                        temp[i, j] = 1 + min(temp[i - 1, j - 1], temp[i - 1, j], temp[i, j - 1]);
                    }
                }
            }
            return temp[str1.Length, str2.Length];
        }
        private static int min(int a, int b, int c)
        {
            int l = Math.Min(a, b);
            return Math.Min(l, c);
        }
    }

}
