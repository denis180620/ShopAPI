namespace ShopApi
{
    public interface IPaymentClient
    {
        Task<PaymentTransaction> CreatePaymentAsync(CreatePaymentRequest request);
        Task<PaymentTransaction> GetStatusPaymentAsync(Guid OrderId);
    }
    public class PaymentClient : IPaymentClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentClient> _logger;

        public PaymentClient(HttpClient httpClient, ILogger<PaymentClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<PaymentTransaction> CreatePaymentAsync(CreatePaymentRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/payment/create", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PaymentTransaction>();
        }
        public async Task<PaymentTransaction> GetStatusPaymentAsync(Guid OrderId)
        {
            var response = await _httpClient.GetAsync("/api/payment/{orderId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PaymentTransaction>();
        }
    }
}