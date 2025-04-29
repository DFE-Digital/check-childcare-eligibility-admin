using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface IParentGateway
{
    Task<EstablishmentSearchResponse> GetSchool(string name);

    Task<UserSaveItemResponse> CreateUser(UserCreateRequest requestBody);

    Task<ApplicationSaveItemResponse> PostApplication_Fsm(ApplicationRequest requestBody);
}