using ShopApi;
using MailKit.Net.Smtp;
using MimeKit;

namespace ShopApi{
    public interface IEmailService
    {
        public Task<Result<ResponseSender>> SendAsync(string Email, string token);
        
    }
    public class EmailSerives : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSerives> _logger;

        public EmailSerives(IConfiguration configuration, ILogger<EmailSerives> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Result<ResponseSender>> SendAsync(string RecipientInfo, string content)
        {
            try
            {
                Console.WriteLine($"RecipientInfo = '{RecipientInfo}'");
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Congratulation Service",
                    _configuration["Email:From"]));
                email.To.Add(new MailboxAddress("", RecipientInfo));
                email.Subject = "Поздравление!";

                email.Body = new TextPart("html")
                {
                    Text = content
                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["Email:SmtpServer"],
                    int.Parse(_configuration["Email:Port"]),
                    MailKit.Security.SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]);

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return Result<ResponseSender>.Success(new ResponseSender { Success = true }, "Код отправлен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки сообщения");
                return Result<ResponseSender>.Failure(500, ex.Message);
            }
        }
    }
}