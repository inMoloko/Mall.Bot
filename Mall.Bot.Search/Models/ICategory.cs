using System;

namespace Mall.Bot.Search.Models
{
    public interface ICategory
    {
        string Name { get; set; }
        int CategoryID { get; set; }
        int? ParentID { get; set; }
        Nullable<int> CustomerID { get; set; }
    }
}
