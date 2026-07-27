namespace ShopApi
{
    public class CreatePaymentRequest
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class CreatePaymentResponse
    {
        public string PaymentId { get; set; }
        public string PaymentUrl { get; set; }
        public string Status { get; set; }
    }    
        public class PaymentTransaction
        {
            public Guid Id { get; set; }
            public string? PaymentId { get; set; }
            public Guid OrderId { get; set; }
            public Guid UserId { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public decimal Amount { get; set; }
            public string? StatusCode { get; set; }
            public PaymentStatus Status { get; set; }
            public string Message { get; set; }
            public string? PaymentUrl { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public DateTime? PaidAt { get; set; }
        }
        public enum PaymentStatus
        {
            Pending = 0,        // Создан
            Redirected = 1,     // Клиент перенаправлен на оплату
            Authorized = 2,     // Средства зарезервированы
            Confirmed = 3,      // Оплачен (финальный статус)
            Rejected = 4,       // Отклонен
            Canceled = 5,       // Отменен пользователем
            Refunded = 6,       // Возвращен
            Error = 7,           // Ошибка при обработке
            Delivery = 8        // Доставка
        }
    
}