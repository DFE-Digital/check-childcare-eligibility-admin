using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Models;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface IGetBulkCheckStatusesUseCase
    {
        Task<IEnumerable<BulkCheck>> Execute(string request, ISession session);
    }

    public class GetBulkCheckStatusesUseCase : IGetBulkCheckStatusesUseCase
    {
        private readonly ICheckGateway _checkGateway;
        private readonly ILogger<GetBulkCheckStatusesUseCase> _logger;

        public GetBulkCheckStatusesUseCase(
            ILogger<GetBulkCheckStatusesUseCase> logger,
            ICheckGateway checkGateway)
        {
            _logger = logger;
            _checkGateway = checkGateway;
        }

        public async Task<IEnumerable<BulkCheck>> Execute(string organisationId, ISession session)
        {
            var response = (await _checkGateway.GetBulkCheckStatuses(organisationId)).Checks.Where(x => x.EligibilityType != "FreeSchoolMeals");

            if (response == null)
            {
                return new List<BulkCheck>();
            }

            return response.Select(x => MapToBulkCheck(x));
        }

        private BulkCheck MapToBulkCheck(CheckEligibilityBulkProgressResponse response)
        {
            return new BulkCheck()
            {
                BulkCheckId = response.Id,
                Status = response.Status,
                SubmittedDate = response.SubmittedDate,
                SubmittedBy = response.SubmittedBy,
                EligibilityType = response.EligibilityType,
                Filename = response.Filename
            };
        }
    }
}
