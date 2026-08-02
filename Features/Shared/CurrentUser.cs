using System.Security.Claims;

namespace PingPong.API.Features.Shared
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
    }
    public class CurrentUser(IHttpContextAccessor _contextAccessor) : ICurrentUser
    {
        public Guid UserId
        {
            get
            {
                var value = _contextAccessor.HttpContext?.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(value, out var userId) ? userId :
                    throw new InvalidOperationException("User ID claim is missing or invalid.");
            }
        }
    }
}