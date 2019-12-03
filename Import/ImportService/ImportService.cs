using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ImportAndValidationTool.Extensions;
using ImportAndValidationTool.Helpers;

namespace ImportAndValidationTool.Import.ImportService
{
    public class ImportService : IImportService
    {

        public async Task ImportProducts(List<CatalogContentExternalImportModel> productsToImport)
        {
            var batches = productsToImport.Batch(10).ToList();
            var batchIndex = 1;
            Console.WriteLine($"Total number of batches {batches.Count}");

            foreach (var batch in batches)
            {
                Console.WriteLine($"- Importing batch {batchIndex}");

                var response = await HttpHelper.ExecuteRequest(HttpHelper.CreateProductImportHttpContent(batch), Application.Url, Application.Endpoint);
                if (response.StatusCode == HttpStatusCode.NoContent)
                    Console.WriteLine($"   Batch {batchIndex} complete --");

                batchIndex++;
            }

            Console.WriteLine("- All batches have been successfully imported");
        }

        public async Task ImportPrices(List<PriceModel> priceModels)
        {
            var prices = new List<SkuPrice>();
            foreach (var product in priceModels)
            {
                try
                {
                    Convert.ToDecimal(product.DefaultPrice);
                }
                catch
                {
                    continue;
                }

                prices.Add(new SkuPrice()
                {
                    CurrencyCode = "SEK",
                    Market = "default",
                    Price = Convert.ToDecimal(product.DefaultPrice),
                    SkuNumber = product.Code
                });
            }

            var batches = prices.Batch(10).ToList();
            var batchIndex = 1;
            Console.WriteLine($"Total number of batches {batches.Count}");

            foreach (var batch in batches)
            {
                Console.WriteLine($"- Importing batch {batchIndex}");

                var response = await HttpHelper.ExecuteRequest(HttpHelper.CreatePriceImportHttpContent(batch), Application.Url, Application.Endpoint);
                if (response.StatusCode == HttpStatusCode.NoContent)
                    Console.WriteLine($"   Batch {batchIndex} complete --");

                batchIndex++;
            }

            Console.WriteLine("- All batches have been successfully imported");
        }

        public async Task ImportStock(List<StockModel> stockModels)
        {
            var stockQuantities = stockModels.Select(p => new StockQuantity
            {
                SkuNumber = p.Code,
                Quantity = p.DefaultStock,
                Warehouse = "V30"
            });

            var batches = stockQuantities.Batch(10).ToList();
            var batchIndex = 1;
            Console.WriteLine($"Total number of batches {batches.Count}");

            foreach (var batch in batches)
            {
                Console.WriteLine($"- Importing batch {batchIndex}");

                var response = await HttpHelper.ExecuteRequest(HttpHelper.CreateStockImportHttpContent(batch), Application.Url, Application.Endpoint);
                if (response.StatusCode == HttpStatusCode.NoContent)
                    Console.WriteLine($"   Batch {batchIndex} complete --");

                batchIndex++;
            }

            Console.WriteLine("- All batches have been successfully imported");
        }
    }
}
