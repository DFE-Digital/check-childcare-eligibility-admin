using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.ViewModels;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadFosterCarerDetailsUseCase
{
    Task<FosterCarerDetailsViewModel> Execute(FosterFamilyResponse response = null);
}

public class LoadFosterCarerDetailsUseCase : ILoadFosterCarerDetailsUseCase
{

    public async Task<FosterCarerDetailsViewModel?> Execute(FosterFamilyResponse response = null)
    {
        FosterCarerDetailsViewModel viewModel = null;
        if (response != null)
        {
            viewModel = new FosterCarerDetailsViewModel
            {
                FosterCarerId = response.FosterCarerId,
                CarerFirstName = response.CarerFirstName,
                CarerLastName = response.CarerLastName,
                CarerDateOfBirth = response.CarerDateOfBirth,
                CarerNationalInsuranceNumber = response.CarerNationalInsuranceNumber,
                HasPartner = response.HasPartner,
                Day = response.CarerDateOfBirth.Day.ToString(),
                Month = response.CarerDateOfBirth.Month.ToString(),
                Year = response.CarerDateOfBirth.Year.ToString()
            };
        }
        return viewModel;
    }
}