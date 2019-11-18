using System;
using ProductImporterTool.Registry;
using StructureMap;

namespace ProductImporterTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = Container.For<RulesRegistry>();
            var app = container.GetInstance<Application>();
            app.Run();
            Console.ReadLine();
        }
    }
}
