using PingPong.API.Exceptions;

namespace PingPong.API.Domain
{
    public class Chat
    {
        public Guid Id { get; private set; }
        private readonly List<ChatMemeber> _ChatMembers = new List<ChatMemeber>();
        public IReadOnlyCollection<ChatMemeber> ChatMembers => _ChatMembers.AsReadOnly();
        private readonly List<Message> _Messages = new List<Message>();
        public IReadOnlyCollection<Message> Messages => _Messages.AsReadOnly();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public static Chat CreateDirectChat(Guid userA, Guid userB)
        {
            if(userA == userB)
                throw new DomainException("You can't DM yourself.");

            var Chat = new Chat() { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            Chat._ChatMembers.Add(new ChatMemeber(Chat.Id, userA));
            Chat._ChatMembers.Add(new ChatMemeber(Chat.Id, userB));

            return Chat;
        }

        public bool HasMember(Guid userId) => _ChatMembers.Any(m => m.UserId == userId);

        public TextMessage SendMessage(Guid senderId, string content)
        {
            if (!HasMember(senderId))
                throw new DomainException("User is not a member of this chat.");

            var message = new TextMessage(this.Id, null, senderId, content, NextNumber(), null);
            _Messages.Add(message);
            return message;
        }

        private long NextNumber() => 
            Messages.Count == 0 ? 1 : Messages.Max(m => m.Number) + 1;
    }
}