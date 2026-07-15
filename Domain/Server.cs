
using PingPong.API.Exceptions;

namespace PingPong.API.Domain
{
    public class Server
    {
        public Guid Id { get; private set; }
        public Guid? ServerIcon { get; private set; }
        public string Name { get; private set; } = null!;
        
        public Guid OwnerId {  get; private set; }
        public User Owner { get; private set; } = null!;
        
        private readonly List<Channel> _channels = new List<Channel>();
        public IReadOnlyCollection<Channel> Channels => _channels.AsReadOnly();

        private readonly List<ChannelGroup> _ChannelGroups = new List<ChannelGroup>();
        public IReadOnlyCollection<ChannelGroup> ChannelGroups => _ChannelGroups.AsReadOnly();

        private readonly List<UserServer> _Memberships = new List<UserServer>();
        public IReadOnlyCollection<UserServer> Memberships => _Memberships.AsReadOnly();

        private readonly List<ServerInvitation> _ServerInvitations = new List<ServerInvitation>();
        public IReadOnlyCollection<ServerInvitation> ServerInvitations => _ServerInvitations.AsReadOnly();

        private readonly List<Role> _ServerRoles = new List<Role>();
        public IReadOnlyCollection<Role> ServerRoles => _ServerRoles.AsReadOnly();

        private Server() { }

        public static Server Create(string name, Guid ownerId, Guid? serverIcon)
        {
            var server = new Server()
            {
                Id = Guid.NewGuid(),
                Name = name,
                OwnerId = ownerId,
                ServerIcon = serverIcon,
            };
            
            server._Memberships.Add(new UserServer(server.Id, ownerId));
            return server;
        }

        public UserServer AddMember(Guid userId)
        {
            if(_Memberships.Any(x => x.UserId == userId))
            {
                throw new InvalidOperationException("User is already a member of this server.");
            }

            var membership = new UserServer(this.Id, userId);
            _Memberships.Add(membership);
            return membership;
        }

        public UserServer RemoveMember(Guid userId)
        {
            var membership = _Memberships.First(x => x.UserId == userId) ??
                throw new DomainException("User is not a member of this server.");

            if(membership.UserId == this.OwnerId)
            {
                throw new DomainException("Cannot remove the owner from the server.");
            }

            _Memberships.Remove(membership);
            return membership;
        }
    }
}