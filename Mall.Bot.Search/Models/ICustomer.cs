using System.Data.Spatial;

namespace Mall.Bot.Search.Models
{
    public interface ICustomer
    {
        int CustomerID { get; set; }
        /// <summary>
        /// Названия городов на разных языках, указанные в базе через запятую.
        /// русское, английское
        /// </summary>
        string City { get; set; }
        /// <summary>
        /// Географические координаты кастомера
        /// </summary>
        DbGeography Location { set; get; }
        string Name { get; set; }
        /// <summary>
        /// Названия городов на разных языках
        /// LocaleCity[0] - русское   LocaleCity[1] - английское
        /// </summary>
        string[] LocaleCity { get; set; }
        string Synonym { get; set; }
    }
}
