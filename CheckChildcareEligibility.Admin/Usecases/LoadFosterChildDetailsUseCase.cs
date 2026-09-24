using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.ViewModels;

namespace CheckChildcareEligibility.Admin.UseCases;

public interface ILoadFosterChildDetailsUseCase
{
    Task<FosterChildDetailsViewModel> Execute(FosterChildResponse response = null);
}

public class LoadFosterChildDetailsUseCase : ILoadFosterChildDetailsUseCase
{

    public async Task<FosterChildDetailsViewModel?> Execute(FosterChildResponse response = null)
    {
        FosterChildDetailsViewModel viewModel = null;
        if (response != null)
        {
            viewModel = new FosterChildDetailsViewModel
            {
                FosterChildId = response.FosterChildId,
                ChildFirstName = response.ChildFirstName,
                ChildLastName = response.ChildLastName,
                ChildPostCode = response.ChildPostCode,
                ChildDateOfBirth = response.ChildDateOfBirth,
                Day = response.ChildDateOfBirth.Day.ToString(),
                Month = response.ChildDateOfBirth.Month.ToString(),
                Year = response.ChildDateOfBirth.Year.ToString()
            };
        }
        return viewModel;
    }
}