namespace ProductImporterTool.Validation
{
    public abstract class ValidationRuleBase<TValidationModel> : IValidationRule<TValidationModel> where TValidationModel : ValidateDataModelBase
    {
        public abstract bool Validate(TValidationModel model, out string message);

        public string GetRuleName()
        {
            return this.GetType().Name;
        }
    }
}
