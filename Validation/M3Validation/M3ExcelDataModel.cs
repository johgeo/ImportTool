namespace ImportAndValidationTool.Validation.M3Validation
{
    public class M3ExcelDataModel : ValidateDataModelBase
    {
        public string SkuNumber { get; set; }
        public string EanCode { get; set; }
        public string Firmness { get; set; }
        public string UnitOfMeasure { get; set; }
        public string NetWeight { get; set; }
        public string GrossWeight { get; set; }
        public string Volume { get; set; }
        //Length
        public string Measurement2 { get; set; }
        //Width
        public string Measurement1 { get; set; }
        //Height
        public string Measurement3 { get; set; }
        public string ColorCode { get; set; }
        public string ColorName { get; set; }
        public string ProductPart{ get; set; }
        public string ECommercePlatform { get; set; }
        public string StockPolicy { get; set; }
        public string Model { get; set; }
    }
}
