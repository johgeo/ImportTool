namespace ProductImporterTool.Validation
{
    public interface IValidationRule<in TValidationModel> where TValidationModel : ValidateDataModelBase
    {
        /// <summary>
        /// Validate model against rule.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>False if rule fails to validate</returns>
        bool Validate(TValidationModel model);

        /// <summary>
        /// Get the calling rule name to identify which rule validated model
        /// </summary>
        /// <returns></returns>
        string GetRuleName();
    }
}
