
# ImportTool

Tool that can fake in products, price, and stock on all environments

  

Feel free to add/change stuff to the solution if you see room for improvments

  

Make sure to insert the neccessary values in App.Debug.config before ypu run the program



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
            public override bool Validate(M3ExcelDataModel model)
            {
                if (string.IsNullOrWhiteSpace(model.ColorCode))
                    return false;
                else return true;
            }
        }
```
Make sure do add the rule to the correpsonding registry.
```csharp
For<IValidationRule<M3ExcelDataModel>>().Add<Rules.ColorCodeMissing>();
```
- Run the tool and choose validate
-  If errors are found the tool will produce a file with all error in the excel with the corresponding sku number, row and rule that it failed on. 
