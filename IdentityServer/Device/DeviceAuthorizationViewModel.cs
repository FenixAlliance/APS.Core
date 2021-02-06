

using FenixAlliance.APS.Core.IdentityServer.Consent;

namespace FenixAlliance.APS.Core.IdentityServer.Device
{
    public class DeviceAuthorizationViewModel : ConsentViewModel
    {
        public string UserCode { get; set; }
        public bool ConfirmUserCode { get; set; }
    }
}