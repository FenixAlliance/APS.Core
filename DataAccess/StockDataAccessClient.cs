using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataAccess;
using FenixAlliance.ABM.Models.Logistics.Stock.Item;
using FenixAlliance.APS.Core.DataHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FenixAlliance.APS.Core.DataAccess
{
    public class StockDataAccessClient : IStockDataAccess
    {
        private ABMContext DataContext { get; set; }
        private IConfiguration Configuration { get; set; }
        private IHostEnvironment Environment { get; set; }
        private AccountUsersHelpers AccountTools { get; set; }
        private AccountGraphHelpers AccountGraphTools { get; set; }
        private BlobStorageDataAccessClient DataTools { get; set; }

        public StockDataAccessClient(ABMContext context, IConfiguration AppConfiguration, IHostEnvironment HostingEnvironment)
        {
            DataContext = context;
            Configuration = AppConfiguration;
            Environment = HostingEnvironment;
            DataTools = new BlobStorageDataAccessClient();
            AccountTools = new AccountUsersHelpers(DataContext);
            AccountGraphTools = new AccountGraphHelpers(DataContext, Configuration);
        }

        public StockDataAccessClient(DbContextOptions<ABMContext> dbContextOptions, IConfiguration AppConfiguration, IHostEnvironment HostingEnvironment)
        {
            Configuration = AppConfiguration;
            Environment = HostingEnvironment;
            DataTools = new BlobStorageDataAccessClient();
            DataContext = new ABMContext(dbContextOptions);
            AccountGraphTools = new AccountGraphHelpers(DataContext, Configuration);
            AccountTools = new AccountUsersHelpers(DataContext);
        }

        public async Task<Item> GetItem(string ItemID)
        {
            return await DataContext.Item.FindAsync(ItemID);
        }

        public async Task<Item> GetItem(string BusinessID, string ItemID)
        {
            return await DataContext.Item.FirstAsync(c=>c.ID == ItemID && c.BusinessID == BusinessID);
        }

        public async Task<List<Item>> GetItems(string BusinessID, List<string> ItemIDs)
        {
            List<Item> list = new List<Item>();

            foreach (var itemID in ItemIDs)
            {
                var item = await DataContext.Item.Where(c => c.BusinessID == BusinessID && c.BusinessID == itemID).FirstAsync();

                if (item != null)
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }
}
