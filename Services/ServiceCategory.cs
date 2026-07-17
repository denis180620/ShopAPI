namespace ShopApi
{
    public interface IServiceCategory
    {
        Task<Result<bool>> CreateCategory(CategoryDTO category);
        Task<Result<Category>> GetCategoryById(int id);
        Task<Result<List<Category>>> GetAllCategories();
        Task<Result<Category>> UpdateCategory(Category category);
        Task<Result<bool>> DeleteCategory(int id);
        Task<Result<Category>> GetChildCategories(string name);
        Task<Result<Category>> UpdateProduct(Product product);
    }
    public class ServiceCategory : IServiceCategory
    {
        private readonly ICategory _category;
        private readonly ILogger<ServiceCategory> _logger;
        public ServiceCategory(ICategory category, ILogger<ServiceCategory> logger)
        {
            _category = category;
            _logger = logger;
        }
        public async Task<Result<bool>> CreateCategory(CategoryDTO category)
        {
            try{
            _logger.LogInformation("Создание категории");
            if(category.Name == null || category.Description == null || category.NameEn == null)
            {
                return Result<bool>.Failure(400, "Некорректные данные");
            }
            if(category.ParentCategoryId != null)
            {
                var parentCategory = await _category.GetCategoryById((int)category.ParentCategoryId);
                if(parentCategory == null)
                {
                    return Result<bool>.Failure(404, "Родительская категория не найдена");
                }
            
            var newCategory = new Category
            {
                Name = category.Name,
                Description = category.Description,
                NameEn = category.NameEn,
                ParentCategoryId = parentCategory.ParentCategoryId,
                NameRu = category.Name,
                DisplayOrder = 0
            };

            var result = await _category.CreateCategory(newCategory);

            if(result == null)
                {
                    return Result<bool>.Failure(500, "Ошибка при создании категории");
                }

            parentCategory.ChildCategories.Add(newCategory);
            await _category.UpdateCategory(parentCategory);
            return Result<bool>.Success(true, "Категория успешно создана");
            }
            else
            {
                var newCategory = new Category
                {
                    Name = category.Name,
                    Description = category.Description,
                    NameEn = category.NameEn,
                    ParentCategoryId = null,
                    NameRu = category.Name,
                    DisplayOrder = 0
                };
                var result = await _category.CreateCategory(newCategory);
                if(result == null)
                {
                    return Result<bool>.Failure(500, "Ошибка при создании категории");
                }
                return Result<bool>.Success(true, "Категория успешно создана");
            }
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании категории");
                return Result<bool>.Failure(500, "Ошибка при создании категории" + ex.Message);
            }
        }
        public async Task<Result<Category>> GetCategoryById(int id)
        {
            try{
            _logger.LogInformation("Получение категории по идентификатору");
            if(id <= 0)
            {
                return Result<Category>.Failure(400, "Некорректный индефекатор категории");
            }
            var category = await _category.GetCategoryById(id);
            if(category == null)
            {
                return Result<Category>.Failure(400, "Не корректный индефекатор категории, убедитесь что категория существует");
            }
            return Result<Category>.Success(category, "Категория успешно получена");
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении категории");
            return Result<Category>.Failure(500, "Ошибка при получении категории" + ex.Message);
        }
        }
        public async Task<Result<List<Category>>> GetAllCategories()
        {
            try
            {
                _logger.LogInformation("Получение всех категорий");
                var categories = await _category.GetAllCategories();
                if(categories == null || categories.Count == 0)
                {
                    return Result<List<Category>>.Failure(404, "Категории не найдены");
                }
                return Result<List<Category>>.Success(categories, "Категории успешно получены");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех категорий");
                return Result<List<Category>>.Failure(500, "Ошибка при получении всех категорий" + ex.Message);
            }
        }
        public async Task<Result<Category>> UpdateCategory(Category category)
        {
            try
            {
                _logger.LogInformation("Обновление категории");
                if(category == null)
                {
                    return Result<Category>.Failure(400, "Некорректные данные");
                }
                var categories = await _category.GetCategoryById(category.Id);
                if(categories == null)
                {
                    return Result<Category>.Failure(400, "Не корректно переданны данные, категории возможно не существует. Убедитесь в ее существовании или создаете новую категории(обратитесь к администратору)");
                }
                categories.Name = category.Name;
                categories.Description = category.Description;
                categories.NameEn = category.NameEn;
                categories.NameRu = category.NameRu;
                categories.DisplayOrder = category.DisplayOrder;
                var result = await _category.UpdateCategory(categories);
                if(result == null)
                {
                    return Result<Category>.Failure(500, "Ошибка при обновлении категории");
                }
                return Result<Category>.Success(result, "Категория успешно обновлена");
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении категории");
                return Result<Category>.Failure(500, "Ошибка при обновлении категории" + ex.Message);
            }
        }
        public async Task<Result<bool>> DeleteCategory(int id)
        {
            try
            {
                _logger.LogInformation("Удаление категории");
                if(id <= 0)
                {
                    return Result<bool>.Failure(400, "Некорректный индефекатор категории");
                }
                var category = await _category.GetCategoryById(id);
                if(category == null)
                {
                    return Result<bool>.Failure(404, "Категория не найдена");
                }
                var result = await _category.DeleteCategory(category);
                if(!result)
                {
                    return Result<bool>.Failure(500, "Ошибка при удалении категории");
                }
                return Result<bool>.Success(true, "Категория успешно удалена");
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении категории");
                return Result<bool>.Failure(500, "Ошибка при удалении категории" + ex.Message);
            }
        }
        public async Task<Result<Category>> GetChildCategories(string name)
        {
            try
            {
                _logger.LogInformation("Получение дочерних категорий");
                if(string.IsNullOrEmpty(name))
                {
                    return Result<Category>.Failure(400, "Некорректные данные");
                }
                var category = await _category.GetChildCategories(name);
                if(category == null)
                {
                    return Result<Category>.Failure(404, "Категория не найдена");
                }
                return Result<Category>.Success(category, "Категория успешно получена");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении дочерних категорий");
                return Result<Category>.Failure(500, "Ошибка при получении дочерних категорий" + ex.Message);
            }
        }
        public async Task<Result<Category>> UpdateProduct(Product product)
        {
            try
            {
                _logger.LogInformation("Обновление продукта в категории");
                if(product == null)
                {
                    return Result<Category>.Failure(400, "Некорректные данные");
                }
                var result = await _category.UpdateProduct(product);
                if(result == null)
                {
                    return Result<Category>.Failure(500, "Ошибка при обновлении продукта в категории");
                }
                return Result<Category>.Success(result, "Продукт успешно обновлен в категории");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении продукта в категории");
                return Result<Category>.Failure(500, "Ошибка при обновлении продукта в категории" + ex.Message);
            }
        }
    }
}