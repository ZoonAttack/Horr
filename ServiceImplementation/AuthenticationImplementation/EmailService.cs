using Entities.Users;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Resend;
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
        private readonly string _apiKey = string.Empty;

        private IResend _resendClient;
        public EmailService(IOptions<EmailSettings> settings, IConfiguration configuration)
        {
            _settings = settings.Value;
            _configuration = configuration;
            _apiKey = _configuration["ResendAPIKey"];
            InitializeResendClient();
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlMessage)
        {
            try
            {
                var message = new EmailMessage();
                message.From = "Acme <onboarding@resend.dev>";
                message.To.Add(to);
                message.Subject = subject;
                message.HtmlBody = htmlMessage;

                await _resendClient.EmailSendAsync(message);
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
                var link = $"{baseUrl}/api/Auth/confirm-email?userId={userId}&token={encodedToken}";

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
                var link = $"{baseUrl}/api/Auth/change-email?userId={userId}&newEmail={to}&token={encodedToken}";
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



        private void InitializeResendClient()
        {
            var options = new ResendClientOptions
            {
                ApiToken = _apiKey
            };
            _resendClient = ResendClient.Create(options);
        }
    }
}
