using Microsoft.EntityFrameworkCore;

namespace ShopApi
{
    public interface IEmailConfirmToken
    {
        Task<EmailConfirmationToken> CreateAsync(EmailConfirmationToken token);
        Task<EmailConfirmationToken> UpdateAsync(EmailConfirmationToken token);
        Task<bool> DeleteAsync(EmailConfirmationToken token);
        Task<EmailConfirmationToken> GetTokenAsync(Guid UserId);
    }    
    public class EmailConfirmToken : IEmailConfirmToken
    {
        private readonly AppDbContext _context;
        private ILogger<EmailConfirmationToken> _logger;
        public EmailConfirmToken(AppDbContext context, ILogger<EmailConfirmationToken> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<EmailConfirmationToken> CreateAsync(EmailConfirmationToken token)
        {
            try
            {
                await _context.EmailConfirmationTokens.AddAsync(token);
                await _context.SaveChangesAsync();
                return token;
            }catch(Exception ex)
            {
                _logger.LogError(ex + "Произошла ошибка записи в бд");
                throw new Exception("Ошибка записи в бд" + ex.Message);
            }
        }
        public async Task<EmailConfirmationToken> UpdateAsync(EmailConfirmationToken token)
        {
            try
            {
                _context.EmailConfirmationTokens.Update(token);
                await _context.SaveChangesAsync();
                return token;
            }catch(Exception ex)
            {
                _logger.LogError(ex + "Произошла ошибка обновления бд");
                throw new Exception("Ошибка обновления бд" + ex.Message);
            }
        }
        public async Task<bool> DeleteAsync(EmailConfirmationToken token)
        {
            try
            {
            _context.EmailConfirmationTokens.Remove(token);
            await _context.SaveChangesAsync();
            return true;
            }catch(Exception ex){
                _logger.LogError(ex + "Произошла ошибка обновления бд");
                throw new Exception("Ошибка обновления бд" + ex.Message);
            }
        }
        public async Task<EmailConfirmationToken> GetTokenAsync(Guid UserId)
        {
            try
            {
                var result = await _context.EmailConfirmationTokens.FirstOrDefaultAsync(s => s.UserId == UserId);
                return result;
            }catch(Exception ex)
            {
                _logger.LogError(ex + "Произошла ошибка поиска в  бд");
                throw new Exception("Ошибка поиска в бд" + ex.Message);
            }
        }
    }
}