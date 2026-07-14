using Microsoft.EntityFrameworkCore;

namespace ShopApi
{
    public interface IProduct
    {
        Task<Product> CreateProduct(Product product);
        Task<Product> UpdateProduct(Product product);
        Task<bool> DeleteProduct(Guid id);
        Task<List<Product>> GetProductsAsync();
        Task<Product> GetNameProduct(string name);
        Task<List<Product>> GetQuantityProductAsync(int quantity);
        Task<List<Product>> GetPriceProduct(decimal price);
    }

    public class RepositoryProduct : IProduct
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RepositoryProduct> _logger;

        public RepositoryProduct(AppDbContext context, ILogger<RepositoryProduct> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Product> CreateProduct(Product product)
        {
            try{
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании продукта");
                throw new Exception("ОШибка при созхдании продукта" + ex.Message);
            }
        }
        public async Task<Product> UpdateProduct(Product product)
        {
            try
            {
                 _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return product;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении количества продукта");
                throw new Exception("ОШибка при обновлени количества продукта" + ex.Message);
            }
        }
        public async Task<bool> DeleteProduct(Guid id)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(c => c.Id == id);

                if(product == null)
                {
                    throw new Exception("Продукт не найден");
                }
                 _context.Products.RemoveRange(product);
                 await _context.SaveChangesAsync();
                 return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении продукта");
                throw new Exception("ОШибка при удалении продукта" + ex.Message);
            }
        }
        public async Task<List<Product>> GetProductsAsync()
        {
            try{
            var products = await _context.Products.ToListAsync();
            return products;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении продуктов");
                throw new Exception("ОШибка при получении продуктов" + ex.Message);
            }
        }
        public async Task<Product> GetNameProduct(string name)
        {
            var product = await _context.Products.FirstOrDefaultAsync(c => c.Name == name);
            return product;
        }
        public async Task<List<Product>> GetQuantityProductAsync(int queantity)
        {
            var product = await _context.Products.AsNoTracking().Where(c => c.StockQuantity < queantity).ToListAsync();
            return product;
        }
        public async Task<List<Product>> GetPriceProduct(decimal price)
        {
            var product = await _context.Products.AsNoTracking().Where(c => c.Price < price).ToListAsync();
            return product;
        }
    }
}