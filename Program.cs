using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ProductImporterTool.Models;
using Newtonsoft.Json;
using ProductImporterTool.Extensions;

namespace ProductImporterTool
{
    public class Program
    {
        private static Environment _env;
        private static ImportMode _mode;

        private static string _url = string.Empty;
        private static string _endpoint = string.Empty;
        private static string _importFilePath = string.Empty;
        private static char _lineSplitter = ',';

        private static readonly HttpClient Client = new HttpClient();

        private static void StartSetup()
        {
            SetupEnvironment();
            SetupSplitDelimiter();
            SetupImportMode();
            SetupPaths();
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("-- Import Tool has started --");
            var run = true;
            while (run)
            {
                StartSetup();

                var linesFromCsvToImport = GetLinesFromCsv(_importFilePath);
                var importModels = CreateImportModels(linesFromCsvToImport);

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                try
                {
                    Task.Run(async () =>
                    {
                        if (_mode.Equals(ImportMode.Product))
                            await ImportProducts(importModels);
                        else if (_mode.Equals(ImportMode.Price))
                            await ImportPrices(importModels);
                        else if (_mode.Equals(ImportMode.Stock))
                            await ImportStock(importModels);
                    }).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    stopWatch.Stop();
                    Console.WriteLine($"The import ran for {stopWatch.Elapsed:g} before it failed");
                    run = false;
                }

                stopWatch.Stop();
                Console.WriteLine($"The import took {stopWatch.Elapsed:g}");
            }
        }

        #region Setup methods

        private static void SetupSplitDelimiter()
        {
            Console.WriteLine("\n What is the split delimiter in the csv file?");
            Console.Write("default is ',': ");
            var delimiter = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(delimiter))
                _lineSplitter = delimiter.ToCharArray().ElementAtOrDefault(0);
        }

        private static void SetupEnvironment()
        {
            Console.WriteLine(
                "\n Choose environment: \n 1. Development \n 2. Integration \n 3. Preproduction \n 4. Production");
            Console.Write("number: ");
            var envChoice = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(envChoice))
            {
                if (envChoice.Equals("1"))
                {
                    _env = Environment.Development;
                }
                else if (envChoice.Equals("2"))
                {
                    _env = Environment.Integration;
                }
                else if (envChoice.Equals("3"))
                {
                    _env = Environment.Preproduction;
                }
                else if (envChoice.Equals("4"))
                {
                    var shouldProceedToSetEnvToProd = TriggerEnvironmentWarningAndReadResult();
                    if (!string.IsNullOrWhiteSpace(shouldProceedToSetEnvToProd))
                    {
                        if (shouldProceedToSetEnvToProd.Equals("y") || shouldProceedToSetEnvToProd.Equals("Y"))
                            _env = Environment.Production;
                        else
                            System.Environment.Exit(0);
                    }
                }
                else
                {
                    throw new ArgumentException("enter a valid number, dummy");
                }
            }
        }

        private static string TriggerEnvironmentWarningAndReadResult()
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\n WARNING: This will push data to production environment, are you sure?");
            Console.ResetColor();
            Console.Write("y/n: ");
            return Console.ReadLine();
        }

        private static void SetupImportMode()
        {
            Console.WriteLine("\n Choose task: \n 1. Product import \n 2. Price import \n 3. Stock import");
            Console.Write("number: ");
            var taskChoice = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(taskChoice))
            {
                if (taskChoice.Equals("1"))
                {
                    _mode = ImportMode.Product;
                }
                else if (taskChoice.Equals("2"))
                {
                    _mode = ImportMode.Price;
                }
                else if (taskChoice.Equals("3"))
                {
                    _mode = ImportMode.Stock;
                }
                else
                {
                    throw new ArgumentException("enter a valid number, dummy");
                }
            }
        }

        private static void SetupPaths()
        {
            switch (_env)
            {
                case Environment.Development:
                    _url = ConfigurationManager.AppSettings["dev-url"];
                    break;
                case Environment.Integration:
                    _url = ConfigurationManager.AppSettings["integration-url"];
                    break;
                case Environment.Preproduction:
                    _url = ConfigurationManager.AppSettings["peprod-url"];
                    break;
                case Environment.Production:
                    _url = ConfigurationManager.AppSettings["prod-url"];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.WriteLine("\n Path to file:");
            Console.Write("Path: ");
            _importFilePath = Console.ReadLine();

            switch (_mode)
            {
                case ImportMode.Product:
                    _endpoint = ConfigurationManager.AppSettings["product-api-endpoint"];
                    break;
                case ImportMode.Price:
                    _endpoint = ConfigurationManager.AppSettings["price-api-endpoint"];
                    break;
                case ImportMode.Stock:
                    _endpoint = ConfigurationManager.AppSettings["stock-api-endpoint"];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Import Methods

        public static async Task ImportProducts(List<CatalogContentExternalImportModel> productsToImport)
        {
            var batches = productsToImport.Batch(10).ToList();
            var batchIndex = 1;
            Console.WriteLine($"Total number of batches {batches.Count}");

            foreach (var batch in batches)
            {
                Console.WriteLine($"- Importing batch {batchIndex}");

                var response = await ExecuteRequest(CreateProductImportContent(batch));
                if (response.StatusCode == HttpStatusCode.NoContent)
                    Console.WriteLine($"   Batch {batchIndex} complete --");

                batchIndex++;
            }

            Console.WriteLine("- All batches have been successfully imported");
        }

        public static async Task ImportPrices(List<CatalogContentExternalImportModel> productsToImport)
        {
            var prices = new List<SkuPrice>();
            foreach (var product in productsToImport)
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

                var response = await ExecuteRequest(CreatePriceImportContent(batch));
                if (response.StatusCode == HttpStatusCode.NoContent)
                    Console.WriteLine($"   Batch {batchIndex} complete --");

                batchIndex++;
            }

            Console.WriteLine("- All batches have been successfully imported");
        }

        public static async Task ImportStock(List<CatalogContentExternalImportModel> productsToImport)
        {
            var stockQuantities = productsToImport.Select(p => new StockQuantity
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

                var response = await ExecuteRequest(CreateStockImportContent(batch));
                if (response.StatusCode == HttpStatusCode.NoContent)
                    Console.WriteLine($"   Batch {batchIndex} complete --");

                batchIndex++;
            }

            Console.WriteLine("- All batches have been successfully imported");
        }

        #endregion

        #region Helper Methods

        private static List<string> GetLinesFromCsv(string pathToImportFile)
        {
            var allLines = new List<string>();

            using (var reader = new StreamReader(pathToImportFile))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) continue;

                    allLines.Add(line);
                }
            }

            Console.WriteLine($"{allLines.Count} lines have been identified.");
            return allLines;
        }

        public static List<CatalogContentExternalImportModel> CreateImportModels(List<string> linesToImport)
        {
            var importModels = new List<CatalogContentExternalImportModel>();

            var index = 0;

            foreach (var line in linesToImport)
            {
                if (index == 0)
                {
                    index++;
                    continue;
                }

                var splitLine = line.Split(_lineSplitter);
                importModels.Add(CreateModel(splitLine));
                index++;
            }

            Console.WriteLine($"{importModels.Count} import models have been created.");
            return importModels;
        }

        private static CatalogContentExternalImportModel CreateModel(IReadOnlyList<string> splitLine)
        {
            switch (_mode)
            {
                case ImportMode.Product:
                    return new CatalogContentExternalImportModel
                    {
                        Code = splitLine[0],
                        Name = splitLine[2],
                        ProductPart = splitLine[11],
                        Firmness = splitLine[3],
                        ColorCode = splitLine[10],
                        ColorName = splitLine[9],
                        Measurement1 = ParseToMillimeter(splitLine[5]),
                        Measurement2 = ParseToMillimeter(splitLine[6]),
                        Measurement3 = ParseToMillimeter(splitLine[7]),
                        Model = splitLine[12]
                    };
                case ImportMode.Price:
                    return new CatalogContentExternalImportModel
                    {
                        Code = splitLine[0],
                        DefaultPrice = splitLine[1]
                    };
                case ImportMode.Stock:
                    return new CatalogContentExternalImportModel
                    {
                        Code = splitLine[0],
                        DefaultStock = 0
                    };
            }

            return new CatalogContentExternalImportModel();
        }

        private static string ParseToMillimeter(string value)
        {
            if (int.TryParse(value, out var intResult))
                return (intResult * 10).ToString();
            return value;
        }

        #endregion

        #region Http Request Methods

        private static HttpContent CreateProductImportContent(IEnumerable<CatalogContentExternalImportModel> batch)
        {
            const string mediaType = "application/json";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(batch));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(mediaType);
            return content;
        }

        private static HttpContent CreatePriceImportContent(IEnumerable<SkuPrice> batch)
        {
            const string mediaType = "application/json";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(batch));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(mediaType);
            return content;
        }

        private static HttpContent CreateStockImportContent(IEnumerable<StockQuantity> batch)
        {
            const string mediaType = "application/json";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(batch));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(mediaType);
            return content;
        }

        private static void SetupHeaders()
        {
            const string mediaType = "application/json";
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ConfigurationManager.AppSettings["api-key"]);
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
        }

        private static async Task<HttpResponseMessage> ExecuteRequest(HttpContent content)
        {
            SetupHeaders();
            return await Client.PostAsync(_url + _endpoint, content);
        }

        #endregion

        #region Enums

        private enum ImportMode
        {
            Product,
            Price,
            Stock
        }

        private enum Environment
        {
            Development,
            Integration,
            Preproduction,
            Production
        }

        #endregion
    }
}
