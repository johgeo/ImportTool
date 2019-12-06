using System.Collections.Generic;
using System.Linq;
using ImportAndValidationTool.Validation.EnrichValidation;
using ImportAndValidationTool.Validation.M3Validation;

namespace ImportAndValidationTool.Validation.ValidationService
{
    public class ValidationService : IValidationService
    {
        private readonly List<IGlobalValidationRule<EnrichmentExcelDataModel>> _globalEnrichmentValidationRules;
        private readonly List<IGlobalValidationRule<M3ExcelDataModel>> _globalM3ValidationRules;
        private readonly List<IValidationRule<EnrichmentExcelDataModel>> _enrichmentValidationRules;
        private readonly List<IValidationRule<M3ExcelDataModel>> _m3ValidationRules;

        public ValidationService(IEnumerable<IValidationRule<M3ExcelDataModel>> m3ValidationRules,
            IEnumerable<IValidationRule<EnrichmentExcelDataModel>> enrichmentValidationRules,
            IEnumerable<IGlobalValidationRule<EnrichmentExcelDataModel>> globalEnrichmentValidationRules,
            IEnumerable<IGlobalValidationRule<M3ExcelDataModel>> globalM3ValidationRules)
        {
            _globalEnrichmentValidationRules = globalEnrichmentValidationRules.ToList();
            _globalM3ValidationRules = globalM3ValidationRules.ToList();
            _enrichmentValidationRules = enrichmentValidationRules.ToList();
            _m3ValidationRules = m3ValidationRules.ToList();
        }

        public void ValidateEnrichmentGlobally(IEnumerable<ValidateDataModelBase> models, List<ValidationError> errorList)
        {
            models = models.ToList();
            foreach (var globalEnrichmentValidationRule in _globalEnrichmentValidationRules)
            {
                var enrichmentModels = models.Cast<EnrichmentExcelDataModel>();
                var isValidRule = globalEnrichmentValidationRule.Valid(enrichmentModels, out var errors);
                if (!isValidRule)
                    errorList.AddRange(errors);
            }
        }

        public void ValidateM3Globally(IEnumerable<ValidateDataModelBase> models, List<ValidationError> errorList)
        {
            models = models.ToList();
            foreach (var globalM3ValidationRule in _globalM3ValidationRules)
            {
                var m3Models = models.Cast<M3ExcelDataModel>();
                var isValidRule = globalM3ValidationRule.Valid(m3Models, out var errors);
                if (!isValidRule)
                    errorList.AddRange(errors);
            }
        }

        public void ValidateEnrichmentRows(ValidateDataModelBase model, ICollection<ValidationError> errorList, int counter)
        {
            foreach (var enrichmentValidationRule in _enrichmentValidationRules)
            {
                var enrichmentModel = (EnrichmentExcelDataModel)model;
                var isValidRule = enrichmentValidationRule.Valid(enrichmentModel, out var message);
                if (!isValidRule)
                    errorList.Add(new RowError()
                    {
                        RowNumber = counter,
                        SkuCode = enrichmentModel.VariantCode,
                        RuleName = enrichmentValidationRule.GetRuleName(),
                        Message = message
                    });
            }
        }

        public void ValidateM3Rows(ValidateDataModelBase model, ICollection<ValidationError> errorList, int counter)
        {
            foreach (var m3ValidationRule in _m3ValidationRules)
            {
                var m3Model = (M3ExcelDataModel)model;
                var isValidRule = m3ValidationRule.Valid(m3Model, out var message);
                if (!isValidRule)
                    errorList.Add(new RowError()
                    {
                        RowNumber = counter,
                        SkuCode = m3Model.SkuNumber,
                        RuleName = m3ValidationRule.GetRuleName(),
                        Message = message
                    });
            }
        }
    }
}
