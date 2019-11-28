using System.Collections.Generic;

namespace ProductImporterTool.Validation
{
    public interface IGlobalValidationRule<in TValidationModel> : IRule where TValidationModel : ValidateDataModelBase
    {
        /// <summary>
        /// Validate all models against a global rule
        /// </summary>
        /// <param name="models">models to validate against</param>
        /// <param name="errors">describing message as to why the validation against the rule failed</param>
        /// <returns>False if rule fails to validate</returns>
        bool Valid(IEnumerable<TValidationModel> models, out IList<ValidationError> errors);
    }
}
