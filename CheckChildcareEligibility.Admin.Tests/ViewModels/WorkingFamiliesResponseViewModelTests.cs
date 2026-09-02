using Azure;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;

namespace CheckChildcareEligibility.Admin.Tests.ViewModels
{
    [TestFixture]
    public class WorkingFamiliesResponseViewModelTests
    {
        [Test]
        public void GracePeriodEndDisplay_WhenChildIsTooYoung_ShouldReturnPlaceholder()
        {
            // Arrange
            var currenttermStart = new DateTime(DateTime.Today.Year, 1, 1); // simulate spring term
            var validityStartDate = currenttermStart.AddDays(-1);
            var childDateOfBirth = validityStartDate.AddMonths(-5);
            var sut = CreateViewModel(childDateOfBirth, validityStartDate, currentTerm: TermName.None, nextTerm: TermName.Summer, status: "eligible");

            // Act
            var result = sut.GracePeriodEndDisplay;

            // Assert
            sut.ChildIsTooYoung.Should().BeTrue(); 
            result.Should().Be("Date will appear here when the code can be used");
        }

        [Test]
        public void GracePeriodEndDisplay_WhenCodeIsNotValidYet_ShouldReturnPlaceholder()
        {
            // Arrange
            var childDateOfBirth = DateTime.Today.AddYears(-3);
            var sut = CreateViewModel(
                childDateOfBirth:childDateOfBirth,    
                currentTerm: TermName.None,
                nextTerm: TermName.Spring);

            // Act
            var result = sut.GracePeriodEndDisplay;

            // Assert
            sut.ChildIsTooYoung.Should().BeFalse();
            sut.IsNotValidYet.Should().BeTrue();
            result.Should().Be("Date will appear here when the code can be used");
        }

        [Test]
        public void GracePeriodEndDisplay_WhenCodeIsValid_ShouldReturnFormattedGracePeriodEndDate()
        {
            // Arrange
            var currenttermStart = new DateTime(DateTime.Today.Year, 1, 1); //simulate spring term
            var validityStartDate = currenttermStart.AddDays(-1);
            var childDateOfBirth = currenttermStart.AddYears(-3);
            var gracePeriodEndDate = currenttermStart.AddMonths(6);
            var sut = CreateViewModel(
                childDateOfBirth,
                validityStartDate,
                gracePeriodEndDate: gracePeriodEndDate,
                currentTerm: TermName.Spring,
                nextTerm: TermName.Summer);

            // Act
            var result = sut.GracePeriodEndDisplay;

            // Assert
            sut.ChildIsTooYoung.Should().BeFalse();
            sut.IsNotValidYet.Should().BeFalse();
            result.Should().Be(gracePeriodEndDate.ToString("d MMMM yyyy"));
        }

        [Test]
        public void ReconfirmationDateLabel_WhenTemporaryCode_ShouldReturnApplyForNewCodeBy()
        {
            // Arrange
            var validityEndDate = DateTime.Today.AddMonths(3);
            var sut = CreateViewModel(
                DateTime.Today.AddYears(-3),
                DateTime.Today.AddMonths(-3),
                validityEndDate: validityEndDate,
                codeType: EligibilityCodeType.Temporary,
                eligibilityCode: "10000000000");

            // Act
            var label = sut.ReconfirmationDateLabel;

            // Assert
            label.Should().Be("Apply for a new code by");
        }

        [Test]
        public void ReconfrimBetween_WhenTemporaryCode_ShouldReturnValidityEndDate()
        {
            // Arrange
            var validityEndDate = DateTime.Today.AddMonths(3);
            var sut = CreateViewModel(
                DateTime.Today.AddYears(-3),
                DateTime.Today.AddMonths(-3),
                validityEndDate: validityEndDate,
                codeType: EligibilityCodeType.Temporary,
                eligibilityCode: "10000000000");

            // Act
            var value = sut.ReconfrimBetween;

            // Assert
            value.Should().Be(validityEndDate.ToString("d MMMM yyyy"));
        }

        [Test]
        public void ReconfrimBetween_WhenPermanentCode_ShouldReturnReconfirmationWindow()
        {
            // Arrange
            var validityEndDate = DateTime.Today.AddMonths(3);
            var sut = CreateViewModel(
                DateTime.Today.AddYears(-3),
                DateTime.Today.AddMonths(-3),
                validityEndDate: validityEndDate,
                reconfirmationStartDate: validityEndDate.AddDays(-28),
                reconfirmationEndDate: validityEndDate,
                reconfirmationStatus: ReconfirmationStatus.Due);

            // Act
            var value = sut.ReconfrimBetween;

            // Assert
            value.Should().Be($"{validityEndDate.AddDays(-28):d MMMM yyyy} and {validityEndDate:d MMMM yyyy}");
        }

        [Test]
        public void ReconfrimBetween_WhenChildTooOld_ShouldReturnNotApplicable()
        {
            // Arrange
            var sut = CreateViewModel(
                DateTime.Today.AddYears(-6),
                DateTime.Today.AddDays(-1),
                reconfirmationStatus: ReconfirmationStatus.ChildTooOld);

            // Act
            var value = sut.ReconfrimBetween;

            // Assert
            value.Should().Be(WorkingFamiliesResponseDetails.StatusNotApplicable);
        }

        [Test]
        public void SetBannerValues_WhenChildIsTooYoung_AndTurnsNineMonthsAfterTheVSD_ShouldSetBlueBanner()
        {
            // Arrange
            var childDateOfBirth = DateTime.Today.AddMonths(-5);
            var sut = CreateViewModel(
                childDateOfBirth:childDateOfBirth,
                validityStartDate:DateTime.Today.AddDays(-1),
                currentTerm: null,
                nextTerm: TermName.Spring,
                status: "eligible");

            // Act
            sut.SetBannerValues();

            // Assert
            sut.CodeStatus.Should().Be(WorkingFamiliesResponseBanner.CodeChildTooYoung);
            sut.BannerColour.Should().Be(WorkingFamiliesResponseBanner.ColourBlue);
            sut.TermValidityDetails.Should().Be($"{WorkingFamiliesResponseBanner.TermValidFrom} {TermName.Spring} {childDateOfBirth.AddMonths(9).Year}");
        }

        [Test]
        public void SetBannerValues_WhenCodeIsNotValidYet_ShouldSetBlueBanner()
        {
            // Arrange
            var gracePerionEndDate = new DateTime(DateTime.Today.Year + 1, 3, 31); 
            var sut = CreateViewModel(
                childDateOfBirth: DateTime.Today.AddYears(-3),
                validityStartDate: DateTime.Today.AddDays(30),       
                gracePeriodEndDate: gracePerionEndDate,
                currentTerm: TermName.None,
                nextTerm: TermName.Spring,
                status: "eligible");

            // Act
            sut.SetBannerValues();

            // Assert
            sut.CodeStatus.Should().Be(WorkingFamiliesResponseBanner.CodeNotValidYet);
            sut.BannerColour.Should().Be(WorkingFamiliesResponseBanner.ColourBlue);
            sut.TermValidityDetails.Should().Be($"{WorkingFamiliesResponseBanner.TermValidFrom} {TermName.Spring} {gracePerionEndDate.Year}");
        }

        [Test]
        public void SetBannerValues_WhenCodeIsInGracePeriod_ShouldSetYellowBanner()
        {
            // Arrange
            var gracePeriodEndDate = DateTime.Today.AddDays(30);
            var sut = CreateViewModel(
                childDateOfBirth: DateTime.Today.AddYears(-3),
                validityStartDate: DateTime.Today.AddMonths(-9),
                validityEndDate: DateTime.Today.AddDays(-1),
                gracePeriodEndDate: gracePeriodEndDate,
                currentTerm: TermName.Spring,
                nextTerm: TermName.None,
                status: "eligible");

            // Act
            sut.SetBannerValues();

            // Assert
            sut.CodeStatus.Should().Be(WorkingFamiliesResponseBanner.CodeInGracePeriod);
            sut.BannerColour.Should().Be(WorkingFamiliesResponseBanner.ColourYellow);
            sut.TermValidityDetails.Should().Be($"{WorkingFamiliesResponseBanner.TermExpiresOn} {gracePeriodEndDate:dd MMMM yyyy}");
        }

        [Test]
        public void IsInGracePeriod_WhenCurrentDateIsBetweenValidityEndAndGracePeriodEnd_ShouldBeTrue()
        {
            // Arrange
            var validityEndDate = DateTime.Today.AddDays(-1);
            var gracePeriodEndDate = DateTime.Today.AddDays(30);
            var sut = CreateViewModel(
                gracePeriodEndDate: gracePeriodEndDate,
                validityEndDate: validityEndDate,
                status: "eligible");

            // Act
            var result = sut.IsInGracePeriod;

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsReconfirmed_WhenEligibleAndNextTermExists_ShouldBeTrue()
        {
            // Arrange
            var sut = CreateViewModel(              
                currentTerm: TermName.Spring,
                nextTerm: TermName.Summer,
                status: "eligible");

            // Act
            var result = sut.IsReconfirmed;

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SetBannerCodeType_WhenStatusIsDue_ShouldReturnBeforeDateMessage()
        {
            // Arrange
            var validityEndDate = DateTime.Today.AddDays(10);
            var sut = CreateViewModel(  
                validityEndDate: validityEndDate,
                reconfirmationStatus: ReconfirmationStatus.Due,
                reconfirmationEndDate: validityEndDate,
                status: "eligible");

            // Act
            var result = sut.SetBannerCodeType();

            // Assert
            result.Should().Be($"Needs reconfirming before {validityEndDate:d MMMM yyyy}");
        }

        [Test]
        public void SetBannerCodeType_WhenStatusIsChildTooOld_ShouldReturnChildTooOldMessage()
        {
            // Arrange
            var sut = CreateViewModel(
                childDateOfBirth: DateTime.Today.AddYears(-6),
                validityStartDate: DateTime.Today.AddDays(-1),
                reconfirmationStatus: ReconfirmationStatus.ChildTooOld,
                status: "eligible");

            // Act
            var result = sut.SetBannerCodeType();

            // Assert
            result.Should().Be(WorkingFamiliesResponseBanner.ReconfirmationChildTooOld);
        }

        [Test]
        public void SetBannerReconfirmationMessage_WhenOverdue_ShouldReturnOverdueMessage()
        {
            // Arrange
            var sut = CreateViewModel(
                reconfirmationStatus: ReconfirmationStatus.Overdue,
                status: "eligible");

            // Act
            var result = sut.SetBannerReconfirmationMessage();

            // Assert
            result.Should().Be(WorkingFamiliesResponseBanner.ReconfirmationOverdue);
        }

        [Test]
        public void SetReconfirmationStatus_WhenDue_ShouldReturnDueNowStatus()
        {
            // Arrange
            var sut = CreateViewModel(  
                reconfirmationStatus: ReconfirmationStatus.Due,
                status: "eligible");

            // Act
            var result = sut.SetReconfirmationStatus();

            // Assert
            result.Should().BeEquivalentTo(WorkingFamiliesResponseDetails.ReconfirmationStatusDueNow);
        }

        [Test]
        public void SetReconfirmationStatus_WhenChildTooOld_ShouldReturnChildTooOldStatus()
        {
            // Arrange
            var sut = CreateViewModel(
                reconfirmationStatus: ReconfirmationStatus.ChildTooOld,
                status: "eligible");

            // Act
            var result = sut.SetReconfirmationStatus();

            // Assert
            result.Should().BeEquivalentTo(WorkingFamiliesResponseDetails.ReconfirmationStatusChildTooOld);
        }

        #region Private
        private static WorkingFamiliesResponseViewModel CreateViewModel(
            DateTime? childDateOfBirth = null,
            DateTime? validityStartDate = null,
            DateTime? gracePeriodEndDate = null,
            DateTime? validityEndDate = null,
            EligibilityCodeType codeType = EligibilityCodeType.Standard,
            string eligibilityCode = "50000000000",
            TermName? currentTerm = null,
            TermName? nextTerm = null,
            ReconfirmationStatus? reconfirmationStatus = null,
            DateTime? reconfirmationStartDate = null,
            DateTime? reconfirmationEndDate = null,
            string status = "eligible")
        {
            var response = new CheckEligibilityItemWorkingFamilies
            {
                NationalInsuranceNumber = "AA123456C",
                EligibilityCodeType = codeType,
                DateOfBirth = (childDateOfBirth ?? DateTime.Today.AddMonths(-15)).ToString("yyyy-MM-dd"),
                Status = status,
                EligibilityCode = eligibilityCode,
                ValidityStartDate = validityStartDate ?? DateTime.Today.AddDays(-1),
                ValidityEndDate = validityEndDate ?? DateTime.Today.AddMonths(3),
                GracePeriodEndDate = gracePeriodEndDate ?? DateTime.Today.AddMonths(6),
                TermValidity = new TermValidity
                {
                    Current = currentTerm,
                    Next = nextTerm
                }
            };

            if (reconfirmationStatus.HasValue || reconfirmationStartDate.HasValue || reconfirmationEndDate.HasValue)
            {
                response.ReconfirmationProperties = new ReconfirmationProperties
                {
                    Status = reconfirmationStatus ?? ReconfirmationStatus.NotDueYet,
                    StartDate = reconfirmationStartDate,
                    EndDate = reconfirmationEndDate
                };
            }

            return new WorkingFamiliesResponseViewModel
            {
                Response = response
            };
        }
#endregion
    }
}