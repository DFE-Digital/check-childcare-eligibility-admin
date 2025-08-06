using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.ViewModels;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadParentAndChildDetailsUseCase
{
    Task<(ParentAndChildViewModel ParentAndChild, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string parentDetailsJson = null,
        string validationErrorsJson = null
    );
}

public class LoadParentAndChildDetailsUseCase : ILoadParentAndChildDetailsUseCase
{
    private readonly ILogger<LoadParentAndChildDetailsUseCase> _logger;

    public LoadParentAndChildDetailsUseCase(ILogger<LoadParentAndChildDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(ParentAndChildViewModel ParentAndChild, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string parentAndChildDetailsJson = null,
        string validationErrorsJson = null)
    {
        ParentAndChildViewModel parentAndChild = null;
        Dictionary<string, List<string>> errors = null;


        if (!string.IsNullOrEmpty(parentAndChildDetailsJson))
            try
            {
                parentAndChild = JsonConvert.DeserializeObject<ParentAndChildViewModel>(parentAndChildDetailsJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing parent details JSON");
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

        return (parentAndChild, errors);
    }
}