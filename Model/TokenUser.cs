namespace ShopApi
{
    public class RefreshToken
    {
        public int Id {get; set;}
        public string Token {get; set;}
        public DateTime Expires {get; set;}
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created {get; set;} = DateTime.UtcNow;
        public DateTime? Revoked {get; set;}
        public string ReplacedByToken {get; set;}

        public string IpAddress {get; set;}
        public string UserAgent {get; set;} 
        public bool IsActive => Revoked == null && !IsExpired;

        public Guid UserId {get; set;}
        public virtual User User {get; set;}
    }
}