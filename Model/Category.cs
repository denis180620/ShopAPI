using Microsoft.AspNetCore.SignalR;

namespace ShopApi
{
    public class Category
    {
        public int Id {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
        public string NameRu {get; set;}
        public string NameEn {get; set;}
        public int?  ParentCategoryId {get; set;}
        public virtual Category? ParentCategory {get; set;}
        public virtual ICollection<Category> ChildCategories {get; set;}
        public virtual ICollection<Product> Products {get; set;}
        public int? DisplayOrder { get; set; }
    }
}