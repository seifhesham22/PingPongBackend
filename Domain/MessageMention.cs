namespace PingPong.API.Domain
{
    public class MessageMention
    {
        public Guid Id { get; private set; }
        public Guid MessageId { get; private set; }
        public Message Message { get; private set; } = null!;
        public Guid? UserId { get; private set; }
        public Guid? RoleId { get; private set; }
        internal static MessageMention User(Guid messageId, Guid userId) => new(messageId, userId, null);
        internal static MessageMention Role(Guid messageId, Guid roleId) => new(messageId, null, roleId);
        private MessageMention() { }

        private MessageMention(Guid messageId, Guid? userId, Guid? roleId)
        {
            Id = Guid.NewGuid();
            MessageId = messageId;
            UserId = userId;
            RoleId = roleId;
        }
    }
}