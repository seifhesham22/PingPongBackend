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

        public ICollection<Guid> TaggedUsers { get; set; } = new List<Guid>();
        public ICollection<Guid> TaggedRoles { get; set; } = new List<Guid>();

        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
