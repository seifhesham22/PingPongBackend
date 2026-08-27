using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ProfileFeatures.UploadProfilePhotoRequest
{
    public sealed class UploadProfilePhoto
    {
        public sealed record Command(IFormFile file) : IRequest<Result<Guid>>;

        public const int MaxSize = 10 * 1024 * 1024;

        public sealed class Handler(IWebHostEnvironment _env,
            PingPongDbContext _db, ICurrentUser _user) : IRequestHandler<Command, Result<Guid>>
        {
            public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userId = _user.UserId;

                await _db.Files.Where(x => x.UserId == userId).ExecuteDeleteAsync();


                if (request.file.Length > MaxSize)
                    return new Error(
                        "File.TooLarge",
                        "file is too large to upload",
                        StatusCodes.Status400BadRequest);

                var uniqueName = $"{Path.GetFileName(request.file.FileName)}_{Guid.NewGuid()}";
                var uploadDirectory = Path.Combine(_env.ContentRootPath, "Storage", "uploads");
                Directory.CreateDirectory(uploadDirectory);
                var filePath = Path.Combine(uploadDirectory, uniqueName);

                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.file.CopyToAsync(stream);
                }

                var metaDate = new FileMetaData(
                    userId,
                    request.file.FileName,
                    uniqueName,
                    $"/Storage/uploads/{uniqueName}",
                    request.file.ContentType,
                    request.file.Length);

                _db.Files.Add(metaDate);
                await _db.SaveChangesAsync();

                return Result<Guid>.Success(metaDate.Id);
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/uploadFile", async (
                IFormFile file,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(file), ct);

                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        title: error.Code,
                        type: error.Message,
                        statusCode: error.StatusCode));
            }).WithName("UploadFileAvatar").DisableAntiforgery();
        }
    }
}