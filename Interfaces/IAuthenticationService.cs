using FenixAlliance.APS.Core.Services.Authentication;
using System.Threading.Tasks;

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