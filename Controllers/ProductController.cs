using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class ProductController : ControllerBase
    {
        private readonly IServiceProduct _product;
        private ILogger<ProductController> _logger;
        public ProductController(IServiceProduct product, ILogger<ProductController> logger)
        {
            _product = product;
            _logger = logger;
        }
    
    [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ResponseProduct product)
        {
            _logger.LogInformation("Принят запрос на создание продукта");
            var currentUserId = GetUserIdFromToken();
            var result = await _product.CreateProduct(product, currentUserId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody] ResponseProduct product)
        {
            _logger.LogInformation("Принят запрос на изменение продукта {id}", product.Id);

            var currentUserId = GetUserIdFromToken();
            var isAdmin = User.IsInRole("Admin");

            var result = await _product.UpdateProduct(product, currentUserId, isAdmin);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(result);
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
            return Ok(result);
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
            return Ok(result);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProduct([FromQuery] PaginationRequest request)
        {
            _logger.LogInformation("Принят запрос на получение всех продуктов");
            var result = await _product.GetAllProduct(request);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(result);
        }

        [HttpGet("{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(Guid Id)
        {
            _logger.LogInformation("Принят запрос на получение продукта {Id}", Id);
            var result = await _product.GetProductById(Id);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(result);
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
            return Ok(result);
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