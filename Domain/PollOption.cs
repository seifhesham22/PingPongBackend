namespace PingPong.API.Domain
{
    public class PollOption
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public Guid PollMessageId { get; set; }
        public PollMessage PollMessage { get; set; } = null!;
        public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
    }
}