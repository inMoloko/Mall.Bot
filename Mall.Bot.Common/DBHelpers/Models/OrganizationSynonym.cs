using Mall.Bot.Search.Models;
using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class OrganizationSynonym : IOrganizationSynonym
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int OrganizationSynonymID { get; set; }
        /// <summary>
        /// Название
        /// </summary>
        public string OrganizationName { get; set; }
        /// <summary>
        /// синонимы
        /// </summary>
        public string Synonyms { get; set; }
    }
}
