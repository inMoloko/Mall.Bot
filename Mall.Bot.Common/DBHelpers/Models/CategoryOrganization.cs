using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mall.Bot.Search.Models;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class CategoryOrganizationCompaper : IEqualityComparer<CategoryOrganization>
    {
        public bool Equals(CategoryOrganization x, CategoryOrganization y)
        {
            return x.CategoryID == y.CategoryID;
        }

        public int GetHashCode(CategoryOrganization obj)
        {
            return obj.CategoryID;
        }
    }



    public class CategoryOrganization : ICategoryOrganization
    {
        [Key, Column(Order = 1)]        
        public int OrganizationID { get; set; }

        [Key, Column(Order = 2)]        
        public int CategoryID { get; set; }
        
        public Category Category { get; set; }

        [NotMapped]
        ICategory ICategoryOrganization.Category => Category;

        public int Something { get; set; }
    }
}
