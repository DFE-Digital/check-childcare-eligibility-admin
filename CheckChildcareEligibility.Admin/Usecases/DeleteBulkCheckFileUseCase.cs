using System.Text.RegularExpressions;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public interface IDeleteBulkCheckFileUseCase
    {
        Task<CheckEligiblityBulkDeleteResponse> Execute(string groupId, ISession session);
    }

    public class DeleteBulkCheckFileUseCase : IDeleteBulkCheckFileUseCase
    {
        private readonly ICheckGateway _checkGateway;
        private readonly ILogger<DeleteBulkCheckFileUseCase> _logger;

        public DeleteBulkCheckFileUseCase(
            ILogger<DeleteBulkCheckFileUseCase> logger,
            ICheckGateway checkGateway)
        {
            _logger = logger;
            _checkGateway = checkGateway;
        }

        public async Task<CheckEligiblityBulkDeleteResponse> Execute(string groupId, ISession session)
        {
            _logger.LogInformation($"Deleting bulk check file {groupId}");

            try
            {
                var result =
                    await _checkGateway.DeleteBulkChecksFor($"bulk-check/{groupId}");

                var response = new CheckEligiblityBulkDeleteResponse();

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot delete file {groupId}", ex);
            }

            return null;
        }
    }
}
