using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ShopApi
{
    public interface IServiceOrder
    {
        Task<Result<Order>> CreateOrder(OrderRequestDTO order);
        Task<Result<Order>> UpdateAddProductsToOrder(Guid OrderId, Guid productId, int Quantity);
        Task<Result<Order>> GetOrderAsync(Guid OrderId);
        Task<Result<List<Order>>> GetOredrs();
        Task<Result<List<Order>>> GetOrderStatus(string status);
        Task<Result<Order>> PutOrder(Order order);
        Task<Result<Order>> GetOrdersByUserId(Guid UserId);
        Task<Result<bool>> DeleteOrder(Guid OrderId);
        Task<Result<bool>> BuyOrder(Guid OrderId);
    }
    public class ServiceOrder : IServiceOrder
    {
        private readonly IOrder _order;
        private readonly IOrderItem _orderItem;
        private readonly ILogger<ServiceOrder> _logger;
        private readonly IProduct _product;
        private readonly UserManager<User> _userManager;

        public ServiceOrder(IOrder order, IOrderItem orderItem, ILogger<ServiceOrder> logger, UserManager<User> userManager, IProduct product)
        {
            _order = order;
            _orderItem = orderItem;
            _logger = logger;
            _userManager = userManager;
            _product = product;
        }
        public async Task<Result<Order>> CreateOrder(OrderRequestDTO order)
        {
            try
            {
                _logger.LogInformation("Создание заказа");
                if(order.UserId == Guid.Empty || order.ProductId == Guid.Empty || order.Quantity <= 0 )
                {
                    return Result<Order>.Failure(400, "Некорректные данные");
                }
               var product = await _product.GetProductById(order.ProductId);
               if(product == null)
                {
                    return Result<Order>.Failure(404, "Продукт не найден");
                }
                if(product.StockQuantity < order.Quantity)
                {
                    return Result<Order>.Failure(400, "Недостаточно товара на складе");
                }
                var user = await _userManager.FindByIdAsync(order.UserId.ToString());
                if(user == null)
                {
                    return Result<Order>.Failure(404, "Пользователь не найден");
                }
                var orderItem = await _orderItem.CreateItem(new OrderItem
                {
                    ProductId = order.ProductId,
                    Quantity = order.Quantity,
                    Product = product,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    DiscountPrice = product.DiscountPrice
                });
                if(orderItem == null)
                {
                    return Result<Order>.Failure(500, "Ошибка при создании позиции заказа");
                }
                var newOrder = new Order
                {
                    OrderId = Guid.NewGuid(),
                    UserId = order.UserId,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    PaidAt = DateTime.UtcNow,
                    TotalAmount =+ orderItem.TotalPrice,
                    DiscountAmount = orderItem.DiscountAmount,
                    OrderItems = new List<OrderItem> { orderItem },
                    Paymentstatus = PaymentStatus.Pending,
                    BonisPointsUsed = user.BonusPoints,
                    user = user,
                    promoCode = null
                };
                var result = await _order.CreateOrder(newOrder);
                if(result == null)
                {
                    return Result<Order>.Failure(500, "Ошибка при создании заказа");
                }
                orderItem.OrderId = result.OrderId;
                orderItem.Order = result;
                var updateOrderItem = await _orderItem.PutItem(orderItem);
                if(updateOrderItem == null)
                {
                    return Result<Order>.Failure(500, "Ошибка при обновлении позиции заказа");
                }
                return Result<Order>.Success(result, "Заказ успешно создан");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания заказа");
                return Result<Order>.Failure(500, "Ошибка создания заказа" + ex.Message);
            }
        }
        public async Task<Result<Order>> UpdateAddProductsToOrder(Guid OrderId, Guid productId, int Quantity)
        {
            try
            {
             _logger.LogInformation("Добавление продукта в заказ");
             if(OrderId == Guid.Empty || productId == Guid.Empty || Quantity <= 0)
                {
                    return Result<Order>.Failure(400, "Некорректные данные");
                }   
                var product = await _product.GetProductById(productId);
                if(product == null)
                {
                    return Result<Order>.Failure(404, "Продукт не найден");
                }
                var order = await _order.GetOrderAsync(OrderId);
                var nameproduct = order.OrderItems.FirstOrDefault(s => s.ProductId == productId);
                if(nameproduct != null)
                {
                    nameproduct.Quantity += Quantity;
                    order.TotalAmount += product.Price * Quantity;
                    order.OrderItems.Add(nameproduct);
                }
                else{
                var orderItems = await _orderItem.CreateItem(new OrderItem
                {
                    ProductId = productId,
                    Quantity = Quantity,
                    Product = product,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    OrderId = OrderId
                });
                    order.OrderItems.Add(orderItems);
                    order.TotalAmount += product.Price * Quantity;
                }
                var result = await _order.PutOrder(order);
                if(result == null)
                {
                    return Result<Order>.Failure(500, "Ошибка обновления заказа");
                }
                return Result<Order>.Success(result, "Добавлен продукт в корзину");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обнвления заказа");
                return Result<Order>.Failure(500, "Ошибка обновления заказа" + ex.Message);
            }
        }
        public async Task<Result<Order>> GetOrderAsync(Guid OrderId)
        {
            try
            {
                _logger.LogInformation("Получение заказа");
                if(OrderId == Guid.Empty)
                {
                    return Result<Order>.Failure(400, "Некорректный идентификатор заказа");
                }
                var order = await _order.GetOrderAsync(OrderId);
                if(order == null)
                {
                    return Result<Order>.Failure(404, "Заказ не найден");
                }
                return Result<Order>.Success(order, "Заказ получен");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения заказа");
                return Result<Order>.Failure(500, "Ошибка получения заказа" + ex.Message);
            }
        }

        public async Task<Result<List<Order>>> GetOredrs()
        {
            try
            {
                _logger.LogInformation("Получение списка заказов");
                var orders = await _order.GetOredrs();
                if(orders == null || orders.Count == 0)
                {
                    return Result<List<Order>>.Failure(404, "Заказы не найдены");
                }
                return Result<List<Order>>.Success(orders, "Список заказов получен");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка заказов");
                return Result<List<Order>>.Failure(500, "Ошибка получения списка заказов" + ex.Message);
            }
        }

        public async Task<Result<List<Order>>> GetOrderStatus(string status)
        {
            try
            {
                _logger.LogInformation("Получение заказов по статусу: {Status}", status);
                if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    return Result<List<Order>>.Failure(400, $"Некорректный статус: {status}. Допустимые значения: {string.Join(", ", Enum.GetNames<OrderStatus>())}");
                }

                var orders = await _order.GetOrderStatus(orderStatus);

                if(orders == null || orders.Count == 0)
                {
                    return Result<List<Order>>.Failure(404, $"Заказы со статусом {status} не найдены");
                }
                return Result<List<Order>>.Success(orders, $"Список заказов со статусом {status} получен");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения заказов по статусу");
                return Result<List<Order>>.Failure(500, "Ошибка получения заказов по статусу" + ex.Message);
            }
        }

        public async Task<Result<Order>> PutOrder(Order order)
        {
            try
            {
                _logger.LogInformation("Обновление заказа: {OrderId}", order.OrderId);
                if(order.OrderId == Guid.Empty)
                {
                    return Result<Order>.Failure(400, "Некорректный идентификатор заказа");
                }
                var result = await _order.PutOrder(order);
                if(result == null)
                {
                    return Result<Order>.Failure(500, "Ошибка при обновлении заказа");
                }
                return Result<Order>.Success(result, "Заказ успешно обновлен");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления заказа");
                return Result<Order>.Failure(500, "Ошибка обновления заказа" + ex.Message);
            }
        }

        public async Task<Result<Order>> GetOrdersByUserId(Guid UserId)
        {
            try
            {
                _logger.LogInformation("Получение заказов пользователя: {UserId}", UserId);
                if(UserId == Guid.Empty)
                {
                    return Result<Order>.Failure(400, "Некорректный идентификатор пользователя");
                }
                var order = await _order.GetOrdersByUserId(UserId);
                if(order == null)
                {
                    return Result<Order>.Failure(404, "Заказы пользователя не найдены");
                }
                return Result<Order>.Success(order, "Заказы пользователя получены");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения заказов пользователя");
                return Result<Order>.Failure(500, "Ошибка получения заказов пользователя" + ex.Message);
            }
        }

        public async Task<Result<bool>> DeleteOrder(Guid OrderId)
        {
            try
            {
                _logger.LogInformation("Удаление заказа: {OrderId}", OrderId);
                if(OrderId == Guid.Empty)
                {
                    return Result<bool>.Failure(400, "Некорректный идентификатор заказа");
                }
                var order = await _order.GetOrderAsync(OrderId);
                if(order == null)
                {
                    return Result<bool>.Failure(404, "Заказ не найден");
                }
                // Note: Repository should have a DeleteOrder method, currently using workaround
                // You may need to add DeleteOrder to IOrder interface and RepositoryOreder
                return Result<bool>.Failure(501, "Метод удаления заказа не реализован в репозитории");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления заказа");
                return Result<bool>.Failure(500, "Ошибка удаления заказа" + ex.Message);
            }
        }

        public async Task<Result<bool>> BuyOrder(Guid OrderId)
        {
            try
            {
                _logger.LogInformation("Покупка заказа: {OrderId}", OrderId);
                if(OrderId == Guid.Empty)
                {
                    return Result<bool>.Failure(400, "Некорректный идентификатор заказа");
                }
                var order = await _order.GetOrderAsync(OrderId);
                if(order == null)
                {
                    return Result<bool>.Failure(404, "Заказ не найден");
                }
                if(order.Status == OrderStatus.Cancelled)
                {
                    return Result<bool>.Failure(400, "Невозможно оплатить отмененный заказ");
                }
                if(order.Paymentstatus == PaymentStatus.Paid)
                {
                    return Result<bool>.Failure(400, "Заказ уже оплачен");
                }
                order.Paymentstatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Processing;
                order.PaidAt = DateTime.UtcNow;
                var result = await _order.PutOrder(order);
                if(result == null)
                {
                    return Result<bool>.Failure(500, "Ошибка при обновлении заказа");
                }
                return Result<bool>.Success(true, "Заказ успешно оплачен");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка оплаты заказа");
                return Result<bool>.Failure(500, "Ошибка оплаты заказа" + ex.Message);
            }
        }
    }
}