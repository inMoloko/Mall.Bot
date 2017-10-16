using System;

namespace Moloko.Utils
{
    public partial class BaseObject
    {
        public BaseObject()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Строковый идентификатор
        /// </summary>
        public string StringID { get; set; }
        /// <summary>
        /// Дата последнего редактирования
        /// </summary>
        public DateTime ModifiedDate { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
