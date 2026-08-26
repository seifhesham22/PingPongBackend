using PingPong.API.Domain;

namespace PingPong.API.Features.Shared
{
    public sealed record MemberAuthority(
        bool IsOwner,
        Permissions Permissions,
        int HighestPosition)
    {
        public static readonly Permissions AllDefined =
            Enum.GetValues<Permissions>().Aggregate(Permissions.None, (all, p) => all | p);

        public static MemberAuthority? From(Server server, Guid userId)
        {
            var membership = server.Memberships.FirstOrDefault(m => m.UserId == userId);
            if (membership is null)
                return null;

            return new MemberAuthority(
                IsOwner: server.OwnerId == userId,
                Permissions: membership.AllPermissions,
                HighestPosition: membership.HighestPosition);
        }

        public bool IsAdmin => IsOwner || Permissions.HasFlag(Domain.Permissions.Admin);

        public bool Can(Permissions permission) =>
            IsOwner
            || Permissions.HasFlag(Permissions.Admin)
            || Permissions.HasFlag(permission);

        public Permissions Ceiling =>
            IsOwner || Permissions.HasFlag(Permissions.Admin)
                ? AllDefined
                : Permissions;

        public Permissions Exceeding(Permissions requested) => requested & ~Ceiling;

        public bool OutranksPosition(int position) => IsOwner || HighestPosition > position;

        public bool CanEditRole(bool isEveryone, int position, bool holdsRole)
        {
            if (!Can(Domain.Permissions.Manage_Roles))
                return false;

            if (isEveryone)
                return IsAdmin;

            if (holdsRole)
                return false;

            return OutranksPosition(position);
        }

        public static string[] Expand(Permissions permissions) =>
            Enum.GetValues<Permissions>()
                .Where(p => p != Permissions.None && permissions.HasFlag(p))
                .Select(p => p.ToString())
                .ToArray();

        public static bool HasUndefinedBits(Permissions requested) =>
            (requested & ~AllDefined) != Permissions.None;
    }
}