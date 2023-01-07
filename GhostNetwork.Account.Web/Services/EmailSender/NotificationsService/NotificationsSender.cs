using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GhostNetwork.Account.Web.Services.EmailSender.NotificationsService;

public class NotificationsSender : IEmailSender
{
    private readonly HttpClient client;

    public NotificationsSender(HttpClient client)
    {
        this.client = client;
    }

    public async Task SendInviteAsync(EmailRecipient recipient, InviteBody body)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new
            {
                Object = body,
                Recipients = new[] { recipient }
            }),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        await client
            .PostAsync("/events/email-confirmation/trigger", content)
            .ConfigureAwait(false);
    }
}