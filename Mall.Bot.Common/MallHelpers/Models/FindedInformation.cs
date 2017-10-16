using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.Helpers;
using System.Collections.Generic;

namespace Mall.Bot.Common.MallHelpers.Models
{
    public class FindedInformation
    {
        /// <summary>
        /// Результат поиска
        /// </summary>
        public SearchResult Result { get; set; }
        /// <summary>
        ///  список изображений этажей в порядке их посещения пользователем
        /// </summary>
        public List<BitmapSettings> FloorsPictures = new List<BitmapSettings>();
        /// <summary>
        /// Сгруппированные полезные данные из Result
        /// </summary>
        public object GroopedResult { get; set; }
        /// <summary>
        /// Текстовое описание маршрута
        /// </summary>
        public List<MapObject> TextDescription = new List<MapObject>();
    }
}
