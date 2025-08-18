// File: Salubrity.Infrastructure/Services/RawSmtpEmailSender.cs
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Salubrity.Application.Interfaces;

public class RawSmtpEmailSender : IRawEmailSender
{
    public Task SendAsync(string toEmail, string subject, string plainTextBody)
    {
        var client = new SmtpClient("your-smtp-host")
        {
            Port = 587,
            Credentials = new NetworkCredential("user", "password"),
            EnableSsl = true
        };

        var message = new MailMessage("noreply@yourdomain.com", toEmail, subject, plainTextBody);

        return client.SendMailAsync(message);
    }
}
