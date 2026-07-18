using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerAuth : ControllerBase
    {
        private readonly IAuthorization _auth;
        ILogger<ControllerAuth> _logger;
        public ControllerAuth(IAuthorization authorization, ILogger<ControllerAuth> logger)
        {
            _auth = authorization;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<Result<IActionResult>> CreateUserAsync([FromBody] RegisterUser user)
        {
            try{
            _logger.LogInformation("Принят запрос на создание пользователя {name}", user.FirstName);
            var result = await _auth.RegisterAsync(user);
            if(!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                SetRefreshTokenCookie(result.data.RefreshToken);
            return Result<IActionResult>.Success(Ok(result.data), result.Message ?? "Пользователь успешно создан");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPost("login")]
        public async Task<Result<IActionResult>> LoginUser([FromBody] LoginUser user)
        {
            try
            {
                _logger.LogInformation("Принят запрос на вход пользователя {name}", user.Email);
                var result = await _auth.LoginUser(user);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                SetRefreshTokenCookie(result.data.RefreshToken);
                return Result<IActionResult>.Success(Ok(result.data), result.Message ?? "Успешный вход");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка входа в систему");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPost("logout")]
        [Authorize]
        public async Task<Result<IActionResult>> LogOutAsync()
        {
            try
            {
                _logger.LogInformation("Принят запрос на выход пользователя из системы");
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Result<IActionResult>.Failure(400, "Refresh token not found");
                }
                var result = await _auth.LogOutAsync(refreshToken);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message ?? "Успешный выход");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка вызхода из системы");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPost("me")]
        [Authorize]
        public async Task<Result<IActionResult>> GetCurrentUser()
        {
            var user = User.Claims.FirstOrDefault(item => item.Type == ClaimTypes.NameIdentifier);
            if(user == null || !Guid.TryParse(user.Value, out var userId)) 
            {
                return Result<IActionResult>.Failure(403, "Invalid token");
            }
            var result = await _auth.GetCurrentUserAsync(userId);
            if (!result.IsSuccess)
            {
                return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
            }
            return Result<IActionResult>.Success(Ok(result.data), result.Message ?? "Данные пользователя получены");
        }
        [HttpPost("forgot")]
        public async Task<Result<IActionResult>> ForgotPasswordAsync([FromBody] string email)
        {
            try{
            _logger.LogInformation("Получен запрос на отправку кода подтверждения почты");
            var result = await _auth.ForgotPasswordAsync(email);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message ?? "Код подтверждения отправлен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки токена");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPost("confirm")]
        public async Task<Result<IActionResult>> ConfirmEmailAsync([FromBody] ConfirmEmailDto request)
        {
            try{
                _logger.LogInformation("Получен запрос на подтверждение токена");
                var result = await _auth.ConfirmEmailAsync(request.Email, request.Token);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message ?? "Email подтвержден");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения и проверки токена");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPost("reset")]
        public async Task<Result<IActionResult>> ResetPassword([FromBody] ResetPassword request)
        {
            try
            {
                _logger.LogInformation("Получен запрос на сброс пароля");
                var result = await _auth.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
                if (!result.IsSuccess)
                {
                    return Result<IActionResult>.Failure(result.StatusCode, result.ErrorMessage);
                }
                return Result<IActionResult>.Success(Ok(result.data), result.Message ?? "Пароль успешно сброшен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения и проверки токена");
                return Result<IActionResult>.Failure(500, "Внутренняя ошибка сервера");
            }
        }
        private void SetRefreshTokenCookie(string RefreshToken)
        {
            Response.Cookies.Append("refreshToken", RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddYears(3),
                Path = "/"
            });
        }
    }
}