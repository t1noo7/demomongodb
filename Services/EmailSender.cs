using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using DemoMongoDB.Models;


// public class EmailSender : IEmailSender
// {
//     private readonly EmailSettings _emailSettings;

//     public EmailSender(IOptions<EmailSettings> emailSettings)
//     {
//         _emailSettings = emailSettings.Value;
//     }

//     public async Task SendEmailAsync(string toEmail, string subject, string message)
//     {
//         var email = new MimeMessage();
//         email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
//         email.To.Add(new MailboxAddress("", toEmail));
//         email.Subject = subject;
//         email.Body = new TextPart(TextFormat.Html) { Text = message };

//         using var client = new SmtpClient();
//         await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
//         await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
//         await client.SendAsync(email);
//         await client.DisconnectAsync(true);
//     }
// }

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        emailMessage.To.Add(new MailboxAddress("",toEmail));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(TextFormat.Html) { Text = message };

        using (var client = new SmtpClient())
        {
            try
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
                await client.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }
    }
}
