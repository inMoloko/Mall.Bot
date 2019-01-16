using System;

namespace Mall.Bot.Search.Models
{
    public enum ServiceCategoryType
    {
        /// <summary>
        /// Есть направление
        /// </summary>
        Terminal = 0,

        /// <summary>
        /// Связь между этажами
        /// </summary>
        Link = 1,

        /// <summary>
        /// Сервистный объект отображается ввиде картинки
        /// </summary>
        Service = 2
    }

    public interface ICategory
    {
        string Name { get; set; }
        int CategoryID { get; set; }
        string StringID { get; set; }
        int? ParentID { get; set; }
        Nullable<int> CustomerID { get; set; }

        string LogoExtension { get; set; }

        ServiceCategoryType? ServiceCategoryType { get; set; }
    }
}