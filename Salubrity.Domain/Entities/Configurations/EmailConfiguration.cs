using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Configurations;

[Table("EmailConfigurations")]
public class EmailConfiguration : BaseAuditableEntity
{
    [Required, MaxLength(200)]
    public string FromName { get; set; } = default!;

    [Required, MaxLength(200)]
    public string FromEmail { get; set; } = default!;

    [Required, MaxLength(200)]
    public string SmtpHost { get; set; } = default!;

    public int SmtpPort { get; set; } = 587;

    [Required, MaxLength(200)]
    public string Username { get; set; } = default!;

    [Required, MaxLength(200)]
    public string Password { get; set; } = default!;

    public bool UseSsl { get; set; } = true;

    public bool EnableDebugging { get; set; } = false;

    public int MaxRetryAttempts { get; set; } = 3;

    public int RetryDelayMilliseconds { get; set; } = 1000;

    public bool IsActive { get; set; } = true;
}
