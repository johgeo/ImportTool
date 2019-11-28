using ImportAndValidationTool.Models;

namespace ImportAndValidationTool.Import
{
    public class StockModel : ModelBase
    {
        public string Code { get; set; }
        public int DefaultStock { get; set; }
    }
}
