using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace GhostNetwork.Account.Web.Services.EmailSender.Smtp
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpClientConfiguration configuration;

        public SmtpEmailSender(SmtpClientConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendInviteAsync(EmailRecipient recipient, InviteBody body)
        {
            var message = $"Please confirm your account by clicking this link: <a href=\"{body.ConfirmationUrl}\">link</a>";

            await SendEmailAsync(recipient, "Confirm your email", message);
        }

        private async Task SendEmailAsync(EmailRecipient recipient, string subject, string body)
        {
            using var client = new SmtpClient();

            await client.ConnectAsync(configuration.Host, configuration.Port, configuration.EnableSsl);
            await client.AuthenticateAsync(configuration.UserName, configuration.Password);

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(configuration.DisplayName, configuration.Email));
            emailMessage.To.Add(new MailboxAddress(recipient.FullName, recipient.Email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                ContentTransferEncoding = ContentEncoding.Default,
                Text = body
            };

            await client.SendAsync(emailMessage).ConfigureAwait(false);
            await client.DisconnectAsync(true);
        }
    }
}