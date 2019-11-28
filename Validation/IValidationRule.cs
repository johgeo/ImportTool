namespace ProductImporterTool.Validation
{
    public interface IValidationRule<in TValidationModel> : IRule where TValidationModel : ValidateDataModelBase
    {
        /// <summary>
        /// Valid model against rule.
        /// </summary>
        /// <param name="model">model to validate against</param>
        /// <param name="message">describing message as to why the validation against the rule failed</param>
        /// <returns>False if rule fails to validate</returns>
        bool Valid(TValidationModel model, out string message);
    }
}
