namespace ShopApi
{
    public class OrderItem
    {
        public int Id {get; set;}
        public Guid OrderId {get; set;}
        public virtual Order Order {get; set;}

        public Guid ProductId {get; set;}
        public virtual Product Product {get; set;}

        public string ProductName {get; set;}

        public decimal UnitPrice {get; set;}
        public decimal DiscountPrice {get; set;}
        public int Quantity {get; set;}


        public decimal TotalPrice => UnitPrice * Quantity - DiscountPrice;
        public decimal DiscountAmount {get; set;}

    }
}