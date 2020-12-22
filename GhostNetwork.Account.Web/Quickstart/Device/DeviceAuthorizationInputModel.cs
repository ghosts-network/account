using GhostNetwork.Account.Web.Quickstart.Consent;

namespace GhostNetwork.Account.Web.Quickstart.Device
{
    public class DeviceAuthorizationInputModel : ConsentInputModel
    {
        public string UserCode { get; set; }
    }
}