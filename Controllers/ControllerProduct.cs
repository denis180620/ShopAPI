using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class ControllerProduct : ControllerBase
    {
        private readonly IServiceProduct _product;
        private ILogger<ControllerProduct> _logger;
        public ControllerProduct(IServiceProduct product, ILogger<ControllerProduct> logger)
        {
            _product = product;
            _logger = logger;
        }
    
    [HttpPost]
        public async Task<Result<IActionResult>> CreateProduct([FromBody] ResponseProduct product)
        {
            try
            {
                _logger.LogInformation("Принят запрос на создании продукта");
                var result = await _product.CreateProduct(product);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания продукции");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPut]
        public async Task<Result<IActionResult>> UpdateProduct([FromBody] ResponseProduct product)
        {
            try
            {
                _logger.LogInformation("ПРинят запрос на измения продукта {id}", product.Id);
                var result = await _product.UpdateProduct(product);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания продукции");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPut("{Id}/{quantity}")]
        public async Task<Result<IActionResult>> GetAddQuantutyProduct(Guid Id, int quantity)
        {
            try
            {
                _logger.LogInformation("Принят запрос на добовление количества продукции");
                var result = await _product.GetAddQuantityProduct(Id, quantity);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления продукции");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpGet]
        public async Task<Result<IActionResult>> GetAdminProduct()
        {
            try
            {
                _logger.LogInformation("Принят запрос на получение продукци");
                var userId = GetUserIdFromToken();
                var result = await _product.GetAdministratorProduct(userId);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения продукции");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpGet("{Id}")]
        public async Task<Result<IActionResult>> GetProductById(Guid Id)
        {
            try
            {
                _logger.LogInformation("Принят запрос на получение продукта");
                var result = await _product.GetProductById(Id);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения продукции");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpDelete("{Id}")]
        public async Task<Result<IActionResult>> DeleteProductId(Guid Id)
        {
            try
            {
                _logger.LogInformation("Принят запрос на удаление продукта");
                var result = await _product.DeleteProduct(Id);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления продукции");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }

        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("UserId not found is token");
            return userId;
        }
    }
}