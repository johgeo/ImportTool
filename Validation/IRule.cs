namespace ProductImporterTool.Validation
{
    public interface IRule
    {
        /// <summary>
        /// Get the calling rule name to identify which rule validated models
        /// </summary>
        /// <returns></returns>
        string GetRuleName();
    }
}
