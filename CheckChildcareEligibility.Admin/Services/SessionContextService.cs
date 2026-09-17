using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Services;

public interface ISessionContextService
{
    public T? GetSessionData<T>(string contextId, string key);

    public void ClearSessionData(string contextId, string key);

    public void SetSessionData<T>(string contextId, string key, T value);
}

public class SessionContextService : ISessionContextService
{
    public SessionContextService(IHttpContextAccessor httpContextAccessor, ILogger<SessionContextService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private readonly ILogger<SessionContextService> _logger;
	protected readonly IHttpContextAccessor _httpContextAccessor;

    public T? GetSessionData<T>(string contextId, string key)
    {
        string sessionKey = $"{contextId}:{key}";
        string? jsonValue = _httpContextAccessor?.HttpContext?.Session.GetString(sessionKey);
        if (jsonValue != null)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonValue);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing JSON to type {TypeName} for session key {SessionKey}.", typeof(T).FullName, sessionKey);
            }
        }
        return default;
    }

    public void SetSessionData<T>(string contextId, string key, T value)
    {
        string sessionKey = $"{contextId}:{key}";
        _httpContextAccessor?.HttpContext?.Session.SetString(sessionKey, JsonConvert.SerializeObject(value));
    }
    public void ClearSessionData(string contextId, string key)
    {
        string sessionKey = $"{contextId}:{key}";
        _httpContextAccessor?.HttpContext?.Session.SetString(sessionKey, string.Empty);
    }
}