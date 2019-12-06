using System.Collections.Generic;
using System.Linq;

namespace ImportAndValidationTool.Validation.EnrichValidation
{
    public class EnrichmentRules
    {
        public class CategoryCodeFormatFaulty : ValidationRuleBase<EnrichmentExcelDataModel>
        {
            public override bool Valid(EnrichmentExcelDataModel model, out string message)
            {
                message = "Data in category field does not include an epi category code";
                if (model.Categories.Contains(","))
                {
                    var codes = model.Categories.Split(',');
                    foreach (var code in codes)
                    {
                        if (!code.Contains("_1"))
                            return false;
                    }
                }
                else
                {
                    if (!model.Categories.Contains("_1"))
                        return false;
                }

                return true;
            }
        }

        public class SellableStatusForBedSkuCorrect : ValidationRuleBase<EnrichmentExcelDataModel>
        {
            public override bool Valid(EnrichmentExcelDataModel model, out string message)
            {
                message = "Beds sku should always have Sellable column set to no";
                return string.IsNullOrWhiteSpace(model.Beds) || model.Sellable.Equals("no");
            }
        }

        public class CategoryCorrectForBedSku : ValidationRuleBase<EnrichmentExcelDataModel>
        {
            public override bool Valid(EnrichmentExcelDataModel model, out string message)
            {
                message = "Bed sku should only be categorized to Configurable-parts_1";
                return string.IsNullOrWhiteSpace(model.Beds) || model.Categories.Equals("Configurable-parts_1");
            }
        }

        public class SellableAndPartOfConfiguratedBedWrongDataFormat: ValidationRuleBase<EnrichmentExcelDataModel>
        {
            public override bool Valid(EnrichmentExcelDataModel model, out string message)
            {
                message = "Sellable and part of configurated bed has been left empty or has wrong format, correct format is yes or no";
                if (string.IsNullOrWhiteSpace(model.SellableAndPartOfConfiguratedBed))
                    return false;
                if (model.SellableAndPartOfConfiguratedBed.ToLower().Equals("no"))
                    return true;
                if (model.SellableAndPartOfConfiguratedBed.ToLower().Equals("yes"))
                    return true;
                else return false;
            }
        }

        //Globals
        public class IsSkuNumberUnique : GlobalValidationRuleBase<EnrichmentExcelDataModel>
        {
            public override bool Valid(IEnumerable<EnrichmentExcelDataModel> models, out IList<ValidationError> errors)
            {
                models = models.ToList();
                errors = new List<ValidationError>();

                var recurringAndNumOfOccurence = models
                    .GroupBy(x => x.VariantCode)
                    .Where(g => g.Count() > 1)
                    .ToDictionary(x => x.Key, y => y.Count());

                foreach (var model in models)
                {
                    if (recurringAndNumOfOccurence.ContainsKey(model.VariantCode))
                    {
                        recurringAndNumOfOccurence.TryGetValue(model.VariantCode, out var recurringCount);
                        var error = new GlobalError()
                        {
                            SkuCode = model.VariantCode,
                            RuleName = base.GetRuleName(),
                            Message = $"Sku with code {model.VariantCode} appears {recurringCount} times in the document."
                        };

                        errors.Add(error);
                    }
                }

                return !errors.Any();
            }
        }
    }
}
