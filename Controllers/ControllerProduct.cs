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
        public async Task<IActionResult> CreateProduct([FromBody] ResponseProduct product)
        {
            _logger.LogInformation("Принят запрос на создание продукта");
            var result = await _product.CreateProduct(product);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody] ResponseProduct product)
        {
            _logger.LogInformation("Принят запрос на изменение продукта {id}", product.Id);
            var result = await _product.UpdateProduct(product);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }
        [HttpPut("{Id}/{quantity}")]
        public async Task<IActionResult> GetAddQuantityProduct(Guid Id, int quantity)
        {
            _logger.LogInformation("Принят запрос на добавление количества продукта");
            var result = await _product.GetAddQuantityProduct(Id, quantity);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }
        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminProduct()
        {
            _logger.LogInformation("Принят запрос на получение продуктов администратора");
            var userId = GetUserIdFromToken();
            var result = await _product.GetAdministratorProduct(userId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProduct([FromQuery] PaginationRequest request)
        {
            _logger.LogInformation("Принят запрос на получение всех продуктов");
            var result = await _product.GetAllProduct(request);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetProductById(Guid Id)
        {
            _logger.LogInformation("Принят запрос на получение продукта");
            var result = await _product.GetProductById(Id);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteProductId(Guid Id)
        {
            _logger.LogInformation("Принят запрос на удаление продукта");
            var result = await _product.DeleteProduct(Id);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
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