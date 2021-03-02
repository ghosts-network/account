using System.Threading.Tasks;

namespace GhostNetwork.Account.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailRecipient recipient, string subject, string body);
    }
}