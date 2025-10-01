using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface ICheckGateway
{
    // bulk
    Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string bulkCheckUrl);
    Task<CheckEligibilityBulkResponse> GetBulkCheckResults(string resultsUrl);
    Task<CheckEligibilityBulkProgressByLAResponse> GetBulkCheckStatuses(string organisationId);

    Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk requestBody);

    // single
    Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest requestBody);
    Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody);
    Task<CheckEligibilityItemWorkingFamiliesResponse> GetWFResult(string getEligibilityCheck);
    Task<CheckEligiblityBulkDeleteResponse> DeleteBulkChecksFor(string bulkCheckDeleteUrl);
}