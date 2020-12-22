using GhostNetwork.Account.Web.Quickstart.Consent;

namespace GhostNetwork.Account.Web.Quickstart.Device
{
    public class DeviceAuthorizationViewModel : ConsentViewModel
    {
        public string UserCode { get; set; }
        public bool ConfirmUserCode { get; set; }
    }
}