using FenixAlliance.ABM.Data;
using FenixAlliance.ACL.Configuration.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FenixAlliance.APS.Core.Helpers
{
    public interface IDnsHelperService
    {
        IConfiguration Configuration { get; set; }
        ABMContext DataContext { get; set; }
        IWebHostEnvironment Environment { get; set; }
        ISuiteOptions SuiteOptions { get; set; }
        Task DoDnsLookup(string Domain);
        Task<IEnumerable<string>> GetTxtRecords(string domain);
    }
}