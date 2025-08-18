

// File: Salubrity.Application/Interfaces/Services/Notifications/IEmailService.cs

#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Salubrity.Application.DTOs.Email;

namespace Salubrity.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(EmailRequestDto request);
    Task SendBatchAsync(BatchEmailRequest request);
    Task<bool> TemplateExistsAsync(string templateKey);
}