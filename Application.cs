using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ImportAndValidationTool.Enums;
using ImportAndValidationTool.Helpers;
using ImportAndValidationTool.Import;
using ImportAndValidationTool.Import.ImportService;
using ImportAndValidationTool.ModelMapper;
using ImportAndValidationTool.Models;
using ImportAndValidationTool.Validation;
using ImportAndValidationTool.Validation.EnrichValidation;
using ImportAndValidationTool.Validation.M3Validation;
using ImportAndValidationTool.Validation.ValidationService;
using Environment = ImportAndValidationTool.Enums.Environment;

namespace ImportAndValidationTool
{
    public class Application
    {
        private readonly IValidationService _validationService;
        private readonly IImportService _importService;
        private static Environment _env;
        private static Mode _mode;
        private static Validate _validate;

        private static char _lineSplitter = '|';

        public static string FilePath = string.Empty;
        public static string Url = string.Empty;
        public static string Endpoint = string.Empty;

        public Application(IValidationService validationService, IImportService importService)
        {
            _validationService = validationService;
            _importService = importService;
        }

        public void Run()
        {
            Console.WriteLine("-- Tool has started --");
            var run = true;
            while (run)
            {
                StartSetup();

                var models = CreateModels(ExcelHelper.GetLinesFromExcel(FilePath));
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                try
                {
                    Task.Run(async () =>
                    {
                        if (_mode.Equals(Mode.Product))
                            await _importService.ImportProducts(models.Cast<CatalogContentExternalImportModel>().ToList());
                        else if (_mode.Equals(Mode.Price))
                            await _importService.ImportPrices(models.Cast<PriceModel>().ToList());
                        else if (_mode.Equals(Mode.Stock))
                            await _importService.ImportStock(models.Cast<StockModel>().ToList());
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
            }
            else
            {
                SetupEnvironment();
                SetupImportMode();
            }
            SetupPaths();
        }

        #region Setup methods

        private static bool IsValidateMode()
        {
            const string exceptionMessage = "enter a valid number, dummy";
            Console.WriteLine("\n Choose work mode: \n 1. Import data\n 2. Validate data");
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
                    Url = ConfigurationManager.AppSettings["dev-url"];
                    break;
                case Environment.Integration:
                    Url = ConfigurationManager.AppSettings["integration-url"];
                    break;
                case Environment.Preproduction:
                    Url = ConfigurationManager.AppSettings["peprod-url"];
                    break;
                case Environment.Production:
                    Url = ConfigurationManager.AppSettings["prod-url"];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.WriteLine("\n Path to file:");
            Console.Write("Path: ");
            FilePath = Console.ReadLine();

            switch (_mode)
            {
                case Mode.Product:
                    Endpoint = ConfigurationManager.AppSettings["product-api-endpoint"];
                    break;
                case Mode.Price:
                    Endpoint = ConfigurationManager.AppSettings["price-api-endpoint"];
                    break;
                case Mode.Stock:
                    Endpoint = ConfigurationManager.AppSettings["stock-api-endpoint"];
                    break;
                case Mode.ValidateData:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Validator Methods

        private void ValidateData<TValidationModel>(IEnumerable<ValidateDataModelBase> models) where TValidationModel : ValidateDataModelBase
        {
            models = models.ToList();
            var fileTitle = string.Empty;
            var errorList = new List<ValidationError>();
            var counter = 2;

            if (typeof(TValidationModel) == typeof(M3ExcelDataModel))
            {
                fileTitle = "M3 data validation";
                foreach (var model in models)
                {
                    _validationService.ValidateM3Rows(model, errorList, counter);
                    counter++;
                }
                _validationService.ValidateM3Globally(models, errorList);
            }
            else if (typeof(TValidationModel) == typeof(EnrichmentExcelDataModel))
            {
                fileTitle = "Enrichment data validation";
                foreach (var model in models)
                {
                    _validationService.ValidateEnrichmentRows(model, errorList, counter);
                    counter++;
                }
                _validationService.ValidateEnrichmentGlobally(models, errorList);
            }

            ExcelHelper.SaveToExcel(errorList, fileTitle);
        }

        #endregion

        #region Create models methods

        private static List<ModelBase> CreateModels(List<string> csvLines)
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
