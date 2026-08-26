using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;

namespace PingPong.API.Features.Shared
{
    public static class ServerQueries
    {
        public static async Task<(Server? Server, MemberAuthority? Authority)> LoadForRoleChangeAsync(
            this PingPongDbContext db,
            Guid serverId,
            Guid userId,
            CancellationToken cancellationToken,
            bool allMemberships = false)
        {
            var query = db.Servers
                .Include(s => s.ServerRoles)
                .AsSingleQuery();

            query = allMemberships
                ? query.Include(s => s.Memberships).ThenInclude(m => m.Roles)
                : query.Include(s => s.Memberships.Where(m => m.UserId == userId))
                       .ThenInclude(m => m.Roles);

            var server = await query.FirstOrDefaultAsync(s => s.Id == serverId, cancellationToken);

            if (server is null)
                return (null, null);

            var authority = MemberAuthority.From(server, userId);
            return authority is null ? (null, null) : (server, authority);
        }

        public static async Task<MemberAuthority?> GetAuthorityAsync(
            this PingPongDbContext db,
            Guid serverId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var membership = await db.UserServers
                .AsNoTracking()
                .Where(m => m.ServerId == serverId && m.UserId == userId)
                .Select(m => new
                {
                    IsOwner = m.Server.OwnerId == userId,
                    Roles = m.Roles.Select(r => new { r.Permissions, r.Position }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (membership is null)
                return null;

            return new MemberAuthority(
                IsOwner: membership.IsOwner,
                Permissions: membership.Roles.Aggregate(
                    Permissions.None, (all, r) => all | r.Permissions),
                HighestPosition: membership.Roles.Count == 0
                    ? Role.EVERY_ONE_POSITION
                    : membership.Roles.Max(r => r.Position));
        }
    }
}