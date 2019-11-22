using System.Net.Http;

namespace ProductImporterTool.Validation.M3Validation.Rules
{
    public class M3Rules
    {
        public class SkuNumberMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model, out string message)
            {
                message = "Sku number is critical and cannot be missing";
                if (string.IsNullOrWhiteSpace(model.SkuNumber))
                    return false;
                else return true;
            }
        }

        public class EanCodeMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model, out string message)
            {
                message = "Ean code field has been left empty";
                if (string.IsNullOrWhiteSpace(model.EanCode))
                    return false;
                else return true;
            }
        }

        public class UnitOfMeasureMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model, out string message)
            {
                message = "Unit of measure field has been left empty";
                if (string.IsNullOrWhiteSpace(model.UnitOfMeasure))
                    return false;
                else return true;
            }
        }

        public class ColorCodeMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model, out string message)
            {
                message = "Color code field has been left empty";
                if (string.IsNullOrWhiteSpace(model.ColorCode))
                    return false;
                else return true;
            }
        }

        public class ColorNameMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model, out string message)
            {
                message = "Color name field has been left empty";
                if (string.IsNullOrWhiteSpace(model.ColorName))
                    return false;
                else return true;
            }
        }

        public class ModelIsMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model, out string message)
            {
                message = "Model field has been left empty";
                if (string.IsNullOrWhiteSpace(model.Model))
                    return false;
                else return true;
            }
        }

        public class ECommPlatformMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model, out string message)
            {
                message = "Ecomm-platform field has been left empty, this will affect how we import products to CDB and Jensen";
                if (string.IsNullOrWhiteSpace(model.ECommercePlatform))
                    return false;
                else return true;
            }
        }

        public class StockPolicyWrongDataOrMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Validate(M3ExcelDataModel model, out string message)
            {
                message = "Stock policy field has been left empty or has wrong format, correct format is yes or no";
                if (string.IsNullOrWhiteSpace(model.StockPolicy))
                    return false;
                if (!model.StockPolicy.ToLower().Equals("false") || !model.StockPolicy.ToLower().Equals("yes"))
                    return false;
                else return true;
            }
        }

    }
}
