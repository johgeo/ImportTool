using ImportAndValidationTool.Models;

namespace ImportAndValidationTool.Import
{
    public class PriceModel : ModelBase
    {
        public string Code { get; set; }
        public string DefaultPrice { get; set; }
    }
}
