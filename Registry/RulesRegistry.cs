using ProductImporterTool.Validation;
using ProductImporterTool.Validation.EnrichValidation;
using ProductImporterTool.Validation.M3Validation;

namespace ProductImporterTool.Registry
{
    public class RulesRegistry : StructureMap.Registry
    {
        public RulesRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            For<IValidationRule<M3ExcelDataModel>>().Add<M3Rules.SkuNumberMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<M3Rules.EanCodeMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<M3Rules.UnitOfMeasureMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<M3Rules.ColorCodeMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<M3Rules.ColorNameMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<M3Rules.ModelIsMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<M3Rules.ECommPlatformMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<M3Rules.StockPolicyWrongDataOrMissing>();

            For<IValidationRule<EnrichmentExcelDataModel>>().Add<EnrichmentRules.CategoryCodeFormatFaulty>();
            For<IValidationRule<EnrichmentExcelDataModel>>().Add<EnrichmentRules.SellableStatusForBedSkuCorrect>();
            For<IValidationRule<EnrichmentExcelDataModel>>().Add<EnrichmentRules.CategoryCorrectForBedSku>();

            //Global rules
            For<IGlobalValidationRule<EnrichmentExcelDataModel>>().Add<EnrichmentRules.IsSkuNumberUnique>();
        }
    }
}
