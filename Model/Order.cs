namespace ShopApi
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }

    public class Order
    {
        public int Id {get; set;}
        public Guid OrderId {get; set;}
        public Guid UserId {get; set;}
        public  User user {get; set;}

        public decimal Subtotal {get; set;}
        public decimal DiscountAmount {get; set;}
        public decimal TotalAmount {get; set;}
        public decimal PromoCodeDiscount {get; set;}
        public string promoCode {get; set;}

        public int BonisPointsUsed {get; set;}

        public OrderStatus Status {get; set;}
        public PaymentStatus Paymentstatus {get; set;}

    public DateTime CreatedAt {get; set;}
    public DateTime PaidAt {get; set;}
    public DateTime ShipedAt {get; set;}
    public DateTime DeliveredAt {get; set;}

    public virtual ICollection<OrderItem> OrderItems {get; set;}

    }
}