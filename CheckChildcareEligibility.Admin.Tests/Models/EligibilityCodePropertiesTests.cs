using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
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

    private static EligibilityCodeProperties CreateGraceProperties(
        bool graceApplied = true,
        bool childTooOld = false,
        bool childTooYoung = false,
        DateTime? graceEnd = null)
    {
        return new EligibilityCodeProperties(new FosterFamiliesSearchItemResponse
        {
            ValidityEndDate = new DateTime(2099, 1, 1),
            GracePeriodEndDate = graceEnd ?? new DateTime(2099, 1, 5),
            IsGracePeriodEndDateApplied = graceApplied,
            ChildTooOld = childTooOld,
            ChildTooYoung = childTooYoung,
            TermValidity = new TermValidity(
                new Term(TermName.Autumn, new DateTime(2098, 9, 1)),
                new Term(TermName.Spring, new DateTime(2099, 1, 1))),
            ReconfirmationProperties = new ReconfirmationProperties()
        });
    }

    [Test]
    public void GracePeriodEndDisplay_UsesLongDate()
    {
        CreateGraceProperties().GracePeriodEndDisplay.Should().Be("5 January 2099");
    }

    [Test]
    public void GracePeriodEndDisplayShort_UsesAbbreviatedMonth()
    {
        CreateGraceProperties().GracePeriodEndDisplayShort.Should().Be("5 Jan 2099");
    }

    [Test]
    public void GracePeriodEndDisplayShort_AbbreviatesOtherMonths()
    {
        CreateGraceProperties(graceEnd: new DateTime(2099, 10, 31)).GracePeriodEndDisplayShort.Should().Be("31 Oct 2099");
    }

    [Test]
    public void GracePeriodEndDisplay_And_Short_ReturnNotApplicable_WhenChildTooOld()
    {
        var properties = CreateGraceProperties(childTooOld: true);

        properties.GracePeriodEndDisplay.Should().Be(WorkingFamiliesResponseDetails.StatusNotApplicable);
        properties.GracePeriodEndDisplayShort.Should().Be(WorkingFamiliesResponseDetails.StatusNotApplicable);
    }

    [Test]
    public void GracePeriodEndDisplay_And_Short_ReturnNotAvailable_WhenGraceNotApplied()
    {
        var properties = CreateGraceProperties(graceApplied: false);

        properties.GracePeriodEndDisplay.Should().Be(WorkingFamiliesResponseDetails.GracePeriodEndDateNotAvailable);
        properties.GracePeriodEndDisplayShort.Should().Be(WorkingFamiliesResponseDetails.GracePeriodEndDateNotAvailable);
    }

    [Test]
    public void GracePeriodEndDisplay_And_Short_ReturnNotAvailable_WhenChildTooYoung()
    {
        var properties = CreateGraceProperties(childTooYoung: true);

        properties.GracePeriodEndDisplay.Should().Be(WorkingFamiliesResponseDetails.GracePeriodEndDateNotAvailable);
        properties.GracePeriodEndDisplayShort.Should().Be(WorkingFamiliesResponseDetails.GracePeriodEndDateNotAvailable);
    }
}
