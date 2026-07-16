namespace PingPong.API.Domain
{
    public class TextChannel : Channel
    {
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}