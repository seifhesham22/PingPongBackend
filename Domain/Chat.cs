using PingPong.API.Exceptions;

namespace PingPong.API.Domain
{
    public class Chat
    {
        public Guid Id { get; private set; }
        private readonly List<ChatMember> _ChatMembers = new List<ChatMember>();
        public IReadOnlyCollection<ChatMember> ChatMembers => _ChatMembers.AsReadOnly();
        private readonly List<Message> _Messages = new List<Message>();
        public IReadOnlyCollection<Message> Messages => _Messages.AsReadOnly();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public long LastMessageNumber { get; private set; }
        public DateTime LastMessageAt { get; private set; } = DateTime.UtcNow;

        public static Chat CreateDirectChat(Guid userA, Guid userB)
        {
            if(userA == userB)
                throw new DomainException("You can't DM yourself.");

            var now = DateTime.UtcNow;
            var Chat = new Chat() { Id = Guid.NewGuid(), CreatedAt = now, LastMessageAt = now };
            Chat._ChatMembers.Add(new ChatMember(Chat.Id, userA));
            Chat._ChatMembers.Add(new ChatMember(Chat.Id, userB));

            return Chat;
        }

        public bool HasMember(Guid userId) => _ChatMembers.Any(m => m.UserId == userId);

        public TextMessage SendMessage(Guid senderId, string content)
        {
            if (!HasMember(senderId))
                throw new DomainException("User is not a member of this chat.");

            if (string.IsNullOrWhiteSpace(content))
                throw new DomainException("A message can't be empty.");

            if (content.Length > TextMessage.MaxLength)
                throw new DomainException($"A message can't exceed {TextMessage.MaxLength} characters.");

            LastMessageNumber++;
            LastMessageAt = DateTime.UtcNow;

            var message = new TextMessage(
                channelId: null,
                chatId: Id,
                authorId: senderId,
                text: content,
                number: LastMessageNumber,
                replyToId: null);

            _Messages.Add(message);
            return message;
        }
    }
}