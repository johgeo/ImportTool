using System.Collections.Generic;
using System.Linq;

namespace ProductImporterTool.Validation.EnrichValidation
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

                var rowCount = 1;
                foreach (var model in models)
                {
                    if (recurringAndNumOfOccurence.ContainsKey(model.VariantCode))
                    {
                        recurringAndNumOfOccurence.TryGetValue(model.VariantCode, out var recurringCount);
                        var error = new ValidationError()
                        {
                            RowNumber = rowCount,
                            SkuCode = model.VariantCode,
                            RuleName = base.GetRuleName(),
                            Message = $"Variant code has been identified {recurringCount} times"
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
