using CheckChildcareEligibility.Admin.ViewModels;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadFosterCarerDetailsUseCase
{
    Task<(FosterCarerDetailsViewModel fosterCarerDetails, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string fosterCarerDetailsJson = null,
        string validationErrorsJson = null
    );
}

public class LoadFosterCarerDetailsUseCase : ILoadFosterCarerDetailsUseCase
{
    private readonly ILogger<LoadFosterCarerDetailsUseCase> _logger;

    public LoadFosterCarerDetailsUseCase(ILogger<LoadFosterCarerDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(FosterCarerDetailsViewModel fosterCarerDetails, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string fosterCarerDetailsJson = null,
        string validationErrorsJson = null)
    {
        FosterCarerDetailsViewModel fosterCarerDetailsViewModel = null;
        Dictionary<string, List<string>> errors = null;


        if (!string.IsNullOrEmpty(fosterCarerDetailsJson))
            try
            {
                fosterCarerDetailsViewModel = JsonConvert.DeserializeObject<FosterCarerDetailsViewModel>(fosterCarerDetailsJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing fosterCarer details JSON");
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

        return (fosterCarerDetailsViewModel, errors);
    }
}