using System;
using ImportAndValidationTool.Registry;
using StructureMap;

namespace ImportAndValidationTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = ConfigureContainer().GetInstance<Application>();
            app.Run();
            Console.ReadLine();
        }

        private static Container ConfigureContainer()
        {
            var registry = new StructureMap.Registry();
            registry.IncludeRegistry<RulesRegistry>();
            registry.IncludeRegistry<ImportRegistry>();
            registry.IncludeRegistry<DefaultRegistry>();
            var container = new Container(registry);
            return container;
        }
    }
}
