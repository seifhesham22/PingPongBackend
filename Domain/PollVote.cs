namespace PingPong.API.Domain
{
    public class PollVote
    {
        public Guid Id { get; set; }
        public Guid PollOptionId { get; set; }
        public PollOption PollOption { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}