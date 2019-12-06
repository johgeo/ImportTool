using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImportAndValidationTool.Import.ImportService
{
    public interface IImportService
    {
        Task ImportProducts(List<CatalogContentExternalImportModel> productsToImport);
        Task ImportPrices(List<PriceModel> priceModels);
        Task ImportStock(List<StockModel> stockModels);
    }
}
