using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Exceptions;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ServerFeatures.DeleteRoleRequest
{
    public sealed class DeleteRole
    {
        public sealed record Command(Guid ServerId, Guid RoleId) : IRequest<Result>;

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser)
            : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var currentUserId = _currentUser.UserId;

                var (server, authority) = await _db.LoadForRoleChangeAsync(
                    request.ServerId, currentUserId, cancellationToken, allMemberships: true);

                if (server is null || authority is null)
                    return Result.Failure(ServerErrors.NotFound);

                if (!authority.Can(Permissions.Manage_Roles))
                    return Result.Failure(ServerErrors.CannotManageRoles);

                var role = server.ServerRoles.FirstOrDefault(r => r.Id == request.RoleId);
                if (role is null)
                    return Result.Failure(ServerErrors.RoleNotFound);

                if (!authority.OutranksPosition(role.Position))
                    return Result.Failure(ServerErrors.Outranked);

                try
                {
                    server.DeleteRole(request.RoleId);
                }
                catch (DomainException ex)
                {
                    return Result.Failure(new Error(
                        "Role.Invalid",
                        ex.Message,
                        StatusCodes.Status400BadRequest));
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapDelete("/{serverId:guid}/roles/{roleId:guid}", async (
                Guid serverId,
                Guid roleId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new Command(serverId, roleId), cancellationToken);

                return result.Match(
                    () => Results.NoContent(),
                    error => Results.Problem(
                        title: error.Message,
                        type: error.Code,
                        statusCode: error.StatusCode));
            })
            .WithName("DeleteServerRole");
        }
    }
}