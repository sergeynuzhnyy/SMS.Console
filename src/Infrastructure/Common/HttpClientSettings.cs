using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Common;

public class HttpClientSettings : IValidatableObject
{
    public string BaseAddress { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(BaseAddress))
        {
            yield return new ValidationResult(
                $"{nameof(HttpClientSettings)}.{nameof(BaseAddress)} is not configured",
                [ nameof(BaseAddress) ]);
        }
        
        if (string.IsNullOrEmpty(Username))
        {
            yield return new ValidationResult(
                $"{nameof(HttpClientSettings)}.{nameof(Username)} is not configured",
                [ nameof(Username) ]);
        }
        
        if (string.IsNullOrEmpty(Password))
        {
            yield return new ValidationResult(
                $"{nameof(HttpClientSettings)}.{nameof(Password)} is not configured",
                [ nameof(Password) ]);
        }
    }
}