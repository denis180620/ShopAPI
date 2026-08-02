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
        public async Task<IActionResult> CreateUserAsync([FromBody] RegisterUser user)
        {
            _logger.LogInformation("Принят запрос на создание пользователя {name}", user.FirstName);
            var result = await _auth.RegisterAsync(user);
            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            SetRefreshTokenCookie(result.Data.RefreshToken);
            return Ok(new { data = result.Data, message = result.Message ?? "Пользователь успешно создан" });
        }

        [HttpPost("create-manager")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateManagerAsync([FromBody] RegisterUser user)
        {
            _logger.LogInformation("Принят запрос на создание менеджера {name} от администратора", user.FirstName);

            // Принудительно устанавливаем роль Manager
            user.Role = "Manager";

            var result = await _auth.RegisterAsync(user);
            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message ?? "Менеджер успешно создан" });
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUser user)
        {
            _logger.LogInformation("Принят запрос на вход пользователя {name}", user.Email);
            var result = await _auth.LoginUser(user);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            SetRefreshTokenCookie(result.Data.RefreshToken);
            return Ok(new { data = result.Data, message = result.Message ?? "Успешный вход" });
        }
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogOutAsync()
        {
            _logger.LogInformation("Принят запрос на выход пользователя из системы");
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { error = "Refresh token not found" });
            }
            var result = await _auth.LogOutAsync(refreshToken);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message ?? "Успешный выход" });
        }
        [HttpPost("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = User.Claims.FirstOrDefault(item => item.Type == ClaimTypes.NameIdentifier);
            if(user == null || !Guid.TryParse(user.Value, out var userId))
            {
                return StatusCode(403, new { error = "Invalid token" });
            }
            var result = await _auth.GetCurrentUserAsync(userId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message ?? "Данные пользователя получены" });
        }
        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] string email)
        {
            _logger.LogInformation("Получен запрос на отправку кода подтверждения почты");
            var result = await _auth.ForgotPasswordAsync(email);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message ?? "Код подтверждения отправлен" });
        }
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailDto request)
        {
            _logger.LogInformation("Получен запрос на подтверждение токена");
            var result = await _auth.ConfirmEmailAsync(request.Email, request.Token);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message ?? "Email подтвержден" });
        }
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword request)
        {
            _logger.LogInformation("Получен запрос на сброс пароля");
            var result = await _auth.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }
            return Ok(new { data = result.Data, message = result.Message ?? "Пароль успешно сброшен" });
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