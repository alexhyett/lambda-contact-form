using System;
using System.Text;
using System.Threading.Tasks;

using ContactForm.Models;

using MailKit.Net.Smtp;

using MimeKit;
using MimeKit.Text;

using Serilog;
using Serilog.Context;

using SerilogTimings.Extensions;

namespace ContactForm
{
    public class App
    {
        private readonly AppSettings _settings;
        private readonly ILogger _logger;

        public App(
            AppSettings settings,
            ILogger logger
        )
        {
            _settings = settings ??
                throw new ArgumentNullException(nameof(settings));
            _logger = logger ??
                throw new ArgumentNullException(nameof(settings));
        }

        public async Task Run(ContactRequest input)
        {
            using(LogContext.PushProperty("Email", input.Email))
            using(LogContext.PushProperty("Phone", input.Phone))
            using(LogContext.PushProperty("Website", input.Website))
            using(LogContext.PushProperty("Body", input.Body))
            {
                _logger.Information("Contact Message Received from {Name}", input.Name);
            }

            var emailBody = new StringBuilder();
            emailBody.AppendLine($"Name: {input.Name}");
            emailBody.AppendLine($"Email: {input.Email}");
            emailBody.AppendLine($"Phone: {input.Phone}");
            emailBody.AppendLine($"Website: {input.Website}");
            emailBody.AppendLine($"Message: {input.Body}");

            using(_logger.TimeOperation("Sending Email"))
            {
                await SendEmail(emailBody.ToString());
            }
        }

        public async Task SendEmail(string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Contact Form", _settings.EmailFrom));
            message.To.Add(new MailboxAddress(_settings.EmailTo));
            message.Subject = "Contact Request";
            message.Body = new TextPart(TextFormat.Text)
            {
                Text = body
            };

            using(var client = new SmtpClient())
            {
                client.Connect(_settings.Host, _settings.Port, _settings.Port == 465);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}