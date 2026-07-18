using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;

namespace ShopApi
{
    public interface IOrder
    {
        Task<Order> CreateOrder(Order order);
        Task<Order> GetOrderAsync(Guid OrderId);
        Task<List<Order>> GetOredrs();
        Task<List<Order>> GetOrderStatus(Order.OrderStatus status);
        Task<Order> PutOrder(Order order);
        Task<Order> GetOrdersByUserId(Guid UserId);
    }
    public class RepositoryOreder : IOrder
    {
        private readonly AppDbContext _context;
        ILogger<RepositoryOreder> _logger;

        public RepositoryOreder(AppDbContext context, ILogger<RepositoryOreder> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Order> CreateOrder(Order order)
        {
            try
            {
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();
                return order;
            }
            catch(Exception ex)
            {
                _logger.LogError("Ошибка записи заказа в БД");
                throw new Exception("Ошибка записи заказа в бд" + ex.Message);
            }
        }
        public async Task<Order> GetOrderAsync(Guid OrderId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(c=> c.OrderId == OrderId);
                return order;
            }
            catch(Exception ex)
            {
                _logger.LogError("Ошибка получения заказа");
                throw new Exception("Ошибка полчения заказа" + ex.Message);
            }
        }
        public async Task<List<Order>> GetOrderStatus(Order.OrderStatus status)
        {
            try{
            var order = await _context.Orders.Where(c => c.Status == status).ToListAsync();
            return order;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка выборки заказов оп статусу");
                throw new Exception("Ошибка выборки заказов" + ex.Message);
            }
        }
        public async Task<List<Order>> GetOredrs()
        {
            try
            {
                return await _context.Orders.ToListAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка выборки");
                throw new Exception("Ошибка выборки" + ex.Message);
            }
        }
        public async Task<Order> PutOrder(Order order)
        {
            try{
             _context.Orders.Update(order);
             await _context.SaveChangesAsync();
             return order;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления заказа");
                throw new Exception("Ошибка обновления заказа" + ex.Message);
            }
        }
        public async Task<Order> GetOrdersByUserId(Guid UserId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(c => c.UserId == UserId);
                return order;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения заказа по UserId");
                throw new Exception("Ошибка получения заказа по UserId" + ex.Message);
            }
        }
    }
}