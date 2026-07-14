namespace ShopApi
{
    public class RefreshToken
    {
        public string Name {get; set;}
        public string Email {get; set;}
        public Guid Id {get; set;}
        public string AccessToken { get; set; }
        public string RefreshTokens{get; set;}
        public string Token {get; set;}
        public DateTime Expires {get; set;}
        public bool IsExpired {get; set;}
        public DateTime Created {get; set;} = DateTime.UtcNow;
        public DateTime? Revoked {get; set;}
        public string ReplacedByToken {get; set;}
        public List<string> Roles { get; set; } = new();

        public string IpAddress {get; set;}
        public string UserAgent {get; set;} 
        public bool IsActive {get; set;}

        public Guid UserId {get; set;}
        public virtual User User {get; set;}
    }
}