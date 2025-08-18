namespace Salubrity.Application.Configurations;

public sealed class EmailOptions
{

    public required string SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string FromEmail { get; init; }
    public required string FromName { get; init; }
    public bool EnableDebugging { get; init; } = false;
    public int MaxRetryAttempts { get; init; } = 3;
    public int RetryDelayMilliseconds { get; init; } = 1000;
    public string TemplatesPath { get; init; } = "EmailTemplates";
}
