// File: Salubrity.Application/DTOs/Email/EmailRequest.cs
#nullable enable
using System.Collections.Generic;

namespace Salubrity.Application.DTOs.Email;

public class EmailRequestDto
{
    public required string ToEmail { get; init; }
    public required string Subject { get; init; }
    public required string TemplateKey { get; init; }
    public required object Model { get; init; }
    public string? FromEmail { get; init; }
    public string? FromName { get; init; }
    public IDictionary<string, string>? Headers { get; init; }
    public List<EmailAttachment>? Attachments { get; init; }
    public EmailPriority Priority { get; init; } = EmailPriority.Normal;
}

public class BatchEmailRequest
{
    public required IEnumerable<string> ToEmails { get; init; }
    public required string Subject { get; init; }
    public required string TemplateKey { get; init; }
    public required object Model { get; init; }
    public string? FromEmail { get; init; }
    public string? FromName { get; init; }
    public IDictionary<string, string>? Headers { get; init; }
    public List<EmailAttachment>? Attachments { get; init; }
    public EmailPriority Priority { get; init; } = EmailPriority.Normal;
}

public class EmailAttachment
{
    public required string FileName { get; init; }
    public required byte[] Content { get; init; }
    public required string ContentType { get; init; }
}

public enum EmailPriority
{
    Low = 1,
    Normal = 2,
    High = 3
}