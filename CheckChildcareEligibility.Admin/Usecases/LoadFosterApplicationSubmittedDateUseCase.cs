using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.ViewModels;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadFosterApplicationSubmittedDateUseCase
{
    Task<(FosterApplicationSubmittedDateViewModel submittedDate, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string submittedDateDetailsJson = null,
        string validationErrorsJson = null
    );
}

public class LoadFosterApplicationSubmittedDateUseCase : ILoadFosterApplicationSubmittedDateUseCase
{
    private readonly ILogger<LoadFosterApplicationSubmittedDateUseCase> _logger;

    public LoadFosterApplicationSubmittedDateUseCase(ILogger<LoadFosterApplicationSubmittedDateUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(FosterApplicationSubmittedDateViewModel submittedDate, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string submittedDateDetailsJson = null,
        string validationErrorsJson = null)
    {
        FosterApplicationSubmittedDateViewModel submittedDate = null;
        Dictionary<string, List<string>> errors = null;


        if (!string.IsNullOrEmpty(submittedDateDetailsJson))
            try
            {
                submittedDate = JsonConvert.DeserializeObject<FosterApplicationSubmittedDateViewModel>(submittedDateDetailsJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing submittedDate details JSON");
            }


        if (!string.IsNullOrEmpty(validationErrorsJson))
        {
            try
            {
                errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(validationErrorsJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing validation errors JSON");
            }
        }

        return (submittedDate, errors);
    }
}