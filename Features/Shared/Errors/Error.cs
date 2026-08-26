namespace PingPong.API.Features.Shared
{
    public sealed record Error(string Code, string Message, int StatusCode = 400);
}