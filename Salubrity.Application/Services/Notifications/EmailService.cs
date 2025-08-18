using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Salubrity.Application.Configurations;
using Salubrity.Application.DTOs.Email;
using Salubrity.Application.Interfaces;

namespace Salubrity.Application.Services.Notifications;

public class EmailService : IEmailService
{
    private readonly EmailOptions _settings;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> options,
                        ITemplateRenderer templates,
                        ILogger<EmailService> log)
    {
        _settings = options.Value;
        _templateRenderer = templates;
        _logger = log;
    }

    public async Task SendAsync(EmailRequestDto request)
    {
        await SendInternalAsync(new[] { request.ToEmail }, request);
    }

    public async Task SendBatchAsync(BatchEmailRequest request)
    {
        await SendInternalAsync(request.ToEmails, new EmailRequestDto
        {
            ToEmail = string.Empty, // Not used in batch
            Subject = request.Subject,
            TemplateKey = request.TemplateKey,
            Model = request.Model,
            FromEmail = request.FromEmail,
            FromName = request.FromName,
            Headers = request.Headers,
            Attachments = request.Attachments,
            Priority = request.Priority
        });
    }

    public async Task<bool> TemplateExistsAsync(string templateKey)
    {
        return await _templateRenderer.TemplateExistsAsync(templateKey);
    }

    private async Task SendInternalAsync(IEnumerable<string> toEmails, EmailRequestDto request)
    {
        var recipients = toEmails.ToList();
        if (!recipients.Any())
        {
            _logger.LogWarning("No recipients provided for email template {TemplateKey}", request.TemplateKey);
            return;
        }

        var retryCount = 0;
        var maxRetries = _settings.MaxRetryAttempts;

        while (retryCount <= maxRetries)
        {
            try
            {
                if (!await _templateRenderer.TemplateExistsAsync(request.TemplateKey))
                {
                    throw new InvalidOperationException($"Email template '{request.TemplateKey}' does not exist");
                }

                var renderedTemplate = await _templateRenderer.RenderAsync(request.TemplateKey, request.Model);
                var message = CreateMessage(recipients, request, renderedTemplate);

                await SendMessageAsync(message);

                _logger.LogInformation("Email sent successfully to {Recipients} using template {TemplateKey}",
                    string.Join(", ", recipients), request.TemplateKey);

                return; // Success, exit retry loop
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to send email (attempt {Attempt}/{MaxAttempts}) to {Recipients} using template {TemplateKey}. Retrying...",
                    retryCount, maxRetries + 1, string.Join(", ", recipients), request.TemplateKey);

                await Task.Delay(_settings.RetryDelayMilliseconds * retryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email after {MaxAttempts} attempts to {Recipients} using template {TemplateKey}",
                    maxRetries + 1, string.Join(", ", recipients), request.TemplateKey);
                throw;
            }
        }
    }

    private MimeMessage CreateMessage(
        IList<string> toEmails,
        EmailRequestDto request,
        RenderedTemplate template)
    {
        var message = new MimeMessage();

        // Set From address
        var fromEmail = request.FromEmail ?? _settings.FromEmail;
        var fromName = request.FromName ?? _settings.FromName;
        message.From.Add(new MailboxAddress(fromName, fromEmail));

        // Set To addresses
        foreach (var email in toEmails)
        {
            message.To.Add(MailboxAddress.Parse(email));
        }

        message.Subject = request.Subject;

        // Set priority
        message.Priority = request.Priority switch
        {
            EmailPriority.High => MessagePriority.Urgent,
            EmailPriority.Low => MessagePriority.NonUrgent,
            _ => MessagePriority.Normal
        };

        // Add custom headers
        if (request.Headers?.Any() == true)
        {
            foreach (var header in request.Headers)
            {
                message.Headers.Add(header.Key, header.Value);
            }
        }

        // Create body
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = template.Html,
            TextBody = template.Text
        };

        // Add attachments
        if (request.Attachments?.Any() == true)
        {
            foreach (var attachment in request.Attachments)
            {
                bodyBuilder.Attachments.Add(
                    attachment.FileName,
                    attachment.Content,
                    ContentType.Parse(attachment.ContentType));
            }
        }

        message.Body = bodyBuilder.ToMessageBody();
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