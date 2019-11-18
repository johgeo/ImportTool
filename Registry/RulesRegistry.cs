using ProductImporterTool.Validation;
using ProductImporterTool.Validation.M3Validation;
using ProductImporterTool.Validation.M3Validation.Rules;

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

            For<IValidationRule<M3ExcelDataModel>>().Add<Rules.SkuNumberMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<Rules.EanCodeMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<Rules.UnitOfMeasureMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<Rules.ColorCodeMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<Rules.ColorNameMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<Rules.ModelIsMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<Rules.ECommPlatformMissing>();
            For<IValidationRule<M3ExcelDataModel>>().Add<Rules.StockPolicyWrongDataOrMissing>();
        }
    }
}
