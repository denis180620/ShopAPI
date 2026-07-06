using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;

namespace ShopApi
{
    public interface ICategory
    {
        Task<Category> CreateCategory(Category category);
        Task<Category> GetCategoryById(int id);
        Task<List<Category>> GetAllCategories();
        Task<Category> UpdateCategory(Category category);
        Task<bool> DeleteCategory(Category category);
        Task<Category> GetChildCategories(string name);
        Task<Category> UpdateProduct(Category category);
    }
    public class RepositoryCategory : ICategory
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RepositoryCategory> _logger;
        public RepositoryCategory(AppDbContext context, ILogger<RepositoryCategory> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Category> CreateCategory(Category category)
        {
            try{
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании категории");
                throw new Exception("Ошибка при создании категории:" +ex.Message);
            }
        }
        public async Task<Category> GetCategoryById(int id)
        {
            try{
            var category = await _context.Categories
                                .AsNoTracking()
                                .FirstOrDefaultAsync(item => item.Id == id);
            return category;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения котегории");
                throw new Exception("Ошибка при получении категории" + ex.Message);
            }                            
        }
        public async Task<List<Category>> GetAllCategories()
        {
            try
            {
                var categories = await _context.Categories
                                        .AsNoTracking()
                                        .OrderBy(c => c.Id)
                                        .ToListAsync();
                    return categories;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения всех категорий");
                throw new Exception("Ошибка получения всех категорий" + ex.Message);
            }
        }
        public async Task<Category> UpdateCategory(Category category)
        {
            try{
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления категории");
                throw new Exception("Ошибка обновления категории" + ex.Message);
            }
        }
        public async Task<bool> DeleteCategory(Category category)
        {
            try{
               _context.Categories.Remove(category); 
               await _context.SaveChangesAsync();
               return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка удадения категории");
                throw new Exception("Ошибка удаления категории" + ex.Message);
            }
        }
        public async Task<Category> GetChildCategories(string name)
        {
            try
            {
                var category = await _context.Categories
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(c => c.Name == name);
                return category;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске категории по имени");
                throw new Exception("Ошибка при получении категории по имени" + ex.Message);
            }
        }
        public async Task<Category> UpdateProduct(Category category)
        {
            try{
            var categories = await _context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);

            if(categories == null)
            {
                throw new Exception($"Категория не найдена {category.Name}");
            }
            foreach (var product in category.Products)
            {
                categories.Products.Add(product);
            }
            await _context.SaveChangesAsync();
            return categories;
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлени продукции в категорию");
                throw new Exception("Ошибка при добавлении продукции в категорию" + ex.Message);
            }
        }
    }
}