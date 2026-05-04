using Entities.Users;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly IConfiguration _configuration;

        public EmailService(IOptions<EmailSettings> settings, IConfiguration configuration)
        {
            _settings = settings.Value;
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlMessage)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_settings.From));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                var builder = new BodyBuilder { HtmlBody = htmlMessage };
                email.Body = builder.ToMessageBody();
                
                using var smtp = new SmtpClient();

                // 1. Connect
                await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);

                // 2. Authenticate
                await smtp.AuthenticateAsync(_settings.Username, _settings.Password);

                // 3. Send
                await smtp.SendAsync(email);

                // 4. Disconnect
                await smtp.DisconnectAsync(true);

                return true; // Success!
            }
            catch (Exception ex)
            {
                // Log the error so you know what happened
                // e.g. _logger.LogError(ex.Message);
                Console.WriteLine($"Email send failed: {ex.Message}");
                return false; // Failure
            }
        }

        public async Task<bool> SendConfirmationEmailAsync(string userId, string to, string token)
        {
            try
            {
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                // Build the confirmation link here
                var baseUrl = _configuration["FrontendBaseUrl"];
                var link = $"{baseUrl}/email-confirmed?userId={userId}&token={encodedToken}";

                string subject = "Confirm your email";
                string body = $@"
                                <h2>Email Confirmation</h2>
                                <p>Please confirm your account:</p>
                                <a href='{link}'>Confirm Email</a>";

                await SendEmailAsync(to, subject, body);
                return true;
            }
            catch(Exception ex)
            {
                // Log the error
                Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendChangeEmailAsync(string userId, string to, string token)
        {
            try
            {
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                // Build the confirmation link here
                var baseUrl = _configuration["FrontendBaseUrl"];
                var link = $"{baseUrl}/change-email?userId={userId}&newEmail={to}&token={encodedToken}";
                string subject = "Confirm your new email";
                string body = $@"
                                <h2>Email Change Confirmation</h2>
                                <p>Please confirm your new email address:</p>
                                <a href='{link}'>Confirm Email Change</a>";
                await SendEmailAsync(to, subject, body);
                return true;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Failed to send change email: {ex.Message}");
                return false;
            }
        }

    }
}
