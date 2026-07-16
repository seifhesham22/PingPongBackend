namespace PingPong.API.Domain
{
    public class UserDeviceToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
