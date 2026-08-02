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
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDTO category)
        {
            _logger.LogInformation("Принят запрос на создание категории");
            var result = await _category.CreateCategory(category);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] Category category)
        {
            _logger.LogInformation("Принят запрос на изменения категории");
            var result = await _category.UpdateCategory(category);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            _logger.LogInformation("Принят запрос на получение категории");
            var result = await _category.GetCategoryById(id);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            _logger.LogInformation("Принят запрос на получение всех категорий");
            var result = await _category.GetAllCategories();
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            _logger.LogInformation("Принят запрос на удаление категории");
            var result = await _category.DeleteCategory(id);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("/{name}")]
        public async Task<IActionResult> GetChildCategories(string name)
        {
            _logger.LogInformation("Принят запрос на получение дочерних категорий");
            var result = await _category.GetChildCategories(name);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message });
        }
    }
}