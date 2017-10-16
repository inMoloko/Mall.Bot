using Mall.Bot.Search.Mall;
using Newtonsoft.Json;
using System;

namespace Mall.Bot.Search.Models
{
    /// <summary>
    /// Результат нечеткого поиска
    /// </summary>
    public class FuzzySearchResult : IEquatable<FuzzySearchResult>, IComparable<FuzzySearchResult>
    {
        /// <summary>
        /// рейтинг
        /// </summary>
        [JsonIgnore]
        public Nullable<float> Raiting { get; set; }
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ид организации
        /// </summary>
        public int ID { get; set; }
        [JsonIgnore]
        public string CustomersKey{ get; set; }
        [JsonIgnore]
        public string [] LocaleCity{ get; set; }
        /// <summary>
        /// Отличие
        /// </summary>
        public double Distinction { get; set; }

        [JsonIgnore]
        public FuzzySearchResultDataType DataType { get; set; }

        [JsonIgnore]
        public string KeyWords { get; set; }

        [JsonIgnore]
        public dynamic OtherData { get; set; }

        public FuzzySearchResult(string _name, int _id, double _distinction, Nullable<float> _raiting, FuzzySearchResultDataType _dataType, dynamic _otherData, string _keyWords = null)
        {
            Name = _name;
            ID = _id;
            Distinction = _distinction;
            Raiting = _raiting;
            DataType = _dataType;
            OtherData = _otherData;
            KeyWords = _keyWords;
        }


        public FuzzySearchResult(string _name, int _id, double _distinction, Nullable<float> _raiting, FuzzySearchResultDataType _dataType, string _keyWords = null)
        {
            Name = _name;
            ID = _id;
            Distinction = _distinction;
            Raiting = _raiting;
            DataType = _dataType;
            KeyWords = _keyWords;
        }

        public FuzzySearchResult(string _name, int _id, string _customersKey, string[] _loccity, double _distinction, Nullable<float> _raiting, FuzzySearchResultDataType _dataType, string _keyWords = null)
        {
            Name = _name;
            ID = _id;
            Distinction = _distinction;
            Raiting = _raiting;
            CustomersKey = _customersKey;
            LocaleCity = _loccity;
            DataType = _dataType;
            KeyWords = _keyWords;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            FuzzySearchResult objAsFuzzySearchResult = obj as FuzzySearchResult;
            if (objAsFuzzySearchResult == null) return false;
            else return Equals(objAsFuzzySearchResult);
        }
        public int SortByDistinctionAscending(double d1, double d2)
        {

            return d1.CompareTo(d2);
        }
        public int CompareTo(FuzzySearchResult compareFuzzySearchResult)
        {
            // A null value means that this object is greater.
            if (compareFuzzySearchResult == null)
                return 1;

            else
                return this.Distinction.CompareTo(compareFuzzySearchResult.Distinction);
        }
        public override int GetHashCode()
        {
            return ID;
        }
        public bool Equals(FuzzySearchResult other)
        {
            if (other == null) return false;
            return (this.Distinction.Equals(other.Distinction));
        }

        public override string ToString()
        {
            return $"{DataType}, {Name}";
        }
    }
}
