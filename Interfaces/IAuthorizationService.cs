using System.Collections.Generic;
using System.Threading.Tasks;

namespace FenixAlliance.APS.Core.Interfaces
{
    public interface IAuthorizationHelpers
    {
        Task AuthorizeClient();
        Task AuthorizeClient(List<string> RequestedScopes);
    }
}
