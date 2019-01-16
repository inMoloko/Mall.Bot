using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mall.Bot.Search.Models
{
    public enum OrganizationType
    {
        [Description("Магазины")]
        Shop = 1,
        [Description("Рестораны и кафе")]
        Cafe = 2,
        [Description("Услуги")]
        Service = 3,
        [Description("Развлечения")]
        Entertainment = 4,
        [Description("Дополнительный")]
        Extra = 5
    }
    public interface IOrganization
    {
        int OrganizationID { get; set; }
        string KeyWords { get; set; }
        Nullable<int> CustomerID { get; set; }
        string Name { get; set; }
        //OrganizationType OrganizationType { get; set; }
        ICollection<ICategoryOrganization> CategoryOrganization { get; set; }
        Nullable<float> Rating { get; set; }
    }
}
