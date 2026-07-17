namespace PingPong.API.Domain
{
    public sealed class ChatMemeber
    {
        public Guid Id { get; private set; }
        public Guid ChatId { get; private set; }
        public Chat Chat { get; private set; } = null!;
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;
        public DateTime JoinedAt { get; private set; }
        public bool IsMuted { get; private set; }

        private ChatMemeber() { }
        public ChatMemeber(Guid chatId, Guid userId)
        {
            Id = Guid.NewGuid();
            ChatId = chatId;
            UserId = userId;
            JoinedAt = DateTime.UtcNow;
            IsMuted = false;
        }
        public void ToggleMute() => IsMuted = !IsMuted;
    }
}
