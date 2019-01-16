using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Media;
using Mall.Bot.Search.Models;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public enum MapSizeType
    {
        Default = 0,
        Middle = 1,
        Big = 2
    }
    //[Table("Organizations")]
    public class Organization : IOrganization 
    {
//        [JsonIgnore]
//        public byte [] Logo { get; set; }
        [JsonIgnore]
        private Color _logoBaseColor;
        /// <summary>
        /// Идентификатор
        /// </summary>                
        [Key]
        [JsonIgnore]
        public int OrganizationID { get; set; }
        ///// <summary>
        ///// Ключевые слова или словосочетания, разделенные запятой.Используется для поиска
        ///// </summary>
        [JsonIgnore]
        public string KeyWords { get; set; }
        /// <summary>
        /// Координата организации на карте по оси X
        /// </summary>
        //[JsonIgnore]
        //public Nullable<double> Longitude { get; set; }
        /// <summary>
        /// Координата организации на карте по оси Y
        /// </summary>
        //[JsonIgnore]
        //public Nullable<double> Latitude { get; set; }
        /// <summary>
        /// Признак, является ли организация якорным арендатором
        /// </summary>
        [JsonIgnore]
        public Nullable<bool> IsAnchor { get; set; }
        /// <summary>
        /// Идентификатор торгового центра
        /// </summary>
        [JsonIgnore]
        public Nullable<int> CustomerID { get; set; }
        /// <summary>
        /// Флаг. Используется ли. true or NULL - да, false - нет
        /// </summary>
        [JsonIgnore]
        public bool? IsUsed { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Тип организации
        /// </summary>
//        [JsonIgnore]
//        public OrganizationType OrganizationType { get; set; }
        /// <summary>
        /// Категории организации
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<CategoryOrganization> CategoryOrganization { get; set; }
        /// <summary>
        /// Базовый цвет логотипа для использования совместно с логотипом. По умолчанию цвет верхнего левого пикселя логотипа.
        /// </summary>
        [JsonIgnore]
        public string LogoBaseColor { get; set; }
        /// <summary>
        /// Коэффициент популярности организации [0;1]
        /// </summary>
        [JsonIgnore]
        public Nullable<float> Rating { get; set; }

        [JsonIgnore]
        public Color LogoBaseColorValue
        {
            get
            {
                _logoBaseColor = LogoBaseColor == null ? Colors.White : (Color)ColorConverter.ConvertFromString(LogoBaseColor);
                return _logoBaseColor;
            }
        }

        [NotMapped]
        ICollection<ICategoryOrganization> IOrganization.CategoryOrganization
        {
            get
            {
                return CategoryOrganization.OfType<ICategoryOrganization>().ToList();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Связь с точками
        /// </summary>
        public virtual HashSet<OrganizationMapObject> OrganizationMapObject { get; set; } = new HashSet<OrganizationMapObject>();

        public override int GetHashCode()
        {
            return OrganizationID;
        }
        public override bool Equals(object obj)
        {
            var org = (Organization)obj;
            return org.OrganizationID == OrganizationID;
        }
    }
}
