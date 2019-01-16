using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class MTerminal
    {
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        [Key]
        public int MTerminalID { get; set; }
        /// <summary>
        /// Связь с точками
        /// </summary>
        public virtual HashSet<TerminalMapObject> TerminalMapObject { get; set; } = new HashSet<TerminalMapObject>();
    }
}
