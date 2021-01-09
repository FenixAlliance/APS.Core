using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataHelpers;
using FenixAlliance.ABM.Models.Logistics.Stock.Item;
using FenixAlliance.APS.Core.DataAccess;
using FenixAlliance.Models.DTOs.Components.Global.Currencies;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace FenixAlliance.APS.Core.DataHelpers
{
    public class StoreHelpers : IStoreHelpers
    {
        private readonly ABMContext DataContext;
        private readonly BlobStorageDataAccessClient BlobStorageDataAccessClient;

        public StoreHelpers(ABMContext context)
        {
            DataContext = context;
            BlobStorageDataAccessClient = new BlobStorageDataAccessClient();
        }
        public async Task<Item> CalculateItemPricesAsync(Item Item)
        {

            if (String.IsNullOrEmpty(Item.OpenCurrencyExchangeRates))
            {
                Item.OpenCurrencyExchangeRates = (await DataContext.Settings.AsNoTracking().FirstOrDefaultAsync(c => c.SettingsPK == "General")).OpenCurrencyExchangeRates;
            }

            var ForexRates = JsonConvert.DeserializeObject<CurrencyExchangeRates>(Item.OpenCurrencyExchangeRates);
            var ItemCurrency = Item.CurrencyID.Split('.')[0];
            var ConversionRate = (ForexRates.Rates.FirstOrDefault(c => c.Key == ItemCurrency).Value);
            var COP_ConversionRate = (ForexRates.Rates.FirstOrDefault(c => c.Key == "COP").Value);

            var Profit = 0.0;
            var Revenue = 0.0;
            var TotalTaxes = 0.0;
            var ePaycoRenta = 0.0;
            var ShippingCost = 0.0;
            var ShippingTaxes = 0.0;
            var ePaycoReteIVA = 0.0;
            var ePaycoReteICA = 0.0;
            var ePaycoCommission = 0.0;
            var PriceAfterDiscounts = 0.0;
            var ePaycoCommissionTax = 0.0;
            var RegularPrice = Item.RegularPrice / ConversionRate;

            // Ger prices after discount
            if (Item.OnDiscount)
            {
                // If is fixed discount, let's take the discount price and set it the base price
                if (Item.IsFixedDiscount)
                {
                    if (Item.IsDeadlineDiscount)
                    {
                        // Check from and due dates to apply discount. if not in range, apply regular price
                        if (DateTime.Compare(DateTime.Now, Item.DeadlineDiscountDueDate) <= 0)
                        {
                            // Meets criteria for deadline discount
                            PriceAfterDiscounts = Item.DiscountPrice / ConversionRate;
                        }
                        else
                        {
                            // Not in range for discount, apply regular price instead.
                            PriceAfterDiscounts = RegularPrice;
                        }
                    }
                    else
                    {
                        // Not deadline discount, just apply discount
                        PriceAfterDiscounts = Item.DiscountPrice / ConversionRate;
                    }
                }
                else
                {
                    if (Item.IsDeadlineDiscount)
                    {
                        if (DateTime.Compare(DateTime.Now, Item.DeadlineDiscountDueDate) <= 0)
                        {
                            // In Range for discount, get discount amount and apply it to regular price
                            PriceAfterDiscounts = (RegularPrice - (RegularPrice * (Item.DiscountPercentage / 100)));
                        }
                        else
                        {
                            // Not in range for discount, apply regular price instead.
                            PriceAfterDiscounts = RegularPrice;
                        }
                    }
                    else
                    {
                        // Not Deadline Discount, apply discount
                        PriceAfterDiscounts = (RegularPrice - (RegularPrice * (Item.DiscountPercentage / 100)));
                    }
                }
            }
            else
            {
                // Item is not on discount
                PriceAfterDiscounts = RegularPrice;
            }

            // Init price after policies
            var PriceBeforePolicies = PriceAfterDiscounts;

            // Init price after SellingMarginPolicy
            if (Item.ItemSellingMarginPolicyRecords == null)
            {
                Item.ItemSellingMarginPolicyRecords = await DataContext.ItemSellingMarginPolicyRecord
                    .AsNoTracking()
                    .Include(c => c.ItemSellingMarginPolicy)
                        .ThenInclude(c => c.Currency)
                    .Where(c => c.ItemID == Item.ID).ToListAsync();
            }

            foreach (var SellingMarginPolicy in Item.ItemSellingMarginPolicyRecords)
            {
                // Increase revenue percentages
                Revenue += ((PriceBeforePolicies * (SellingMarginPolicy.ItemSellingMarginPolicy.Percentage / 100)));
                // Get Revenue Policy Exchange Rate for fixed costs
                var SellingMarginPolicyExchangeRate = ForexRates.Rates.FirstOrDefault(c => c.Key == SellingMarginPolicy.ItemSellingMarginPolicy.CurrencyID.Split('.')[0]);
                Revenue += (SellingMarginPolicy.ItemSellingMarginPolicy.Value / SellingMarginPolicyExchangeRate.Value);
            }

            // Init price after PricingRules
            if (Item.ItemPricingRuleRecords == null)
            {
                Item.ItemPricingRuleRecords = await DataContext.ItemPricingRuleRecord
                    .AsNoTracking()
                    .Include(c => c.ItemPricingRule)
                        .ThenInclude(c => c.Currency)
                    .Where(c => c.ItemID == Item.ID).ToListAsync();
            }

            foreach (var ItemPricingRuleRecord in Item.ItemPricingRuleRecords)
            {
                // Increase revenue percentages
                Profit += ((PriceBeforePolicies * (ItemPricingRuleRecord.ItemPricingRule.Percentage / 100)));
                // Get Revenue Policy Exchange Rate for fixed costs
                var SellingMarginPolicyExchangeRate = ForexRates.Rates.FirstOrDefault(c => c.Key == ItemPricingRuleRecord.ItemPricingRule.CurrencyID.Split('.')[0]);
                Profit += (ItemPricingRuleRecord.ItemPricingRule.Value / SellingMarginPolicyExchangeRate.Value);
            }



            var PriceBeforeTaxes = PriceBeforePolicies + Revenue + Profit;

            if (Item.ItemTaxPolicyRecords == null)
            {
                Item.ItemTaxPolicyRecords = await DataContext.ItemTaxPolicyRecord
                    .AsNoTracking()
                    .Include(c => c.TaxPolicy)
                        .ThenInclude(c => c.Currency)
                    .Where(c => c.ItemID == Item.ID).ToListAsync();
            }
            foreach (var TaxPolicyRecord in Item.ItemTaxPolicyRecords)
            {
                // Apply Tax Rate
                TotalTaxes += (PriceBeforeTaxes * (TaxPolicyRecord.TaxPolicy.Percentage / 100));
                // Get Tax Policy Exchange Rate for fixed costs
                var TaxPolicyPolicyExchangeRate = ForexRates.Rates.FirstOrDefault(c => c.Key == TaxPolicyRecord.TaxPolicy.CurrencyID.Split('.')[0]);
                TotalTaxes += (TaxPolicyRecord.TaxPolicy.Value / TaxPolicyPolicyExchangeRate.Value);

            }
            // Cost + Base Taxes
            var CostTaxIncluded = (PriceBeforeTaxes + TotalTaxes);

            // Init shipping cost
            if (Item.ItemShippingPolicyRecords == null)
            {
                Item.ItemShippingPolicyRecords = await DataContext.ItemShippingPolicyRecord
                    .AsNoTracking()
                     .Include(c => c.ItemShippingPolicy)
                         .ThenInclude(c => c.Currency)
                     .Where(c => c.ItemID == Item.ID).ToListAsync();

            }

            foreach (var ShippingPolicyRecord in Item.ItemShippingPolicyRecords)
            {
                // Apply Shipping Cost
                ShippingCost += (CostTaxIncluded * (ShippingPolicyRecord.ItemShippingPolicy.Percentage / 100));
                // Get Shipping Policy Exchange Rate for fixed costs
                var ShippingPolicyPolicyExchangeRate = ForexRates.Rates.FirstOrDefault(c => c.Key == ShippingPolicyRecord.ItemShippingPolicy.CurrencyID.Split('.')[0]);
                ShippingCost += (ShippingPolicyRecord.ItemShippingPolicy.Value / ShippingPolicyPolicyExchangeRate.Value);
            }

            ShippingTaxes += (ShippingCost * (0.19));
            var TotalShippingCost = (ShippingCost + ShippingTaxes);
            var PriceAfterTaxes = (PriceBeforeTaxes + TotalShippingCost);

            // Calculate Payment Cost
            ePaycoCommission = (PriceAfterTaxes * (2.99 / 100)) + (7400 / COP_ConversionRate);
            ePaycoCommissionTax = (ePaycoCommission ) * (0.19);
            ePaycoReteICA = CostTaxIncluded * (0.02);
            ePaycoReteIVA = TotalTaxes * (0.15);
            ePaycoRenta = PriceAfterTaxes * (0.015);
            var ePaycoMaxPaymentCost = ePaycoCommission + ePaycoCommissionTax + ePaycoReteICA + ePaycoReteIVA + ePaycoRenta;

            // Increase Taxes
            TotalTaxes += ShippingTaxes;
            TotalTaxes += ePaycoCommissionTax;

            // Set Revenue & Profit
            Item.EstimatedProfitInUSD = Profit;
            Item.EstimatedRevenueInUSD = Revenue;

            // Set Final Price
            Item.EstimatedTotalPriceInUSD = CostTaxIncluded + ePaycoMaxPaymentCost + ShippingCost;

            // Set final taxes
            Item.EstimatedTaxesInUSD = TotalTaxes;

            Item.EstimatedPaymentCostInUSD = ePaycoMaxPaymentCost + ePaycoCommissionTax + 0.99;

            Item.EstimatedDefaultShippingCostInUSD = TotalShippingCost;

            Item.EstimatedDealSavingsInUSD = (RegularPrice + Revenue + Profit + Item.EstimatedTaxesInUSD) - (PriceAfterDiscounts + Profit + Revenue + Item.EstimatedTaxesInUSD);

            return Item;
        }
        public async Task<bool> CreateSARUsageRecord(string CartID, string ItemID, string UsageRecordType)
        {
            // Adds SAR Usage data
            CloudBlobContainer SAR_container = await BlobStorageDataAccessClient.GetCloudBlobContainerAsync("data");
            BlobContainerPermissions SAR_container_permissions = await SAR_container.GetPermissionsAsync();
            SAR_container_permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            await SAR_container.SetPermissionsAsync(SAR_container_permissions);

            var UsageAppendBlobReference = SAR_container.GetBlockBlobReference("usage/usage.csv");
            var csv = new StringBuilder();
            var UsageRecord = $"{CartID},{ItemID},{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ},{UsageRecordType}";
            csv.AppendLine(UsageRecord);
            try
            {
                if (!string.IsNullOrEmpty(CartID) && !string.IsNullOrWhiteSpace(CartID) && !string.IsNullOrEmpty(ItemID) && !string.IsNullOrWhiteSpace(ItemID) && !String.IsNullOrEmpty(UsageRecordType) && !String.IsNullOrWhiteSpace(UsageRecordType))
                {
                    await using var stream = new MemoryStream();
                    await UsageAppendBlobReference.DownloadToStreamAsync(stream);
                    await using var sw = new StreamWriter(stream);
                    sw.Write(csv.ToString());
                    sw.Flush();
                    stream.Position = 0;
                    await UsageAppendBlobReference.UploadFromStreamAsync(stream);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        public async Task<bool> CreateCustomWeightSARUsageRecord(string CartID, string ItemID, string CustomWeight)
        {
            // Adds SAR Usage data
            var SAR_container = await BlobStorageDataAccessClient.GetCloudBlobContainerAsync("data");
            var SAR_container_permissions = await SAR_container.GetPermissionsAsync();
            SAR_container_permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            await SAR_container.SetPermissionsAsync(SAR_container_permissions);

            var UsageAppendBlobReference = SAR_container.GetBlockBlobReference("usage/usage.csv");

            var csv = new StringBuilder();
            var UsageRecord = $"{CartID},{ItemID},{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ},,{CustomWeight}";
            csv.AppendLine(UsageRecord);
            try
            {
                if (!string.IsNullOrEmpty(CartID) && !string.IsNullOrWhiteSpace(CartID)
                    && !string.IsNullOrEmpty(ItemID) && !string.IsNullOrWhiteSpace(ItemID)
                    && !string.IsNullOrEmpty(CustomWeight) && !string.IsNullOrWhiteSpace(CustomWeight))
                {
                    await using var stream = new MemoryStream();
                    await UsageAppendBlobReference.DownloadToStreamAsync(stream);
                    await using var sw = new StreamWriter(stream);
                    sw.Write(csv.ToString());
                    sw.Flush();
                    stream.Position = 0;
                    await UsageAppendBlobReference.UploadFromStreamAsync(stream);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
