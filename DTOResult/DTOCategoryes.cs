namespace ShopApi
{
    public class CategoryDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string NameEn { get; set; }
        public int? ParentCategoryId { get; set; }

    }
}