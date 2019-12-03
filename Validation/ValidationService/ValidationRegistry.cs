namespace ImportAndValidationTool.Validation.ValidationService
{
    public class ValidationRegistry : StructureMap.Registry
    {
        public ValidationRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            For<IValidationService>().Use<ValidationService>();
        }
    }
}
