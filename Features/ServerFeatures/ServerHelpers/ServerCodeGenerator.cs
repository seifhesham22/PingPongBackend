using System.Security.Cryptography;

namespace PingPong.API.Features.ServerFeatures.ServerHelpers
{
    public static class ServerCodeGenerator
    {
        public static string GenerateJoinCode(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789";

            return string.Create(length, chars, (buffer, chars) =>
            {
                for(int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
                }
            });
        }
    }
}