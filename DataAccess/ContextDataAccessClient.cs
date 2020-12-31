using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataAccess;

namespace FenixAlliance.Data.Access.DataAccess
{
    public class ContextDataAccessClient : IContextDataAccess
    {
        public ABMContext GetNewMainContext(DbContextOptions<ABMContext> Options)
        {
            return new ABMContext(Options);
        }
    }
}
