using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.ViewModels;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadFosterCarerDetailsUseCase
{
    Task<FosterCarerDetailsViewModel> Execute(FosterFamilyResponse fosterFamilyResponse = null);
}

public class LoadFosterCarerDetailsUseCase : ILoadFosterCarerDetailsUseCase
{

    public async Task<FosterCarerDetailsViewModel?> Execute(FosterFamilyResponse fosterFamilyResponse = null)
    {
        FosterCarerDetailsViewModel fosterCarerDetailsViewModel = null;

        if (fosterFamilyResponse != null)
        {
            fosterCarerDetailsViewModel = new FosterCarerDetailsViewModel
            {
                CarerFirstName = fosterFamilyResponse.CarerFirstName,
                CarerLastName = fosterFamilyResponse.CarerLastName,
                CarerDateOfBirth = fosterFamilyResponse.CarerDateOfBirth,
                CarerNationalInsuranceNumber = fosterFamilyResponse.CarerNationalInsuranceNumber,
                HasPartner = fosterFamilyResponse.HasPartner
            };
        }
        return fosterCarerDetailsViewModel;
    }
}