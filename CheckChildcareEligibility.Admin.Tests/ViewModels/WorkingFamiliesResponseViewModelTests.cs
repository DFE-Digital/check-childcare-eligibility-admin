using CheckChildcareEligibility.Admin.Boundary.Responses;
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
            var currentTermStart = WorkingFamiliesResponseViewModel.GetTermStart(DateTime.Now);
            var childDateOfBirth = currentTermStart.AddMonths(-1);
            var validityStartDate = currentTermStart.AddDays(-1);
            var sut = CreateViewModel(childDateOfBirth, validityStartDate);

            // Act
            var result = sut.GracePeriodEndDisplay;

            // Assert
            sut.ChildIsTooYoung.Should().BeTrue();
            sut.IsNotValidYet.Should().BeFalse();
            result.Should().Be("Date will appear here when the code can be used");
        }

        [Test]
        public void GracePeriodEndDisplay_WhenCodeIsNotValidYet_ShouldReturnPlaceholder()
        {
            // Arrange
            var currentTermStart = WorkingFamiliesResponseViewModel.GetTermStart(DateTime.Now);
            var childDateOfBirth = currentTermStart.AddYears(-3);
            var validityStartDate = currentTermStart;
            var sut = CreateViewModel(childDateOfBirth, validityStartDate);

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
            var currentTermStart = WorkingFamiliesResponseViewModel.GetTermStart(DateTime.Now);
            var childDateOfBirth = currentTermStart.AddYears(-3);
            var validityStartDate = currentTermStart.AddDays(-1);
            var gracePeriodEndDate = DateTime.Today.AddMonths(6);
            var sut = CreateViewModel(
                childDateOfBirth,
                validityStartDate,
                gracePeriodEndDate);

            // Act
            var result = sut.GracePeriodEndDisplay;

            // Assert
            sut.ChildIsTooYoung.Should().BeFalse();
            sut.IsNotValidYet.Should().BeFalse();
            result.Should().Be(gracePeriodEndDate.ToString("d MMMM yyyy"));
        }

        [Test]
        public void GracePeriodEndDisplay_WhenChildIsTooOld_ShouldReturnFormattedGracePeriodEndDate()
        {
            // Arrange
            var currentTermStart = WorkingFamiliesResponseViewModel.GetTermStart(DateTime.Now);
            var childDateOfBirth = currentTermStart.AddYears(-6);
            var validityStartDate = currentTermStart.AddDays(-1);
            var gracePeriodEndDate = DateTime.Today.AddMonths(6);
            var sut = CreateViewModel(
                childDateOfBirth,
                validityStartDate,
                gracePeriodEndDate);

            // Act
            var result = sut.GracePeriodEndDisplay;

            // Assert
            sut.ChildIsTooOld.Should().BeTrue();
            result.Should().Be(gracePeriodEndDate.ToString("d MMMM yyyy"));
        }

        [Test]
        public void ReconfirmationDetails_WhenTemporaryCode_ShouldShowValidityEndDate()
        {
            // Arrange
            var validityEndDate = DateTime.Today.AddMonths(3);
            var sut = CreateViewModel(
                DateTime.Today.AddYears(-3),
                DateTime.Today.AddMonths(-3),
                validityEndDate: validityEndDate,
                eligibilityCode: "10000000000");

            // Act
            var label = sut.ReconfirmationDateLabel;
            var value = sut.ReconfirmationDateDisplay;

            // Assert
            sut.IsTemporaryCode.Should().BeTrue();
            label.Should().Be("Apply for a new code by");
            value.Should().Be(validityEndDate.ToString("d MMMM yyyy"));
        }

        [Test]
        public void ReconfirmationDetails_WhenPermanentCode_ShouldShowReconfirmationWindow()
        {
            // Arrange
            var validityEndDate = DateTime.Today.AddMonths(3);
            var sut = CreateViewModel(
                DateTime.Today.AddYears(-3),
                DateTime.Today.AddMonths(-3),
                validityEndDate: validityEndDate);

            // Act
            var label = sut.ReconfirmationDateLabel;
            var value = sut.ReconfirmationDateDisplay;

            // Assert
            sut.IsTemporaryCode.Should().BeFalse();
            label.Should().Be("Reconfirm between");
            value.Should().Be(
                $"{validityEndDate.AddDays(-28):d MMMM yyyy} and {validityEndDate:d MMMM yyyy}");
        }

        [Test]
        public void ReconfirmationDetails_WhenChildIsTooOld_ShouldRemainNotApplicable()
        {
            // Arrange
            var currentTermStart = WorkingFamiliesResponseViewModel.GetTermStart(DateTime.Now);
            var sut = CreateViewModel(
                currentTermStart.AddYears(-6),
                currentTermStart.AddDays(-1));

            // Act
            var label = sut.ReconfirmationDateLabel;
            var value = sut.ReconfirmationDateDisplay;

            // Assert
            sut.ChildIsTooOld.Should().BeTrue();
            label.Should().Be("Reconfirm between");
            value.Should().Be("Not applicable");
        }

        private static WorkingFamiliesResponseViewModel CreateViewModel(
            DateTime childDateOfBirth,
            DateTime validityStartDate,
            DateTime? gracePeriodEndDate = null,
            DateTime? validityEndDate = null,
            string eligibilityCode = "50000000000")
        {
            return new WorkingFamiliesResponseViewModel
            {
                Response = new CheckEligibilityItemWorkingFamilies
                {
                    NationalInsuranceNumber = "QQ123456C",
                    DateOfBirth = childDateOfBirth.ToString("yyyy-MM-dd"),
                    Status = "eligible",
                    EligibilityCode = eligibilityCode,
                    ValidityStartDate = validityStartDate.ToString("yyyy-MM-dd"),
                    ValidityEndDate = (validityEndDate ?? DateTime.Today.AddMonths(3))
                        .ToString("yyyy-MM-dd"),
                    GracePeriodEndDate = (gracePeriodEndDate ?? DateTime.Today.AddMonths(6))
                        .ToString("yyyy-MM-dd")
                }
            };
        }
    }
}