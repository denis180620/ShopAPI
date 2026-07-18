namespace ShopApi
{
    public class RegisterUser
    {
        public string FirstName {get; set;}
        public string Password {get; set;}
        public string DeliveryAddress {get; set;}
        public string Email {get; set;}

    }
    public class LoginUser
    {
        public string Email {get; set;}
        public string Password {get; set;}
        public string ipAddress {get; set;}
        public string UserAgent {get; set;}
    }
    public class ResetPassword
    {
        public string Email {get; set;}
        public string Token {get; set;}
        public string NewPassword {get; set;}
    }
    public class SessionUser
    {
        public string RefreshToken {get; set;}
        public string AccessToken {get; set;}
        public Guid UserId {get; set;}
    }
    public class ResponseLoginUser
    {
        public string AccessToken {get; set;}
        public string RefreshToken {get; set;}
        public string Name {get; set;}
        public string Email {get; set;}
        public int BonusPoints{get; set;}
        public List<string> Role {get; set;}
        public DateTime Expires {get; set;}
    }
    public class ResponseUser
    {
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public string DeliveryAddress { get; set; }

        public int BonusPoints { get; set; }
        public decimal TotalSpent { get; set; } = 0;
        public string Email {get; set;}

    }
    public class ResetPasswordDto
    {
        public string newPassword {get; set;}
        public string Token {get; set;}
    }
    public class ConfirmEmailDto
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
    }
    public class ResponseSender
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}