using System.ComponentModel.DataAnnotations.Schema;

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
        [NotMapped]

        public Permissions AllPermissions =>
            _Roles.Aggregate(Permissions.None, (all, role) => all | role.Permissions);

        [NotMapped]
        public int HighestPosition
            => _Roles.Count == 0 ? Role.EVERY_ONE_POSITION : _Roles.Max(p => p.Position);

        public static UserServer Create(Guid serverId, Guid userId)
        {
            return new UserServer
            {
                ServerId = serverId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
            };
        }

        public void AddToRole(Role role)
        {
            _Roles.Add(role);
        }

        public void RemoveFromRole(Role role)
        {
            _Roles.RemoveAll(r => r.Id == role.Id);
        }

        public bool HasPermission(Permissions permission)
        {
            var all = AllPermissions;
            return all.HasFlag(Permissions.Admin) || all.HasFlag(permission);
        }
    }
}