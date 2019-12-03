namespace ImportAndValidationTool.Registry
{
    public class DefaultRegistry : StructureMap.Registry
    {
        public DefaultRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });
        }
    }
}
