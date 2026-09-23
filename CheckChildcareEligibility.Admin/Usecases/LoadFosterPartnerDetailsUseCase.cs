using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.ViewModels;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadFosterPartnerDetailsUseCase
{
    Task<FosterPartnerDetailsViewModel> Execute(FosterFamilyResponse response = null);
}

public class LoadFosterPartnerDetailsUseCase : ILoadFosterPartnerDetailsUseCase
{

    public async Task<FosterPartnerDetailsViewModel?> Execute(FosterFamilyResponse response = null)
    {
        FosterPartnerDetailsViewModel viewModel = null;

        if (response != null)
        {
            viewModel = new FosterPartnerDetailsViewModel
            {
                FosterCarerId = response.FosterCarerId,
                PartnerFirstName = response?.PartnerFirstName,
                PartnerLastName = response?.PartnerLastName,
                PartnerNationalInsuranceNumber = response?.PartnerNationalInsuranceNumber,
                Day = response.PartnerDateOfBirth?.Day.ToString(),
                Month = response.PartnerDateOfBirth?.Month.ToString(),
                Year = response.PartnerDateOfBirth?.Year.ToString()
            };
        }
        return viewModel;
    }
}