namespace ImportAndValidationTool.Validation.EnrichValidation
{
    public class EnrichmentExcelDataModel : ValidateDataModelBase
    {
        public string Brand { get; set; }
        public string Categories { get; set; }
        public string Beds { get; set; }
        public string VariantCode { get; set; }
        public string VariantBrand { get; set; }
        public string DescriptionSv { get; set; }
        public string DescriptionEn { get; set; }
        public string LongDescriptionSv { get; set; }
        public string LongDescriptionEn { get; set; }
        public string Size { get; set; }
        public string Sellable { get; set; }
        public string PurchasePriceSek { get; set; }
        public string PurchasePriceUsd { get; set; }
        public string ProductGroup { get; set; }
        public string Sorting { get; set; }
        public string Label { get; set; }
        public string IsBulkSku { get; set; }
        public string SellableAndPartOfConfiguratedBed { get; set; }
        public string ProductNameSv { get; set; }
        public string ProductNameEn { get; set; }


    }
}
