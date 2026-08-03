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

        private static WorkingFamiliesResponseViewModel CreateViewModel(
            DateTime childDateOfBirth,
            DateTime validityStartDate,
            DateTime? gracePeriodEndDate = null)
        {
            return new WorkingFamiliesResponseViewModel
            {
                Response = new CheckEligibilityItemWorkingFamilies
                {
                    NationalInsuranceNumber = "QQ123456C",
                    DateOfBirth = childDateOfBirth.ToString("yyyy-MM-dd"),
                    Status = "eligible",
                    EligibilityCode = "50000000000",
                    ValidityStartDate = validityStartDate.ToString("yyyy-MM-dd"),
                    ValidityEndDate = DateTime.Today.AddMonths(3).ToString("yyyy-MM-dd"),
                    GracePeriodEndDate = (gracePeriodEndDate ?? DateTime.Today.AddMonths(6))
                        .ToString("yyyy-MM-dd")
                }
            };
        }
    }
}