using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ServerFeatures.GenerateServerInviteLink
{
    public sealed class GenerateLink
    {
        public sealed record Command(Guid serverId) : IRequest<Result<string>>;
        public sealed class Handler(ICurrentUser _currentUser, PingPongDbContext _db) : IRequestHandler<Command, Result<string>>
        {
            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var currentUserId = _currentUser.UserId;

                
                var currentUserExist = await _currentUser.UserExistsAsync(currentUserId);
                if (!currentUserExist)
                    return new Error(
                        "User.NotFound",
                        "couldn't find a user with this id",
                        StatusCodes.Status404NotFound);

                
                var server = await _db.Servers
                    .Include(s => s.Memberships)
                    .FirstOrDefaultAsync(s => s.Id == request.serverId, cancellationToken);

                if (server is null)
                    return new Error(
                        "Server.NotFound",
                        "Server not found",
                        StatusCodes.Status404NotFound);


                
                var invitation = server.CreateInvitation(currentUserId);
                _db.Add(invitation);

                await _db.SaveChangesAsync(cancellationToken);
                return Result<string>.Success(invitation.Token);
            }
        }

        public static void MapEndpoints(RouteGroupBuilder group)
        {
            group.MapPost("{id:guid}/generate", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id), ct);

                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        title: error.Code,
                        type: error.Message,
                        statusCode: error.StatusCode));

            }).WithName("GenerateInviatationLink");
        }
    }
}