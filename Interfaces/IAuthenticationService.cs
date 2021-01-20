using System.Threading.Tasks;
using FenixAlliance.APS.Core.Services.Authentication;

namespace FenixAlliance.APS.Core.Interfaces
{
    public interface IAuthenticationService
    {
        void SetParent(object parent);
        Task<UserContext> SignInAsync();
        Task<UserContext> SignOutAsync();
        Task<UserContext> EditProfileAsync();
        Task<UserContext> ResetPasswordAsync();
    }
}