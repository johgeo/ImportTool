﻿using System;
using System.Collections.Generic;
using ImportAndValidationTool.Import;
using ImportAndValidationTool.Models;
using ImportAndValidationTool.Validation.EnrichValidation;
using ImportAndValidationTool.Validation.M3Validation;

namespace ImportAndValidationTool.ModelMapper
{
    public static class Mapper
    {
        public static ModelBase Map<T>(IReadOnlyList<string> splitLine) where T : ModelBase
        {
            try
            {
                if(typeof(T) == typeof(CatalogContentExternalImportModel))
                    return new CatalogContentExternalImportModel
                    {
                        Code = splitLine[0],
                        EANcode = splitLine[1],
                        Firmness = splitLine[2],
                        UnitOfMeasure = splitLine[3],
                        NetWeight = splitLine[4],
                        GrossWeight = splitLine[5],
                        Volume = splitLine[6],
                        Measurement2 = splitLine[7],
                        Measurement1 = splitLine[8],
                        Measurement3 = splitLine[9],
                        ColorCode = splitLine[10],
                        ColorName = splitLine[11],
                        ProductPart = splitLine[12],
                        EcommercePlatform = splitLine[13],
                        StockPolicy = splitLine[14],
                        Model = splitLine[15],
                        Name = splitLine[0]
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
                        Firmness = splitLine[2],
                        UnitOfMeasure = splitLine[3],
                        NetWeight = splitLine[4],
                        GrossWeight = splitLine[5],
                        Volume = splitLine[6],
                        Measurement2 = splitLine[7],
                        Measurement1 = splitLine[8],
                        Measurement3 = splitLine[9],
                        ColorCode = splitLine[10],
                        ColorName = splitLine[11],
                        ProductPart = splitLine[12],
                        ECommercePlatform = splitLine[13],
                        StockPolicy = splitLine[14],
                        Model = splitLine[15]
                    };
                if(typeof(T) == typeof(EnrichmentExcelDataModel))
                    return new EnrichmentExcelDataModel
                    {
                        Brand = splitLine[0],
                        Categories = splitLine[1],
                        Beds = splitLine[2],
                        VariantCode = splitLine[3],
                        VariantBrand = splitLine[4],
                        DescriptionSv = splitLine[5],
                        DescriptionEn = splitLine[6],
                        LongDescriptionSv = splitLine[7],
                        LongDescriptionEn = splitLine[8],
                        Size = splitLine[9],
                        Sellable = splitLine[10],
                        PurchasePriceSek = splitLine[11],
                        PurchasePriceUsd = splitLine[12],
                        ProductGroup = splitLine[13],
                        Sorting = splitLine[14],
                        Label = splitLine[15],
                        IsBulkSku = splitLine[16],  
                        SellableAndPartOfConfiguratedBed = splitLine[17],
                        ProductNameSv = splitLine[18],
                        ProductNameEn = splitLine[19]
                    };
            }
            catch
            {
                Console.WriteLine($"You are trying to map {typeof(T).Name} to an excel that has {splitLine.Count} columns to a model that has {typeof(T).GetProperties().Length} fields, " +
                                  $"Make sure the excel file corresponds to the model you are trying to map");
            }

            throw new ArgumentException($"Could not map type parameter to model {typeof(T).Name}");
        }
    }
}
