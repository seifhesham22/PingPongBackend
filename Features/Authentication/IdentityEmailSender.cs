using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using PingPong.API.Domain;

namespace PingPong.API.Features.Authentication
{
    public class IdentityEmailSender : IEmailSender<User>
    {
        private readonly IConfiguration _configs;

        public IdentityEmailSender(IConfiguration configs)
        {
            _configs = configs;
        }
        public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
        {
            return SendEmailAsync(email,
                "Confirm your email",
                $"Please confirm your email by clicking this link: {confirmationLink}");
        }

        public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
        {
            return SendEmailAsync(email,
                "Reset your password",
                $"Your password reset code is: {resetCode}");
        }

        public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
        {
            throw new NotImplementedException();
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var settings = _configs.GetSection("MailSettings");

            var message = new MimeMessage();
            var fromAddress = new MailboxAddress(settings["FromName"], settings["UserName"]!);
            message.From.Add(fromAddress);
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"<p>{body}</p>";
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(settings["Host"], int.Parse(settings["Port"]!), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(settings["UserName"]!, settings["Password"]!);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
