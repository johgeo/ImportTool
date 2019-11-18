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
using ProductImporterTool.Import;
using ProductImporterTool.Validation;
using ProductImporterTool.Validation.EnrichValidation;
using ProductImporterTool.Validation.M3Validation;

namespace ProductImporterTool
{
    public class Application
    {
        private readonly List<IValidationRule<M3ExcelDataModel>> _m3ValidationRules;

        private static Environment _env;
        private static Mode _mode;

        private static string _url = string.Empty;
        private static string _endpoint = string.Empty;
        private static string _filePath = string.Empty;
        private static char _lineSplitter = ',';

        private static readonly HttpClient Client = new HttpClient();

        public Application(IEnumerable<IValidationRule<M3ExcelDataModel>> m3ValidationRules)
        {
            _m3ValidationRules = m3ValidationRules.ToList();
        }

        public void Run()
        {
            Console.WriteLine("-- Tool has started --");
            var run = true;
            while (run)
            {
                StartSetup();

                var linesFromCsv = GetLinesFromCsv(_filePath);
                var models = CreateModels(linesFromCsv);

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                try
                {
                    Task.Run(async () =>
                    {
                        if (_mode.Equals(Mode.Product))
                            await ImportProducts(models.Cast<CatalogContentExternalImportModel>().ToList());
                        else if (_mode.Equals(Mode.Price))
                            await ImportPrices(models.Cast<PriceModel>().ToList());
                        else if (_mode.Equals(Mode.Stock))
                            await ImportStock(models.Cast<StockModel>().ToList());
                        else if (_mode.Equals(Mode.ValidateData))
                            ValidateM3ExcelData(models.Cast<M3ExcelDataModel>());
                    }).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    stopWatch.Stop();
                    Console.WriteLine($"The tool ran for {stopWatch.Elapsed:g} before it failed");
                    run = false;
                }

                stopWatch.Stop();
                Console.WriteLine($"The tool took {stopWatch.Elapsed:g} to finish");
            }
        }

        private static void StartSetup()
        {
            if (IsValidateMode())
                _mode = Mode.ValidateData;
            else
            {
                SetupEnvironment();
                SetupImportMode();
            }
            SetupSplitDelimiter();
            SetupPaths();
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

        private static bool IsValidateMode()
        {
            Console.WriteLine("\n Choose work mode: \n 1. Import data\n 2. Validate data");
            Console.Write("number: ");
            var envChoice = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(envChoice))
                throw new ArgumentException("enter a valid number, dummy");
            if (envChoice.Equals("1"))
                return false;
            if (envChoice.Equals("2"))
                return true;
            throw new ArgumentException("enter a valid number, dummy");
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
                    _mode = Mode.Product;
                }
                else if (taskChoice.Equals("2"))
                {
                    _mode = Mode.Price;
                }
                else if (taskChoice.Equals("3"))
                {
                    _mode = Mode.Stock;
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
            _filePath = Console.ReadLine();

            switch (_mode)
            {
                case Mode.Product:
                    _endpoint = ConfigurationManager.AppSettings["product-api-endpoint"];
                    break;
                case Mode.Price:
                    _endpoint = ConfigurationManager.AppSettings["price-api-endpoint"];
                    break;
                case Mode.Stock:
                    _endpoint = ConfigurationManager.AppSettings["stock-api-endpoint"];
                    break;
                case Mode.ValidateData:
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

        public static async Task ImportPrices(List<PriceModel> priceModels)
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

                var response = await ExecuteRequest(CreatePriceImportContent(batch));
                if (response.StatusCode == HttpStatusCode.NoContent)
                    Console.WriteLine($"   Batch {batchIndex} complete --");

                batchIndex++;
            }

            Console.WriteLine("- All batches have been successfully imported");
        }

        public static async Task ImportStock(List<StockModel> stockModels)
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

                var response = await ExecuteRequest(CreateStockImportContent(batch));
                if (response.StatusCode == HttpStatusCode.NoContent)
                    Console.WriteLine($"   Batch {batchIndex} complete --");

                batchIndex++;
            }

            Console.WriteLine("- All batches have been successfully imported");
        }

        #endregion

        #region Validator Methods

        private void ValidateM3ExcelData(IEnumerable<M3ExcelDataModel> models)
        {
            var errorList = new List<ValidationError>();
            var counter = 2;
            foreach (var model in models)
            {
                foreach (var m3ValidationRule in _m3ValidationRules)
                {
                    var isValidRule = m3ValidationRule.Validate(model);
                    if(!isValidRule)
                        errorList.Add(new ValidationError()
                        {
                            SkuCode = model.SkuNumber,
                            RowNumber = counter,
                            RuleName = m3ValidationRule.GetRuleName()
                        });
                }

                counter++;
            }

            SaveToTxt(errorList, "M3 field validation");
        }

        //TODO: Implement validation for Enrichment excel
        private void ValidateEnrichmentData(IEnumerable<EnrichmentExcelDataModel> models)
        {
            return;
        }

        private static void SaveToTxt(List<ValidationError> errorList, string title)
        {
            var pathSeparated = _filePath.Split('\\');
            var pathRemoved = pathSeparated.Take(pathSeparated.Length - 1).ToList();
            pathRemoved.Add("validation_errors.txt");
            var folderPath = string.Join("\\", pathRemoved.ToArray());
            if (errorList.Any())
            {
                using (TextWriter tw = new StreamWriter(folderPath))
                {
                    tw.WriteLine(title);
                    foreach (var error in errorList)
                        tw.WriteLine($"Sku:{error.SkuCode}, Row:{error.RowNumber} failed on rule: {error.RuleName}");
                    Console.WriteLine($"Found {errorList.Count} validation errors");
                    Console.WriteLine($"Saved validation errors to file {folderPath}");
                }
            }
            else
            {
                Console.WriteLine("No validation errors found");
            }
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

        public static List<ModelBase> CreateModels(List<string> csvLines)
        {
            var models = new List<ModelBase>();

            var index = 0;

            foreach (var line in csvLines)
            {
                if (index == 0)
                {
                    index++;
                    continue;
                }

                var splitLine = line.Split(_lineSplitter);
                models.Add(CreateModel(splitLine));
                index++;
            }

            Console.WriteLine($"{models.Count} models have been created.");
            return models;
        }

        private static ModelBase CreateModel(IReadOnlyList<string> splitLine)
        {
            switch (_mode)
            {
                case Mode.Product:
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
                case Mode.Price:
                    return new PriceModel
                    {
                        Code = splitLine[0],
                        DefaultPrice = splitLine[1]
                    };
                case Mode.Stock:
                    return new StockModel
                    {
                        Code = splitLine[0],
                        DefaultStock = 0
                    };
                case Mode.ValidateData:
                    return new M3ExcelDataModel
                    {
                        SkuNumber = splitLine[0],
                        EanCode = splitLine[1],
                        Firmness = splitLine[3],
                        UnitOfMeasure = splitLine[4],
                        NetWeight = splitLine[5],
                        GrossWeight = splitLine[6],
                        Volume = splitLine[7],
                        Measurement2 = splitLine[8],
                        Measurement1 = splitLine[9],
                        Measurement3 = splitLine[10],
                        ColorCode = splitLine[12],
                        ColorName = splitLine[13],
                        ProductPart = splitLine[14],
                        ECommercePlatform = splitLine[15],
                        StockPolicy = splitLine[16],
                        Model = splitLine[17]
                    };
            }

            return default;
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

        private enum Mode
        {
            Product,
            Price,
            Stock,
            ValidateData
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
