namespace ProductImporterTool.Validation.EnrichValidation.Rules
{
    public class EnrichmentRules
    {
        public class CategoryCodeFormatFaulty : ValidationRuleBase<EnrichmentExcelDataModel>
        {
            public override bool Validate(EnrichmentExcelDataModel model, out string message)
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
    }
}
