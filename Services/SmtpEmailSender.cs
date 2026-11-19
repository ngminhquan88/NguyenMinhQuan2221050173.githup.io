using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace StudentManagement.Services;

public class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var host = configuration["Email:Smtp:Host"];
        var from = configuration["Email:Smtp:FromAddress"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("Configure Email:Smtp:Host and Email:Smtp:FromAddress before sending email.");

        var port = configuration.GetValue("Email:Smtp:Port", 587);
        var username = configuration["Email:Smtp:UserName"];
        var password = configuration["Email:Smtp:Password"];
        using var message = new MailMessage(from, email, subject, htmlMessage) { IsBodyHtml = true };
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = configuration.GetValue("Email:Smtp:EnableSsl", true),
            Credentials = string.IsNullOrWhiteSpace(username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(username, password)
        };
        await client.SendMailAsync(message);
    }
}