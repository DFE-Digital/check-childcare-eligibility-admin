using CheckChildcareEligibility.Admin.ViewModels;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadFosterPartnerDetailsUseCase
{
    Task<(FosterPartnerDetailsViewModel fosterPartnerDetails, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string fosterPartnerDetailsJson = null,
        string validationErrorsJson = null
    );
}

public class LoadFosterPartnerDetailsUseCase : ILoadFosterPartnerDetailsUseCase
{
    private readonly ILogger<LoadFosterPartnerDetailsUseCase> _logger;

    public LoadFosterPartnerDetailsUseCase(ILogger<LoadFosterPartnerDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(FosterPartnerDetailsViewModel fosterPartnerDetails, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string fosterPartnerDetailsJson = null,
        string validationErrorsJson = null)
    {
        FosterPartnerDetailsViewModel fosterPartnerDetailsViewModel = null;
        Dictionary<string, List<string>> errors = null;


        if (!string.IsNullOrEmpty(fosterPartnerDetailsJson))
            try
            {
                fosterPartnerDetailsViewModel = JsonConvert.DeserializeObject<FosterPartnerDetailsViewModel>(fosterPartnerDetailsJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing fosterPartner details JSON");
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

        return (fosterPartnerDetailsViewModel, errors);
    }
}