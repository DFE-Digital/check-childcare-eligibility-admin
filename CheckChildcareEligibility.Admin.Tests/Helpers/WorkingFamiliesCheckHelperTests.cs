using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;
using CheckChildcareEligibility.Admin.Helpers;
using CheckChildcareEligibility.Admin.Models;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.Helpers
{
    [TestFixture]
    public class WorkingFamiliesCheckHelperTests
    {
        private static EligibilityCodeProperties CreateProperties(
            EligibilityCodeType type = EligibilityCodeType.Foster,
            bool childTooOld = false,
            DateTime? validityEnd = null)
        {
            return new EligibilityCodeProperties(new FosterFamiliesSearchItemResponse
            {
                ValidityEndDate = validityEnd ?? new DateTime(2026, 8, 31),
                ChildTooOld = childTooOld,
                TermValidity = new TermValidity(null, null),
                ReconfirmationProperties = new ReconfirmationProperties
                {
                    StartDate = new DateTime(2026, 1, 5),
                    EndDate = new DateTime(2026, 3, 20)
                }
            })
            { Type = type };
        }

        [Test]
        public void GetReconfirmBetween_DefaultsToLongDates()
        {
            WorkingFamiliesCheckHelper.GetReconfirmBetween(CreateProperties())
                .Should().Be("5 January 2026 and 20 March 2026");
        }

        [Test]
        public void GetReconfirmBetween_LongDatesTrue_MatchesDefault()
        {
            var properties = CreateProperties();

            WorkingFamiliesCheckHelper.GetReconfirmBetween(properties, longDates: true)
                .Should().Be(WorkingFamiliesCheckHelper.GetReconfirmBetween(properties));
        }

        [Test]
        public void GetReconfirmBetween_LongDatesFalse_UsesAbbreviatedMonths()
        {
            WorkingFamiliesCheckHelper.GetReconfirmBetween(CreateProperties(), longDates: false)
                .Should().Be("5 Jan 2026 and 20 Mar 2026");
        }

        [Test]
        public void GetReconfirmBetween_Temporary_DefaultUsesLongDate()
        {
            WorkingFamiliesCheckHelper.GetReconfirmBetween(CreateProperties(EligibilityCodeType.Temporary))
                .Should().Be("31 August 2026");
        }

        [Test]
        public void GetReconfirmBetween_Temporary_LongDatesFalseUsesAbbreviatedMonth()
        {
            WorkingFamiliesCheckHelper.GetReconfirmBetween(
                    CreateProperties(EligibilityCodeType.Temporary, validityEnd: new DateTime(2026, 10, 31)),
                    longDates: false)
                .Should().Be("31 Oct 2026");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetReconfirmBetween_ChildTooOld_ReturnsNotApplicableRegardlessOfFormat(bool longDates)
        {
            WorkingFamiliesCheckHelper.GetReconfirmBetween(CreateProperties(childTooOld: true), longDates)
                .Should().Be(WorkingFamiliesResponseDetails.StatusNotApplicable);
        }

        [Test]
        public void GetReconfirmBetween_MissingReconfirmationDates_DoesNotThrow()
        {
            var properties = new EligibilityCodeProperties(new FosterFamiliesSearchItemResponse
            {
                TermValidity = new TermValidity(null, null),
                ReconfirmationProperties = new ReconfirmationProperties()
            });

            WorkingFamiliesCheckHelper.GetReconfirmBetween(properties, longDates: false)
                .Should().Be(" and ");
        }
    }
}
