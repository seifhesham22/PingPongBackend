namespace PingPong.API.Domain
{
    public class LastReadMessage
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid? ChannelId { get; set; }
        public Guid? ChatId { get; set; }
        public long LastReadMessageNumber { get; set; }
    }
}