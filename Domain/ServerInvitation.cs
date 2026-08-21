using PingPong.API.Features.ServerFeatures.ServerHelpers;

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

        public ServerInvitation(Guid serverId, Guid createdByUserId)
        {
            ServerId = serverId;
            ExpiresAt = DateTime.UtcNow.AddHours(2);
            Token = ServerCodeGenerator.GenerateJoinCode();
            CreatedByUserId = createdByUserId;
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiresAt;
        }
    }
}