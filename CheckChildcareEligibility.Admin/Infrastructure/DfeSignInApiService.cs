using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CheckChildcareEligibility.Admin.Domain.DfeSignIn;
using Microsoft.IdentityModel.Tokens;

namespace CheckChildcareEligibility.Admin.Infrastructure;

/// <summary>
///     Service for interacting with the DfE Sign-in public API.
/// </summary>
public class DfeSignInApiService : IDfeSignInApiService
{
    private readonly HttpClient _httpClient;
    private readonly IDfeSignInConfiguration _configuration;
    private readonly ILogger<DfeSignInApiService> _logger;

    public DfeSignInApiService(
        HttpClient httpClient,
        IDfeSignInConfiguration configuration,
        ILogger<DfeSignInApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IList<Role>> GetUserRolesAsync(string userId, Guid organisationId)
    {
        var roles = new List<Role>();

        try
        {
            if (string.IsNullOrEmpty(_configuration.APIServiceProxyUrl) ||
                string.IsNullOrEmpty(_configuration.APIServiceSecret))
            {
                _logger.LogWarning("DfE Sign-in API configuration is missing. APIServiceProxyUrl or APIServiceSecret not set.");
                return roles;
            }

            var token = GenerateApiToken();
            var url = $"{_configuration.APIServiceProxyUrl.TrimEnd('/')}/services/{_configuration.ClientId}/organisations/{organisationId}/users/{userId}";

            _logger.LogDebug("Calling DfE Sign-in API: {Url}", url);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to get user roles from DfE Sign-in API. Status: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
                return roles;
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("DfE Sign-in API response: {Response}", content);
            
            var userAccessResponse = JsonSerializer.Deserialize<DfeSignInUserAccessResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (userAccessResponse?.Roles != null)
            {
                roles.AddRange(userAccessResponse.Roles);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user roles from DfE Sign-in API");
        }

        return roles;
    }

    private string GenerateApiToken()
    {
        // Use UTF-8 bytes for the secret (DfE Sign-in uses hyphenated word secrets)
        var keyBytes = Encoding.UTF8.GetBytes(_configuration.APIServiceSecret);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var iat = (long)(now - epoch).TotalSeconds;
        var exp = (long)(now.AddMinutes(5) - epoch).TotalSeconds;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Iss, _configuration.ClientId),
            new Claim(JwtRegisteredClaimNames.Aud, "signin.education.gov.uk"),
            new Claim(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogDebug("Generated JWT token for DfE Sign-in API with issuer: {Issuer}", _configuration.ClientId);

        return tokenString;
    }
}

/// <summary>
///     Response from the DfE Sign-in API for user access information.
/// </summary>
public class DfeSignInUserAccessResponse
{
    public string UserId { get; set; } = null!;
    public string ServiceId { get; set; } = null!;
    public string OrganisationId { get; set; } = null!;
    public IList<Role> Roles { get; set; } = new List<Role>();
    public IList<object> Identifiers { get; set; } = new List<object>();
}
