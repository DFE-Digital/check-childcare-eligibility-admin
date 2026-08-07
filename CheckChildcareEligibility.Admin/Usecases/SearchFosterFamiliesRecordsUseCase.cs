using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface ISearchFosterFamiliesRecordsUseCase
    {
        Task<FosterFamiliesSearchResponse> Execute(FosterFamiliesSearchRequest request);
    }

    public class SearchFosterFamiliesRecordsUseCase : ISearchFosterFamiliesRecordsUseCase
    {
        private readonly ICheckGateway _checkGateway;
        private readonly ILogger<SearchFosterFamiliesRecordsUseCase> _logger;

        public SearchFosterFamiliesRecordsUseCase(
            ILogger<SearchFosterFamiliesRecordsUseCase> logger,
            ICheckGateway checkGateway)
        {
            _logger = logger;
            _checkGateway = checkGateway;
        }

        public async Task<FosterFamiliesSearchResponse> Execute(FosterFamiliesSearchRequest request)
        {
            var response = await _checkGateway.GetFosterFamiliesSearchRecords(request.PageNumber, request.PageSize);

            if (response == null)
            {
                return new FosterFamiliesSearchResponse();
            }

            return response;
        }
    }
}
