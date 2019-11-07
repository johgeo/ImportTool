namespace ProductImporterTool.Models
{
    public class CatalogContentExternalImportModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ProductPart { get; set; }
        public string Firmness { get; set; }
        public string ColorCode { get; set; }
        public string ColorName { get; set; }
        public string Model { get; set; }
        public string Measurement1 { get; set; }
        public string Measurement2 { get; set; }
        public string Measurement3 { get; set; }
        public string DefaultPrice { get; set; }
        public int DefaultStock { get; set; }
        // fields not used by import tool but exist on epi model
        public int Discontinued { get; set; }
        public string GrossWeight { get; set; }
        public string NetWeight { get; set; }
        public string Volume { get; set; }
        public string UnitOfMeasure { get; set; }
        public string EANcode { get; set; }
        public string EcommercePlatform { get; set; }
    }
}
