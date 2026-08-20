using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PingPong.API.Domain;

namespace PingPong.API.Features.Authentication
{
    public class CustomUserManager : UserManager<User>
    {
        public CustomUserManager(
            IUserStore<User> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<User>> logger)
            : base(store, optionsAccessor, passwordHasher,
                  userValidators, passwordValidators, keyNormalizer,
                  errors, services, logger) { }

        public override Task<IdentityResult> CreateAsync(User user, string password)
        {
            if(string.IsNullOrEmpty(user.UserName) || user.UserName == user.Email)
                user.UserName = GenerateCustomUserName(user.Email);

            return base.CreateAsync(user, password);
        }
        
        private string GenerateCustomUserName(string email)
        {
            var prefix = email.Split('@')[0];
            return $"{prefix}_{Guid.NewGuid().ToString("N")[..4]}";
        }

        public override async Task<User> FindByNameAsync(string userName)
        {
            if (userName.Contains('@'))
            {
                return await FindByEmailAsync(userName);
            }

            return await base.FindByNameAsync(userName);
        }
    }
}