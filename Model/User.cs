using Microsoft.AspNetCore.Identity;

namespace ShopApi
{
    public class User : IdentityUser<Guid>
    {
        public Guid UserId {get; set;}
        public string Role {get; set;}
        public string Email {get; set;}
        
    }
}