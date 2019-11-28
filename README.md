
# Import And Validation Tool

Tool that can fake in products, price, and stock on all environments

  

Feel free to add/change stuff to the solution if you see room for improvments

  

Make sure to insert the neccessary values in App.Debug.config before you run the program



```xml

<appSettings>
    <add key="dev-url" value="" xdt:Transform="Replace" xdt:Locator="Match(key)"/>
    <add key="integration-url" value="" xdt:Transform="Replace" xdt:Locator="Match(key)" />
    <add key="peprod-url" value="" xdt:Transform="Replace" xdt:Locator="Match(key)"/>
    <add key="prod-url" value="" xdt:Transform="Replace" xdt:Locator="Match(key)"/>
    <add key="product-api-endpoint" value="" xdt:Transform="Replace" xdt:Locator="Match(key)"/>
    <add key="price-api-endpoint" value="t" xdt:Transform="Replace" xdt:Locator="Match(key)"/>
    <add key="stock-api-endpoint" value="" xdt:Transform="Replace" xdt:Locator="Match(key)" />
    <add key="api-key" value="" xdt:Transform="Replace" xdt:Locator="Match(key)"/>
  </appSettings>

```

## Setup rules for validating excel files
- First create a model for which to map the excel columns to respective field.
- Make sure that model inherits from ValidateDataModelBase. This is just to make sure that we do not configure validation rules for import models.
```csharp
public class M3ExcelDataModel : ValidateDataModelBase
    {
        public string SkuNumber { get; set; }
        public string ColorCode { get; set; }
        public string ColorName { get; set; }
        public string Model { get; set; }
    }
```
Create a descriptive rule that inherits from ValidationRuleBase and pass in the type of the model to validate as a type parameter
```csharp
  public class ColorCodeMissing : ValidationRuleBase<M3ExcelDataModel>
        {
            public override bool Valid(M3ExcelDataModel model, out string message)
            {
                message = "Color code field has been left empty";
                if (string.IsNullOrWhiteSpace(model.ColorCode))
                    return false;
                else return true;
            }
        }
```
Make sure to add the rule to the corresponding registry.
```csharp
For<IValidationRule<M3ExcelDataModel>>().Add<Rules.ColorCodeMissing>();
```
Make sure to add the rule to the corresponding registry.
```csharp
For<IValidationRule<M3ExcelDataModel>>().Add<Rules.ColorCodeMissing>();
```

### Validate globally

If you wish to validate on something else than what is available on a row level for instance if a sku exists more than once in the excel you could define a global rule. The principal is the same as above but you will want to create a rule and derive from GlobalValidationRuleBase. This time we want to check this in the enrichment excel so we will pass in EnrichmentExcelDataModel as a type argument when we inherint the base class.
```csharp
   public class IsSkuNumberUnique : GlobalValidationRuleBase<EnrichmentExcelDataModel>
        {
            public override bool Valid(IEnumerable<EnrichmentExcelDataModel> models, out IList<ValidationError> errors)
            {
                models = models.ToList();
                errors = new List<ValidationError>();

                var recurringAndNumOfOccurence = models
                    .GroupBy(x => x.VariantCode)
                    .Where(g => g.Count() > 1)
                    .ToDictionary(x => x.Key, y => y.Count());

                var rowCount = 1;
                foreach (var model in models)
                {
                    if (recurringAndNumOfOccurence.ContainsKey(model.VariantCode))
                    {
                        recurringAndNumOfOccurence.TryGetValue(model.VariantCode, out var recurringCount);
                        var error = new ValidationError()
                        {
                            RowNumber = rowCount,
                            SkuCode = model.VariantCode,
                            RuleName = base.GetRuleName(),
                            Message = $"Variant code has been identified {recurringCount} times"
                        };

                        errors.Add(error);
                    }

                    rowCount++;
                }

                return !errors.Any();
            }
        }
```

Make sure to add the rule to the registry.
```csharp
For<IGlobalValidationRule<EnrichmentExcelDataModel>>().Add<EnrichmentRules.IsSkuNumberUnique>();
```

- Run the tool and choose validate
-  If errors are found the tool will produce an excel with all error in the excel with the corresponding sku number, row and rule that it failed on with a message describing why it failed. 
