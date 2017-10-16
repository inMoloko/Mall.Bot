using Mall.Bot.Search.Models;
using Moloko.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media.Imaging;

namespace Mall.Bot.Common.DBHelpers.Models
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
    /// <summary>
    /// Категория организации
    /// </summary>
    public partial class Category : BaseObject, ICategory
    {
        private BitmapImage _logo = null;

        public Category() : base()
        {
        }
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int CategoryID { get; set; }
        /// <summary>
        /// Логотип
        /// </summary>
        public byte[] Logo { get; set; }
        /// <summary>
        /// Признак сипользуется категория или нет
        /// </summary>
        public bool IsUsed { get; set; }
        /// <summary>
        /// Порядок сортировки. Чем меньше значение, тем более высокий приоритет
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// ID родительской категории
        /// </summary>
        public int? ParentID { get; set; }
        /// <summary>
        /// Идентификатор торгового центра
        /// </summary>
        public int? CustomerID { get; set; }
        public string ImportMetadata { get; set; }
        /// <summary>
        /// тип категории
        /// </summary>
        public ServiceCategoryType? ServiceCategoryType { get; set; }

        public string LogoExtension { get; set; }

        [JsonIgnore]
        public BitmapImage LogoValue
        {
            get
            {
                if (_logo == null)
                {
                    _logo = ImagingHelper.GetBitMapImage(Logo);
                }
                return _logo;
            }
        }
    }
    
}
