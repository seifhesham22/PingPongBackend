using Microsoft.AspNetCore.Identity;
using PingPong.API.Domain;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace PingPong.API.Features.Shared
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        public Task<bool> UserExistsAsync(Guid userId);
    }
    public class CurrentUser(
        IHttpContextAccessor _contextAccessor,
        UserManager<User> _userManager) : ICurrentUser
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

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null;
        }
    }
}