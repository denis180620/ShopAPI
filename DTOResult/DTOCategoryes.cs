namespace ShopApi
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string NameEn { get; set; }
        public int? ParentCategoryId { get; set; }

    }
}