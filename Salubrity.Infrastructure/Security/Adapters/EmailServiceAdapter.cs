// File: Salubrity.Infrastructure/Security/Adapters/EmailServiceAdapter.cs
using Salubrity.Application.DTOs.Email;
using Salubrity.Application.Interfaces;

namespace Salubrity.Infrastructure.Security.Adapters;

public sealed class EmailServiceAdapter : IEmailService
{
    private readonly IRawEmailSender _mailer;

    public EmailServiceAdapter(IRawEmailSender mailer)
    {
        _mailer = mailer;
    }

    public Task SendAsync(EmailRequestDto request)
    {
        throw new NotImplementedException();
    }

    public Task SendBatchAsync(BatchEmailRequest request)
    {
        throw new NotImplementedException();
    }

    public Task SendCampAccessAsync(
        string toEmail,
        string displayName,
        string signInUrl,
        string tempPassword,
        DateTimeOffset expiresAt)
    {
        var subject = "Your Health Camp Access";
        var body =
$@"Hi {displayName},

Scan the QR at the venue or click the link to sign in:
{signInUrl}

Backup login (valid until {expiresAt:yyyy-MM-dd HH:mm} UTC):
Email: {toEmail}
Temp password: {tempPassword}

Thanks,
Salubrity Team";

        return _mailer.SendAsync(toEmail, subject, body);
    }

    public Task<bool> TemplateExistsAsync(string templateKey)
    {
        throw new NotImplementedException();
    }
}
