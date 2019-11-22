namespace ProductImporterTool.Validation
{
    public interface IValidationRule<in TValidationModel> where TValidationModel : ValidateDataModelBase
    {
        /// <summary>
        /// Validate model against rule.
        /// </summary>
        /// <param name="model">model to validate against</param>
        /// <param name="message">describing message as to why the validation against the rule failed</param>
        /// <returns>False if rule fails to validate</returns>
        bool Validate(TValidationModel model, out string message);

        /// <summary>
        /// Get the calling rule name to identify which rule validated model
        /// </summary>
        /// <returns></returns>
        string GetRuleName();
    }
}
