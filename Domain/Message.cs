using System.Net.Mail;

namespace PingPong.API.Domain
{
    public abstract class Message
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;
        public Guid? ChannelId { get; set; }
        public TextChannel? Channel { get; set; }
        public Guid? ChatId { get; set; }
        public Chat? Chat { get; set; }
        public long Number { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? ReplyToId { get; set; }
        public Message? ReplyTo { get; set; }
        //public ICollection<MessageMention> Mentions { get; set; } = new List<MessageMentions>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        protected Message() { }

        protected Message(
            Guid? channelId,
            Guid? chatId,
            Guid authorId,
            long number,
            Guid? replyToId)
        {
            if((channelId is not null && chatId is null) || (channelId is null && chatId is not null))
            {
                throw new ArgumentException("A message must belong to either a channel or a chat, but not both.");
            }

            Id = Guid.NewGuid();
            ChannelId = channelId;
            ChatId = chatId;
            AuthorId = authorId;
            Number = number;
            CreatedAt = DateTime.UtcNow;
            ReplyToId = replyToId;
        }

        public bool IsDeleted => DeletedAt.HasValue;
    }
}