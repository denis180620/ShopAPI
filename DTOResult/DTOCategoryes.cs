namespace ShopApi
{
    public class CategoryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string NameEn { get; set; }
        public Guid? ParentCategoryId { get; set; }

    }
}