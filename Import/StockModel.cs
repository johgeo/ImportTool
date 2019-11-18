using ProductImporterTool.Models;

namespace ProductImporterTool.Import
{
    public class StockModel : ModelBase
    {
        public string Code { get; set; }
        public int DefaultStock { get; set; }
    }
}
