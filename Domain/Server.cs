
using PingPong.API.Exceptions;
using System.Runtime.CompilerServices;

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

            var everyOne = Role.CreateEveryOne(server.Id);
            server._ServerRoles.Add(everyOne);

            var owner = UserServer.Create(server.Id, ownerId);
            owner.AddToRole(everyOne);

            server._Memberships.Add(owner);
            return server;
        }

        public UserServer AddMember(Guid userId)
        {
            if(IsMember(userId))
            {
                throw new DomainException("User is already a member of this server.");
            }

            var membership = UserServer.Create(this.Id, userId);
            
            var everyOneRole = _ServerRoles.First(x => x.IsEveryone == true);

            membership.AddToRole(everyOneRole);
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

        public ServerInvitation CreateInvitation(Guid createdByUserId)
        {
            if(!IsMember(createdByUserId))
                throw new DomainException("User is not a member of this server.");

            return new ServerInvitation(this.Id, createdByUserId);
        }

        public void AddChannelGroup(ChannelGroup group)
        {
            if (group.ServerId != Id)
                throw new DomainException("This group doesn't belong to the server");

            if (_ChannelGroups.Any(g => g.Id == group.Id))
                throw new DomainException("You already have this channel group");

            _ChannelGroups.Add(group);
        }

        public UserServer AcceptInvitation(Guid userId, string InvitationToken)
        {
            var invitation = _ServerInvitations.FirstOrDefault(x => x.Token == InvitationToken)
                ?? throw new DomainException("Couldn't find an invitation with this id");

            if (invitation.IsExpired())
                throw new DomainException("This invitation has expired");

            var userServer = AddMember(userId);
            return userServer;
        }
        private bool IsMember(Guid userId)
        {
            if (_Memberships.Any(x => x.UserId == userId))
                return true;

            return false;
        }
    }
}