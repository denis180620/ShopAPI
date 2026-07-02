using Microsoft.AspNetCore.Identity;

namespace ShopApi
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName {get; set;}
        public ICollection<Order> Orders {get; set;} = new List<Order>();
        public virtual ICollection<RefreshToken> RefreshToekens {get; set;} = new List<RefreshToken>();
        
        public string DeliveryAddress {get; set;}

        public int BonusPoints {get; set;}
        public UserStatus Status {get; set;}
        public decimal TotalSpent {get; set;} = 0;

        public DateTime RegisterAt {get; set;} = DateTime.UtcNow;
        public DateTime LastLoginAt {get; set;}
        public enum UserStatus
        {
            Active,
            Inactive,
            Suspended
        }
      
    }
    public class Role : IdentityRole<Guid>
    {
        public string? Description { get; set; } = string.Empty;
    }
}