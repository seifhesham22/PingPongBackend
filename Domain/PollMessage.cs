namespace PingPong.API.Domain
{
    public class PollMessage : Message
    {
        public string Title { get; set; } = null!;
        public bool IsAnonymous { get; set; }
        public bool AllowMultiple { get; set; }
        public DateTime? Deadline { get; set; }
        public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
    }
}
