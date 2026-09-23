using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;

namespace CheckChildcareEligibility.Admin.Models
{
    public class EligibilityCodeProperties
    {

        public EligibilityCodeProperties(FosterChildResponse fosterChild)
        {
            Type = EligibilityCodeType.Foster;
            Status = CheckEligibilityStatus.eligible;
            EligibilityCode = fosterChild.EligibilityCode;
            ValidityStartDate = fosterChild.ValidityStartDate;
            ValidityEndDate = fosterChild.ValidityEndDate;
            GracePeriodEndDate = fosterChild.GracePeriodEndDate;
            ChildDateOfBirth = fosterChild.ChildDateOfBirth;
            TermValidity = fosterChild.TermValidity;
            ReconfirmationProperties = fosterChild.ReconfirmationProperties;
            ChildIsTooYoung = fosterChild.ChildTooYoung;
        }

        public EligibilityCodeProperties(CheckEligibilityItemWorkingFamilies workingFamiliesResponse)
        {
            Type = workingFamiliesResponse.EligibilityCodeType ?? EligibilityCodeType.Standard;
            Status = Enum.Parse<CheckEligibilityStatus>(workingFamiliesResponse.Status);
            EligibilityCode = workingFamiliesResponse.EligibilityCode;
            ValidityStartDate = workingFamiliesResponse.ValidityStartDate;
            ValidityEndDate = workingFamiliesResponse.ValidityEndDate;
            GracePeriodEndDate = workingFamiliesResponse.GracePeriodEndDate;
            ChildDateOfBirth = DateTime.Parse(workingFamiliesResponse.DateOfBirth);
            TermValidity = workingFamiliesResponse.TermValidity;
            ReconfirmationProperties = workingFamiliesResponse.ReconfirmationProperties;
            ChildIsTooYoung = workingFamiliesResponse.ChildTooYoung;
        }

        public CheckEligibilityStatus Status { get; init; }

        public EligibilityCodeType Type { get; init; }

        public string EligibilityCode { get; init; }

        public DateTime ChildDateOfBirth { get; init; }

        public DateTime ValidityStartDate { get; init; }

        public DateTime ValidityEndDate { get; init; }

        public DateTime GracePeriodEndDate { get; init; }

        public ReconfirmationProperties ReconfirmationProperties { get; init; }

        public TermValidity TermValidity { get; init; }

        public bool ChildIsTooYoung { get; set; }

        // Calvulated properties

        public bool IsEligible => Status == CheckEligibilityStatus.eligible;

        public bool IsExpired => GracePeriodEndDate < DateTime.UtcNow.Date;

        public Term CurrentTerm => TermValidity.Current ?? Term.None;

        public Term NextTerm => TermValidity.Next ?? Term.None;

        public bool ChildIsTooOld => ReconfirmationProperties.Status == ReconfirmationStatus.ChildTooOld;

        public bool IsInGracePeriod => DateTime.UtcNow.Date > ValidityEndDate && DateTime.UtcNow.Date <= GracePeriodEndDate;

        public bool IsNotValidYet => CurrentTerm.Name == TermName.None && NextTerm.Name != TermName.None;

        public bool IsReconfirmed => IsEligible && !IsNotValidYet && !IsInGracePeriod && NextTerm.Name != TermName.None;

    }
}