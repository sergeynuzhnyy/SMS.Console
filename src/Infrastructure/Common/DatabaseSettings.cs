using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Common;

public class DatabaseSettings : IValidatableObject
{
    public string SmsDb { get; set; } = null!;
    
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(SmsDb))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseSettings)}.{nameof(SmsDb)} is not configured",
                [ nameof(SmsDb) ]);
        }
    }
}