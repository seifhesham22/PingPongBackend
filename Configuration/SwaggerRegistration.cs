using Microsoft.OpenApi;

namespace PingPong.API.Configuration
{
    public static class SwaggerRegistration
    {
        private const string BearerScheme = "Bearer";

        public static IServiceCollection AddSwaggerWithBearerAuth(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Paste the accessToken from /auth/login. The \"Bearer \" prefix is added for you.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = BearerScheme
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference(BearerScheme, document),
                        new List<string>()
                    }
                });
            });

            return services;
        }
    }
}