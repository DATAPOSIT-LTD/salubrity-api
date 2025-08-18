// File: Salubrity.Application/Interfaces/IRawEmailSender.cs
namespace Salubrity.Application.Interfaces;

public interface IRawEmailSender
{
    Task SendAsync(string toEmail, string subject, string plainTextBody);
}
