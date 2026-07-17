namespace PingPong.API.Domain
{
    public class TextMessage : Message
    {
        public string Text { get; private set; } = null!;
        public DateTime? EditedAt { get; private set; }

        public const int MaxLength = 4000;

        private TextMessage() { }

        public TextMessage(Guid? channelId, Guid? chatId, Guid authorId, string text, long number, Guid? replyToId)
    : base(channelId, chatId, authorId, number, replyToId)
        {
            Text = text;
        }
    }
}