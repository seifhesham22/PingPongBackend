namespace PingPong.API.Configuration
{
    public static class HubAuthenticationMiddleware
    {
        private const string HubPathPrefix = "/hubs";
        private const string AccessTokenQueryKey = "access_token";

        /// <summary>
        /// Moves the bearer token from the query string into the Authorization header
        /// for hub requests.
        /// </summary>
        /// <remarks>
        /// Browsers can't set headers on a WebSocket handshake, so the SignalR client
        /// sends the token as ?access_token= instead. The JWT bearer handler has an
        /// OnMessageReceived hook for this, but the Identity bearer scheme that
        /// MapIdentityApi registers does not, so the token is copied across before
        /// authentication runs and the handler sees an ordinary request.
        ///
        /// Must be registered before UseAuthentication.
        /// </remarks>
        public static IApplicationBuilder UseHubAuthentication(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var isHubRequest = context.Request.Path.StartsWithSegments(HubPathPrefix);

                // A real header always wins, so native clients that can set one
                // are unaffected.
                var hasHeader = context.Request.Headers.ContainsKey("Authorization");

                if (isHubRequest && !hasHeader &&
                    context.Request.Query.TryGetValue(AccessTokenQueryKey, out var token) &&
                    !string.IsNullOrEmpty(token))
                {
                    context.Request.Headers.Authorization = $"Bearer {token}";
                }

                await next();
            });
        }
    }
}
