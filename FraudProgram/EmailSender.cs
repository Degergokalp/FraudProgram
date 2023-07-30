using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string fromAddress, string fromPassword, string toAddress, string subject, string body)
    {
        // Create a new MimeMessage
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("", fromAddress));
        message.To.Add(new MailboxAddress("", toAddress));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = body;
        message.Body = bodyBuilder.ToMessageBody();

        using (var smtpClient = new SmtpClient())
        {
            await smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(fromAddress, fromPassword);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }
    }
}
