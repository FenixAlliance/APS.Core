using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataAccess;
using FenixAlliance.ABM.Models.Logistics.Stock.Item;
using FenixAlliance.Data.Access.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FenixAlliance.Data.Access.DataAccess
{
    public class StockDataAccessClient : IStockDataAccess
    {
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ABMContext _context;
        private BlobStorageDataAccessClient _dataTools;
        private AccountUsersHelpers AccountTools { get; set; }
        private AccountGraphHelpers AccountGraphTools { get; set; }

        public StockDataAccessClient(ABMContext context, IConfiguration AppConfiguration, IHostEnvironment HostingEnvironment)
        {
            _context = context;
            _config = AppConfiguration;
            _env = HostingEnvironment;
            _dataTools = new BlobStorageDataAccessClient();
            AccountTools = new AccountUsersHelpers(_context);
            AccountGraphTools = new AccountGraphHelpers(_context, _config);
        }

        public StockDataAccessClient(DbContextOptions<ABMContext> dbContextOptions, IConfiguration AppConfiguration, IHostEnvironment HostingEnvironment)
        {
            _config = AppConfiguration;
            _env = HostingEnvironment;
            _dataTools = new BlobStorageDataAccessClient();
            _context = new ABMContext(dbContextOptions);
            AccountGraphTools = new AccountGraphHelpers(_context, _config);
            AccountTools = new AccountUsersHelpers(_context);
        }

        public Task<Item> GetItem(string ItemID)
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetItem(string BusinessID, string ItemID)
        {
            throw new NotImplementedException();
        }

        public Task<List<Item>> GetItems(string BusinessID, List<string> ItemIDs)
        {
            throw new NotImplementedException();
        }
    }
}
