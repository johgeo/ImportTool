using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProductImporterTool.Models;
using ProductImporterTool.Enums;
using ProductImporterTool.Extensions;
using ProductImporterTool.Helpers;
using ProductImporterTool.Import;
using ProductImporterTool.ModelMapper;
using ProductImporterTool.Validation;
using ProductImporterTool.Validation.EnrichValidation;
using ProductImporterTool.Validation.M3Validation;
using Environment = ProductImporterTool.Enums.Environment;

namespace ProductImporterTool
{
    public class Application
    {
        private readonly List<IGlobalValidationRule<EnrichmentExcelDataModel>> _globalEnrichmentValidationRules;
        private readonly List<IValidationRule<EnrichmentExcelDataModel>> _enrichmentValidationRules;
        private readonly List<IValidationRule<M3ExcelDataModel>> _m3ValidationRules;

        private static Environment _env;
        private static Mode _mode;
        private static Validate _validate;

        private static string _url = string.Empty;
        private static string _endpoint = string.Empty;
        private static string _filePath = string.Empty;
        private static char _lineSplitter = ',';

        public Application(IEnumerable<IValidationRule<M3ExcelDataModel>> m3ValidationRules, 
            IEnumerable<IValidationRule<EnrichmentExcelDataModel>> enrichmentValidationRules, 
            IEnumerable<IGlobalValidationRule<EnrichmentExcelDataModel>> globalEnrichmentValidationRules)
        {
            _globalEnrichmentValidationRules = globalEnrichmentValidationRules.ToList();
            _enrichmentValidationRules = enrichmentValidationRules.ToList();
            _m3ValidationRules = m3ValidationRules.ToList();
        }

        public void Run()
        {
            Console.WriteLine("-- Tool has started --");
            var run = true;
            while (run)
            {
                StartSetup();

                var models = CreateModels(GetLinesFromExcel(_filePath));
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
                        {
                            if(_validate.Equals(Validate.M3Data))
                                ValidateData<M3ExcelDataModel>(models.Cast<M3ExcelDataModel>());
                            else if (_validate.Equals(Validate.EnrichmentData))
                                ValidateData<EnrichmentExcelDataModel>(models.Cast<EnrichmentExcelDataModel>());
                        }
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
            {
                _mode = Mode.ValidateData;
                _lineSplitter = '|';
            }
            else
            {
                SetupEnvironment();
                SetupImportMode();
                SetupSplitDelimiter();
            }
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
            const string exceptionMessage = "enter a valid number, dummy";
            Console.WriteLine("\n Choose work mode: \n 1. Import data\n 2. Valid data");
            Console.Write("number: ");
            var envChoice = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(envChoice))
                throw new ArgumentException(exceptionMessage);
            if (envChoice.Equals("1"))
                return false;
            if (envChoice.Equals("2"))
            {
                Console.WriteLine("\n Choose which type of data to validate \n 1. M3 Data \n 2. Enrichment Data");
                Console.Write("number: ");
                var validationChoice = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(validationChoice))
                    throw new ArgumentException(exceptionMessage);
                if (validationChoice.Equals("1"))
                    _validate = Validate.M3Data;
                if (validationChoice.Equals("2"))
                    _validate = Validate.EnrichmentData;
                return true;
            }
                
            throw new ArgumentException(exceptionMessage);
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

                var response = await HttpHelper.ExecuteRequest(HttpHelper.CreateProductImportHttpContent(batch), _url, _endpoint);
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

                var response = await HttpHelper.ExecuteRequest(HttpHelper.CreatePriceImportHttpContent(batch), _url, _endpoint);
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

                var response = await HttpHelper.ExecuteRequest(HttpHelper.CreateStockImportHttpContent(batch), _url, _endpoint);
                if (response.StatusCode == HttpStatusCode.NoContent)
                    Console.WriteLine($"   Batch {batchIndex} complete --");

                batchIndex++;
            }

            Console.WriteLine("- All batches have been successfully imported");
        }

        #endregion

        #region Validator Methods

        private void ValidateData<TValidationModel>(IEnumerable<ValidateDataModelBase> models) where TValidationModel : ValidateDataModelBase
        {
            models = models.ToList();
            var fileTitle = string.Empty;
            var errorList = new List<ValidationError>();
            var counter = 2;
            foreach (var model in models)
            {
                if (typeof(TValidationModel) == typeof(M3ExcelDataModel))
                {
                    fileTitle = "M3 field validation";
                    ValidateM3Rows(model, errorList, counter);
                } else if (typeof(TValidationModel) == typeof(EnrichmentExcelDataModel))
                {
                    fileTitle = "Enrichment field validation";
                    ValidateEnrichmentRows(model, errorList, counter);
                    ValidateEnrichmentGlobally(models, errorList);
                }

                counter++;
            }

            SaveToExcel(errorList, fileTitle);
        }

        private void ValidateEnrichmentGlobally(IEnumerable<ValidateDataModelBase> models, List<ValidationError> errorList)
        {
            models = models.ToList();
            foreach (var globalEnrichmentValidationRule in _globalEnrichmentValidationRules)
            {
                var enrichmentModels = models.Cast<EnrichmentExcelDataModel>();
                var isValidRule = globalEnrichmentValidationRule.Valid(enrichmentModels, out var errors);
                if (!isValidRule)
                    errorList.AddRange(errors);
            }
        }

        private void ValidateEnrichmentRows(ValidateDataModelBase model, ICollection<ValidationError> errorList, int counter)
        {
            foreach (var enrichmentValidationRule in _enrichmentValidationRules)
            {
                var enrichmentModel = (EnrichmentExcelDataModel) model;
                var isValidRule = enrichmentValidationRule.Valid(enrichmentModel, out var message);
                if (!isValidRule)
                    errorList.Add(new ValidationError()
                    {
                        RowNumber = counter,
                        SkuCode = enrichmentModel.VariantCode,
                        RuleName = enrichmentValidationRule.GetRuleName(),
                        Message = message
                    });
            }
        }

        private void ValidateM3Rows(ValidateDataModelBase model, ICollection<ValidationError> errorList, int counter)
        {
            foreach (var m3ValidationRule in _m3ValidationRules)
            {
                var m3Model = (M3ExcelDataModel) model;
                var isValidRule = m3ValidationRule.Valid(m3Model, out var message);
                if (!isValidRule)
                    errorList.Add(new ValidationError()
                    {
                        RowNumber = counter,
                        SkuCode = m3Model.SkuNumber,
                        RuleName = m3ValidationRule.GetRuleName(),
                        Message = message
                    });
            }
        }

        private static ExcelPackage CreateExcel(IList<ValidationError> errorList, string workSheetTitle)
        {
            ExcelPackage excel = null;

            try
            {
                excel = new ExcelPackage();
                var ws = excel.Workbook.Worksheets.Add(workSheetTitle);
                foreach (var prop in new ValidationError().GetType().GetProperties())
                {
                    var cell = ws.Cells[1, (ws.Dimension?.Columns ?? 0) + 1];
                    cell.Value = prop.Name;
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    cell.Style.Font.Bold = true;
                }

                for (var i = 0; i < errorList.Count; i++)
                {
                    var row = i + 2;
                    var props = errorList[i].GetType().GetProperties();
                    for (var j = 0; j < props.Length; j++)
                    {
                        var column = j + 1;
                        var cell = ws.Cells[row, column];
                        cell.Value = props[j].GetValue(errorList[i]);
                        cell.Style.Border.Bottom.Style = ExcelBorderStyle.None;
                        cell.Style.Font.Bold = false;
                    }
                }

                ws.Cells.AutoFitColumns(0);
                for (var i = 1; i <= ws.Dimension.Columns; i++)
                    ws.Column(i).Width += 2;

                excel.Save();
                excel.Stream.Position = 0;
            }
            catch (Exception)
            {
                excel?.Dispose();
                throw;
            }

            return excel;
        }

        private void SaveToExcel(IList<ValidationError> errorList, string title)
        {
            var folderPath = CreateFolderPath(".xlsx");
            using (var excel = CreateExcel(errorList, title))
            using (var fs = new FileStream(folderPath, FileMode.Create, FileAccess.Write))
                excel.Stream.CopyTo(fs);
            Console.WriteLine($"Found {errorList.Count} validation errors");
            Console.WriteLine($"Saved validation errors to file {folderPath}");
        }

        private static string CreateFolderPath(string fileEnding)
        {
            var pathSeparated = _filePath.Split('\\');
            var pathRemoved = pathSeparated.Take(pathSeparated.Length - 1).ToList();
            pathRemoved.Add($"validation_errors{fileEnding}");
            var folderPath = string.Join("\\", pathRemoved.ToArray());
            return folderPath;
        }

        #endregion

        #region Helper Methods

        private static List<string> GetLinesFromExcel(string pathToFile)
        {
            using (var streamReader = new StreamReader(pathToFile))
            using (var excel = new ExcelPackage(streamReader.BaseStream))
            {
                var firstWorkSheet = excel.Workbook.Worksheets.First();
                var lastColumnCount = firstWorkSheet.Dimension.Columns;
                var allLines = new List<string>();
                for (var row = 2; row < firstWorkSheet.Dimension.Rows; row++)
                {
                    var sb = new StringBuilder();
                    for (var column = 1; column < firstWorkSheet.Dimension.Columns; column++)
                    {
                        var value = firstWorkSheet.GetValue(row, column);
                        if (column != lastColumnCount)
                            sb.Append(value).Append("|");
                    }
                    allLines.Add(sb.ToString());
                }

                Console.WriteLine($"{allLines.Count} lines have been identified.");
                return allLines;
            }
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
                    return Mapper.Map<CatalogContentExternalImportModel>(splitLine);
                case Mode.Price:
                    return Mapper.Map<PriceModel>(splitLine);
                case Mode.Stock:
                    return Mapper.Map<StockModel>(splitLine);
                case Mode.ValidateData:
                    return _validate.Equals(Validate.M3Data)
                        ? Mapper.Map<M3ExcelDataModel>(splitLine)
                        : Mapper.Map<EnrichmentExcelDataModel>(splitLine);
            }

            return default;
        }

        [Obsolete("We really should not need to parse data to millimeter as it should already be in that format")]
        private static string ParseToMillimeter(string value)
        {
            if (int.TryParse(value, out var intResult))
                return (intResult * 10).ToString();
            return value;
        }

        #endregion
    }
}
