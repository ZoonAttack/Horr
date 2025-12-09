using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Services.Interfaces;
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

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
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
    }
}