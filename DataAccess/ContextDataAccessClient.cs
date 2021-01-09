using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace FenixAlliance.APS.Core.DataAccess
{
    public class ContextDataAccessClient : IContextDataAccess
    {
        public ABMContext GetNewMainContext(DbContextOptions<ABMContext> Options)
        {
            return new ABMContext(Options);
        }
    }
}
