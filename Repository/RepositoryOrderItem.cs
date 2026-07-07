using Microsoft.EntityFrameworkCore;

namespace ShopApi
{
    public interface IOrderItem
    {
        Task<OrderItem> CreateItem(OrderItem order);
        Task<OrderItem> PutItem(OrderItem order);
        Task<List<OrderItem>> GetOrderItemsAsync(Guid OrderId);
        Task<bool> DeleteItem(OrderItem order);
        Task<List<OrderItem>> GetOrderByItemAsync();
        Task<OrderItem> GetOrderItemAsync(int Id);
    }
    public class RepositoryOrderItem : IOrderItem
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RepositoryOrderItem> _logger;

        public RepositoryOrderItem(AppDbContext context, ILogger<RepositoryOrderItem> logger)
        {
            _context =context;
            _logger = logger;
        }
        public async Task<OrderItem> CreateItem(OrderItem order)
        {
            try
            {
                _context.OrderItems.Add(order);
                await _context.SaveChangesAsync();
                return order;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания позиции заказа");
                throw new Exception("Ошибка создания позиции заказа" + ex.Message);
            }
        }
        public async Task<OrderItem> PutItem(OrderItem order)
        {
            try
            {
                _context.OrderItems.Update(order);
                await _context.SaveChangesAsync();
                return order;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления заказа");
                throw new Exception("Ошибка обновления заказа" + ex.Message);
            }
        }
        public async Task<List<OrderItem>> GetOrderItemsAsync(Guid OrderId)
        {
            try{
            return await _context.OrderItems.Where(c => c.OrderId == OrderId).ToListAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "ошибка выборки позиций");
                throw new Exception("Ошибка выборки позиций для заказа" + ex.Message);
            }
        }
        public async Task<List<OrderItem>> GetOrderByItemAsync()
        {
            try
            {
                return await _context.OrderItems.OrderBy(c=> c.OrderId).ToListAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения позиций заказов");
                throw new Exception("ошибка получения выборки по заказам" + ex.Message);
            }
        }
        public async Task<bool> DeleteItem(OrderItem order)
        {
            try{
            _context.OrderItems.Remove(order);
            await _context.SaveChangesAsync();
            return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления позиции заказа");
                return false;
                throw new Exception("Ошибка удаления позиции заказа" + ex.Message);
            }
        }
        public async Task<OrderItem> GetOrderItemAsync(int Id)
        {
            try
            {
                return await _context.OrderItems.FirstOrDefaultAsync(c => c.Id == Id);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения позиции заказа");
                throw new Exception("ошибка получения позиции заказа" + ex.Message);
            }
        }
    }
}