// File: Salubrity.Infrastructure/Services/EmailService.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Salubrity.Application.DTOs.Email;
using Salubrity.Application.Interfaces;
using Salubrity.Infrastructure.Configuration;

namespace Salubrity.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        ITemplateRenderer templateRenderer,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string templateKey,
        object model,
        IDictionary<string, string>? headers = null)
    {
        await SendAsync(new[] { toEmail }, subject, templateKey, model, headers);
    }

    public async Task SendAsync(
        IEnumerable<string> toEmails,
        string subject,
        string templateKey,
        object model,
        IDictionary<string, string>? headers = null)
    {
        try
        {
            var recipients = toEmails.ToList();
            if (!recipients.Any())
            {
                _logger.LogWarning("No recipients provided for email template {TemplateKey}", templateKey);
                return;
            }

            if (!await _templateRenderer.TemplateExistsAsync(templateKey))
            {
                throw new InvalidOperationException($"Email template '{templateKey}' does not exist");
            }

            (string html, string text) = await _templateRenderer.RenderAsync(templateKey, model);
            var message = CreateMessage(recipients, subject, html, text, headers);

            await SendMessageAsync(message);

            _logger.LogInformation("Email sent successfully to {Recipients} using template {TemplateKey}",
                string.Join(", ", recipients), templateKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email using template {TemplateKey} to {Recipients}",
                templateKey, string.Join(", ", toEmails));
            throw;
        }
    }

    public Task SendAsync(EmailRequestDto request)
    {
        throw new NotImplementedException();
    }

    public Task SendBatchAsync(BatchEmailRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TemplateExistsAsync(string templateKey)
    {
        throw new NotImplementedException();
    }
    private MimeMessage CreateMessage(
        IList<string> toEmails,
        string subject,
        string htmlBody,
        string textBody,
        IDictionary<string, string>? headers)
    {
        var message = new MimeMessage();

        try
        {
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        }
        catch
        {
            // Fallback to using just the email if name is malformed
            // message.From.Add(new MailboxAddress(_settings.FromEmail));
        }

        //  Skip bad email addresses
        foreach (var email in toEmails)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) continue;

            try
            {
                message.To.Add(MailboxAddress.Parse(email));
            }
            catch
            {
                // Skip silently
            }
        }





        message.Subject = subject;

        // Add custom headers
        if (headers?.Any() == true)
        {
            foreach (var header in headers)
            {
                message.Headers.Add(header.Key, header.Value);
            }
        }

        // Create multipart body with both HTML and text
        var multipart = new Multipart("alternative")
    {
        new TextPart("plain") { Text = textBody },
        new TextPart("html") { Text = htmlBody }
    };

        message.Body = multipart;

        return message;
    }


    private async Task SendMessageAsync(MimeMessage message)
    {
        using var client = new SmtpClient();

        if (_settings.EnableDebugging)
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        }

        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, _settings.UseSsl);

        if (!string.IsNullOrEmpty(_settings.Username))
        {
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}