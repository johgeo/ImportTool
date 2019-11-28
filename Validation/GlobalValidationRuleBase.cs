using System.Collections.Generic;

namespace ProductImporterTool.Validation
{
    public abstract class GlobalValidationRuleBase<TValidationModel> : IGlobalValidationRule<TValidationModel> where TValidationModel : ValidateDataModelBase
    {
        public abstract bool Valid(IEnumerable<TValidationModel> models, out IList<ValidationError> error);

        public string GetRuleName()
        {
            return this.GetType().Name;
        }
    }
}
