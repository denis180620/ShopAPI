using System.Net;
using Microsoft.AspNetCore.Identity;
using MimeKit.Encodings;

namespace ShopApi
{
    public interface IServiceConfirmEmail
    {
        public  Task<Result<bool>> SendConfirmEmail(Guid UserId, string IpAddress); 
    }
    public class ServiceConfirmEmail : IServiceConfirmEmail
    {
        private readonly IEmailService _service;
        private readonly ILogger<ServiceConfirmEmail> _logger;
        private readonly IEmailConfirmToken _repository;
        private readonly UserManager<User> _userManager;
         public ServiceConfirmEmail(IEmailService service, ILogger<ServiceConfirmEmail> logger, IEmailConfirmToken repository, UserManager<User> userManager)
        {
            _logger = logger;
            _service = service;
            _repository = repository;
            _userManager = userManager;
        }
        public async Task<Result<bool>> SendConfirmEmail(Guid UserId, string IpAddress)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(UserId.ToString());
                if(user == null)
                {
                    _logger.LogWarning("Пользователь не найден");
                    return Result<bool>.Failure(404, "Пользователь не найден");
                }
                var token = Guid.NewGuid().ToString();
                var tokensemail = await _repository.CreateAsync(new EmailConfirmationToken
                {
                    UserId = user.Id,
                    User = user,
                    Token = token,
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = IpAddress,
                });
                if(tokensemail != null)
                {
                    var email = await _service.SendAsync(user.Email, token);
                    if (email.IsSuccess)
                    {
                        return Result<bool>.Success(true, "Сообщение успешно отправлено");
                    }
                    return Result<bool>.Failure(500, "Ошибка отправки сообщения");
                }
                return Result<bool>.Failure(500, "Ошибка создания токена");
            }catch(Exception ex)
            {
                _logger.LogError("Внутренняя ошибка сервера");
                throw new Exception("Внутренняя ошибка сервера" + ex.Message);
            }
        }
    }
}