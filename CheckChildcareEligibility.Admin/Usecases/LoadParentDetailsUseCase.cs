using CheckChildcareEligibility.Admin.Models;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadParentDetailsUseCase
{
    Task<(ParentGuardian Parent, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string parentDetailsJson = null,
        string validationErrorsJson = null
    );
}

public class LoadParentDetailsUseCase : ILoadParentDetailsUseCase
{
    private readonly ILogger<LoadParentDetailsUseCase> _logger;

    public LoadParentDetailsUseCase(ILogger<LoadParentDetailsUseCase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(ParentGuardian Parent, Dictionary<string, List<string>> ValidationErrors)> Execute(
        string parentDetailsJson = null,
        string validationErrorsJson = null)
    {
        ParentGuardian parent = null;
        Dictionary<string, List<string>> errors = null;


        if (!string.IsNullOrEmpty(parentDetailsJson))
            try
            {
                parent = JsonConvert.DeserializeObject<ParentGuardian>(parentDetailsJson);
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

        return (parent, errors);
    }
}