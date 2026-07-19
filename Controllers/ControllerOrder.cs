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
        [HttpPost]
        public async Task<Result<IActionResult>> CreateOrder([FromBody] OrderRequestDTO order)
        {
            try
            {
                _logger.LogInformation("Принят запрос на создание заказа");
                var result = await _order.CreateOrder(order);
                if(!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания заказа");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPut]
        public async Task<Result<IActionResult>> UpdateOrder([FromBody] Guid OrderId, Guid productId, int Quantity)
        {
            try
            {
                _logger.LogInformation("Принят запрос на редоктирование заказа");
                var result = await _order.UpdateAddProductsToOrder(OrderId, productId, Quantity);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка редоктрования заказа");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        } 
        [HttpGet("{id}")]
        public async Task<Result<IActionResult>> GetOrderId(Guid Id)
        {
            try
            {
                _logger.LogInformation("Принят запрос на получение заказа");
                var result = await _order.GetOrderAsync(Id);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения заказа");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<Result<IActionResult>> GetOrders()
        {
            try
            {
                _logger.LogInformation("Принят запрос получение всех заказов");
                var result = await _order.GetOredrs();
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения всех заказов");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("status")]
        public async Task<Result<IActionResult>> GetStatusResult(string status)
        {
            try
            {
                _logger.LogInformation("Принят запрос на получение заказов по статусу: {Status}", status);

                var result = await _order.GetOrderStatus(status);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message ?? $"Заказы со статусом {status} получены");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения заказов по статусу");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut]
        public async Task<Result<IActionResult>> PutOrder([FromBody] Order order)
        {
            try
            {
                _logger.LogInformation("ПРинят запрос на изменения заказа");
                var result = await _order.PutOrder(order);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления заказа");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize]
        [HttpGet]
        public async Task<Result<IActionResult>> GetOrderUserId()
        {
            try
            {
                _logger.LogInformation("Принят запрос получение заказов клиента");
                var userId = GetUserIdFromToken();
                var result = await _order.GetOrdersByUserId(userId);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения заказов");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpDelete("{id}")]
        public async Task<Result<IActionResult>> DeleteOrder(Guid Id)
        {
            try
            {
                _logger.LogInformation("Принят запрос на удаление заказа");
                var result = await _order.DeleteOrder(Id);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления заказа");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [Authorize]
        [HttpGet("/{id}")]
        public async Task<Result<IActionResult>> BuyOrder(Guid Id)
        {
            try
            {
                _logger.LogInformation("Получен запрос на оплату заказа");
                var result = await _order.BuyOrder(Id);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка оплаты заказа");
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