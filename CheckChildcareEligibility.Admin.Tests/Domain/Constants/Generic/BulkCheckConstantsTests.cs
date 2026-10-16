using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Domain.Constants.Generic;

[TestFixture]
public class BulkCheckConstantsTests
{
    [TestCase(BulkCheckStatus.InProgress, "In Progress", "govuk-tag--blue")]
    [TestCase(BulkCheckStatus.Completed, "Completed", "govuk-tag--green")]
    [TestCase(BulkCheckStatus.Failed, "Failed", "govuk-tag--red")]
    [TestCase(BulkCheckStatus.ApplicationCreationInProgress, "Application creation in progress", "govuk-tag--blue")]
    [TestCase(BulkCheckStatus.ApplicationsCreated, "Applications created", "govuk-tag--green")]
    [TestCase(BulkCheckStatus.ApplicationCreationFailed, "Application creation failed", "govuk-tag--failed")]
    public void StatusTags_ShouldMapStatusToDescriptionAndColour(
        BulkCheckStatus status,
        string expectedDescription,
        string expectedColour)
    {
        var statusTag = BulkCheckConstants.StatusTags[status.ToString()];

        statusTag.Description.Should().Be(expectedDescription);
        statusTag.Colour.Should().Be(expectedColour);
    }
}
