using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ShopApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IServiceOrder _order;
        private readonly ILogger<OrderController> _logger;
        public OrderController(IServiceOrder order, ILogger<OrderController> logger)
        {
            _order = order;
            _logger = logger;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDTO order)
        {  
                _logger.LogInformation("Принят запрос на создание заказа");
                order.UserId = GetUserIdFromToken();
                var result = await _order.CreateOrder(order);
                if(!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(result);
        }
        [Authorize]
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(Guid OrderId, [FromBody] RequestUpdateOrderProduct requestUpdate)
        {
            
                _logger.LogInformation("Принят запрос на редактирование заказа");
                var result = await _order.UpdateAddProductsToOrder(OrderId, requestUpdate.ProductId, requestUpdate.Quantity);
                if (!result.IsSuccess)
                {
                return BadRequest(result.ErrorMessage);
                }
                return Ok(result); 
        } 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderId(Guid Id)
        {   
                _logger.LogInformation("Принят запрос на получение заказа");
                var result = await _order.GetOrderAsync(Id);
                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(result);  
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("users")]
        public async Task<IActionResult> GetOrders()
        {
                _logger.LogInformation("Принят запрос получение всех заказов");
                var result = await _order.GetOredrs();
                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(result);            
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("status")]
        public async Task<ActionResult> GetStatusResult(string status)
        {
           
            
                _logger.LogInformation("Принят запрос на получение заказов по статусу: {Status}", status);

                var result = await _order.GetOrderStatus(status);
                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(result);
            
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut]
        public async Task<IActionResult> PutOrder([FromBody] Order order)
        {
                _logger.LogInformation("Принят запрос на изменения заказа");
                var result = await _order.PutOrder(order);
                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(result);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrderUserId()
        {
                _logger.LogInformation("Принят запрос получение заказов клиента");
                var userId = GetUserIdFromToken();
                var result = await _order.GetOrdersByUserId(userId);
                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(result);   
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid Id)
        {                       
                _logger.LogInformation("Принят запрос на удаление заказа");
                var result = await _order.DeleteOrder(Id);
                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(result);           
        }
        [Authorize]
        [HttpPost("by-order/{id}")]
        public async Task<IActionResult> BuyOrder(Guid Id)
        { 
                _logger.LogInformation("Получен запрос на оплату заказа");
                var result = await _order.BuyOrder(Id);
                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(result);
        }
        [Authorize]
        [HttpDelete("delete/{OrderId}")]
        public async Task<IActionResult> DeleteProductOrder(Guid OrderId, [FromBody] Guid ProductId)
        {
            _logger.LogInformation("Принят запрос на удаление продукта из заказа");
            var result = await _order.DeleteProductOrder(OrderId, ProductId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result);
        }
        [Authorize(Roles ="Admin,Manager")]
        [HttpPatch("{orderId}")]
        public async Task<IActionResult> PutStatusOrder(Guid orderId, [FromBody] UpdateStatusOrder status)
        {
            _logger.LogInformation("Принят запрос на изменение статуса заказа");
            var result = await _order.PutOrderStatus(orderId, status.Status);
            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
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