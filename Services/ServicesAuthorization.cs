using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Org.BouncyCastle.Bcpg.OpenPgp;


namespace ShopApi
{
    public interface IAuthorization
    {
        Task<Result<ResponseLoginUser>> RegisterAsync(RegisterUser user);
        Task<Result<ResponseLoginUser>> LoginUser(LoginUser user);
        Task<Result<bool>> LogOutAsync(string refreshToken);
        Task<Result<ResponseUser>> GetCurrentUserAsync(Guid UserId);
        Task<Result<RefreshToken>>RefreshTokenAsync(string refreshToken);
        Task<Result<bool>> ConfirmEmailAsync(string Email, string token);
        Task<Result<bool>> ForgotPasswordAsync(string email);
        Task<Result<bool>> ResetPasswordAsync(string Email, string token, string Password); 
    }
    public class AuthorizationServices : IAuthorization
    {
        private readonly ILogger<AuthorizationServices> _logger;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IConfiguration configuration;
        private readonly AppDbContext _context;
        private readonly User _user;
        private readonly IEmailService _emailService;
        public AuthorizationServices(ILogger<AuthorizationServices> logger, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, IConfiguration configuration, AppDbContext context, User user, IEmailService emailService)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            this.configuration = configuration;
            _context = context;
            _user = user;
            _emailService = emailService;
        }

        public async Task<Result<ResponseLoginUser>> RegisterAsync(RegisterUser user)
        {
            _logger.LogInformation("Регистрация нового пользователя: {FirstName}", user.FirstName);

            var existist = await _userManager.FindByEmailAsync(user.Email);
            if(existist != null)
            {
                return Result<ResponseLoginUser>.Failure(409, "Пользователь с таким Email существует");
            }
            var users = new User
            {
                FirstName = user.FirstName,
                Email = user.Email,
                RegisterAt = DateTime.UtcNow,
                DeliveryAddress = user.DeliveryAddress
                
            };
            var result = await _userManager.CreateAsync(users, user.Password);

            if (!result.Succeeded)
            {
                var error = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("ОШибка регистрации {Errors}", error);
                return Result<ResponseLoginUser>.Failure(500, $"Ошибка регистрации: {error}");
            }
            await _userManager.AddToRoleAsync(users, "User");

            var session = await CreateSessionAsync(users);
            _logger.LogInformation("Пользователь зарегестрирован: {Email} {UserId}", users.Email, users.Id);

            return Result<ResponseLoginUser>.Success(new ResponseLoginUser
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("JwtSetting:ExpirationMinutes", 60)),
                Email = users.Email,
                Name = users.FirstName,
                Role = new List<string> {"User"}
            }, "Пользователь успешно зарегистрирован");
        }
        public async Task<Result<ResponseLoginUser>> LoginUser(LoginUser user)
        {
            _logger.LogInformation("Вход пользователя: {Email}", user.Email);

            var users = await _userManager.FindByEmailAsync(user.Email);
            if(user == null)
            {
                _logger.LogWarning("ПОльзователь не найден: {Email}", user.Email);
                return Result<ResponseLoginUser>.Failure(404, "Неверный email или пароль");
            }
            
            if (users.Status == User.UserStatus.Suspended)
            {
                _logger.LogWarning("Пльзователь заблокирован: {Email}", users.Email);
                return Result<ResponseLoginUser>.Failure(403, "Вы заблокированны");
            }
            var isPassword = await _userManager.CheckPasswordAsync(users, user.Password);
            if (!isPassword)
            {
                _logger.LogWarning("Неверный пароль: {Email}", users.Email);
                return Result<ResponseLoginUser>.Failure(403, "Неверный email или пароль");
            }
            if (!await _userManager.IsEmailConfirmedAsync(users))
            {
                return Result<ResponseLoginUser>.Failure(403, "Подтвердите email для входа");
            }
            users.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(users);

            var session = await CreateSessionAsync(users, user.ipAddress, user.UserAgent);

            var role = await _userManager.GetRolesAsync(users);
            return Result<ResponseLoginUser>.Success(new ResponseLoginUser
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                Email = users.Email,
                Name = users.FirstName,
                BonusPoints = users.BonusPoints,
                Role = role.ToList()
            }, "Пользователь успешно вошел в систему");
        }
        public async Task<Result<bool>> LogOutAsync(string RefreshToken)
        {
            _logger.LogInformation("Выход из приложения");

            if (string.IsNullOrWhiteSpace(RefreshToken))
            {
                return Result<bool>.Failure(404,"Отсутстве токена авторизации");
            }
            var session = await _context.RefreshTokens.FirstOrDefaultAsync(s => s.RefreshTokens == RefreshToken);
            if(session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Скссия для пользователя {UserId} остановленна", session.UserId);
            }
            return Result<bool>.Success(true, "Сессия успешно завершена");
        }
        public async Task<Result<ResponseUser>> GetCurrentUserAsync(Guid UserId)
        {
            _logger.LogInformation("Запрос на получение информации о пользователе");
            var user = await _userManager.FindByIdAsync(UserId.ToString());
            if(user == null)
            {
                _logger.LogWarning("Пользователь с {UserId} не найден", UserId);
                return Result<ResponseUser>.Failure(404, "Пользователь не найден");
            }
            var role = await _userManager.GetRolesAsync(user);

            return Result<ResponseUser>.Success(new ResponseUser
            {
                Email = user.Email,
                Name = user.FirstName,
                BonusPoints = user.BonusPoints,
                Orders = user.Orders,
                DeliveryAddress = user.DeliveryAddress,
                TotalSpent = user.TotalSpent
            }, "Информация о пользователе успешно получена");
        }

        public async Task<Result<RefreshToken>> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Получен запрос на изменение токена");

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("Пусткой токен");
                return Result<RefreshToken>.Failure(404, "Передан пустой токен");
            }
            var session = await _context.RefreshTokens.Include(s => s.User).FirstOrDefaultAsync(s => s.RefreshTokens == refreshToken);
            
            if(session == null)
            {
                _logger.LogWarning("Сессия не найдена");
                return Result<RefreshToken>.Failure(404, "Сессия не найдена");
            }
            if(session.IsActive == false)
            {
                _logger.LogWarning("Сессия не активна");
                return Result<RefreshToken>.Failure(404, "Сессия не активна");
            }
            if (session.IsExpired)
            {
                _logger.LogWarning("Refresh token истек");
                session.IsActive = false;
                await _context.SaveChangesAsync();
                return Result<RefreshToken>.Failure(404, "Истек срок токена");
            }
            session.IsActive = false;
            await _context.SaveChangesAsync();

            var user = session.User;
            var newsession = await CreateSessionAsync(user);
            var role = await _userManager.GetRolesAsync(user);

            return Result<RefreshToken>.Success(new RefreshToken
            {
                Name = user.FirstName,
                AccessToken = newsession.AccessToken,
                RefreshTokens = newsession.RefreshToken,
                Email = user.Email,
                Roles = role.ToList()
            }, "Токен успешно обновлен"); 
        }
        public async Task<Result<bool>> ConfirmEmailAsync(string Email, string token)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if(user == null)
            {
                _logger.LogWarning("Пользователь по {userId} не найден", Email);
                return Result<bool>.Failure(404, "Пользователь не найден");
            }
             
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.BonusPoints += 50;
                await _userManager.UpdateAsync(user);
                return Result<bool>.Success(true, "Email подтвержден");
            }
            return Result<bool>.Failure(400, "Ошибка подтверждения email");
        }

        public async Task<Result<bool>> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if(user == null)
            {
                return Result<bool>.Success(true, "Если email существует, то код будет отправлен");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var sander = await _emailService.SendAsync(user.Email, token);
            if (!sander.IsSuccess)
            {
                _logger.LogWarning("Ошибка отправки кода");
                return Result<bool>.Failure(500, "Ошибка отправки кода, проверить Email на правильность");
            }
            return Result<bool>.Success(true, "Код отправлен на email");
        }
        public async Task<Result<bool>> ResetPasswordAsync(string Email, string token, string Password)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return Result<bool>.Failure(404, "Пользователь не найден");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, Password);

            if (result.Succeeded)
            {
                // Отзываем все сессии при смене пароля
                var sessions = await _context.RefreshTokens
                    .Where(s => s.UserId == user.Id && s.IsActive)
                    .ToListAsync();

                foreach (var session in sessions)
                {
                    session.IsActive = false;
                }

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true, "Пароль успешно сброшен");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<bool>.Failure(500, $"Ошибка сброса пароля: {errors}");
        }
        private async Task<SessionUser> CreateSessionAsync(User user, string IpAddress = null, string UserAgent = null)
        {
            var accessToken = await GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenExpirationDays = configuration.GetValue<int>("JwtSetting:RefreshTokenExpirationDays", 7);
            var accessTokenExpirationMinutes = configuration.GetValue<int>("JwtSettingd: ExpirationMinutes", 60);

            var session = new RefreshToken
            {
                UserId = user.Id,
                RefreshTokens = refreshToken,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
                IpAddress = IpAddress,
                UserAgent = UserAgent
            };
            await _context.RefreshTokens.AddAsync(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Сесия создана для пользователя: {userId}", user.Id);

            return new SessionUser
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = session.Id
            };
        }
        private async Task<string> GenerateAccessToken(User user)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(
                jwtSettings["Secret"] ?? throw new InvalidOperationException("Jwt Secret not configured")
            );

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FistName", user.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Name, user.FirstName ?? string.Empty),
                new Claim("status", user.Status.ToString()),
                new Claim("bonusPoints", user.BonusPoints.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(secretKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings ["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpirationMinutes", 60)),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rgn = RandomNumberGenerator.Create();
            rgn.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}