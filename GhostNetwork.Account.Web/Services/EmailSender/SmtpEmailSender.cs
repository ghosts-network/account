using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace GhostNetwork.Account.Web.Services.EmailSender
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpClientConfiguration configuration;

        public SmtpEmailSender(SmtpClientConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(EmailRecipient recipient, string subject, string body)
        {
            using var client = new SmtpClient();

            await client.ConnectAsync(configuration.Host, configuration.Port, configuration.EnableSsl);
            await client.AuthenticateAsync(configuration.UserName, configuration.Password);
            
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(configuration.DisplayName, configuration.Email));
            emailMessage.To.Add(new MailboxAddress(recipient.Name, recipient.Email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                ContentTransferEncoding = ContentEncoding.Default,
                Text = body
            };
            
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}