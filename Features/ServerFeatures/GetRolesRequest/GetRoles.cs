using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ServerFeatures.GetRolesRequest
{
    public sealed class GetRoles
    {
        public sealed record RoleDto(
            Guid id,
            string name,
            int position,
            bool isEveryone,
            Permissions permissions,
            string[] permissionNames,
            bool canEdit,
            bool isMine);

        public sealed record RolesDto(
            bool isOwner,
            bool isAdmin,
            bool canManageRoles,
            int myPosition,
            Permissions grantablePermissions,
            string[] grantablePermissionNames,
            RoleDto[] roles);

        public sealed record Query(Guid ServerId) : IRequest<Result<RolesDto>>;

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser)
            : IRequestHandler<Query, Result<RolesDto>>
        {
            public async Task<Result<RolesDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var currentUserId = _currentUser.UserId;

                var authority = await _db.GetAuthorityAsync(
                    request.ServerId, currentUserId, cancellationToken);

                if (authority is null)
                    return ServerErrors.NotFound;

                var myRoles = (await _db.UserServers
                    .AsNoTracking()
                    .Where(m => m.ServerId == request.ServerId && m.UserId == currentUserId)
                    .SelectMany(m => m.Roles.Select(r => r.Id))
                    .ToListAsync(cancellationToken)).ToHashSet();

                var canManageRoles = authority.Can(Permissions.Manage_Roles);

                var roles = await _db.ServerRoles
                    .AsNoTracking()
                    .Where(r => r.ServerId == request.ServerId)
                    .OrderByDescending(r => r.Position)
                    .Select(r => new { r.Id, r.Name, r.Position, r.IsEveryone, r.Permissions })
                    .ToListAsync(cancellationToken);

                var roleDtos = roles
                    .Select(r => new RoleDto(
                        id: r.Id,
                        name: r.Name,
                        position: r.Position,
                        isEveryone: r.IsEveryone,
                        permissions: r.Permissions,
                        permissionNames: MemberAuthority.Expand(r.Permissions),
                        canEdit: authority.CanEditRole(
                            r.IsEveryone, r.Position, myRoles.Contains(r.Id)),
                        isMine: myRoles.Contains(r.Id)))
                    .ToArray();

                return Result<RolesDto>.Success(new RolesDto(
                    isOwner: authority.IsOwner,
                    isAdmin: authority.IsAdmin,
                    canManageRoles: canManageRoles,
                    myPosition: authority.HighestPosition,
                    grantablePermissions: authority.Ceiling,
                    grantablePermissionNames: MemberAuthority.Expand(authority.Ceiling),
                    roles: roleDtos));

            }

        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/{serverId:guid}/roles", async (
                Guid serverId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new Query(serverId), cancellationToken);

                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        title: error.Message,
                        type: error.Code,
                        statusCode: error.StatusCode));
            })
            .WithName("GetServerRoles");
        }
    }
}