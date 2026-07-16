namespace PingPong.API.Domain
{
    public class Chat
    {
        public Guid Id { get; private set; }
        private readonly List<User> _Users = new List<User>();
        public IReadOnlyCollection<User> Users => _Users.AsReadOnly();
        private readonly List<Message> _Messages = new List<Message>();
        public IReadOnlyCollection<Message> Messages => _Messages.AsReadOnly();
    }
}