namespace ShopApi
{
    public class Product
    {
        public Guid Id {get; set;}
        public string Name {get; set;}
        public decimal Price {get; set;}
        public decimal DiscountPrice {get; set;}
        public string Description {get; set;}
        public int StockQuantity {get; set;}
        public string ImageURL {get; set;}

        public int CategoryId {get; set;}
        public virtual Category Category {get; set;}

        public string NameRu { get; set; }
        public string NameEn { get; set; }
        public string DescriptionRu { get; set; }
        public string DescriptionEn { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


    }
}