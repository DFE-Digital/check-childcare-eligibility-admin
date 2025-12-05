using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface ICheckGateway
{
    // bulk
    Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string bulkCheckUrl);
    Task<CheckEligibilityBulkProgressByLAResponse> GetBulkCheckStatuses(string organisationId);
    Task<T> GetBulkCheckResults<T>(string resultsUrl) where T : CheckEligibilityBulkResponseBase, new();
    Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk requestBody);
    Task<IEnumerable<IBulkExport>> LoadBulkCheckResults(string bulkCheckId, CheckEligibilityType eligibilityType);

    // single
    Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest requestBody);
    Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody);
    Task<CheckEligibilityItemWorkingFamiliesResponse> GetWFResult(string getEligibilityCheck);
    Task<CheckEligiblityBulkDeleteResponse> DeleteBulkChecksFor(string bulkCheckDeleteUrl);
}