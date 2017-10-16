using Mall.Bot.Common.MallHelpers;
using Mall.Bot.Search.Models;
using Moloko.Utils;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Spatial;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public partial class Customer : BaseObject, ICustomer
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int CustomerID { get; set; }
        /// <summary>
        /// Город
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// Внешний теккстовый идентификатор
        /// </summary>
        public string Synonym { get; set; }
        /// <summary>
        /// Показывает был ли опубликован ТЦ
        /// </summary>
        [NotMapped]
        public int IsPublish { set; get; }
        /// <summary>
        /// долгота
        /// </summary>
        [NotMapped]
        public DbGeography Location{ set; get; }
        /// <summary>
        /// Названия городов, указанные в базе
        /// </summary>
        [NotMapped]
        public string[] LocaleCity
        {
            get
            {
                if (string.IsNullOrWhiteSpace( City))
                {
                    string [] res = { "" };
                    return res;
                }

                var cities = City.Trim(AnalyseHelper.splitters).Split(',');
                return cities;
            }
            set
            {
                
            }
        }
    }
}
