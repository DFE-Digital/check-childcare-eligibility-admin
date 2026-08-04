namespace CheckChildcareEligibility.Admin.UseCases;

public interface IValidateEligibilityCodeUseCase
{
    ValidationResult Execute(string eligibilityCode);
}

public class ValidateEligibilityCodeUseCase : IValidateEligibilityCodeUseCase
{
    private readonly ILogger<ValidateEligibilityCodeUseCase> _logger;

    public ValidateEligibilityCodeUseCase(
        ILogger<ValidateEligibilityCodeUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValidationResult Execute(string eligibilityCode)
    {
        var errors = ProcessErrors(eligibilityCode);

        if (errors.Any())
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors
            };
        }

        return new ValidationResult
        {
            IsValid = true
        };
    }

    private Dictionary<string, List<string>> ProcessErrors(
        string eligibilityCode)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(eligibilityCode))
        {
            errors.Add(
                "EligibilityCode",
                new List<string>
                {
                    "Enter an eligibility code that is 11 digits long"
                });

            return errors;
        }

        if (eligibilityCode.Length != 11)
        {
            errors.Add(
                "EligibilityCode",
                new List<string>
                {
                    "Eligibility code must be 11 digits long"
                });

            return errors;
        }

        if (!eligibilityCode.All(char.IsDigit))
        {
            errors.Add(
                "EligibilityCode",
                new List<string>
                {
                    "Eligibility code must only contain numbers"
                });
        }

        return errors;
    }
}