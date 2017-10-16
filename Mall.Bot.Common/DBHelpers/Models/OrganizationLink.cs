using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Bot.Common.DBHelpers.Models
{
    /// <summary>
    /// Связь организаций
    /// </summary>
    public partial class OrganizationLink
    {
        /// <summary>
        /// Первичный ключ
        /// </summary>
        public int OrganizationLinkID { get; set; }

        public int OrganizationFromID { get; set; }


        public int OrganizationToID { get; set; }
        
    }
}
