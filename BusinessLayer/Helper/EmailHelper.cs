using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;
namespace BusinessLayer.Helper;

public class EmailHelper
{
     private const string SmtpServer = "mail.etatvasoft.com";
        private const int SmtpPort = 587;
        private const string SmtpUser = "test.dotnet@etatvasoft.com";
        private const string SmtpPassword = "P}N^{z-]7Ilp"; // Consider using appsettings.json instead

        public static async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(new MailboxAddress("PizzaShop", SmtpUser));
            emailToSend.To.Add(new MailboxAddress("", email));
            emailToSend.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message
            };

            emailToSend.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var emailClient = new SmtpClient();
                await emailClient.ConnectAsync(SmtpServer, SmtpPort, SecureSocketOptions.StartTls);
                await emailClient.AuthenticateAsync(SmtpUser, SmtpPassword);
                await emailClient.SendAsync(emailToSend);
                await emailClient.DisconnectAsync(true);
                return true;
            }
            catch
            {
                return false;
            }
        }
}
