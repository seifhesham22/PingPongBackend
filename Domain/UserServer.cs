namespace PingPong.API.Domain
{
    public class UserServer
    { 
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;
        public Guid ServerId { get; private set; }
        public Server Server { get; private set; } = null!;

        public DateTime JoinedAt { get; private set; }
        private readonly List<Role> _Roles = new List<Role>();
        public IReadOnlyCollection<Role> Roles => _Roles.AsReadOnly();

        public static UserServer Create(Guid serverId, Guid userId)
        {
            return new UserServer
            {
                Id = Guid.NewGuid(),
                ServerId = serverId,
                UserId = userId
            };
        }
    }
}