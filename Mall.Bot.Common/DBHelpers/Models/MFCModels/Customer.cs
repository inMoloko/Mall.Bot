using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class Customer
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int CustomerID{ get; set; }
        /// <summary>
        /// Название
        /// </summary>
        public string Name{ get; set; }
    }
}
