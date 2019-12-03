using System.Collections.Generic;

namespace ImportAndValidationTool.Validation.ValidationService
{
    public interface IValidationService
    {
        void ValidateEnrichmentGlobally(IEnumerable<ValidateDataModelBase> models,
            List<ValidationError> errorList);

        void ValidateM3Globally(IEnumerable<ValidateDataModelBase> models, List<ValidationError> errorList);

        void ValidateEnrichmentRows(ValidateDataModelBase model, ICollection<ValidationError> errorList,
            int counter);

        void ValidateM3Rows(ValidateDataModelBase model, ICollection<ValidationError> errorList, int counter);
    }
}
