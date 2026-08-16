namespace ShopApi
{
    public interface IServiceProduct
    {
        Task<Result<bool>> CreateProduct(ResponseProduct product, Guid currentUserId);
        Task<Result<bool>> UpdateProduct(ResponseProduct product, Guid currentUserId, bool isAdmin);
        Task<Result<PaginationResponse<Product>>> GetAllProduct(PaginationRequest request);
        Task<Result<Product>> GetProductById(Guid id);
        Task<Result<List<Product>>> GetProductCategoryById(Guid Id);
        Task<Result<bool>> DeleteProduct(Guid id);
        Task<Result<Product>> GetAddQuantityProduct(Guid id, int quantity);
        Task<Result<List<Product>>> GetAdministratorProduct(Guid UserId);
    }
    public class ServiceProduct : IServiceProduct
    {
        private readonly ILogger<ServiceProduct> _logger;
        private readonly ICategory _category;
        private readonly IProduct _product;
        public ServiceProduct(ICategory category, ILogger<ServiceProduct> logger, IProduct product)
        {
            _category = category;
            _logger = logger;
            _product = product;
        }
        /// <summary>
        /// Создание продукта, создание продукта возможно только с ролью Admin Managaer
        /// </summary>
        /// <param name="product"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Result<bool>> CreateProduct(ResponseProduct product, Guid currentUserId)
        {
            try{
            _logger.LogInformation("Принят запрос на создание продукции {Name}", product.Name);
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                return Result<bool>.Failure(400, "Пустое имя продукта");
            }
            if(string.IsNullOrWhiteSpace(product.NameEn))
            {
                return Result<bool>.Failure(400, "ПОстое имя продукта на русском или английском языке");
            }
            if(string.IsNullOrWhiteSpace(product.DescriptionEn)  && string.IsNullOrWhiteSpace(product.Description))
            {
                return Result<bool>.Failure(400, "Отсутствует описание продукта");
            }
            if(product.Price < product.DiscountPrice)
            {
                return Result<bool>.Failure(400, "Скидка превышает стоимтость товара");
            }
            if(product.StockQuantity < 0)
            {
                return Result<bool>.Failure(400, "Количество товара должно быть больше 1");
            }
            if (product.CategoryId == Guid.Empty)
            {
                return Result<bool>.Failure(400, "Не указано имя категории, создание продукта не возможно");
            }
            var category = await _category.GetCategoryById(product.CategoryId);
            if(category == null)
            {
                return Result<bool>.Failure(400, "Не корректный индитифекатор категории товара");
            }
            var Resultproduct = new Product
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Name = product.Name,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Description = product.Description,
                StockQuantity = product.StockQuantity,
                ImageURL = product.ImageURL,
                CategoryId = product.CategoryId,
                // Category не устанавливаем - связь создаётся через CategoryId
                NameRu = product.Name,
                NameEn = product.NameEn,
                DescriptionRu = product.Description,
                DescriptionEn = product.DescriptionEn,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _product.CreateProduct(Resultproduct);
            if(result == null)
            {
                return Result<bool>.Failure(500, "Ошибка создания продукта повторите попытку позже или обратитесь к администратору");
            }

            // Связь продукта с категорией создаётся автоматически через CategoryId
            // EF Core загрузит продукты при обращении к category.Products

            return Result<bool>.Success(true, $"Продукт создан для посмотра товара {product.Name} перейдите в каталог товара");
        }
        catch(Exception ex)
            {
                _logger.LogError("Произошла ошибка сервера" + ex.Message);
                throw new Exception("Внутрення ошибка сервера"+ ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="currentUserId"></param>
        /// <param name="isAdmin"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Result<bool>> UpdateProduct(ResponseProduct product, Guid currentUserId, bool isAdmin)
        {
            try
            {
                _logger.LogInformation("Принят запрос на создание продукции {Name}", product.Name);
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    return Result<bool>.Failure(400, "Пустое имя продукта");
                }
                if (string.IsNullOrWhiteSpace(product.NameEn))
                {
                    return Result<bool>.Failure(400, "Постое имя продукта на русском или английском языке");
                }
                if (string.IsNullOrWhiteSpace(product.DescriptionEn) && string.IsNullOrWhiteSpace(product.Description))
                {
                    return Result<bool>.Failure(400, "Отсутствует описание продукта");
                }
                if (product.Price < product.DiscountPrice)
                {
                    return Result<bool>.Failure(400, "Скидка превышает стоимость товара");
                }
                if (product.StockQuantity < 2)
                {
                    return Result<bool>.Failure(400, "Количество товара должно быть больше 1");
                }
                if (product.CategoryId == Guid.Empty)
                {
                    return Result<bool>.Failure(400, "Не указано имя категории, создание продукта не возможно");
                }
                var category = await _category.GetCategoryById(product.CategoryId);
                if (category == null)
                {
                    return Result<bool>.Failure(400, "Не корректный индитифекатор категории товара");
                }
                var existingProduct = await _product.GetProductById(product.Id);
                if(existingProduct == null)
                {
                    return Result<bool>.Failure(400, "Некорректный номер продукта");
                }

                _logger.LogInformation("Обновление продукта: Текущее имя='{CurrentName}', новое имя='{NewName}'",
                    existingProduct.Name, product.Name);

                if(!isAdmin || existingProduct.UserId != currentUserId)
                {
                    return Result<bool>.Failure(403, "Нет прав доступа на измения продукта");
                }

                existingProduct.Name = product.Name;
                existingProduct.NameRu = product.Name;
                existingProduct.NameEn = product.NameEn;
                existingProduct.Price = product.Price;
                existingProduct.DiscountPrice = product.DiscountPrice;
                existingProduct.Description = product.Description;
                existingProduct.DescriptionRu = product.Description;
                existingProduct.DescriptionEn = product.DescriptionEn;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.CategoryId = product.CategoryId;
              //  existingProduct.Category = category;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                var result = await _product.UpdateProduct(existingProduct);
                if(result == null)
                {
                    return Result<bool>.Failure(500, "Ошибка обновления продукта, для обновления продукта обратитесь к администратору");
                }
                return Result<bool>.Success(true, "Обновление продукта прошел успешно");
            }
            catch (Exception ex)
            {
                _logger.LogError("Произошла ошибка сервера" + ex.Message);
                throw new Exception("Внутрення ошибка сервера" + ex.Message);
            }
        }
        /// <summary>
        /// Получение всех существующий продуктов с пагинацией 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Result<PaginationResponse<Product>>> GetAllProduct(PaginationRequest request)
        {
            try
            {
                _logger.LogInformation("Получен запрос на получение продуктов по фильтрам");
                if(request.PageNumber > 0 && request.PageSize > 0)
                {
                    if(request.PageSize > 100)
                    {
                        return Result<PaginationResponse<Product>>.Failure(400, "Превышенно максимальное количество продукции на одной странице");
                    }
                }
                if(request.minPrice > request.maxPrice)
                {
                    return Result<PaginationResponse<Product>>.Failure(400, "Минимальная цена не может быть больше максимальной цены");

                }
                if(request.maxPrice == 0)
                {
                    request.maxPrice = decimal.MaxValue;
                }
                if (string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchRequest = new PaginationRequest
                    {
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                        SortBy = request.SortBy,
                        SortDescending = request.SortDescending,
                        CategoryId = request.CategoryId,
                        minPrice = request.minPrice,
                        maxPrice = request.maxPrice
                    };
                    var result = await _product.GetProductsAsync(searchRequest);
                    if(result.Items == null || result.TotalCount == 0)
                    {
                        _logger.LogWarning("Продукты по указынным фильтрам не найдены");
                        return Result<PaginationResponse<Product>>.Failure(404, "Продукты по указанным фильтрам не найдены");
                    }
                    var responses = new PaginationResponse<Product>(result.Items, result.TotalCount, request.PageNumber, request.PageSize);
                    return Result<PaginationResponse<Product>>.Success(responses, "Продукты успешно получены");
                }
                var SearshRequestWithTerm = new PaginationRequest
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SortBy = request.SortBy,
                    SortDescending = request.SortDescending,
                    CategoryId = request.CategoryId,
                    minPrice = request.minPrice,
                    maxPrice = request.maxPrice,
                    SearchTerm = request.SearchTerm
                };
                var results = await _product.GetProductsPaginated(SearshRequestWithTerm);
                if(results.Items == null || results.TotalCount == 0)
                {
                    _logger.LogWarning("Продукты по указынным фильтрам не найдены");
                    return Result<PaginationResponse<Product>>.Failure(404, "Продукты по указанным фильтрам не найдены");
                }
                var response = new PaginationResponse<Product>(results.Items, results.TotalCount, request.PageNumber, request.PageSize);
                return Result<PaginationResponse<Product>>.Success(response, "Продукты успешно получены");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении продуктов: {Message}", ex.Message);
                _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {InnerMessage}", ex.InnerException.Message);
                    _logger.LogError("Inner Stack Trace: {InnerStackTrace}", ex.InnerException.StackTrace);
                }
                throw;
            }
        }
        /// <summary>
        /// Получение продуктов из конкретной категории
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<Result<List<Product>>> GetProductCategoryById(Guid Id)
        {
            _logger.LogInformation("ПРинят запрос на получение продукта из категории по индетефекатору {id}", Id);
            if(Id == Guid.Empty)
            {
                return Result<List<Product>>.Failure(400, "Некорректный индефекатор категории");
            }
            var products = await _product.GetProductCategoryById(Id);
            if(products == null)
            {
                return Result<List<Product>>.Failure(404, "Продуктов по указанному индетейикатору не найдено");
            }
            return Result<List<Product>>.Success(new List<Product> { products }, "Продукт успешно получен");
        }
        /// <summary>
        /// Получение подробной информации о продукте
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Result<Product>> GetProductById(Guid id)
        {
            _logger.LogInformation("Принят запрос на получение продукта по индитифекатору {id}", id);
            if(id == Guid.Empty)
            {
                return Result<Product>.Failure(400, "Некорректный индефекатор продукта");
            }
            var product = await _product.GetProductById(id);
            if(product == null)
            {
                return Result<Product>.Failure(404, "Продукт по указанному индетейикатору не найден");
            }
            return Result<Product>.Success( product , "Продукт успешно получен");
        }
        /// <summary>
        /// Удаление продукта
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Result<bool>> DeleteProduct(Guid id)
        {
            _logger.LogInformation("Принят запрос на удаление продукта по индетефикатору {id}", id);
            if(id == Guid.Empty)
            {
                return Result<bool>.Failure(400, "Некорректный индефекатор продукта");
            }
            var product = await _product.GetProductById(id);
            if(product == null)
            {
                return Result<bool>.Failure(404, "Продукт по указанному индетейикатору не найден");
            }
            var result = await _product.DeleteProduct(id);
            if (!result)
            {
                return Result<bool>.Failure(500, "Ошибка при удалении продукта, обратитесь к администратору");
            }

            return Result<bool>.Success(true, "Продукт успешно удален");
        }
        /// <summary>
        /// Добавление количества к продукту 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<Result<Product>> GetAddQuantityProduct(Guid id, int quantity)
        {
            _logger.LogInformation("Принят запрос на добаление количества продукта по индефукатору {id}", id);
            if(id == Guid.Empty)
            {
                return Result<Product>.Failure(400, "Некорректный индетефикатор продукта");
            }
            if(quantity < 1)
            {
                return Result<Product>.Failure(400, "Количество добавляемого продукта должго быть больше 1");
            }
            var product = await _product.GetProductById(id);
            if(product == null)
            {
                return Result<Product>.Failure(404, "Не корректно указанный индефекатор продукта");
            }
            product.StockQuantity += quantity;
            var result = await _product.UpdateProduct(product);
            if(result == null)
            {
                return Result<Product>.Failure(500, "Ошибка при добавлении продукции, обратитесь к администратору");
            }
            return Result<Product>.Success(result, "Количество продукта успешно обновлено");
        }
        /// <summary>
        /// Получение созданных продуктов менеджером и админом
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<Result<List<Product>>> GetAdministratorProduct(Guid UserId)
        {
            _logger.LogInformation("Принят запрос на получание созданных администратором продуктов по индефекатору {UserId}", UserId);
            if(UserId == Guid.Empty)
            {
                return Result<List<Product>>.Failure(400, "Некорректный индефекатор пользователя");
            }
            var products = await _product.GetAdministratorProduct(UserId);
            if(products == null || products.Count == 0)
            {
                return Result<List<Product>>.Failure(404, "Продуктов созданных вами еще не создана, для создания продукта перейдите в каталог и нажмите кнопку создать продукт");
            }
            return Result<List<Product>>.Success(products, "Продукты успешно получены");
        }
    }
}