namespace ImportAndValidationTool.Validation
{
    public class ValidationError
    {
        public int RowNumber { get; set; }
        public string SkuCode { get; set; }
        public string RuleName { get; set; }
        public string Message { get; set; }
    }
}
