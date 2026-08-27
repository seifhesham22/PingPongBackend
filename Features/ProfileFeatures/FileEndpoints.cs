using PingPong.API.Features.ProfileFeatures.UploadProfilePhotoRequest;

namespace PingPong.API.Features.ProfileFeatures
{
    public static class FileEndpoints
    {
        public static void MapProfileEndpoints(this WebApplication app)
        {
            var ProfileGroup = app.MapGroup("/profile")
                .WithTags("profile")
                .RequireAuthorization();

            UploadProfilePhoto.MapEndpoint(ProfileGroup);
        }
    }
}
