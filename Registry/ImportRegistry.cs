using ImportAndValidationTool.Import.ImportService;

namespace ImportAndValidationTool.Registry
{
    public class ImportRegistry : StructureMap.Registry
    {
        public ImportRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            For<IImportService>().Use<ImportService>();
        }
    }
}
