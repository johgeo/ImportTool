using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using OfficeOpenXml.ConditionalFormatting.Contracts;

namespace ImportAndValidationTool.Validation.M3Validation
{
    public class M3Rules
    {
        public class SkuNumberMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(M3ExcelDataModel model, out string message)
            {
                message = "Sku number is critical and cannot be missing";
                if (string.IsNullOrWhiteSpace(model.SkuNumber))
                    return false;
                else return true;
            }
        }

        public class EanCodeMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(M3ExcelDataModel model, out string message)
            {
                message = "Ean code field has been left empty";
                if (string.IsNullOrWhiteSpace(model.EanCode))
                    return false;
                else return true;
            }
        }

        public class UnitOfMeasureMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(M3ExcelDataModel model, out string message)
            {
                message = "Unit of measure field has been left empty";
                if (string.IsNullOrWhiteSpace(model.UnitOfMeasure))
                    return false;
                else return true;
            }
        }

        public class ColorCodeMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(M3ExcelDataModel model, out string message)
            {
                message = "Color code field has been left empty";
                if (string.IsNullOrWhiteSpace(model.ColorCode))
                    return false;
                else return true;
            }
        }

        public class ColorNameMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(M3ExcelDataModel model, out string message)
            {
                message = "Color name field has been left empty";
                if (string.IsNullOrWhiteSpace(model.ColorName))
                    return false;
                else return true;
            }
        }

        public class ModelIsMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(M3ExcelDataModel model, out string message)
            {
                message = "Model field has been left empty";
                if (string.IsNullOrWhiteSpace(model.Model))
                    return false;
                else return true;
            }
        }

        public class ECommPlatformMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(M3ExcelDataModel model, out string message)
            {
                message = "Ecomm-platform field has been left empty, this will affect how we import products to CDB and Jensen";
                if (string.IsNullOrWhiteSpace(model.ECommercePlatform))
                    return false;
                else return true;
            }
        }

        public class StockPolicyWrongDataOrMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(M3ExcelDataModel model, out string message)
            {
                message = "Stock policy field has been left empty or has wrong format, correct format is yes or no";
                if (string.IsNullOrWhiteSpace(model.StockPolicy))
                    return false;
                if (model.StockPolicy.ToLower().Equals("no"))
                    return true;
                if (model.StockPolicy.ToLower().Equals("yes"))
                    return true;
                else return false;
            }
        }

        //Globals
        public class IsSkuNumberUnique : GlobalValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(IEnumerable<M3ExcelDataModel> models, out IList<ValidationError> errors)
            {
                models = models.ToList();
                errors = new List<ValidationError>();

                var recurringAndNumOfOccurence = models
                    .GroupBy(x => x.SkuNumber)
                    .Where(g => g.Count() > 1)
                    .ToDictionary(x => x.Key, y => y.Count());

                var rowCount = 1;
                foreach (var model in models)
                {
                    if (recurringAndNumOfOccurence.ContainsKey(model.SkuNumber))
                    {
                        recurringAndNumOfOccurence.TryGetValue(model.SkuNumber, out var recurringCount);
                        var error = new GlobalError()
                        {
                            SkuCode = model.SkuNumber,
                            RuleName = base.GetRuleName(),
                            Message = $"Sku number has been identified {recurringCount} times"
                        };

                        errors.Add(error);
                    }

                    rowCount++;
                }

                return !errors.Any();
            }
        }
    }
}
