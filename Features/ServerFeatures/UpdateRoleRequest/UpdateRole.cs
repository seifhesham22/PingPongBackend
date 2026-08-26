using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Exceptions;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ServerFeatures.UpdateRoleRequest
{
    public sealed class UpdateRole
    {
        public sealed record UpdateRoleDto(string? name, Permissions? permissions);

        public sealed record RoleUpdatedDto(Guid id, string name, int position, Permissions permissions);

        public sealed record Command(Guid ServerId, Guid RoleId, string? Name, Permissions? Permissions)
            : IRequest<Result<RoleUpdatedDto>>;

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser)
            : IRequestHandler<Command, Result<RoleUpdatedDto>>
        {
            public async Task<Result<RoleUpdatedDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var currentUserId = _currentUser.UserId;

                var (server, authority) = await _db.LoadForRoleChangeAsync(
                    request.ServerId, currentUserId, cancellationToken);

                if (server is null || authority is null)
                    return ServerErrors.NotFound;

                if (!authority.Can(Domain.Permissions.Manage_Roles))
                    return ServerErrors.CannotManageRoles;

                var role = server.ServerRoles.FirstOrDefault(r => r.Id == request.RoleId);
                if (role is null)
                    return ServerErrors.RoleNotFound;

                if (role.IsEveryone)
                {
                    if (!authority.IsAdmin)
                        return ServerErrors.AdminOnly;
                }
                else
                {
                    var membership = server.Memberships.First(m => m.UserId == currentUserId);
                    if (membership.Roles.Any(r => r.Id == role.Id))
                        return ServerErrors.OwnRole;
                }

                if (!authority.OutranksPosition(role.Position))
                    return ServerErrors.Outranked;

                if (request.Permissions is not null)
                {
                    if ((request.Permissions.Value & ~MemberAuthority.AllDefined) != Domain.Permissions.None)
                        return ServerErrors.UnknownPermission;

                    var changed = role.Permissions ^ request.Permissions.Value;
                    if ((changed & ~authority.Ceiling) != Domain.Permissions.None)
                        return ServerErrors.PermissionNotHeld;
                }

                try
                {
                    server.UpdateRole(request.RoleId, request.Name, request.Permissions);
                }
                catch (DomainException ex)
                {
                    return new Error(
                        "Role.Invalid",
                        ex.Message,
                        StatusCodes.Status400BadRequest);
                }

                await _db.SaveChangesAsync(cancellationToken);

                return Result<RoleUpdatedDto>.Success(new RoleUpdatedDto(
                    role.Id, role.Name, role.Position, role.Permissions));
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPatch("/{serverId:guid}/roles/{roleId:guid}", async (
                Guid serverId,
                Guid roleId,
                UpdateRoleDto body,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new Command(serverId, roleId, body.name, body.permissions), cancellationToken);

                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        title: error.Message,
                        type: error.Code,
                        statusCode: error.StatusCode));
            })
            .WithName("UpdateServerRole");
        }
    }
}
