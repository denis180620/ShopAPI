using Microsoft.EntityFrameworkCore;

namespace ShopApi
{
    public interface IProduct
    {
        Task<Product> CreateProduct(Product product);
        Task<Product> UpdateProduct(Product product);
        Task<bool> DeleteProduct(Guid id);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsAsync(PaginationRequest request);

        Task<List<Product>> GetQuantityProductAsync(int quantity);
        Task<Product> GetProductById(Guid Id);
        Task<List<Product>> GetProductCategoryById(Guid Id);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsPaginated(
           PaginationRequest request);
        Task<List<Product>> GetAdministratorProduct(Guid UserId);
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
            _logger.LogInformation("Создание продукта: Id={Id}, UserId={UserId}, Name={Name}, CategoryId={CategoryId}",
                product.Id, product.UserId, product.Name, product.CategoryId);
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
                _logger.LogInformation("Репозиторий: Обновление продукта Id={Id}, Name='{Name}'", product.Id, product.Name);
                 _context.Products.Update(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Репозиторий: Продукт успешно сохранён в БД");
                return product;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении продукта");
                throw new Exception("ОШибка при обновлении продукта" + ex.Message);
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
        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsAsync(PaginationRequest request)
        {
            // Валидация PageNumber
            if (request.PageNumber < 1)
            {
                request.PageNumber = 1;
            }

            try
            {
                // Базовый запрос без Include для подсчета
                var query = _context.Products.AsNoTracking();

                // Фильтрация по категории
                if (request.CategoryId > Guid.Empty)
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId);
                }

                // Фильтрация по цене
                if (request.minPrice > -1)
                {
                    query = query.Where(p => p.Price >= request.minPrice);
                }

                // maxPrice == 0 означает "без ограничения" (или decimal.MaxValue из сервиса)
                if (request.maxPrice > 0)
                {
                    query = query.Where(p => p.Price <= request.maxPrice);
                }

                // Подсчет общего количества (ДО Include и сортировки)
                var totalCount = await query.CountAsync();

                // Применяем сортировку с обработкой null SortBy
                query = (!string.IsNullOrEmpty(request.SortBy) ? request.SortBy.ToLower() : "") switch
                {
                    "name" => request.SortDescending
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name),
                    "price" => request.SortDescending
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price),
                    "createdat" => request.SortDescending
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.Id)
                };

                // Добавляем Include только для получения данных
                // query = query.Include(p => p.Category);

                // Применяем пагинацию
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении продуктов");
                throw new Exception("Ошибка при получении продуктов: " + ex.Message);
            }
        }
        public async Task<List<Product>> GetQuantityProductAsync(int queantity)
        {
            var product = await _context.Products.AsNoTracking().Where(c => c.StockQuantity < queantity).ToListAsync();
            return product;
        }
        public async Task<List<Product>> GetProductCategoryById(Guid Id)
        {
            var product = await _context.Products.Where(s => s.CategoryId == Id).ToListAsync();
            return product;
        }
        public async Task<Product> GetProductById(Guid Id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(s => s.Id == Id);
            return product;
        }
        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsPaginated(
           PaginationRequest request)
        {
            // Валидация PageNumber
            if (request.PageNumber < 1)
            {
                request.PageNumber = 1;
            }

            // Базовый запрос без Include для фильтрации и подсчета
            var query = _context.Products.AsNoTracking();

            // Применяем фильтры
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.NameEn.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search) ||
                    p.DescriptionEn.ToLower().Contains(search));
            }

            if (request.CategoryId > Guid.Empty)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId);
            }

            if (request.minPrice > -1)
            {
                query = query.Where(p => p.Price >= request.minPrice);
            }

            if (request.maxPrice > 0)
            {
                query = query.Where(p => p.Price <= request.maxPrice);
            }

            // Подсчет общего количества (ДО Include и сортировки)
            var totalCount = await query.CountAsync();

            // Применяем сортировку
            query = (!string.IsNullOrEmpty(request.SortBy) ? request.SortBy.ToLower() : "") switch
            {
                "name" => request.SortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "price" => request.SortDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "createdat" => request.SortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Id)
            };

            // Добавляем Include только для получения данных
            // query = query.Include(p => p.Category);

            // Применяем пагинацию
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task<List<Product>> GetAdministratorProduct(Guid UserId)
        {
            var products = await _context.Products.AsNoTracking().Where(c => c.UserId == UserId).ToListAsync();
            return products;
        }
    }
}