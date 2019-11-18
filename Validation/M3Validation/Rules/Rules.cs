namespace ProductImporterTool.Validation.M3Validation.Rules
{
    public class Rules
    {
        public class SkuNumberMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model)
            {
                if (string.IsNullOrWhiteSpace(model.SkuNumber))
                    return false;
                else return true;
            }
        }

        public class EanCodeMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model)
            {
                if (string.IsNullOrWhiteSpace(model.EanCode))
                    return false;
                else return true;
            }
        }

        public class UnitOfMeasureMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model)
            {
                if (string.IsNullOrWhiteSpace(model.UnitOfMeasure))
                    return false;
                else return true;
            }
        }

        public class ColorCodeMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model)
            {
                if (string.IsNullOrWhiteSpace(model.ColorCode))
                    return false;
                else return true;
            }
        }

        public class ColorNameMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model)
            {
                if (string.IsNullOrWhiteSpace(model.ColorName))
                    return false;
                else return true;
            }
        }

        public class ModelIsMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model)
            {
                if (string.IsNullOrWhiteSpace(model.Model))
                    return false;
                else return true;
            }
        }

        public class ECommPlatformMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model)
            {
                if (string.IsNullOrWhiteSpace(model.ECommercePlatform))
                    return false;
                else return true;
            }
        }

        public class StockPolicyWrongDataOrMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model)
            {
                if (string.IsNullOrWhiteSpace(model.StockPolicy))
                    return false;
                if (!model.StockPolicy.ToLower().Equals("false") || !model.StockPolicy.ToLower().Equals("yes"))
                    return false;
                else return true;
            }
        }

    }
}
