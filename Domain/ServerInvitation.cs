namespace PingPong.API.Domain
{
    public class ServerInvitation
    {
        public Guid Id { get; private set; }
        public Guid ServerId { get; private set; }
        public Server Server { get; private set; } = null!;
        public Guid CreatedByUserId { get; private set; }
        public User CreatedByUser { get; private set; } = null!;
        public string Token { get; private set; } = null!;
        public DateTime ExpiresAt { get; private set; }

        private ServerInvitation() { }

        public ServerInvitation(Guid serverId, DateTime expiresAt, Guid createdByUserId)
        {
            Id = Guid.NewGuid();
            ServerId = serverId;
            ExpiresAt = expiresAt;
            Token = Guid.NewGuid().ToString();
            CreatedByUserId = createdByUserId;
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiresAt;
        }
    }
}