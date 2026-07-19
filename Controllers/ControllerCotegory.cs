using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ShopApi
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ControllerCategory : ControllerBase
    {
        private readonly IServiceCategory _category;
        private readonly ILogger<ControllerCategory> _logger;
        public ControllerCategory(IServiceCategory category, ILogger<ControllerCategory> logger)
        {
            _category = category;
            _logger = logger;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<Result<IActionResult>> CreateCategory([FromBody] CategoryDTO category)
        {
            try
            {
                _logger.LogInformation("Принят запрос на создание категории");
                var result = await _category.CreateCategory(category);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания категории");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<Result<IActionResult>> UpdateCategory([FromBody] Category category)
        {
            try
            {
                _logger.LogInformation("Принят запрос на изменния категории");
                var result = await _category.UpdateCategory(category);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка изменения категории");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<Result<IActionResult>> GetCategoryById(int id)
        {
            try
            {
                _logger.LogInformation("Принят запрос на полчение категории");
                var result = await _category.GetCategoryById(id);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения категории");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<Result<IActionResult>> GetAllCategories()
        {
            try
            {
                _logger.LogInformation("Принят запрос на полчение всех категорий");
                var result = await _category.GetAllCategories();
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения категорий");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<Result<IActionResult>> DeleteCategory(int id)
        {
            try
            {
                _logger.LogInformation("Принят запрос на удаление категории");
                var result = await _category.DeleteCategory(id);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения категорий");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("/{name}")]
        public async Task<Result<IActionResult>> GetChildCategories(string name)
        {
            try
            {
                _logger.LogInformation("Принят запрос на получение дочерних категорий");
                var result = await _category.GetChildCategories(name);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения дочерних категорий");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
    }
}