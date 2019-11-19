using System;
using System.Collections.Generic;
using ProductImporterTool.Import;
using ProductImporterTool.Models;
using ProductImporterTool.Validation.M3Validation;

namespace ProductImporterTool.ModelMapper
{
    public static class Mapper
    {
        public static ModelBase Map<T>(IReadOnlyList<string> splitLine) where T : ModelBase
        {
            if(typeof(T) == typeof(CatalogContentExternalImportModel))
                return new CatalogContentExternalImportModel
                {
                    Code = splitLine[0],
                    Name = splitLine[2],
                    ProductPart = splitLine[11],
                    Firmness = splitLine[3],
                    ColorCode = splitLine[10],
                    ColorName = splitLine[9],
                    Measurement1 = splitLine[5],
                    Measurement2 = splitLine[6],
                    Measurement3 = splitLine[7],
                    Model = splitLine[12]
                };
            if (typeof(T) == typeof(PriceModel))
                return new PriceModel
                {
                    Code = splitLine[0],
                    DefaultPrice = splitLine[1]
                };
            if (typeof(T) == typeof(StockModel))
                return new StockModel
                {
                    Code = splitLine[0],
                    DefaultStock = 0
                };
            if (typeof(T) == typeof(M3ExcelDataModel))
                return new M3ExcelDataModel
                {
                    SkuNumber = splitLine[0],
                    EanCode = splitLine[1],
                    Firmness = splitLine[3],
                    UnitOfMeasure = splitLine[4],
                    NetWeight = splitLine[5],
                    GrossWeight = splitLine[6],
                    Volume = splitLine[7],
                    Measurement2 = splitLine[8],
                    Measurement1 = splitLine[9],
                    Measurement3 = splitLine[10],
                    ColorCode = splitLine[12],
                    ColorName = splitLine[13],
                    ProductPart = splitLine[14],
                    ECommercePlatform = splitLine[15],
                    StockPolicy = splitLine[16],
                    Model = splitLine[17]
                };

            throw new ArgumentNullException("Could not map type parameter to a model");
        }
    }
}
