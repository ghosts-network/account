using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GhostNetwork.Account.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailRecipient recipient, string subject, string body);
    }

    public class NullEmailSender : IEmailSender
    {
        private readonly ILogger<NullEmailSender> logger;

        public NullEmailSender(ILogger<NullEmailSender> logger)
        {
            this.logger = logger;
        }

        public Task SendEmailAsync(EmailRecipient recipient, string subject, string body)
        {
            logger.LogInformation($"Send email to {recipient.Name} (${recipient.Email}). Subject: {subject}. Body: {body}");
            return Task.CompletedTask;
        }
    }
}