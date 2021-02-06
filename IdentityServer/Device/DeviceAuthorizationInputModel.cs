

using FenixAlliance.APS.Core.IdentityServer.Consent;

namespace FenixAlliance.APS.Core.IdentityServer.Device
{
    public class DeviceAuthorizationInputModel : ConsentInputModel
    {
        public string UserCode { get; set; }
    }
}