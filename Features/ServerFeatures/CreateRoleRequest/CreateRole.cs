using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Exceptions;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ServerFeatures.CreateRoleRequest
{
    public sealed class CreateRole
    {
        public sealed record CreateRoleDto(string roleName, Permissions permissions);
        public sealed record RoleCreatedDto(Guid id, string name, int position, Permissions permissions);

        public sealed record Command(Guid ServerId, string RoleName, Permissions Permissions)
            : IRequest<Result<RoleCreatedDto>>;

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser)
            : IRequestHandler<Command, Result<RoleCreatedDto>>
        {
            public async Task<Result<RoleCreatedDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var currentUserId = _currentUser.UserId;

                var (server, authority) = await _db.LoadForRoleChangeAsync(
                    request.ServerId, currentUserId, cancellationToken);

                if (server is null || authority is null)
                    return ServerErrors.NotFound;

                if (!authority.Can(Permissions.Manage_Roles))
                    return ServerErrors.CannotManageRoles;

                if (MemberAuthority.HasUndefinedBits(request.Permissions))
                    return ServerErrors.UnknownPermission;

                if (authority.Exceeding(request.Permissions) != Permissions.None)
                    return ServerErrors.PermissionNotHeld;

                Role role;
                try
                {
                    role = server.CreateRole(
                        request.RoleName,
                        request.Permissions,
                        authority.HighestPosition,
                        authority.IsOwner);
                }
                catch (DomainException ex)
                {
                    return new Error(
                        "Role.Invalid",
                        ex.Message,
                        StatusCodes.Status400BadRequest);
                }

                await _db.SaveChangesAsync(cancellationToken);

                return Result<RoleCreatedDto>.Success(new RoleCreatedDto(
                    role.Id, role.Name, role.Position, role.Permissions));
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/{serverId:guid}/roles", async (
                Guid serverId,
                CreateRoleDto body,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new Command(serverId, body.roleName, body.permissions), cancellationToken);

                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        title: error.Message,
                        type: error.Code,
                        statusCode: error.StatusCode));
            })
            .WithName("CreateServerRole");
        }
    }
}