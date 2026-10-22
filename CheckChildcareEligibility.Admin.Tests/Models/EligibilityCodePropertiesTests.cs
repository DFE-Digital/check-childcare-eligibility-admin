using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;
using CheckChildcareEligibility.Admin.Models;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Models;

[TestFixture]
public class EligibilityCodePropertiesTests
{
    [Test]
    public void WorkingFamiliesResponseConstructor_UsesChildTooOldFlag()
    {
        var response = new CheckEligibilityItemWorkingFamilies
        {
            Status = "eligible",
            DateOfBirth = "2020-01-01",
            ChildTooOld = true,
            ReconfirmationProperties = new ReconfirmationProperties
            {
                Status = ReconfirmationStatus.NotDueYet
            }
        };

        var sut = new EligibilityCodeProperties(response);

        sut.ChildTooOld.Should().BeTrue();
    }

    [Test]
    public void FosterChildResponseConstructor_UsesChildTooOldFlag()
    {
        var sut = new EligibilityCodeProperties(new FosterChildResponse
        {
            ChildTooOld = true,
            ReconfirmationProperties = new ReconfirmationProperties
            {
                Status = ReconfirmationStatus.NotDueYet
            }
        });

        sut.ChildTooOld.Should().BeTrue();
    }

    [Test]
    public void FosterCodePreviewConstructor_UsesChildTooOldFlagAndChildDateOfBirth()
    {
        var childDateOfBirth = new DateTime(2020, 1, 1);
        var sut = new EligibilityCodeProperties(new FosterCodePreviewResponse
        {
            ChildTooOld = true,
            ReconfirmationProperties = new ReconfirmationProperties
            {
                Status = ReconfirmationStatus.NotDueYet
            }
        }, childDateOfBirth);

        sut.ChildTooOld.Should().BeTrue();
        sut.ChildDateOfBirth.Should().Be(childDateOfBirth);
    }

    [Test]
    public void FosterFamiliesSearchItemConstructor_UsesChildTooOldFlag()
    {
        var sut = new EligibilityCodeProperties(new FosterFamiliesSearchItemResponse
        {
            ChildTooOld = true,
            ReconfirmationProperties = new ReconfirmationProperties
            {
                Status = ReconfirmationStatus.NotDueYet
            }
        });

        sut.ChildTooOld.Should().BeTrue();
    }

    [Test]
    public void WorkingFamiliesConstructor_UsesTooOldReconfirmationStatusWhenFlagIsFalse()
    {
        var response = new CheckEligibilityItemWorkingFamilies
        {
            Status = "eligible",
            DateOfBirth = "2020-01-01",
            ReconfirmationProperties = new ReconfirmationProperties
            {
                Status = ReconfirmationStatus.ChildTooOld
            }
        };

        var sut = new EligibilityCodeProperties(response);

        sut.ChildTooOld.Should().BeTrue();
    }
}
