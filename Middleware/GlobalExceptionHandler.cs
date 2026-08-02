using Microsoft.AspNetCore.Mvc;

namespace ShopApi
{
    public class GlobalExceptionHandler : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Необработанное исключение: {Message}", ex.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { error = "Внутренняя ошибка сервера" });
            }
        }
    }
}
