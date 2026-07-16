namespace PingPong.API.Domain
{
    public class TextMessage : Message
    {
        public string Text { get; set; } = null!;
        public DateTime? EditedAt { get; set; }
    }
}