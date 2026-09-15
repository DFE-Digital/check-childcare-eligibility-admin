using CheckChildcareEligibility.Admin.ViewModels;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadFosterChildDetailsUseCase
{
    Task<(FosterChildDetailsViewModel fosterChildDetails, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string fosterChildDetailsJson = null,
        string validationErrorsJson = null
    );
}

public class LoadFosterChildDetailsUseCase : ILoadFosterChildDetailsUseCase
{
    private readonly ILogger<LoadFosterChildDetailsUseCase> _logger;

    public LoadFosterChildDetailsUseCase(ILogger<LoadFosterChildDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(FosterChildDetailsViewModel fosterChildDetails, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string fosterChildDetailsJson = null,
        string validationErrorsJson = null)
    {
        FosterChildDetailsViewModel fosterChildDetails = null;
        Dictionary<string, List<string>> errors = null;


        if (!string.IsNullOrEmpty(fosterChildDetailsJson))
            try
            {
                fosterChildDetails = JsonConvert.DeserializeObject<FosterChildDetailsViewModel>(fosterChildDetailsJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing fosterChildDetails details JSON");
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

        return (fosterChildDetails, errors);
    }
}