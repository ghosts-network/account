using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GhostNetwork.Account.Web.Services.EmailSender
{
    public interface IEmailSender
    {
        Task SendInviteAsync(EmailRecipient recipient, InviteBody body);
    }

    public class NullEmailSender : IEmailSender
    {
        private readonly ILogger<NullEmailSender> logger;

        public NullEmailSender(ILogger<NullEmailSender> logger)
        {
            this.logger = logger;
        }

        public Task SendInviteAsync(EmailRecipient recipient, InviteBody body)
        {
            logger.LogInformation("Send invitation email to {RecipientFullName} (${RecipientEmail}). Body: {Body}", recipient.FullName, recipient.Email, body);
            return Task.CompletedTask;
        }
    }
}