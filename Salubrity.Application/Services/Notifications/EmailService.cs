using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Salubrity.Application.Configurations;
using Salubrity.Application.DTOs.Email;
using Salubrity.Application.Interfaces;
using Salubrity.Domain.Entities.Configurations;

namespace Salubrity.Application.Services.Notifications;

public class EmailService : IEmailService
{
    private readonly EmailOptions _fallback;
    private readonly IEmailConfigurationRepository _emailConfigurationRepository;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailOptions> fallbackOptions,
        ITemplateRenderer templates,
        IEmailConfigurationRepository emailConfigurationRepository,
        ILogger<EmailService> log)
    {
        _fallback = fallbackOptions.Value;
        _templateRenderer = templates;
        _logger = log;
        _emailConfigurationRepository = emailConfigurationRepository;
    }

    public async Task SendAsync(EmailRequestDto request)
    {
        await SendInternalAsync(new[] { request.ToEmail }, request);
    }

    public async Task SendBatchAsync(BatchEmailRequest request)
    {
        await SendInternalAsync(request.ToEmails, new EmailRequestDto
        {
            ToEmail = string.Empty,
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

        var config = await _emailConfigurationRepository.GetActiveAsync() ?? ToEntity(_fallback);
        var retryCount = 0;
        var maxRetries = config.MaxRetryAttempts;

        while (retryCount <= maxRetries)
        {
            try
            {
                if (!await _templateRenderer.TemplateExistsAsync(request.TemplateKey))
                    throw new InvalidOperationException($"Email template '{request.TemplateKey}' does not exist");

                var renderedTemplate = await _templateRenderer.RenderAsync(request.TemplateKey, request.Model);
                var message = CreateMessage(recipients, request, renderedTemplate, config);

                await SendMessageAsync(message, config);

                _logger.LogInformation("Email sent successfully to {Recipients} using template {TemplateKey}",
                    string.Join(", ", recipients), request.TemplateKey);
                return;
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to send email (attempt {Attempt}/{MaxAttempts}) to {Recipients} using template {TemplateKey}. Retrying...",
                    retryCount, maxRetries + 1, string.Join(", ", recipients), request.TemplateKey);

                await Task.Delay(config.RetryDelayMilliseconds * retryCount);
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
        RenderedTemplate template,
        EmailConfiguration config)
    {
        var message = new MimeMessage();

        // Set From
        var fromEmail = request.FromEmail ?? config.FromEmail;
        var fromName = request.FromName ?? config.FromName;
        message.From.Add(new MailboxAddress(fromName, fromEmail));

        foreach (var email in toEmails)
        {
            message.To.Add(MailboxAddress.Parse(email));
        }

        message.Subject = request.Subject;

        message.Priority = request.Priority switch
        {
            EmailPriority.High => MessagePriority.Urgent,
            EmailPriority.Low => MessagePriority.NonUrgent,
            _ => MessagePriority.Normal
        };

        if (request.Headers?.Any() == true)
        {
            foreach (var header in request.Headers)
            {
                message.Headers.Add(header.Key, header.Value);
            }
        }

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = template.Html,
            TextBody = template.Text
        };

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

    private async Task SendMessageAsync(MimeMessage message, EmailConfiguration config)
    {
        using var client = new SmtpClient();

        if (config.EnableDebugging)
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        }

        await client.ConnectAsync(config.SmtpHost, config.SmtpPort, config.UseSsl);

        if (!string.IsNullOrEmpty(config.Username))
        {
            await client.AuthenticateAsync(config.Username, config.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private static EmailConfiguration ToEntity(EmailOptions options)
    {
        return new EmailConfiguration
        {
            FromEmail = options.FromEmail,
            FromName = options.FromName,
            SmtpHost = options.SmtpHost,
            SmtpPort = options.SmtpPort,
            Username = options.Username,
            Password = options.Password,
            UseSsl = options.UseSsl,
            EnableDebugging = options.EnableDebugging,
            MaxRetryAttempts = options.MaxRetryAttempts,
            RetryDelayMilliseconds = options.RetryDelayMilliseconds
        };
    }
}
