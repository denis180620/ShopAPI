using System.ComponentModel.DataAnnotations;

namespace ShopApi
{
    public class EmailConfirmationToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public bool IsValid => !IsUsed && ExpiresAt > DateTime.UtcNow;

        // Для отслеживания
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}