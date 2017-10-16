using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class TerminalMapObject
    {
        [Key, Column(Order = 1)]
        public int MapObjectID { get; set; }
        public MapObject MapObject { get; set; }
        [Key, Column(Order = 2)]
        public int MTerminalID { get; set; }
        public MTerminal MTerminal { get; set; }
    }
}
